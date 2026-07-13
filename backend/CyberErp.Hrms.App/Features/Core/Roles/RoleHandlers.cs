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
        IValidator<SaveRoleDto> validator,
        ILogger<SaveRole> logger) : ISaveRole
    {
        public async Task<Guid> SaveAsync(SaveRoleDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(r => r.Name == dto.Name && r.Id != dto.Id))
                throw new DuplicateException(nameof(Role), nameof(dto.Name), dto.Name);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Role), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.Code);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                return entity.Id;
            }

            var created = Role.Create(dto.Name, dto.Code);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Role {Id} ({Name})", created.Id, created.Name);
            return created.Id;
        }
    }

    public class GetAllRoles(IRepository<Role> repository) : IGetAllRoles
    {
        public async Task<PaginatedResponse<RoleDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
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
        ILogger<DeleteRole> logger) : IDeleteRole
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Role), id.ToString());
            if (await userRoleRepository.GetAll().AnyAsync(u => u.RoleId == id))
                throw new ValidationException(nameof(id), "Users are assigned to this role. Remove the assignments first.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted Role {Id}", id);
        }
    }

    // ---- UserRole handlers ---------------------------------------------------------
    public class SaveUserRole(
        IRepository<UserRole> repository,
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        ILogger<SaveUserRole> logger) : ISaveUserRole
    {
        public async Task<Guid> SaveAsync(SaveUserRoleDto dto)
        {
            if (dto.UserId == Guid.Empty || dto.RoleId == Guid.Empty)
                throw new ValidationException("roleId", "A user and a role are both required.");

            // The role/user must be visible in the current tenant (blocks stale or cross-tenant
            // ids from ever reaching the database FK).
            if (!await userRepository.GetAll().AnyAsync(u => u.Id == dto.UserId))
                throw new NotFoundException(nameof(User), dto.UserId.ToString());
            if (!await roleRepository.GetAll().AnyAsync(r => r.Id == dto.RoleId))
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
                    return entity.Id;
                }

                var created = UserRole.Create(dto.RoleId, dto.UserId);
                await repository.AddAsync(created);
                await repository.SaveChangesAsync();
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
        ILogger<DeleteUserRole> logger) : IDeleteUserRole
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(UserRole), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted UserRole {Id}", id);
        }
    }

    // ---- User lookup (list only — creation stays on the auth register flow) --------
    public class GetAllUsers(IRepository<User> repository) : IGetAllUsers
    {
        public async Task<PaginatedResponse<UserDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
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
