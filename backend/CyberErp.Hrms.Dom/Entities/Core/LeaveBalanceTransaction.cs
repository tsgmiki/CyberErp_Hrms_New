using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LeaveBalanceTransactionType
{
    Opening = 0,
    Entitlement = 1,
    CarryForward = 2,
    Accrual = 3,
    Deduction = 4,
    Reversal = 5,
    Adjustment = 6,
    /// <summary>Unused carried-forward leave written off on fiscal-year rollover.</summary>
    Expiry = 7
}

/// <summary>
/// Append-only ledger entry behind <see cref="LeaveBalance"/>. Every credit/debit is recorded with a
/// signed <see cref="Delta"/> (in days) so balances are auditable and reconstructable.
/// </summary>
public class LeaveBalanceTransaction : BaseEntity, IAggregateRoot
{
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public Guid FiscalYearId { get; private set; }
    public LeaveBalanceTransactionType TransactionType { get; private set; }
    /// <summary>Signed change in available days (positive = credit, negative = debit).</summary>
    public decimal Delta { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string? Reason { get; private set; }
    /// <summary>Originating record (e.g. the leave request id) for traceability.</summary>
    public Guid? ReferenceId { get; private set; }
    public DateTime TransactionDate { get; private set; }

    private LeaveBalanceTransaction() : base() { }

    public static LeaveBalanceTransaction Create(
        Guid employeeId, Guid leaveTypeId, Guid fiscalYearId, LeaveBalanceTransactionType type,
        decimal delta, decimal balanceAfter, string? reason, Guid? referenceId)
    {
        return new LeaveBalanceTransaction
        {
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            FiscalYearId = fiscalYearId,
            TransactionType = type,
            Delta = delta,
            BalanceAfter = balanceAfter,
            Reason = reason,
            ReferenceId = referenceId,
            TransactionDate = DateTime.UtcNow
        };
    }
}
