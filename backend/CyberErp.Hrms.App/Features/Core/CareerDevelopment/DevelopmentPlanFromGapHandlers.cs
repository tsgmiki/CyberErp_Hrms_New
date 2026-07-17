using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.CareerDevelopment
{
    public class CreateDevelopmentPlanResultDto
    {
        public Guid DevelopmentPlanId { get; set; }
        public int ActionCount { get; set; }
    }

    // ---- HC130 / HC155 / HC156: structured Individual Development Plan from a competency gap ----
    public interface ICreateDevelopmentPlanFromGap
    {
        /// <summary>Build an IDP from an employee's next career-step gap (HC130/HC164).</summary>
        Task<CreateDevelopmentPlanResultDto> FromCareerPathAsync(Guid employeeCareerPathId);
        /// <summary>Build an IDP from a succession candidate's target-role gap (HC155/HC156).</summary>
        Task<CreateDevelopmentPlanResultDto> FromSuccessionCandidateAsync(Guid successionCandidateId);
    }

    /// <summary>
    /// Turns a career/succession competency gap into a STRUCTURED Individual Development Plan on the
    /// Performance engine (HC130/HC155) — one development action per missing competency, each tied to the
    /// <see cref="Competency"/> and a learning intervention. Reuses the existing gap analysers.
    /// </summary>
    public class CreateDevelopmentPlanFromGap(
        IGetCareerPathRecommendations recommendationsHandler,
        IGetSuccessionCandidateGap gapHandler,
        IRepository<SuccessionCandidate> candidateRepository,
        IRepository<IndividualDevelopmentPlan> planRepository,
        ILogger<CreateDevelopmentPlanFromGap> logger) : ICreateDevelopmentPlanFromGap
    {
        public async Task<CreateDevelopmentPlanResultDto> FromCareerPathAsync(Guid employeeCareerPathId)
        {
            var recos = await recommendationsHandler.GetAsync(employeeCareerPathId);
            if (recos.Recommendations.Count == 0)
                throw new ValidationException("recommendations", "No competency gaps to build a development plan from — the employee already meets the next step's requirements.");

            var title = recos.TargetStepName != null ? $"Career development plan — toward {recos.TargetStepName}" : "Career development plan";
            var specs = recos.Recommendations.Select((r, i) => new DevelopmentActionSpec(
                null, $"Develop: {r.Name}", r.CompetencyId, r.SuggestedAction, null, DevelopmentActionStatus.Planned, 0, i)).ToList();
            return await CreatePlanAsync(recos.EmployeeId, title,
                "Auto-generated from the career-path competency gap (HC130/HC164).", specs);
        }

        public async Task<CreateDevelopmentPlanResultDto> FromSuccessionCandidateAsync(Guid successionCandidateId)
        {
            var employeeId = await candidateRepository.GetAll().Where(c => c.Id == successionCandidateId)
                .Select(c => c.EmployeeId).FirstOrDefaultAsync();
            if (employeeId == Guid.Empty)
                throw new NotFoundException(nameof(SuccessionCandidate), successionCandidateId.ToString());

            var gap = await gapHandler.GetAsync(successionCandidateId);
            if (gap.Gaps.Count == 0)
                throw new ValidationException("gap", "No competency gaps to build a development plan from — the successor already covers the target role's competencies.");

            var specs = gap.Gaps.Select((g, i) => new DevelopmentActionSpec(
                null, $"Develop: {g.Name}", g.CompetencyId, "Training", null, DevelopmentActionStatus.Planned, 0, i)).ToList();
            return await CreatePlanAsync(employeeId, "Succession development plan",
                "Auto-generated from the successor competency gap (HC155/HC156).", specs);
        }

        private async Task<CreateDevelopmentPlanResultDto> CreatePlanAsync(Guid employeeId, string title, string description, List<DevelopmentActionSpec> specs)
        {
            var now = DateTime.UtcNow;
            var plan = IndividualDevelopmentPlan.Create(employeeId, title, now, now.AddDays(90), description, status: DevelopmentPlanStatus.Active);
            plan.SetActions(specs);
            await planRepository.AddAsync(plan);
            // The repository stamps only the aggregate root — cascade-inserted actions copy its tenant here.
            foreach (var a in plan.Actions)
                if (string.IsNullOrEmpty(a.TenantId)) a.TenantId = plan.TenantId;
            await planRepository.SaveChangesAsync();

            logger.LogInformation("Created IDP {Id} with {N} action(s) from a competency gap for employee {Emp}", plan.Id, plan.Actions.Count, employeeId);
            return new CreateDevelopmentPlanResultDto { DevelopmentPlanId = plan.Id, ActionCount = plan.Actions.Count };
        }
    }
}
