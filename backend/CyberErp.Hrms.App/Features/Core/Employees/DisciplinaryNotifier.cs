using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    /// <summary>Stakeholder notifications for the disciplinary-case lifecycle (HC222/HC220).</summary>
    public interface IDisciplinaryNotifier
    {
        Task SubmittedAsync(Guid measureId);
        Task ApprovedAsync(Guid measureId);
        Task CancelledAsync(Guid measureId);
    }

    /// <summary>
    /// Notifies the subject employee's unit manager (their supervisor / work-unit head) when a
    /// disciplinary case is raised, confirmed or voided. Mirrors <see cref="MovementNotifier"/>:
    /// runs AFTER the business transaction, resolves recipients from the org tree without the
    /// tenant filter, and never throws — a notification failure must never break the case operation.
    /// Approvers still get their in-app "My Approvals" inbox from the workflow engine.
    /// </summary>
    public class DisciplinaryNotifier(
        IRepository<DisciplinaryMeasure> measureRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IOrgManagerResolver managerResolver,
        IEmailService emailService,
        ILogger<DisciplinaryNotifier> logger) : IDisciplinaryNotifier
    {
        public Task SubmittedAsync(Guid measureId) => NotifyAsync(measureId,
            "raised",
            "A disciplinary case ({0}) has been raised for {1} and routed for review.");

        public Task ApprovedAsync(Guid measureId) => NotifyAsync(measureId,
            "confirmed",
            "The disciplinary case ({0}) for {1} has been reviewed and CONFIRMED.");

        public Task CancelledAsync(Guid measureId) => NotifyAsync(measureId,
            "cancelled",
            "The disciplinary case ({0}) for {1} has been cancelled/voided.");

        private async Task NotifyAsync(Guid measureId, string eventLabel, string bodyTemplate)
        {
            try
            {
                var measure = await measureRepository.GetAllWithoutTenantFilter().AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == measureId);
                if (measure is null) return;

                var subject = await employeeRepository.GetAllWithoutTenantFilter()
                    .Where(e => e.Id == measure.EmployeeId)
                    .Select(e => new
                    {
                        Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber,
                        UnitId = e.Position != null ? (Guid?)e.Position.OrganizationUnitId : null
                    })
                    .FirstOrDefaultAsync();
                if (subject is null || subject.UnitId is null) return;

                var mailSubject = $"Disciplinary case {eventLabel} — {subject.Name}";
                var body = string.Format(bodyTemplate, measure.ViolationType, subject.Name);

                // The subject's unit manager (supervisor / work-unit head) — the key stakeholder.
                var resolved = await managerResolver.ResolveUnitManagerAsync(subject.UnitId.Value, measure.EmployeeId);
                if (resolved is null || resolved.EmployeeIds.Count == 0) return;

                var mgrIds = resolved.EmployeeIds.ToList();
                var emails = await employeeRepository.GetAllWithoutTenantFilter()
                    .Where(e => mgrIds.Contains(e.Id) && e.Email != null && e.Email != "")
                    .Select(e => e.Email!)
                    .ToListAsync();

                foreach (var to in emails)
                    await emailService.SendAsync(to, mailSubject, body);
            }
            catch (Exception ex)
            {
                // Notification mail must never break the disciplinary operation.
                logger.LogWarning(ex, "Disciplinary notification ({Event}) failed for {MeasureId}", eventLabel, measureId);
            }
        }
    }
}
