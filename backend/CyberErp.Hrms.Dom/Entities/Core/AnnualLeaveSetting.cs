using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Which accrual algorithm a leave policy applies (client-configurable).</summary>
public enum LeaveAccrualRuleType
{
    /// <summary>
    /// Two-phase statutory policy split at <see cref="AnnualLeaveSetting.MilestoneDate"/>: pre-milestone
    /// hires accrue by the pre-milestone rule (e.g. 14 + 1/yr up to the milestone, then 1 per 2 yrs);
    /// post-milestone hires accrue by the base rule (e.g. 16 + 1 per 2 yrs, external experience ignored).
    /// <para>⚠️ The pre-milestone rule is a FLOOR, not a replacement — a pre-milestone hire receives the
    /// BETTER of the two, so length of service can never cost somebody days. Managerial staff keep
    /// <see cref="AnnualLeaveSetting.ManagerialLeaveDays"/> as their base in both phases;
    /// <see cref="AnnualLeaveSetting.PreMilestoneBaseLeaveDays"/> applies to non-managerial staff only
    /// (logic §12.94).</para>
    /// </summary>
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
///          + floor(serviceYears / IncrementIntervalYears) × IncrementDays, capped at MaxLeaveDays,
/// where serviceYears is measured to the fiscal year's START.
/// <para>
/// ⚠️ THIS COMMENT USED TO SAY <c>floor((serviceYears − 1) / …)</c>, which the code has never done and
/// which reads as a one-day over-grant when checked against the code. It is not: the statute grants
/// the first year plus one day per two ADDITIONAL years, i.e. <c>floor((serviceAtYearEnd − 1) /
/// interval)</c>, and a full fiscal year normally contains exactly one hire anniversary — so that is
/// the same number as the year-start form above.
/// <para>
/// ⚠️ WITH ONE EXCEPTION: an employee hired ON the fiscal-year start date reaches their anniversary on
/// day one, so the year contains no FURTHER anniversary and the two forms differ by a day. The
/// year-start form used here is the correct one for them — they already hold that service for the
/// whole leave year. Two employees in the live data (both hired 08 July) sit on this boundary
/// (logic §12.95).
/// </para>
/// Employees with less than one year of service receive <see cref="NewEmployeeLeaveDays"/> prorated
/// by months of service in the fiscal year. Requests are blocked until
/// <see cref="MinExperienceMonths"/> of service (probation guard).
/// </para>
/// </summary>
public class AnnualLeaveSetting : BaseEntity, IAggregateRoot, IAuditable
{
    // NOTE: this policy is intrinsically ANNUAL leave — it carries no LeaveType relationship.
    // The LeaveType master now relates to hrmsOtherLeaveSetting (the non-annual leaves) instead;
    // ledger generation resolves "the annual leave type" by its Annual accrual method.
    public Guid FiscalYearId { get; private set; }

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

    // ---- Policy figures moved here from LeaveType ---------------------------
    /// <summary>Maximum days that may carry forward into the next year (null = unlimited, 0 = none).</summary>
    public decimal? CarryForwardMaxDays { get; private set; }
    /// <summary>Optional cap on the length of a single continuous request.</summary>
    public int? MaxConsecutiveDays { get; private set; }
    /// <summary>Whether a request line may cover half a day. Moved here from <c>LeaveType.AllowHalfDay</c>
    /// when annual leave stopped being a leave type — this policy is now the only place it lives.</summary>
    public bool AllowHalfDay { get; private set; } = true;

    // ---- Flexible accrual configuration ------------------------------------
    /// <summary>Which accrual algorithm this policy applies.</summary>
    public LeaveAccrualRuleType RuleType { get; private set; } = LeaveAccrualRuleType.ServiceYears;
    /// <summary>Count qualifying external (government) experience toward service years. Never applied to
    /// post-milestone hires under <see cref="LeaveAccrualRuleType.ServiceMilestone"/>.</summary>
    public bool ConsiderExternalExperience { get; private set; }
    /// <summary>
    /// Cutover date for <see cref="LeaveAccrualRuleType.ServiceMilestone"/>, as a GREGORIAN date.
    /// Live value: <c>2019-07-08</c> = <b>Hamle 1, 2011 EC</b>, the first day of the Ethiopian fiscal
    /// year matching the current Labour Proclamation — the one that raised the statutory base from
    /// <b>14</b> to <b>16</b> days, which is exactly what <see cref="PreMilestoneBaseLeaveDays"/> and
    /// <see cref="BaseLeaveDays"/> hold.
    /// </summary>
    /// <remarks>
    /// ⚠️ THE EXAMPLE HERE USED TO READ "e.g. 2011-07-07", WHICH IS A TRAP. That is a Gregorian date
    /// falling in Sene 2003 EC — a different Ethiopian year entirely — so it reads as though an
    /// Ethiopian year had been typed into a Gregorian field and makes a correctly-configured 2019 date
    /// look wrong. Configuring that example value instead would cost 131 employees a total of 413 days
    /// (logic §12.96).
    ///
    /// <para>⚠️ Moving this date does not only reclassify hires around it — it re-splits the pre/post
    /// service of EVERY pre-milestone employee, because the two phases accrue at different rates
    /// (1/yr before, 1 per 2 yrs after). Shifting it by two months changes 18 long-serving employees
    /// whose anniversary falls near the boundary, while changing none of the 22 hired just after it.
    /// It is a policy date, not a tuning knob.</para>
    /// </remarks>
    public DateTime? MilestoneDate { get; private set; }
    /// <summary>
    /// Base entitlement for NON-MANAGERIAL pre-milestone hires (e.g. 14). Managerial staff use
    /// <see cref="ManagerialLeaveDays"/> in both phases, so there is no managerial counterpart here.
    /// </summary>
    public int PreMilestoneBaseLeaveDays { get; private set; }
    /// <summary>Increment days for the pre-milestone phase (e.g. 1).</summary>
    public int PreMilestoneIncrementDays { get; private set; }
    /// <summary>Service-year interval for the pre-milestone increment (e.g. 1 = one day per year).</summary>
    public int PreMilestoneIntervalYears { get; private set; } = 1;

    public bool IsActive { get; private set; } = true;

    private FiscalYear? _fiscalYear;
    public FiscalYear? FiscalYear => _fiscalYear;

    private AnnualLeaveSetting() : base() { }

    public static AnnualLeaveSetting Create(
        Guid fiscalYearId, int minExperienceMonths, int newEmployeeLeaveDays,
        int baseLeaveDays, int managerialLeaveDays, int incrementDays, int incrementIntervalYears,
        int maxLeaveDays, int expiryYears, LeaveAccrualRuleType ruleType, bool considerExternalExperience,
        DateTime? milestoneDate, int preMilestoneBaseLeaveDays, int preMilestoneIncrementDays,
        int preMilestoneIntervalYears, decimal? carryForwardMaxDays,
        int? maxConsecutiveDays, bool isActive = true, bool allowHalfDay = true)
    {
        Validate(fiscalYearId, minExperienceMonths, newEmployeeLeaveDays, baseLeaveDays,
            managerialLeaveDays, incrementDays, incrementIntervalYears, maxLeaveDays, expiryYears,
            ruleType, milestoneDate, preMilestoneIntervalYears,
            carryForwardMaxDays, maxConsecutiveDays);
        return new AnnualLeaveSetting
        {
            FiscalYearId = fiscalYearId,
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
            CarryForwardMaxDays = carryForwardMaxDays,
            MaxConsecutiveDays = maxConsecutiveDays,
            AllowHalfDay = allowHalfDay,
            IsActive = isActive
        };
    }

    public void Update(
        Guid fiscalYearId, int minExperienceMonths, int newEmployeeLeaveDays,
        int baseLeaveDays, int managerialLeaveDays, int incrementDays, int incrementIntervalYears,
        int maxLeaveDays, int expiryYears, LeaveAccrualRuleType ruleType, bool considerExternalExperience,
        DateTime? milestoneDate, int preMilestoneBaseLeaveDays, int preMilestoneIncrementDays,
        int preMilestoneIntervalYears, decimal? carryForwardMaxDays,
        int? maxConsecutiveDays, bool isActive, bool allowHalfDay = true)
    {
        Validate(fiscalYearId, minExperienceMonths, newEmployeeLeaveDays, baseLeaveDays,
            managerialLeaveDays, incrementDays, incrementIntervalYears, maxLeaveDays, expiryYears,
            ruleType, milestoneDate, preMilestoneIntervalYears,
            carryForwardMaxDays, maxConsecutiveDays);
        FiscalYearId = fiscalYearId;
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
        CarryForwardMaxDays = carryForwardMaxDays;
        MaxConsecutiveDays = maxConsecutiveDays;
        AllowHalfDay = allowHalfDay;
        IsActive = isActive;
        base.Update();
    }

    private static void Validate(Guid fiscalYearId, int minExperienceMonths,
        int newEmployeeLeaveDays, int baseLeaveDays, int managerialLeaveDays, int incrementDays,
        int incrementIntervalYears, int maxLeaveDays, int expiryYears,
        LeaveAccrualRuleType ruleType, DateTime? milestoneDate, int preMilestoneIntervalYears,
        decimal? carryForwardMaxDays, int? maxConsecutiveDays)
    {
        if (carryForwardMaxDays is < 0)
            throw new ArgumentException("Carry-forward maximum cannot be negative.", nameof(carryForwardMaxDays));
        if (maxConsecutiveDays is < 1)
            throw new ArgumentException("Maximum consecutive days must be at least 1.", nameof(maxConsecutiveDays));
        if (fiscalYearId == Guid.Empty)
            throw new ArgumentException("Fiscal year is required.", nameof(fiscalYearId));
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
