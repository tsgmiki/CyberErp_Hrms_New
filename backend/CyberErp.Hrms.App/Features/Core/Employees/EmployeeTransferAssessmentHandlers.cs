using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs (HC173: eligibility + impact, advisory — flags, not hard blocks) ----

    public class TransferPlacementDto
    {
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public string? BranchName { get; set; }
    }

    public class TransferCompetencyDto
    {
        public Guid CompetencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        /// <summary>The current role's competency profile already covers this requirement.</summary>
        public bool CoveredByCurrentRole { get; set; }
    }

    public class TransferUnitImpactDto
    {
        public string? UnitName { get; set; }
        public int TotalPositions { get; set; }
        public int VacantPositions { get; set; }
    }

    public class TransferAssessmentDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public TransferPlacementDto Current { get; set; } = new();
        public TransferPlacementDto Target { get; set; } = new();

        // Eligibility inputs
        /// <summary>Latest appraisal overall score normalised 0–100 against its rating scale (null = none).</summary>
        public decimal? PerformanceScorePercent { get; set; }
        public string? PerformanceCycleName { get; set; }
        public int TenureMonthsTotal { get; set; }
        /// <summary>Months since the last executed movement (or hire) — tenure in the CURRENT role.</summary>
        public int TenureMonthsInCurrentRole { get; set; }
        public List<string> Qualifications { get; set; } = [];

        // Skill gap: the target position's competency requirements vs the current role's profile.
        public List<TransferCompetencyDto> TargetCompetencies { get; set; } = [];
        public int SkillGapCount { get; set; }

        // Impact tracking
        public TransferUnitImpactDto CurrentUnitImpact { get; set; } = new();
        public TransferUnitImpactDto TargetUnitImpact { get; set; } = new();

        // Budget: a transfer never changes pay (domain rule); relocation is captured on the request.
        public decimal? CurrentSalary { get; set; }
        public bool SalaryUnchanged { get; set; } = true;

        /// <summary>Advisory eligibility/impact warnings — HR decides; nothing here hard-blocks the request.</summary>
        public List<string> Flags { get; set; } = [];
    }

    public interface IAssessEmployeeTransfer { Task<TransferAssessmentDto> AssessAsync(Guid employeeId, Guid toPositionId); }

    /// <summary>
    /// HC173 — transfer eligibility + impact assessment. Read-only composition of ~8 small indexed
    /// single-record/count queries (no list scans): performance, tenure, qualifications, competency
    /// gap, unit vacancy impact and budget facts, plus advisory flags.
    /// </summary>
    public class AssessEmployeeTransfer(
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeMovement> movementRepository,
        IRepository<Position> positionRepository,
        IRepository<PositionCompetency> positionCompetencyRepository,
        IRepository<Competency> competencyRepository,
        IRepository<EmployeeEducation> educationRepository,
        IRepository<Appraisal> appraisalRepository,
        IRepository<ReviewCycle> reviewCycleRepository,
        IRepository<RatingScaleLevel> ratingLevelRepository,
        IDisciplinaryEligibilityService disciplineEligibility,
        Performance.IPerformanceVisibilityService visibility) : IAssessEmployeeTransfer
    {
        /// <summary>Advisory minimum tenure in the current role before a transfer (months).</summary>
        private const int MinTenureMonths = 12;

        public async Task<TransferAssessmentDto> AssessAsync(Guid employeeId, Guid toPositionId)
        {
            // Same visibility rule as initiating the request: self, their manager, or HR.
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException("employeeId", "You do not have access to assess this employee.");

            var employee = await employeeRepository.GetAll()
                .Where(e => e.Id == employeeId)
                .Select(e => new
                {
                    e.Id, e.PositionId, e.Salary, e.HireDate, e.PersonId,
                    Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var dto = new TransferAssessmentDto { EmployeeId = employee.Id, EmployeeName = employee.Name };

            dto.Current = await PlacementAsync(employee.PositionId);
            dto.Target = await PlacementAsync(toPositionId);
            if (dto.Target.PositionId is null)
                throw new NotFoundException(nameof(Position), toPositionId.ToString());

            // ---- Eligibility: performance, tenure, qualifications --------------------
            var latest = await appraisalRepository.GetAll().AsNoTracking()
                .Where(a => a.EmployeeId == employeeId && a.OverallScore != null)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new { a.OverallScore, a.ReviewCycleId })
                .FirstOrDefaultAsync();
            if (latest?.OverallScore is decimal score)
            {
                var cycle = await reviewCycleRepository.GetAll()
                    .Where(c => c.Id == latest.ReviewCycleId)
                    .Select(c => new { c.Name, c.RatingScaleId })
                    .FirstOrDefaultAsync();
                dto.PerformanceCycleName = cycle?.Name;
                var max = cycle is null ? 0m : await ratingLevelRepository.GetAll()
                    .Where(l => l.RatingScaleId == cycle.RatingScaleId)
                    .Select(l => (decimal?)l.Value).MaxAsync() ?? 0m;
                dto.PerformanceScorePercent = max > 0 ? Math.Round(score / max * 100m, 1) : null;
            }

            var now = DateTime.UtcNow.Date;
            var hire = employee.HireDate ?? now;
            dto.TenureMonthsTotal = Months(hire, now);
            // Tenure in the CURRENT role = since the last executed movement, else since hire.
            var lastMove = await movementRepository.GetAll()
                .Where(m => m.EmployeeId == employeeId && m.Status == MovementStatus.Completed)
                .OrderByDescending(m => m.EffectiveDate)
                .Select(m => (DateTime?)m.EffectiveDate)
                .FirstOrDefaultAsync();
            dto.TenureMonthsInCurrentRole = Months(lastMove ?? hire, now);

            dto.Qualifications = await educationRepository.GetAll()
                .Where(x => x.PersonId == employee.PersonId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => (x.EducationLevel ?? "") +
                             (x.FieldOfStudy != null ? " — " + x.FieldOfStudy : "") +
                             (x.Institution != null ? " (" + x.Institution + ")" : ""))
                .Take(10)
                .ToListAsync();

            // ---- Skill gap: target requirements vs the current role's profile --------
            var currentCompetencyIds = employee.PositionId.HasValue
                ? (await positionCompetencyRepository.GetAll()
                    .Where(pc => pc.PositionId == employee.PositionId.Value)
                    .Select(pc => pc.CompetencyId).ToListAsync()).ToHashSet()
                : [];
            dto.TargetCompetencies = await positionCompetencyRepository.GetAll()
                .Where(pc => pc.PositionId == toPositionId)
                .Join(competencyRepository.GetAll(), pc => pc.CompetencyId, c => c.Id,
                    (pc, c) => new TransferCompetencyDto { CompetencyId = c.Id, Name = c.Name, Weight = pc.Weight })
                .ToListAsync();
            foreach (var c in dto.TargetCompetencies)
                c.CoveredByCurrentRole = currentCompetencyIds.Contains(c.CompetencyId);
            dto.SkillGapCount = dto.TargetCompetencies.Count(c => !c.CoveredByCurrentRole);

            // ---- Impact: unit vacancy before/after + onboarding requirements ---------
            dto.CurrentUnitImpact = await UnitImpactAsync(dto.Current.OrganizationUnitId, dto.Current.OrganizationUnitName);
            dto.TargetUnitImpact = await UnitImpactAsync(dto.Target.OrganizationUnitId, dto.Target.OrganizationUnitName);

            // ---- Budget facts ---------------------------------------------------------
            dto.CurrentSalary = employee.Salary;
            dto.SalaryUnchanged = true;   // domain rule: a transfer never changes pay

            // ---- Advisory flags -------------------------------------------------------
            if (dto.PerformanceScorePercent is null)
                dto.Flags.Add("No completed appraisal on record — performance cannot be assessed.");
            if (dto.TenureMonthsInCurrentRole < MinTenureMonths)
                dto.Flags.Add($"Tenure in the current role is {dto.TenureMonthsInCurrentRole} month(s) — below the advisory minimum of {MinTenureMonths}.");
            if (dto.SkillGapCount > 0)
                dto.Flags.Add($"The target role requires {dto.SkillGapCount} competency(ies) not in the current role's profile — plan onboarding/training.");
            if (dto.Current.PositionId.HasValue)
                dto.Flags.Add($"Executing the transfer vacates '{dto.Current.PositionName}' in {dto.Current.OrganizationUnitName} — plan backfill.");

            // HC224/HC225 — surface active disciplinary measures (advisory here; a promotion movement
            // is hard-blocked separately when the measure is flagged AffectsPromotion).
            var discipline = await disciplineEligibility.EvaluateAsync(employeeId);
            if (discipline.ActiveMeasures.Count > 0)
            {
                var promo = discipline.IsBlockedForPromotion ? " (blocks promotion)" : "";
                dto.Flags.Add($"{discipline.ActiveMeasures.Count} active disciplinary measure(s) on record{promo} — review before proceeding.");
            }

            return dto;
        }

        private async Task<TransferPlacementDto> PlacementAsync(Guid? positionId)
        {
            if (!positionId.HasValue) return new TransferPlacementDto();
            return await positionRepository.GetAll()
                .Where(p => p.Id == positionId.Value)
                .Select(p => new TransferPlacementDto
                {
                    PositionId = p.Id,
                    PositionName = p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : ""),
                    OrganizationUnitId = p.OrganizationUnitId,
                    OrganizationUnitName = p.OrganizationUnit != null ? p.OrganizationUnit.Name : null,
                    BranchName = p.Branch != null ? p.Branch.Name : null
                })
                .FirstOrDefaultAsync() ?? new TransferPlacementDto();
        }

        private async Task<TransferUnitImpactDto> UnitImpactAsync(Guid? unitId, string? unitName)
        {
            if (!unitId.HasValue) return new TransferUnitImpactDto { UnitName = unitName };
            var counts = await positionRepository.GetAll()
                .Where(p => p.OrganizationUnitId == unitId.Value)
                .GroupBy(_ => 1)
                .Select(g => new { Total = g.Count(), Vacant = g.Count(p => p.IsVacant) })
                .FirstOrDefaultAsync();
            return new TransferUnitImpactDto
            {
                UnitName = unitName,
                TotalPositions = counts?.Total ?? 0,
                VacantPositions = counts?.Vacant ?? 0
            };
        }

        private static int Months(DateTime from, DateTime to) =>
            Math.Max(0, (to.Year - from.Year) * 12 + to.Month - from.Month - (to.Day < from.Day ? 1 : 0));
    }
}
