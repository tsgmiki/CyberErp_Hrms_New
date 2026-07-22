using FluentValidation;

namespace CyberErp.Hrms.App.Common.DTOs
{
    public class GetAllRequestValidator : AbstractValidator<GetAllRequest>
    {
        public GetAllRequestValidator()
        {
           RuleFor(x => x.Skip)
                .NotEmpty().WithMessage("Skip is required.")
                .MaximumLength(50).WithMessage("Skip must not exceed 50 characters.");

            RuleFor(x => x.Take)
                .NotEmpty().WithMessage("Take is required.")
                .MaximumLength(200).WithMessage("Take must not exceed 50 characters.");

            RuleFor(x => x.SortCol)
                .MaximumLength(100).WithMessage("SortCol must not exceed 100 characters.");
            RuleFor(x => x.Dir)
                 .MaximumLength(100).WithMessage("SortCol must not exceed 100 characters.");
            RuleFor(x => x.SearchText)
                       .MaximumLength(100).WithMessage("SearchText must not exceed 100 characters.");
            
            // Validation for sales report filters
            RuleFor(x => x.FromDate)
                .MaximumLength(50).WithMessage("FromDate must not exceed 50 characters.");
                 RuleFor(x => x.ToDate)
                .MaximumLength(50).WithMessage("ToDate must not exceed 50 characters.");
          }
    }
    public class GetAllRequest
    {
        public string? Skip { get; set; }
        public string? Take { get; set; }
        public string? SortCol { get; set; }
        public string? Dir { get; set; }
        public string? SearchText { get; set; }
        public Guid? CategoryId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? ReportName { get; set; }
        public string? ReportCategory { get; set; }
        public string? BatchNumber { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ItemId { get; set; }
        public string? VoucherType { get; set; }
        public bool? IsDirect { get; set; }

        // Organizational Structure hierarchy filters (HRMS §3.1)
        public Guid? ParentId { get; set; }
        public bool? IsRoot { get; set; }
        public string? Status { get; set; }

        /// <summary>When true, restrict positions to vacant (open) ones — used by the employee form.</summary>
        public bool? IsVacant { get; set; }

        /// <summary>Filters salary-scale rows to a single job grade.</summary>
        public Guid? JobGradeId { get; set; }

        /// <summary>Filters records (e.g. leave requests/balances) to a single employee.</summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>Filters custom-field definitions to a single owner form (Employee/Education/…).</summary>
        public string? OwnerType { get; set; }

        /// <summary>Filters dynamic forms to a single owning module (e.g. "Employee").</summary>
        public string? Module { get; set; }

        /// <summary>Filters performance records (objectives/goals) to a single review cycle.</summary>
        public Guid? ReviewCycleId { get; set; }

        /// <summary>Filters employee goals to a single linked organizational objective.</summary>
        public Guid? ObjectiveId { get; set; }

        /// <summary>Filters organizational objectives to a single owning organization unit.</summary>
        public Guid? OrganizationUnitId { get; set; }

        /// <summary>Filters recognitions to public ones (the recognition board feed).</summary>
        public bool? IsPublic { get; set; }

        /// <summary>Appraisal worklist: restrict to records the current user must act on at their current stage.</summary>
        public bool? AssignedToMe { get; set; }

        /// <summary>Filters personnel actions to one movement type (Transfer / Promotion / Demotion).</summary>
        public string? MovementType { get; set; }

        /// <summary>Filters training needs to one type (Local / Abroad).</summary>
        public string? NeedType { get; set; }

        /// <summary>Filters training sessions to one catalog course.</summary>
        public Guid? CourseId { get; set; }

        /// <summary>Filters training enrollments to one session.</summary>
        public Guid? SessionId { get; set; }

        /// <summary>Filters communities to one kind (Learning / InterestGroup / Club).</summary>
        public string? Kind { get; set; }
    }
}

