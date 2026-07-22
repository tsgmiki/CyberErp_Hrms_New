using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    public class EmployeeOptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
    }

    /// <summary>Role-scoped employee picker payload: the caller's scope drives the form behavior
    /// ("Self" → auto-filled + locked field) and the server never returns out-of-scope employees.</summary>
    public class EmployeeOptionsDto
    {
        /// <summary>"All" (HR admin) | "Unit" (manager: own unit + child units) | "Self".</summary>
        public string Scope { get; set; } = "Self";
        public EmployeeOptionDto? Self { get; set; }
        public List<EmployeeOptionDto> Options { get; set; } = [];
    }

    public interface IGetEmployeeOptions
    {
        /// <param name="exclude">Optional employee to omit (e.g. the appraisee must not appear as a peer-reviewer option).</param>
        Task<EmployeeOptionsDto> GetAsync(string? search, Guid? exclude = null);
    }

    /// <summary>
    /// Scoped, searchable employee options for performance-module comboboxes. PERFORMANCE: one indexed
    /// query projecting 4 columns, TOP 20, scope applied as a SQL predicate — never loads the employee table.
    /// </summary>
    public class GetEmployeeOptions(
        IPerformanceVisibilityService visibility,
        IRepository<Employee> employees) : IGetEmployeeOptions
    {
        private const int MaxOptions = 20;

        public async Task<EmployeeOptionsDto> GetAsync(string? search, Guid? exclude = null)
        {
            var scope = await visibility.GetScopeAsync();
            var myEmp = scope.EmployeeId ?? Guid.Empty;

            var query = employees.GetAll()
                .Where(e => !e.IsTerminated && e.EmploymentStatus != EmploymentStatus.Terminated);
            if (exclude.HasValue)
                query = query.Where(e => e.Id != exclude.Value);
            if (!scope.IsAdmin)
            {
                if (scope.IsManager)
                {
                    var unitIds = scope.UnitIds;
                    query = query.Where(e => e.Id == myEmp || (e.Position != null && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(e => e.Id == myEmp);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(e => e.EmployeeNumber.Contains(term) ||
                    (e.Person != null && (
                        e.Person.FirstName.Contains(term) ||
                        (e.Person.FatherName != null && e.Person.FatherName.Contains(term)) ||
                        e.Person.GrandFatherName.Contains(term))));
            }

            var rows = await query.OrderBy(e => e.EmployeeNumber).Take(MaxOptions)
                .Select(e => new
                {
                    e.Id,
                    e.EmployeeNumber,
                    First = e.Person != null ? e.Person.FirstName : "",
                    Father = e.Person != null ? e.Person.FatherName : null,
                    Grand = e.Person != null ? e.Person.GrandFatherName : "",
                })
                .ToListAsync();
            var options = rows.Select(r => new EmployeeOptionDto
            {
                Id = r.Id,
                EmployeeNumber = r.EmployeeNumber,
                Name = string.Join(" ", new[] { r.First, r.Father, r.Grand }.Where(p => !string.IsNullOrWhiteSpace(p))),
            }).ToList();

            // The caller's own row (drives the locked Self field even when it doesn't match the search).
            EmployeeOptionDto? self = null;
            if (scope.EmployeeId.HasValue)
            {
                self = options.FirstOrDefault(o => o.Id == myEmp);
                if (self is null)
                {
                    var me = await employees.GetAll().Where(e => e.Id == myEmp)
                        .Select(e => new
                        {
                            e.Id,
                            e.EmployeeNumber,
                            First = e.Person != null ? e.Person.FirstName : "",
                            Father = e.Person != null ? e.Person.FatherName : null,
                            Grand = e.Person != null ? e.Person.GrandFatherName : "",
                        })
                        .FirstOrDefaultAsync();
                    if (me is not null)
                        self = new EmployeeOptionDto
                        {
                            Id = me.Id,
                            EmployeeNumber = me.EmployeeNumber,
                            Name = string.Join(" ", new[] { me.First, me.Father, me.Grand }.Where(p => !string.IsNullOrWhiteSpace(p))),
                        };
                }
            }

            return new EmployeeOptionsDto
            {
                Scope = scope.IsAdmin ? "All" : scope.IsManager ? "Unit" : "Self",
                Self = self,
                Options = options,
            };
        }
    }
}
