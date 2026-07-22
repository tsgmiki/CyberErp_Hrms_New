using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Training
{
    // ---- DTOs ---------------------------------------------------------------
    public class TrainingCourseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public Guid? TrainingCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string? Objectives { get; set; }
        public string? TargetAudience { get; set; }
        public string? Prerequisites { get; set; }
        public decimal? DurationHours { get; set; }
        public string DeliveryMode { get; set; } = string.Empty;
        public decimal CpdHours { get; set; }
        public bool IsExternal { get; set; }
        public string? ProviderName { get; set; }
        public string? ExternalUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveTrainingCourseDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public Guid? TrainingCategoryId { get; set; }
        public string? Description { get; set; }
        public string? Objectives { get; set; }
        public string? TargetAudience { get; set; }
        public string? Prerequisites { get; set; }
        public decimal? DurationHours { get; set; }
        /// <summary>InPerson | Online | Hybrid (HC196).</summary>
        public string DeliveryMode { get; set; } = nameof(TrainingDeliveryMode.InPerson);
        public decimal CpdHours { get; set; }
        public bool IsExternal { get; set; }
        public string? ProviderName { get; set; }
        public string? ExternalUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveTrainingCourseDtoValidator : AbstractValidator<SaveTrainingCourseDto>
    {
        public SaveTrainingCourseDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).MaximumLength(50);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.Objectives).MaximumLength(2000);
            RuleFor(x => x.TargetAudience).MaximumLength(500);
            RuleFor(x => x.Prerequisites).MaximumLength(1000);
            RuleFor(x => x.DeliveryMode)
                .Must(m => Enum.TryParse<TrainingDeliveryMode>(m, true, out _))
                .WithMessage("Delivery mode must be InPerson, Online or Hybrid.");
            RuleFor(x => x.DurationHours).GreaterThanOrEqualTo(0).When(x => x.DurationHours.HasValue);
            RuleFor(x => x.CpdHours).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ProviderName).NotEmpty().When(x => x.IsExternal)
                .WithMessage("An external course needs its provider named.");
            RuleFor(x => x.ExternalUrl).MaximumLength(500);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveTrainingCourse { Task<Guid> SaveAsync(SaveTrainingCourseDto dto); }
    public interface IDeleteTrainingCourse { Task DeleteAsync(Guid id); }
    public interface IGetTrainingCourseById { Task<TrainingCourseDto> GetAsync(Guid id); }
    public interface IGetAllTrainingCourses { Task<PaginatedResponse<TrainingCourseDto>> GetAsync(GetAllRequest request); }

    internal static class TrainingCourseQueryShared
    {
        internal static IQueryable<TrainingCourseDto> Project(
            IQueryable<TrainingCourse> query, IQueryable<TrainingCategory> categories)
        {
            return query.Select(c => new TrainingCourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                TrainingCategoryId = c.TrainingCategoryId,
                CategoryName = categories.Where(k => k.Id == c.TrainingCategoryId).Select(k => k.Name).FirstOrDefault(),
                Description = c.Description,
                Objectives = c.Objectives,
                TargetAudience = c.TargetAudience,
                Prerequisites = c.Prerequisites,
                DurationHours = c.DurationHours,
                DeliveryMode = c.DeliveryMode.ToString(),
                CpdHours = c.CpdHours,
                IsExternal = c.IsExternal,
                ProviderName = c.ProviderName,
                ExternalUrl = c.ExternalUrl,
                IsActive = c.IsActive
            });
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveTrainingCourse(
        IRepository<TrainingCourse> repository,
        IRepository<TrainingCategory> categoryRepository,
        IValidator<SaveTrainingCourseDto> validator,
        ILogger<SaveTrainingCourse> logger) : ISaveTrainingCourse
    {
        public async Task<Guid> SaveAsync(SaveTrainingCourseDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(TrainingCourse), nameof(dto.Name), dto.Name);
            if (dto.TrainingCategoryId.HasValue &&
                !await categoryRepository.GetAll().AnyAsync(k => k.Id == dto.TrainingCategoryId.Value))
                throw new NotFoundException(nameof(TrainingCategory), dto.TrainingCategoryId.Value.ToString());

            var mode = Enum.Parse<TrainingDeliveryMode>(dto.DeliveryMode, true);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(TrainingCourse), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Code, dto.TrainingCategoryId, dto.Description, dto.Objectives,
                    dto.TargetAudience, dto.Prerequisites, dto.DurationHours, mode, dto.CpdHours,
                    dto.IsExternal, dto.ProviderName, dto.ExternalUrl, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated TrainingCourse {Id}", entity.Id);
                return entity.Id;
            }

            var created = TrainingCourse.Create(dto.Name, dto.Code, dto.TrainingCategoryId, dto.Description,
                dto.Objectives, dto.TargetAudience, dto.Prerequisites, dto.DurationHours, mode, dto.CpdHours,
                dto.IsExternal, dto.ProviderName, dto.ExternalUrl, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created TrainingCourse {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class DeleteTrainingCourse(
        IRepository<TrainingCourse> repository,
        IRepository<TrainingNeed> needRepository,
        ILogger<DeleteTrainingCourse> logger) : IDeleteTrainingCourse
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(TrainingCourse), id.ToString());
            if (await needRepository.GetAll().AnyAsync(n => n.TrainingCourseId == id))
                throw new ValidationException(nameof(id), "Cannot delete a course that training needs reference.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted TrainingCourse {Id}", id);
        }
    }

    public class GetTrainingCourseById(
        IRepository<TrainingCourse> repository,
        IRepository<TrainingCategory> categoryRepository) : IGetTrainingCourseById
    {
        public async Task<TrainingCourseDto> GetAsync(Guid id)
        {
            return await TrainingCourseQueryShared.Project(
                    repository.GetAll().AsNoTracking().Where(x => x.Id == id), categoryRepository.GetAll())
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(TrainingCourse), id.ToString());
        }
    }

    public class GetAllTrainingCourses(
        IRepository<TrainingCourse> repository,
        IRepository<TrainingCategory> categoryRepository) : IGetAllTrainingCourses
    {
        public async Task<PaginatedResponse<TrainingCourseDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || (x.Code != null && x.Code.Contains(term))
                    || (x.ProviderName != null && x.ProviderName.Contains(term)));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(x => x.IsActive == active);
            if (request.CategoryId.HasValue)
                query = query.Where(x => x.TrainingCategoryId == request.CategoryId.Value);

            var total = await query.CountAsync();
            var data = await TrainingCourseQueryShared.Project(
                    query.OrderBy(x => x.Name).Skip(skip).Take(take), categoryRepository.GetAll())
                .ToListAsync();

            return new PaginatedResponse<TrainingCourseDto> { Total = total, Data = data };
        }
    }
}
