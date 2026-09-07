using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Notifications;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    public interface IExaminerNotifier
    {
        /// <summary>Tells the requisition's assigned examiners that the vacancy is now open.</summary>
        Task NotifyVacancyPostedAsync(Guid requisitionId);
    }

    /// <summary>
    /// Alerts the examiners assigned to a vacancy's screening criteria when it is posted — in the
    /// Home portal AND by e-mail.
    ///
    /// <para>Until now an examiner learned they had work only by being told in person: assignment
    /// happens when the requisition is drafted, and nothing announced the posting. The two channels
    /// are deliberate — the portal alert is what they see when they next sign in, the e-mail is what
    /// reaches them when they do not.</para>
    ///
    /// <para>⚠️ BEST EFFORT, ALWAYS. Every path is wrapped: posting a vacancy must not fail because
    /// a mail relay is down or an examiner has no account. The same rule the rest of the notifiers
    /// follow.</para>
    /// </summary>
    public class ExaminerNotifier(
        IRepository<JobRequisition> requisitions,
        IRepository<CriterionEvaluator> evaluators,
        IRepository<Employee> employees,
        IRepository<User> users,
        INotificationDispatcher dispatcher,
        IPortalNotifier portalNotifier,
        IEmailService emailService,
        ILogger<ExaminerNotifier> logger) : IExaminerNotifier
    {
        /// <summary>Correlation key for examiner alerts, so resolving one never clears another feed's.</summary>
        private const string Source = nameof(JobRequisition);

        public async Task NotifyVacancyPostedAsync(Guid requisitionId)
        {
            try
            {
                var head = await requisitions.GetAll().AsNoTracking()
                    .Where(q => q.Id == requisitionId)
                    .Select(q => new { q.RequisitionNumber, q.Title, q.OpenUntil, q.NumberOfPositions })
                    .FirstOrDefaultAsync();
                if (head is null) return;

                // The examiners of THIS vacancy: evaluators attached to its own screening criteria.
                // External evaluators carry no EmployeeId and so have neither a portal account nor a
                // known address — they are counted in the log but cannot be reached from here.
                var criterionIds = await requisitions.GetAll().AsNoTracking()
                    .SelectMany(q => q.ScreeningCriteria)
                    .Where(c => c.RequisitionId == requisitionId)
                    .Select(c => c.Id)
                    .ToListAsync();
                if (criterionIds.Count == 0) return;

                var employeeIds = (await evaluators.GetAll().AsNoTracking()
                    .Where(ev => criterionIds.Contains(ev.CriterionId) && ev.EmployeeId != null)
                    .Select(ev => ev.EmployeeId!.Value)
                    .ToListAsync()).Distinct().ToList();
                if (employeeIds.Count == 0)
                {
                    logger.LogInformation(
                        "Requisition {Id} posted: no employee examiners assigned — no alert raised.", requisitionId);
                    return;
                }

                var accounts = await users.GetAll().AsNoTracking()
                    .Where(u => u.EmployeeId != null && employeeIds.Contains(u.EmployeeId.Value))
                    .Select(u => new { u.Id, EmployeeId = u.EmployeeId!.Value, u.Email })
                    .ToListAsync();

                var closes = head.OpenUntil.HasValue ? $" Applications close on {head.OpenUntil:dd MMM yyyy}." : string.Empty;
                var title = "You are an examiner for a new vacancy";
                var body = $"{head.RequisitionNumber} — {head.Title} ({head.NumberOfPositions} position(s)) is now open." +
                           $"{closes} Your assigned criteria are waiting under My Evaluations.";

                // ---- Portal ------------------------------------------------------------------
                // Deep-links to the evaluator's own screen, so the alert is one click from the work.
                try
                {
                    await portalNotifier.NotifyUsersAsync(
                        accounts.Select(a => a.Id), title, body, "/myEvaluations",
                        "Action", Source, requisitionId);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Requisition {Id}: portal alert to examiners failed", requisitionId);
                }

                // ---- E-mail ------------------------------------------------------------------
                // Template first so a client can reword it, hardcoded fallback so the message still
                // goes out before anyone has configured one — the pattern the other notifiers use.
                var addresses = accounts
                    .Select(a => a.Email)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e!.Trim())
                    .Distinct()
                    .ToList();
                if (addresses.Count == 0)
                {
                    logger.LogInformation(
                        "Requisition {Id} posted: {Count} examiner(s) alerted in the portal, none has an e-mail address.",
                        requisitionId, accounts.Count);
                    return;
                }

                var dispatched = await dispatcher.DispatchAsync(new NotificationContext(
                    NotificationEvents.VacancyPosted,
                    new Dictionary<string, string?>
                    {
                        ["RequisitionNumber"] = head.RequisitionNumber,
                        ["VacancyTitle"] = head.Title,
                        ["NumberOfPositions"] = head.NumberOfPositions.ToString(),
                        ["OpenUntil"] = head.OpenUntil?.ToString("dd MMM yyyy"),
                    },
                    EntityType: Source,
                    EntityId: requisitionId,
                    SubjectAddresses: addresses));

                if (dispatched == 0)
                    foreach (var address in addresses)
                        await emailService.SendAsync(address, $"Examiner assignment — {head.RequisitionNumber}", body);

                logger.LogInformation(
                    "Requisition {Id} posted: alerted {Portal} examiner account(s), {Mail} address(es).",
                    requisitionId, accounts.Count, addresses.Count);
            }
            catch (Exception ex)
            {
                // Never let an alert break the posting itself.
                logger.LogWarning(ex, "Requisition {Id}: examiner posting alerts failed", requisitionId);
            }
        }
    }
}
