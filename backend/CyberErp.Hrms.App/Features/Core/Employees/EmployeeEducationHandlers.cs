using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class EmployeeEducationDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EducationLevel { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string? FieldOfStudy { get; set; }
        public string? Qualification { get; set; }
        public int? GraduationYear { get; set; }
        public string? Remark { get; set; }
        public int DocumentCount { get; set; }
    }

    public class SaveEmployeeEducationDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EducationLevel { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string? FieldOfStudy { get; set; }
        public string? Qualification { get; set; }
        public int? GraduationYear { get; set; }
        public string? Remark { get; set; }
    }

    public class SaveEmployeeEducationDtoValidator : AbstractValidator<SaveEmployeeEducationDto>
    {
        public SaveEmployeeEducationDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.EducationLevel).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Institution).NotEmpty().MaximumLength(300);
            RuleFor(x => x.GraduationYear).InclusiveBetween(1900, 2100).When(x => x.GraduationYear.HasValue);
        }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface ISaveEmployeeEducation { Task<Guid> SaveAsync(SaveEmployeeEducationDto dto); }
    public interface IDeleteEmployeeEducation { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeEducations { Task<List<EmployeeEducationDto>> GetAsync(Guid employeeId); }

    internal static class EmployeeGuard
    {
        /// <summary>Child access goes through the (tenant + branch filtered) employee repository.</summary>
        internal static async Task EnsureEmployeeVisibleAsync(IRepository<Employee> employees, Guid employeeId)
        {
            if (!await employees.GetAll().AnyAsync(e => e.Id == employeeId))
                throw new NotFoundException(nameof(Employee), employeeId.ToString());
        }

        /// <summary>
        /// The child APIs stay employee-scoped while the rows are person-owned: resolves the
        /// employee's PersonId (branch-visibility enforced by the filtered employee repository).
        /// </summary>
        internal static async Task<Guid> ResolvePersonIdAsync(IRepository<Employee> employees, Guid employeeId)
        {
            var personId = await employees.GetAll()
                .Where(e => e.Id == employeeId)
                .Select(e => (Guid?)e.PersonId)
                .FirstOrDefaultAsync();
            return personId ?? throw new NotFoundException(nameof(Employee), employeeId.ToString());
        }

        /// <summary>Visibility for person-owned rows: some visible employee must reference the person.</summary>
        internal static async Task EnsurePersonVisibleAsync(IRepository<Employee> employees, Guid personId)
        {
            if (!await employees.GetAll().AnyAsync(e => e.PersonId == personId))
                throw new NotFoundException(nameof(Person), personId.ToString());
        }
    }

    // ---- Handlers ---------------------------------------------------------------
    public class SaveEmployeeEducation(
        IRepository<EmployeeEducation> repository,
        IRepository<Employee> employeeRepository,
        IValidator<SaveEmployeeEducationDto> validator,
        ILogger<SaveEmployeeEducation> logger) : ISaveEmployeeEducation
    {
        public async Task<Guid> SaveAsync(SaveEmployeeEducationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            // API stays employee-scoped; the row is owned by the employee's person.
            var personId = await EmployeeGuard.ResolvePersonIdAsync(employeeRepository, dto.EmployeeId);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.PersonId == personId)
                    ?? throw new NotFoundException(nameof(EmployeeEducation), dto.Id.Value.ToString());
                entity.Update(dto.EducationLevel, dto.Institution, dto.FieldOfStudy, dto.Qualification, dto.GraduationYear, dto.Remark);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeEducation {Id}", entity.Id);
                return entity.Id;
            }

            var created = EmployeeEducation.Create(personId, dto.EducationLevel, dto.Institution,
                dto.FieldOfStudy, dto.Qualification, dto.GraduationYear, dto.Remark);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeEducation {Id}", created.Id);
            return created.Id;
        }
    }

    public class DeleteEmployeeEducation(
        IRepository<EmployeeEducation> repository,
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeDocument> documentRepository,
        ILogger<DeleteEmployeeEducation> logger) : IDeleteEmployeeEducation
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeEducation), id.ToString());
            await EmployeeGuard.EnsurePersonVisibleAsync(employeeRepository, entity.PersonId);

            // Attached documents cascade with the record (no FK — polymorphic owner).
            await DocumentStorage.DeleteForOwnerAsync(documentRepository, EmployeeDocumentOwner.Education, id);
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeEducation {Id}", id);
        }
    }

    public class GetEmployeeEducations(
        IRepository<EmployeeEducation> repository,
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeDocument> documentRepository) : IGetEmployeeEducations
    {
        public async Task<List<EmployeeEducationDto>> GetAsync(Guid employeeId)
        {
            var personId = await EmployeeGuard.ResolvePersonIdAsync(employeeRepository, employeeId);

            return await repository.GetAll()
                .Where(x => x.PersonId == personId)
                .OrderByDescending(x => x.GraduationYear)
                .Select(x => new EmployeeEducationDto
                {
                    Id = x.Id,
                    EmployeeId = employeeId,
                    EducationLevel = x.EducationLevel,
                    Institution = x.Institution,
                    FieldOfStudy = x.FieldOfStudy,
                    Qualification = x.Qualification,
                    GraduationYear = x.GraduationYear,
                    Remark = x.Remark,
                    DocumentCount = documentRepository.GetAll()
                        .Count(d => d.OwnerType == EmployeeDocumentOwner.Education && d.OwnerId == x.Id)
                })
                .ToListAsync();
        }
    }
}
