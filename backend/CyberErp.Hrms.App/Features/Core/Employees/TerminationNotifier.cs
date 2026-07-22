using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    /// <summary>Stakeholder notifications for exit-case lifecycle events (HC209/HC220).</summary>
    public interface ITerminationNotifier
    {
        Task SubmittedAsync(Guid terminationId);
        Task ApprovedAsync(Guid terminationId);
        Task SettledAsync(Guid terminationId);
        Task CancelledAsync(Guid terminationId);
    }

    /// <summary>
    /// Composes and sends exit-case notifications to the employee and their unit manager. Runs AFTER
    /// the business transaction commits and never throws — the e-mail service itself never throws
    /// (disabled/unconfigured mail returns false), and any resolution error is logged and swallowed
    /// so the exit operation always stands. HR approvers get their in-app "My Approvals" inbox from
    /// the workflow engine; these mails cover the surrounding stakeholders (HC209: a request raised
    /// by the employee reaches HR personnel and managers).
    /// </summary>
    public class TerminationNotifier(
        IRepository<EmployeeTermination> terminationRepository,
        IRepository<Employee> employeeRepository,
        IOrgManagerResolver managerResolver,
        IEmailService emailService,
        ILogger<TerminationNotifier> logger) : ITerminationNotifier
    {
        public Task SubmittedAsync(Guid terminationId) => NotifyAsync(terminationId,
            "exit request submitted",
            "A {0} exit case (last working day {1}) has been submitted and routed for approval.");

        public Task ApprovedAsync(Guid terminationId) => NotifyAsync(terminationId,
            "exit approved — clearance opened",
            "The {0} exit case (last working day {1}) has been APPROVED. The departmental clearance checklist is now open.");

        public Task SettledAsync(Guid terminationId) => NotifyAsync(terminationId,
            "exit settled",
            "The {0} exit case (last working day {1}) has been SETTLED. The employment record is now inactive.");

        public Task CancelledAsync(Guid terminationId) => NotifyAsync(terminationId,
            "exit cancelled",
            "The {0} exit case (last working day {1}) has been cancelled/rejected. The process is terminated.");

        private async Task NotifyAsync(Guid terminationId, string eventLabel, string bodyTemplate)
        {
            try
            {
                var termination = await terminationRepository.GetAllWithoutTenantFilter().AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == terminationId);
                if (termination is null) return;

                var employee = await employeeRepository.GetAllWithoutTenantFilter()
                    .Where(e => e.Id == termination.EmployeeId)
                    .Select(e => new
                    {
                        e.Email,
                        UnitId = e.Position != null ? (Guid?)e.Position.OrganizationUnitId : null,
                        Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                    })
                    .FirstOrDefaultAsync();
                if (employee is null) return;

                var subject = $"{termination.TerminationType} {eventLabel} — {employee.Name}";
                var body = string.Format(bodyTemplate, termination.TerminationType,
                               termination.LastWorkingDate.ToString("dd MMM yyyy"))
                           + (string.IsNullOrWhiteSpace(termination.Reason) ? "" : $"\n\nReason: {termination.Reason}");

                // The employee themselves.
                if (!string.IsNullOrWhiteSpace(employee.Email))
                    await emailService.SendAsync(employee.Email!, subject, $"Dear {employee.Name},\n\n{body}");

                // Their unit manager (stakeholder) — resolved from the org tree, excluding the employee.
                if (employee.UnitId is Guid unitId)
                {
                    var manager = await managerResolver.ResolveUnitManagerAsync(unitId, termination.EmployeeId);
                    if (manager is not null)
                    {
                        var managerEmails = await employeeRepository.GetAllWithoutTenantFilter()
                            .Where(e => manager.EmployeeIds.Contains(e.Id) && e.Email != null && e.Email != "")
                            .Select(e => e.Email!).ToListAsync();
                        foreach (var email in managerEmails.Distinct(StringComparer.OrdinalIgnoreCase))
                            await emailService.SendAsync(email, subject,
                                $"Team update:\n\n{body}\n\nEmployee: {employee.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Termination notification ({Event}) failed for {Id}", eventLabel, terminationId);
            }
        }
    }
}
