using CyberErp.Hrms.App.Features.Core.Search.DTOs;

namespace CyberErp.Hrms.App.Features.Core.Search
{
    /// <summary>
    /// A pluggable global-search source for one module. Adding a searchable module is one class:
    /// implement this, query through the module's tenant-scoped <c>IRepository&lt;T&gt;</c>, and register
    /// it in DI — the orchestrator (<see cref="IGlobalSearch"/>) fans out to every registered provider,
    /// so no central switch needs editing. Providers MUST cap their own results (honour <c>take</c>) and
    /// query read-only/projected — they run on the request's shared DbContext.
    /// </summary>
    public interface ISearchProvider
    {
        /// <summary>Display name of the result group, e.g. "Employees".</summary>
        string Category { get; }

        /// <summary>lucide-react icon name the dropdown renders for this group (e.g. "users").</summary>
        string Icon { get; }

        /// <summary>Group display order (ascending) in the dropdown.</summary>
        int Order { get; }

        /// <summary>Return up to <paramref name="take"/> matches for <paramref name="term"/> (already
        /// trimmed and length-validated). Tenant/branch isolation comes for free from the repository.</summary>
        Task<IReadOnlyList<SearchResultItem>> SearchAsync(string term, int take, CancellationToken ct);
    }
}
