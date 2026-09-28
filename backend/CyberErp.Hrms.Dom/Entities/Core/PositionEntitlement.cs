using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Which side of the compensation module an entitlement points at.</summary>
public enum EntitlementKind
{
    /// <summary>An <see cref="AllowanceType"/> — transport, housing, representation.</summary>
    Allowance = 0,
    /// <summary>A <see cref="BenefitPlan"/> — health, life, pension.</summary>
    BenefitPlan = 1
}

/// <summary>
/// An allowance or benefit that belongs to a POST rather than to whoever currently holds it.
/// </summary>
/// <remarks>
/// <para>⚠️ THIS IS THE GAP §12.118 NAMED. Salary was already a property of the post — through
/// <c>PositionClass.SalaryScale</c> — but allowances and benefit enrolments attach to PEOPLE, so
/// "the benefits associated with the deputized position" had nothing to read. A deputy could be
/// paid the post's salary and not its car allowance, which is not what covering a post means.</para>
///
/// <para>Hung off <see cref="PositionClass"/>, not <see cref="Position"/>, deliberately: the class
/// is the JOB DEFINITION and is already where the salary scale lives. Two Finance Officer posts in
/// different units are the same job on the same terms, and duplicating the entitlement set per
/// post would invite them to drift apart.</para>
///
/// <para>⚠️ Defining an entitlement does NOT enrol the post's current holder. This is a statement
/// about the job, and applying it to substantive holders is a separate exercise with real payroll
/// consequences — it is read here only to decide what a DEPUTY receives while acting.</para>
/// </remarks>
public class PositionEntitlement : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid PositionClassId { get; private set; }
    public EntitlementKind Kind { get; private set; }

    /// <summary>Set when <see cref="Kind"/> is <see cref="EntitlementKind.Allowance"/>.</summary>
    public Guid? AllowanceTypeId { get; private set; }
    /// <summary>Set when <see cref="Kind"/> is <see cref="EntitlementKind.BenefitPlan"/>.</summary>
    public Guid? BenefitPlanId { get; private set; }

    /// <summary>
    /// The post's own figure, overriding the type's default rate. Null = use the type's default.
    /// </summary>
    /// <remarks>
    /// Interpreted by the referenced allowance type's calc method, exactly as
    /// <see cref="EmployeeAllowance.Value"/> is — a fixed amount, or a percent of base pay. A
    /// percent entitlement therefore resolves against the DEPUTY's acting salary, which is the
    /// right answer: a housing allowance of 15% of base follows the base being paid.
    /// </remarks>
    public decimal? Value { get; private set; }

    /// <summary>
    /// Whether somebody ACTING in the post receives this.
    /// </summary>
    /// <remarks>
    /// ⚠️ Defaults true — the point of acting compensation is that the deputy gets the post's
    /// package. But not everything a post carries should follow a stand-in: a long-service award,
    /// a relocation benefit, or anything tied to the substantive holder personally would be wrong
    /// to hand over for a few months, and there must be a way to say so that is not "delete the
    /// entitlement".
    /// </remarks>
    public bool GrantedWhenActing { get; private set; } = true;

    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private PositionEntitlement() : base() { }

    public static PositionEntitlement Create(
        Guid positionClassId, EntitlementKind kind, Guid? allowanceTypeId, Guid? benefitPlanId,
        decimal? value, bool grantedWhenActing = true, bool isActive = true, string? notes = null)
    {
        Guard(positionClassId, kind, allowanceTypeId, benefitPlanId, value);
        return new PositionEntitlement
        {
            PositionClassId = positionClassId,
            Kind = kind,
            AllowanceTypeId = kind == EntitlementKind.Allowance ? allowanceTypeId : null,
            BenefitPlanId = kind == EntitlementKind.BenefitPlan ? benefitPlanId : null,
            Value = value,
            GrantedWhenActing = grantedWhenActing,
            IsActive = isActive,
            Notes = notes
        };
    }

    public void Update(EntitlementKind kind, Guid? allowanceTypeId, Guid? benefitPlanId,
        decimal? value, bool grantedWhenActing, bool isActive, string? notes)
    {
        Guard(PositionClassId, kind, allowanceTypeId, benefitPlanId, value);
        Kind = kind;
        // Clear the other side, so a kind change cannot leave a stale reference behind that a
        // later reader might follow.
        AllowanceTypeId = kind == EntitlementKind.Allowance ? allowanceTypeId : null;
        BenefitPlanId = kind == EntitlementKind.BenefitPlan ? benefitPlanId : null;
        Value = value;
        GrantedWhenActing = grantedWhenActing;
        IsActive = isActive;
        Notes = notes;
        base.Update();
    }

    /// <summary>The referenced catalogue row, whichever side it is on.</summary>
    public Guid ReferenceId =>
        (Kind == EntitlementKind.Allowance ? AllowanceTypeId : BenefitPlanId) ?? Guid.Empty;

    private static void Guard(Guid positionClassId, EntitlementKind kind,
        Guid? allowanceTypeId, Guid? benefitPlanId, decimal? value)
    {
        if (positionClassId == Guid.Empty)
            throw new ArgumentException("Position class is required.", nameof(positionClassId));
        if (value is < 0)
            throw new ArgumentException("An entitlement value cannot be negative.", nameof(value));

        // ⚠️ Exactly one reference, matching the kind. A row carrying both, or neither, would read
        // as valid and then resolve to nothing at the moment somebody is being paid.
        if (kind == EntitlementKind.Allowance && (allowanceTypeId is null || allowanceTypeId == Guid.Empty))
            throw new ArgumentException("An allowance entitlement needs an allowance type.", nameof(allowanceTypeId));
        if (kind == EntitlementKind.BenefitPlan && (benefitPlanId is null || benefitPlanId == Guid.Empty))
            throw new ArgumentException("A benefit entitlement needs a benefit plan.", nameof(benefitPlanId));
    }
}
