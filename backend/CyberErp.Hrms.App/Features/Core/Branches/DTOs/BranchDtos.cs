using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Branches.DTOs
{
    public class BranchDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public bool IsHeadOffice { get; set; }
        public bool IsActive { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public bool HasChildren { get; set; }
    }

    public class CreateBranchDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public bool IsHeadOffice { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? ParentId { get; set; }
    }

    public class UpdateBranchDto : CreateBranchDto
    {
        public Guid Id { get; set; }
    }

    public class CreateBranchDtoValidator : AbstractValidator<CreateBranchDto>
    {
        public CreateBranchDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }

    public class UpdateBranchDtoValidator : AbstractValidator<UpdateBranchDto>
    {
        public UpdateBranchDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ParentId)
                .Must((dto, parentId) => parentId != dto.Id)
                .WithMessage("A branch cannot be its own parent.");
        }
    }
}
