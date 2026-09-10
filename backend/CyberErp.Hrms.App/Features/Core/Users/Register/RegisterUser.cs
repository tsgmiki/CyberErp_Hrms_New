using FluentValidation;
using CyberErp.Hrms.App.Features.Core.Users.Register;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Users.Register
{
    public class RegisterUser(
        IRegisterRepository registerRepository,
        IValidator<RegisterUserDto> validator,
        Common.Services.IPasswordPolicyService passwordPolicy,
        ILogger<RegisterUser> logger) : IRegisterUser
    {
        private readonly IRegisterRepository _registerRepository = registerRepository;
        private readonly IValidator<RegisterUserDto> _validator = validator;
        private readonly ILogger<RegisterUser> _logger = logger;

        public async Task<RegisterResult> RegisterAsync(RegisterUserDto dto)
        {
            _logger.LogInformation("Registering new user: {UserName}, Tenant: {TenantName}", dto.UserName, dto.TenantName);

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Registration failed validation for UserName: {UserName}", dto.UserName);
                throw new ValidationException(validationResult.Errors);
            }

            // ⚠️ The CONFIGURED policy, not the validator's hardcoded six-character floor. Until now
            // MinimumPasswordLength, RequireUppercase, RequireNumbers and RequireSpecialCharacters
            // were settable on the Settings screen and read by nothing at all (logic §12.90).
            await passwordPolicy.EnsureMeetsPolicyAsync(dto.Password);

            var result = await _registerRepository.RegisterAsync(dto);
            _logger.LogInformation("User successfully registered with UserName: {UserName}, TenantId: {TenantId}", dto.UserName, result.TenantId);

            return result;
        }
    }

    public class RegisterWithGoogle(
        IRegisterRepository registerRepository,
        IValidator<RegisterWithGoogleDto> validator,
        ILogger<RegisterWithGoogle> logger) : IRegisterWithGoogle
    {
        private readonly IRegisterRepository _registerRepository = registerRepository;
        private readonly IValidator<RegisterWithGoogleDto> _validator = validator;
        private readonly ILogger<RegisterWithGoogle> _logger = logger;

        public async Task<RegisterResult> RegisterWithGoogleAsync(RegisterWithGoogleDto dto)
        {
            _logger.LogInformation("Registering new user with Google: {Email}, Tenant: {TenantName}", dto.Email, dto.TenantName);

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Google registration failed validation for Email: {Email}", dto.Email);
                throw new ValidationException(validationResult.Errors);
            }

            var result = await _registerRepository.RegisterWithGoogleAsync(dto);
            _logger.LogInformation("User successfully registered with Google: {Email}, TenantId: {TenantId}", dto.Email, result.TenantId);

            return result;
        }
    }
}
