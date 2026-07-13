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
        public DateTime ViolationDate { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MeasureType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public string? Resolution { get; set; }
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
        /// <summary>Submitted values for this form's dynamic custom fields (HC021).</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
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
    public interface IDeleteDisciplinaryMeasure { Task DeleteAsync(Guid id); }

    // ---- Handlers ---------------------------------------------------------------
    public class SaveDisciplinaryMeasure(
        IRepository<DisciplinaryMeasure> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        ICustomFieldService customFields,
        IValidator<SaveDisciplinaryMeasureDto> validator,
        ILogger<SaveDisciplinaryMeasure> logger) : ISaveDisciplinaryMeasure
    {
        public async Task<Guid> SaveAsync(SaveDisciplinaryMeasureDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, dto.EmployeeId);

            var measureType = Enum.Parse<DisciplinaryMeasureType>(dto.MeasureType, true);
            var status = Enum.Parse<DisciplinaryStatus>(dto.Status, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.DisciplinaryMeasure, dto.Id.Value);

                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.EmployeeId == dto.EmployeeId)
                    ?? throw new NotFoundException(nameof(DisciplinaryMeasure), dto.Id.Value.ToString());
                entity.Update(dto.ViolationDate, dto.ViolationType, measureType, status,
                    dto.Description, dto.EffectiveDate, dto.Resolution);
                repository.UpdateAsync(entity);
                await customFields.ApplyAsync(EmployeeFieldOwnerType.Discipline, entity.Id, dto.CustomFields);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated DisciplinaryMeasure {Id}", entity.Id);
                return entity.Id;
            }

            var created = DisciplinaryMeasure.Create(dto.EmployeeId, dto.ViolationDate, dto.ViolationType,
                measureType, status, dto.Description, dto.EffectiveDate, dto.Resolution);
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

            return created.Id;
        }
    }

    public class GetDisciplinaryMeasures(
        IRepository<DisciplinaryMeasure> repository,
        IRepository<Employee> employeeRepository,
        ICustomFieldService customFields) : IGetDisciplinaryMeasures
    {
        public async Task<List<DisciplinaryMeasureDto>> GetAsync(Guid employeeId)
        {
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, employeeId);

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
                    Resolution = x.Resolution
                })
                .ToListAsync();

            var byOwner = await customFields.GetValuesForOwnersAsync(
                EmployeeFieldOwnerType.Discipline, list.Select(x => x.Id).ToList());
            foreach (var item in list)
                item.CustomFields = byOwner.TryGetValue(item.Id, out var m) ? m : new();

            return list;
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
