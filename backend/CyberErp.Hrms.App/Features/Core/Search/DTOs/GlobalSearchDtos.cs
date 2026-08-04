namespace CyberErp.Hrms.App.Features.Core.Search.DTOs
{
    /// <summary>One hit in the global search dropdown. <see cref="Route"/> is the SPA path the UI
    /// navigates to when the item is chosen; <see cref="Icon"/>-less items inherit their group icon.</summary>
    public record SearchResultItem(Guid Id, string Title, string? Subtitle, string Route);

    /// <summary>A category of results (e.g. "Employees"), pre-ordered for a stable dropdown layout.</summary>
    public record SearchResultGroup(string Category, string Icon, int Order, IReadOnlyList<SearchResultItem> Items);

    /// <summary>The categorized global-search payload. Empty <see cref="Groups"/> = no matches (or the
    /// query was below the minimum length).</summary>
    public class GlobalSearchResponse
    {
        public string Query { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public IReadOnlyList<SearchResultGroup> Groups { get; set; } = [];
    }
}
