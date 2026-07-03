using FluentValidation;
using CyberErp.Hrms.App.Features.Core.Users.DTOs;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Users.Login
{
    public class LoginUser(
        ILoginRepository loginRepository,
        IValidator<LoginUserDto> validator,
        ILogger<LoginUser> logger) : ILoginUser
    {
        private readonly ILoginRepository _loginRepository = loginRepository;
        private readonly IValidator<LoginUserDto> _validator = validator;
        private readonly ILogger<LoginUser> _logger = logger;

        public async Task<UserResult> Loginsync(LoginUserDto dto)
        {
            _logger.LogInformation("Login with UserName: {UserName}", dto.UserName);

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Login failed validation for UserName: {UserName}", dto.UserName);
                throw new ValidationException(validationResult.Errors);
            }
            var result= await _loginRepository.Loginsync(dto);
            _logger.LogInformation("User successfully logged in with UserName: {UserName}", dto.UserName);

            return result;
        }

    }
}

