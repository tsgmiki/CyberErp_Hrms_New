using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Delegations
{
    /// <summary>
    /// Supplies the monetary value of a request, so a delegation's approval ceiling can be applied.
    /// </summary>
    /// <remarks>
    /// The same shape as <c>IWorkflowEntityHandler</c>: a module opts in by registering one of
    /// these for its entity-type key. A process with no provider has no amount, and an amount of
    /// null is treated as within any ceiling — a limit can only hold back a request whose value is
    /// actually known.
    /// </remarks>
    public interface IDelegationAmountProvider
    {
        bool Supports(string entityType);
        /// <summary>The request's value, or null when it has none / cannot be read.</summary>
        Task<decimal?> GetAmountAsync(string entityType, Guid entityId);
    }

    /// <summary>One delegation that currently applies to the caller.</summary>
    /// <param name="DelegationId">The record, for the audit line.</param>
    /// <param name="FromEmployeeId">Whose authority is being exercised.</param>
    /// <param name="ApprovalLimit">Ceiling, or null when uncapped.</param>
    public record ActiveDelegation(Guid DelegationId, Guid FromEmployeeId, decimal? ApprovalLimit);

    public interface IApprovalDelegationResolver
    {
        /// <summary>
        /// Employee ids whose approval authority the CURRENT user holds today for
        /// <paramref name="entityType"/> — empty when they hold none.
        /// </summary>
        Task<IReadOnlyList<ActiveDelegation>> DelegationsToMeAsync(string entityType);

        /// <summary>
        /// Whether the current user may act on this request THROUGH a delegation, and on whose
        /// behalf. Null when no delegation applies.
        /// </summary>
        /// <param name="couldDecide">
        /// Asks whether a given EMPLOYEE could decide the step on their own authority. Supplied by
        /// the workflow layer, which owns approver resolution — this service must not reimplement it.
        /// </param>
        Task<ActiveDelegation?> ResolveForRequestAsync(
            string entityType, Guid entityId, Guid? subjectEmployeeId, Func<Guid, Task<bool>> couldDecide);

        /// <summary>Employee ids currently standing in for <paramref name="forEmployeeId"/>.</summary>
        Task<IReadOnlyList<Guid>> DelegatesOfAsync(Guid forEmployeeId, string entityType);
    }

    /// <summary>
    /// The one place that answers "is this person currently standing in for somebody?".
    /// </summary>
    /// <remarks>
    /// <para>⚠️ SCOPED, and memoised per request. The approval inbox asks this once per instance;
    /// without caching, a 40-row inbox is 40 identical delegation queries.</para>
    ///
    /// <para>⚠️ IT DOES NOT RECURSE. Only delegations naming the caller directly are considered, so
    /// A to B and B to C never lets C act for A. Re-delegation looks harmless one record at a time
    /// and ends with an executive's authority somewhere nobody chose.</para>
    /// </remarks>
    public class ApprovalDelegationResolver(
        IRepository<ApprovalDelegation> delegations,
        IRepository<User> users,
        IEnumerable<IDelegationAmountProvider> amountProviders,
        Common.Services.ICurrentUserService currentUser) : IApprovalDelegationResolver
    {
        private List<ApprovalDelegation>? _toMe;
        private Guid? _myEmployeeId;
        private bool _myEmployeeLoaded;
        private readonly Dictionary<(string, Guid), decimal?> _amounts = [];

        private async Task<Guid?> MyEmployeeIdAsync()
        {
            if (_myEmployeeLoaded) return _myEmployeeId;
            _myEmployeeLoaded = true;
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return _myEmployeeId = null;
            return _myEmployeeId = await users.GetAll().AsNoTracking()
                .Where(u => u.Id == userId.Value).Select(u => u.EmployeeId).FirstOrDefaultAsync();
        }

        /// <summary>Every effective delegation naming the caller as delegate, scopes included.</summary>
        private async Task<List<ApprovalDelegation>> LoadToMeAsync()
        {
            if (_toMe is not null) return _toMe;

            var me = await MyEmployeeIdAsync();
            if (me is null) return _toMe = [];

            var today = DateTime.UtcNow.Date;
            _toMe = await delegations.GetAll().AsNoTracking()
                .Include(d => d.Scopes)
                .Where(d => d.ToEmployeeId == me.Value
                            && !d.IsRevoked
                            && d.StartDate <= today
                            && d.EndDate >= today)
                .ToListAsync();
            return _toMe;
        }

        public async Task<IReadOnlyList<ActiveDelegation>> DelegationsToMeAsync(string entityType)
        {
            var all = await LoadToMeAsync();
            return [.. all
                .Where(d => d.Covers(entityType))
                .Select(d => new ActiveDelegation(d.Id, d.FromEmployeeId, d.ApprovalLimit))];
        }

        public async Task<ActiveDelegation?> ResolveForRequestAsync(
            string entityType, Guid entityId, Guid? subjectEmployeeId, Func<Guid, Task<bool>> couldDecide)
        {
            var candidates = await DelegationsToMeAsync(entityType);
            if (candidates.Count == 0) return null;

            // ⚠️ Never let a delegation approve the delegate's OWN request. Standing in for your
            // manager must not become a way to sign off your own leave while they are away — the
            // one outcome no approval chain can intend, and the easiest to reach by accident.
            var me = await MyEmployeeIdAsync();
            if (me.HasValue && subjectEmployeeId == me.Value) return null;

            decimal? amount = null;
            var amountLoaded = false;

            foreach (var candidate in candidates)
            {
                // Does the person who lent their authority actually have any on this step? A
                // delegation confers the delegator's rights; it never invents new ones.
                if (!await couldDecide(candidate.FromEmployeeId)) continue;

                if (candidate.ApprovalLimit is not null && !amountLoaded)
                {
                    amount = await AmountAsync(entityType, entityId);
                    amountLoaded = true;
                }

                if (candidate.ApprovalLimit is decimal limit && amount is decimal value && value > limit)
                    continue;   // above this stand-in's ceiling — it waits for the real approver

                return candidate;
            }

            return null;
        }

        public async Task<IReadOnlyList<Guid>> DelegatesOfAsync(Guid forEmployeeId, string entityType)
        {
            var today = DateTime.UtcNow.Date;
            var rows = await delegations.GetAll().AsNoTracking()
                .Include(d => d.Scopes)
                .Where(d => d.FromEmployeeId == forEmployeeId
                            && !d.IsRevoked
                            && d.StartDate <= today
                            && d.EndDate >= today)
                .ToListAsync();

            return [.. rows.Where(d => d.Covers(entityType)).Select(d => d.ToEmployeeId).Distinct()];
        }

        private async Task<decimal?> AmountAsync(string entityType, Guid entityId)
        {
            if (_amounts.TryGetValue((entityType, entityId), out var cached)) return cached;

            var provider = amountProviders.FirstOrDefault(p => p.Supports(entityType));
            var amount = provider is null ? null : await provider.GetAmountAsync(entityType, entityId);
            _amounts[(entityType, entityId)] = amount;
            return amount;
        }
    }
}
