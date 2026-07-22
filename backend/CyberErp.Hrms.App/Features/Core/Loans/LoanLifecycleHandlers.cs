using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Loans
{
    // ---- DTOs ---------------------------------------------------------------
    public class DisburseLoanDto { public string? Reference { get; set; } }
    public class RecordLoanRepaymentDto { public decimal Amount { get; set; } }
    public class IncrementInstallmentDto { public decimal NewMonthlyInstallment { get; set; } }

    // ---- Interfaces ---------------------------------------------------------
    public interface IDisburseLoan { Task DisburseAsync(Guid id, string? reference); }
    public interface IRecordLoanRepayment { Task RecordAsync(Guid id, decimal amount); }
    public interface IIncrementLoanInstallment { Task IncrementAsync(Guid id, decimal newMonthlyInstallment); }
    public interface IGiveLoanConsent { Task ConsentAsync(Guid id); }

    // ---- Handlers -----------------------------------------------------------
    /// <summary>HC256/HC259 — records loan disbursement (finance/CBS hand-off) and activates repayment.</summary>
    public class DisburseLoan(
        IRepository<Loan> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate,
        ILogger<DisburseLoan> logger) : IDisburseLoan
    {
        public async Task DisburseAsync(Guid id, string? reference)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can disburse loans.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.EmployeeLoan, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(nameof(Loan), id.ToString());
            if (entity.Status != LoanStatus.Approved)
                throw new ValidationException(nameof(id), "Only an approved loan can be disbursed.");
            entity.Disburse(DateTime.UtcNow.Date, reference);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Disbursed Loan {Id} (ref {Reference})", id, reference);
        }
    }

    /// <summary>
    /// HC251/HC254 — records a repayment against the loan ledger: marks the oldest pending installments paid
    /// (oldest first) up to the amount, and settles the loan when the schedule is fully paid.
    /// </summary>
    public class RecordLoanRepayment(
        IRepository<Loan> repository,
        IRepository<LoanRepaymentScheduleLine> scheduleRepository,
        IPerformanceVisibilityService visibility) : IRecordLoanRepayment
    {
        public async Task RecordAsync(Guid id, decimal amount)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can record repayments.");
            if (amount <= 0) throw new ValidationException(nameof(amount), "Repayment amount must be positive.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(nameof(Loan), id.ToString());
            if (entity.Status != LoanStatus.Active)
                throw new ValidationException(nameof(id), "Only an active loan can receive repayments.");

            var pending = await scheduleRepository.GetAll()
                .Where(sl => sl.LoanId == id && sl.Status == LoanInstallmentStatus.Pending)
                .OrderBy(sl => sl.InstallmentNo).ToListAsync();

            var today = DateTime.UtcNow.Date;
            var remaining = amount;
            var marked = 0;
            foreach (var line in pending)
            {
                if (remaining + 0.005m < line.Amount) break;   // not enough to cover this installment
                line.MarkPaid(today);
                scheduleRepository.UpdateAsync(line);
                remaining -= line.Amount;
                marked++;
            }
            if (marked == 0)
                throw new ValidationException(nameof(amount), "The amount is not enough to cover the next installment.");

            if (pending.Count == marked)   // nothing left pending → fully repaid
                entity.Settle(today);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    /// <summary>
    /// HC255 — raises the monthly installment: the remaining (pending) schedule is regenerated with the new,
    /// larger monthly amount, keeping the outstanding balance and its principal/interest split intact.
    /// </summary>
    public class IncrementLoanInstallment(
        IRepository<Loan> repository,
        IRepository<LoanRepaymentScheduleLine> scheduleRepository,
        IPerformanceVisibilityService visibility) : IIncrementLoanInstallment
    {
        public async Task IncrementAsync(Guid id, decimal newMonthlyInstallment)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can change the installment.");
            if (newMonthlyInstallment <= 0) throw new ValidationException(nameof(newMonthlyInstallment), "Installment must be positive.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(nameof(Loan), id.ToString());
            if (entity.Status != LoanStatus.Active)
                throw new ValidationException(nameof(id), "Only an active loan's installment can be changed.");
            if (newMonthlyInstallment < entity.MonthlyInstallment)
                throw new ValidationException(nameof(newMonthlyInstallment), "The installment can only be increased.");

            var all = await scheduleRepository.GetAll().Where(sl => sl.LoanId == id).ToListAsync();
            var paid = all.Where(sl => sl.Status == LoanInstallmentStatus.Paid).ToList();
            var pending = all.Where(sl => sl.Status == LoanInstallmentStatus.Pending).ToList();
            if (pending.Count == 0) throw new ValidationException(nameof(id), "There are no pending installments to reschedule.");

            var outstanding = entity.TotalRepayable - paid.Sum(p => p.Amount);
            var remainingInterest = entity.TotalInterest - paid.Sum(p => p.InterestPortion);
            if (outstanding <= 0) throw new ValidationException(nameof(id), "The loan is already fully scheduled.");

            foreach (var p in pending) scheduleRepository.Delete(p);

            var lastPaidDue = paid.Count > 0 ? paid.Max(p => p.DueDate) : entity.RequestDate;
            var startNo = (paid.Count > 0 ? paid.Max(p => p.InstallmentNo) : 0) + 1;
            var count = (int)Math.Ceiling(outstanding / newMonthlyInstallment);
            decimal allocA = 0, allocI = 0;
            for (var i = 0; i < count; i++)
            {
                var last = i == count - 1;
                var amt = last ? outstanding - allocA : newMonthlyInstallment;
                var interest = last ? remainingInterest - allocI
                    : (outstanding > 0 ? Math.Round(newMonthlyInstallment * remainingInterest / outstanding, 2) : 0m);
                var principal = amt - interest;
                allocA += amt; allocI += interest;
                await scheduleRepository.AddAsync(LoanRepaymentScheduleLine.Create(id, startNo + i, lastPaidDue.AddMonths(i + 1), principal, interest, amt));
            }
            entity.SetMonthlyInstallment(newMonthlyInstallment);
            repository.UpdateAsync(entity);
            await scheduleRepository.SaveChangesAsync();
        }
    }

    /// <summary>HC257 — the borrower's online service-commitment consent after endorsement/disbursement.</summary>
    public class GiveLoanConsent(
        IRepository<Loan> repository,
        IPerformanceVisibilityService visibility) : IGiveLoanConsent
    {
        public async Task ConsentAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            var entity = await repository.GetAll().FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException(nameof(Loan), id.ToString());
            // The borrower gives their own consent; HR may record it on their behalf.
            if (!scope.IsAdmin && entity.EmployeeId != (scope.EmployeeId ?? Guid.Empty))
                throw new ValidationException(nameof(id), "You can only consent to your own loan.");
            entity.GiveServiceCommitmentConsent(DateTime.UtcNow.Date);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }
}
