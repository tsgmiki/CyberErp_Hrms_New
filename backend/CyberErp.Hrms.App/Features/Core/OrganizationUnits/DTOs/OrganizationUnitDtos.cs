using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.OrganizationUnits.DTOs
{
    public class OrganizationUnitDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public Guid? WorkLocationId { get; set; }
        public string? WorkLocationName { get; set; }
        public int? AllocatedHeadcount { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool HasChildren { get; set; }
    }

    /// <summary>Nested node used to render the read-only org chart (HC001/HC013).</summary>
    public class OrgUnitTreeNodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public int? AllocatedHeadcount { get; set; }
        public List<OrgUnitTreeNodeDto> Children { get; set; } = new();
    }

    public class CreateOrganizationUnitDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public int? AllocatedHeadcount { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateOrganizationUnitDto : CreateOrganizationUnitDto
    {
        public Guid Id { get; set; }
    }

    public class CreateOrganizationUnitDtoValidator : AbstractValidator<CreateOrganizationUnitDto>
    {
        public CreateOrganizationUnitDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.UnitType)
                .NotEmpty()
                .Must(v => Enum.TryParse<OrganizationUnitType>(v, out _))
                .WithMessage("UnitType must be one of: BusinessUnit, Directorate, Division, Department, Team.");
            RuleFor(x => x.AllocatedHeadcount).GreaterThanOrEqualTo(0).When(x => x.AllocatedHeadcount.HasValue);
        }
    }

    public class UpdateOrganizationUnitDtoValidator : AbstractValidator<UpdateOrganizationUnitDto>
    {
        public UpdateOrganizationUnitDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.UnitType)
                .NotEmpty()
                .Must(v => Enum.TryParse<OrganizationUnitType>(v, out _))
                .WithMessage("UnitType must be one of: BusinessUnit, Directorate, Division, Department, Team.");
            RuleFor(x => x.AllocatedHeadcount).GreaterThanOrEqualTo(0).When(x => x.AllocatedHeadcount.HasValue);
            RuleFor(x => x.ParentId)
                .Must((dto, parentId) => parentId != dto.Id)
                .WithMessage("An organization unit cannot be its own parent.");
        }
    }
}
