using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Notifications;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>Approval e-mails to the employee who RAISED a leave request.</summary>
    public interface ILeaveNotifier
    {
        /// <summary>
        /// An annual-leave request has just entered its approval workflow — the hop that tells the
        /// APPROVER there is something waiting. Call AFTER the workflow has started, so the running
        /// instance exists to resolve the current step's approvers from.
        /// </summary>
        Task AnnualLeaveSubmittedAsync(Guid annualLeaveHeaderId);
        /// <summary>The requester's annual-leave request has been approved.</summary>
        Task AnnualLeaveApprovedAsync(Guid annualLeaveHeaderId);
        /// <summary>The requester's other-leave request has been approved.</summary>
        /// <summary>
        /// An other-leave request has just entered its approval workflow. Same contract as
        /// <see cref="AnnualLeaveSubmittedAsync"/>: call AFTER the workflow has started.
        /// </summary>
        Task OtherLeaveSubmittedAsync(Guid otherLeaveHeaderId);
        Task OtherLeaveApprovedAsync(Guid otherLeaveHeaderId);
    }

    /// <summary>
    /// Tells the REQUESTER their leave was approved.
    ///
    /// <para>This mail matters more here than for most events, because approval is the moment the
    /// request LEAVES every list the employee was watching: the approver's inbox drops it (the
    /// workflow instance is no longer running) and the requester's "My Pending Requests" feed drops
    /// it too (that feed is filtered to Pending by design). Without a message, an employee who
    /// submitted leave would simply find it gone and have to go looking in their profile to learn
    /// whether it was granted or refused.</para>
    ///
    /// <para>Runs AFTER the approving transaction has committed and NEVER throws: the e-mail service
    /// itself returns false rather than throwing when mail is disabled or unconfigured, and any
    /// resolution error is caught and logged here. An approval must never fail because a
    /// notification could not be sent — the leave is granted either way.</para>
    /// </summary>
    public class LeaveNotifier(
        IRepository<AnnualLeaveHeader> annualLeave,
        IRepository<OtherLeaveHeader> otherLeave,
        IRepository<LeaveBalance> ledgers,
        IRepository<Employee> employees,
        IRepository<WorkflowInstance> workflowInstances,
        IEmailService emailService,
        INotificationDispatcher dispatcher,
        ILogger<LeaveNotifier> logger) : ILeaveNotifier
    {
        public async Task AnnualLeaveSubmittedAsync(Guid annualLeaveHeaderId)
        {
            try
            {
                // Same tenant-filter caveat as the approval path: this runs from the submitting
                // request, but the header is read without the filter for consistency with the rest
                // of this class.
                var header = await annualLeave.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(h => h.Id == annualLeaveHeaderId)
                    .Select(h => new { h.Id, h.AnnualLeaveLedgerId, h.TotalLeaveDays })
                    .FirstOrDefaultAsync();
                if (header is null) return;

                var employeeId = await ledgers.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(b => b.Id == header.AnnualLeaveLedgerId)
                    .Select(b => (Guid?)b.EmployeeId).FirstOrDefaultAsync();
                if (employeeId is null) return;

                var dates = await annualLeave.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(h => h.Id == annualLeaveHeaderId)
                    .SelectMany(h => h.Details.Select(d => new { d.StartDate, d.EndDate }))
                    .ToListAsync();

                await DispatchSubmittedAsync(
                    WorkflowEntityTypes.AnnualLeave, annualLeaveHeaderId, employeeId.Value,
                    "Annual leave", header.TotalLeaveDays,
                    [.. dates.Select(d => (d.StartDate, d.EndDate))]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Leave-submitted notification failed for annual leave {Id}; the request itself is unaffected.",
                    annualLeaveHeaderId);
            }
        }

        /// <summary>
        /// Dispatches Leave.Submitted with the workflow coordinates, which is what lets a
        /// "current approver" recipient rule resolve to the people actually holding this step.
        ///
        /// <para>⚠️ Without the running instance there is no step, so a CurrentApprover rule would
        /// resolve to nobody. A request whose entity type has no active workflow definition simply
        /// has no approver to tell — that is not an error, and nothing is sent.</para>
        /// </summary>
        private async Task DispatchSubmittedAsync(
            string entityType, Guid entityId, Guid employeeId, string leaveName, decimal totalDays,
            IReadOnlyList<(DateTime StartDate, DateTime EndDate)> dates)
        {
            var instance = await workflowInstances.GetAllWithoutTenantFilter().AsNoTracking()
                .Where(w => w.EntityType == entityType && w.EntityId == entityId)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new { w.DefinitionId, w.CurrentStepOrder, w.CurrentStepName })
                .FirstOrDefaultAsync();

            var employee = await employees.GetAllWithoutTenantFilter().AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                })
                .FirstOrDefaultAsync();

            var sent = await dispatcher.DispatchAsync(new NotificationContext(
                NotificationEvents.LeaveSubmitted,
                new Dictionary<string, string?>
                {
                    ["EmployeeName"] = employee?.Name,
                    ["EmployeeNumber"] = employee?.EmployeeNumber,
                    ["LeaveType"] = leaveName,
                    ["TotalDays"] = totalDays.ToString("0.##"),
                    ["StartDate"] = dates.Count > 0 ? dates[0].StartDate.ToString("dd MMM yyyy") : null,
                    ["EndDate"] = dates.Count > 0 ? dates[^1].EndDate.ToString("dd MMM yyyy") : null,
                    ["RequestDate"] = DateTime.UtcNow.ToString("dd MMM yyyy"),
                    ["StepName"] = instance?.CurrentStepName,
                },
                RequesterEmployeeId: employeeId,
                WorkflowDefinitionId: instance?.DefinitionId,
                StepOrder: instance?.CurrentStepOrder,
                EntityType: entityType,
                EntityId: entityId));

            logger.LogInformation(
                "Leave submitted {EntityType} {EntityId}: {Count} configured notification(s) sent.",
                entityType, entityId, sent);
        }

        public async Task AnnualLeaveApprovedAsync(Guid annualLeaveHeaderId)
        {
            try
            {
                // Read WITHOUT the tenant filter: this runs from the workflow callback, where the
                // ambient tenant is whoever approved, not necessarily the requester's context.
                var header = await annualLeave.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(h => h.Id == annualLeaveHeaderId)
                    .Select(h => new { h.Id, h.AnnualLeaveLedgerId, h.TotalLeaveDays, h.RequestDate })
                    .FirstOrDefaultAsync();
                if (header is null) return;

                // The annual request carries the LEDGER, not the employee — resolve through it.
                var employeeId = await ledgers.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(b => b.Id == header.AnnualLeaveLedgerId)
                    .Select(b => (Guid?)b.EmployeeId).FirstOrDefaultAsync();
                if (employeeId is null) return;

                var dates = await annualLeave.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(h => h.Id == annualLeaveHeaderId)
                    .SelectMany(h => h.Details)
                    .OrderBy(d => d.StartDate)
                    .Select(d => new { d.StartDate, d.EndDate })
                    .ToListAsync();

                await SendAsync(employeeId.Value, "Annual leave", header.TotalLeaveDays,
                    dates.Select(d => (d.StartDate, d.EndDate)).ToList());
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Annual leave approval e-mail failed for {Id}", annualLeaveHeaderId);
            }
        }

        public async Task OtherLeaveSubmittedAsync(Guid otherLeaveHeaderId)
        {
            try
            {
                var header = await otherLeave.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(h => h.Id == otherLeaveHeaderId)
                    .Select(h => new
                    {
                        h.EmployeeId,
                        h.TotalLeaveDays,
                        LeaveName = h.Setting != null && h.Setting.LeaveType != null
                            ? h.Setting.LeaveType.Name : "Leave",
                        Dates = h.Details.OrderBy(d => d.StartDate)
                            .Select(d => new { d.StartDate, d.EndDate }).ToList()
                    })
                    .FirstOrDefaultAsync();
                if (header is null) return;

                await DispatchSubmittedAsync(
                    WorkflowEntityTypes.OtherLeave, otherLeaveHeaderId, header.EmployeeId,
                    header.LeaveName, header.TotalLeaveDays,
                    [.. header.Dates.Select(d => (d.StartDate, d.EndDate))]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Leave-submitted notification failed for other leave {Id}; the request itself is unaffected.",
                    otherLeaveHeaderId);
            }
        }

        public async Task OtherLeaveApprovedAsync(Guid otherLeaveHeaderId)
        {
            try
            {
                var header = await otherLeave.GetAllWithoutTenantFilter().AsNoTracking()
                    .Where(h => h.Id == otherLeaveHeaderId)
                    .Select(h => new
                    {
                        h.EmployeeId,
                        h.TotalLeaveDays,
                        LeaveName = h.Setting != null && h.Setting.LeaveType != null
                            ? h.Setting.LeaveType.Name : "Leave",
                        Dates = h.Details.OrderBy(d => d.StartDate)
                            .Select(d => new { d.StartDate, d.EndDate }).ToList()
                    })
                    .FirstOrDefaultAsync();
                if (header is null) return;

                await SendAsync(header.EmployeeId, header.LeaveName, header.TotalLeaveDays,
                    header.Dates.Select(d => (d.StartDate, d.EndDate)).ToList());
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Other leave approval e-mail failed for {Id}", otherLeaveHeaderId);
            }
        }

        /// <summary>
        /// Dispatches the administrator's Leave.Approved template and, only if none is configured,
        /// falls back to the original hardcoded message to the requester.
        /// </summary>
        private async Task SendAsync(
            Guid employeeId, string leaveName, decimal totalDays,
            IReadOnlyList<(DateTime StartDate, DateTime EndDate)> dates)
        {
            var employee = await employees.GetAllWithoutTenantFilter().AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => new
                {
                    e.Email,
                    e.EmployeeNumber,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                })
                .FirstOrDefaultAsync();

            if (employee is null) return;

            var when = dates.Count == 0
                ? ""
                : "\n\nDates:\n" + string.Join("\n", dates.Select(d =>
                    d.StartDate.Date == d.EndDate.Date
                        ? $"  • {d.StartDate:dd MMM yyyy}"
                        : $"  • {d.StartDate:dd MMM yyyy} → {d.EndDate:dd MMM yyyy}"));

            var subject = $"{leaveName} approved — {totalDays:0.##} day(s)";
            var body =
                $"Dear {employee.Name},\n\n" +
                $"Your {leaveName.ToLowerInvariant()} request for {totalDays:0.##} day(s) has been APPROVED." +
                when +
                "\n\nThe request has now left your pending list. You can find it under the leave tab of your " +
                "employee profile.\n\nThis is an automated message — please do not reply.";

            // The ADMINISTRATOR's template wins, if they wrote one. It decides both the wording and
            // WHO hears about it — the requester, HR, the whole company — none of which this class
            // gets to assume any more.
            var dispatched = await dispatcher.DispatchAsync(new NotificationContext(
                NotificationEvents.LeaveApproved,
                new Dictionary<string, string?>
                {
                    ["EmployeeName"] = employee.Name,
                    ["EmployeeNumber"] = employee.EmployeeNumber,
                    ["LeaveType"] = leaveName,
                    ["TotalDays"] = totalDays.ToString("0.##"),
                    ["StartDate"] = dates.Count > 0 ? dates[0].StartDate.ToString("dd MMM yyyy") : null,
                    ["EndDate"] = dates.Count > 0 ? dates[^1].EndDate.ToString("dd MMM yyyy") : null,
                },
                RequesterEmployeeId: employeeId));

            if (dispatched > 0)
            {
                logger.LogInformation(
                    "Leave approval: {Count} configured notification(s) sent for employee {EmployeeId}.",
                    dispatched, employeeId);
                return;
            }

            // ⚠️ FALLBACK, on purpose. A client who has not written a template yet must not silently
            // stop being told their leave was approved — making the feature available should not be
            // the same as switching the old behaviour off. Once a template exists for
            // Leave.Approved, this line stops running.
            //
            // ⚠️ The address check belongs HERE, not before the dispatch. It gates only this
            // hardcoded mail, which can reach nobody but the requester. Checking it earlier let one
            // employee's missing address silently cancel an administrator's template — including
            // rules addressed to HR or the whole company, who have nothing to do with that address.
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                // Not an error: plenty of employee records carry no address. Log it so an unexplained
                // "I was never told" can be traced to the missing address rather than to the mail.
                logger.LogInformation(
                    "Leave approved for employee {EmployeeId} but no e-mail address is on file, and no template is configured; no message sent.",
                    employeeId);
                return;
            }

            var sent = await emailService.SendAsync(employee.Email!, subject, body);
            logger.LogInformation("Leave approval e-mail to {Email} for employee {EmployeeId}: {Result} (no template configured)",
                employee.Email, employeeId, sent ? "sent" : "not sent (mail disabled or failed)");
        }
    }
}
