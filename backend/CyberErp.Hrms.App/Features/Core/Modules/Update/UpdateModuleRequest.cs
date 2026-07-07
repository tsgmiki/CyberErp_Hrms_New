using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Modules.Update;

public record UpdateModuleRequest(Guid Id, string SubSystem, string Name, string? Icon);

public class UpdateModuleRequestValidator : AbstractValidator<UpdateModuleRequest>
{
    public UpdateModuleRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

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