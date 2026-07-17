using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    // ---- DTOs ---------------------------------------------------------------
    public class AnnualLeaveLedgerRowDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal ServiceYears { get; set; }
        public bool IsManagerial { get; set; }
        /// <summary>Owning organization unit (via the employee's position) — the ledger groups on this.</summary>
        public string? OrganizationUnitName { get; set; }

        /// <summary>Entitlement the policy computes from service length (preview, before persisting).</summary>
        public decimal CalculatedEntitlement { get; set; }
        /// <summary>True once a balance row has been generated for this employee under the setting.</summary>
        public bool IsGenerated { get; set; }

        // Persisted ledger figures (zero until generated).
        public decimal Entitled { get; set; }
        public decimal CarriedForward { get; set; }
        public decimal Adjusted { get; set; }
        public decimal Taken { get; set; }
        public decimal Available { get; set; }
    }

    public class AnnualLeaveLedgerDto
    {
        public Guid SettingId { get; set; }
        public Guid FiscalYearId { get; set; }
        public string? FiscalYearName { get; set; }
        public DateTime FiscalYearStart { get; set; }
        public DateTime FiscalYearEnd { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public bool FiscalYearClosed { get; set; }
        public int TotalEmployees { get; set; }
        public int GeneratedCount { get; set; }
        public List<AnnualLeaveLedgerRowDto> Rows { get; set; } = [];
    }

    // ---- Interface ----------------------------------------------------------
    public interface IGetAnnualLeaveLedger { Task<AnnualLeaveLedgerDto> GetAsync(Guid settingId); }

    // ---- Handler ------------------------------------------------------------
    public class GetAnnualLeaveLedger(
        IRepository<AnnualLeaveSetting> settings,
        IRepository<Employee> employees,
        IRepository<EmployeeExperience> experiences,
        IRepository<FiscalYear> fiscalYears,
        IRepository<LeaveBalance> balances,
        IRepository<Position> positions,
        IRepository<OrganizationUnit> organizationUnits,
        ILeaveAccrualService accrualService) : IGetAnnualLeaveLedger
    {
        public async Task<AnnualLeaveLedgerDto> GetAsync(Guid settingId)
        {
            var setting = await settings.GetAll()
                .Include(s => s.FiscalYear)
                .Include(s => s.LeaveType)
                .FirstOrDefaultAsync(s => s.Id == settingId)
                ?? throw new NotFoundException(nameof(AnnualLeaveSetting), settingId.ToString());
            var fy = setting.FiscalYear
                ?? throw new ValidationException("id", "The setting's fiscal year could not be loaded.");

            var fyStart = fy.StartDate.ToDateTimeUtc().Date;
            var fyEnd = fy.EndDate.ToDateTimeUtc().Date;

            var staff = await employees.GetAll()
                .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
                .Select(e => new
                {
                    e.Id,
                    e.PersonId,
                    e.EmployeeNumber,
                    e.HireDate,
                    e.IsManagerial,
                    e.PositionId,
                    First = e.Person != null ? e.Person.FirstName : "",
                    Grand = e.Person != null ? e.Person.GrandFatherName : ""
                })
                .ToListAsync();

            // Per-employee facts the flexible accrual rules need (external gov experience + fiscal-year count).
            var personIds = staff.Where(s => s.PersonId != Guid.Empty).Select(s => s.PersonId).Distinct().ToList();
            var extRows = personIds.Count == 0
                ? new List<ExpSpan>()
                : await experiences.GetAll()
                    .Where(x => personIds.Contains(x.PersonId) && x.IsExternal && x.IsGovernmental
                        && x.StartDate != null && x.EndDate != null)
                    .Select(x => new ExpSpan(x.PersonId, x.StartDate!.Value, x.EndDate!.Value))
                    .ToListAsync();
            var externalMonths = extRows.GroupBy(r => r.PersonId)
                .ToDictionary(g => g.Key, g => g.Sum(r => MonthsBetween(r.Start.Date, r.End.Date)));
            var fyStartDates = await fiscalYears.GetAll().Select(f => f.StartDate).ToListAsync();

            // Resolve each employee's owning unit (via position) in a single batched query.
            var positionIds = staff.Where(s => s.PositionId.HasValue).Select(s => s.PositionId!.Value).Distinct().ToList();
            var unitByPosition = positionIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await positions.GetAll()
                    .Where(p => positionIds.Contains(p.Id))
                    .Join(organizationUnits.GetAll(), p => p.OrganizationUnitId, u => u.Id,
                        (p, u) => new { p.Id, UnitName = u.Name })
                    .ToDictionaryAsync(x => x.Id, x => x.UnitName);

            var existing = await balances.GetAll()
                .Where(b => b.FiscalYearId == setting.FiscalYearId && b.LeaveTypeId == setting.LeaveTypeId)
                .Select(b => new
                {
                    b.EmployeeId,
                    b.Entitled,
                    b.CarriedForward,
                    b.Adjusted,
                    b.Taken
                })
                .ToDictionaryAsync(b => b.EmployeeId);

            var rows = new List<AnnualLeaveLedgerRowDto>(staff.Count);
            foreach (var e in staff)
            {
                var input = new EmployeeAccrualInput(
                    e.HireDate, e.IsManagerial,
                    externalMonths.GetValueOrDefault(e.PersonId),
                    CountFiscalYearsOfService(fyStartDates, e.HireDate, fyStart));
                var calculated = accrualService.CalculateEntitlement(input, setting, fyStart, fyEnd);
                var row = new AnnualLeaveLedgerRowDto
                {
                    EmployeeId = e.Id,
                    EmployeeName = $"{e.First} {e.Grand}".Trim(),
                    EmployeeNumber = e.EmployeeNumber,
                    HireDate = e.HireDate,
                    ServiceYears = ServiceYearsAt(e.HireDate, fyStart),
                    IsManagerial = e.IsManagerial,
                    OrganizationUnitName = e.PositionId.HasValue && unitByPosition.TryGetValue(e.PositionId.Value, out var unit)
                        ? unit : null,
                    CalculatedEntitlement = calculated
                };

                if (existing.TryGetValue(e.Id, out var bal))
                {
                    row.IsGenerated = true;
                    row.Entitled = bal.Entitled;
                    row.CarriedForward = bal.CarriedForward;
                    row.Adjusted = bal.Adjusted;
                    row.Taken = bal.Taken;
                    row.Available = bal.Entitled + bal.CarriedForward + bal.Adjusted - bal.Taken;
                }
                rows.Add(row);
            }

            rows = rows
                .OrderByDescending(r => r.CalculatedEntitlement)
                .ThenBy(r => r.EmployeeNumber)
                .ToList();

            return new AnnualLeaveLedgerDto
            {
                SettingId = setting.Id,
                FiscalYearId = setting.FiscalYearId,
                FiscalYearName = fy.Name,
                FiscalYearStart = fyStart,
                FiscalYearEnd = fyEnd,
                FiscalYearClosed = fy.IsClosed,
                LeaveTypeId = setting.LeaveTypeId,
                LeaveTypeName = setting.LeaveType?.Name,
                TotalEmployees = rows.Count,
                GeneratedCount = rows.Count(r => r.IsGenerated),
                Rows = rows
            };
        }

        private static decimal ServiceYearsAt(DateTime? hireDate, DateTime asOf)
        {
            if (!hireDate.HasValue || hireDate.Value.Date > asOf) return 0m;
            var months = (asOf.Year - hireDate.Value.Year) * 12 + asOf.Month - hireDate.Value.Month
                         - (asOf.Day < hireDate.Value.Day ? 1 : 0);
            return Math.Round(Math.Max(0, months) / 12m, 1);
        }

        private static int MonthsBetween(DateTime fromDate, DateTime toDate)
        {
            if (toDate < fromDate) return 0;
            var months = (toDate.Year - fromDate.Year) * 12 + toDate.Month - fromDate.Month;
            if (toDate.Day < fromDate.Day) months--;
            return Math.Max(0, months);
        }

        private static int CountFiscalYearsOfService(List<Instant> fiscalYearStarts, DateTime? hireDate, DateTime currentFyStart)
        {
            if (!hireDate.HasValue) return 0;
            var hire = hireDate.Value.Date;
            var current = currentFyStart.Date;
            return fiscalYearStarts.Count(s =>
            {
                var d = s.ToDateTimeUtc().Date;
                return d > hire && d <= current;
            });
        }

        private readonly record struct ExpSpan(Guid PersonId, DateTime Start, DateTime End);
    }
}
