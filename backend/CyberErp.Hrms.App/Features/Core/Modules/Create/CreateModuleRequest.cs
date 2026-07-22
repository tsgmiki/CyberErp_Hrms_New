using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Modules.Create;

public record CreateModuleRequest(Guid SubsystemId, string Name, string? Icon, int SortOrder = 0);

public class CreateModuleRequestValidator : AbstractValidator<CreateModuleRequest>
{
    public CreateModuleRequestValidator()
    {
        RuleFor(x => x.SubsystemId)
            .NotEmpty().WithMessage("Subsystem is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Icon)
            .MaximumLength(100).WithMessage("Icon must not exceed 100 characters.");
    }
}
