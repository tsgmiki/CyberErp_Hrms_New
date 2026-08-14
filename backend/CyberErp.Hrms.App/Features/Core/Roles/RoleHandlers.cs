using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Roles
{
    /*
     * ⚠️ READ-ONLY SINCE 2026-08-14 — the Roles and User Roles modules were removed from HRMS and are
     * managed by SRMS, which runs against this same CERP database.
     *
     * The SaveRole / DeleteRole / SaveUserRole / DeleteUserRole handlers are gone with them. They were
     * the only callers of ITenantAuthorizationProjector.SyncAsync() on this path, so a role or
     * assignment changed in SRMS no longer needs projecting from here — SRMS writes the tenant tables
     * the runtime reads.
     *
     * What survives is lookup, which HRMS genuinely needs for its own features: workflow definitions
     * and clearance departments pick approvers by role, and the user list backs those pickers.
     */

    // ---- DTOs ---------------------------------------------------------------
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        /// <summary>The employee this login is linked to (null = system/owner account).</summary>
        public Guid? EmployeeId { get; set; }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface IGetAllRoles { Task<PaginatedResponse<RoleDto>> GetAsync(GetAllRequest request); }
    public interface IGetAllUsers { Task<PaginatedResponse<UserDto>> GetAsync(GetAllRequest request); }

    // ---- Role lookup ----------------------------------------------------------
    public class GetAllRoles(
        IRepository<Role> repository,
        IRepository<TenantRole> tenantRoles) : IGetAllRoles
    {
        public async Task<PaginatedResponse<RoleDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            // Core.Role lost its TenantId on 2026-08-13, so repository.GetAll() is now GLOBAL — every
            // tenant's templates, not this one's. Scoping through TenantRole restores exactly what the
            // pickers showed before: the roles THIS tenant holds an instance of.
            var mine = tenantRoles.GetAll()
                .Where(tr => tr.SourceTemplateId != null)
                .Select(tr => tr.SourceTemplateId!.Value);

            var query = repository.GetAll().Where(r => mine.Contains(r.Id));
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.Name).Skip(skip).Take(take)
                .Select(r => new RoleDto { Id = r.Id, Name = r.Name, Code = r.Code })
                .ToListAsync();
            return new PaginatedResponse<RoleDto> { Total = total, Data = data };
        }
    }

    // ---- User lookup ----------------------------------------------------------
    public class GetAllUsers(
        IRepository<User> repository,
        IRepository<TenantUser> tenantUsers) : IGetAllUsers
    {
        public async Task<PaginatedResponse<UserDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            // Core.User lost its TenantId on 2026-08-13, so repository.GetAll() spans EVERY tenant —
            // 506 accounts rather than this tenant's 500. Membership is the scope now.
            var members = tenantUsers.GetAll().Select(tu => tu.UserId);

            var query = repository.GetAll().Where(u => members.Contains(u.Id));
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.FullName.Contains(term) || x.UserName.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query.OrderBy(x => x.FullName).Skip(skip).Take(take)
                .Select(u => new UserDto { Id = u.Id, FullName = u.FullName, UserName = u.UserName, Email = u.Email, PhoneNumber = u.PhoneNumber })
                .ToListAsync();
            return new PaginatedResponse<UserDto> { Total = total, Data = data };
        }
    }
}
