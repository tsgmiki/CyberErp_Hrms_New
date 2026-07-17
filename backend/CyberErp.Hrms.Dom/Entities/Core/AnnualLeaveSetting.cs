using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Which accrual algorithm a leave policy applies (client-configurable).</summary>
public enum LeaveAccrualRuleType
{
    /// <summary>Two-phase statutory policy split at <see cref="AnnualLeaveSetting.MilestoneDate"/>: pre-milestone
    /// hires accrue by the pre-milestone rule (e.g. 14 + 1/yr up to the milestone, then 1 per 2 yrs); post-milestone
    /// hires accrue by the base rule (e.g. 16 + 1 per 2 yrs, external experience ignored).</summary>
    ServiceMilestone = 0,
    /// <summary>Single-phase service-based accrual: base + increment per N service years.</summary>
    ServiceYears = 1,
    /// <summary>Fiscal-year-based accrual: base + increment per N completed fiscal years.</summary>
    FiscalYears = 2
}

/// <summary>
/// Annual-leave accrual policy for one fiscal year (successor of the legacy
/// <c>hrmsAnnualLeaveSetting</c>). Governs a single leave type and drives entitlement generation:
/// <para>
/// entitled = (IsManagerial ? ManagerialLeaveDays : BaseLeaveDays)
///          + floor((serviceYears − 1) / IncrementIntervalYears) × IncrementDays, capped at MaxLeaveDays.
/// Employees with less than one year of service receive <see cref="NewEmployeeLeaveDays"/> prorated
/// by months of service in the fiscal year. Requests are blocked until
/// <see cref="MinExperienceMonths"/> of service (probation guard).
/// </para>
/// </summary>
public class AnnualLeaveSetting : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid FiscalYearId { get; private set; }
    /// <summary>The leave type this policy governs (the "Annual Leave" type).</summary>
    public Guid LeaveTypeId { get; private set; }

    /// <summary>Months of service required before annual leave may be requested (legacy MinExperience).</summary>
    public int MinExperienceMonths { get; private set; }
    /// <summary>Prorated basis for employees with under a year of service (legacy NewEmployeeLeaveDays).</summary>
    public int NewEmployeeLeaveDays { get; private set; }
    /// <summary>Base entitlement for non-managerial staff (legacy LeaveDays).</summary>
    public int BaseLeaveDays { get; private set; }
    /// <summary>Base entitlement for managerial staff (legacy LeaveDaysForManagerial).</summary>
    public int ManagerialLeaveDays { get; private set; }
    /// <summary>Extra days granted per service interval (legacy Increment).</summary>
    public int IncrementDays { get; private set; }
    /// <summary>Length of the service interval in years (Ethiopian labour law: 2).</summary>
    public int IncrementIntervalYears { get; private set; } = 2;
    /// <summary>Ceiling on the computed entitlement (legacy MaxLeaveDays).</summary>
    public int MaxLeaveDays { get; private set; }
    /// <summary>Years unused leave survives before expiring on rollover (legacy NoOfExpiryYears; law: 2).</summary>
    public int ExpiryYears { get; private set; } = 2;

    // ---- Flexible accrual configuration ------------------------------------
    /// <summary>Which accrual algorithm this policy applies.</summary>
    public LeaveAccrualRuleType RuleType { get; private set; } = LeaveAccrualRuleType.ServiceYears;
    /// <summary>Count qualifying external (government) experience toward service years. Never applied to
    /// post-milestone hires under <see cref="LeaveAccrualRuleType.ServiceMilestone"/>.</summary>
    public bool ConsiderExternalExperience { get; private set; }
    /// <summary>Cutover date for <see cref="LeaveAccrualRuleType.ServiceMilestone"/> (e.g. 2011-07-07).</summary>
    public DateTime? MilestoneDate { get; private set; }
    /// <summary>Base entitlement for pre-milestone hires (e.g. 14).</summary>
    public int PreMilestoneBaseLeaveDays { get; private set; }
    /// <summary>Increment days for the pre-milestone phase (e.g. 1).</summary>
    public int PreMilestoneIncrementDays { get; private set; }
    /// <summary>Service-year interval for the pre-milestone increment (e.g. 1 = one day per year).</summary>
    public int PreMilestoneIntervalYears { get; private set; } = 1;

    public bool IsActive { get; private set; } = true;

    private FiscalYear? _fiscalYear;
    public FiscalYear? FiscalYear => _fiscalYear;
    private LeaveType? _leaveType;
    public LeaveType? LeaveType => _leaveType;

    private AnnualLeaveSetting() : base() { }

    public static AnnualLeaveSetting Create(
        Guid fiscalYearId, Guid leaveTypeId, int minExperienceMonths, int newEmployeeLeaveDays,
        int baseLeaveDays, int managerialLeaveDays, int incrementDays, int incrementIntervalYears,
        int maxLeaveDays, int expiryYears, LeaveAccrualRuleType ruleType, bool considerExternalExperience,
        DateTime? milestoneDate, int preMilestoneBaseLeaveDays, int preMilestoneIncrementDays,
        int preMilestoneIntervalYears, bool isActive = true)
    {
        Validate(fiscalYearId, leaveTypeId, minExperienceMonths, newEmployeeLeaveDays, baseLeaveDays,
            managerialLeaveDays, incrementDays, incrementIntervalYears, maxLeaveDays, expiryYears,
            ruleType, milestoneDate, preMilestoneIntervalYears);
        return new AnnualLeaveSetting
        {
            FiscalYearId = fiscalYearId,
            LeaveTypeId = leaveTypeId,
            MinExperienceMonths = minExperienceMonths,
            NewEmployeeLeaveDays = newEmployeeLeaveDays,
            BaseLeaveDays = baseLeaveDays,
            ManagerialLeaveDays = managerialLeaveDays,
            IncrementDays = incrementDays,
            IncrementIntervalYears = incrementIntervalYears,
            MaxLeaveDays = maxLeaveDays,
            ExpiryYears = expiryYears,
            RuleType = ruleType,
            ConsiderExternalExperience = considerExternalExperience,
            MilestoneDate = milestoneDate,
            PreMilestoneBaseLeaveDays = preMilestoneBaseLeaveDays,
            PreMilestoneIncrementDays = preMilestoneIncrementDays,
            PreMilestoneIntervalYears = preMilestoneIntervalYears < 1 ? 1 : preMilestoneIntervalYears,
            IsActive = isActive
        };
    }

    public void Update(
        Guid fiscalYearId, Guid leaveTypeId, int minExperienceMonths, int newEmployeeLeaveDays,
        int baseLeaveDays, int managerialLeaveDays, int incrementDays, int incrementIntervalYears,
        int maxLeaveDays, int expiryYears, LeaveAccrualRuleType ruleType, bool considerExternalExperience,
        DateTime? milestoneDate, int preMilestoneBaseLeaveDays, int preMilestoneIncrementDays,
        int preMilestoneIntervalYears, bool isActive)
    {
        Validate(fiscalYearId, leaveTypeId, minExperienceMonths, newEmployeeLeaveDays, baseLeaveDays,
            managerialLeaveDays, incrementDays, incrementIntervalYears, maxLeaveDays, expiryYears,
            ruleType, milestoneDate, preMilestoneIntervalYears);
        FiscalYearId = fiscalYearId;
        LeaveTypeId = leaveTypeId;
        MinExperienceMonths = minExperienceMonths;
        NewEmployeeLeaveDays = newEmployeeLeaveDays;
        BaseLeaveDays = baseLeaveDays;
        ManagerialLeaveDays = managerialLeaveDays;
        IncrementDays = incrementDays;
        IncrementIntervalYears = incrementIntervalYears;
        MaxLeaveDays = maxLeaveDays;
        ExpiryYears = expiryYears;
        RuleType = ruleType;
        ConsiderExternalExperience = considerExternalExperience;
        MilestoneDate = milestoneDate;
        PreMilestoneBaseLeaveDays = preMilestoneBaseLeaveDays;
        PreMilestoneIncrementDays = preMilestoneIncrementDays;
        PreMilestoneIntervalYears = preMilestoneIntervalYears < 1 ? 1 : preMilestoneIntervalYears;
        IsActive = isActive;
        base.Update();
    }

    private static void Validate(Guid fiscalYearId, Guid leaveTypeId, int minExperienceMonths,
        int newEmployeeLeaveDays, int baseLeaveDays, int managerialLeaveDays, int incrementDays,
        int incrementIntervalYears, int maxLeaveDays, int expiryYears,
        LeaveAccrualRuleType ruleType, DateTime? milestoneDate, int preMilestoneIntervalYears)
    {
        if (fiscalYearId == Guid.Empty)
            throw new ArgumentException("Fiscal year is required.", nameof(fiscalYearId));
        if (leaveTypeId == Guid.Empty)
            throw new ArgumentException("Leave type is required.", nameof(leaveTypeId));
        if (minExperienceMonths < 0)
            throw new ArgumentException("Minimum experience cannot be negative.", nameof(minExperienceMonths));
        if (newEmployeeLeaveDays < 0 || baseLeaveDays < 0 || managerialLeaveDays < 0 || incrementDays < 0)
            throw new ArgumentException("Leave day figures cannot be negative.");
        if (incrementIntervalYears < 1)
            throw new ArgumentException("Increment interval must be at least 1 year.", nameof(incrementIntervalYears));
        // 0 = uncapped; otherwise the cap must not sit below the base entitlement.
        if (maxLeaveDays != 0 && maxLeaveDays < baseLeaveDays)
            throw new ArgumentException("Maximum leave days cannot be below the base entitlement.", nameof(maxLeaveDays));
        if (expiryYears < 1)
            throw new ArgumentException("Expiry years must be at least 1.", nameof(expiryYears));
        if (ruleType == LeaveAccrualRuleType.ServiceMilestone && !milestoneDate.HasValue)
            throw new ArgumentException("A milestone date is required for the service-milestone rule.", nameof(milestoneDate));
        if (preMilestoneIntervalYears < 0)
            throw new ArgumentException("Pre-milestone interval cannot be negative.", nameof(preMilestoneIntervalYears));
    }
}
