using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>
    /// Single source of truth for "working days" (HC040): the span between two dates minus weekends
    /// and active holidays (fixed + recurring). Reused by leave requests, attendance and timesheets.
    /// Weekend is Saturday/Sunday for now; this becomes shift/policy-driven in the attendance phase.
    /// </summary>
    public interface IWorkingCalendar
    {
        Task<decimal> CountWorkingDaysAsync(DateTime startDate, DateTime endDate, bool halfDay = false);
        Task<bool> IsWorkingDayAsync(DateTime date);
        /// <summary>Dates that are non-working (holiday or weekend) within the inclusive range.</summary>
        Task<IReadOnlyList<DateTime>> GetNonWorkingDaysAsync(DateTime startDate, DateTime endDate);
    }

    public class WorkingCalendar(IRepository<Holiday> holidayRepository) : IWorkingCalendar
    {
        private static bool IsWeekend(DateTime d) =>
            d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday;

        public async Task<decimal> CountWorkingDaysAsync(DateTime startDate, DateTime endDate, bool halfDay = false)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            if (end < start)
                throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

            var (exact, recurring) = await LoadHolidaysAsync(start, end);

            int workingDays = 0;
            for (var day = start; day <= end; day = day.AddDays(1))
            {
                if (IsWeekend(day)) continue;
                if (exact.Contains(day)) continue;
                if (recurring.Contains((day.Month, day.Day))) continue;
                workingDays++;
            }

            if (halfDay)
            {
                // A half-day request must be a single working day.
                if (start != end)
                    throw new ArgumentException("Half-day leave must be a single day.");
                return workingDays == 0 ? 0m : 0.5m;
            }

            return workingDays;
        }

        public async Task<bool> IsWorkingDayAsync(DateTime date)
        {
            var d = date.Date;
            if (IsWeekend(d)) return false;
            var (exact, recurring) = await LoadHolidaysAsync(d, d);
            return !exact.Contains(d) && !recurring.Contains((d.Month, d.Day));
        }

        public async Task<IReadOnlyList<DateTime>> GetNonWorkingDaysAsync(DateTime startDate, DateTime endDate)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            var (exact, recurring) = await LoadHolidaysAsync(start, end);

            var result = new List<DateTime>();
            for (var day = start; day <= end; day = day.AddDays(1))
            {
                if (IsWeekend(day) || exact.Contains(day) || recurring.Contains((day.Month, day.Day)))
                    result.Add(day);
            }
            return result;
        }

        private async Task<(HashSet<DateTime> Exact, HashSet<(int Month, int Day)> Recurring)> LoadHolidaysAsync(
            DateTime start, DateTime end)
        {
            // Recurring holidays match on month/day every year, so they are always loaded; fixed
            // holidays are filtered to the requested window.
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
