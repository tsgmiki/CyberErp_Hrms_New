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
    public class HolidayDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameA { get; set; }
        public string HolidayType { get; set; } = nameof(Dom.Entities.Core.HolidayType.Public);
        public bool IsRecurring { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveHolidayDto
    {
        public Guid? Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameA { get; set; }
        public HolidayType HolidayType { get; set; } = HolidayType.Public;
        public bool IsRecurring { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SaveHolidayDtoValidator : AbstractValidator<SaveHolidayDto>
    {
        public SaveHolidayDtoValidator()
        {
            RuleFor(x => x.Date).NotEmpty().WithMessage("Holiday date is required.");
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.NameA).MaximumLength(200);
        }
    }

    public class WorkingDaysDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal WorkingDays { get; set; }
        public int CalendarDays { get; set; }
        public List<DateTime> NonWorkingDays { get; set; } = [];
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveHoliday { Task<Guid> SaveAsync(SaveHolidayDto dto); }
    public interface IGetHolidayById { Task<HolidayDto> GetAsync(Guid id); }
    public interface IGetAllHolidays { Task<PaginatedResponse<HolidayDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteHoliday { Task DeleteAsync(Guid id); }
    public interface IGetWorkingDays { Task<WorkingDaysDto> GetAsync(DateTime startDate, DateTime endDate, bool halfDay); }

    // ---- Save (create/update) ----------------------------------------------
    public class SaveHoliday(
        IRepository<Holiday> repository,
        IValidator<SaveHolidayDto> validator,
        ILogger<SaveHoliday> logger) : ISaveHoliday
    {
        public async Task<Guid> SaveAsync(SaveHolidayDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var date = dto.Date.Date;
            if (await repository.GetAll().AnyAsync(x => x.Date == date && x.Id != dto.Id))
                throw new DuplicateException(nameof(Holiday), nameof(dto.Date), date.ToString("yyyy-MM-dd"));

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Holiday), dto.Id.Value.ToString());
                entity.Update(dto.Date, dto.Name, dto.NameA, dto.HolidayType, dto.IsRecurring, dto.Description, dto.IsActive);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = Holiday.Create(dto.Date, dto.Name, dto.NameA, dto.HolidayType, dto.IsRecurring, dto.Description, dto.IsActive);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Holiday {Id} ({Date})", created.Id, created.Date);
            return created.Id;
        }
    }

    // ---- Get by id ----------------------------------------------------------
    public class GetHolidayById(IRepository<Holiday> repository) : IGetHolidayById
    {
        public async Task<HolidayDto> GetAsync(Guid id)
        {
            var e = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Holiday), id.ToString());
            return HolidayMapper.Projection.Compile()(e);
        }
    }

    // ---- Get all (paged) ----------------------------------------------------
    public class GetAllHolidays(IRepository<Holiday> repository) : IGetAllHolidays
    {
        public async Task<PaginatedResponse<HolidayDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Date)
                .Skip(skip).Take(take)
                .Select(HolidayMapper.Projection)
                .ToListAsync();

            return new PaginatedResponse<HolidayDto> { Total = total, Data = data };
        }
    }

    // ---- Delete -------------------------------------------------------------
    public class DeleteHoliday(
        IRepository<Holiday> repository,
        ILogger<DeleteHoliday> logger) : IDeleteHoliday
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Holiday), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Holiday {Id}", id);
        }
    }

    // ---- Working-days computation (HC040) -----------------------------------
    public class GetWorkingDays(IWorkingCalendar calendar) : IGetWorkingDays
    {
        public async Task<WorkingDaysDto> GetAsync(DateTime startDate, DateTime endDate, bool halfDay)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            if (end < start)
                throw new ValidationException(nameof(endDate), "End date cannot be before start date.");

            var working = await calendar.CountWorkingDaysAsync(start, end, halfDay);
            var nonWorking = await calendar.GetNonWorkingDaysAsync(start, end);
            return new WorkingDaysDto
            {
                StartDate = start,
                EndDate = end,
                WorkingDays = working,
                CalendarDays = (end - start).Days + 1,
                NonWorkingDays = nonWorking.ToList()
            };
        }
    }

    internal static class HolidayMapper
    {
        public static readonly System.Linq.Expressions.Expression<Func<Holiday, HolidayDto>> Projection = e => new HolidayDto
        {
            Id = e.Id,
            Date = e.Date,
            Name = e.Name,
            NameA = e.NameA,
            HolidayType = e.HolidayType.ToString(),
            IsRecurring = e.IsRecurring,
            Description = e.Description,
            IsActive = e.IsActive
        };
    }
}
