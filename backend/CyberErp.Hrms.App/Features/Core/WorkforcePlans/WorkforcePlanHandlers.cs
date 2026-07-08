using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.WorkforcePlans
{
    // ---- Interfaces -----------------------------------------------------------

    public interface ISaveWorkforcePlan { Task<Guid> SaveAsync(SaveWorkforcePlanDto dto); }
    public interface IGetWorkforcePlanById { Task<WorkforcePlanDto> GetAsync(Guid id); }
    public interface IGetAllWorkforcePlans { Task<PaginatedResponse<WorkforcePlanDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteWorkforcePlan { Task DeleteAsync(Guid id); }
    public interface ISubmitWorkforcePlan { Task SubmitAsync(SubmitWorkforcePlanDto dto); }
    public interface ICreateWorkforcePlanVersion { Task<Guid> CreateAsync(Guid sourcePlanId); }

    // ---- Shared helpers ---------------------------------------------------------

    internal static class WorkforcePlanShared
    {
        /// <summary>The repository stamps only aggregate roots — cascade-inserted lines copy it here.</summary>
        internal static void StampLineTenant(WorkforcePlan plan)
        {
            foreach (var line in plan.Lines)
                if (string.IsNullOrEmpty(line.TenantId))
                    line.TenantId = plan.TenantId;
        }

        /// <summary>
        /// Builds line specs from the write DTO, defaulting a zero salary cost from the role's
        /// salary scale (monthly salary × 12 — the pay point drives cost projections, HC064).
        /// </summary>
        internal static async Task<List<WorkforcePlanLineSpec>> BuildLineSpecsAsync(
            SaveWorkforcePlanDto dto,
            IRepository<PositionClass> positionClasses,
            IRepository<OrganizationUnit> organizationUnits)
        {
            var unitIds = dto.Lines.Select(l => l.OrganizationUnitId).Distinct().ToList();
            var classIds = dto.Lines.Select(l => l.PositionClassId).Distinct().ToList();

            var knownUnits = (await organizationUnits.GetAll()
                .Where(u => unitIds.Contains(u.Id)).Select(u => u.Id).ToListAsync()).ToHashSet();
            var classSalaries = await positionClasses.GetAll()
                .Where(c => classIds.Contains(c.Id))
                .Select(c => new { c.Id, MonthlySalary = c.SalaryScale != null ? c.SalaryScale.Salary : 0m })
                .ToDictionaryAsync(c => c.Id, c => c.MonthlySalary);

            var specs = new List<WorkforcePlanLineSpec>();
            foreach (var l in dto.Lines)
            {
                if (!knownUnits.Contains(l.OrganizationUnitId))
                    throw new NotFoundException(nameof(OrganizationUnit), l.OrganizationUnitId.ToString());
                if (!classSalaries.TryGetValue(l.PositionClassId, out var monthlySalary))
                    throw new NotFoundException(nameof(PositionClass), l.PositionClassId.ToString());
                if (l.PeriodIndex >= dto.PeriodCount)
                    throw new ValidationException("lines",
                        $"A line references period {l.PeriodIndex + 1} but the horizon spans {dto.PeriodCount} period(s).");

                specs.Add(new WorkforcePlanLineSpec(
                    l.OrganizationUnitId,
                    l.PositionClassId,
                    Enum.Parse<PlannedEmploymentType>(l.EmploymentType, true),
                    l.PeriodIndex,
                    l.AuthorizedHeadcount, l.FilledCount, l.VacantCount,
                    l.NewHires, l.Replacements, l.TemporaryStaff,
                    l.MobilityIn, l.Promotions, l.ActingAssignments,
                    l.Retirements, l.Resignations, l.ContractExpiries,
                    l.IsCriticalRole, l.RequiredCompetencies,
                    l.AnnualSalaryCost > 0 ? l.AnnualSalaryCost : monthlySalary * 12,
                    l.AnnualAllowances, l.AnnualBenefits,
                    l.Remark));
            }

            // The unique line key must hold inside the submitted grid too.
            var dupes = specs.GroupBy(s => new { s.OrganizationUnitId, s.PositionClassId, s.EmploymentType, s.PeriodIndex })
                .Where(g => g.Count() > 1).ToList();
            if (dupes.Count > 0)
                throw new ValidationException("lines",
                    "Duplicate plan lines: each unit × role × employment type × period may appear only once.");

            return specs;
        }

        internal static WorkforcePlanDto ToDto(
            WorkforcePlan p,
            IReadOnlyDictionary<Guid, string> unitNames,
            IReadOnlyDictionary<Guid, (string Title, string? Grade, string? Category)> classInfo,
            string? fiscalYearName,
            bool awaitingWorkflow = false)
    {
            return new WorkforcePlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Horizon = p.Horizon.ToString(),
                Scenario = p.Scenario.ToString(),
                Status = p.Status.ToString(),
                OrganizationUnitId = p.OrganizationUnitId,
                OrganizationUnitName = p.OrganizationUnitId.HasValue
                    ? unitNames.GetValueOrDefault(p.OrganizationUnitId.Value)
                    : null,
                StartFiscalYearId = p.StartFiscalYearId,
                StartFiscalYearName = fiscalYearName,
                PeriodCount = p.PeriodCount,
                Version = p.Version,
                RootPlanId = p.RootPlanId,
                TotalBudget = p.TotalBudget,
                BudgetThresholdPercent = p.BudgetThresholdPercent,
                EscalationJustification = p.EscalationJustification,
                ProjectedCost = p.ProjectedCost,
                BudgetVariance = p.TotalBudget - p.ProjectedCost,
                ExcessBeyondThreshold = p.ExcessBeyondThreshold(),
                SubmittedAt = p.SubmittedAt,
                ApprovedAt = p.ApprovedAt,
                AwaitingWorkflow = awaitingWorkflow,
                Lines = p.Lines
                    .OrderBy(l => l.PeriodIndex).ThenBy(l => unitNames.GetValueOrDefault(l.OrganizationUnitId))
                    .Select(l =>
                    {
                        var info = classInfo.GetValueOrDefault(l.PositionClassId);
                        return new WorkforcePlanLineDto
                        {
                            Id = l.Id,
                            OrganizationUnitId = l.OrganizationUnitId,
                            OrganizationUnitName = unitNames.GetValueOrDefault(l.OrganizationUnitId),
                            PositionClassId = l.PositionClassId,
                            PositionClassTitle = info.Title,
                            JobGradeName = info.Grade,
                            JobCategoryName = info.Category,
                            EmploymentType = l.EmploymentType.ToString(),
                            PeriodIndex = l.PeriodIndex,
                            AuthorizedHeadcount = l.AuthorizedHeadcount,
                            FilledCount = l.FilledCount,
                            VacantCount = l.VacantCount,
                            NewHires = l.NewHires,
                            Replacements = l.Replacements,
                            TemporaryStaff = l.TemporaryStaff,
                            MobilityIn = l.MobilityIn,
                            Promotions = l.Promotions,
                            ActingAssignments = l.ActingAssignments,
                            Retirements = l.Retirements,
                            Resignations = l.Resignations,
                            ContractExpiries = l.ContractExpiries,
                            IsCriticalRole = l.IsCriticalRole,
                            RequiredCompetencies = l.RequiredCompetencies,
                            AnnualSalaryCost = l.AnnualSalaryCost,
                            AnnualAllowances = l.AnnualAllowances,
                            AnnualBenefits = l.AnnualBenefits,
                            EndHeadcount = l.EndHeadcount,
                            HeadcountGap = l.HeadcountGap,
                            LineCost = l.LineCost,
                            Remark = l.Remark
                        };
                    })
                    .ToList()
            };
        }

        /// <summary>Batch name lookups for one plan's DTO projection.</summary>
        internal static async Task<(Dictionary<Guid, string> Units,
            Dictionary<Guid, (string, string?, string?)> Classes,
            string? FiscalYearName)> ResolveNamesAsync(
            WorkforcePlan plan,
            IRepository<OrganizationUnit> organizationUnits,
            IRepository<PositionClass> positionClasses,
            IRepository<FiscalYear> fiscalYears)
        {
            var unitIds = plan.Lines.Select(l => l.OrganizationUnitId).Distinct().ToList();
            if (plan.OrganizationUnitId.HasValue) unitIds.Add(plan.OrganizationUnitId.Value);
            var classIds = plan.Lines.Select(l => l.PositionClassId).Distinct().ToList();

            var units = await organizationUnits.GetAll()
                .Where(u => unitIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name })
                .ToDictionaryAsync(u => u.Id, u => u.Name);
            var classes = await positionClasses.GetAll()
                .Where(c => classIds.Contains(c.Id))
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    Grade = c.SalaryScale != null && c.SalaryScale.JobGrade != null ? c.SalaryScale.JobGrade.Name : null,
                    Category = c.JobCategory != null ? c.JobCategory.Name : null
                })
                .ToDictionaryAsync(c => c.Id, c => (c.Title, c.Grade, c.Category));
            var fyName = await fiscalYears.GetAll()
                .Where(f => f.Id == plan.StartFiscalYearId)
                .Select(f => f.Name)
                .FirstOrDefaultAsync();

            return (units, classes, fyName);
        }
    }

    // ---- Save (create / update while editable) ------------------------------------

    public class SaveWorkforcePlan(
        IRepository<WorkforcePlan> repository,
        IRepository<WorkforcePlanLine> lineRepository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<FiscalYear> fiscalYearRepository,
        IWorkflowGate workflowGate,
        IValidator<SaveWorkforcePlanDto> validator,
        ILogger<SaveWorkforcePlan> logger) : ISaveWorkforcePlan
    {
        public async Task<Guid> SaveAsync(SaveWorkforcePlanDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await fiscalYearRepository.GetAll().AnyAsync(f => f.Id == dto.StartFiscalYearId))
                throw new NotFoundException(nameof(FiscalYear), dto.StartFiscalYearId.ToString());
            if (dto.OrganizationUnitId.HasValue &&
                !await organizationUnitRepository.GetAll().AnyAsync(u => u.Id == dto.OrganizationUnitId.Value))
                throw new NotFoundException(nameof(OrganizationUnit), dto.OrganizationUnitId.Value.ToString());

            var horizon = Enum.Parse<PlanningHorizon>(dto.Horizon, true);
            var scenario = Enum.Parse<WorkforcePlanScenario>(dto.Scenario, true);
            var specs = await WorkforcePlanShared.BuildLineSpecsAsync(dto, positionClassRepository, organizationUnitRepository);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.WorkforcePlan, dto.Id.Value);

                var entity = await repository.GetAll()
                        .Include(p => p.Lines)
                        .FirstOrDefaultAsync(p => p.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(WorkforcePlan), dto.Id.Value.ToString());

                // Pre-check as a 400 (the domain re-enforces this as an invariant).
                if (entity.Status is not (WorkforcePlanStatus.Draft or WorkforcePlanStatus.Rejected))
                    throw new ValidationException("status",
                        $"A {entity.Status} workforce plan can no longer be edited — create a new version instead.");

                entity.Update(dto.Name, horizon, scenario, dto.StartFiscalYearId, dto.PeriodCount,
                    dto.OrganizationUnitId, dto.TotalBudget, dto.BudgetThresholdPercent, dto.Description);
                entity.SetLines(specs);
                WorkforcePlanShared.StampLineTenant(entity);
                // Replacement lines are new rows: mark them Added explicitly, otherwise
                // context.Update(root) treats the app-generated keys as existing (Modified).
                foreach (var line in entity.Lines)
                    await lineRepository.AddAsync(line);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated WorkforcePlan {Id} ({Lines} lines)", entity.Id, entity.Lines.Count);
                return entity.Id;
            }

            var created = WorkforcePlan.Create(dto.Name, horizon, scenario, dto.StartFiscalYearId,
                dto.PeriodCount, dto.OrganizationUnitId, dto.TotalBudget, dto.BudgetThresholdPercent, dto.Description);
            created.SetLines(specs);
            await repository.AddAsync(created);   // stamps the root's TenantId
            WorkforcePlanShared.StampLineTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created WorkforcePlan {Id} ({Scenario}, {Lines} lines)", created.Id, scenario, created.Lines.Count);
            return created.Id;
        }
    }

    // ---- Get by id (full, with lines + resolved names) -----------------------------

    public class GetWorkforcePlanById(
        IRepository<WorkforcePlan> repository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<PositionClass> positionClassRepository,
        IRepository<FiscalYear> fiscalYearRepository,
        IWorkflowGate workflowGate) : IGetWorkforcePlanById
    {
        public async Task<WorkforcePlanDto> GetAsync(Guid id)
        {
            var plan = await repository.GetAll()
                    .Include(p => p.Lines)
                    .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException(nameof(WorkforcePlan), id.ToString());

            var (units, classes, fyName) = await WorkforcePlanShared.ResolveNamesAsync(
                plan, organizationUnitRepository, positionClassRepository, fiscalYearRepository);
            var awaiting = plan.Status == WorkforcePlanStatus.Submitted
                && await workflowGate.HasRunningAsync(WorkflowEntityTypes.WorkforcePlan, plan.Id);

            return WorkforcePlanShared.ToDto(plan, units, classes, fyName, awaiting);
        }
    }

    // ---- Get all (paged headers) ---------------------------------------------------

    public class GetAllWorkforcePlans(
        IRepository<WorkforcePlan> repository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<FiscalYear> fiscalYearRepository) : IGetAllWorkforcePlans
    {
        public async Task<PaginatedResponse<WorkforcePlanDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<WorkforcePlanStatus>(request.Status, true, out var status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(p => p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip).Take(take)
                .Select(p => new
                {
                    Plan = p,
                    OrganizationUnitName = organizationUnitRepository.GetAll()
                        .Where(u => u.Id == p.OrganizationUnitId).Select(u => u.Name).FirstOrDefault(),
                    FiscalYearName = fiscalYearRepository.GetAll()
                        .Where(f => f.Id == p.StartFiscalYearId).Select(f => f.Name).FirstOrDefault()
                })
                .ToListAsync();

            // Header projection only — lines load on GetById (plan grids can be large).
            var data = rows.Select(r => new WorkforcePlanDto
            {
                Id = r.Plan.Id,
                Name = r.Plan.Name,
                Description = r.Plan.Description,
                Horizon = r.Plan.Horizon.ToString(),
                Scenario = r.Plan.Scenario.ToString(),
                Status = r.Plan.Status.ToString(),
                OrganizationUnitId = r.Plan.OrganizationUnitId,
                OrganizationUnitName = r.OrganizationUnitName,
                StartFiscalYearId = r.Plan.StartFiscalYearId,
                StartFiscalYearName = r.FiscalYearName,
                PeriodCount = r.Plan.PeriodCount,
                Version = r.Plan.Version,
                RootPlanId = r.Plan.RootPlanId,
                TotalBudget = r.Plan.TotalBudget,
                BudgetThresholdPercent = r.Plan.BudgetThresholdPercent,
                ProjectedCost = r.Plan.ProjectedCost,
                BudgetVariance = r.Plan.TotalBudget - r.Plan.ProjectedCost,
                ExcessBeyondThreshold = r.Plan.ExcessBeyondThreshold(),
                SubmittedAt = r.Plan.SubmittedAt,
                ApprovedAt = r.Plan.ApprovedAt
            }).ToList();

            return new PaginatedResponse<WorkforcePlanDto> { Total = total, Data = data };
        }
    }

    // ---- Delete (drafts / rejected only) ---------------------------------------------

    public class DeleteWorkforcePlan(
        IRepository<WorkforcePlan> repository,
        IWorkflowGate workflowGate,
        ILogger<DeleteWorkforcePlan> logger) : IDeleteWorkforcePlan
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.WorkforcePlan, id);

            var plan = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(WorkforcePlan), id.ToString());
            if (plan.Status is not (WorkforcePlanStatus.Draft or WorkforcePlanStatus.Rejected))
                throw new ValidationException("status",
                    $"A {plan.Status} plan is part of the planning record and cannot be deleted — archive it via a new version instead.");

            repository.Delete(plan);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted WorkforcePlan {Id}", id);
        }
    }

    // ---- Submit (budget gate + workflow, HC066/HC070) ----------------------------------

    public class SubmitWorkforcePlan(
        IRepository<WorkforcePlan> repository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        ILogger<SubmitWorkforcePlan> logger) : ISubmitWorkforcePlan
    {
        public async Task SubmitAsync(SubmitWorkforcePlanDto dto)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.WorkforcePlan, dto.Id);

            var plan = await repository.GetAll()
                    .Include(p => p.Lines)
                    .FirstOrDefaultAsync(p => p.Id == dto.Id)
                ?? throw new NotFoundException(nameof(WorkforcePlan), dto.Id.ToString());

            // Pre-checks as 400s (the domain re-enforces them as invariants).
            if (plan.Status is not (WorkforcePlanStatus.Draft or WorkforcePlanStatus.Rejected))
                throw new ValidationException("status", $"A {plan.Status} plan cannot be submitted.");
            if (plan.Lines.Count == 0)
                throw new ValidationException("lines", "A workforce plan needs at least one line before submission.");
            if (plan.ExcessBeyondThreshold() > 0 && string.IsNullOrWhiteSpace(dto.EscalationJustification))
                throw new ValidationException("escalationJustification",
                    "The projected cost exceeds the budget threshold — an escalation justification is required (HC066).");

            plan.Submit(dto.EscalationJustification);
            repository.UpdateAsync(plan);
            await repository.SaveChangesAsync();

            // Route through the approval chain when configured (HC070); without a definition the
            // plan is approved directly (consistent with the other workflow-backed modules).
            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.WorkforcePlan, plan.Id, null,
                $"Workforce Plan — {plan.Name} (v{plan.Version}, {plan.Scenario}, cost {plan.ProjectedCost:N0})");

            if (!await workflowGate.HasRunningAsync(WorkflowEntityTypes.WorkforcePlan, plan.Id))
            {
                plan.Approve();
                repository.UpdateAsync(plan);
                await repository.SaveChangesAsync();
            }

            logger.LogInformation("Submitted WorkforcePlan {Id} (excess {Excess})", plan.Id, plan.ExcessBeyondThreshold());
        }
    }

    // ---- New version (HC071) ------------------------------------------------------------

    public class CreateWorkforcePlanVersion(
        IRepository<WorkforcePlan> repository,
        IRepository<WorkforcePlanLine> lineRepository,
        ILogger<CreateWorkforcePlanVersion> logger) : ICreateWorkforcePlanVersion
    {
        public async Task<Guid> CreateAsync(Guid sourcePlanId)
        {
            var source = await repository.GetAll()
                    .Include(p => p.Lines)
                    .FirstOrDefaultAsync(p => p.Id == sourcePlanId)
                ?? throw new NotFoundException(nameof(WorkforcePlan), sourcePlanId.ToString());

            if (source.Status is WorkforcePlanStatus.Draft)
                throw new ValidationException("status", "The plan is still a draft — edit it directly instead of creating a version.");

            var rootId = source.RootPlanId ?? source.Id;
            if (await repository.GetAll().AnyAsync(p =>
                    (p.RootPlanId == rootId || p.Id == rootId) &&
                    (p.Status == WorkforcePlanStatus.Draft || p.Status == WorkforcePlanStatus.Submitted) &&
                    p.Id != source.Id))
                throw new ValidationException("version", "An open draft or submitted version of this plan already exists.");

            var maxVersion = await repository.GetAll()
                .Where(p => p.RootPlanId == rootId || p.Id == rootId)
                .MaxAsync(p => p.Version);

            var clone = WorkforcePlan.Create(source.Name, source.Horizon, source.Scenario,
                source.StartFiscalYearId, source.PeriodCount, source.OrganizationUnitId,
                source.TotalBudget, source.BudgetThresholdPercent, source.Description);
            clone.SetVersion(maxVersion + 1, rootId);
            clone.SetLines(source.Lines.Select(l => new WorkforcePlanLineSpec(
                l.OrganizationUnitId, l.PositionClassId, l.EmploymentType, l.PeriodIndex,
                l.AuthorizedHeadcount, l.FilledCount, l.VacantCount,
                l.NewHires, l.Replacements, l.TemporaryStaff,
                l.MobilityIn, l.Promotions, l.ActingAssignments,
                l.Retirements, l.Resignations, l.ContractExpiries,
                l.IsCriticalRole, l.RequiredCompetencies,
                l.AnnualSalaryCost, l.AnnualAllowances, l.AnnualBenefits,
                l.Remark)));

            await repository.AddAsync(clone);
            WorkforcePlanShared.StampLineTenant(clone);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created version {Version} of WorkforcePlan chain {Root} (new id {Id})",
                clone.Version, rootId, clone.Id);
            return clone.Id;
        }
    }
}
