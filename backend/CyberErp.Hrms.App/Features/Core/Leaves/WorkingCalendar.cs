using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>
    /// Single source of truth for "working days" (HC040): the chargeable value of a date span, honoring
    /// the client's <see cref="WorkWeekConfiguration"/> (Full=1 / Half=0.5 / Rest=0 per weekday) and active
    /// holidays (fixed + recurring). A half-work Saturday costs 0.5; a rest day or holiday costs 0. Reused
    /// by leave requests, attendance and timesheets. Absent a configuration, falls back to a Sat/Sun rest week.
    /// </summary>
    public interface IWorkingCalendar
    {
        Task<decimal> CountWorkingDaysAsync(DateTime startDate, DateTime endDate, bool halfDay = false);
        Task<bool> IsWorkingDayAsync(DateTime date);
        /// <summary>Dates that are non-working (holiday or rest day) within the inclusive range.</summary>
        Task<IReadOnlyList<DateTime>> GetNonWorkingDaysAsync(DateTime startDate, DateTime endDate);
    }

    public class WorkingCalendar(
        IRepository<Holiday> holidayRepository,
        IRepository<WorkWeekConfiguration> workWeeks) : IWorkingCalendar
    {
        public async Task<decimal> CountWorkingDaysAsync(DateTime startDate, DateTime endDate, bool halfDay = false)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            if (end < start)
                throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

            var config = await GetEffectiveConfigAsync();
            var (exact, recurring) = await LoadHolidaysAsync(start, end);

            if (halfDay)
            {
                // A half-day request must be a single day; charge half of that day's work value.
                if (start != end)
                    throw new ArgumentException("Half-day leave must be a single day.");
                var value = DayValue(start, config, exact, recurring);
                return value * 0.5m;
            }

            decimal total = 0m;
            for (var day = start; day <= end; day = day.AddDays(1))
                total += DayValue(day, config, exact, recurring);
            return total;
        }

        public async Task<bool> IsWorkingDayAsync(DateTime date)
        {
            var d = date.Date;
            var config = await GetEffectiveConfigAsync();
            var (exact, recurring) = await LoadHolidaysAsync(d, d);
            return DayValue(d, config, exact, recurring) > 0m;
        }

        public async Task<IReadOnlyList<DateTime>> GetNonWorkingDaysAsync(DateTime startDate, DateTime endDate)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            var config = await GetEffectiveConfigAsync();
            var (exact, recurring) = await LoadHolidaysAsync(start, end);

            var result = new List<DateTime>();
            for (var day = start; day <= end; day = day.AddDays(1))
                if (DayValue(day, config, exact, recurring) == 0m)
                    result.Add(day);
            return result;
        }

        private static decimal DayValue(DateTime day, WorkWeekConfiguration config,
            HashSet<DateTime> exact, HashSet<(int Month, int Day)> recurring)
        {
            if (exact.Contains(day) || recurring.Contains((day.Month, day.Day))) return 0m; // holiday
            return config.WorkValueFor(day.DayOfWeek);
        }

        private async Task<WorkWeekConfiguration> GetEffectiveConfigAsync()
        {
            return await workWeeks.GetAll().AsNoTracking().FirstOrDefaultAsync(w => w.IsActive)
                // Fallback: a standard Mon–Fri work week with Saturday & Sunday as rest days.
                ?? WorkWeekConfiguration.Create("Default", WorkDayType.Full, WorkDayType.Full, WorkDayType.Full,
                    WorkDayType.Full, WorkDayType.Full, WorkDayType.Rest, WorkDayType.Rest);
        }

        private async Task<(HashSet<DateTime> Exact, HashSet<(int Month, int Day)> Recurring)> LoadHolidaysAsync(
            DateTime start, DateTime end)
        {
            var holidays = await holidayRepository.GetAll()
                .Where(h => h.IsActive && (h.IsRecurring || (h.Date >= start && h.Date <= end)))
                .Select(h => new { h.Date, h.IsRecurring })
                .ToListAsync();

            var exact = holidays.Where(h => !h.IsRecurring).Select(h => h.Date.Date).ToHashSet();
            var recurring = holidays.Where(h => h.IsRecurring).Select(h => (h.Date.Month, h.Date.Day)).ToHashSet();
            return (exact, recurring);
        }
    }
}
