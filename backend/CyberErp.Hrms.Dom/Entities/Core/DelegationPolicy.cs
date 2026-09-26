using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Tenant-wide rules deciding WHO may hold somebody else's approval authority.
/// </summary>
/// <remarks>
/// <para>A singleton per tenant, like <c>Setting</c>. Without it, "delegation" means any approver
/// can hand their authority to anyone at all — which is how an approval chain designed around
/// seniority gets quietly routed through whoever happened to be free.</para>
///
/// <para>⚠️ Every rule here is a FLOOR, not a replacement for judgement. They stop the obviously
/// wrong delegation (an executive's authority to a new junior); they cannot tell whether a
/// particular stand-in is the right one. HR still chooses.</para>
///
/// <para>The two seniority rules read the employee data the client asked delegation to respect:
/// <b>experience</b> (internal service from the hire date plus prior <c>EmployeeExperience</c>
/// records) and <b>salary</b>.</para>
/// </remarks>
public class DelegationPolicy : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>
    /// Total years of experience the delegate must hold — internal service plus prior experience.
    /// 0 disables the rule.
    /// </summary>
    public int MinDelegateExperienceYears { get; private set; } = 2;

    /// <summary>
    /// The delegate's salary as a percentage of the delegator's, at minimum. 80 means a delegate
    /// earning less than 80% of the delegator's salary cannot hold their approval authority.
    /// 0 disables the rule.
    /// </summary>
    /// <remarks>
    /// ⚠️ Salary, not job grade, because <c>JobGrade</c> carries only a name and a code — it has no
    /// rank, so "within N grades" is not computable from this schema. Salary is the ordered measure
    /// the data actually has, and it is what the grade ultimately expresses
    /// (<c>SalaryScale.Salary</c> per grade and step).
    /// </remarks>
    public int MinSalaryRatioPercent { get; private set; } = 80;

    /// <summary>When the delegator is managerial, the delegate must be too.</summary>
    public bool RequireManagerialDelegate { get; private set; } = true;

    /// <summary>
    /// Confine a delegate to the approver's own department and the departments beneath it.
    /// </summary>
    /// <remarks>
    /// <para>On by default, and the rule most organisations want: a department head covers their
    /// own area, not somebody else's. Turning it OFF lets an approver name anyone in the tenant,
    /// which suits a flat organisation or one that covers across sites — the seniority rules still
    /// apply, so it widens WHO may be chosen, never what they may do.</para>
    ///
    /// <para>⚠️ This is the ONLY one of the delegation guards that is configurable, and
    /// deliberately so. The other three are invariants, not preferences: authority received through
    /// a delegation cannot be delegated onward (the chain moves real approval rights to somebody
    /// nobody chose), a delegate can never approve their own request, and an open workflow step
    /// confers nothing because there is no authority there to lend. An organisation that wanted any
    /// of those switched off would be asking for an approval chain that does not mean anything, and
    /// a settings page that offers the option implies it is a reasonable thing to want.</para>
    /// </remarks>
    public bool RestrictToOwnDepartment { get; private set; } = true;

    /// <summary>
    /// Longest delegation window, in days. 0 disables the rule.
    /// </summary>
    /// <remarks>
    /// An indefinite delegation is not a delegation — it is an undocumented change to the approval
    /// chain that nobody revisits. A bounded window forces the question to be asked again.
    /// </remarks>
    public int MaxDelegationDays { get; private set; } = 90;

    /// <summary>Ceiling applied when a delegation names none. Null = uncapped by default.</summary>
    public decimal? DefaultApprovalLimit { get; private set; }

    /// <summary>
    /// Whether an approver may create their own delegation. When false, only an HR administrator can.
    /// </summary>
    public bool AllowSelfServiceDelegation { get; private set; } = true;

    private DelegationPolicy() : base() { }

    /// <summary>The shipped defaults, used when a tenant has never configured the policy.</summary>
    public static DelegationPolicy CreateDefault() => new();

    public void Update(int minDelegateExperienceYears, int minSalaryRatioPercent,
        bool requireManagerialDelegate, int maxDelegationDays, decimal? defaultApprovalLimit,
        bool allowSelfServiceDelegation, bool restrictToOwnDepartment)
    {
        if (minDelegateExperienceYears < 0)
            throw new ArgumentException("Minimum experience cannot be negative.", nameof(minDelegateExperienceYears));
        if (minSalaryRatioPercent is < 0 or > 1000)
            throw new ArgumentException("Salary ratio must be a sensible percentage.", nameof(minSalaryRatioPercent));
        if (maxDelegationDays < 0)
            throw new ArgumentException("Maximum delegation length cannot be negative.", nameof(maxDelegationDays));
        if (defaultApprovalLimit is < 0)
            throw new ArgumentException("A default approval limit cannot be negative.", nameof(defaultApprovalLimit));

        MinDelegateExperienceYears = minDelegateExperienceYears;
        MinSalaryRatioPercent = minSalaryRatioPercent;
        RequireManagerialDelegate = requireManagerialDelegate;
        MaxDelegationDays = maxDelegationDays;
        DefaultApprovalLimit = defaultApprovalLimit;
        AllowSelfServiceDelegation = allowSelfServiceDelegation;
        RestrictToOwnDepartment = restrictToOwnDepartment;
        base.Update();
    }
}
