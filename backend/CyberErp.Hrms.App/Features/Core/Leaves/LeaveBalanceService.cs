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
        Task<decimal> GetAvailableAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId);
        Task DeductAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason);
        Task ReverseAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason);
        Task SetOpeningAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, decimal entitled, decimal carriedForward, decimal adjusted, string? reason);
    }

    public class LeaveBalanceService(
        IRepository<LeaveBalance> balances,
        IRepository<LeaveBalanceTransaction> transactions,
        IRepository<LeaveType> leaveTypes) : ILeaveBalanceService
    {
        public async Task<decimal> GetAvailableAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId)
        {
            var balance = await balances.GetAll()
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.FiscalYearId == fiscalYearId);
            if (balance != null) return balance.Available;

            // Not yet materialized → the implicit opening is the type's default annual entitlement.
            var entitled = await leaveTypes.GetAll().Where(t => t.Id == leaveTypeId)
                .Select(t => (decimal?)t.DefaultAnnualEntitlement).FirstOrDefaultAsync();
            return entitled ?? 0m;
        }

        public async Task DeductAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason)
        {
            var balance = await GetOrCreateAsync(employeeId, leaveTypeId, fiscalYearId, postInitialEntitlement: true);
            balance.RecordTaken(days);
            await transactions.AddAsync(LeaveBalanceTransaction.Create(
                employeeId, leaveTypeId, fiscalYearId, LeaveBalanceTransactionType.Deduction,
                -days, balance.Available, reason, referenceId));
            await balances.SaveChangesAsync();
        }

        public async Task ReverseAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, decimal days, Guid referenceId, string reason)
        {
            var balance = await balances.GetAll()
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.FiscalYearId == fiscalYearId);
            if (balance is null) return; // nothing to reverse

            balance.ReverseTaken(days);
            await transactions.AddAsync(LeaveBalanceTransaction.Create(
                employeeId, leaveTypeId, fiscalYearId, LeaveBalanceTransactionType.Reversal,
                days, balance.Available, reason, referenceId));
            await balances.SaveChangesAsync();
        }

        public async Task SetOpeningAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId,
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

        private async Task<LeaveBalance> GetOrCreateAsync(Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, bool postInitialEntitlement)
        {
            var balance = await balances.GetAll()
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.FiscalYearId == fiscalYearId);
            if (balance != null) return balance;

            var entitled = await leaveTypes.GetAll().Where(t => t.Id == leaveTypeId)
                .Select(t => (decimal?)t.DefaultAnnualEntitlement).FirstOrDefaultAsync() ?? 0m;

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
    }
}
