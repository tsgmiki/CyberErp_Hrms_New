using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.EmployeeFields;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class DisciplinaryMeasureDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public DateTime ViolationDate { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MeasureType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public string? Resolution { get; set; }
        /// <summary>HC223 — end of the measure's lifetime (null = until Cancelled).</summary>
        public DateTime? ValidUntil { get; set; }
        public bool AffectsPromotion { get; set; }
        public bool AffectsReward { get; set; }
        /// <summary>HC222 — who raised the case (null = HR/system).</summary>
        public Guid? RaisedByEmployeeId { get; set; }
        public string? RaisedByName { get; set; }
        /// <summary>Values of this form's dynamic custom fields (HC021), keyed by field name.</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveDisciplinaryMeasureDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime ViolationDate { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MeasureType { get; set; } = nameof(DisciplinaryMeasureType.VerbalWarning);
        public string Status { get; set; } = nameof(DisciplinaryStatus.Open);
        public DateTime? EffectiveDate { get; set; }
        public string? Resolution { get; set; }
        /// <summary>HC223 — end of the measure's lifetime (null = until Cancelled).</summary>
        public DateTime? ValidUntil { get; set; }
        /// <summary>HC223/HC225 — while active, this measure blocks promotion.</summary>
        public bool AffectsPromotion { get; set; }
        /// <summary>HC223/HC225 — while active, this measure blocks reward/bonus.</summary>
        public bool AffectsReward { get; set; }
        /// <summary>Submitted values for this form's dynamic custom fields (HC021).</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    /// <summary>Row of the standalone, role-scoped disciplinary case list (HC222/HC225).</summary>
    public class DisciplinaryCaseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public DateTime ViolationDate { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string MeasureType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool AffectsPromotion { get; set; }
        public bool AffectsReward { get; set; }
        public Guid? RaisedByEmployeeId { get; set; }
        public string? RaisedByName { get; set; }
    }

    public class SaveDisciplinaryMeasureDtoValidator : AbstractValidator<SaveDisciplinaryMeasureDto>
    {
        public SaveDisciplinaryMeasureDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.ViolationDate).NotEmpty().WithMessage("Violation date is required.");
            RuleFor(x => x.ViolationType).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.Resolution).MaximumLength(2000);
            RuleFor(x => x.MeasureType).NotEmpty()
                .Must(v => Enum.TryParse<DisciplinaryMeasureType>(v, true, out _))
                .WithMessage("MeasureType must be one of: VerbalWarning, WrittenWarning, FinalWarning, Suspension, SalaryDeduction, Demotion, Termination.");
            RuleFor(x => x.Status).NotEmpty()
                .Must(v => Enum.TryParse<DisciplinaryStatus>(v, true, out _))
                .WithMessage("Status must be one of: Open, UnderReview, Resolved, Cancelled.");
        }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface ISaveDisciplinaryMeasure { Task<Guid> SaveAsync(SaveDisciplinaryMeasureDto dto); }
    public interface IGetDisciplinaryMeasures { Task<List<DisciplinaryMeasureDto>> GetAsync(Guid employeeId); }
    public interface IGetDisciplinaryMeasureById { Task<DisciplinaryMeasureDto?> GetAsync(Guid id); }
    public interface IGetDisciplinaryCases { Task<Common.DTOs.PaginatedResponse<DisciplinaryCaseDto>> GetAsync(Common.DTOs.GetAllRequest request); }
    public interface IGetDisciplinaryEligibility { Task<DisciplinaryEligibilityDto> GetAsync(Guid employeeId); }
    public interface IDeleteDisciplinaryMeasure { Task DeleteAsync(Guid id); }

    // ---- Handlers ---------------------------------------------------------------
    public class SaveDisciplinaryMeasure(
        IRepository<DisciplinaryMeasure> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        Performance.IPerformanceVisibilityService visibility,
        IDisciplinaryNotifier notifier,
        ICustomFieldService customFields,
        IValidator<SaveDisciplinaryMeasureDto> validator,
        ILogger<SaveDisciplinaryMeasure> logger) : ISaveDisciplinaryMeasure
    {
        public async Task<Guid> SaveAsync(SaveDisciplinaryMeasureDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // HC222 — work units raise cases against employees in their subtree; HR/admin anywhere;
            // never against yourself. EnsureEmployeeVisibleAsync is the existence/branch precondition
            // (NotFound on a bad id); CanAccessEmployeeAsync is the authorization (admin | manager-of-
            // subtree). This intentionally closes the prior gap where any branch user could record a
            // measure — only HR and the employee's own management line may now raise a case.
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, dto.EmployeeId);
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin && scope.EmployeeId.HasValue && scope.EmployeeId.Value == dto.EmployeeId)
                throw new ValidationException(nameof(dto.EmployeeId), "You cannot raise a disciplinary case against yourself.");
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(dto.EmployeeId), "You do not have permission to raise a disciplinary case for this employee.");

            var measureType = Enum.Parse<DisciplinaryMeasureType>(dto.MeasureType, true);
            var status = Enum.Parse<DisciplinaryStatus>(dto.Status, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.DisciplinaryMeasure, dto.Id.Value);

                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.EmployeeId == dto.EmployeeId)
                    ?? throw new NotFoundException(nameof(DisciplinaryMeasure), dto.Id.Value.ToString());
                entity.Update(dto.ViolationDate, dto.ViolationType, measureType, status,
                    dto.Description, dto.EffectiveDate, dto.Resolution,
                    dto.ValidUntil, dto.AffectsPromotion, dto.AffectsReward);
                repository.UpdateAsync(entity);
                await customFields.ApplyAsync(EmployeeFieldOwnerType.Discipline, entity.Id, dto.CustomFields);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated DisciplinaryMeasure {Id}", entity.Id);
                return entity.Id;
            }

            var created = DisciplinaryMeasure.Create(dto.EmployeeId, dto.ViolationDate, dto.ViolationType,
                measureType, status, dto.Description, dto.EffectiveDate, dto.Resolution,
                dto.ValidUntil, dto.AffectsPromotion, dto.AffectsReward, scope.EmployeeId);
            await repository.AddAsync(created);
            await customFields.ApplyAsync(EmployeeFieldOwnerType.Discipline, created.Id, dto.CustomFields);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created DisciplinaryMeasure {Id} for Employee {EmployeeId}", created.Id, dto.EmployeeId);

            // Route the case through its approval chain when one is configured.
            var employeeName = await employeeRepository.GetAll()
                .Where(e => e.Id == dto.EmployeeId && e.Person != null)
                .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName + " (" + e.EmployeeNumber + ")")
                .FirstOrDefaultAsync();
            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.DisciplinaryMeasure, created.Id, dto.EmployeeId,
                $"Disciplinary ({dto.ViolationType}) — {employeeName}");

            // Best-effort stakeholder notification (never blocks the operation).
            await notifier.SubmittedAsync(created.Id);

            return created.Id;
        }
    }

    public class GetDisciplinaryMeasures(
        IRepository<DisciplinaryMeasure> repository,
        IRepository<Employee> employeeRepository,
        Performance.IPerformanceVisibilityService visibility,
        ICustomFieldService customFields) : IGetDisciplinaryMeasures
    {
        public async Task<List<DisciplinaryMeasureDto>> GetAsync(Guid employeeId)
        {
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, employeeId);
            // Same visibility rule as the by-id/paged reads: the employee themselves, their manager, or HR.
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException("access", "You do not have access to this employee's disciplinary records.");

            var employees = employeeRepository.GetAll();
            var list = await repository.GetAll()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.ViolationDate)
                .Select(x => new DisciplinaryMeasureDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    ViolationDate = x.ViolationDate,
                    ViolationType = x.ViolationType,
                    Description = x.Description,
                    MeasureType = x.MeasureType.ToString(),
                    Status = x.Status.ToString(),
                    EffectiveDate = x.EffectiveDate,
                    Resolution = x.Resolution,
                    ValidUntil = x.ValidUntil,
                    AffectsPromotion = x.AffectsPromotion,
                    AffectsReward = x.AffectsReward,
                    RaisedByEmployeeId = x.RaisedByEmployeeId,
                    RaisedByName = employees.Where(e => e.Id == x.RaisedByEmployeeId && e.Person != null)
                        .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault()
                })
                .ToListAsync();

            var byOwner = await customFields.GetValuesForOwnersAsync(
                EmployeeFieldOwnerType.Discipline, list.Select(x => x.Id).ToList());
            foreach (var item in list)
                item.CustomFields = byOwner.TryGetValue(item.Id, out var m) ? m : new();

            return list;
        }
    }

    // ---- Standalone scoped list (HC222 intake / HC225 tracking) --------------
    // Role-scoped: admin → all; manager → their unit subtree (+ cases they raised); others → own.
    public class GetDisciplinaryCases(
        IRepository<DisciplinaryMeasure> repository,
        IRepository<Employee> employeeRepository,
        Performance.IPerformanceVisibilityService visibility) : IGetDisciplinaryCases
    {
        public async Task<Common.DTOs.PaginatedResponse<DisciplinaryCaseDto>> GetAsync(Common.DTOs.GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            var employees = employeeRepository.GetAll();

            // Visibility as a single SQL predicate (same model as the movement list).
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var unitIds = scope.UnitIds;
                    query = query.Where(d => d.EmployeeId == myEmp || d.RaisedByEmployeeId == myEmp ||
                        employees.Any(e => e.Id == d.EmployeeId && e.Position != null && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(d => d.EmployeeId == myEmp || d.RaisedByEmployeeId == myEmp);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DisciplinaryStatus>(request.Status, true, out var st))
                query = query.Where(d => d.Status == st);
            if (request.EmployeeId.HasValue)
                query = query.Where(d => d.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(d => d.ViolationType.Contains(term) ||
                    employees.Any(e => e.Id == d.EmployeeId &&
                        (e.EmployeeNumber.Contains(term) ||
                         (e.Person != null && (e.Person.FirstName.Contains(term) || e.Person.GrandFatherName.Contains(term))))));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(d => d.ViolationDate).ThenByDescending(d => d.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new DisciplinaryCaseDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId && e.Person != null)
                        .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    EmployeeNumber = employees.Where(e => e.Id == x.EmployeeId).Select(e => e.EmployeeNumber).FirstOrDefault(),
                    ViolationDate = x.ViolationDate,
                    ViolationType = x.ViolationType,
                    MeasureType = x.MeasureType.ToString(),
                    Status = x.Status.ToString(),
                    EffectiveDate = x.EffectiveDate,
                    ValidUntil = x.ValidUntil,
                    AffectsPromotion = x.AffectsPromotion,
                    AffectsReward = x.AffectsReward,
                    RaisedByEmployeeId = x.RaisedByEmployeeId,
                    RaisedByName = employees.Where(e => e.Id == x.RaisedByEmployeeId && e.Person != null)
                        .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault()
                })
                .ToListAsync();

            return new Common.DTOs.PaginatedResponse<DisciplinaryCaseDto> { Total = total, Data = data };
        }
    }

    // ---- Get one (for the standalone case form) — visibility-scoped ---------
    public class GetDisciplinaryMeasureById(
        IRepository<DisciplinaryMeasure> repository,
        IRepository<Employee> employeeRepository,
        Performance.IPerformanceVisibilityService visibility,
        ICustomFieldService customFields) : IGetDisciplinaryMeasureById
    {
        public async Task<DisciplinaryMeasureDto?> GetAsync(Guid id)
        {
            var employees = employeeRepository.GetAll();
            var dto = await repository.GetAll().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new DisciplinaryMeasureDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId && e.Person != null)
                        .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    EmployeeNumber = employees.Where(e => e.Id == x.EmployeeId).Select(e => e.EmployeeNumber).FirstOrDefault(),
                    ViolationDate = x.ViolationDate,
                    ViolationType = x.ViolationType,
                    Description = x.Description,
                    MeasureType = x.MeasureType.ToString(),
                    Status = x.Status.ToString(),
                    EffectiveDate = x.EffectiveDate,
                    Resolution = x.Resolution,
                    ValidUntil = x.ValidUntil,
                    AffectsPromotion = x.AffectsPromotion,
                    AffectsReward = x.AffectsReward,
                    RaisedByEmployeeId = x.RaisedByEmployeeId,
                    RaisedByName = employees.Where(e => e.Id == x.RaisedByEmployeeId && e.Person != null)
                        .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault()
                })
                .FirstOrDefaultAsync();
            if (dto is null) return null;

            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(id), "You do not have access to this disciplinary case.");

            var byOwner = await customFields.GetValuesForOwnersAsync(EmployeeFieldOwnerType.Discipline, [dto.Id]);
            dto.CustomFields = byOwner.TryGetValue(dto.Id, out var m) ? m : new();
            return dto;
        }
    }

    // ---- Eligibility read (HC225) — visibility-scoped snapshot for the UI ----
    public class GetDisciplinaryEligibility(
        IRepository<Employee> employeeRepository,
        Performance.IPerformanceVisibilityService visibility,
        IDisciplinaryEligibilityService eligibility) : IGetDisciplinaryEligibility
    {
        public async Task<DisciplinaryEligibilityDto> GetAsync(Guid employeeId)
        {
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, employeeId);
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(employeeId), "You do not have access to this employee's disciplinary status.");
            return await eligibility.EvaluateAsync(employeeId);
        }
    }

    public class DeleteDisciplinaryMeasure(
        IRepository<DisciplinaryMeasure> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        ICustomFieldService customFields,
        ILogger<DeleteDisciplinaryMeasure> logger) : IDeleteDisciplinaryMeasure
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.DisciplinaryMeasure, id);

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(DisciplinaryMeasure), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, entity.EmployeeId);

            await customFields.DeleteForOwnerAsync(EmployeeFieldOwnerType.Discipline, id);
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted DisciplinaryMeasure {Id}", id);
        }
    }
}
