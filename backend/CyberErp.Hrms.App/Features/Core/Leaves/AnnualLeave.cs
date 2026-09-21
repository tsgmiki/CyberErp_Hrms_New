using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.App.Features.Core.Leaves
{
    /// <summary>
    /// When a fiscal year's leave may be used — stated once, because it was previously expressed
    /// four different ways and no two agreed.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ THE RULE IS: the year's <see cref="AnnualLeaveSetting"/> is ACTIVE **and** the
    /// <see cref="FiscalYear"/> is NOT CLOSED. Both halves matter and neither is the
    /// <c>FiscalYear.IsActive</c> flag, which is a singleton ("only one active fiscal year at a time",
    /// enforced by <c>SaveFiscalYear</c>) and so can never describe more than one usable year
    /// (logic §12.102).</para>
    ///
    /// <para>What it replaced, and what each one got wrong:</para>
    /// <list type="bullet">
    /// <item><c>f.IsActive</c> — the dashboard cards: singleton, so only ever one year (§12.101).</item>
    /// <item><c>!f.IsClosed</c> — the same cards after §12.101: admitted years whose policy had been
    /// deactivated.</item>
    /// <item>nothing at all — the request form's ledger dropdown and <c>SubmitAnnualLeave</c>, which
    /// accepted any ledger the caller named.</item>
    /// <item><c>FiscalYear.IsActive</c> — Other Leave, whose error message already said "closed"
    /// while the check said "active".</item>
    /// </list>
    /// </remarks>
    public static class LeaveYearRule
    {
        /// <summary>The fiscal years whose leave may currently be drawn on.</summary>
        /// <remarks>
        /// Returns an <see cref="IQueryable{T}"/> of ids so callers compose it into their own query
        /// rather than round-tripping — and so the rule itself lives in exactly one expression.
        /// </remarks>
        public static IQueryable<Guid> UsableFiscalYearIds(IRepository<AnnualLeaveSetting> settings) =>
            settings.GetAll()
                .Where(s => s.IsActive && s.FiscalYear != null && !s.FiscalYear.IsClosed)
                .Select(s => s.FiscalYearId);
    }

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
