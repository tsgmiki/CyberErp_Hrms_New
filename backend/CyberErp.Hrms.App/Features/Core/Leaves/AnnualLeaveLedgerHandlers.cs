using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

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
        IRepository<LeaveBalance> balances,
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
                    e.EmployeeNumber,
                    e.HireDate,
                    e.IsManagerial,
                    First = e.Person != null ? e.Person.FirstName : "",
                    Grand = e.Person != null ? e.Person.GrandFatherName : ""
                })
                .ToListAsync();

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
                var calculated = accrualService.CalculateEntitlement(e.HireDate, e.IsManagerial, setting, fyStart, fyEnd);
                var row = new AnnualLeaveLedgerRowDto
                {
                    EmployeeId = e.Id,
                    EmployeeName = $"{e.First} {e.Grand}".Trim(),
                    EmployeeNumber = e.EmployeeNumber,
                    HireDate = e.HireDate,
                    ServiceYears = ServiceYearsAt(e.HireDate, fyStart),
                    IsManagerial = e.IsManagerial,
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
    }
}
