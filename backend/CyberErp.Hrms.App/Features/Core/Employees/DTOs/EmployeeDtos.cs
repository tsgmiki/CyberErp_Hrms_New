using FluentValidation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.App.Features.Core.Employees.DTOs
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;

        // Person (Core.CorePerson) — Ethiopian naming with Amharic variants
        public string FirstName { get; set; } = string.Empty;
        public string? FirstNameA { get; set; }
        public string? FatherName { get; set; }
        public string? FatherNameA { get; set; }
        public string GrandFatherName { get; set; } = string.Empty;
        public string? GrandFatherNameA { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public Guid? NationalityId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LocationName { get; set; }

        // Employment record
        public string EmploymentStatus { get; set; } = string.Empty;
        public string EmploymentNature { get; set; } = nameof(Dom.Entities.Core.EmploymentNature.Permanent);
        public int? ContractPeriod { get; set; }
        public bool IsProbation { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public bool IsTerminated { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? SpouseName { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }
        public string? NationalId { get; set; }
        public string? Tin { get; set; }
        public string? PensionNumber { get; set; }
        public DateTime? HireDate { get; set; }
        public Guid? JobGradeId { get; set; }
        public string? JobGradeName { get; set; }
        public Guid? SalaryScaleId { get; set; }
        /// <summary>Display label of the linked salary scale (its step name).</summary>
        public string? SalaryScaleStep { get; set; }
        public decimal? SalaryScaleAmount { get; set; }
        public decimal? Salary { get; set; }
        /// <summary>Managerial staff receive the managerial annual-leave entitlement and may head units.</summary>
        public bool IsManagerial { get; set; }

        public Guid? PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionClassTitle { get; set; }
        /// <summary>Derived from the position's organization unit (not stored on the employee).</summary>
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }

        /// <summary>Dynamic field values keyed by field definition name (HC021).</summary>
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }

    public class CreateEmployeeDto
    {
        public string EmployeeNumber { get; set; } = string.Empty;

        // Person fields — saved transactionally with the employment record
        public string FirstName { get; set; } = string.Empty;
        public string? FirstNameA { get; set; }
        public string? FatherName { get; set; }
        public string? FatherNameA { get; set; }
        public string GrandFatherName { get; set; } = string.Empty;
        public string? GrandFatherNameA { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public Guid? NationalityId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LocationName { get; set; }

        // Employment fields
        public string EmploymentStatus { get; set; } = nameof(Dom.Entities.Core.EmploymentStatus.Active);
        public string EmploymentNature { get; set; } = nameof(Dom.Entities.Core.EmploymentNature.Permanent);
        public int? ContractPeriod { get; set; }
        public bool IsProbation { get; set; }
        /// <summary>Managerial staff receive the managerial annual-leave entitlement and may head units.</summary>
        public bool IsManagerial { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? SpouseName { get; set; }
        public string? Email { get; set; }
        public string? NationalId { get; set; }
        public string? Tin { get; set; }
        public string? PensionNumber { get; set; }
        public DateTime? HireDate { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? SalaryScaleId { get; set; }
        public decimal? Salary { get; set; }

        /// <summary>Dynamic field values keyed by field definition name (HC021).</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class UpdateEmployeeDto : CreateEmployeeDto
    {
        public Guid Id { get; set; }
    }

    public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeDtoValidator()
        {
            RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.GrandFatherName).NotEmpty().WithMessage("Grandfather name is required.").MaximumLength(100);
            RuleFor(x => x.FatherName).MaximumLength(100);
            RuleFor(x => x.FirstNameA).MaximumLength(100);
            RuleFor(x => x.FatherNameA).MaximumLength(100);
            RuleFor(x => x.GrandFatherNameA).MaximumLength(100);
            RuleFor(x => x.Gender).NotEmpty()
                .Must(v => Enum.TryParse<Gender>(v, out _)).WithMessage("Gender must be Male or Female.");
            RuleFor(x => x.MaritalStatus).NotEmpty()
                .Must(v => Enum.TryParse<MaritalStatus>(v, out _))
                .WithMessage("MaritalStatus must be one of: Single, Married, Divorced, Widowed.");
            RuleFor(x => x.EmploymentStatus).NotEmpty()
                .Must(v => Enum.TryParse<EmploymentStatus>(v, out _))
                .WithMessage("EmploymentStatus must be one of: Active, Probation, OnLeave, Suspended, Terminated, Retired.");
            RuleFor(x => x.EmploymentNature).NotEmpty()
                .Must(v => Enum.TryParse<EmploymentNature>(v, out _))
                .WithMessage("EmploymentNature must be Permanent or Contract.");
            // Conditional requirements mirroring the form logic.
            RuleFor(x => x.ContractPeriod).NotNull().GreaterThan(0)
                .When(x => x.EmploymentNature == nameof(EmploymentNature.Contract))
                .WithMessage("Contract period (months) is required for contract employees.");
            RuleFor(x => x.ProbationEndDate).NotNull()
                .When(x => x.IsProbation)
                .WithMessage("Probation end date is required when the employee is on probation.");
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
            RuleFor(x => x.Salary).GreaterThanOrEqualTo(0).When(x => x.Salary.HasValue);
        }
    }

    public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
    {
        public UpdateEmployeeDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            Include(new CreateEmployeeDtoValidator());
        }
    }
}
