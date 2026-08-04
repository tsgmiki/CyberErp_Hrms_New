using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Search.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Search.Providers
{
    /// <summary>Global search over organizational units (departments): by name or code.</summary>
    public class OrganizationUnitSearchProvider(IRepository<OrganizationUnit> units) : ISearchProvider
    {
        public string Category => "Departments";
        public string Icon => "building-2";
        public int Order => 2;

        public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(string term, int take, CancellationToken ct)
        {
            var rows = await units.GetAll().AsNoTracking()
                .Where(u => u.Name.Contains(term) || u.Code.Contains(term))
                .OrderBy(u => u.Name)
                .Take(take)
                // UnitType is an enum → project raw and format in memory (avoids enum-to-SQL translation).
                .Select(u => new { u.Id, u.Name, u.Code, u.UnitType })
                .ToListAsync(ct);

            return rows.Select(u => new SearchResultItem(
                u.Id, u.Name, $"{u.Code} · {u.UnitType}", "/organizationUnit")).ToList();
        }
    }
}
