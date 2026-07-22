using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    /// <summary>Stakeholder notifications for personnel-movement lifecycle events (HC172/HC175).</summary>
    public interface IMovementNotifier
    {
        Task SubmittedAsync(Guid movementId);
        Task ApprovedAsync(Guid movementId);
        Task ExecutedAsync(Guid movementId);
        Task CancelledAsync(Guid movementId);
    }

    /// <summary>
    /// Composes and sends movement notifications to the employee and the current/target unit managers.
    /// Runs AFTER the business transaction commits and never throws — the e-mail service itself never
    /// throws (disabled/unconfigured mail returns false), and any resolution error is logged and
    /// swallowed so the movement operation always stands. Approvers get their in-app "My Approvals"
    /// inbox from the workflow engine; these mails cover the surrounding stakeholders.
    /// </summary>
    public class MovementNotifier(
        IRepository<EmployeeMovement> movementRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IOrgManagerResolver managerResolver,
        IEmailService emailService,
        ILogger<MovementNotifier> logger) : IMovementNotifier
    {
        public Task SubmittedAsync(Guid movementId) => NotifyAsync(movementId,
            "submitted for approval",
            "Your {0} request effective {1} has been submitted and routed for approval.");

        public Task ApprovedAsync(Guid movementId) => NotifyAsync(movementId,
            "approved",
            "Your {0} effective {1} has been APPROVED. It will be applied on the effective date.");

        public Task ExecutedAsync(Guid movementId) => NotifyAsync(movementId,
            "executed",
            "Your {0} effective {1} has been applied — your organizational records now reflect the new assignment.");

        public Task CancelledAsync(Guid movementId) => NotifyAsync(movementId,
            "cancelled",
            "Your {0} request effective {1} has been cancelled/rejected. The process is terminated.");

        private async Task NotifyAsync(Guid movementId, string eventLabel, string bodyTemplate)
        {
            try
            {
                var movement = await movementRepository.GetAllWithoutTenantFilter().AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == movementId);
                if (movement is null) return;

                var employee = await employeeRepository.GetAllWithoutTenantFilter()
                    .Where(e => e.Id == movement.EmployeeId)
                    .Select(e => new
                    {
                        e.Email,
                        Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                    })
                    .FirstOrDefaultAsync();
                if (employee is null) return;

                var subject = $"{movement.MovementType} {eventLabel} — {employee.Name}";
                var body = string.Format(bodyTemplate, movement.MovementType, movement.EffectiveDate.ToString("dd MMM yyyy"))
                           + (string.IsNullOrWhiteSpace(movement.Reason) ? "" : $"\n\nReason: {movement.Reason}");

                // The employee themselves.
                if (!string.IsNullOrWhiteSpace(employee.Email))
                    await emailService.SendAsync(employee.Email!, subject, $"Dear {employee.Name},\n\n{body}");

                // Current + target unit managers (stakeholders, HC175) — resolved from the org tree.
                var managerEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var positionId in new[] { movement.FromPositionId, movement.ToPositionId })
                {
                    if (!positionId.HasValue) continue;
                    var unitId = await positionRepository.GetAllWithoutTenantFilter()
                        .Where(p => p.Id == positionId.Value)
                        .Select(p => (Guid?)p.OrganizationUnitId)
                        .FirstOrDefaultAsync();
                    if (unitId is null) continue;
                    var resolved = await managerResolver.ResolveUnitManagerAsync(unitId.Value, movement.EmployeeId);
                    if (resolved is null || resolved.EmployeeIds.Count == 0) continue;
                    var mgrIds = resolved.EmployeeIds.ToList();
                    var emails = await employeeRepository.GetAllWithoutTenantFilter()
                        .Where(e => mgrIds.Contains(e.Id) && e.Email != null && e.Email != "")
                        .Select(e => e.Email!)
                        .ToListAsync();
                    foreach (var m in emails) managerEmails.Add(m);
                }
                foreach (var to in managerEmails)
                    await emailService.SendAsync(to, subject,
                        $"Personnel action update for {employee.Name}:\n\n{body}");
            }
            catch (Exception ex)
            {
                // Notification mail must never break the movement operation.
                logger.LogWarning(ex, "Movement notification ({Event}) failed for {MovementId}", eventLabel, movementId);
            }
        }
    }
}
