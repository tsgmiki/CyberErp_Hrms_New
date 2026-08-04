using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Search.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Search.Providers
{
    /// <summary>Global search over annual-leave requests, found by the requesting employee's name or
    /// number (the natural way a user looks a leave request up). Shows status + total days as context.</summary>
    public class LeaveRequestSearchProvider(IRepository<AnnualLeaveHeader> leaves) : ISearchProvider
    {
        public string Category => "Leave Requests";
        public string Icon => "calendar-check";
        public int Order => 3;

        public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(string term, int take, CancellationToken ct)
        {
            var rows = await leaves.GetAll().AsNoTracking()
                .Where(h => h.Employee != null && h.Employee.Person != null && (
                    h.Employee.EmployeeNumber.Contains(term) ||
                    h.Employee.Person.FirstName.Contains(term) ||
                    h.Employee.Person.GrandFatherName.Contains(term) ||
                    (h.Employee.Person.FirstName + " " + h.Employee.Person.GrandFatherName).Contains(term)))
                .OrderByDescending(h => h.RequestDate)
                .Take(take)
                .Select(h => new
                {
                    h.Id,
                    First = h.Employee!.Person!.FirstName,
                    Grand = h.Employee.Person.GrandFatherName,
                    h.Employee.EmployeeNumber,
                    h.Status,
                    h.TotalLeaveDays
                })
                .ToListAsync(ct);

            return rows.Select(h => new SearchResultItem(
                h.Id,
                $"Annual Leave — {$"{h.First} {h.Grand}".Trim()}",
                $"{h.EmployeeNumber} · {h.Status} · {h.TotalLeaveDays:0.#}d",
                "/annualLeave")).ToList();
        }
    }
}
