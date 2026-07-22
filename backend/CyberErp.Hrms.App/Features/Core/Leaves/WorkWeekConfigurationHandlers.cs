using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    // ---- DTOs ---------------------------------------------------------------
    public class WorkWeekConfigurationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Monday { get; set; } = string.Empty;
        public string Tuesday { get; set; } = string.Empty;
        public string Wednesday { get; set; } = string.Empty;
        public string Thursday { get; set; } = string.Empty;
        public string Friday { get; set; } = string.Empty;
        public string Saturday { get; set; } = string.Empty;
        public string Sunday { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SaveWorkWeekConfigurationDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Monday { get; set; } = nameof(WorkDayType.Full);
        public string Tuesday { get; set; } = nameof(WorkDayType.Full);
        public string Wednesday { get; set; } = nameof(WorkDayType.Full);
        public string Thursday { get; set; } = nameof(WorkDayType.Full);
        public string Friday { get; set; } = nameof(WorkDayType.Full);
        public string Saturday { get; set; } = nameof(WorkDayType.Rest);
        public string Sunday { get; set; } = nameof(WorkDayType.Rest);
        public bool IsActive { get; set; } = true;
    }

    public class SaveWorkWeekConfigurationDtoValidator : AbstractValidator<SaveWorkWeekConfigurationDto>
    {
        public SaveWorkWeekConfigurationDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            static bool Valid(string v) => Enum.TryParse<WorkDayType>(v, out _);
            const string msg = "Each day must be Full, Half or Rest.";
            RuleFor(x => x.Monday).Must(Valid).WithMessage(msg);
            RuleFor(x => x.Tuesday).Must(Valid).WithMessage(msg);
            RuleFor(x => x.Wednesday).Must(Valid).WithMessage(msg);
            RuleFor(x => x.Thursday).Must(Valid).WithMessage(msg);
            RuleFor(x => x.Friday).Must(Valid).WithMessage(msg);
            RuleFor(x => x.Saturday).Must(Valid).WithMessage(msg);
            RuleFor(x => x.Sunday).Must(Valid).WithMessage(msg);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveWorkWeekConfiguration { Task<Guid> SaveAsync(SaveWorkWeekConfigurationDto dto); }
    public interface IDeleteWorkWeekConfiguration { Task DeleteAsync(Guid id); }
    public interface IGetWorkWeekConfigurationById { Task<WorkWeekConfigurationDto> GetAsync(Guid id); }
    public interface IGetAllWorkWeekConfigurations { Task<PaginatedResponse<WorkWeekConfigurationDto>> GetAsync(GetAllRequest request); }

    internal static class WorkWeekConfigurationMapper
    {
        public static readonly System.Linq.Expressions.Expression<Func<WorkWeekConfiguration, WorkWeekConfigurationDto>> Projection = w => new WorkWeekConfigurationDto
        {
            Id = w.Id,
            Name = w.Name,
            Monday = w.Monday.ToString(),
            Tuesday = w.Tuesday.ToString(),
            Wednesday = w.Wednesday.ToString(),
            Thursday = w.Thursday.ToString(),
            Friday = w.Friday.ToString(),
            Saturday = w.Saturday.ToString(),
            Sunday = w.Sunday.ToString(),
            IsActive = w.IsActive
        };
    }

    // ---- Handlers -----------------------------------------------------------
    public class SaveWorkWeekConfiguration(
        IRepository<WorkWeekConfiguration> repository,
        IValidator<SaveWorkWeekConfigurationDto> validator,
        ILogger<SaveWorkWeekConfiguration> logger) : ISaveWorkWeekConfiguration
    {
        public async Task<Guid> SaveAsync(SaveWorkWeekConfigurationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            WorkDayType D(string v) => Enum.Parse<WorkDayType>(v);

            Guid id;
            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(WorkWeekConfiguration), dto.Id.Value.ToString());
                entity.Update(dto.Name, D(dto.Monday), D(dto.Tuesday), D(dto.Wednesday), D(dto.Thursday),
                    D(dto.Friday), D(dto.Saturday), D(dto.Sunday), dto.IsActive);
                repository.UpdateAsync(entity);
                id = entity.Id;
            }
            else
            {
                var created = WorkWeekConfiguration.Create(dto.Name, D(dto.Monday), D(dto.Tuesday), D(dto.Wednesday),
                    D(dto.Thursday), D(dto.Friday), D(dto.Saturday), D(dto.Sunday), dto.IsActive);
                await repository.AddAsync(created);
                id = created.Id;
            }

            // Exactly one active work-week per tenant — deactivate any others when this one is active.
            if (dto.IsActive)
            {
                var others = await repository.GetAll().Where(x => x.IsActive && x.Id != id).ToListAsync();
                foreach (var other in others)
                    other.Update(other.Name, other.Monday, other.Tuesday, other.Wednesday, other.Thursday,
                        other.Friday, other.Saturday, other.Sunday, false);
            }

            await repository.SaveChangesAsync();
            logger.LogInformation("Saved WorkWeekConfiguration {Id}", id);
            return id;
        }
    }

    public class DeleteWorkWeekConfiguration(
        IRepository<WorkWeekConfiguration> repository,
        ILogger<DeleteWorkWeekConfiguration> logger) : IDeleteWorkWeekConfiguration
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(WorkWeekConfiguration), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted WorkWeekConfiguration {Id}", id);
        }
    }

    public class GetWorkWeekConfigurationById(IRepository<WorkWeekConfiguration> repository) : IGetWorkWeekConfigurationById
    {
        public async Task<WorkWeekConfigurationDto> GetAsync(Guid id) =>
            await repository.GetAll().Where(x => x.Id == id).Select(WorkWeekConfigurationMapper.Projection).FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(WorkWeekConfiguration), id.ToString());
    }

    public class GetAllWorkWeekConfigurations(IRepository<WorkWeekConfiguration> repository) : IGetAllWorkWeekConfigurations
    {
        public async Task<PaginatedResponse<WorkWeekConfigurationDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(x => x.Name.Contains(request.SearchText.Trim()));

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Name)
                .Skip(skip).Take(take).Select(WorkWeekConfigurationMapper.Projection).ToListAsync();
            return new PaginatedResponse<WorkWeekConfigurationDto> { Total = total, Data = data };
        }
    }
}
