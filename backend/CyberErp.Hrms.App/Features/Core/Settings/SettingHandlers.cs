using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Settings
{
    /*
     * Core.Setting had NO read or write path at all until 2026-08-13 — the table and the entity
     * existed, a row had been seeded, and nothing in the application touched it. That is why the SMTP
     * columns had never taken effect: SmtpEmailService went straight to configuration.
     *
     * These handlers give it the missing half. See logic.md §12.12.
     */

    // ---- DTOs ---------------------------------------------------------------

    /// <summary>
    /// ⚠️ Carries NO SMTP password, matching the table. The credential lives in configuration
    /// (user-secrets locally, environment variables elsewhere) and is never returned to a client.
    /// <see cref="HasSmtpPassword"/> reports only whether one is configured, so the screen can warn.
    /// </summary>
    public class SettingDto
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = string.Empty;
        public bool SmtpUseTls { get; set; }
        public bool AutoBackup { get; set; }
        public string BackupFrequency { get; set; } = "daily";
        public int BackupRetentionDays { get; set; }

        /// <summary>True when Email:Password is set. Mail cannot authenticate without it.</summary>
        public bool HasSmtpPassword { get; set; }
        /// <summary>The Email:Enabled master switch — a deployment concern, not editable here.</summary>
        public bool EmailEnabled { get; set; }
    }

    public class SaveSettingDto
    {
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public string? SmtpUser { get; set; }
        public bool SmtpUseTls { get; set; } = true;
        public bool AutoBackup { get; set; }
        public string? BackupFrequency { get; set; }
        public int BackupRetentionDays { get; set; }
    }

    public class SaveSettingDtoValidator : AbstractValidator<SaveSettingDto>
    {
        public SaveSettingDtoValidator()
        {
            RuleFor(x => x.SmtpHost).MaximumLength(255);
            RuleFor(x => x.SmtpUser).MaximumLength(255);
            RuleFor(x => x.SmtpPort).InclusiveBetween(1, 65535)
                .WithMessage("SMTP port must be between 1 and 65535.");
            RuleFor(x => x.BackupRetentionDays).GreaterThanOrEqualTo(0);
        }
    }

    public class TestEmailDto
    {
        public string To { get; set; } = string.Empty;
    }

    public class TestEmailResult
    {
        public bool Queued { get; set; }
        public string Message { get; set; } = string.Empty;
        /// <summary>The host the message will actually be relayed through — the point of the test.</summary>
        public string? ResolvedHost { get; set; }
        public string? ResolvedUser { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------

    public interface IGetSetting { Task<SettingDto> GetAsync(); }
    public interface ISaveSetting { Task SaveAsync(SaveSettingDto dto); }
    public interface ISendTestEmail { Task<TestEmailResult> SendAsync(TestEmailDto dto); }

    // ---- Handlers -----------------------------------------------------------

    public class GetSetting(
        IRepository<Setting> repository,
        ISmtpSettingsResolver smtpSettings,
        IEmailConfiguration email) : IGetSetting
    {
        public async Task<SettingDto> GetAsync()
        {
            var setting = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync();

            // Report what will ACTUALLY be used, which is the stored value or the configured
            // fallback — showing a blank host when configuration supplies one would be misleading.
            var relay = await smtpSettings.ResolveAsync();

            return new SettingDto
            {
                SmtpHost = relay.Host ?? string.Empty,
                SmtpPort = relay.Port,
                SmtpUser = relay.UserName ?? string.Empty,
                SmtpUseTls = relay.UseTls,
                AutoBackup = setting?.AutoBackup ?? false,
                BackupFrequency = setting?.BackupFrequency ?? "daily",
                BackupRetentionDays = setting?.BackupRetentionDays ?? 30,
                HasSmtpPassword = email.HasPassword,
                EmailEnabled = email.Enabled,
            };
        }
    }

    public class SaveSetting(
        IRepository<Setting> repository,
        IValidator<SaveSettingDto> validator,
        ILogger<SaveSetting> logger) : ISaveSetting
    {
        public async Task SaveAsync(SaveSettingDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var setting = await repository.GetAll().FirstOrDefaultAsync();
            if (setting is null)
            {
                setting = Setting.CreateDefault();
                setting.UpdateOperations(dto.SmtpHost ?? string.Empty, dto.SmtpPort, dto.SmtpUser ?? string.Empty,
                    dto.SmtpUseTls, dto.AutoBackup, dto.BackupFrequency ?? "daily", dto.BackupRetentionDays);
                await repository.AddAsync(setting);
            }
            else
            {
                setting.UpdateOperations(dto.SmtpHost ?? string.Empty, dto.SmtpPort, dto.SmtpUser ?? string.Empty,
                    dto.SmtpUseTls, dto.AutoBackup, dto.BackupFrequency ?? "daily", dto.BackupRetentionDays);
                repository.UpdateAsync(setting);
            }
            await repository.SaveChangesAsync();
            logger.LogInformation("Saved operations settings (SMTP host '{Host}')", dto.SmtpHost);
        }
    }

    /// <summary>
    /// Queues one message so an administrator can confirm the relay actually works. Without this,
    /// SMTP settings can be saved and there is no way to tell whether they are right short of waiting
    /// for a real notification to go missing.
    /// </summary>
    public class SendTestEmail(
        IEmailService email,
        ISmtpSettingsResolver smtpSettings,
        IEmailConfiguration configuration) : ISendTestEmail
    {
        public async Task<TestEmailResult> SendAsync(TestEmailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.To))
                throw new ValidationException(nameof(dto.To), "A recipient address is required.");

            var relay = await smtpSettings.ResolveAsync();

            if (!configuration.Enabled)
                return new TestEmailResult
                {
                    Queued = false,
                    Message = "E-mail is disabled for this deployment (Email:Enabled is false), so nothing was sent.",
                    ResolvedHost = relay.Host,
                    ResolvedUser = relay.UserName,
                };

            if (string.IsNullOrWhiteSpace(relay.Host))
                return new TestEmailResult
                {
                    Queued = false,
                    Message = "No SMTP host is configured, here or in the Email configuration section.",
                };

            // A missing password is the single most likely reason a correctly-addressed relay refuses
            // the message, and it is invisible from this screen — so say it plainly rather than let
            // the send fail silently in a background job.
            if (!string.IsNullOrWhiteSpace(relay.UserName) && !configuration.HasPassword)
                return new TestEmailResult
                {
                    Queued = false,
                    Message = $"SMTP user '{relay.UserName}' is set but no password is configured "
                              + "(Email:Password, from user-secrets or an environment variable), so the relay would reject it.",
                    ResolvedHost = relay.Host,
                    ResolvedUser = relay.UserName,
                };

            var queued = await email.SendAsync(dto.To, "CyberERP HRMS test message",
                $"This is a test message from CyberERP HRMS, relayed through {relay.Host}:{relay.Port}. "
                + "If you received it, outbound e-mail is working.");

            return new TestEmailResult
            {
                Queued = queued,
                Message = queued
                    ? $"Queued for delivery through {relay.Host}:{relay.Port}. Check the recipient's inbox; "
                      + "a failure will appear on the Hangfire dashboard."
                    : "The message could not be queued — see the server log.",
                ResolvedHost = relay.Host,
                ResolvedUser = relay.UserName,
            };
        }
    }
}
