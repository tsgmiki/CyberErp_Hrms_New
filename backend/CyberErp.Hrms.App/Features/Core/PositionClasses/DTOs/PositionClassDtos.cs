using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.PositionClasses.DTOs
{
    public class PositionClassDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int AllocatedHeadcount { get; set; }
        public string? MinQualifications { get; set; }
        public int? MinExperienceYears { get; set; }
        public string? Skills { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }
        public decimal? WeeklyWorkingHours { get; set; }

        // Pay point: the position class links to a salary scale (grade + step + exact salary).
        public Guid SalaryScaleId { get; set; }
        public Guid JobGradeId { get; set; }          // derived from the salary scale, for the UI grade filter
        public string? JobGradeName { get; set; }
        public string? SalaryStep { get; set; }
        public decimal? Salary { get; set; }

        public Guid JobCategoryId { get; set; }
        public string? JobCategoryName { get; set; }
        public Guid? WorkLocationId { get; set; }
        public string? WorkLocationName { get; set; }
        public Guid? ReportsToPositionClassId { get; set; }
        public string? ReportsToPositionClassTitle { get; set; }
    }

    public class CreatePositionClassDto
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int AllocatedHeadcount { get; set; } = 1;
        public string? MinQualifications { get; set; }
        public int? MinExperienceYears { get; set; }
        public string? Skills { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }
        public decimal? WeeklyWorkingHours { get; set; }

        public Guid SalaryScaleId { get; set; }
        public Guid JobCategoryId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? ReportsToPositionClassId { get; set; }
    }

    public class UpdatePositionClassDto : CreatePositionClassDto
    {
        public Guid Id { get; set; }
    }

    public class CreatePositionClassDtoValidator : AbstractValidator<CreatePositionClassDto>
    {
        public CreatePositionClassDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.AllocatedHeadcount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.SalaryScaleId).NotEmpty().WithMessage("Salary scale is required.");
            RuleFor(x => x.JobCategoryId).NotEmpty().WithMessage("Job category is required.");
            RuleFor(x => x.MinExperienceYears).GreaterThanOrEqualTo(0).When(x => x.MinExperienceYears.HasValue);
            ApplyAgeAndHoursRules(this);
        }

        internal static void ApplyAgeAndHoursRules<T>(AbstractValidator<T> v) where T : CreatePositionClassDto
        {
            v.RuleFor(x => x.MinimumAge).GreaterThanOrEqualTo(0).When(x => x.MinimumAge.HasValue);
            v.RuleFor(x => x.MaximumAge).GreaterThanOrEqualTo(0).When(x => x.MaximumAge.HasValue);
            v.RuleFor(x => x.MaximumAge)
                .GreaterThanOrEqualTo(x => x.MinimumAge!.Value)
                .When(x => x.MinimumAge.HasValue && x.MaximumAge.HasValue)
                .WithMessage("Maximum age must be greater than or equal to minimum age.");
            v.RuleFor(x => x.WeeklyWorkingHours)
                .InclusiveBetween(0, 168)
                .When(x => x.WeeklyWorkingHours.HasValue)
                .WithMessage("Weekly working hours must be between 0 and 168.");
        }
    }

    public class UpdatePositionClassDtoValidator : AbstractValidator<UpdatePositionClassDto>
    {
        public UpdatePositionClassDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.AllocatedHeadcount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.SalaryScaleId).NotEmpty().WithMessage("Salary scale is required.");
            RuleFor(x => x.JobCategoryId).NotEmpty().WithMessage("Job category is required.");
            RuleFor(x => x.MinExperienceYears).GreaterThanOrEqualTo(0).When(x => x.MinExperienceYears.HasValue);
            RuleFor(x => x.ReportsToPositionClassId)
                .Must((dto, rep) => rep != dto.Id)
                .WithMessage("A position class cannot report to itself.");
            CreatePositionClassDtoValidator.ApplyAgeAndHoursRules(this);
        }
    }
}
