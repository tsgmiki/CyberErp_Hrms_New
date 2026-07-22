using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.JobCategories.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.JobCategories
{
    public interface ICreateJobCategory { Task<Guid> CreateAsync(CreateJobCategoryDto dto); }
    public interface IUpdateJobCategory { Task UpdateAsync(UpdateJobCategoryDto dto); }
    public interface IDeleteJobCategory { Task DeleteAsync(Guid id); }
    public interface IGetJobCategoryById { Task<JobCategoryDto> GetAsync(Guid id); }
    public interface IGetAllJobCategories { Task<PaginatedResponse<JobCategoryDto>> GetAsync(GetAllRequest request); }

    public class CreateJobCategory(
        IRepository<JobCategory> repository,
        IValidator<CreateJobCategoryDto> validator,
        ILogger<CreateJobCategory> logger) : ICreateJobCategory
    {
        public async Task<Guid> CreateAsync(CreateJobCategoryDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code))
                throw new DuplicateException(nameof(JobCategory), nameof(dto.Code), dto.Code);

            var entity = JobCategory.Create(dto.Name, dto.Code, dto.Description, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created JobCategory {Id} ({Code})", entity.Id, entity.Code);
            return entity.Id;
        }
    }

    public class UpdateJobCategory(
        IRepository<JobCategory> repository,
        IValidator<UpdateJobCategoryDto> validator,
        ILogger<UpdateJobCategory> logger) : IUpdateJobCategory
    {
        public async Task UpdateAsync(UpdateJobCategoryDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Load tracked so EF keeps the real original RowVersion for the concurrency check.
            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(JobCategory), dto.Id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != dto.Id))
                throw new DuplicateException(nameof(JobCategory), nameof(dto.Code), dto.Code);

            entity.Update(dto.Name, dto.Code, dto.Description, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated JobCategory {Id}", entity.Id);
        }
    }

    public class DeleteJobCategory(
        IRepository<JobCategory> repository,
        IRepository<PositionClass> positionClassRepository,
        ILogger<DeleteJobCategory> logger) : IDeleteJobCategory
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(JobCategory), id.ToString());

            if (await positionClassRepository.GetAll().AnyAsync(p => p.JobCategoryId == id))
                throw new ValidationException(nameof(id), "Cannot delete a job category that is referenced by one or more position classes.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted JobCategory {Id}", id);
        }
    }

    public class GetJobCategoryById(IRepository<JobCategory> repository) : IGetJobCategoryById
    {
        public async Task<JobCategoryDto> GetAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(JobCategory), id.ToString());
            return new JobCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }
    }

    public class GetAllJobCategories(IRepository<JobCategory> repository) : IGetAllJobCategories
    {
        public async Task<PaginatedResponse<JobCategoryDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(c => new JobCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return new PaginatedResponse<JobCategoryDto> { Total = total, Data = data };
        }
    }
}
