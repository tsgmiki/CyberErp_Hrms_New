namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>
    /// Annual leave is deliberately NOT modelled as a <c>LeaveType</c>.
    ///
    /// Its entitlement is computed per fiscal year from <c>AnnualLeaveSetting</c> (service length,
    /// milestones, managerial basis, carry-forward cap …), so there is nothing for a LeaveType row to
    /// contribute — and requiring one meant the whole annual ledger fell over when no type happened to
    /// be flagged with the Annual accrual method. <c>LeaveType</c> now covers only the OTHER leave
    /// kinds, which is what it was for.
    ///
    /// The ledger therefore identifies annual balances by a null <c>LeaveTypeId</c>. This type exists
    /// so that convention is stated once and searchable, instead of bare nulls scattered through the
    /// leave handlers.
    /// </summary>
    public static class AnnualLeave
    {
        /// <summary>The <c>LeaveTypeId</c> stored on annual <c>LeaveBalance</c> / transaction rows.</summary>
        public static readonly Guid? LeaveTypeId = null;

        /// <summary>What to show wherever a leave-type name is displayed for annual leave.</summary>
        public const string DisplayName = "Annual Leave";
    }
}
