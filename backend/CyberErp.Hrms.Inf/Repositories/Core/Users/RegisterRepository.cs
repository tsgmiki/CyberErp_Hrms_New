using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.Dom.Entities.Core;
using UserEntity = CyberErp.Hrms.Dom.Entities.Core.User;
using CyberErp.Hrms.Inf.Common;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Users.Register;
using CyberErp.Hrms.App.Features.Core.Users.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace CyberErp.Hrms.Inf.Repositories.Core.Users
{
    public class RegisterRepository(
        IRepository<UserEntity> userRepository,
        IRepository<Tenant> tenantRepository,
        IRepository<Organization> organizationRepository,
        IRepository<TenantUser> tenantUserRepository,
        IRepository<TenantSubscription> tenantSubscriptionRepository,
        IAuthentication authentication,
        ITokenStore tokenStore,
        ITokenParser tokenParser,
        ILogger<RegisterRepository> logger,
        IHttpContextAccessor httpContextAccessor,
        IExceptionHandler exceptionHandler) : IRegisterRepository
    {
        private readonly IRepository<UserEntity> _userRepository = userRepository;
        private readonly IRepository<Tenant> _tenantRepository = tenantRepository;
        private readonly IRepository<Organization> _organizationRepository = organizationRepository;
        private readonly IRepository<TenantUser> _tenantUserRepository = tenantUserRepository;
        private readonly IRepository<TenantSubscription> _tenantSubscriptionRepository = tenantSubscriptionRepository;
        private readonly IAuthentication _authentication = authentication;
        private readonly ITokenStore _tokenStore = tokenStore;
        private readonly ITokenParser _tokenParser = tokenParser;
        private readonly ILogger<RegisterRepository> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IExceptionHandler _exceptionHandler = exceptionHandler;

        /// <summary>
        /// Creates the legal entity a new tenant belongs to, and returns its id.
        /// </summary>
        /// <remarks>
        /// ⚠️ THIS IS WHY REGISTRATION WAS RETURNING 500. <c>Tenant.OrganizationId</c> is a REQUIRED
        /// foreign key added when CompanyProfile was consolidated into <c>Organization</c>
        /// (2026-08-15), but <c>Tenant.Create</c> does not take one and this path never called
        /// <c>SetOrganization</c> — so every self-registration died inside SaveChanges with
        /// "The value of 'Tenant.OrganizationId' is unknown when attempting to save changes."
        ///
        /// <para>A new organization per registration is the right shape: an organization sits ABOVE
        /// the tenant and carries no TenantId of its own, so there is no chicken-and-egg, and a
        /// self-registration genuinely IS a new legal entity signing up. One organization may later
        /// hold several tenants, which is an administrative act, not a signup one.</para>
        /// </remarks>
        private async Task<Guid> CreateOrganizationForAsync(string tenantName, string tenantIdentifier, string? email)
        {
            var code = string.IsNullOrWhiteSpace(tenantIdentifier) ? tenantName : tenantIdentifier;
            var organization = Organization.Create(
                code: code.Trim(),
                legalName: tenantName.Trim(),
                displayName: tenantName.Trim());

            await _organizationRepository.AddAsync(organization);
            await _organizationRepository.SaveChangesAsync();
            _logger.LogInformation("Organization created for new tenant: {OrganizationId} ({Code})", organization.Id, code);
            return organization.Id;
        }

        /// <summary>
        /// Records the new user's membership of the new tenant.
        /// </summary>
        /// <remarks>
        /// ⚠️ THE SECOND REASON REGISTRATION DID NOT WORK. Even once the tenant saved, the account
        /// could not sign in: login answered "This account is not assigned to any organization."
        ///
        /// <para><c>Core.User</c> lost its <c>TenantId</c> on 2026-08-13 and <c>LoginRepository</c>
        /// now resolves the session's tenant from a <c>Core.TenantUser</c> membership row instead.
        /// Registration was still only setting <c>user.TenantId</c> — a column the login path no
        /// longer consults — so it created a tenant and a user that had nothing joining them.</para>
        ///
        /// <para>Default membership, active: this is the account that just created the tenant.</para>
        /// </remarks>
        private async Task AddTenantMembershipAsync(Guid tenantId, Guid userId)
        {
            var membership = TenantUser.Create(tenantId, userId, status: true, isDefaultTenant: true);
            await _tenantUserRepository.AddAsync(membership);
            await _tenantUserRepository.SaveChangesAsync();
            _logger.LogInformation("Tenant membership created: user {UserId} -> tenant {TenantId}", userId, tenantId);
        }

        public Task<RegisterResult> RegisterAsync(RegisterUserDto dto) =>
            RepositoryExecutor.ExecuteAsync(
                _exceptionHandler,
                _logger,
                new ExceptionHandlingContext
                { OperationName = "Register", EntityType = nameof(UserEntity) },
                "Create",
                async () =>
                {
                    _logger.LogInformation("Starting registration for user: {UserName }, Tenant: {TenantName }", dto.UserName, dto.TenantName);

                    var existingUser = await _userRepository.GetAllWithoutTenantFilter()
                .Where(u => u.UserName == dto.UserName || u.Email == dto.Email || u.PhoneNumber == dto.PhoneNumber)
                .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        if (existingUser.Email == dto.Email)
                        {
                            _logger.LogWarning("User with email already exists: {Email }", dto.Email);
                            throw new DuplicateException("User", "Email", dto.Email);
                        }
                        if (existingUser.PhoneNumber == dto.PhoneNumber)
                        {
                            _logger.LogWarning("User with phone number already exists: {PhoneNumber }", dto.PhoneNumber);
                            throw new DuplicateException("User", "PhoneNumber", dto.PhoneNumber);
                        }
                        if (existingUser.UserName == dto.UserName)
                        {
                            _logger.LogWarning("User with username already exists: {UserName }", dto.UserName);
                            throw new DuplicateException("User", "UserName", dto.UserName);
                        }
                    }

                    var identifier = string.IsNullOrEmpty(dto.TenantIdentifier) ? "00" : dto.TenantIdentifier;

                    // The owning legal entity first — the tenant's FK to it is required.
                    var organizationId = await CreateOrganizationForAsync(dto.TenantName, identifier, dto.Email);

                    // Create tenant
                    var tenant = Tenant.Create(
                        name: dto.TenantName,
                        identifier: identifier,
                        address: dto.TenantAddress,
                        phoneNumber: dto.TenantPhoneNumber,
                        email: dto.Email
                    );
                    tenant.SetOrganization(organizationId);

                    await _tenantRepository.AddAsync(tenant);
                    await _tenantRepository.SaveChangesAsync();
                    _logger.LogInformation("Tenant created: {TenantId }, Name: {TenantName }", tenant.Id, tenant.Name);

                    // Create user with tenant ID
                    var user = UserEntity.Create(
                        fullName: dto.FullName,
                        email: dto.Email,
                        phoneNumber: dto.PhoneNumber,
                        userName: dto.UserName,
                        password: _authentication.EncryptPassword(dto.Password)
                    );

                    // Set tenant ID on user (BaseEntity has TenantId property)
                    user.TenantId = tenant.Id.ToString();
                    // The tenant owner has no employee link, so it resolves to Head Office (global
                    // visibility) at login — no explicit flag needed on the User record.

                    await _userRepository.AddAsync(user);
                    await _userRepository.SaveChangesAsync();
                    _logger.LogInformation("User created: {UserId }, UserName: {UserName }, TenantId: {TenantId }", user.Id, user.UserName, tenant.Id);

                    await AddTenantMembershipAsync(tenant.Id, user.Id);

                    // Generate token
                    var tokenId = Guid.NewGuid();
                    var userResult = new UserResult
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        UserName = user.UserName,
                        TenantId = Guid.Parse(tenant.Id.ToString())
                    };

                    var token = _authentication.GenerateToken(userResult, tokenId);
                    var jwtToken = _tokenParser.ParseToken(token);
                    await _tokenStore.StoreAsync(tokenId.ToString(), jwtToken.ValidTo);

                    _logger.LogInformation("Registration completed for user: {UserName }, Tenant: {TenantName }", dto.UserName, dto.TenantName);

                    return new RegisterResult
                    {
                        UserId = user.Id,
                        TenantId = tenant.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        UserName = user.UserName,
                        Token = token
                    };
                });

        public Task<RegisterResult> RegisterWithGoogleAsync(RegisterWithGoogleDto dto) =>
            RepositoryExecutor.ExecuteAsync(
                _exceptionHandler,
                _logger,
                new ExceptionHandlingContext
                { OperationName = "RegisterWithGoogle", EntityType = nameof(UserEntity) },
                "Create",
                async () =>
                {
                    _logger.LogInformation("Starting Google registration for email: {Email }, Tenant: {TenantName }", dto.Email, dto.TenantName);

                    var existingUser = await _userRepository.GetAllWithoutTenantFilter()
                .Where(u => u.Email == dto.Email || (dto.PhoneNumber != null && u.PhoneNumber == dto.PhoneNumber))
                .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        if (existingUser.Email == dto.Email)
                        {
                            _logger.LogWarning("User already exists with email: {Email }", dto.Email);
                            throw new DuplicateException("User", "Email", dto.Email);
                        }
                        if (dto.PhoneNumber != null && existingUser.PhoneNumber == dto.PhoneNumber)
                        {
                            _logger.LogWarning("User already exists with phone number: {PhoneNumber }", dto.PhoneNumber);
                            throw new DuplicateException("User", "PhoneNumber", dto.PhoneNumber);
                        }
                    }

                    // The owning legal entity first — the tenant's FK to it is required.
                    var organizationId = await CreateOrganizationForAsync(dto.TenantName, dto.TenantIdentifier, dto.Email);

                    // Create tenant
                    var tenant = Tenant.Create(
                        name: dto.TenantName,
                        identifier: dto.TenantIdentifier,
                        address: dto.TenantAddress,
                        phoneNumber: dto.TenantPhoneNumber,
                        email: dto.Email
                    );
                    tenant.SetOrganization(organizationId);

                    await _tenantRepository.AddAsync(tenant);
                    await _tenantRepository.SaveChangesAsync();
                    _logger.LogInformation("Tenant created via Google signup: {TenantId }, Name: {TenantName }", tenant.Id, tenant.Name);

                    // Generate random password for Google users (they won't use it)
                    var randomPassword = Guid.NewGuid().ToString();

                    // Create user with tenant ID
                    var user = UserEntity.Create(
                        fullName: dto.FullName,
                        email: dto.Email,
                        phoneNumber: dto.PhoneNumber ?? string.Empty,
                        userName: dto.Email, // Use email as username for Google users
                        password: _authentication.EncryptPassword(randomPassword)
                    );

                    // Set tenant ID on user
                    user.TenantId = tenant.Id.ToString();
                    // No employee link → resolves to Head Office (global visibility) at login.

                    await _userRepository.AddAsync(user);
                    // ⚠️ This SaveChanges was missing entirely, so the Google path created a tenant
                    // and an organization, handed back a token, and never persisted the USER — the
                    // account it reported creating could not sign in. The password path beside it
                    // has always saved; this one simply never did.
                    await _userRepository.SaveChangesAsync();
                    _logger.LogInformation("User created via Google signup: {UserId }, Email: {Email }, TenantId: {TenantId }", user.Id, user.Email, tenant.Id);

                    await AddTenantMembershipAsync(tenant.Id, user.Id);

                    // Generate token
                    var tokenId = Guid.NewGuid();
                    var userResult = new UserResult
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        UserName = user.UserName,
                        TenantId = Guid.Parse(tenant.Id.ToString())
                    };

                    var token = _authentication.GenerateToken(userResult, tokenId);
                    var jwtToken = _tokenParser.ParseToken(token);
                    await _tokenStore.StoreAsync(tokenId.ToString(), jwtToken.ValidTo);

                    _logger.LogInformation("Google registration completed for email: {Email }, Tenant: {TenantName }", dto.Email, dto.TenantName);

                    return new RegisterResult
                    {
                        UserId = user.Id,
                        TenantId = tenant.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        UserName = user.UserName,
                        Token = token
                    };
                });
    }
}
