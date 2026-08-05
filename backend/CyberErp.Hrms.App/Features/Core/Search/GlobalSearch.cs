using CyberErp.Hrms.App.Features.Core.Search.DTOs;

namespace CyberErp.Hrms.App.Features.Core.Search
{
    public interface IGlobalSearch
    {
        Task<GlobalSearchResponse> SearchAsync(string? query, int perCategory, CancellationToken ct);
    }

    /// <summary>
    /// Orchestrates the header global search: fans the query out to every registered
    /// <see cref="ISearchProvider"/> and assembles a categorized, ordered result set.
    /// </summary>
    /// <remarks>
    /// Performance / safety:
    /// - Below <see cref="MinLength"/> characters we return empty WITHOUT touching the DB — a single-char
    ///   query would force a full leading-wildcard scan of every module for no useful result.
    /// - Each provider is hard-capped by <c>take</c> (clamped to <see cref="MaxPerCategory"/>), so the
    ///   dropdown stays small and every query is bounded regardless of table size.
    /// - Providers run SEQUENTIALLY: they share the request's scoped EF <c>DbContext</c>, which is not
    ///   safe for concurrent queries. Each is a small, capped, projected read, so total latency is the
    ///   sum of a few fast round-trips. (To parallelize later, give each provider its own DI scope.)
    /// - Cancellation is honoured between providers and inside each query, so an abandoned keystroke
    ///   (the client cancels superseded requests) stops work promptly.
    /// </remarks>
    public class GlobalSearch(IEnumerable<ISearchProvider> providers) : IGlobalSearch
    {
        private const int MinLength = 2;
        private const int DefaultPerCategory = 5;
        private const int MaxPerCategory = 10;

        public async Task<GlobalSearchResponse> SearchAsync(string? query, int perCategory, CancellationToken ct)
        {
            var term = (query ?? string.Empty).Trim();
            var response = new GlobalSearchResponse { Query = term };
            if (term.Length < MinLength) return response;

            var take = Math.Clamp(perCategory <= 0 ? DefaultPerCategory : perCategory, 1, MaxPerCategory);

            var groups = new List<SearchResultGroup>();
            foreach (var provider in providers.OrderBy(p => p.Order))
            {
                ct.ThrowIfCancellationRequested();
                var items = await provider.SearchAsync(term, take, ct);
                if (items.Count > 0)
                    groups.Add(new SearchResultGroup(provider.Category, provider.Icon, provider.Order, items));
            }

            response.Groups = groups;
            response.TotalCount = groups.Sum(g => g.Items.Count);
            return response;
        }
    }
}
