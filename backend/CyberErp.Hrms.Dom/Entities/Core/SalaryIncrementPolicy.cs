using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A client's salary-increment eligibility rules. One configuration is active per tenant (same shape
/// as <see cref="WorkWeekConfiguration"/>); absent one, no minimum-service gate is applied.
///
/// <para>Minimum service varies by client — 3, 6 and 9 months are all in use — which is why it is
/// configuration rather than a constant.</para>
/// </summary>
public class SalaryIncrementPolicy : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Completed months of service an employee must have at the revision's effective date to qualify.
    /// 0 means no tenure gate. Measured from <c>Employee.HireDate</c>; an employee with no hire date
    /// cannot be assessed and is excluded rather than assumed eligible.
    /// </summary>
    public int MinimumServiceMonths { get; private set; }

    /// <summary>
    /// Prorate the increment for anyone with less than 12 months of service, by months actually
    /// worked. Off means everyone eligible receives the full increment.
    /// </summary>
    public bool ProrateFirstYear { get; private set; } = true;

    /// <summary>
    /// Exclude employees carrying an active (non-cancelled, unexpired) disciplinary case.
    /// </summary>
    public bool ExcludeActiveDisciplinary { get; private set; } = true;

    /// <summary>
    /// When a step increment would carry an employee past the top rung of their grade, move them onto
    /// the next grade up instead of holding them at the ceiling.
    ///
    /// <para>Defaults to OFF. Promotion changes an employee's grade, not just their pay, so it stays
    /// something a client turns on deliberately — an existing revision must not start promoting people
    /// because the software was upgraded.</para>
    /// </summary>
    public bool PromoteOnGradeCeiling { get; private set; }

    public bool IsActive { get; private set; } = true;

    private SalaryIncrementPolicy() : base() { }

    public static SalaryIncrementPolicy Create(string name, int minimumServiceMonths,
        bool prorateFirstYear = true, bool excludeActiveDisciplinary = true, bool isActive = true,
        bool promoteOnGradeCeiling = false)
    {
        Guard(name, minimumServiceMonths);
        return new SalaryIncrementPolicy
        {
            Name = name.Trim(),
            MinimumServiceMonths = minimumServiceMonths,
            ProrateFirstYear = prorateFirstYear,
            ExcludeActiveDisciplinary = excludeActiveDisciplinary,
            PromoteOnGradeCeiling = promoteOnGradeCeiling,
            IsActive = isActive
        };
    }

    public void Update(string name, int minimumServiceMonths, bool prorateFirstYear,
        bool excludeActiveDisciplinary, bool isActive, bool promoteOnGradeCeiling = false)
    {
        Guard(name, minimumServiceMonths);
        Name = name.Trim();
        MinimumServiceMonths = minimumServiceMonths;
        ProrateFirstYear = prorateFirstYear;
        ExcludeActiveDisciplinary = excludeActiveDisciplinary;
        PromoteOnGradeCeiling = promoteOnGradeCeiling;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name, int minimumServiceMonths)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Policy name cannot be empty.", nameof(name));
        if (minimumServiceMonths < 0)
            throw new ArgumentException("Minimum service months cannot be negative.", nameof(minimumServiceMonths));
        // A gate longer than a year would silently exclude everyone in their first year AND collide
        // with the proration window; treat it as a configuration mistake.
        if (minimumServiceMonths > 60)
            throw new ArgumentException("Minimum service months cannot exceed 60.", nameof(minimumServiceMonths));
    }
}
