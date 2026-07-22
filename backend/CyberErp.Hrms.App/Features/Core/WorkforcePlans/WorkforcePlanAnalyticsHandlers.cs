using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.WorkforcePlans
{
    // ---- Interfaces -----------------------------------------------------------

    public interface IGetEstablishmentOverview { Task<List<EstablishmentRowDto>> GetAsync(Guid? organizationUnitId); }
    public interface IPopulateWorkforcePlan { Task<int> PopulateAsync(Guid planId); }
    public interface ISuggestPlanSeparations { Task<List<SeparationSuggestionDto>> GetAsync(Guid planId); }
    public interface IGetWorkforcePlanSummary { Task<WorkforcePlanSummaryDto> GetAsync(Guid planId); }
    public interface ICompareWorkforcePlans { Task<List<WorkforcePlanComparisonDto>> GetAsync(IReadOnlyList<Guid> planIds); }
    public interface IGetApprovedDemand { Task<List<ApprovedDemandRowDto>> GetAsync(); }

    // ---- Shared establishment query -----------------------------------------------

    internal static class EstablishmentShared
    {
        internal record EstablishmentGroup(Guid OrganizationUnitId, Guid PositionClassId, int Authorized, int Filled, int Vacant);

        /// <summary>
        /// Live establishment per unit × role from the position seats (HC055/HC056): a Position row
        /// is one authorized seat; IsVacant (kept in sync by the employee handlers) splits
        /// filled from vacant. Optionally scoped to an organization-unit subtree (HC054).
        /// </summary>
        internal static async Task<List<EstablishmentGroup>> QueryAsync(
            IRepository<Position> positions, HashSet<Guid>? scopeUnitIds)
        {
            var query = positions.GetAll();
            if (scopeUnitIds is not null)
                query = query.Where(p => scopeUnitIds.Contains(p.OrganizationUnitId));

            return (await query
                    .GroupBy(p => new { p.OrganizationUnitId, p.PositionClassId })
                    .Select(g => new
                    {
                        g.Key.OrganizationUnitId,
                        g.Key.PositionClassId,
                        Authorized = g.Count(),
                        Vacant = g.Count(x => x.IsVacant)
                    })
                    .ToListAsync())
                .Select(g => new EstablishmentGroup(
                    g.OrganizationUnitId, g.PositionClassId, g.Authorized, g.Authorized - g.Vacant, g.Vacant))
                .ToList();
        }

        /// <summary>
        /// Resolves the id set of a unit's subtree (unit + all descendants) for scope filtering,
        /// or null for organization-wide (no filter). Units are few — the walk happens in memory.
        /// </summary>
        internal static async Task<HashSet<Guid>?> ResolveSubtreeAsync(
            IRepository<OrganizationUnit> units, Guid? rootUnitId)
        {
            if (!rootUnitId.HasValue) return null;

            var all = await units.GetAll()
                .Select(u => new { u.Id, u.ParentId })
                .ToListAsync();
            var byParent = all.Where(u => u.ParentId.HasValue)
                .GroupBy(u => u.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(u => u.Id).ToList());

            var result = new HashSet<Guid> { rootUnitId.Value };
            var queue = new Queue<Guid>([rootUnitId.Value]);
            while (queue.Count > 0)
                foreach (var child in byParent.GetValueOrDefault(queue.Dequeue()) ?? [])
                    if (result.Add(child))
                        queue.Enqueue(child);
            return result;
        }
    }

    // ---- Establishment overview (HC056 + vacancy aging, HC073) ------------------------

    public class GetEstablishmentOverview(
        IRepository<Position> positionRepository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository) : IGetEstablishmentOverview
    {
        public async Task<List<EstablishmentRowDto>> GetAsync(Guid? organizationUnitId)
        {
            var scope = await EstablishmentShared.ResolveSubtreeAsync(organizationUnitRepository, organizationUnitId);
            var groups = await EstablishmentShared.QueryAsync(positionRepository, scope);

            // Vacancy aging approximation: a vacant seat's UpdatedAt is its last occupancy change,
            // so (now − UpdatedAt) ≈ how long it has been open. Vacant seats are few — aggregate in memory.
            var vacantQuery = positionRepository.GetAll().Where(p => p.IsVacant);
            if (scope is not null)
                vacantQuery = vacantQuery.Where(p => scope.Contains(p.OrganizationUnitId));
            var vacantSeats = await vacantQuery
                .Select(p => new { p.OrganizationUnitId, p.PositionClassId, p.UpdatedAt, p.CreatedAt })
                .ToListAsync();
            var now = DateTime.UtcNow;
            var agingByGroup = vacantSeats
                .GroupBy(v => (v.OrganizationUnitId, v.PositionClassId))
                .ToDictionary(g => g.Key,
                    // A never-touched seat has been vacant since creation.
                    g => (int)g.Average(v => (now - (v.UpdatedAt ?? v.CreatedAt).ToDateTimeUtc()).TotalDays));

            var unitIds = groups.Select(g => g.OrganizationUnitId).Distinct().ToList();
            var classIds = groups.Select(g => g.PositionClassId).Distinct().ToList();
            var unitNames = await organizationUnitRepository.GetAll()
                .Where(u => unitIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name })
                .ToDictionaryAsync(u => u.Id, u => u.Name);
            var classInfo = await positionClassRepository.GetAll()
                .Where(c => classIds.Contains(c.Id))
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    Grade = c.SalaryScale != null && c.SalaryScale.JobGrade != null ? c.SalaryScale.JobGrade.Name : null,
                    Category = c.JobCategory != null ? c.JobCategory.Name : null
                })
                .ToDictionaryAsync(c => c.Id, c => c);

            return groups
                .Select(g =>
                {
                    var info = classInfo.GetValueOrDefault(g.PositionClassId);
                    return new EstablishmentRowDto
                    {
                        OrganizationUnitId = g.OrganizationUnitId,
                        OrganizationUnitName = unitNames.GetValueOrDefault(g.OrganizationUnitId),
                        PositionClassId = g.PositionClassId,
                        PositionClassTitle = info?.Title,
                        JobGradeName = info?.Grade,
                        JobCategoryName = info?.Category,
                        Authorized = g.Authorized,
                        Filled = g.Filled,
                        Vacant = g.Vacant,
                        AvgVacantDays = agingByGroup.TryGetValue((g.OrganizationUnitId, g.PositionClassId), out var days)
                            ? days : null
                    };
                })
                .OrderBy(r => r.OrganizationUnitName).ThenBy(r => r.PositionClassTitle)
                .ToList();
        }
    }

    // ---- Populate a plan from the live establishment (HC055/HC056) ---------------------

    public class PopulateWorkforcePlan(
        IRepository<WorkforcePlan> repository,
        IRepository<WorkforcePlanLine> lineRepository,
        IRepository<Position> positionRepository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IWorkflowGate workflowGate,
        ILogger<PopulateWorkforcePlan> logger) : IPopulateWorkforcePlan
    {
        public async Task<int> PopulateAsync(Guid planId)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.WorkforcePlan, planId);

            var plan = await repository.GetAll()
                    .Include(p => p.Lines)
                    .FirstOrDefaultAsync(p => p.Id == planId)
                ?? throw new NotFoundException(nameof(WorkforcePlan), planId.ToString());

            var scope = await EstablishmentShared.ResolveSubtreeAsync(organizationUnitRepository, plan.OrganizationUnitId);
            var groups = await EstablishmentShared.QueryAsync(positionRepository, scope);
            if (groups.Count == 0)
                throw new ValidationException("plan", "No positions exist in the plan's scope — create the establishment first.");

            var classIds = groups.Select(g => g.PositionClassId).Distinct().ToList();
            var salaries = await positionClassRepository.GetAll()
                .Where(c => classIds.Contains(c.Id))
                .Select(c => new { c.Id, Monthly = c.SalaryScale != null ? c.SalaryScale.Salary : 0m })
                .ToDictionaryAsync(c => c.Id, c => c.Monthly);

            // Baseline grid: one Permanent line per unit × role in period 1, costed from the pay
            // point; planners then adjust demand/supply/separations per line.
            plan.SetLines(groups.Select(g => new WorkforcePlanLineSpec(
                g.OrganizationUnitId, g.PositionClassId, PlannedEmploymentType.Permanent, 0,
                g.Authorized, g.Filled, g.Vacant,
                0, 0, 0, 0, 0, 0, 0, 0, 0,
                false, null,
                salaries.GetValueOrDefault(g.PositionClassId) * 12, 0, 0,
                null)));
            WorkforcePlanShared.StampLineTenant(plan);
            foreach (var line in plan.Lines)
                await lineRepository.AddAsync(line);
            repository.UpdateAsync(plan);
            await repository.SaveChangesAsync();

            logger.LogInformation("Populated WorkforcePlan {Id} with {Count} establishment lines", planId, plan.Lines.Count);
            return plan.Lines.Count;
        }
    }

    // ---- Suggest separations: retirements within the horizon (HC060) --------------------

    public class SuggestPlanSeparations(
        IRepository<WorkforcePlan> repository,
        IRepository<Employee> employeeRepository,
        IRepository<FiscalYear> fiscalYearRepository) : ISuggestPlanSeparations
    {
        /// <summary>Statutory retirement age — same constant the dashboard retirement forecast uses.</summary>
        private const int RetirementAge = 60;

        public async Task<List<SeparationSuggestionDto>> GetAsync(Guid planId)
        {
            var plan = await repository.GetAll().FirstOrDefaultAsync(p => p.Id == planId)
                ?? throw new NotFoundException(nameof(WorkforcePlan), planId.ToString());

            var fyStart = await fiscalYearRepository.GetAll()
                .Where(f => f.Id == plan.StartFiscalYearId)
                .Select(f => f.StartDate)
                .FirstOrDefaultAsync();

            // Horizon end ≈ start-of-horizon + PeriodCount fiscal years. Retiring within the horizon
            // ⟺ DateOfBirth < (horizonEnd − 60y): the threshold is a constant, so the filter stays
            // SARGABLE (the same pattern as the dashboard's upcoming-retirements query).
            var horizonEnd = fyStart.ToDateTimeUtc().AddYears(plan.PeriodCount);
            var dobThreshold = horizonEnd.AddYears(-RetirementAge);

            var suggestions = await employeeRepository.GetAll()
                .Where(e => !e.IsTerminated
                    && e.EmploymentStatus != EmploymentStatus.Terminated
                    && e.PositionId != null
                    && e.DateOfBirth != null && e.DateOfBirth < dobThreshold)
                .GroupBy(e => new { e.Position!.OrganizationUnitId, e.Position!.PositionClassId })
                .Select(g => new SeparationSuggestionDto
                {
                    OrganizationUnitId = g.Key.OrganizationUnitId,
                    PositionClassId = g.Key.PositionClassId,
                    Retirements = g.Count()
                })
                .ToListAsync();

            return suggestions;
        }
    }

    // ---- Plan summary: budget position + time-phased aggregates (HC069/HC073) ------------

    public class GetWorkforcePlanSummary(IRepository<WorkforcePlan> repository) : IGetWorkforcePlanSummary
    {
        public async Task<WorkforcePlanSummaryDto> GetAsync(Guid planId)
        {
            var plan = await repository.GetAll()
                    .Include(p => p.Lines)
                    .FirstOrDefaultAsync(p => p.Id == planId)
                ?? throw new NotFoundException(nameof(WorkforcePlan), planId.ToString());

            var periods = Enumerable.Range(0, plan.PeriodCount)
                .Select(i =>
                {
                    var lines = plan.Lines.Where(l => l.PeriodIndex == i).ToList();
                    return new WorkforcePlanPeriodSummaryDto
                    {
                        PeriodIndex = i,
                        EndHeadcount = lines.Sum(l => l.EndHeadcount),
                        Demand = lines.Sum(l => l.NewHires + l.Replacements + l.TemporaryStaff),
                        Supply = lines.Sum(l => l.MobilityIn + l.Promotions + l.ActingAssignments),
                        Separations = lines.Sum(l => l.Retirements + l.Resignations + l.ContractExpiries),
                        Cost = lines.Sum(l => l.LineCost)
                    };
                })
                .ToList();

            return new WorkforcePlanSummaryDto
            {
                PlanId = plan.Id,
                TotalBudget = plan.TotalBudget,
                ProjectedCost = plan.ProjectedCost,
                BudgetVariance = plan.TotalBudget - plan.ProjectedCost,
                ExcessBeyondThreshold = plan.ExcessBeyondThreshold(),
                TotalEndHeadcount = plan.Lines.Sum(l => l.EndHeadcount),
                TotalGap = plan.Lines.Sum(l => l.HeadcountGap),
                CriticalRoles = plan.Lines.Count(l => l.IsCriticalRole),
                Periods = periods
            };
        }
    }

    // ---- Scenario comparison (HC068) ------------------------------------------------------

    public class CompareWorkforcePlans(IRepository<WorkforcePlan> repository) : ICompareWorkforcePlans
    {
        public async Task<List<WorkforcePlanComparisonDto>> GetAsync(IReadOnlyList<Guid> planIds)
        {
            if (planIds.Count is < 2 or > 5)
                throw new ValidationException("ids", "Select 2–5 plans to compare.");

            var plans = await repository.GetAll()
                .Include(p => p.Lines)
                .Where(p => planIds.Contains(p.Id))
                .ToListAsync();
            if (plans.Count != planIds.Count)
                throw new NotFoundException(nameof(WorkforcePlan), string.Join(",", planIds.Except(plans.Select(p => p.Id))));

            // Preserve the caller's ordering (baseline first, alternatives after).
            return planIds
                .Select(id => plans.First(p => p.Id == id))
                .Select(p => new WorkforcePlanComparisonDto
                {
                    PlanId = p.Id,
                    Name = p.Name,
                    Scenario = p.Scenario.ToString(),
                    Status = p.Status.ToString(),
                    Version = p.Version,
                    TotalEndHeadcount = p.Lines.Sum(l => l.EndHeadcount),
                    TotalDemand = p.Lines.Sum(l => l.NewHires + l.Replacements + l.TemporaryStaff),
                    TotalSeparations = p.Lines.Sum(l => l.Retirements + l.Resignations + l.ContractExpiries),
                    TotalGap = p.Lines.Sum(l => l.HeadcountGap),
                    CriticalRoles = p.Lines.Count(l => l.IsCriticalRole),
                    ProjectedCost = p.ProjectedCost,
                    TotalBudget = p.TotalBudget,
                    BudgetVariance = p.TotalBudget - p.ProjectedCost
                })
                .ToList();
        }
    }

    // ---- Approved demand feed for recruitment (HC075) --------------------------------------

    public class GetApprovedDemand(
        IRepository<WorkforcePlan> repository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository) : IGetApprovedDemand
    {
        public async Task<List<ApprovedDemandRowDto>> GetAsync()
        {
            // Only APPROVED (not archived/superseded) plans feed recruitment; lines without any
            // hiring demand are noise and are filtered out.
            var rows = await repository.GetAll()
                .Where(p => p.Status == WorkforcePlanStatus.Approved)
                .SelectMany(p => p.Lines, (p, l) => new { p.Id, p.Name, Line = l })
                .Where(x => x.Line.NewHires > 0 || x.Line.Replacements > 0 || x.Line.TemporaryStaff > 0)
                .Select(x => new
                {
                    x.Id, x.Name,
                    x.Line.OrganizationUnitId, x.Line.PositionClassId, x.Line.EmploymentType,
                    x.Line.PeriodIndex, x.Line.NewHires, x.Line.Replacements, x.Line.TemporaryStaff,
                    x.Line.IsCriticalRole, x.Line.RequiredCompetencies
                })
                .ToListAsync();

            var unitIds = rows.Select(r => r.OrganizationUnitId).Distinct().ToList();
            var classIds = rows.Select(r => r.PositionClassId).Distinct().ToList();
            var unitNames = await organizationUnitRepository.GetAll()
                .Where(u => unitIds.Contains(u.Id)).Select(u => new { u.Id, u.Name })
                .ToDictionaryAsync(u => u.Id, u => u.Name);
            var classTitles = await positionClassRepository.GetAll()
                .Where(c => classIds.Contains(c.Id)).Select(c => new { c.Id, c.Title })
                .ToDictionaryAsync(c => c.Id, c => c.Title);

            return rows
                .Select(r => new ApprovedDemandRowDto
                {
                    PlanId = r.Id,
                    PlanName = r.Name,
                    OrganizationUnitId = r.OrganizationUnitId,
                    OrganizationUnitName = unitNames.GetValueOrDefault(r.OrganizationUnitId),
                    PositionClassId = r.PositionClassId,
                    PositionClassTitle = classTitles.GetValueOrDefault(r.PositionClassId),
                    EmploymentType = r.EmploymentType.ToString(),
                    PeriodIndex = r.PeriodIndex,
                    NewHires = r.NewHires,
                    Replacements = r.Replacements,
                    TemporaryStaff = r.TemporaryStaff,
                    IsCriticalRole = r.IsCriticalRole,
                    RequiredCompetencies = r.RequiredCompetencies
                })
                .OrderBy(r => r.OrganizationUnitName).ThenBy(r => r.PositionClassTitle)
                .ToList();
        }
    }
}
