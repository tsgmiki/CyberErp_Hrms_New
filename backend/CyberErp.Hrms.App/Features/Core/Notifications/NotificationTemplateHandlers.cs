using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Notifications
{
    // ---- DTOs -----------------------------------------------------------------------------

    public class NotificationEventDto
    {
        public Guid Id { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>The merge tokens this event publishes — the editor's token palette.</summary>
        public List<string> Tokens { get; set; } = [];
        public bool IsWorkflowEvent { get; set; }
    }

    public class NotificationRecipientDto
    {
        public Guid Id { get; set; }
        public string Kind { get; set; } = nameof(RecipientKind.Requester);
        public Guid? TargetId { get; set; }
        public string? Address { get; set; }
        public string Delivery { get; set; } = nameof(RecipientDelivery.To);
        public bool IsActive { get; set; } = true;
    }

    public class NotificationTemplateDto
    {
        public Guid Id { get; set; }
        public Guid NotificationEventId { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Channel { get; set; } = nameof(NotificationChannel.Email);
        public Guid? WorkflowDefinitionId { get; set; }
        public int? StepOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<NotificationRecipientDto> Recipients { get; set; } = [];
    }

    // ---- Interfaces -----------------------------------------------------------------------

    public interface IGetNotificationEvents { Task<List<NotificationEventDto>> GetAsync(); }
    public interface IGetAllNotificationTemplates { Task<PaginatedResponse<NotificationTemplateDto>> GetAsync(GetAllRequest request); }
    public interface IGetNotificationTemplateById { Task<NotificationTemplateDto> GetAsync(Guid id); }
    public interface ISaveNotificationTemplate { Task<Guid> SaveAsync(NotificationTemplateDto dto); }
    public interface IDeleteNotificationTemplate { Task DeleteAsync(Guid id); }
    public interface ISeedNotificationEvents { Task<int> SeedAsync(); }

    // ---- Handlers -------------------------------------------------------------------------

    /// <summary>
    /// Puts the code's event catalogue into this tenant, idempotently.
    ///
    /// <para>Existing rows are REFRESHED (a name or token list can improve) and none are ever
    /// deleted — a template points at one, and removing the row would orphan the client's
    /// configuration.</para>
    /// </summary>
    public class SeedNotificationEvents(IRepository<NotificationEvent> repository) : ISeedNotificationEvents
    {
        public async Task<int> SeedAsync()
        {
            var existing = await repository.GetAll().ToListAsync();
            var written = 0;

            foreach (var seed in NotificationEvents.All)
            {
                var row = existing.FirstOrDefault(e => e.EventKey == seed.EventKey);
                if (row is null)
                {
                    await repository.AddAsync(NotificationEvent.Create(
                        seed.EventKey, seed.Name, seed.Category, seed.Tokens, seed.Description, seed.IsWorkflowEvent));
                    written++;
                }
                else
                {
                    row.Update(seed.Name, seed.Category, seed.Tokens, seed.Description);
                    repository.UpdateAsync(row);
                }
            }

            await repository.SaveChangesAsync();
            return written;
        }
    }

    public class GetNotificationEvents(IRepository<NotificationEvent> repository) : IGetNotificationEvents
    {
        public async Task<List<NotificationEventDto>> GetAsync() =>
            await repository.GetAll().AsNoTracking()
                .OrderBy(e => e.Category).ThenBy(e => e.Name)
                .Select(e => new NotificationEventDto
                {
                    Id = e.Id,
                    EventKey = e.EventKey,
                    Name = e.Name,
                    Category = e.Category,
                    Description = e.Description,
                    Tokens = e.Tokens.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                    IsWorkflowEvent = e.IsWorkflowEvent,
                })
                .ToListAsync();
    }

    public class GetAllNotificationTemplates(
        IRepository<NotificationTemplate> repository,
        IRepository<NotificationEvent> events) : IGetAllNotificationTemplates
    {
        public async Task<PaginatedResponse<NotificationTemplateDto>> GetAsync(GetAllRequest request)
        {
            var query = repository.GetAll().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(t => t.Name.Contains(request.SearchText)
                                      || t.Subject.Contains(request.SearchText)
                                      || t.EventKey.Contains(request.SearchText));

            var total = await query.CountAsync();
            int skip = int.TryParse(request.Skip, out var s) ? s : 0;
            int take = int.TryParse(request.Take, out var t) ? t : 15;

            var rows = await query.OrderBy(x => x.EventKey).ThenBy(x => x.Name)
                .Skip(skip).Take(take).ToListAsync();

            var names = await events.GetAll().AsNoTracking()
                .ToDictionaryAsync(e => e.EventKey, e => e.Name);

            return new PaginatedResponse<NotificationTemplateDto>
            {
                Total = total,
                Data = rows.Select(x => Map(x, names.GetValueOrDefault(x.EventKey, x.EventKey), [])).ToList(),
            };
        }

        internal static NotificationTemplateDto Map(NotificationTemplate t, string eventName,
            List<NotificationRecipientDto> recipients) => new()
        {
            Id = t.Id,
            NotificationEventId = t.NotificationEventId,
            EventKey = t.EventKey,
            EventName = eventName,
            Name = t.Name,
            Subject = t.Subject,
            Body = t.Body,
            Channel = t.Channel.ToString(),
            WorkflowDefinitionId = t.WorkflowDefinitionId,
            StepOrder = t.StepOrder,
            IsActive = t.IsActive,
            Recipients = recipients,
        };
    }

    public class GetNotificationTemplateById(
        IRepository<NotificationTemplate> repository,
        IRepository<NotificationRecipient> recipients,
        IRepository<NotificationEvent> events) : IGetNotificationTemplateById
    {
        public async Task<NotificationTemplateDto> GetAsync(Guid id)
        {
            var template = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new ValidationException(nameof(id), "The template was not found.");

            var rules = await recipients.GetAll().AsNoTracking()
                .Where(r => r.NotificationTemplateId == id)
                .Select(r => new NotificationRecipientDto
                {
                    Id = r.Id,
                    Kind = r.Kind.ToString(),
                    TargetId = r.TargetId,
                    Address = r.Address,
                    Delivery = r.Delivery.ToString(),
                    IsActive = r.IsActive,
                })
                .ToListAsync();

            var eventName = await events.GetAll().AsNoTracking()
                .Where(e => e.Id == template.NotificationEventId)
                .Select(e => e.Name).FirstOrDefaultAsync() ?? template.EventKey;

            return GetAllNotificationTemplates.Map(template, eventName, rules);
        }
    }

    /// <summary>
    /// Upserts a template WITH its recipient rules — they are one aggregate to an administrator, and
    /// saving them separately would let a template exist with nobody to send to.
    /// </summary>
    public class SaveNotificationTemplate(
        IRepository<NotificationTemplate> repository,
        IRepository<NotificationRecipient> recipients,
        IRepository<NotificationEvent> events) : ISaveNotificationTemplate
    {
        public async Task<Guid> SaveAsync(NotificationTemplateDto dto)
        {
            var catalogue = await events.GetAll()
                .FirstOrDefaultAsync(e => e.Id == dto.NotificationEventId || e.EventKey == dto.EventKey)
                ?? throw new ValidationException(nameof(dto.EventKey),
                    "Choose an event this template responds to.");

            var channel = Enum.TryParse<NotificationChannel>(dto.Channel, true, out var c)
                ? c : NotificationChannel.Email;

            NotificationTemplate entity;
            if (dto.Id == Guid.Empty)
            {
                entity = NotificationTemplate.Create(catalogue.Id, catalogue.EventKey, dto.Name,
                    dto.Subject, dto.Body, channel, dto.WorkflowDefinitionId, dto.StepOrder, dto.IsActive);
                await repository.AddAsync(entity);
            }
            else
            {
                entity = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == dto.Id)
                    ?? throw new ValidationException(nameof(dto.Id), "The template was not found.");
                entity.Update(dto.Name, dto.Subject, dto.Body, channel,
                    dto.WorkflowDefinitionId, dto.StepOrder, dto.IsActive);
                repository.UpdateAsync(entity);
            }
            await repository.SaveChangesAsync();

            // Rules are REPLACED wholesale: the editor sends the full list, and diffing rows the user
            // deleted in the browser would be more code for the same result.
            var current = await recipients.GetAll()
                .Where(r => r.NotificationTemplateId == entity.Id).ToListAsync();
            foreach (var stale in current) recipients.Delete(stale);

            foreach (var rule in dto.Recipients)
            {
                if (!Enum.TryParse<RecipientKind>(rule.Kind, true, out var kind)) continue;
                var delivery = Enum.TryParse<RecipientDelivery>(rule.Delivery, true, out var d)
                    ? d : RecipientDelivery.To;
                await recipients.AddAsync(NotificationRecipient.Create(
                    entity.Id, kind, rule.TargetId, rule.Address, delivery, rule.IsActive));
            }
            await recipients.SaveChangesAsync();

            return entity.Id;
        }
    }

    public class DeleteNotificationTemplate(
        IRepository<NotificationTemplate> repository,
        IRepository<NotificationRecipient> recipients) : IDeleteNotificationTemplate
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new ValidationException(nameof(id), "The template was not found.");

            var rules = await recipients.GetAll().Where(r => r.NotificationTemplateId == id).ToListAsync();
            foreach (var rule in rules) recipients.Delete(rule);
            await recipients.SaveChangesAsync();

            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }
}
