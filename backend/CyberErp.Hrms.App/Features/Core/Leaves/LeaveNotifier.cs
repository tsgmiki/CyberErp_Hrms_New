using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>Approval e-mails to the employee who RAISED a leave request.</summary>
    public interface ILeaveNotifier
    {
        /// <summary>The requester's annual-leave request has been approved.</summary>
        Task AnnualLeaveApprovedAsync(Guid annualLeaveHeaderId);
        /// <summary>The requester's other-leave request has been approved.</summary>
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
        IEmailService emailService,
        ILogger<LeaveNotifier> logger) : ILeaveNotifier
    {
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

        /// <summary>Composes and sends the one message. Silent (logged) when the employee has no address.</summary>
        private async Task SendAsync(
            Guid employeeId, string leaveName, decimal totalDays,
            IReadOnlyList<(DateTime StartDate, DateTime EndDate)> dates)
        {
            var employee = await employees.GetAllWithoutTenantFilter().AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => new
                {
                    e.Email,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                })
                .FirstOrDefaultAsync();

            if (employee is null || string.IsNullOrWhiteSpace(employee.Email))
            {
                // Not an error: plenty of employee records carry no address. Log it so an unexplained
                // "I was never told" can be traced to the missing address rather than to the mail.
                logger.LogInformation(
                    "Leave approved for employee {EmployeeId} but no e-mail address is on file; no message sent.",
                    employeeId);
                return;
            }

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

            var sent = await emailService.SendAsync(employee.Email!, subject, body);
            logger.LogInformation("Leave approval e-mail to {Email} for employee {EmployeeId}: {Result}",
                employee.Email, employeeId, sent ? "sent" : "not sent (mail disabled or failed)");
        }
    }
}
