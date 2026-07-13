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
        private readonly IRepository<TenantSubscription> _tenantSubscriptionRepository = tenantSubscriptionRepository;
        private readonly IAuthentication _authentication = authentication;
        private readonly ITokenStore _tokenStore = tokenStore;
        private readonly ITokenParser _tokenParser = tokenParser;
        private readonly ILogger<RegisterRepository> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IExceptionHandler _exceptionHandler = exceptionHandler;

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

                    // Create tenant
                    var tenant = Tenant.Create(
                        name: dto.TenantName,
                        identifier: string.IsNullOrEmpty(dto.TenantIdentifier) ?
                        "00" : dto.TenantIdentifier,
                        address: dto.TenantAddress,
                        phoneNumber: dto.TenantPhoneNumber,
                        email: dto.Email
                    );

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

                    // Create tenant
                    var tenant = Tenant.Create(
                        name: dto.TenantName,
                        identifier: dto.TenantIdentifier,
                        address: dto.TenantAddress,
                        phoneNumber: dto.TenantPhoneNumber,
                        email: dto.Email
                    );

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
                    _logger.LogInformation("User created via Google signup: {UserId }, Email: {Email }, TenantId: {TenantId }", user.Id, user.Email, tenant.Id);

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
