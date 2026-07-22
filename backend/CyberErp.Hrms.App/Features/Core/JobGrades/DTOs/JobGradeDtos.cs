using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.JobGrades.DTOs
{
    public class JobGradeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameA { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class CreateJobGradeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? NameA { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class UpdateJobGradeDto : CreateJobGradeDto
    {
        public Guid Id { get; set; }
    }

    public class CreateJobGradeDtoValidator : AbstractValidator<CreateJobGradeDto>
    {
        public CreateJobGradeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.NameA).MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        }
    }

    public class UpdateJobGradeDtoValidator : AbstractValidator<UpdateJobGradeDto>
    {
        public UpdateJobGradeDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.NameA).MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        }
    }
}
