using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.WorkLocations.DTOs
{
    public class WorkLocationDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool HasChildren { get; set; }
    }

    public class CreateWorkLocationDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkLocationDto : CreateWorkLocationDto
    {
        public Guid Id { get; set; }
    }

    public class CreateWorkLocationDtoValidator : AbstractValidator<CreateWorkLocationDto>
    {
        public CreateWorkLocationDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.LocationType)
                .NotEmpty()
                .Must(v => Enum.TryParse<WorkLocationType>(v, out _))
                .WithMessage("LocationType must be one of: Country, Region, City, Office.");
        }
    }

    public class UpdateWorkLocationDtoValidator : AbstractValidator<UpdateWorkLocationDto>
    {
        public UpdateWorkLocationDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.LocationType)
                .NotEmpty()
                .Must(v => Enum.TryParse<WorkLocationType>(v, out _))
                .WithMessage("LocationType must be one of: Country, Region, City, Office.");
            RuleFor(x => x.ParentId)
                .Must((dto, parentId) => parentId != dto.Id)
                .WithMessage("A work location cannot be its own parent.");
        }
    }
}
