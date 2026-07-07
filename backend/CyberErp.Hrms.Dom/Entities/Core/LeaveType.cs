using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>How a leave type's entitlement is granted over a leave year.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LeaveAccrualMethod
{
    /// <summary>Full entitlement granted up-front at the start of the year / on hire.</summary>
    Annual = 0,
    /// <summary>Entitlement accrues in equal monthly increments across the year.</summary>
    Monthly = 1,
    /// <summary>No automatic entitlement (e.g. unpaid leave — always available, never accrued).</summary>
    None = 2
}

/// <summary>Restricts a leave type to a gender (e.g. maternity/paternity).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LeaveGenderEligibility
{
    Any = 0,
    Male = 1,
    Female = 2
}

/// <summary>
/// Attendance &amp; Leave (HC030): a configurable category of leave (annual, sick, maternity, unpaid…)
/// with its entitlement and policy rules. Balances and requests reference it.
/// </summary>
public class LeaveType : BaseEntity, IAggregateRoot, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? NameA { get; private set; }

    /// <summary>Paid leave draws salary; unpaid (LWOP) feeds payroll deductions.</summary>
    public bool IsPaid { get; private set; } = true;
    /// <summary>When true a request routes through the approval workflow; otherwise it auto-approves.</summary>
    public bool RequiresApproval { get; private set; } = true;
    public bool AllowHalfDay { get; private set; }
    public LeaveGenderEligibility GenderEligibility { get; private set; } = LeaveGenderEligibility.Any;

    /// <summary>Default annual entitlement in days (per HC031); balances seed from this.</summary>
    public decimal DefaultAnnualEntitlement { get; private set; }
    public LeaveAccrualMethod AccrualMethod { get; private set; } = LeaveAccrualMethod.Annual;
    /// <summary>Maximum days that may carry forward into the next year (null = unlimited, 0 = none).</summary>
    public decimal? CarryForwardMaxDays { get; private set; }
    /// <summary>Optional cap on the length of a single continuous request.</summary>
    public int? MaxConsecutiveDays { get; private set; }

    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private LeaveType() : base() { }

    public static LeaveType Create(
        string code,
        string name,
        string? nameA = null,
        bool isPaid = true,
        bool requiresApproval = true,
        bool allowHalfDay = false,
        LeaveGenderEligibility genderEligibility = LeaveGenderEligibility.Any,
        decimal defaultAnnualEntitlement = 0,
        LeaveAccrualMethod accrualMethod = LeaveAccrualMethod.Annual,
        decimal? carryForwardMaxDays = null,
        int? maxConsecutiveDays = null,
        string? description = null,
        bool isActive = true)
    {
        Validate(code, name, defaultAnnualEntitlement, carryForwardMaxDays, maxConsecutiveDays);
        return new LeaveType
        {
            Code = code.Trim(),
            Name = name.Trim(),
            NameA = nameA,
            IsPaid = isPaid,
            RequiresApproval = requiresApproval,
            AllowHalfDay = allowHalfDay,
            GenderEligibility = genderEligibility,
            DefaultAnnualEntitlement = defaultAnnualEntitlement,
            AccrualMethod = accrualMethod,
            CarryForwardMaxDays = carryForwardMaxDays,
            MaxConsecutiveDays = maxConsecutiveDays,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(
        string code,
        string name,
        string? nameA,
        bool isPaid,
        bool requiresApproval,
        bool allowHalfDay,
        LeaveGenderEligibility genderEligibility,
        decimal defaultAnnualEntitlement,
        LeaveAccrualMethod accrualMethod,
        decimal? carryForwardMaxDays,
        int? maxConsecutiveDays,
        string? description,
        bool isActive)
    {
        Validate(code, name, defaultAnnualEntitlement, carryForwardMaxDays, maxConsecutiveDays);
        Code = code.Trim();
        Name = name.Trim();
        NameA = nameA;
        IsPaid = isPaid;
        RequiresApproval = requiresApproval;
        AllowHalfDay = allowHalfDay;
        GenderEligibility = genderEligibility;
        DefaultAnnualEntitlement = defaultAnnualEntitlement;
        AccrualMethod = accrualMethod;
        CarryForwardMaxDays = carryForwardMaxDays;
        MaxConsecutiveDays = maxConsecutiveDays;
        Description = description;
        IsActive = isActive;
        base.Update();
    }

    private static void Validate(string code, string name, decimal entitlement, decimal? carryForward, int? maxConsecutive)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Leave type code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Leave type name cannot be empty.", nameof(name));
        if (entitlement < 0)
            throw new ArgumentException("Default annual entitlement cannot be negative.", nameof(entitlement));
        if (carryForward is < 0)
            throw new ArgumentException("Carry-forward maximum cannot be negative.", nameof(carryForward));
        if (maxConsecutive is < 1)
            throw new ArgumentException("Maximum consecutive days must be at least 1.", nameof(maxConsecutive));
    }
}
