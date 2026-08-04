using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Search.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Search.Providers
{
    /// <summary>Global search over the employee master: by number, e-mail, and the person's names
    /// (including the concatenated full name, so "abebe kebede" matches across name columns).</summary>
    public class EmployeeSearchProvider(IRepository<Employee> employees) : ISearchProvider
    {
        public string Category => "Employees";
        public string Icon => "users";
        public int Order => 1;

        public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(string term, int take, CancellationToken ct)
        {
            // Repository.GetAll() already applies tenant + branch isolation. Projected + AsNoTracking +
            // Take(take): only the columns we render for the capped result set leave the database.
            var rows = await employees.GetAll().AsNoTracking()
                .Where(e =>
                    e.EmployeeNumber.Contains(term) ||
                    (e.Email != null && e.Email.Contains(term)) ||
                    (e.Person != null && (
                        e.Person.FirstName.Contains(term) ||
                        (e.Person.FatherName != null && e.Person.FatherName.Contains(term)) ||
                        e.Person.GrandFatherName.Contains(term) ||
                        (e.Person.FirstName + " " + e.Person.FatherName + " " + e.Person.GrandFatherName).Contains(term))))
                .OrderBy(e => e.EmployeeNumber)
                .Take(take)
                .Select(e => new
                {
                    e.Id,
                    e.EmployeeNumber,
                    First = e.Person != null ? e.Person.FirstName : "",
                    Grand = e.Person != null ? e.Person.GrandFatherName : "",
                    Unit = e.Position != null && e.Position.OrganizationUnit != null ? e.Position.OrganizationUnit.Name : null
                })
                .ToListAsync(ct);

            return rows.Select(e =>
            {
                var name = $"{e.First} {e.Grand}".Trim();
                if (name.Length == 0) name = e.EmployeeNumber;
                var subtitle = e.Unit is null ? e.EmployeeNumber : $"{e.EmployeeNumber} · {e.Unit}";
                return new SearchResultItem(e.Id, name, subtitle, "/employee");
            }).ToList();
        }
    }
}
