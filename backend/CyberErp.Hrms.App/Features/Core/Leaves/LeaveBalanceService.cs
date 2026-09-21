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
        IRepository<LeaveBalanceTransaction> transactions) : ILeaveBalanceService
    {
        /// <summary>
        /// Days the employee may still draw for a leave type in a fiscal year.
        /// </summary>
        /// <remarks>
        /// ⚠️ NO LEDGER ROW MEANS ZERO, NOT AN IMPLICIT ALLOWANCE. This used to fall back to the
        /// fiscal year's <c>DefaultAnnualEntitlement</c>, and that leaked in two directions: it
        /// granted a balance to employees whose entitlements had never been generated, and — because
        /// the fallback was keyed on the YEAR and not the leave type — it handed the ANNUAL policy's
        /// default to every other accruing leave type as well, so a type with no ledger row of its own
        /// silently inherited 16 days that no policy had ever granted it. The ledger is the only
        /// source of entitlement; nothing in it means nothing to draw (logic §12.100).
        /// </remarks>
        public async Task<decimal> GetAvailableAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId)
        {
            var balance = await FindAsync(employeeId, leaveTypeId, fiscalYearId);
            return balance?.Available ?? 0m;
        }

        public async Task DeductAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason)
        {
            var balance = await GetOrCreateAsync(employeeId, leaveTypeId, fiscalYearId);
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
            var balance = await GetOrCreateAsync(employeeId, leaveTypeId, fiscalYearId);
            var before = balance.Available;
            balance.SetOpening(entitled, carriedForward, adjusted);
            await transactions.AddAsync(LeaveBalanceTransaction.Create(
                employeeId, leaveTypeId, fiscalYearId, LeaveBalanceTransactionType.Opening,
                balance.Available - before, balance.Available, reason ?? "Opening balance set", null));
            await balances.SaveChangesAsync();
        }

        private async Task<LeaveBalance> GetOrCreateAsync(Guid employeeId, Guid? leaveTypeId, Guid fiscalYearId)
        {
            var balance = await FindAsync(employeeId, leaveTypeId, fiscalYearId);
            if (balance != null) return balance;

            // ⚠️ OPENS AT ZERO. It used to seed the new row from the policy default and post an
            // "Initial annual entitlement" transaction for it, inventing an entitlement nobody had
            // granted. A row created here exists only to carry a deduction or reversal; the
            // sufficiency check upstream has already decided whether that is allowed
            // (logic §12.100). The `postInitialEntitlement` flag went with it — there is no opening
            // figure left to post, and a parameter that no longer does anything only misleads.
            balance = LeaveBalance.Create(employeeId, leaveTypeId, fiscalYearId, 0m);
            await balances.AddAsync(balance);
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
    }
}
