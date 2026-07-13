using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
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
        IEmailService emailService,
        ILogger<InterviewNotifier> logger) : IInterviewNotifier
    {
        private sealed record Context(string Email, string CandidateName, string VacancyTitle);

        private async Task<Context?> ResolveAsync(Guid applicationId)
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

            if (context?.Candidate is null || string.IsNullOrWhiteSpace(context.Candidate.Email))
            {
                logger.LogInformation(
                    "Interview notification skipped for application {ApplicationId} — the candidate has no e-mail address",
                    applicationId);
                return null;
            }
            var name = $"{context.Candidate.FirstName} {context.Candidate.FatherName}".Trim();
            return new Context(context.Candidate.Email, name, context.Title ?? "the advertised position");
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

        public async Task ScheduledAsync(Interview interview)
        {
            try
            {
                var ctx = await ResolveAsync(interview.ApplicationId);
                if (ctx is null) return;
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
            try
            {
                var ctx = await ResolveAsync(interview.ApplicationId);
                if (ctx is null) return;
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
            try
            {
                var ctx = await ResolveAsync(interview.ApplicationId);
                if (ctx is null) return;
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
