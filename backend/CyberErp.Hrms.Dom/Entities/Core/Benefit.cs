using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Kind of benefit plan (HC230/HC276): health, life, disability, pension, etc.</summary>
public enum BenefitCategory
{
    Health = 0,
    Life = 1,
    Disability = 2,
    Pension = 3,
    Other = 4
}

/// <summary>State of an employee's enrollment in a plan.</summary>
public enum BenefitEnrollmentStatus
{
    Enrolled = 0,
    Waived = 1,
    Terminated = 2
}

/// <summary>
/// A benefit plan employees can enroll in (HC230): its category, employee/employer contribution
/// rules (reusing <see cref="AllowanceCalcMethod"/>) and an optional open-enrollment window. A null
/// window means enrollment is always open (e.g. onboarding).
/// </summary>
public class BenefitPlan : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public BenefitCategory Category { get; private set; }
    public string? Description { get; private set; }
    public AllowanceCalcMethod EmployeeContributionMethod { get; private set; }
    public decimal EmployeeContributionRate { get; private set; }
    public AllowanceCalcMethod EmployerContributionMethod { get; private set; }
    public decimal EmployerContributionRate { get; private set; }
    public DateTime? EnrollmentOpenFrom { get; private set; }
    public DateTime? EnrollmentOpenTo { get; private set; }
    public bool IsActive { get; private set; } = true;

    private BenefitPlan() : base() { }

    public static BenefitPlan Create(string name, BenefitCategory category, string? description,
        AllowanceCalcMethod employeeMethod, decimal employeeRate,
        AllowanceCalcMethod employerMethod, decimal employerRate,
        DateTime? openFrom, DateTime? openTo, bool isActive = true)
    {
        Guard(name, employeeMethod, employeeRate, employerMethod, employerRate, openFrom, openTo);
        return new BenefitPlan
        {
            Name = name.Trim(),
            Category = category,
            Description = description,
            EmployeeContributionMethod = employeeMethod,
            EmployeeContributionRate = employeeRate,
            EmployerContributionMethod = employerMethod,
            EmployerContributionRate = employerRate,
            EnrollmentOpenFrom = openFrom,
            EnrollmentOpenTo = openTo,
            IsActive = isActive
        };
    }

    public void Update(string name, BenefitCategory category, string? description,
        AllowanceCalcMethod employeeMethod, decimal employeeRate,
        AllowanceCalcMethod employerMethod, decimal employerRate,
        DateTime? openFrom, DateTime? openTo, bool isActive)
    {
        Guard(name, employeeMethod, employeeRate, employerMethod, employerRate, openFrom, openTo);
        Name = name.Trim();
        Category = category;
        Description = description;
        EmployeeContributionMethod = employeeMethod;
        EmployeeContributionRate = employeeRate;
        EmployerContributionMethod = employerMethod;
        EmployerContributionRate = employerRate;
        EnrollmentOpenFrom = openFrom;
        EnrollmentOpenTo = openTo;
        IsActive = isActive;
        base.Update();
    }

    /// <summary>HC230 — enrollment is open when active and the date falls in the window (null = always).</summary>
    public bool IsEnrollmentOpenOn(DateTime on) =>
        IsActive
        && (EnrollmentOpenFrom is null || on.Date >= EnrollmentOpenFrom.Value.Date)
        && (EnrollmentOpenTo is null || on.Date <= EnrollmentOpenTo.Value.Date);

    private static void Guard(string name, AllowanceCalcMethod em, decimal er, AllowanceCalcMethod pm, decimal pr,
        DateTime? from, DateTime? to)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name cannot be empty.", nameof(name));
        if (er < 0 || pr < 0)
            throw new ArgumentException("Contribution rate cannot be negative.");
        if (em == AllowanceCalcMethod.PercentOfBase && er > 100)
            throw new ArgumentException("Employee percent-of-base cannot exceed 100.");
        if (pm == AllowanceCalcMethod.PercentOfBase && pr > 100)
            throw new ArgumentException("Employer percent-of-base cannot exceed 100.");
        if (from.HasValue && to.HasValue && to.Value < from.Value)
            throw new ArgumentException("Enrollment end cannot precede the start.");
    }
}

/// <summary>An employee's enrollment in a <see cref="BenefitPlan"/> (HC230).</summary>
public class EmployeeBenefitEnrollment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid BenefitPlanId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public BenefitEnrollmentStatus Status { get; private set; } = BenefitEnrollmentStatus.Enrolled;
    public DateTime EnrolledOn { get; private set; }
    public DateTime CoverageStart { get; private set; }
    public DateTime? CoverageEnd { get; private set; }
    /// <summary>Optional employee-elected contribution overriding the plan's computed employee share.</summary>
    public decimal? ElectedEmployeeContribution { get; private set; }
    public string? Remark { get; private set; }

    private EmployeeBenefitEnrollment() : base() { }

    public static EmployeeBenefitEnrollment Create(Guid benefitPlanId, Guid employeeId, DateTime enrolledOn,
        DateTime coverageStart, decimal? electedContribution, string? remark)
    {
        if (benefitPlanId == Guid.Empty) throw new ArgumentException("Plan is required.", nameof(benefitPlanId));
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (electedContribution is < 0) throw new ArgumentException("Elected contribution cannot be negative.");
        return new EmployeeBenefitEnrollment
        {
            BenefitPlanId = benefitPlanId,
            EmployeeId = employeeId,
            Status = BenefitEnrollmentStatus.Enrolled,
            EnrolledOn = enrolledOn,
            CoverageStart = coverageStart,
            ElectedEmployeeContribution = electedContribution,
            Remark = remark
        };
    }

    public void Waive(string? remark)
    {
        Status = BenefitEnrollmentStatus.Waived;
        Remark = remark ?? Remark;
        base.Update();
    }

    public void Terminate(DateTime coverageEnd, string? remark)
    {
        Status = BenefitEnrollmentStatus.Terminated;
        CoverageEnd = coverageEnd;
        Remark = remark ?? Remark;
        base.Update();
    }

    public void UpdateElection(decimal? electedContribution, string? remark)
    {
        if (electedContribution is < 0) throw new ArgumentException("Elected contribution cannot be negative.");
        ElectedEmployeeContribution = electedContribution;
        Remark = remark;
        base.Update();
    }
}
