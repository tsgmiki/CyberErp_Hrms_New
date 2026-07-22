using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    // ---- DTOs ---------------------------------------------------------------
    public class AchievementDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime AchievementDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public Guid? AppraisalId { get; set; }
    }

    public class SaveAchievementDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime AchievementDate { get; set; }
        public string Category { get; set; } = nameof(AchievementCategory.Milestone);
        public Guid? AppraisalId { get; set; }
    }

    public class SaveAchievementDtoValidator : AbstractValidator<SaveAchievementDto>
    {
        public SaveAchievementDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.AchievementDate).NotEmpty();
            RuleFor(x => x.Category).NotEmpty()
                .Must(v => Enum.TryParse<AchievementCategory>(v, out _))
                .WithMessage("Category must be Milestone, Award, Project, Certification or Other.");
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveAchievement { Task<Guid> SaveAsync(SaveAchievementDto dto); }
    public interface IDeleteAchievement { Task DeleteAsync(Guid id); }
    public interface IGetAchievementById { Task<AchievementDto> GetAsync(Guid id); }
    public interface IGetAllAchievements { Task<PaginatedResponse<AchievementDto>> GetAsync(GetAllRequest request); }

    internal static class AchievementMapper
    {
        internal static AchievementDto Map(Achievement x, string? employeeName) => new()
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            EmployeeName = employeeName,
            Title = x.Title,
            Description = x.Description,
            AchievementDate = x.AchievementDate,
            Category = x.Category.ToString(),
            AppraisalId = x.AppraisalId
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveAchievement(
        IRepository<Achievement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Appraisal> appraisalRepository,
        IPerformanceHistoryWriter history,
        IValidator<SaveAchievementDto> validator,
        ILogger<SaveAchievement> logger) : ISaveAchievement
    {
        public async Task<Guid> SaveAsync(SaveAchievementDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == dto.EmployeeId))
                throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());
            if (dto.AppraisalId.HasValue && !await appraisalRepository.GetAll().AnyAsync(a => a.Id == dto.AppraisalId.Value))
                throw new NotFoundException(nameof(Appraisal), dto.AppraisalId.Value.ToString());

            var category = Enum.Parse<AchievementCategory>(dto.Category);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Achievement), dto.Id.Value.ToString());
                entity.Update(dto.EmployeeId, dto.Title, dto.AchievementDate, category, dto.Description, dto.AppraisalId);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated Achievement {Id}", entity.Id);
                return entity.Id;
            }

            var created = Achievement.Create(dto.EmployeeId, dto.Title, dto.AchievementDate, category, dto.Description, dto.AppraisalId);
            await repository.AddAsync(created);
            await history.WriteAsync("Achievement", created.Id, "Recorded", $"Achievement '{dto.Title}' recorded.");
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Achievement {Id} ({Title})", created.Id, created.Title);
            return created.Id;
        }
    }

    public class DeleteAchievement(
        IRepository<Achievement> repository,
        ILogger<DeleteAchievement> logger) : IDeleteAchievement
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Achievement), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Achievement {Id}", id);
        }
    }

    public class GetAchievementById(
        IRepository<Achievement> repository,
        IRepository<Employee> employeeRepository) : IGetAchievementById
    {
        public async Task<AchievementDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(Achievement), id.ToString());
            var employeeName = await employeeRepository.GetAll().Where(e => e.Id == entity.EmployeeId)
                .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync();
            return AchievementMapper.Map(entity, employeeName);
        }
    }

    public class GetAllAchievements(
        IRepository<Achievement> repository,
        IRepository<Employee> employeeRepository) : IGetAllAchievements
    {
        public async Task<PaginatedResponse<AchievementDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Title.Contains(request.SearchText.Trim()));
            if (request.EmployeeId.HasValue)
                query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);

            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.AchievementDate).Skip(skip).Take(take).ToListAsync();

            // PERFORMANCE: batch-load the employee names for the page in ONE query (was one per row).
            var empIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
            var employeeNames = await employeeRepository.GetAll().AsNoTracking()
                .Where(e => empIds.Contains(e.Id))
                .Select(e => new { e.Id, Name = e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "" })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var data = new List<AchievementDto>(rows.Count);
            foreach (var r in rows)
            {
                data.Add(AchievementMapper.Map(r, employeeNames.GetValueOrDefault(r.EmployeeId)));
            }
            return new PaginatedResponse<AchievementDto> { Total = total, Data = data };
        }
    }
}
