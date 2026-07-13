using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Roles;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Users
{
    // ---- DTOs ---------------------------------------------------------------
    public class SaveUserDto
    {
        public Guid? Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        /// <summary>Required on create; optional on update (blank = keep the current password).</summary>
        public string? Password { get; set; }
        /// <summary>The employee this login belongs to (null = system/owner; drives branch scope).</summary>
        public Guid? EmployeeId { get; set; }
    }

    public class SaveUserDtoValidator : AbstractValidator<SaveUserDto>
    {
        public SaveUserDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(200);
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(200);
            // Password is mandatory only when creating a brand-new user.
            RuleFor(x => x.Password)
                .NotEmpty().MinimumLength(6)
                .When(x => !x.Id.HasValue || x.Id.Value == Guid.Empty);
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISaveUser { Task<Guid> SaveAsync(SaveUserDto dto); }
    public interface IGetUserById { Task<UserDto> GetAsync(Guid id); }
    public interface IDeleteUser { Task DeleteAsync(Guid id); }

    // ---- Handlers -----------------------------------------------------------
    public class SaveUser(
        IRepository<User> repository,
        IAuthentication authentication,
        IValidator<SaveUserDto> validator,
        ILogger<SaveUser> logger) : ISaveUser
    {
        public async Task<Guid> SaveAsync(SaveUserDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Username / email must be unique within the tenant.
            if (await repository.GetAll().AnyAsync(u => u.UserName == dto.UserName && u.Id != dto.Id))
                throw new DuplicateException(nameof(User), nameof(dto.UserName), dto.UserName);
            if (await repository.GetAll().AnyAsync(u => u.Email == dto.Email && u.Id != dto.Id))
                throw new DuplicateException(nameof(User), nameof(dto.Email), dto.Email);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(User), dto.Id.Value.ToString());

                entity.Update(dto.FullName, dto.Email, dto.PhoneNumber, dto.UserName);
                entity.LinkEmployee(dto.EmployeeId);
                // Only rotate the password when a new one is supplied.
                if (!string.IsNullOrWhiteSpace(dto.Password))
                    entity.UpdateCredentials(dto.UserName, authentication.EncryptPassword(dto.Password));

                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated User {Id} ({UserName})", entity.Id, entity.UserName);
                return entity.Id;
            }

            var created = User.Create(
                dto.FullName, dto.Email, dto.PhoneNumber, dto.UserName,
                authentication.EncryptPassword(dto.Password!));
            created.LinkEmployee(dto.EmployeeId);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created User {Id} ({UserName})", created.Id, created.UserName);
            return created.Id;
        }
    }

    public class GetUserById(IRepository<User> repository) : IGetUserById
    {
        public async Task<UserDto> GetAsync(Guid id)
        {
            var u = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(User), id.ToString());
            return new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                EmployeeId = u.EmployeeId
                // Password is never returned.
            };
        }
    }

    public class DeleteUser(
        IRepository<User> repository,
        IRepository<UserRole> userRoleRepository,
        ILogger<DeleteUser> logger) : IDeleteUser
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(User), id.ToString());

            // Remove the user's role assignments first so the UserRole FK does not block the delete.
            var assignments = await userRoleRepository.GetAll().Where(ur => ur.UserId == id).ToListAsync();
            foreach (var assignment in assignments)
                userRoleRepository.Delete(assignment);

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted User {Id} ({Count} role assignment(s) removed)", id, assignments.Count);
        }
    }
}
