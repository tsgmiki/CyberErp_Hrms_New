using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.EmployeeFields;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class EmployeeDependentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsDependent { get; set; }
        public Guid? RelatedEmployeeId { get; set; }
        public string? RelatedEmployeeName { get; set; }
        public string? Remark { get; set; }
        /// <summary>Values of this form's dynamic custom fields (HC021), keyed by field name.</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveEmployeeDependentDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsDependent { get; set; }
        public Guid? RelatedEmployeeId { get; set; }
        public string? Remark { get; set; }
        /// <summary>Submitted values for this form's dynamic custom fields (HC021).</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveEmployeeDependentDtoValidator : AbstractValidator<SaveEmployeeDependentDto>
    {
        public SaveEmployeeDependentDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Relationship).NotEmpty().MaximumLength(100);
            RuleFor(x => x.RelatedEmployeeId)
                .Must((dto, related) => related != dto.EmployeeId)
                .WithMessage("A relative cannot reference the employee's own record.");
        }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface ISaveEmployeeDependent { Task<Guid> SaveAsync(SaveEmployeeDependentDto dto); }
    public interface IDeleteEmployeeDependent { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeDependents { Task<List<EmployeeDependentDto>> GetAsync(Guid employeeId); }

    // ---- Handlers ---------------------------------------------------------------
    public class SaveEmployeeDependent(
        IRepository<EmployeeDependent> repository,
        IRepository<Employee> employeeRepository,
        ICustomFieldService customFields,
        IValidator<SaveEmployeeDependentDto> validator,
        ILogger<SaveEmployeeDependent> logger) : ISaveEmployeeDependent
    {
        public async Task<Guid> SaveAsync(SaveEmployeeDependentDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            // API stays employee-scoped; the row is owned by the employee's person.
            var personId = await EmployeeGuard.ResolvePersonIdAsync(employeeRepository, dto.EmployeeId);

            // Internal relationship must point at a real (visible) employee (HC020) — not oneself.
            if (dto.RelatedEmployeeId.HasValue)
            {
                if (dto.RelatedEmployeeId.Value == dto.EmployeeId)
                    throw new ValidationException("relatedEmployeeId", "A relative cannot reference the employee's own record.");
                await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, dto.RelatedEmployeeId.Value);
            }

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.PersonId == personId)
                    ?? throw new NotFoundException(nameof(EmployeeDependent), dto.Id.Value.ToString());
                entity.Update(dto.FullName, dto.Relationship, dto.DateOfBirth, dto.PhoneNumber,
                    dto.Address, dto.IsDependent, dto.RelatedEmployeeId, dto.Remark);
                repository.UpdateAsync(entity);
                await customFields.ApplyAsync(EmployeeFieldOwnerType.Dependent, entity.Id, dto.CustomFields);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeDependent {Id}", entity.Id);
                return entity.Id;
            }

            var created = EmployeeDependent.Create(personId, dto.FullName, dto.Relationship,
                dto.DateOfBirth, dto.PhoneNumber, dto.Address, dto.IsDependent, dto.RelatedEmployeeId, dto.Remark);
            await repository.AddAsync(created);
            await customFields.ApplyAsync(EmployeeFieldOwnerType.Dependent, created.Id, dto.CustomFields);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeDependent {Id}", created.Id);
            return created.Id;
        }
    }

    public class DeleteEmployeeDependent(
        IRepository<EmployeeDependent> repository,
        IRepository<Employee> employeeRepository,
        ICustomFieldService customFields,
        ILogger<DeleteEmployeeDependent> logger) : IDeleteEmployeeDependent
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeDependent), id.ToString());
            await EmployeeGuard.EnsurePersonVisibleAsync(employeeRepository, entity.PersonId);

            await customFields.DeleteForOwnerAsync(EmployeeFieldOwnerType.Dependent, id);
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeDependent {Id}", id);
        }
    }

    public class GetEmployeeDependents(
        IRepository<EmployeeDependent> repository,
        IRepository<Employee> employeeRepository,
        ICustomFieldService customFields) : IGetEmployeeDependents
    {
        public async Task<List<EmployeeDependentDto>> GetAsync(Guid employeeId)
        {
            var personId = await EmployeeGuard.ResolvePersonIdAsync(employeeRepository, employeeId);

            // Left-join the related employee's person for internal relationships (HC020).
            var list = await repository.GetAll()
                .Where(x => x.PersonId == personId)
                .OrderBy(x => x.FullName)
                .Select(x => new EmployeeDependentDto
                {
                    Id = x.Id,
                    EmployeeId = employeeId,
                    FullName = x.FullName,
                    Relationship = x.Relationship,
                    DateOfBirth = x.DateOfBirth,
                    PhoneNumber = x.PhoneNumber,
                    Address = x.Address,
                    IsDependent = x.IsDependent,
                    RelatedEmployeeId = x.RelatedEmployeeId,
                    RelatedEmployeeName = x.RelatedEmployeeId != null
                        ? employeeRepository.GetAllWithoutTenantFilter()
                            .Where(e => e.Id == x.RelatedEmployeeId && e.Person != null)
                            .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName)
                            .FirstOrDefault()
                        : null,
                    Remark = x.Remark
                })
                .ToListAsync();

            var byOwner = await customFields.GetValuesForOwnersAsync(
                EmployeeFieldOwnerType.Dependent, list.Select(x => x.Id).ToList());
            foreach (var item in list)
                item.CustomFields = byOwner.TryGetValue(item.Id, out var m) ? m : new();

            return list;
        }
    }
}
