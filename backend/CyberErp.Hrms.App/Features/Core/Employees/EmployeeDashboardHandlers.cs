using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class ProbationEmployeeDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string? PositionTitle { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        /// <summary>Days until probation ends (negative = overdue). Null when no end date.</summary>
        public int? DaysRemaining { get; set; }
    }

    public class RetirementEmployeeDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public DateTime RetirementDate { get; set; }
        /// <summary>Days until retirement (negative = already due).</summary>
        public int DaysRemaining { get; set; }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetEmployeesOnProbation { Task<List<ProbationEmployeeDto>> GetAsync(); }
    public interface IGetUpcomingRetirements { Task<List<RetirementEmployeeDto>> GetAsync(); }

    // ---- Employees on probation ---------------------------------------------
    public class GetEmployeesOnProbation(IRepository<Employee> repository) : IGetEmployeesOnProbation
    {
        public async Task<List<ProbationEmployeeDto>> GetAsync()
        {
            var today = DateTime.Today;

            // Active + on probation. Supported by the (EmploymentStatus, IsProbation) index; projection
            // pulls only the columns the widget needs. Tenant/branch scoping is applied by GetAll().
            var rows = await repository.GetAll()
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active && e.IsProbation)
                .OrderBy(e => e.ProbationEndDate)
                .Select(e => new ProbationEmployeeDto
                {
                    Id = e.Id,
                    FullName = e.Person != null ? (e.Person.FirstName + " " + e.Person.GrandFatherName) : string.Empty,
                    EmployeeNumber = e.EmployeeNumber,
                    PositionTitle = e.Position != null && e.Position.PositionClass != null ? e.Position.PositionClass.Title : null,
                    HireDate = e.HireDate,
                    ProbationEndDate = e.ProbationEndDate
                })
                .Take(100)   // dashboard widget — never return an unbounded list on a large tenant
                .ToListAsync();

            foreach (var r in rows)
            {
                r.FullName = r.FullName.Trim();
                r.DaysRemaining = r.ProbationEndDate.HasValue ? (int)(r.ProbationEndDate.Value.Date - today).TotalDays : null;
            }
            return rows;
        }
    }

    // ---- Upcoming retirements -----------------------------------------------
    public class GetUpcomingRetirements(IRepository<Employee> repository) : IGetUpcomingRetirements
    {
        /// <summary>Statutory retirement age (Ethiopian public service). Kept as a constant here; can be lifted to config.</summary>
        private const int RetirementAgeYears = 60;

        public async Task<List<RetirementEmployeeDto>> GetAsync()
        {
            var today = DateTime.Today;
            // Retirement date = DateOfBirth + 60y. "Retires within a month" <=> RetirementDate < today+1mo
            // <=> DateOfBirth < today+1mo-60y. The threshold is a constant, so the filter stays SARGABLE
            // (plain range scan on the DateOfBirth index) with no per-row date function.
            var threshold = today.AddMonths(1).AddYears(-RetirementAgeYears);

            var rows = await repository.GetAll()
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active
                            && e.DateOfBirth != null && e.DateOfBirth < threshold)
                .OrderBy(e => e.DateOfBirth)
                .Select(e => new
                {
                    e.Id,
                    Name = e.Person != null ? (e.Person.FirstName + " " + e.Person.GrandFatherName) : string.Empty,
                    e.EmployeeNumber,
                    e.DateOfBirth
                })
                .Take(100)   // dashboard widget — never return an unbounded list on a large tenant
                .ToListAsync();

            return rows.Select(r =>
            {
                var retirementDate = r.DateOfBirth!.Value.Date.AddYears(RetirementAgeYears);
                return new RetirementEmployeeDto
                {
                    Id = r.Id,
                    FullName = r.Name.Trim(),
                    EmployeeNumber = r.EmployeeNumber,
                    DateOfBirth = r.DateOfBirth,
                    RetirementDate = retirementDate,
                    DaysRemaining = (int)(retirementDate - today).TotalDays
                };
            }).ToList();
        }
    }
}
