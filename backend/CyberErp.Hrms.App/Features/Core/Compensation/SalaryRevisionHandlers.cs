using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Compensation
{
    // ---- DTOs ---------------------------------------------------------------
    public class SalaryRevisionLineDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public decimal CurrentSalary { get; set; }
        public decimal ProposedSalary { get; set; }
        public decimal Increase => ProposedSalary - CurrentSalary;
        public decimal IncreasePercent => CurrentSalary > 0 ? Math.Round((ProposedSalary - CurrentSalary) / CurrentSalary * 100m, 2) : 0m;
        // ---- Step basis only (null for Percentage/FixedAmount) ----
        /// <summary>Rung the employee sits on today.</summary>
        public int? CurrentStep { get; set; }
        /// <summary>Rung they land on, e.g. 5.5 — may be fractional.</summary>
        public decimal? ProposedStep { get; set; }
        /// <summary>True when the salary was interpolated between two rungs rather than read directly.</summary>
        public bool Interpolated { get; set; }
        /// <summary>Why the salary did not move (off-scale, no scale rows, ceiling); null when it did.</summary>
        public string? Note { get; set; }
    }

    public class SalaryRevisionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RevisionType { get; set; } = string.Empty;
        public string Basis { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Guid? TargetJobGradeId { get; set; }
        public Guid? TargetOrganizationUnitId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? AppliedOn { get; set; }
        public string? Notes { get; set; }
        // Aggregate (the scenario summary)
        public int EmployeeCount { get; set; }
        public decimal TotalCurrent { get; set; }
        public decimal TotalProposed { get; set; }
        public decimal TotalIncrease { get; set; }
        public decimal AveragePercent { get; set; }
        public List<SalaryRevisionLineDto> Lines { get; set; } = [];
    }

    public class SaveSalaryRevisionDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RevisionType { get; set; } = nameof(SalaryRevisionType.Merit);
        public string Basis { get; set; } = nameof(SalaryAdjustmentBasis.Percentage);
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Guid? TargetJobGradeId { get; set; }
        public Guid? TargetOrganizationUnitId { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveSalaryRevisionDtoValidator : AbstractValidator<SaveSalaryRevisionDto>
    {
        public SaveSalaryRevisionDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Rate).GreaterThanOrEqualTo(0);
            RuleFor(x => x.EffectiveDate).NotEmpty();
            RuleFor(x => x.RevisionType).Must(v => Enum.TryParse<SalaryRevisionType>(v, true, out _))
                .WithMessage("Revision type must be Merit, Market or CostOfLiving.");
            RuleFor(x => x.Basis).Must(v => Enum.TryParse<SalaryAdjustmentBasis>(v, true, out _))
                .WithMessage("Basis must be Percentage, FixedAmount or Step.");
            // A step increment is a ladder distance, so fractions are expected (1.5, 2.5) but it must
            // still be a real move on a real ladder.
            RuleFor(x => x.Rate)
                .GreaterThan(0)
                .When(x => string.Equals(x.Basis, nameof(SalaryAdjustmentBasis.Step), StringComparison.OrdinalIgnoreCase))
                .WithMessage("A step revision needs a step increment greater than zero.");
        }
    }

    /// <summary>Stateless scenario simulation input (HC228) — try a rate without persisting a plan.</summary>
    public class SimulateSalaryRevisionDto
    {
        public string Basis { get; set; } = nameof(SalaryAdjustmentBasis.Percentage);
        public decimal Rate { get; set; }
        public Guid? TargetJobGradeId { get; set; }
        public Guid? TargetOrganizationUnitId { get; set; }
    }

    public class SalarySimulationDto
    {
        public int EmployeeCount { get; set; }
        public decimal TotalCurrent { get; set; }
        public decimal TotalProposed { get; set; }
        public decimal TotalIncrease { get; set; }
        public decimal AveragePercent { get; set; }
        /// <summary>Per-employee preview, capped for the response; the aggregate covers everyone.</summary>
        public List<SalaryRevisionLineDto> Lines { get; set; } = [];
        public bool LinesTruncated { get; set; }
        /// <summary>Employees the scale could not move (off-scale, no grade rows, already at ceiling).</summary>
        public int UnresolvedCount { get; set; }
        /// <summary>Employees whose new salary was interpolated between two rungs.</summary>
        public int InterpolatedCount { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISimulateSalaryRevision { Task<SalarySimulationDto> SimulateAsync(SimulateSalaryRevisionDto dto); }
    public interface ISaveSalaryRevision { Task<Guid> SaveAsync(SaveSalaryRevisionDto dto); }
    public interface IGetSalaryRevisionById { Task<SalaryRevisionDto> GetAsync(Guid id); }
    public interface IGetAllSalaryRevisions { Task<PaginatedResponse<SalaryRevisionDto>> GetAsync(GetAllRequest request); }
    public interface ISetSalaryRevisionLine { Task SetAsync(Guid lineId, decimal proposedSalary); }
    public interface ISubmitSalaryRevision { Task SubmitAsync(Guid id); }
    public interface IApproveSalaryRevision { Task ApproveAsync(Guid id); }
    public interface IApplySalaryRevision { Task ApplyAsync(Guid id); }
    public interface IDeleteSalaryRevision { Task DeleteAsync(Guid id); }

    // ---- Shared -------------------------------------------------------------
    internal class EmployeeCompRow
    {
        public Guid EmployeeId { get; set; }
        public string? Name { get; set; }
        public string? Number { get; set; }
        public decimal? Salary { get; set; }
        public decimal? ScaleSalary { get; set; }
        public decimal Base => Salary ?? ScaleSalary ?? 0m;
        /// <summary>Grade + rung the employee currently sits on — the origin for a Step revision.</summary>
        public Guid? JobGradeId { get; set; }
        public int? StepOrdinal { get; set; }
    }

    internal static class SalaryRevisionShared
    {
        /// <summary>
        /// Resolves the targeted employees (with a positive base salary), filtered by grade/unit.
        /// PERF: one projected query over the filtered set; bounded by the target filter (a whole-org
        /// revision loads a small projection per employee — not the full entities, and only once).
        /// </summary>
        internal static async Task<List<EmployeeCompRow>> TargetsAsync(
            IRepository<Employee> employees, Guid? gradeId, Guid? unitId)
        {
            var q = employees.GetAll().AsNoTracking();
            if (gradeId.HasValue)
                q = q.Where(e => e.SalaryScale != null && e.SalaryScale.JobGradeId == gradeId.Value);
            if (unitId.HasValue)
                q = q.Where(e => e.Position != null && e.Position.OrganizationUnitId == unitId.Value);

            var rows = await q.Select(e => new EmployeeCompRow
            {
                EmployeeId = e.Id,
                Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : e.EmployeeNumber,
                Number = e.EmployeeNumber,
                Salary = e.Salary,
                ScaleSalary = e.SalaryScale != null ? (decimal?)e.SalaryScale.Salary : null,
                JobGradeId = e.SalaryScale != null ? (Guid?)e.SalaryScale.JobGradeId : null,
                StepOrdinal = e.SalaryScale != null ? (int?)e.SalaryScale.Step.Ordinal : null
            }).ToListAsync();

            return rows.Where(r => r.Base > 0).ToList();
        }

        /// <summary>
        /// Percentage/FixedAmount are pure functions of the current salary. Step is not — it has to
        /// consult the employee's own grade ladder, so it takes the pre-built <paramref name="ladder"/>
        /// (see <see cref="SalaryScaleLadderFactory"/> for why that is loaded once, not per employee).
        /// </summary>
        internal static StepResolution Propose(
            SalaryAdjustmentBasis basis, decimal rate, EmployeeCompRow row, ISalaryScaleLadder? ladder)
        {
            var current = row.Base;
            return basis switch
            {
                SalaryAdjustmentBasis.Percentage =>
                    new StepResolution(Math.Round(current * (1 + rate / 100m), 2), 0m, false, false, null),
                SalaryAdjustmentBasis.FixedAmount =>
                    new StepResolution(current + rate, 0m, false, false, null),
                SalaryAdjustmentBasis.Step =>
                    ladder is null
                        ? StepResolution.Unchanged(current, "No salary scale loaded.")
                        : HoldPay(ladder.Resolve(row.JobGradeId, row.StepOrdinal, rate, current), current),
                _ => StepResolution.Unchanged(current, "Unsupported basis.")
            };
        }

        /// <summary>
        /// A step revision must never CUT pay. Employees paid above their rung (red-circled, off-scale,
        /// or promoted ahead of a scale refresh) are common, and for them the scale value can sit below
        /// what they already earn — an unguarded "advance 1 step" would hand thousands of people a pay
        /// cut in one click. Hold the current salary instead and say why, so it shows up in the
        /// simulation's "not moved" count rather than in next month's payroll.
        /// </summary>
        private static StepResolution HoldPay(StepResolution resolved, decimal current) =>
            resolved.Salary >= current
                ? resolved
                : resolved with
                {
                    Salary = current,
                    Reason = "Scale value is below current pay; salary held (paid above their step)."
                };

        /// <summary>Step revisions read the ladder; the other bases never touch the scale table.</summary>
        internal static async Task<ISalaryScaleLadder?> LadderIfNeededAsync(
            SalaryAdjustmentBasis basis, ISalaryScaleLadderFactory factory, Guid? targetGradeId) =>
            basis == SalaryAdjustmentBasis.Step ? await factory.BuildAsync(targetGradeId) : null;

        /// <summary>
        /// Basis-specific bounds. The percentage ceiling must NOT be applied to a step increment —
        /// "2.5" is a perfectly ordinary number of steps but would look like a 2.5% rise.
        /// </summary>
        internal static void GuardRate(SalaryAdjustmentBasis basis, decimal rate)
        {
            if (basis == SalaryAdjustmentBasis.Percentage && rate > 100)
                throw new ValidationException("Rate", "A percentage revision cannot exceed 100.");
            if (basis == SalaryAdjustmentBasis.Step && rate > MaxStepIncrement)
                throw new ValidationException("Rate", $"A step revision cannot advance more than {MaxStepIncrement} steps.");
        }

        /// <summary>Sanity bound — real ladders are ~10 rungs, so this only catches fat-finger input.</summary>
        internal const decimal MaxStepIncrement = 50m;

        internal static void FillAggregate(SalaryRevisionDto dto)
        {
            dto.EmployeeCount = dto.Lines.Count;
            dto.TotalCurrent = dto.Lines.Sum(l => l.CurrentSalary);
            dto.TotalProposed = dto.Lines.Sum(l => l.ProposedSalary);
            dto.TotalIncrease = dto.TotalProposed - dto.TotalCurrent;
            dto.AveragePercent = dto.TotalCurrent > 0 ? Math.Round(dto.TotalIncrease / dto.TotalCurrent * 100m, 2) : 0m;
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SimulateSalaryRevision(
        IRepository<Employee> employeeRepository,
        ISalaryScaleLadderFactory ladderFactory,
        IPerformanceVisibilityService visibility) : ISimulateSalaryRevision
    {
        private const int PreviewCap = 200;

        public async Task<SalarySimulationDto> SimulateAsync(SimulateSalaryRevisionDto dto)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can run salary simulations.");

            var basis = Enum.Parse<SalaryAdjustmentBasis>(dto.Basis, true);
            SalaryRevisionShared.GuardRate(basis, dto.Rate);

            var rows = await SalaryRevisionShared.TargetsAsync(employeeRepository, dto.TargetJobGradeId, dto.TargetOrganizationUnitId);
            var ladder = await SalaryRevisionShared.LadderIfNeededAsync(basis, ladderFactory, dto.TargetJobGradeId);

            var lines = rows.Select(r =>
            {
                var res = SalaryRevisionShared.Propose(basis, dto.Rate, r, ladder);
                return new SalaryRevisionLineDto
                {
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Name,
                    EmployeeNumber = r.Number,
                    CurrentSalary = r.Base,
                    ProposedSalary = res.Salary,
                    CurrentStep = r.StepOrdinal,
                    ProposedStep = basis == SalaryAdjustmentBasis.Step ? res.ResolvedStep : null,
                    Interpolated = res.Interpolated,
                    Note = res.Reason
                };
            }).ToList();

            var totalCurrent = lines.Sum(l => l.CurrentSalary);
            var totalProposed = lines.Sum(l => l.ProposedSalary);
            return new SalarySimulationDto
            {
                EmployeeCount = lines.Count,
                TotalCurrent = totalCurrent,
                TotalProposed = totalProposed,
                TotalIncrease = totalProposed - totalCurrent,
                AveragePercent = totalCurrent > 0 ? Math.Round((totalProposed - totalCurrent) / totalCurrent * 100m, 2) : 0m,
                Lines = lines.OrderByDescending(l => l.Increase).Take(PreviewCap).ToList(),
                LinesTruncated = lines.Count > PreviewCap,
                // Surfaced so HR can see, before committing, how many people the scale could not
                // move (off-scale employees, grades with no rows, ceiling hits) rather than
                // discovering a silent no-op after applying.
                UnresolvedCount = lines.Count(l => l.Note != null),
                InterpolatedCount = lines.Count(l => l.Interpolated)
            };
        }
    }

    public class SaveSalaryRevision(
        IRepository<SalaryRevision> repository,
        IRepository<SalaryRevisionLine> lineRepository,
        IRepository<Employee> employeeRepository,
        ISalaryScaleLadderFactory ladderFactory,
        IPerformanceVisibilityService visibility,
        IValidator<SaveSalaryRevisionDto> validator,
        ILogger<SaveSalaryRevision> logger) : ISaveSalaryRevision
    {
        public async Task<Guid> SaveAsync(SaveSalaryRevisionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can plan salary revisions.");

            var type = Enum.Parse<SalaryRevisionType>(dto.RevisionType, true);
            var basis = Enum.Parse<SalaryAdjustmentBasis>(dto.Basis, true);
            SalaryRevisionShared.GuardRate(basis, dto.Rate);

            var rows = await SalaryRevisionShared.TargetsAsync(employeeRepository, dto.TargetJobGradeId, dto.TargetOrganizationUnitId);
            var ladder = await SalaryRevisionShared.LadderIfNeededAsync(basis, ladderFactory, dto.TargetJobGradeId);

            Guid planId;
            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(SalaryRevision), dto.Id.Value.ToString());
                if (entity.Status != SalaryRevisionStatus.Draft)
                    throw new ValidationException(nameof(dto.Id), "Only a draft revision can be edited.");
                entity.UpdateDraft(dto.Name, type, basis, dto.Rate, dto.EffectiveDate, dto.TargetJobGradeId, dto.TargetOrganizationUnitId, dto.Notes);
                repository.UpdateAsync(entity);
                // Regenerate the scenario lines against the new parameters.
                await lineRepository.Delete(l => l.SalaryRevisionId == entity.Id);
                planId = entity.Id;
            }
            else
            {
                var created = SalaryRevision.Create(dto.Name, type, basis, dto.Rate, dto.EffectiveDate,
                    dto.TargetJobGradeId, dto.TargetOrganizationUnitId, dto.Notes);
                await repository.AddAsync(created);
                planId = created.Id;
            }

            foreach (var r in rows)
                await lineRepository.AddAsync(SalaryRevisionLine.Create(planId, r.EmployeeId, r.Base,
                    SalaryRevisionShared.Propose(basis, dto.Rate, r, ladder).Salary));

            await repository.SaveChangesAsync();
            logger.LogInformation("Saved SalaryRevision {Id} with {Count} lines", planId, rows.Count);
            return planId;
        }
    }

    public class GetSalaryRevisionById(
        IRepository<SalaryRevision> repository,
        IRepository<SalaryRevisionLine> lineRepository,
        IRepository<Employee> employeeRepository) : IGetSalaryRevisionById
    {
        public async Task<SalaryRevisionDto> GetAsync(Guid id)
        {
            var dto = await repository.GetAll().AsNoTracking().Where(x => x.Id == id)
                .Select(x => new SalaryRevisionDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    RevisionType = x.RevisionType.ToString(),
                    Basis = x.Basis.ToString(),
                    Rate = x.Rate,
                    EffectiveDate = x.EffectiveDate,
                    TargetJobGradeId = x.TargetJobGradeId,
                    TargetOrganizationUnitId = x.TargetOrganizationUnitId,
                    Status = x.Status.ToString(),
                    AppliedOn = x.AppliedOn,
                    Notes = x.Notes
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(SalaryRevision), id.ToString());

            var employees = employeeRepository.GetAll();
            dto.Lines = await lineRepository.GetAll().AsNoTracking()
                .Where(l => l.SalaryRevisionId == id)
                .OrderByDescending(l => l.ProposedSalary - l.CurrentSalary)
                .Select(l => new SalaryRevisionLineDto
                {
                    Id = l.Id,
                    EmployeeId = l.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == l.EmployeeId && e.Person != null)
                        .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    EmployeeNumber = employees.Where(e => e.Id == l.EmployeeId).Select(e => e.EmployeeNumber).FirstOrDefault(),
                    CurrentSalary = l.CurrentSalary,
                    ProposedSalary = l.ProposedSalary
                }).ToListAsync();

            SalaryRevisionShared.FillAggregate(dto);
            return dto;
        }
    }

    public class GetAllSalaryRevisions(IRepository<SalaryRevision> repository) : IGetAllSalaryRevisions
    {
        public async Task<PaginatedResponse<SalaryRevisionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<SalaryRevisionStatus>(request.Status, true, out var st))
                query = query.Where(x => x.Status == st);

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take)
                .Select(x => new SalaryRevisionDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    RevisionType = x.RevisionType.ToString(),
                    Basis = x.Basis.ToString(),
                    Rate = x.Rate,
                    EffectiveDate = x.EffectiveDate,
                    Status = x.Status.ToString(),
                    AppliedOn = x.AppliedOn,
                    EmployeeCount = x.Lines.Count,
                    TotalCurrent = x.Lines.Sum(l => l.CurrentSalary),
                    TotalProposed = x.Lines.Sum(l => l.ProposedSalary)
                }).ToListAsync();
            foreach (var d in data)
            {
                d.TotalIncrease = d.TotalProposed - d.TotalCurrent;
                d.AveragePercent = d.TotalCurrent > 0 ? Math.Round(d.TotalIncrease / d.TotalCurrent * 100m, 2) : 0m;
            }

            return new PaginatedResponse<SalaryRevisionDto> { Total = total, Data = data };
        }
    }

    public class SetSalaryRevisionLine(
        IRepository<SalaryRevisionLine> lineRepository,
        IRepository<SalaryRevision> repository,
        IPerformanceVisibilityService visibility) : ISetSalaryRevisionLine
    {
        public async Task SetAsync(Guid lineId, decimal proposedSalary)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can adjust revision lines.");

            var line = await lineRepository.GetAll().FirstOrDefaultAsync(l => l.Id == lineId)
                ?? throw new NotFoundException(nameof(SalaryRevisionLine), lineId.ToString());
            var status = await repository.GetAll().Where(r => r.Id == line.SalaryRevisionId).Select(r => r.Status).FirstOrDefaultAsync();
            if (status != SalaryRevisionStatus.Draft)
                throw new ValidationException(nameof(lineId), "Lines can only be adjusted while the revision is a draft.");

            line.SetProposed(proposedSalary);
            lineRepository.UpdateAsync(line);
            await lineRepository.SaveChangesAsync();
        }
    }

    public class SubmitSalaryRevision(
        IRepository<SalaryRevision> repository,
        IRepository<SalaryRevisionLine> lineRepository,
        IPerformanceVisibilityService visibility,
        IWorkflowService workflowService) : ISubmitSalaryRevision
    {
        public async Task SubmitAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can submit salary revisions.");

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(SalaryRevision), id.ToString());
            if (!await lineRepository.GetAll().AnyAsync(l => l.SalaryRevisionId == id))
                throw new ValidationException(nameof(id), "The revision has no employees to revise.");

            entity.Submit();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();

            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.SalaryRevision, entity.Id, null,
                $"Salary revision — {entity.Name}");
        }
    }

    public class ApproveSalaryRevision(
        IRepository<SalaryRevision> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IApproveSalaryRevision
    {
        public async Task ApproveAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can approve salary revisions.");

            // Direct approval only when no workflow governs it (otherwise approve via the workflow).
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.SalaryRevision, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(SalaryRevision), id.ToString());
            entity.Approve();
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class ApplySalaryRevision(
        IRepository<SalaryRevision> repository,
        IRepository<SalaryRevisionLine> lineRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        ILogger<ApplySalaryRevision> logger) : IApplySalaryRevision
    {
        public async Task ApplyAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can apply salary revisions.");

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(SalaryRevision), id.ToString());
            if (entity.Status != SalaryRevisionStatus.Approved)
                throw new ValidationException(nameof(id), "Only an approved revision can be applied.");

            var lines = await lineRepository.GetAll().AsNoTracking()
                .Where(l => l.SalaryRevisionId == id)
                .Select(l => new { l.EmployeeId, l.ProposedSalary }).ToListAsync();

            var empIds = lines.Select(l => l.EmployeeId).ToList();
            var employees = await employeeRepository.GetAll().Where(e => empIds.Contains(e.Id)).ToListAsync();
            var byId = employees.ToDictionary(e => e.Id);

            var applied = 0;
            foreach (var l in lines)
            {
                if (!byId.TryGetValue(l.EmployeeId, out var emp)) continue;   // employee gone since planning
                emp.ApplyMovement(false, null, null, l.ProposedSalary);
                employeeRepository.UpdateAsync(emp);
                applied++;
            }

            entity.MarkApplied(DateTime.UtcNow.Date);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Applied SalaryRevision {Id} to {Count} employees", id, applied);
        }
    }

    public class DeleteSalaryRevision(
        IRepository<SalaryRevision> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IDeleteSalaryRevision
    {
        public async Task DeleteAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin) throw new ValidationException("scope", "Only HR can delete salary revisions.");

            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.SalaryRevision, id);
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(SalaryRevision), id.ToString());
            if (entity.Status == SalaryRevisionStatus.Applied)
                throw new ValidationException(nameof(id), "An applied revision cannot be deleted.");

            repository.Delete(entity);   // lines cascade
            await repository.SaveChangesAsync();
        }
    }
}
