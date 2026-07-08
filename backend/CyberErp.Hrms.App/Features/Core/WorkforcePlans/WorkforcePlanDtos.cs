using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.WorkforcePlans
{
    // ---- Read DTOs ------------------------------------------------------------

    public class WorkforcePlanLineDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionClassId { get; set; }
        public string? PositionClassTitle { get; set; }
        public string? JobGradeName { get; set; }
        public string? JobCategoryName { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public int PeriodIndex { get; set; }
        public int AuthorizedHeadcount { get; set; }
        public int FilledCount { get; set; }
        public int VacantCount { get; set; }
        public int NewHires { get; set; }
        public int Replacements { get; set; }
        public int TemporaryStaff { get; set; }
        public int MobilityIn { get; set; }
        public int Promotions { get; set; }
        public int ActingAssignments { get; set; }
        public int Retirements { get; set; }
        public int Resignations { get; set; }
        public int ContractExpiries { get; set; }
        public bool IsCriticalRole { get; set; }
        public string? RequiredCompetencies { get; set; }
        public decimal AnnualSalaryCost { get; set; }
        public decimal AnnualAllowances { get; set; }
        public decimal AnnualBenefits { get; set; }
        // Computed by the domain (projections, gap and cost — HC062/HC064)
        public int EndHeadcount { get; set; }
        public int HeadcountGap { get; set; }
        public decimal LineCost { get; set; }
        public string? Remark { get; set; }
    }

    public class WorkforcePlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Horizon { get; set; } = string.Empty;
        public string Scenario { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid StartFiscalYearId { get; set; }
        public string? StartFiscalYearName { get; set; }
        public int PeriodCount { get; set; }
        public int Version { get; set; }
        public Guid? RootPlanId { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal BudgetThresholdPercent { get; set; }
        public string? EscalationJustification { get; set; }
        public decimal ProjectedCost { get; set; }
        /// <summary>TotalBudget − ProjectedCost (negative = over budget, HC065).</summary>
        public decimal BudgetVariance { get; set; }
        /// <summary>Cost beyond budget + threshold — &gt; 0 forces an escalation justification (HC066).</summary>
        public decimal ExcessBeyondThreshold { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool AwaitingWorkflow { get; set; }
        public List<WorkforcePlanLineDto> Lines { get; set; } = [];
    }

    // ---- Write DTOs -----------------------------------------------------------

    public class SaveWorkforcePlanLineDto
    {
        public Guid OrganizationUnitId { get; set; }
        public Guid PositionClassId { get; set; }
        public string EmploymentType { get; set; } = nameof(PlannedEmploymentType.Permanent);
        public int PeriodIndex { get; set; }
        public int AuthorizedHeadcount { get; set; }
        public int FilledCount { get; set; }
        public int VacantCount { get; set; }
        public int NewHires { get; set; }
        public int Replacements { get; set; }
        public int TemporaryStaff { get; set; }
        public int MobilityIn { get; set; }
        public int Promotions { get; set; }
        public int ActingAssignments { get; set; }
        public int Retirements { get; set; }
        public int Resignations { get; set; }
        public int ContractExpiries { get; set; }
        public bool IsCriticalRole { get; set; }
        public string? RequiredCompetencies { get; set; }
        /// <summary>0 = default from the role's salary scale (monthly salary × 12) at save time.</summary>
        public decimal AnnualSalaryCost { get; set; }
        public decimal AnnualAllowances { get; set; }
        public decimal AnnualBenefits { get; set; }
        public string? Remark { get; set; }
    }

    public class SaveWorkforcePlanDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Horizon { get; set; } = nameof(PlanningHorizon.Annual);
        public string Scenario { get; set; } = nameof(WorkforcePlanScenario.Baseline);
        public Guid? OrganizationUnitId { get; set; }
        public Guid StartFiscalYearId { get; set; }
        public int PeriodCount { get; set; } = 1;
        public decimal TotalBudget { get; set; }
        public decimal BudgetThresholdPercent { get; set; }
        public List<SaveWorkforcePlanLineDto> Lines { get; set; } = [];
    }

    public class SubmitWorkforcePlanDto
    {
        public Guid Id { get; set; }
        /// <summary>Required when the projected cost breaches budget + threshold (HC066).</summary>
        public string? EscalationJustification { get; set; }
    }

    public class SaveWorkforcePlanDtoValidator : AbstractValidator<SaveWorkforcePlanDto>
    {
        public SaveWorkforcePlanDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Horizon)
                .Must(v => Enum.TryParse<PlanningHorizon>(v, true, out _))
                .WithMessage("Horizon must be Annual, MediumTerm or MultiYear.");
            RuleFor(x => x.Scenario)
                .Must(v => Enum.TryParse<WorkforcePlanScenario>(v, true, out _))
                .WithMessage("Scenario must be Baseline, Growth, Contraction or Restructuring.");
            RuleFor(x => x.StartFiscalYearId).NotEmpty().WithMessage("A starting fiscal year is required.");
            RuleFor(x => x.PeriodCount).InclusiveBetween(1, 10)
                .WithMessage("The planning horizon must span 1–10 fiscal-year periods.");
            RuleFor(x => x.TotalBudget).GreaterThanOrEqualTo(0);
            RuleFor(x => x.BudgetThresholdPercent).InclusiveBetween(0, 100);
            RuleForEach(x => x.Lines).ChildRules(line =>
            {
                line.RuleFor(l => l.OrganizationUnitId).NotEmpty();
                line.RuleFor(l => l.PositionClassId).NotEmpty();
                line.RuleFor(l => l.EmploymentType)
                    .Must(v => Enum.TryParse<PlannedEmploymentType>(v, true, out _))
                    .WithMessage("EmploymentType must be Permanent, Contract, Intern or Consultant.");
                line.RuleFor(l => l.PeriodIndex).GreaterThanOrEqualTo(0);
                line.RuleFor(l => l.RequiredCompetencies).MaximumLength(1000);
                line.RuleFor(l => l.Remark).MaximumLength(1000);
                line.RuleFor(l => l.AnnualSalaryCost).GreaterThanOrEqualTo(0);
                line.RuleFor(l => l.AnnualAllowances).GreaterThanOrEqualTo(0);
                line.RuleFor(l => l.AnnualBenefits).GreaterThanOrEqualTo(0);
            });
        }
    }

    // ---- Analytics DTOs ---------------------------------------------------------

    /// <summary>One establishment row: authorized / filled / vacant per unit × role (HC056).</summary>
    public class EstablishmentRowDto
    {
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionClassId { get; set; }
        public string? PositionClassTitle { get; set; }
        public string? JobGradeName { get; set; }
        public string? JobCategoryName { get; set; }
        public int Authorized { get; set; }
        public int Filled { get; set; }
        public int Vacant { get; set; }
        /// <summary>Approximate average days the vacant seats have been open (vacancy aging, HC073).</summary>
        public int? AvgVacantDays { get; set; }
    }

    /// <summary>Suggested per-line retirement counts inside the plan horizon (HC060).</summary>
    public class SeparationSuggestionDto
    {
        public Guid OrganizationUnitId { get; set; }
        public Guid PositionClassId { get; set; }
        public int Retirements { get; set; }
    }

    /// <summary>Per-period aggregate of one plan (time-phased projection, HC069/HC073).</summary>
    public class WorkforcePlanPeriodSummaryDto
    {
        public int PeriodIndex { get; set; }
        public int EndHeadcount { get; set; }
        public int Demand { get; set; }
        public int Supply { get; set; }
        public int Separations { get; set; }
        public decimal Cost { get; set; }
    }

    public class WorkforcePlanSummaryDto
    {
        public Guid PlanId { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal ProjectedCost { get; set; }
        public decimal BudgetVariance { get; set; }
        public decimal ExcessBeyondThreshold { get; set; }
        public int TotalEndHeadcount { get; set; }
        public int TotalGap { get; set; }
        public int CriticalRoles { get; set; }
        public List<WorkforcePlanPeriodSummaryDto> Periods { get; set; } = [];
    }

    /// <summary>One plan's figures in a scenario comparison (HC068).</summary>
    public class WorkforcePlanComparisonDto
    {
        public Guid PlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Scenario { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Version { get; set; }
        public int TotalEndHeadcount { get; set; }
        public int TotalDemand { get; set; }
        public int TotalSeparations { get; set; }
        public int TotalGap { get; set; }
        public int CriticalRoles { get; set; }
        public decimal ProjectedCost { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal BudgetVariance { get; set; }
    }

    /// <summary>Outstanding approved hiring demand — the recruitment-module feed (HC075).</summary>
    public class ApprovedDemandRowDto
    {
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionClassId { get; set; }
        public string? PositionClassTitle { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public int PeriodIndex { get; set; }
        public int NewHires { get; set; }
        public int Replacements { get; set; }
        public int TemporaryStaff { get; set; }
        public bool IsCriticalRole { get; set; }
        public string? RequiredCompetencies { get; set; }
    }
}
