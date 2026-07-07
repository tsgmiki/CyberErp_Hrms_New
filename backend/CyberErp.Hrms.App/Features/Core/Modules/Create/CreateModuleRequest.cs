using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Modules.Create;

public record CreateModuleRequest(string SubSystem, string Name, string? Icon);

public class CreateModuleRequestValidator : AbstractValidator<CreateModuleRequest>
{
    public CreateModuleRequestValidator()
    {
        RuleFor(x => x.SubSystem)
            .NotEmpty().WithMessage("SubSystem is required.")
            .MaximumLength(200).WithMessage("SubSystem must not exceed 200 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Icon)
            .MaximumLength(100).WithMessage("Icon must not exceed 100 characters.");
    }
}