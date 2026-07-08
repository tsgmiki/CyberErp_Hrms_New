using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Planning horizon of a workforce plan (HC053).</summary>
public enum PlanningHorizon
{
    Annual = 0,        // one fiscal year
    MediumTerm = 1,    // 2–3 fiscal years
    MultiYear = 2      // 4+ fiscal years
}

/// <summary>Planning scenario a plan models (HC067). Scenarios of one exercise share a RootPlanId chain.</summary>
public enum WorkforcePlanScenario
{
    Baseline = 0,
    Growth = 1,
    Contraction = 2,
    Restructuring = 3
}

/// <summary>
/// Lifecycle of a workforce plan: drafted → submitted (routed through the generic workflow
/// engine, HC070) → approved / rejected. Approved plans feed recruitment demand (HC075);
/// superseded ones are archived. Rejected plans stay editable and can be resubmitted.
/// </summary>
public enum WorkforcePlanStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Archived = 4
}

/// <summary>
/// Employment type a workforce-plan line plans for (HC057). Deliberately a planning-level enum —
/// broader than the employee master's EmploymentNature (interns/consultants are planned before any
/// employment record exists), so the Employee entity is not touched.
/// </summary>
public enum PlannedEmploymentType
{
    Permanent = 0,
    Contract = 1,
    Intern = 2,
    Consultant = 3
}

/// <summary>
/// Workforce plan (HC053–HC076): a versioned, scenario-tagged headcount/cost plan anchored to the
/// approved organizational structure. Scoped organization-wide or to one unit's subtree (HC054);
/// time-phased over <see cref="PeriodCount"/> fiscal years from <see cref="StartFiscalYearId"/>
/// (HC069). Carries the approved budget envelope and escalation threshold (HC065/HC066); the
/// projected cost is denormalized from the lines for list views. Approval routes through the
/// generic workflow engine (entity type "WorkforcePlan"); revising an approved/archived plan
/// creates a new Draft version linked via <see cref="RootPlanId"/> (HC071).
/// </summary>
public class WorkforcePlan : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public PlanningHorizon Horizon { get; private set; }
    public WorkforcePlanScenario Scenario { get; private set; }
    public WorkforcePlanStatus Status { get; private set; } = WorkforcePlanStatus.Draft;

    /// <summary>Planning scope: null = organization-wide, else the unit (and its subtree) planned for.</summary>
    public Guid? OrganizationUnitId { get; private set; }

    /// <summary>First fiscal year of the horizon; periods are consecutive fiscal years from here.</summary>
    public Guid StartFiscalYearId { get; private set; }
    /// <summary>Number of fiscal-year periods in the horizon (1 = annual).</summary>
    public int PeriodCount { get; private set; } = 1;

    // Version control (HC071): first version has RootPlanId = null; revisions point at the
    // original plan's id, so a chain groups by (RootPlanId ?? Id).
    public int Version { get; private set; } = 1;
    public Guid? RootPlanId { get; private set; }

    // Budget control (HC064–HC066)
    /// <summary>Approved budget envelope for the whole horizon (0 = no budget captured).</summary>
    public decimal TotalBudget { get; private set; }
    /// <summary>Allowed overrun percentage before submission requires an escalation justification.</summary>
    public decimal BudgetThresholdPercent { get; private set; }
    /// <summary>Mandatory justification when the projected cost exceeds budget + threshold (HC066).</summary>
    public string? EscalationJustification { get; private set; }
    /// <summary>Denormalized Σ line costs — recomputed whenever the lines change.</summary>
    public decimal ProjectedCost { get; private set; }

    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    private readonly List<WorkforcePlanLine> _lines = [];
    public IReadOnlyCollection<WorkforcePlanLine> Lines => _lines;

    private WorkforcePlan() : base() { }

    public static WorkforcePlan Create(
        string name,
        PlanningHorizon horizon,
        WorkforcePlanScenario scenario,
        Guid startFiscalYearId,
        int periodCount,
        Guid? organizationUnitId = null,
        decimal totalBudget = 0,
        decimal budgetThresholdPercent = 0,
        string? description = null)
    {
        Guard(name, startFiscalYearId, periodCount, totalBudget, budgetThresholdPercent);
        return new WorkforcePlan
        {
            Name = name,
            Horizon = horizon,
            Scenario = scenario,
            StartFiscalYearId = startFiscalYearId,
            PeriodCount = periodCount,
            OrganizationUnitId = organizationUnitId,
            TotalBudget = totalBudget,
            BudgetThresholdPercent = budgetThresholdPercent,
            Description = description
        };
    }

    /// <summary>Header corrections — only while the plan is editable (Draft or Rejected).</summary>
    public void Update(
        string name,
        PlanningHorizon horizon,
        WorkforcePlanScenario scenario,
        Guid startFiscalYearId,
        int periodCount,
        Guid? organizationUnitId,
        decimal totalBudget,
        decimal budgetThresholdPercent,
        string? description)
    {
        EnsureEditable();
        Guard(name, startFiscalYearId, periodCount, totalBudget, budgetThresholdPercent);
        Name = name;
        Horizon = horizon;
        Scenario = scenario;
        StartFiscalYearId = startFiscalYearId;
        PeriodCount = periodCount;
        OrganizationUnitId = organizationUnitId;
        TotalBudget = totalBudget;
        BudgetThresholdPercent = budgetThresholdPercent;
        Description = description;
        base.Update();
    }

    /// <summary>Replaces the plan lines (the editor posts the whole grid) and recomputes the cost.</summary>
    public void SetLines(IEnumerable<WorkforcePlanLineSpec> lines)
    {
        EnsureEditable();
        _lines.Clear();
        foreach (var spec in lines)
            _lines.Add(WorkforcePlanLine.Create(Id, spec));
        RecomputeProjectedCost();
        base.Update();
    }

    /// <summary>Σ over lines of end-headcount × annual cost per head (HC064).</summary>
    public void RecomputeProjectedCost()
    {
        ProjectedCost = _lines.Sum(l => l.LineCost);
        base.Update();
    }

    /// <summary>Projected overrun beyond budget + threshold (0 when within limits or no budget set).</summary>
    public decimal ExcessBeyondThreshold()
    {
        if (TotalBudget <= 0) return 0;
        var ceiling = TotalBudget * (1 + BudgetThresholdPercent / 100m);
        return ProjectedCost > ceiling ? ProjectedCost - ceiling : 0;
    }

    /// <summary>
    /// Submits the plan for approval. When the projected cost breaches budget + threshold, an
    /// escalation justification is mandatory (HC066) and is recorded with the submission.
    /// </summary>
    public void Submit(string? escalationJustification)
    {
        EnsureEditable();
        if (_lines.Count == 0)
            throw new InvalidOperationException("A workforce plan needs at least one line before submission.");
        if (ExcessBeyondThreshold() > 0 && string.IsNullOrWhiteSpace(escalationJustification))
            throw new InvalidOperationException(
                "The projected cost exceeds the budget threshold — an escalation justification is required.");
        EscalationJustification = escalationJustification;
        Status = WorkforcePlanStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Workflow outcome: the plan becomes the approved workforce baseline (feeds recruitment, HC075).</summary>
    public void Approve()
    {
        if (Status != WorkforcePlanStatus.Submitted)
            throw new InvalidOperationException($"Only a submitted plan can be approved (current: {Status}).");
        Status = WorkforcePlanStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Workflow outcome: back to the planner — a rejected plan stays editable for resubmission.</summary>
    public void Reject()
    {
        if (Status != WorkforcePlanStatus.Submitted)
            throw new InvalidOperationException($"Only a submitted plan can be rejected (current: {Status}).");
        Status = WorkforcePlanStatus.Rejected;
        base.Update();
    }

    /// <summary>Archives a superseded approved plan (a newer version was approved, HC071).</summary>
    public void Archive()
    {
        if (Status != WorkforcePlanStatus.Approved)
            throw new InvalidOperationException($"Only an approved plan can be archived (current: {Status}).");
        Status = WorkforcePlanStatus.Archived;
        base.Update();
    }

    /// <summary>Stamps this plan as version N of a chain rooted at <paramref name="rootPlanId"/>.</summary>
    public void SetVersion(int version, Guid rootPlanId)
    {
        if (version < 2)
            throw new ArgumentException("Revision versions start at 2.", nameof(version));
        Version = version;
        RootPlanId = rootPlanId;
        base.Update();
    }

    private void EnsureEditable()
    {
        if (Status is not (WorkforcePlanStatus.Draft or WorkforcePlanStatus.Rejected))
            throw new InvalidOperationException($"A {Status} workforce plan can no longer be edited — create a new version instead.");
    }

    private static void Guard(string name, Guid startFiscalYearId, int periodCount, decimal totalBudget, decimal thresholdPercent)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name cannot be empty.", nameof(name));
        if (startFiscalYearId == Guid.Empty)
            throw new ArgumentException("A starting fiscal year is required.", nameof(startFiscalYearId));
        if (periodCount is < 1 or > 10)
            throw new ArgumentException("The planning horizon must span 1–10 fiscal-year periods.", nameof(periodCount));
        if (totalBudget < 0)
            throw new ArgumentException("Budget cannot be negative.", nameof(totalBudget));
        if (thresholdPercent is < 0 or > 100)
            throw new ArgumentException("The budget threshold must be between 0 and 100 percent.", nameof(thresholdPercent));
    }
}

/// <summary>Input spec for one plan line (see <see cref="WorkforcePlan.SetLines"/>).</summary>
public record WorkforcePlanLineSpec(
    Guid OrganizationUnitId,
    Guid PositionClassId,
    PlannedEmploymentType EmploymentType,
    int PeriodIndex,
    int AuthorizedHeadcount,
    int FilledCount,
    int VacantCount,
    int NewHires,
    int Replacements,
    int TemporaryStaff,
    int MobilityIn,
    int Promotions,
    int ActingAssignments,
    int Retirements,
    int Resignations,
    int ContractExpiries,
    bool IsCriticalRole,
    string? RequiredCompetencies,
    decimal AnnualSalaryCost,
    decimal AnnualAllowances,
    decimal AnnualBenefits,
    string? Remark);

/// <summary>
/// One row of a workforce plan: a role (position class) at an organizational unit for one
/// employment type and one horizon period. Carries the establishment snapshot (HC056), planned
/// demand (HC058), expected supply (HC059), anticipated separations (HC060), role criticality and
/// required competencies (HC061–HC062), and the per-head annual cost components (HC064).
/// </summary>
public class WorkforcePlanLine : BaseEntity
{
    public Guid PlanId { get; private set; }
    public Guid OrganizationUnitId { get; private set; }
    public Guid PositionClassId { get; private set; }
    public PlannedEmploymentType EmploymentType { get; private set; }
    /// <summary>0-based fiscal-year period within the plan's horizon (HC069).</summary>
    public int PeriodIndex { get; private set; }

    // Establishment snapshot at capture (auto-populated from live positions; authorized editable).
    public int AuthorizedHeadcount { get; private set; }
    public int FilledCount { get; private set; }
    public int VacantCount { get; private set; }

    // Demand (HC058)
    public int NewHires { get; private set; }
    public int Replacements { get; private set; }
    public int TemporaryStaff { get; private set; }

    // Supply (HC059)
    public int MobilityIn { get; private set; }
    public int Promotions { get; private set; }
    public int ActingAssignments { get; private set; }

    // Anticipated separations (HC060)
    public int Retirements { get; private set; }
    public int Resignations { get; private set; }
    public int ContractExpiries { get; private set; }

    // Competencies & criticality (HC061–HC062; structured skills model deferred to L&D, HC063)
    public bool IsCriticalRole { get; private set; }
    public string? RequiredCompetencies { get; private set; }

    // Annual cost components per head (HC064)
    public decimal AnnualSalaryCost { get; private set; }
    public decimal AnnualAllowances { get; private set; }
    public decimal AnnualBenefits { get; private set; }

    public string? Remark { get; private set; }

    /// <summary>Projected end-of-period headcount: filled − separations + demand + internal supply in.</summary>
    public int EndHeadcount =>
        Math.Max(0,
            FilledCount
            - (Retirements + Resignations + ContractExpiries)
            + (NewHires + Replacements + TemporaryStaff)
            + (MobilityIn + Promotions + ActingAssignments));

    /// <summary>Planned staffing still to source: demand beyond what the establishment can absorb.</summary>
    public int HeadcountGap => Math.Max(0, EndHeadcount - AuthorizedHeadcount);

    /// <summary>Projected annual cost of this line (HC064).</summary>
    public decimal LineCost => EndHeadcount * (AnnualSalaryCost + AnnualAllowances + AnnualBenefits);

    private WorkforcePlanLine() : base() { }

    public static WorkforcePlanLine Create(Guid planId, WorkforcePlanLineSpec spec)
    {
        if (spec.OrganizationUnitId == Guid.Empty)
            throw new ArgumentException("A plan line needs an organization unit.", nameof(spec));
        if (spec.PositionClassId == Guid.Empty)
            throw new ArgumentException("A plan line needs a position class (role).", nameof(spec));
        if (spec.PeriodIndex < 0)
            throw new ArgumentException("Period index cannot be negative.", nameof(spec));
        int[] counts =
        [
            spec.AuthorizedHeadcount, spec.FilledCount, spec.VacantCount,
            spec.NewHires, spec.Replacements, spec.TemporaryStaff,
            spec.MobilityIn, spec.Promotions, spec.ActingAssignments,
            spec.Retirements, spec.Resignations, spec.ContractExpiries
        ];
        if (counts.Any(c => c < 0))
            throw new ArgumentException("Headcount figures cannot be negative.", nameof(spec));
        if (spec.AnnualSalaryCost < 0 || spec.AnnualAllowances < 0 || spec.AnnualBenefits < 0)
            throw new ArgumentException("Cost components cannot be negative.", nameof(spec));

        return new WorkforcePlanLine
        {
            PlanId = planId,
            OrganizationUnitId = spec.OrganizationUnitId,
            PositionClassId = spec.PositionClassId,
            EmploymentType = spec.EmploymentType,
            PeriodIndex = spec.PeriodIndex,
            AuthorizedHeadcount = spec.AuthorizedHeadcount,
            FilledCount = spec.FilledCount,
            VacantCount = spec.VacantCount,
            NewHires = spec.NewHires,
            Replacements = spec.Replacements,
            TemporaryStaff = spec.TemporaryStaff,
            MobilityIn = spec.MobilityIn,
            Promotions = spec.Promotions,
            ActingAssignments = spec.ActingAssignments,
            Retirements = spec.Retirements,
            Resignations = spec.Resignations,
            ContractExpiries = spec.ContractExpiries,
            IsCriticalRole = spec.IsCriticalRole,
            RequiredCompetencies = spec.RequiredCompetencies,
            AnnualSalaryCost = spec.AnnualSalaryCost,
            AnnualAllowances = spec.AnnualAllowances,
            AnnualBenefits = spec.AnnualBenefits,
            Remark = spec.Remark
        };
    }
}
