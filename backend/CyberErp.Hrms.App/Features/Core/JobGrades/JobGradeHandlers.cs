using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.JobGrades.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.JobGrades
{
    // ---- Interfaces ---------------------------------------------------------
    public interface ICreateJobGrade { Task<Guid> CreateAsync(CreateJobGradeDto dto); }
    public interface IUpdateJobGrade { Task UpdateAsync(UpdateJobGradeDto dto); }
    public interface IDeleteJobGrade { Task DeleteAsync(Guid id); }
    public interface IGetJobGradeById { Task<JobGradeDto> GetAsync(Guid id); }
    public interface IGetAllJobGrades { Task<PaginatedResponse<JobGradeDto>> GetAsync(GetAllRequest request); }

    // ---- Create -------------------------------------------------------------
    public class CreateJobGrade(
        IRepository<JobGrade> repository,
        IValidator<CreateJobGradeDto> validator,
        ILogger<CreateJobGrade> logger) : ICreateJobGrade
    {
        public async Task<Guid> CreateAsync(CreateJobGradeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code))
                throw new DuplicateException(nameof(JobGrade), nameof(dto.Code), dto.Code);

            var entity = JobGrade.Create(dto.Name, dto.Code, dto.NameA);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created JobGrade {Id} ({Code})", entity.Id, entity.Code);
            return entity.Id;
        }
    }

    // ---- Update -------------------------------------------------------------
    public class UpdateJobGrade(
        IRepository<JobGrade> repository,
        IValidator<UpdateJobGradeDto> validator,
        ILogger<UpdateJobGrade> logger) : IUpdateJobGrade
    {
        public async Task UpdateAsync(UpdateJobGradeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Load tracked so EF keeps the real original RowVersion for the concurrency check.
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(JobGrade), dto.Id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != dto.Id))
                throw new DuplicateException(nameof(JobGrade), nameof(dto.Code), dto.Code);

            entity.Update(dto.Name, dto.Code, dto.NameA);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated JobGrade {Id}", entity.Id);
        }
    }

    // ---- Delete -------------------------------------------------------------
    public class DeleteJobGrade(
        IRepository<JobGrade> repository,
        IRepository<SalaryScale> salaryScaleRepository,
        IRepository<Employee> employeeRepository,
        ILogger<DeleteJobGrade> logger) : IDeleteJobGrade
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(JobGrade), id.ToString());

            if (await salaryScaleRepository.GetAll().AnyAsync(s => s.JobGradeId == id))
                throw new ValidationException(nameof(id), "Cannot delete a job grade that is referenced by one or more salary scales.");
            if (await employeeRepository.GetAll().AnyAsync(e => e.JobGradeId == id))
                throw new ValidationException(nameof(id), "Cannot delete a job grade that is assigned to one or more employees.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted JobGrade {Id}", id);
        }
    }

    // ---- Get by id ----------------------------------------------------------
    public class GetJobGradeById(IRepository<JobGrade> repository) : IGetJobGradeById
    {
        public async Task<JobGradeDto> GetAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(JobGrade), id.ToString());
            return Map(entity);
        }

        internal static JobGradeDto Map(JobGrade g) => new()
        {
            Id = g.Id,
            Name = g.Name,
            NameA = g.NameA,
            Code = g.Code
        };
    }

    // ---- Get all (paged) ----------------------------------------------------
    public class GetAllJobGrades(IRepository<JobGrade> repository) : IGetAllJobGrades
    {
        public async Task<PaginatedResponse<JobGradeDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Code).ThenBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(g => new JobGradeDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    NameA = g.NameA,
                    Code = g.Code
                })
                .ToListAsync();

            return new PaginatedResponse<JobGradeDto> { Total = total, Data = data };
        }
    }
}
