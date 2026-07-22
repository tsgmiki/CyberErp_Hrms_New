using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Positions.DTOs
{
    public class PositionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;

        public Guid PositionClassId { get; set; }
        public string? PositionClassTitle { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public bool IsVacant { get; set; } = true;
    }

    public class CreatePositionDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid PositionClassId { get; set; }
        public Guid OrganizationUnitId { get; set; }
    }

    public class UpdatePositionDto : CreatePositionDto
    {
        public Guid Id { get; set; }
    }

    public class CreatePositionDtoValidator : AbstractValidator<CreatePositionDto>
    {
        public CreatePositionDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.PositionClassId).NotEmpty().WithMessage("Position class is required.");
            RuleFor(x => x.OrganizationUnitId).NotEmpty().WithMessage("Organization unit is required.");
        }
    }

    public class UpdatePositionDtoValidator : AbstractValidator<UpdatePositionDto>
    {
        public UpdatePositionDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.PositionClassId).NotEmpty().WithMessage("Position class is required.");
            RuleFor(x => x.OrganizationUnitId).NotEmpty().WithMessage("Organization unit is required.");
        }
    }
}
