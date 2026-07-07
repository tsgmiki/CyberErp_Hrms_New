using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Roles;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class RoleController(
        ISaveRole saveHandler,
        IGetAllRoles getAllHandler,
        IDeleteRole deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<RoleDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveRoleDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveRoleDto dto)
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

    public class UserRoleController(
        ISaveUserRole saveHandler,
        IGetAllUserRoles getAllHandler,
        IDeleteUserRole deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<UserRoleDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveUserRoleDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveUserRoleDto dto)
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

    public class UserController(
        IGetAllUsers getAllHandler,
        CyberErp.Hrms.App.Features.Core.Users.IGetUserById getByIdHandler,
        CyberErp.Hrms.App.Features.Core.Users.ISaveUser saveHandler,
        CyberErp.Hrms.App.Features.Core.Users.IDeleteUser deleteHandler) : BaseController
    {
        /// <summary>User lookup for pickers (workflow approvers, role assignment) and the admin list.</summary>
        [HttpGet]
        public Task<PaginatedResponse<UserDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<UserDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CyberErp.Hrms.App.Features.Core.Users.SaveUserDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CyberErp.Hrms.App.Features.Core.Users.SaveUserDto dto)
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
