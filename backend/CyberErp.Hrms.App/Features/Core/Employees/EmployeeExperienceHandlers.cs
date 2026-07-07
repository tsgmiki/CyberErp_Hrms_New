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
    public class EmployeeExperienceDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string Organization { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Responsibilities { get; set; }
        public int DocumentCount { get; set; }
    }

    public class SaveEmployeeExperienceDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string Organization { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Responsibilities { get; set; }
    }

    public class SaveEmployeeExperienceDtoValidator : AbstractValidator<SaveEmployeeExperienceDto>
    {
        public SaveEmployeeExperienceDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Organization).NotEmpty().MaximumLength(300);
            RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(200);
            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate!.Value)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("End date cannot be before start date.");
        }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface ISaveEmployeeExperience { Task<Guid> SaveAsync(SaveEmployeeExperienceDto dto); }
    public interface IDeleteEmployeeExperience { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeExperiences { Task<List<EmployeeExperienceDto>> GetAsync(Guid employeeId); }

    // ---- Handlers ---------------------------------------------------------------
    public class SaveEmployeeExperience(
        IRepository<EmployeeExperience> repository,
        IRepository<Employee> employeeRepository,
        IValidator<SaveEmployeeExperienceDto> validator,
        ILogger<SaveEmployeeExperience> logger) : ISaveEmployeeExperience
    {
        public async Task<Guid> SaveAsync(SaveEmployeeExperienceDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());
            // API stays employee-scoped; the row is owned by the employee's person.
            var personId = await EmployeeGuard.ResolvePersonIdAsync(employeeRepository, dto.EmployeeId);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.PersonId == personId)
                    ?? throw new NotFoundException(nameof(EmployeeExperience), dto.Id.Value.ToString());
                entity.Update(dto.Organization, dto.JobTitle, dto.StartDate, dto.EndDate, dto.Responsibilities);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeExperience {Id}", entity.Id);
                return entity.Id;
            }

            var created = EmployeeExperience.Create(personId, dto.Organization, dto.JobTitle,
                dto.StartDate, dto.EndDate, dto.Responsibilities);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeExperience {Id}", created.Id);
            return created.Id;
        }
    }

    public class DeleteEmployeeExperience(
        IRepository<EmployeeExperience> repository,
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeDocument> documentRepository,
        ILogger<DeleteEmployeeExperience> logger) : IDeleteEmployeeExperience
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeExperience), id.ToString());
            await EmployeeGuard.EnsurePersonVisibleAsync(employeeRepository, entity.PersonId);

            // Attached documents cascade with the record (no FK — polymorphic owner).
            await DocumentStorage.DeleteForOwnerAsync(documentRepository, EmployeeDocumentOwner.Experience, id);
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeExperience {Id}", id);
        }
    }

    public class GetEmployeeExperiences(
        IRepository<EmployeeExperience> repository,
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeDocument> documentRepository) : IGetEmployeeExperiences
    {
        public async Task<List<EmployeeExperienceDto>> GetAsync(Guid employeeId)
        {
            var personId = await EmployeeGuard.ResolvePersonIdAsync(employeeRepository, employeeId);

            return await repository.GetAll()
                .Where(x => x.PersonId == personId)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new EmployeeExperienceDto
                {
                    Id = x.Id,
                    EmployeeId = employeeId,
                    Organization = x.Organization,
                    JobTitle = x.JobTitle,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Responsibilities = x.Responsibilities,
                    DocumentCount = documentRepository.GetAll()
                        .Count(d => d.OwnerType == EmployeeDocumentOwner.Experience && d.OwnerId == x.Id)
                })
                .ToListAsync();
        }
    }
}
