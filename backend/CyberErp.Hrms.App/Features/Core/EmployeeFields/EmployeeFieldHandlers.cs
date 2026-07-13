using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.EmployeeFields
{
    // ---- DTOs ---------------------------------------------------------------
    public class EmployeeFieldDto
    {
        public Guid Id { get; set; }
        /// <summary>Which form this field applies to (Employee/Education/Experience/Dependent/Movement/Discipline/Termination).</summary>
        public string OwnerType { get; set; } = nameof(EmployeeFieldOwnerType.Employee);
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateEmployeeFieldDto
    {
        /// <summary>Which form this field applies to; defaults to the main Employee form.</summary>
        public string OwnerType { get; set; } = nameof(EmployeeFieldOwnerType.Employee);
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class UpdateEmployeeFieldDto : CreateEmployeeFieldDto
    {
        public Guid Id { get; set; }
    }

    public class CreateEmployeeFieldDtoValidator : AbstractValidator<CreateEmployeeFieldDto>
    {
        public CreateEmployeeFieldDtoValidator()
        {
            RuleFor(x => x.OwnerType).NotEmpty()
                .Must(v => Enum.TryParse<EmployeeFieldOwnerType>(v, out _))
                .WithMessage("OwnerType must be one of: Employee, Education, Experience, Dependent, Movement, Discipline, Termination.");
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
                .Matches("^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("Name must be a letter followed by letters, digits or underscores.");
            RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DataType).NotEmpty()
                .Must(v => Enum.TryParse<EmployeeFieldDataType>(v, out _))
                .WithMessage("DataType must be one of: Text, Number, Date, Boolean, Select.");
            RuleFor(x => x.Options).NotEmpty()
                .When(x => x.DataType == nameof(EmployeeFieldDataType.Select))
                .WithMessage("Select fields require comma-separated options.");
        }
    }

    public class UpdateEmployeeFieldDtoValidator : AbstractValidator<UpdateEmployeeFieldDto>
    {
        public UpdateEmployeeFieldDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.OwnerType).NotEmpty()
                .Must(v => Enum.TryParse<EmployeeFieldOwnerType>(v, out _))
                .WithMessage("OwnerType must be one of: Employee, Education, Experience, Dependent, Movement, Discipline, Termination.");
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
                .Matches("^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("Name must be a letter followed by letters, digits or underscores.");
            RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DataType).NotEmpty()
                .Must(v => Enum.TryParse<EmployeeFieldDataType>(v, out _))
                .WithMessage("DataType must be one of: Text, Number, Date, Boolean, Select.");
            RuleFor(x => x.Options).NotEmpty()
                .When(x => x.DataType == nameof(EmployeeFieldDataType.Select))
                .WithMessage("Select fields require comma-separated options.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ICreateEmployeeField { Task<Guid> CreateAsync(CreateEmployeeFieldDto dto); }
    public interface IUpdateEmployeeField { Task UpdateAsync(UpdateEmployeeFieldDto dto); }
    public interface IDeleteEmployeeField { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeFieldById { Task<EmployeeFieldDto> GetAsync(Guid id); }
    public interface IGetAllEmployeeFields { Task<PaginatedResponse<EmployeeFieldDto>> GetAsync(GetAllRequest request); }

    internal static class EmployeeFieldMapper
    {
        internal static readonly System.Linq.Expressions.Expression<Func<EmployeeFieldDefinition, EmployeeFieldDto>> Projection = f => new EmployeeFieldDto
        {
            Id = f.Id,
            OwnerType = f.OwnerType.ToString(),
            Name = f.Name,
            Label = f.Label,
            DataType = f.DataType.ToString(),
            Options = f.Options,
            IsRequired = f.IsRequired,
            IsActive = f.IsActive,
            SortOrder = f.SortOrder
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class CreateEmployeeField(
        IRepository<EmployeeFieldDefinition> repository,
        IValidator<CreateEmployeeFieldDto> validator,
        ILogger<CreateEmployeeField> logger) : ICreateEmployeeField
    {
        public async Task<Guid> CreateAsync(CreateEmployeeFieldDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var ownerType = Enum.Parse<EmployeeFieldOwnerType>(dto.OwnerType);
            // Names are unique per (tenant, owner type) — each form has its own field namespace.
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.OwnerType == ownerType))
                throw new DuplicateException(nameof(EmployeeFieldDefinition), nameof(dto.Name), dto.Name);

            var entity = EmployeeFieldDefinition.Create(
                dto.Name, dto.Label, Enum.Parse<EmployeeFieldDataType>(dto.DataType),
                ownerType, dto.Options, dto.IsRequired, dto.IsActive, dto.SortOrder);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeFieldDefinition {Id} ({Name})", entity.Id, entity.Name);
            return entity.Id;
        }
    }

    public class UpdateEmployeeField(
        IRepository<EmployeeFieldDefinition> repository,
        IValidator<UpdateEmployeeFieldDto> validator,
        ILogger<UpdateEmployeeField> logger) : IUpdateEmployeeField
    {
        public async Task UpdateAsync(UpdateEmployeeFieldDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(EmployeeFieldDefinition), dto.Id.ToString());

            var ownerType = Enum.Parse<EmployeeFieldOwnerType>(dto.OwnerType);
            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.OwnerType == ownerType && x.Id != dto.Id))
                throw new DuplicateException(nameof(EmployeeFieldDefinition), nameof(dto.Name), dto.Name);

            entity.Update(dto.Name, dto.Label, Enum.Parse<EmployeeFieldDataType>(dto.DataType),
                ownerType, dto.Options, dto.IsRequired, dto.IsActive, dto.SortOrder);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated EmployeeFieldDefinition {Id}", entity.Id);
        }
    }

    public class DeleteEmployeeField(
        IRepository<EmployeeFieldDefinition> repository,
        IRepository<EmployeeFieldValue> valueRepository,
        ILogger<DeleteEmployeeField> logger) : IDeleteEmployeeField
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeFieldDefinition), id.ToString());

            if (await valueRepository.GetAll().AnyAsync(v => v.FieldDefinitionId == id))
                throw new ValidationException(nameof(id),
                    "This field has stored employee values. Deactivate it instead of deleting to preserve history.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeFieldDefinition {Id}", id);
        }
    }

    public class GetEmployeeFieldById(IRepository<EmployeeFieldDefinition> repository) : IGetEmployeeFieldById
    {
        public async Task<EmployeeFieldDto> GetAsync(Guid id)
        {
            return await repository.GetAll()
                .Where(x => x.Id == id)
                .Select(EmployeeFieldMapper.Projection)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(EmployeeFieldDefinition), id.ToString());
        }
    }

    public class GetAllEmployeeFields(IRepository<EmployeeFieldDefinition> repository) : IGetAllEmployeeFields
    {
        public async Task<PaginatedResponse<EmployeeFieldDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Label.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);
            if (!string.IsNullOrWhiteSpace(request.OwnerType) && Enum.TryParse<EmployeeFieldOwnerType>(request.OwnerType, out var owner))
                query = query.Where(x => x.OwnerType == owner);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Label)
                .Skip(skip).Take(take)
                .Select(EmployeeFieldMapper.Projection)
                .ToListAsync();

            return new PaginatedResponse<EmployeeFieldDto> { Total = total, Data = data };
        }
    }
}
