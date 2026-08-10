using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>
    /// Owns the leave-balance ledger (HC033): materializes per-employee/type/fiscal-year balances,
    /// applies debits/credits and writes a matching <see cref="LeaveBalanceTransaction"/> for every
    /// change. Callers use this rather than touching balances directly.
    /// </summary>
    public interface ILeaveBalanceService
    {
        Task<decimal> GetAvailableAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId);
        Task DeductAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason);
        Task ReverseAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason);
        Task SetOpeningAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId, decimal entitled, decimal carriedForward, decimal adjusted, string? reason);
    }

    public class LeaveBalanceService(
        IRepository<LeaveBalance> balances,
        IRepository<LeaveBalanceTransaction> transactions,
        IRepository<AnnualLeaveSetting> leaveSettings) : ILeaveBalanceService
    {
        public async Task<decimal> GetAvailableAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId)
        {
            var balance = await FindAsync(employeeId, leaveTypeId, fiscalYearId);
            if (balance != null) return balance.Available;

            // Not yet materialized → the implicit opening is the fiscal year's policy default
            // (DefaultAnnualEntitlement moved from LeaveType to the per-FY leave setting).
            return await DefaultEntitlementAsync(fiscalYearId);
        }

        public async Task DeductAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason)
        {
            var balance = await GetOrCreateAsync(employeeId, leaveTypeId, fiscalYearId, postInitialEntitlement: true);
            balance.RecordTaken(days);
            await transactions.AddAsync(LeaveBalanceTransaction.Create(
                employeeId, leaveTypeId, fiscalYearId, LeaveBalanceTransactionType.Deduction,
                -days, balance.Available, reason, referenceId));
            await balances.SaveChangesAsync();
        }

        public async Task ReverseAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason)
        {
            var balance = await FindAsync(employeeId, leaveTypeId, fiscalYearId);
            if (balance is null) return; // nothing to reverse

            balance.ReverseTaken(days);
            await transactions.AddAsync(LeaveBalanceTransaction.Create(
                employeeId, leaveTypeId, fiscalYearId, LeaveBalanceTransactionType.Reversal,
                days, balance.Available, reason, referenceId));
            await balances.SaveChangesAsync();
        }

        public async Task SetOpeningAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId,
            decimal entitled, decimal carriedForward, decimal adjusted, string? reason)
        {
            var balance = await GetOrCreateAsync(employeeId, leaveTypeId, fiscalYearId, postInitialEntitlement: false);
            var before = balance.Available;
            balance.SetOpening(entitled, carriedForward, adjusted);
            await transactions.AddAsync(LeaveBalanceTransaction.Create(
                employeeId, leaveTypeId, fiscalYearId, LeaveBalanceTransactionType.Opening,
                balance.Available - before, balance.Available, reason ?? "Opening balance set", null));
            await balances.SaveChangesAsync();
        }

        private async Task<LeaveBalance> GetOrCreateAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId, bool postInitialEntitlement)
        {
            var balance = await FindAsync(employeeId, leaveTypeId, fiscalYearId);
            if (balance != null) return balance;

            var entitled = await DefaultEntitlementAsync(fiscalYearId);

            balance = LeaveBalance.Create(employeeId, leaveTypeId, fiscalYearId, entitled);
            await balances.AddAsync(balance);
            if (postInitialEntitlement && entitled > 0)
            {
                await transactions.AddAsync(LeaveBalanceTransaction.Create(
                    employeeId, leaveTypeId, fiscalYearId, LeaveBalanceTransactionType.Entitlement,
                    entitled, entitled, "Initial annual entitlement", null));
            }
            return balance;
        }

        /// <summary>
        /// The balance row for an employee/type/year, where a null <paramref name="leaveTypeId"/> means
        /// ANNUAL leave.
        ///
        /// The <c>== leaveTypeId</c> comparison is safe with a null argument: EF Core's null semantics
        /// compile it to <c>[LeaveTypeId] IS NULL</c> rather than SQL equality (which would never match),
        /// and it caches the two shapes separately. Verified against the generated SQL — do not "fix" this
        /// into an equality that bypasses EF's translation.
        /// </summary>
        private Task<LeaveBalance?> FindAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId) =>
            balances.GetAll()
                .Where(b => b.EmployeeId == employeeId && b.FiscalYearId == fiscalYearId
                            && b.LeaveTypeId == leaveTypeId)
                .FirstOrDefaultAsync();

        /// <summary>The active per-FY leave setting's fallback entitlement (0 when no setting exists).</summary>
        private async Task<decimal> DefaultEntitlementAsync(Guid fiscalYearId)
        {
            return await leaveSettings.GetAll()
                .Where(s => s.FiscalYearId == fiscalYearId && s.IsActive)
                .Select(s => (decimal?)s.DefaultAnnualEntitlement).FirstOrDefaultAsync() ?? 0m;
        }
    }
}
