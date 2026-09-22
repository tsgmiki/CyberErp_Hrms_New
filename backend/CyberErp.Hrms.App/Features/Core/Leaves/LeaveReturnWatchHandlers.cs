using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>One employee whose approved leave has ended without a confirmed return.</summary>
    public class OverdueLeaveReturnDto
    {
        public Guid RequestId { get; set; }
        /// <summary>"AnnualLeave" or "OtherLeave" — which module the row came from.</summary>
        public string Source { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string? PositionTitle { get; set; }
        /// <summary>"Annual Leave", or the other-leave type's name.</summary>
        public string LeaveName { get; set; } = string.Empty;
        /// <summary>The last approved day — the day they were due back after.</summary>
        public DateTime PlannedEndDate { get; set; }
        /// <summary>Whole days since the leave ended. Always ≥ 1 on this list.</summary>
        public int DaysOverdue { get; set; }
        public decimal TotalLeaveDays { get; set; }

        /// <summary>
        /// Whether this row can be CLEARED by confirming a return.
        /// </summary>
        /// <remarks>
        /// ⚠️ TRUE ONLY FOR ANNUAL LEAVE. Annual leave has a return step — confirming moves the
        /// request to ReturnPending or Closed and the row leaves this list. Other Leave has no such
        /// step at all: its statuses are Pending/Approved/Rejected/Cancelled and nothing records a
        /// return, so those rows report "this leave has ended" and cannot be acknowledged away. The
        /// flag exists so the dashboard can say which is which rather than inviting HR to chase an
        /// action that does not exist (logic §12.106).
        /// </remarks>
        public bool HasReturnConfirmation { get; set; }
    }

    public interface IGetOverdueLeaveReturns { Task<List<OverdueLeaveReturnDto>> GetAsync(); }

    /// <summary>
    /// Employees whose approved leave has ended and who have not been recorded as back.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ THE TWO MODULES DEFINE "RETURNED" DIFFERENTLY, so the query does too. Annual leave is
    /// still <c>Approved</c> until somebody confirms the return, at which point it becomes
    /// <c>ReturnPending</c> or <c>Closed</c> — so "approved and the last day has passed" is exactly
    /// the unreturned set. Other Leave has no return step, so the same shape of query says only that
    /// the leave has ended; see <see cref="OverdueLeaveReturnDto.HasReturnConfirmation"/>.</para>
    ///
    /// <para>⚠️ Scoped by JOINING the employee repository rather than walking the header's navigation
    /// property. <c>IRepository.GetAll()</c> applies the tenant and branch filters to the table it is
    /// called on; a navigation from an unfiltered header would reach employees outside the caller's
    /// branch. The join makes the employee table — and therefore its filters — part of the query.</para>
    /// </remarks>
    public class GetOverdueLeaveReturns(
        IRepository<AnnualLeaveHeader> annualLeave,
        IRepository<OtherLeaveHeader> otherLeave,
        IRepository<Employee> employees) : IGetOverdueLeaveReturns
    {
        /// <summary>Dashboard widget — never return an unbounded list on a large tenant.</summary>
        private const int MaxRows = 100;

        public async Task<List<OverdueLeaveReturnDto>> GetAsync()
        {
            var today = DateTime.Today;

            // "Every day of this request is in the past." Expressed as "no line ends today or later"
            // rather than Max(EndDate) < today: both are correct, but this form is a plain EXISTS and
            // stays translatable without relying on an aggregate inside a predicate.
            var annualRows = await (
                from h in annualLeave.GetAll()
                join e in employees.GetAll() on h.EmployeeId equals e.Id
                where h.Status == AnnualLeaveStatus.Approved
                      && h.Details.Any()
                      && !h.Details.Any(d => d.EndDate >= today)
                select new OverdueLeaveReturnDto
                {
                    RequestId = h.Id,
                    Source = "AnnualLeave",
                    EmployeeId = e.Id,
                    FullName = e.Person != null ? (e.Person.FirstName + " " + e.Person.GrandFatherName) : string.Empty,
                    EmployeeNumber = e.EmployeeNumber,
                    PositionTitle = e.Position != null && e.Position.PositionClass != null ? e.Position.PositionClass.Title : null,
                    LeaveName = AnnualLeave.DisplayName,
                    PlannedEndDate = h.Details.Max(d => d.EndDate),
                    TotalLeaveDays = h.TotalLeaveDays,
                    HasReturnConfirmation = true
                })
                .Take(MaxRows)
                .ToListAsync();

            var otherRows = await (
                from h in otherLeave.GetAll()
                join e in employees.GetAll() on h.EmployeeId equals e.Id
                where h.Status == OtherLeaveStatus.Approved
                      && h.Details.Any()
                      && !h.Details.Any(d => d.EndDate >= today)
                select new OverdueLeaveReturnDto
                {
                    RequestId = h.Id,
                    Source = "OtherLeave",
                    EmployeeId = e.Id,
                    FullName = e.Person != null ? (e.Person.FirstName + " " + e.Person.GrandFatherName) : string.Empty,
                    EmployeeNumber = e.EmployeeNumber,
                    PositionTitle = e.Position != null && e.Position.PositionClass != null ? e.Position.PositionClass.Title : null,
                    LeaveName = h.Setting != null && h.Setting.LeaveType != null ? h.Setting.LeaveType.Name : "Other Leave",
                    PlannedEndDate = h.Details.Max(d => d.EndDate),
                    TotalLeaveDays = h.TotalLeaveDays,
                    HasReturnConfirmation = false
                })
                .Take(MaxRows)
                .ToListAsync();

            // Longest overdue first: the person missing for three weeks matters more than yesterday's.
            var rows = annualRows.Concat(otherRows).ToList();
            foreach (var r in rows)
            {
                r.FullName = r.FullName.Trim();
                r.DaysOverdue = (int)(today - r.PlannedEndDate.Date).TotalDays;
            }

            return [.. rows.OrderByDescending(r => r.DaysOverdue).ThenBy(r => r.FullName).Take(MaxRows)];
        }
    }
}
