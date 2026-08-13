using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Operations.Create;

/// <summary>
/// Creates a menu entry. <paramref name="ModuleId"/> is the PARENT group's id; leave it null to
/// create a group itself, in which case <paramref name="SubsystemId"/> is required (a group has no
/// parent to inherit a subsystem from).
/// </summary>
public record CreateOperationRequest(
    Guid? ModuleId,
    string Name,
    string Link,
    string Filter,
    string Icon,
    int SortOrder = 0,
    Guid? SubsystemId = null);

public class CreateOperationRequestValidator : AbstractValidator<CreateOperationRequest>
{
    public CreateOperationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        // A group is not navigable, so it has no link — and must not have one, or the permission gate
        // would treat it as a screen and start granting access to it.
        RuleFor(x => x.Link)
            .NotEmpty().WithMessage("Link is required.")
            .MaximumLength(200).WithMessage("Link must not exceed 200 characters.")
            .When(x => x.ModuleId.HasValue && x.ModuleId != Guid.Empty);

        RuleFor(x => x.Filter)
            .MaximumLength(200).WithMessage("Filter must not exceed 200 characters.");

        RuleFor(x => x.Icon)
            .MaximumLength(100).WithMessage("Icon must not exceed 100 characters.");
    }
}
