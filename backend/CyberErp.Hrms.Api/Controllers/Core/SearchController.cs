using CyberErp.Hrms.App.Features.Core.Search;
using CyberErp.Hrms.App.Features.Core.Search.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Header global search — one call fans out to every registered module search provider.</summary>
    public class SearchController(IGlobalSearch search) : BaseController
    {
        /// <summary>GET api/v1/Search?q=abebe&amp;limit=5 — categorized cross-module results.</summary>
        [HttpGet]
        public Task<GlobalSearchResponse> Get(
            [FromQuery] string? q,
            [FromQuery] int limit = 5,
            CancellationToken ct = default) => search.SearchAsync(q, limit, ct);
    }
}
