using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Operations.DTOs
{
    public class OperationDtoValidator : AbstractValidator<OperationDto>
    {
        public OperationDtoValidator()
        {
            RuleFor(x => x.ModuleId)
                .NotEmpty().WithMessage("ModuleId is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Link)
                .NotEmpty().WithMessage("Link is required.")
                .MaximumLength(500).WithMessage("Link must not exceed 500 characters.");

            RuleFor(x => x.Filter)
                .MaximumLength(500).WithMessage("Filter must not exceed 500 characters.");

            RuleFor(x => x.Icon)
                .MaximumLength(100).WithMessage("Icon must not exceed 100 characters.");
        }
    }

    public class UpdateOperationDtoValidator : AbstractValidator<UpdateOperationDto>
    {
        public UpdateOperationDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.ModuleId)
                .NotEmpty().WithMessage("ModuleId is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Link)
                .NotEmpty().WithMessage("Link is required.")
                .MaximumLength(500).WithMessage("Link must not exceed 500 characters.");

            RuleFor(x => x.Filter)
                .MaximumLength(500).WithMessage("Filter must not exceed 500 characters.");

            RuleFor(x => x.Icon)
                .MaximumLength(100).WithMessage("Icon must not exceed 100 characters.");
        }
    }

    public class OperationDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
    }

    public class UpdateOperationDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }

    public class OperationResult
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedAt { get; set; }
    }
}

