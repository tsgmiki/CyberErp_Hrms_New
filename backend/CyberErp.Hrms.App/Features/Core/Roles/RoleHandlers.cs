using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Roles
{
    // ---- DTOs ---------------------------------------------------------------
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }

    public class SaveRoleDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }

    public class SaveRoleDtoValidator : AbstractValidator<SaveRoleDto>
    {
        public SaveRoleDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).MaximumLength(100);
        }
    }

    public class UserRoleDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? User { get; set; }
        public Guid RoleId { get; set; }
        public string? Role { get; set; }
    }

    public class SaveUserRoleDto
    {
        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
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
    public interface ISaveRole { Task<Guid> SaveAsync(SaveRoleDto dto); }
    public interface IGetAllRoles { Task<PaginatedResponse<RoleDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteRole { Task DeleteAsync(Guid id); }
    public interface ISaveUserRole { Task<Guid> SaveAsync(SaveUserRoleDto dto); }
    public interface IGetAllUserRoles { Task<PaginatedResponse<UserRoleDto>> GetAsync(GetAllRequest request); }
    public interface IDeleteUserRole { Task DeleteAsync(Guid id); }
    public interface IGetAllUsers { Task<PaginatedResponse<UserDto>> GetAsync(GetAllRequest request); }

    // ---- Role handlers -----------------------------------------------------------
    public class SaveRole(
        IRepository<Role> repository,
        IRepository<TenantRole> tenantRoles,
        ICurrentTenantService currentTenant,
        IValidator<SaveRoleDto> validator,
        ITenantAuthorizationProjector projector,
        ILogger<SaveRole> logger) : ISaveRole
    {
        public async Task<Guid> SaveAsync(SaveRoleDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // ⚠️ Core.Role is GLOBAL since 2026-08-13, so repository.GetAll() no longer scopes these
            // checks — a name unique to this tenant would collide with another tenant's role. Both
            // uniqueness rules are therefore evaluated over THIS tenant's instances.
            var mine = await tenantRoles.GetAll()
                .Where(tr => tr.SourceTemplateId != null)
                .Select(tr => tr.SourceTemplateId!.Value)
                .ToListAsync();

            if (await repository.GetAll().AnyAsync(r => mine.Contains(r.Id) && r.Name == dto.Name && r.Id != dto.Id))
                throw new DuplicateException(nameof(Role), nameof(dto.Name), dto.Name);

            var code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim();
            if (code is not null && await repository.GetAll()
                    .AnyAsync(r => mine.Contains(r.Id) && r.Code == code && r.Id != dto.Id))
                throw new DuplicateException(nameof(Role), nameof(dto.Code), code);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Role), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Code);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                await projector.SyncAsync();
                return entity.Id;
            }

            var created = Role.Create(dto.Name, dto.Code);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();

            // ⚠️ The instance is created HERE, not by the projector. The projector only updates what
            // already exists — with Core.Role global it can no longer tell which templates belong to
            // this tenant, and instantiating them all would hand us every other tenant's roles. The
            // tenant that creates a role is the one that gets it.
            var tenantId = currentTenant.GetCurrentTenantId();
            if (tenantId is not null && tenantId != Guid.Empty)
            {
                await tenantRoles.AddAsync(TenantRole.Create(
                    tenantId.Value, created.Code, created.Name, created.Id));
                await tenantRoles.SaveChangesAsync();
            }

            await projector.SyncAsync();
            logger.LogInformation("Created Role {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

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
            // screen showed before: the roles THIS tenant holds an instance of.
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

    public class DeleteRole(
        IRepository<Role> repository,
        IRepository<UserRole> userRoleRepository,
        IRepository<TenantRole> tenantRoleRepository,
        IRepository<TenantRolePermission> tenantRolePermissionRepository,
        ITenantAuthorizationProjector projector,
        ILogger<DeleteRole> logger) : IDeleteRole
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Role), id.ToString());
            if (await userRoleRepository.GetAll().AnyAsync(u => u.RoleId == id))
                throw new ValidationException(nameof(id), "Users are assigned to this role. Remove the assignments first.");

            // The tenant instance has to be removed HERE, not left to the projector. TenantRole ->
            // Role is SetNull, so deleting the template would blank SourceTemplateId instead of
            // failing — and a null source reads as a bespoke role, which the projector deliberately
            // never touches. The role would live on, invisible, still granting its permissions.
            var instances = await tenantRoleRepository.GetAll().Where(r => r.SourceTemplateId == id).ToListAsync();
            foreach (var instance in instances)
            {
                var grants = await tenantRolePermissionRepository.GetAll()
                    .Where(p => p.TenantRoleId == instance.Id).ToListAsync();
                foreach (var grant in grants)
                    tenantRolePermissionRepository.Delete(grant);
                tenantRoleRepository.Delete(instance);
            }

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            await projector.SyncAsync();
            logger.LogInformation("Deleted Role {Id} ({Count} tenant instance(s) removed)", id, instances.Count);
        }
    }

    // ---- UserRole handlers ---------------------------------------------------------
    public class SaveUserRole(
        IRepository<UserRole> repository,
        IRepository<TenantUser> tenantUsers,
        IRepository<TenantRole> tenantRoles,
        ITenantAuthorizationProjector projector,
        ILogger<SaveUserRole> logger) : ISaveUserRole
    {
        public async Task<Guid> SaveAsync(SaveUserRoleDto dto)
        {
            if (dto.UserId == Guid.Empty || dto.RoleId == Guid.Empty)
                throw new ValidationException("roleId", "A user and a role are both required.");

            // The role/user must belong to the current tenant (blocks stale or cross-tenant ids from
            // ever reaching the database FK).
            //
            // ⚠️ Both tables are GLOBAL since 2026-08-13, so existence alone no longer proves the row
            // is ours — that check would happily accept another tenant's user. Membership does.
            if (!await tenantUsers.GetAll().AnyAsync(tu => tu.UserId == dto.UserId))
                throw new NotFoundException(nameof(User), dto.UserId.ToString());
            if (!await tenantRoles.GetAll().AnyAsync(tr => tr.SourceTemplateId == dto.RoleId))
                throw new NotFoundException(nameof(Role), dto.RoleId.ToString());
            if (await repository.GetAll().AnyAsync(x => x.UserId == dto.UserId && x.RoleId == dto.RoleId && x.Id != dto.Id))
                throw new ValidationException("roleId", "The user already holds this role.");

            try
            {
                if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
                {
                    var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                        ?? throw new NotFoundException(nameof(UserRole), dto.Id.Value.ToString());
                    entity.Update(dto.RoleId, dto.UserId);
                    repository.UpdateAsync(entity);
                    await repository.SaveChangesAsync();
                    await projector.SyncAsync();
                    return entity.Id;
                }

                var created = UserRole.Create(dto.RoleId, dto.UserId);
                await repository.AddAsync(created);
                await repository.SaveChangesAsync();
                // Creates the tenant membership as well — without it the user has no access at all.
                await projector.SyncAsync();
                logger.LogInformation("Assigned Role {RoleId} to User {UserId}", dto.RoleId, dto.UserId);
                return created.Id;
            }
            catch (DbUpdateException ex)
            {
                // Safety net: a referential-integrity failure (the selected user/role was removed
                // concurrently, or a stale client sent a now-invalid id) is reported as a clean
                // validation error, never a raw SQL 500.
                logger.LogWarning(ex, "UserRole save rejected by the database (User {UserId}, Role {RoleId})", dto.UserId, dto.RoleId);
                throw new ValidationException("roleId",
                    "The selected user or role is no longer available. Please refresh and try again.");
            }
        }
    }

    public class GetAllUserRoles(
        IRepository<UserRole> repository,
        IRepository<User> userRepository,
        IRepository<Role> roleRepository) : IGetAllUserRoles
    {
        public async Task<PaginatedResponse<UserRoleDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            var total = await query.CountAsync();

            var users = userRepository.GetAllWithoutTenantFilter();
            var roles = roleRepository.GetAllWithoutTenantFilter();

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new UserRoleDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    User = users.Where(u => u.Id == x.UserId).Select(u => u.FullName).FirstOrDefault(),
                    RoleId = x.RoleId,
                    Role = roles.Where(r => r.Id == x.RoleId).Select(r => r.Name).FirstOrDefault()
                })
                .ToListAsync();
            return new PaginatedResponse<UserRoleDto> { Total = total, Data = data };
        }
    }

    public class DeleteUserRole(
        IRepository<UserRole> repository,
        ITenantAuthorizationProjector projector,
        ILogger<DeleteUserRole> logger) : IDeleteUserRole
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(UserRole), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            await projector.SyncAsync();
            logger.LogInformation("Deleted UserRole {Id}", id);
        }
    }

    // ---- User lookup (list only — creation stays on the auth register flow) --------
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
