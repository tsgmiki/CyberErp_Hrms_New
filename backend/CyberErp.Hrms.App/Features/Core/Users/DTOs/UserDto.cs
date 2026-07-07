using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Users.DTOs
{
    public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
    {
        public LoginUserDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required.")
                .MaximumLength(200).WithMessage("UserName must not exceed 200 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MaximumLength(200).WithMessage("Password must not exceed 200 characters.");
        }
    }

    public class LoginUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? TenantId { get; set; }
    }

    public class UserResult
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsHeadOffice { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
