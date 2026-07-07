using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.JobCategories.DTOs
{
    public class JobCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateJobCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateJobCategoryDto : CreateJobCategoryDto
    {
        public Guid Id { get; set; }
    }

    public class CreateJobCategoryDtoValidator : AbstractValidator<CreateJobCategoryDto>
    {
        public CreateJobCategoryDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        }
    }

    public class UpdateJobCategoryDtoValidator : AbstractValidator<UpdateJobCategoryDto>
    {
        public UpdateJobCategoryDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        }
    }
}
