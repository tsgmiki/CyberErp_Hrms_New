using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    /// <summary>One active disciplinary measure that bears on eligibility (HC224/HC225).</summary>
    public class DisciplinaryBlockDto
    {
        public Guid Id { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string MeasureType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ValidUntil { get; set; }
        public bool AffectsPromotion { get; set; }
        public bool AffectsReward { get; set; }
    }

    /// <summary>Disciplinary eligibility snapshot for one employee (HC225).</summary>
    public class DisciplinaryEligibilityDto
    {
        public Guid EmployeeId { get; set; }
        public bool IsBlockedForPromotion { get; set; }
        public bool IsBlockedForReward { get; set; }
        /// <summary>The active, in-lifetime, flag-bearing measures behind the block (empty when clear).</summary>
        public List<DisciplinaryBlockDto> ActiveMeasures { get; set; } = [];
    }

    /// <summary>
    /// HC224/HC225 — the single source other modules query to learn whether an employee's active
    /// disciplinary measures block promotion or reward. "Active" = not Cancelled, still within its
    /// lifetime (<see cref="DisciplinaryMeasure.ValidUntil"/>), and flagged for the relevant area
    /// (opt-in hard block: a measure only counts when HR set AffectsPromotion / AffectsReward).
    /// Mirrors <c>IWorkflowGate</c>: an Evaluate read plus throwing Ensure guards.
    /// </summary>
    public interface IDisciplinaryEligibilityService
    {
        Task<DisciplinaryEligibilityDto> EvaluateAsync(Guid employeeId);
        Task EnsureEligibleForPromotionAsync(Guid employeeId);
        Task EnsureEligibleForRewardAsync(Guid employeeId);
    }

    public class DisciplinaryEligibilityService(
        IRepository<DisciplinaryMeasure> repository) : IDisciplinaryEligibilityService
    {
        public async Task<DisciplinaryEligibilityDto> EvaluateAsync(Guid employeeId)
        {
            var today = DateTime.UtcNow.Date;

            // Single indexed query (composite EmployeeId, Status): non-cancelled, still in lifetime,
            // and flagged for at least one eligibility area. Per-employee, low volume.
            var active = await repository.GetAll().AsNoTracking()
                .Where(d => d.EmployeeId == employeeId
                    && d.Status != DisciplinaryStatus.Cancelled
                    && (d.ValidUntil == null || d.ValidUntil >= today)
                    && (d.AffectsPromotion || d.AffectsReward))
                .Select(d => new DisciplinaryBlockDto
                {
                    Id = d.Id,
                    ViolationType = d.ViolationType,
                    MeasureType = d.MeasureType.ToString(),
                    Status = d.Status.ToString(),
                    ValidUntil = d.ValidUntil,
                    AffectsPromotion = d.AffectsPromotion,
                    AffectsReward = d.AffectsReward
                })
                .ToListAsync();

            return new DisciplinaryEligibilityDto
            {
                EmployeeId = employeeId,
                ActiveMeasures = active,
                IsBlockedForPromotion = active.Any(m => m.AffectsPromotion),
                IsBlockedForReward = active.Any(m => m.AffectsReward)
            };
        }

        public async Task EnsureEligibleForPromotionAsync(Guid employeeId)
        {
            var eligibility = await EvaluateAsync(employeeId);
            if (eligibility.IsBlockedForPromotion)
                throw new ValidationException("discipline",
                    "This employee has an active disciplinary measure that blocks promotion.");
        }

        public async Task EnsureEligibleForRewardAsync(Guid employeeId)
        {
            var eligibility = await EvaluateAsync(employeeId);
            if (eligibility.IsBlockedForReward)
                throw new ValidationException("discipline",
                    "This employee has an active disciplinary measure that blocks reward/recognition.");
        }
    }
}
