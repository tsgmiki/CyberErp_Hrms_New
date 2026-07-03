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
    }
}

