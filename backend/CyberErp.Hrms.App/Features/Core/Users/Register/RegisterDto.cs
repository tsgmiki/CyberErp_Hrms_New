using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Users.Register
{
    // Interfaces
    public interface IRegisterUser
    {
        Task<RegisterResult> RegisterAsync(RegisterUserDto dto);
    }

    public interface IRegisterWithGoogle
    {
        Task<RegisterResult> RegisterWithGoogleAsync(RegisterWithGoogleDto dto);
    }

    // DTOs
    // DTOs
    public class RegisterUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        // Tenant Information
        public string TenantName { get; set; } = string.Empty;
        public string? TenantIdentifier { get; set; } = string.Empty;
        public string? TenantAddress { get; set; }
        public string? TenantPhoneNumber { get; set; }
    }

    public class RegisterWithGoogleDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        
        // Tenant Information
        public string TenantName { get; set; } = string.Empty;
        public string TenantIdentifier { get; set; } = string.Empty;
        public string? TenantAddress { get; set; }
        public string? TenantPhoneNumber { get; set; }
        
        // Google Token
        public string GoogleToken { get; set; } = string.Empty;
    }

    public class RegisterResult
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    // Validators
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required.")
                .MaximumLength(200).WithMessage("FullName must not exceed 200 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required.")
                .MaximumLength(100).WithMessage("UserName must not exceed 100 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.TenantName)
                .NotEmpty().WithMessage("TenantName is required.")
                .MaximumLength(200).WithMessage("TenantName must not exceed 200 characters.");

                 }
    }

    public class RegisterWithGoogleDtoValidator : AbstractValidator<RegisterWithGoogleDto>
    {
        public RegisterWithGoogleDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required.")
                .MaximumLength(200).WithMessage("FullName must not exceed 200 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.GoogleToken)
                .NotEmpty().WithMessage("GoogleToken is required.");

            RuleFor(x => x.TenantName)
                .NotEmpty().WithMessage("TenantName is required.")
                .MaximumLength(200).WithMessage("TenantName must not exceed 200 characters.");

            RuleFor(x => x.TenantIdentifier)
                .NotEmpty().WithMessage("TenantIdentifier is required.")
                .MaximumLength(100).WithMessage("TenantIdentifier must not exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9-_]+$").WithMessage("TenantIdentifier can only contain letters, numbers, hyphens, and underscores.");
        }
    }
}
