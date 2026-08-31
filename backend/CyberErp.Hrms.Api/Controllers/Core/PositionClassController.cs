using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.PositionClasses;
using CyberErp.Hrms.App.Features.Core.PositionClasses.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    [RequirePermission("positionClass")]
    public class PositionClassController(
        ICreatePositionClass createHandler,
        IUpdatePositionClass updateHandler,
        IDeletePositionClass deleteHandler,
        IGetPositionClassById getByIdHandler,
        IGetAllPositionClasses getAllHandler) : BaseController
    {
        /// <summary>
        /// Readable by anyone who can raise a HIRING REQUEST, not only by whoever maintains the
        /// catalogue.
        ///
        /// <para>⚠️ The role picker on the hiring-request form is this list. Gating it on
        /// <c>positionClass</c> alone meant a department manager — who holds <c>hiringRequest</c> but
        /// not the catalogue screen — got a 403 and an EMPTY dropdown with no error shown, which
        /// reads as missing data rather than a refused request (logic §12.65). Same shape as
        /// <c>JobGradeController</c>, which opens its reads to the employee and profile screens.</para>
        ///
        /// <para>⚠️ Reads only. Creating, editing and deleting a position class stay on
        /// <c>positionClass</c> via the controller-level attribute. And because an action-level
        /// attribute REPLACES the class-level one rather than adding to it, <c>positionClass</c> is
        /// repeated here — omitting it would REVOKE the catalogue owner's own access (logic §12.54).</para>
        /// </summary>
        [HttpGet]
        [RequirePermission("positionClass", "hiringRequest")]
        public Task<PaginatedResponse<PositionClassDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        [RequirePermission("positionClass", "hiringRequest")]
        public Task<PositionClassDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreatePositionClassDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePositionClassDto dto)
        {
            await updateHandler.UpdateAsync(dto);
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
