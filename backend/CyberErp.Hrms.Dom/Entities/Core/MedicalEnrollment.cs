using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Category of a medical-coverage beneficiary (HC237/HC246 reporting).</summary>
public enum BeneficiaryCategory
{
    Employee = 0,
    Spouse = 1,
    Child = 2,
    Parent = 3,
    Pensioner = 4,
    Other = 5
}

/// <summary>Lifecycle of a medical enrollment.</summary>
public enum MedicalEnrollmentStatus
{
    Active = 0,
    Suspended = 1,
    Terminated = 2
}

/// <summary>
/// HC235 — an employee's enrollment in a <see cref="MedicalPlan"/> with its coverage window and the
/// covered <see cref="MedicalBeneficiary"/> people (self + dependents, HC237).
/// </summary>
public class MedicalEnrollment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid MedicalPlanId { get; private set; }
    public DateTime EnrolledOn { get; private set; }
    public DateTime CoverageStart { get; private set; }
    public DateTime? CoverageEnd { get; private set; }
    public MedicalEnrollmentStatus Status { get; private set; } = MedicalEnrollmentStatus.Active;
    public string? Remark { get; private set; }

    private readonly List<MedicalBeneficiary> _beneficiaries = [];
    public IReadOnlyCollection<MedicalBeneficiary> Beneficiaries => _beneficiaries;

    private MedicalEnrollment() : base() { }

    public static MedicalEnrollment Create(Guid employeeId, Guid medicalPlanId, DateTime enrolledOn,
        DateTime coverageStart, string? remark)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (medicalPlanId == Guid.Empty) throw new ArgumentException("Medical plan is required.", nameof(medicalPlanId));
        return new MedicalEnrollment
        {
            EmployeeId = employeeId,
            MedicalPlanId = medicalPlanId,
            EnrolledOn = enrolledOn,
            CoverageStart = coverageStart,
            Status = MedicalEnrollmentStatus.Active,
            Remark = remark
        };
    }

    public void UpdateCoverage(DateTime coverageStart, string? remark)
    {
        CoverageStart = coverageStart;
        Remark = remark;
        base.Update();
    }

    public void Suspend()
    {
        if (Status == MedicalEnrollmentStatus.Terminated)
            throw new InvalidOperationException("A terminated enrollment cannot be suspended.");
        Status = MedicalEnrollmentStatus.Suspended;
        base.Update();
    }

    public void Reactivate()
    {
        if (Status == MedicalEnrollmentStatus.Terminated)
            throw new InvalidOperationException("A terminated enrollment cannot be reactivated.");
        Status = MedicalEnrollmentStatus.Active;
        base.Update();
    }

    public void Terminate(DateTime coverageEnd)
    {
        Status = MedicalEnrollmentStatus.Terminated;
        CoverageEnd = coverageEnd;
        base.Update();
    }
}

/// <summary>
/// HC237 — one covered person under a <see cref="MedicalEnrollment"/>: the employee themselves or a
/// dependent (linked to the shared <see cref="EmployeeDependent"/> record). Details are snapshotted so
/// the coverage roster stays readable even if the dependent record changes.
/// </summary>
public class MedicalBeneficiary : BaseEntity
{
    public Guid MedicalEnrollmentId { get; private set; }
    public BeneficiaryCategory Category { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    /// <summary>Link to the underlying dependent record (null for the employee-self beneficiary).</summary>
    public Guid? EmployeeDependentId { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Relationship { get; private set; }
    public bool IsActive { get; private set; } = true;

    private MedicalBeneficiary() : base() { }

    public static MedicalBeneficiary Create(Guid enrollmentId, BeneficiaryCategory category, string fullName,
        Guid? employeeDependentId, DateTime? dateOfBirth, string? relationship)
    {
        if (enrollmentId == Guid.Empty) throw new ArgumentException("Enrollment is required.", nameof(enrollmentId));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Beneficiary name is required.", nameof(fullName));
        return new MedicalBeneficiary
        {
            MedicalEnrollmentId = enrollmentId,
            Category = category,
            FullName = fullName.Trim(),
            EmployeeDependentId = employeeDependentId,
            DateOfBirth = dateOfBirth,
            Relationship = relationship,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        base.Update();
    }
}
