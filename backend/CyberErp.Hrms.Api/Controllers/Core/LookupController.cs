using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Lookups;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Generic, centralized lookup API. Every system-wide reference list (Education Level, Field of
    /// Study, …) is served from the two lookup tables via this one controller — comboboxes read
    /// <c>GET api/v1/Lookup/items/{categoryCode}</c>; the rest is admin management of categories/values.
    /// </summary>
    public class LookupController(
        IGetLookupItems getItemsHandler,
        IGetAllLookupCategories getAllHandler,
        ISaveLookupCategory saveHandler,
        IDeleteLookupCategory deleteHandler) : BaseController
    {
        /// <summary>The combobox feed: the value list of a category, by its code.</summary>
        [HttpGet("items/{categoryCode}")]
        public Task<List<LookupItemDto>> GetItems(string categoryCode)
            => getItemsHandler.GetAsync(categoryCode);

        [HttpGet]
        public Task<PaginatedResponse<LookupCategoryDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveLookupCategoryDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveLookupCategoryDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
