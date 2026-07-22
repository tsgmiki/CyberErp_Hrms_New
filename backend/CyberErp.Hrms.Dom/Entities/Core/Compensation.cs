using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>How an allowance's monetary value is derived (HC226).</summary>
public enum AllowanceCalcMethod
{
    /// <summary>A fixed monetary amount per period.</summary>
    Fixed = 0,
    /// <summary>A percentage (0–100) of the employee's base salary.</summary>
    PercentOfBase = 1
}

/// <summary>
/// Catalogue of allowance/earning kinds (HC226): transport, housing, meal, etc. Defines how the
/// value is computed and whether it is taxable — the per-employee <see cref="EmployeeAllowance"/>
/// carries the actual value assigned to an individual.
/// </summary>
public class AllowanceType : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public AllowanceCalcMethod CalcMethod { get; private set; }
    /// <summary>Default value seeded into new assignments — a fixed amount or a percent (0–100).</summary>
    public decimal? DefaultRate { get; private set; }
    /// <summary>Taxable allowances add to the taxable gross; non-taxable are exempt (HC231/232 input).</summary>
    public bool IsTaxable { get; private set; } = true;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private AllowanceType() : base() { }

    public static AllowanceType Create(string name, string? code, AllowanceCalcMethod calcMethod,
        decimal? defaultRate, bool isTaxable = true, bool isActive = true, int sortOrder = 0)
    {
        Guard(name, calcMethod, defaultRate);
        return new AllowanceType
        {
            Name = name.Trim(),
            Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim(),
            CalcMethod = calcMethod,
            DefaultRate = defaultRate,
            IsTaxable = isTaxable,
            IsActive = isActive,
            SortOrder = sortOrder
        };
    }

    public void Update(string name, string? code, AllowanceCalcMethod calcMethod,
        decimal? defaultRate, bool isTaxable, bool isActive, int sortOrder)
    {
        Guard(name, calcMethod, defaultRate);
        Name = name.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        CalcMethod = calcMethod;
        DefaultRate = defaultRate;
        IsTaxable = isTaxable;
        IsActive = isActive;
        SortOrder = sortOrder;
        base.Update();
    }

    private static void Guard(string name, AllowanceCalcMethod calcMethod, decimal? defaultRate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Allowance type name cannot be empty.", nameof(name));
        if (defaultRate is < 0)
            throw new ArgumentException("Default rate cannot be negative.", nameof(defaultRate));
        if (calcMethod == AllowanceCalcMethod.PercentOfBase && defaultRate is > 100)
            throw new ArgumentException("A percent-of-base rate cannot exceed 100.", nameof(defaultRate));
    }
}

/// <summary>
/// An allowance assigned to an individual employee (HC226), with an effective window so the
/// compensation history is preserved. <see cref="Value"/> is a fixed amount or a percent depending
/// on the referenced <see cref="AllowanceType"/>'s calc method; the resolved monetary figure is
/// computed against the employee's base salary at read time.
/// </summary>
public class EmployeeAllowance : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid AllowanceTypeId { get; private set; }
    /// <summary>Amount (Fixed) or percent 0–100 (PercentOfBase) — interpreted by the type's calc method.</summary>
    public decimal Value { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    /// <summary>End of the effective window; null = ongoing.</summary>
    public DateTime? EffectiveTo { get; private set; }
    public string? Remark { get; private set; }

    private EmployeeAllowance() : base() { }

    public static EmployeeAllowance Create(Guid employeeId, Guid allowanceTypeId, decimal value,
        DateTime effectiveFrom, DateTime? effectiveTo, string? remark)
    {
        Guard(employeeId, allowanceTypeId, value, effectiveFrom, effectiveTo);
        return new EmployeeAllowance
        {
            EmployeeId = employeeId,
            AllowanceTypeId = allowanceTypeId,
            Value = value,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            Remark = remark
        };
    }

    public void Update(Guid allowanceTypeId, decimal value, DateTime effectiveFrom,
        DateTime? effectiveTo, string? remark)
    {
        Guard(EmployeeId, allowanceTypeId, value, effectiveFrom, effectiveTo);
        AllowanceTypeId = allowanceTypeId;
        Value = value;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Remark = remark;
        base.Update();
    }

    private static void Guard(Guid employeeId, Guid allowanceTypeId, decimal value,
        DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (allowanceTypeId == Guid.Empty)
            throw new ArgumentException("Allowance type is required.", nameof(allowanceTypeId));
        if (value < 0)
            throw new ArgumentException("Allowance value cannot be negative.", nameof(value));
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new ArgumentException("The end date cannot precede the start date.", nameof(effectiveTo));
    }
}
