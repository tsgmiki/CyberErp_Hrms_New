using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Notifications;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    /// <summary>Automatic applicant e-mails for interview lifecycle events (HC100 hook).</summary>
    public interface IInterviewNotifier
    {
        Task ScheduledAsync(Interview interview);
        Task RescheduledAsync(Interview interview, DateTime oldStart, DateTime oldEnd);
        Task CancelledAsync(Interview interview);
    }

    /// <summary>
    /// Composes and sends the applicant's interview notifications. Runs AFTER the business
    /// transaction commits and never throws — a candidate without an e-mail address (or a mail
    /// outage) is logged and skipped, the interview operation always stands.
    /// </summary>
    public class InterviewNotifier(
        IRepository<JobApplication> applicationRepository,
        IRepository<Candidate> candidateRepository,
        IRepository<JobRequisition> requisitionRepository,
        IRepository<InterviewPanelist> panelistRepository,
        IRepository<User> userRepository,
        IEmailService emailService,
        INotificationDispatcher dispatcher,
        IPortalNotifier portalNotifier,
        ILogger<InterviewNotifier> logger) : IInterviewNotifier
    {
        private sealed record Context(string Email, string CandidateName, string VacancyTitle);

        /// <param name="requireEmail">
        /// True for the applicant's own mail, where no address means there is nothing to send.
        /// FALSE for the panel notice, which only borrows the candidate's NAME and the vacancy
        /// title for its wording — refusing to build that because the candidate is unreachable
        /// would punish the evaluators for the applicant's missing address.
        /// </param>
        private async Task<Context?> ResolveAsync(Guid applicationId, bool requireEmail = true)
        {
            var context = await applicationRepository.GetAll()
                .Where(a => a.Id == applicationId)
                .Select(a => new
                {
                    Candidate = candidateRepository.GetAll()
                        .Where(c => c.Id == a.CandidateId)
                        .Select(c => new { c.Email, c.FirstName, c.FatherName })
                        .FirstOrDefault(),
                    Title = requisitionRepository.GetAll()
                        .Where(q => q.Id == a.RequisitionId)
                        .Select(q => q.Title)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (context?.Candidate is null)
                return null;
            if (requireEmail && string.IsNullOrWhiteSpace(context.Candidate.Email))
            {
                // WARNING, not Information. "The invitation was never sent" is not routine, and at
                // Info it sat below the level anyone reads — which is exactly why the missing mail
                // was reported as a system fault rather than as the missing address it is.
                logger.LogWarning(
                    "Interview invitation NOT sent for application {ApplicationId} — the candidate has no e-mail address on record",
                    applicationId);
                return null;
            }
            var name = $"{context.Candidate.FirstName} {context.Candidate.FatherName}".Trim();
            return new Context(context.Candidate.Email ?? string.Empty, name, context.Title ?? "the advertised position");
        }

        private static string When(DateTime start, DateTime end) =>
            $"{start:dddd, dd MMMM yyyy HH:mm} – {end:HH:mm}";

        private static string Where(Interview i)
        {
            var parts = new List<string> { $"Format: {i.Format}" };
            if (!string.IsNullOrWhiteSpace(i.Location)) parts.Add($"Location: {i.Location}");
            if (!string.IsNullOrWhiteSpace(i.MeetingLink)) parts.Add($"Meeting link: {i.MeetingLink}");
            return string.Join("\n", parts);
        }

        /// <summary>
        /// Dispatches the administrator-defined message for a candidate-facing interview event.
        ///
        /// <para>⚠️ The candidate's address rides in as <c>SubjectAddresses</c> and is reachable only
        /// through an <c>EventSubject</c> recipient rule. A template built purely from staff rules
        /// would take over from the hardcoded mail and cut the candidate out of their own
        /// invitation — the token palette and the event description both call this out.</para>
        /// </summary>
        private Task<int> DispatchAsync(string eventKey, Interview interview, Context ctx,
            DateTime? previousStart = null, DateTime? previousEnd = null) =>
            dispatcher.DispatchAsync(new NotificationContext(
                eventKey,
                new Dictionary<string, string?>
                {
                    ["CandidateName"] = ctx.CandidateName,
                    ["VacancyTitle"] = ctx.VacancyTitle,
                    ["Round"] = interview.Round.ToString(),
                    ["InterviewDate"] = interview.ScheduledStart.ToString("dd MMM yyyy"),
                    ["StartTime"] = interview.ScheduledStart.ToString("HH:mm"),
                    ["EndTime"] = interview.ScheduledEnd.ToString("HH:mm"),
                    ["Mode"] = interview.Format.ToString(),
                    ["Location"] = string.IsNullOrWhiteSpace(interview.Location)
                        ? interview.MeetingLink : interview.Location,
                    ["PreviousDate"] = previousStart?.ToString("dd MMM yyyy"),
                    ["PreviousTime"] = previousStart is null ? null
                        : $"{previousStart:HH:mm}–{previousEnd:HH:mm}",
                },
                EntityType: nameof(Interview),
                EntityId: interview.Id,
                SubjectAddresses: [ctx.Email]));

        /// <summary>
        /// Tells the PANEL — the evaluators on this interview — that it is booked: an alert in the
        /// Home portal and an e-mail.
        ///
        /// <para>⚠️ Raised SEPARATELY from the applicant's invitation, and deliberately before it.
        /// <c>ResolveAsync</c> returns null when the candidate has no e-mail address and
        /// <c>ScheduledAsync</c> then returns immediately — so while the panel notice lived inside
        /// that flow, ONE missing candidate address silenced the evaluators too. The two audiences
        /// have nothing to do with each other and must not share a failure (logic §12.83).</para>
        ///
        /// <para>Best effort throughout: scheduling an interview must not fail over a notification.
        /// External panelists carry no EmployeeId, so they have neither a portal account nor a known
        /// address — they are counted in the log rather than silently dropped.</para>
        /// </summary>
        private async Task NotifyPanelAsync(Interview interview, string verb, string? whenLine = null)
        {
            try
            {
                var employeeIds = (await panelistRepository.GetAll().AsNoTracking()
                    .Where(x => x.InterviewId == interview.Id && x.EmployeeId != null)
                    .Select(x => x.EmployeeId!.Value)
                    .ToListAsync()).Distinct().ToList();
                if (employeeIds.Count == 0)
                {
                    logger.LogInformation(
                        "Interview {Id} {Verb}: no employee panelists — no evaluator alert raised.", interview.Id, verb);
                    return;
                }

                var accounts = await userRepository.GetAll().AsNoTracking()
                    .Where(u => u.EmployeeId != null && employeeIds.Contains(u.EmployeeId.Value))
                    .Select(u => new { u.Id, u.Email })
                    .ToListAsync();

                var ctx = await ResolveAsync(interview.ApplicationId, requireEmail: false);
                var who = ctx?.CandidateName ?? "a candidate";
                var role = ctx?.VacancyTitle ?? "the advertised position";
                var body = $"You are on the interview panel for {who} ({role}), round {interview.Round}. " +
                           (whenLine ?? When(interview.ScheduledStart, interview.ScheduledEnd)) + ". " +
                           Where(interview).Replace(Environment.NewLine, " ").Trim();

                try
                {
                    await portalNotifier.NotifyUsersAsync(
                        accounts.Select(a => a.Id), $"Interview {verb} — you are on the panel", body,
                        "/myEvaluations", "Action", nameof(Interview), interview.Id);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Interview {Id}: portal alert to the panel failed", interview.Id);
                }

                var addresses = accounts.Select(a => a.Email)
                    .Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e!.Trim()).Distinct().ToList();
                if (addresses.Count == 0)
                {
                    logger.LogInformation(
                        "Interview {Id} {Verb}: {Count} panelist account(s) alerted in the portal, none has an e-mail address.",
                        interview.Id, verb, accounts.Count);
                    return;
                }

                var dispatched = await dispatcher.DispatchAsync(new NotificationContext(
                    NotificationEvents.InterviewPanelNotified,
                    new Dictionary<string, string?>
                    {
                        ["CandidateName"] = ctx?.CandidateName,
                        ["VacancyTitle"] = ctx?.VacancyTitle,
                        ["Round"] = interview.Round.ToString(),
                        ["InterviewDate"] = interview.ScheduledStart.ToString("dd MMM yyyy"),
                        ["StartTime"] = interview.ScheduledStart.ToString("HH:mm"),
                        ["EndTime"] = interview.ScheduledEnd.ToString("HH:mm"),
                        ["Mode"] = interview.Format.ToString(),
                        ["Action"] = verb,
                    },
                    EntityType: nameof(Interview),
                    EntityId: interview.Id,
                    SubjectAddresses: addresses));

                if (dispatched == 0)
                    foreach (var address in addresses)
                        await emailService.SendAsync(address, $"Interview {verb} — {who} ({role})", body);

                logger.LogInformation(
                    "Interview {Id} {Verb}: alerted {Portal} panelist account(s), {Mail} address(es).",
                    interview.Id, verb, accounts.Count, addresses.Count);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Interview {Id}: panel notification failed", interview.Id);
            }
        }

        public async Task ScheduledAsync(Interview interview)
        {
            // The panel is told first, and on its own — a candidate with no address must not take
            // the evaluators' notice down with it.
            await NotifyPanelAsync(interview, "scheduled");

            try
            {
                var ctx = await ResolveAsync(interview.ApplicationId);
                if (ctx is null) return;
                if (await DispatchAsync(NotificationEvents.InterviewScheduled, interview, ctx) > 0) return;

                await emailService.SendAsync(ctx.Email,
                    $"Interview Invitation — {ctx.VacancyTitle}",
                    $"""
                    Dear {ctx.CandidateName},

                    We are pleased to invite you to an interview (round {interview.Round}) for the
                    position of {ctx.VacancyTitle}.

                    When: {When(interview.ScheduledStart, interview.ScheduledEnd)}
                    {Where(interview)}

                    Please confirm your availability by replying to this message. If the proposed
                    time does not work for you, let us know and we will reschedule.

                    Kind regards,
                    Human Resources
                    """);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Interview-scheduled notification failed for {InterviewId}", interview.Id);
            }
        }

        public async Task RescheduledAsync(Interview interview, DateTime oldStart, DateTime oldEnd)
        {
            await NotifyPanelAsync(interview, "rescheduled",
                $"Moved from {When(oldStart, oldEnd)} to {When(interview.ScheduledStart, interview.ScheduledEnd)}");

            try
            {
                var ctx = await ResolveAsync(interview.ApplicationId);
                if (ctx is null) return;
                if (await DispatchAsync(NotificationEvents.InterviewRescheduled, interview, ctx, oldStart, oldEnd) > 0) return;

                await emailService.SendAsync(ctx.Email,
                    $"Interview Rescheduled — {ctx.VacancyTitle}",
                    $"""
                    Dear {ctx.CandidateName},

                    The interview (round {interview.Round}) for the position of {ctx.VacancyTitle}
                    has been rescheduled.

                    Previous time: {When(oldStart, oldEnd)}
                    New time:      {When(interview.ScheduledStart, interview.ScheduledEnd)}
                    {Where(interview)}

                    We apologize for any inconvenience. Please confirm the new time by replying to
                    this message.

                    Kind regards,
                    Human Resources
                    """);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Interview-rescheduled notification failed for {InterviewId}", interview.Id);
            }
        }

        public async Task CancelledAsync(Interview interview)
        {
            await NotifyPanelAsync(interview, "cancelled");

            try
            {
                var ctx = await ResolveAsync(interview.ApplicationId);
                if (ctx is null) return;
                if (await DispatchAsync(NotificationEvents.InterviewCancelled, interview, ctx) > 0) return;

                await emailService.SendAsync(ctx.Email,
                    $"Interview Cancelled — {ctx.VacancyTitle}",
                    $"""
                    Dear {ctx.CandidateName},

                    The interview (round {interview.Round}) for the position of {ctx.VacancyTitle},
                    previously scheduled for {When(interview.ScheduledStart, interview.ScheduledEnd)},
                    has been cancelled.

                    We will contact you if a new time is arranged. Thank you for your understanding.

                    Kind regards,
                    Human Resources
                    """);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Interview-cancelled notification failed for {InterviewId}", interview.Id);
            }
        }
    }
}
