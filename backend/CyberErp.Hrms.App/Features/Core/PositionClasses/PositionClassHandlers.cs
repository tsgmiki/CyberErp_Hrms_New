using CyberErp.Hrms.App.Common;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.PositionClasses.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.PositionClasses
{
    public interface ICreatePositionClass { Task<Guid> CreateAsync(CreatePositionClassDto dto); }
    public interface IUpdatePositionClass { Task UpdateAsync(UpdatePositionClassDto dto); }
    public interface IDeletePositionClass { Task DeleteAsync(Guid id); }
    public interface IGetPositionClassById { Task<PositionClassDto> GetAsync(Guid id); }
    public interface IGetAllPositionClasses { Task<PaginatedResponse<PositionClassDto>> GetAsync(GetAllRequest request); }

    public class CreatePositionClass(
        IRepository<PositionClass> repository,
        IRepository<SalaryScale> salaryScaleRepository,
        IRepository<JobCategory> jobCategoryRepository,
        IValidator<CreatePositionClassDto> validator,
        ILogger<CreatePositionClass> logger) : ICreatePositionClass
    {
        public async Task<Guid> CreateAsync(CreatePositionClassDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code))
                throw new DuplicateException(nameof(PositionClass), nameof(dto.Code), dto.Code);

            await EnsureReferencesExistAsync(dto, salaryScaleRepository, jobCategoryRepository, repository);

            var entity = PositionClass.Create(
                dto.Code, dto.Title, dto.SalaryScaleId, dto.JobCategoryId, dto.AllocatedHeadcount,
                dto.ReportsToPositionClassId, dto.WorkLocationId, dto.MinQualifications,
                dto.MinExperienceYears, dto.Skills, dto.Description, dto.IsActive,
                dto.MinimumAge, dto.MaximumAge, dto.WeeklyWorkingHours);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created PositionClass {Id} ({Code})", entity.Id, entity.Code);
            return entity.Id;
        }

        internal static async Task EnsureReferencesExistAsync(
            CreatePositionClassDto dto,
            IRepository<SalaryScale> salaryScales,
            IRepository<JobCategory> jobCategories,
            IRepository<PositionClass> classes)
        {
            if (!await salaryScales.GetAll().AnyAsync(x => x.Id == dto.SalaryScaleId))
                throw new NotFoundException(nameof(SalaryScale), dto.SalaryScaleId.ToString());
            if (!await jobCategories.GetAll().AnyAsync(x => x.Id == dto.JobCategoryId))
                throw new NotFoundException(nameof(JobCategory), dto.JobCategoryId.ToString());
            if (dto.ReportsToPositionClassId.HasValue && !await classes.GetAll().AnyAsync(x => x.Id == dto.ReportsToPositionClassId.Value))
                throw new NotFoundException(nameof(PositionClass), dto.ReportsToPositionClassId.Value.ToString(), "Reports-to position class was not found.");
        }
    }

    public class UpdatePositionClass(
        IRepository<PositionClass> repository,
        IRepository<SalaryScale> salaryScaleRepository,
        IRepository<JobCategory> jobCategoryRepository,
        IValidator<UpdatePositionClassDto> validator,
        ILogger<UpdatePositionClass> logger) : IUpdatePositionClass
    {
        public async Task UpdateAsync(UpdatePositionClassDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(PositionClass), dto.Id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.Code == dto.Code && x.Id != dto.Id))
                throw new DuplicateException(nameof(PositionClass), nameof(dto.Code), dto.Code);

            await CreatePositionClass.EnsureReferencesExistAsync(dto, salaryScaleRepository, jobCategoryRepository, repository);

            if (dto.ReportsToPositionClassId.HasValue)
            {
                var parentMap = await repository.GetAll()
                    .Select(x => new { x.Id, ParentId = x.ReportsToPositionClassId })
                    .ToDictionaryAsync(x => x.Id, x => x.ParentId);
                if (HierarchyGuard.WouldCreateCycle(parentMap, dto.Id, dto.ReportsToPositionClassId))
                    throw new ValidationException(nameof(dto.ReportsToPositionClassId), "The selected reporting line would create a cycle.");
            }

            entity.Update(
                dto.Code, dto.Title, dto.SalaryScaleId, dto.JobCategoryId, dto.AllocatedHeadcount,
                dto.ReportsToPositionClassId, dto.WorkLocationId, dto.MinQualifications,
                dto.MinExperienceYears, dto.Skills, dto.Description, dto.IsActive,
                dto.MinimumAge, dto.MaximumAge, dto.WeeklyWorkingHours);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated PositionClass {Id}", entity.Id);
        }
    }

    public class DeletePositionClass(
        IRepository<PositionClass> repository,
        IRepository<Position> positionRepository,
        ILogger<DeletePositionClass> logger) : IDeletePositionClass
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(PositionClass), id.ToString());

            if (await positionRepository.GetAll().AnyAsync(p => p.PositionClassId == id))
                throw new ValidationException(nameof(id), "Cannot delete a position class that is used by one or more positions.");
            if (await repository.GetAll().AnyAsync(c => c.ReportsToPositionClassId == id))
                throw new ValidationException(nameof(id), "Cannot delete a position class that other classes report to.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted PositionClass {Id}", id);
        }
    }

    public class GetPositionClassById(IRepository<PositionClass> repository) : IGetPositionClassById
    {
        public async Task<PositionClassDto> GetAsync(Guid id)
        {
            return await repository.GetAll()
                .Where(p => p.Id == id)
                .Select(Projection)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(PositionClass), id.ToString());
        }

        internal static readonly System.Linq.Expressions.Expression<Func<PositionClass, PositionClassDto>> Projection = p => new PositionClassDto
        {
            Id = p.Id,
            Code = p.Code,
            Title = p.Title,
            AllocatedHeadcount = p.AllocatedHeadcount,
            MinQualifications = p.MinQualifications,
            MinExperienceYears = p.MinExperienceYears,
            Skills = p.Skills,
            Description = p.Description,
            IsActive = p.IsActive,
            MinimumAge = p.MinimumAge,
            MaximumAge = p.MaximumAge,
            WeeklyWorkingHours = p.WeeklyWorkingHours,
            SalaryScaleId = p.SalaryScaleId,
            JobGradeId = p.SalaryScale != null ? p.SalaryScale.JobGradeId : Guid.Empty,
            JobGradeName = p.SalaryScale != null && p.SalaryScale.JobGrade != null ? p.SalaryScale.JobGrade.Name : null,
            SalaryStep = p.SalaryScale != null && p.SalaryScale.Step != null ? p.SalaryScale.Step.Name : null,
            Salary = p.SalaryScale != null ? p.SalaryScale.Salary : (decimal?)null,
            JobCategoryId = p.JobCategoryId,
            JobCategoryName = p.JobCategory != null ? p.JobCategory.Name : null,
            WorkLocationId = p.WorkLocationId,
            WorkLocationName = p.WorkLocation != null ? p.WorkLocation.Name : null,
            ReportsToPositionClassId = p.ReportsToPositionClassId,
            ReportsToPositionClassTitle = p.ReportsToPositionClass != null ? p.ReportsToPositionClass.Title : null
        };
    }

    public class GetAllPositionClasses(IRepository<PositionClass> repository) : IGetAllPositionClasses
    {
        public async Task<PaginatedResponse<PositionClassDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Title.Contains(term) || x.Code.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Title)
                .Skip(skip).Take(take)
                .Select(Projection)
                .ToListAsync();

            return new PaginatedResponse<PositionClassDto> { Total = total, Data = data };
        }

        private static readonly System.Linq.Expressions.Expression<Func<PositionClass, PositionClassDto>> Projection =
            GetPositionClassById.Projection;
    }
}
