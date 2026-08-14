using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Roles;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /*
     * ⚠️ LOOKUP-ONLY SINCE 2026-08-14. Users, Roles and User Roles are MANAGED BY SRMS now, which runs
     * against this same CERP database — the HRMS screens for them were removed (handoff 0107), and so
     * were every create/update/delete action and the UserRole controller entirely.
     *
     * What remains is one read each, and only because HRMS features of its own depend on them: the
     * workflow-definition and clearance-department forms pick approvers by user and by role, and the
     * report viewer filters by them.
     *
     * ⚠️ THE GATES NAME THE CONSUMING SCREENS, NOT "user"/"role". They used to require the `user` and
     * `role` permissions — but those menu operations were DELETED with the screens, and a permission
     * that no longer exists can never be granted, so the old gates would have returned 403 to
     * everyone, permanently, and silently emptied those pickers. Naming the screens that actually
     * consume the data keeps the check meaningful: you may list users if you may open a screen that
     * needs the list.
     *
     * Do not add create/update/delete back here. If HRMS needs to change a user or a role, that is a
     * request to SRMS, not a second writer against a shared table.
     */

    [RequirePermission("workflowDefinition", "clearanceDepartment", "reports")]
    public class RoleController(IGetAllRoles getAllHandler) : BaseController
    {
        /// <summary>Role lookup for pickers (workflow approvers, clearance departments, report filters).</summary>
        [HttpGet]
        public Task<PaginatedResponse<RoleDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);
    }

    [RequirePermission("workflowDefinition", "clearanceDepartment", "reports")]
    public class UserController(IGetAllUsers getAllHandler) : BaseController
    {
        /// <summary>User lookup for the same pickers.</summary>
        [HttpGet]
        public Task<PaginatedResponse<UserDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);
    }
}
