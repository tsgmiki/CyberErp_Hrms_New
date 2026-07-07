using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

public enum Gender
{
    Male = 0,
    Female = 1
}

public enum MaritalStatus
{
    Single = 0,
    Married = 1,
    Divorced = 2,
    Widowed = 3
}

public enum EmploymentStatus
{
    Active = 0,
    Probation = 1,
    OnLeave = 2,
    Suspended = 3,
    Terminated = 4,
    Retired = 5
}

/// <summary>
/// Employee master record (HC015-HC016): personal details, identification numbers and
/// organizational placement. The organization unit is derived from the assigned
/// <see cref="Position"/> (a position belongs to exactly one unit) — it is not stored here.
/// Extended data lives in child collections and dynamic custom fields (HC021).
/// Branch-isolated; every change is audited (HC029) via the SaveChanges interceptor.
/// </summary>
public class Employee : BaseEntity, IAggregateRoot, IBranchScoped, IAuditable
{
    // Personal identity lives on the shared person record (Core.CorePerson).
    public Guid PersonId { get; private set; }
    private Person? _person;
    public Person? Person => _person;

    // Identification (HC016)
    public string EmployeeNumber { get; private set; } = string.Empty;
    public string? NationalId { get; private set; }
    public string? Tin { get; private set; }
    public string? PensionNumber { get; private set; }

    // Employment-record-specific personal details (not part of the CorePerson column set)
    public DateTime? DateOfBirth { get; private set; }
    public string? PlaceOfBirth { get; private set; }
    public string? SpouseName { get; private set; }
    public string? Email { get; private set; }
    /// <summary>Stored photo file name (served via GET Employee/{id}/photo).</summary>
    public string? PhotoUrl { get; private set; }

    // Employment
    public DateTime? HireDate { get; private set; }
    public EmploymentStatus EmploymentStatus { get; private set; } = EmploymentStatus.Active;
    /// <summary>Managerial staff receive the managerial annual-leave entitlement (legacy IsManager).</summary>
    public bool IsManagerial { get; private set; }
    /// <summary>Personal grade (may differ from the position class grade during transitions).</summary>
    public Guid? JobGradeId { get; private set; }
    private JobGrade? _jobGrade;
    public JobGrade? JobGrade => _jobGrade;
    public decimal? Salary { get; private set; }

    // Organizational placement — the unit is derived from the position.
    public Guid? PositionId { get; private set; }
    private Position? _position;
    public Position? Position => _position;

    // Branch isolation (derived from the position's organization unit)
    public Guid? BranchId { get; private set; }
    private Branch? _branch;
    public Branch? Branch => _branch;

    private Employee() : base() { }

    /// <summary>Flags the employee as managerial for leave-entitlement purposes.</summary>
    public void SetManagerial(bool isManagerial)
    {
        IsManagerial = isManagerial;
        base.Update();
    }

    public static Employee Create(
        Guid personId,
        string employeeNumber,
        EmploymentStatus employmentStatus = EmploymentStatus.Active,
        DateTime? dateOfBirth = null,
        string? placeOfBirth = null,
        string? spouseName = null,
        string? email = null,
        string? nationalId = null,
        string? tin = null,
        string? pensionNumber = null,
        DateTime? hireDate = null,
        Guid? positionId = null,
        Guid? jobGradeId = null,
        decimal? salary = null,
        Guid? branchId = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("A person record is required.", nameof(personId));
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new ArgumentException("Employee number cannot be empty.", nameof(employeeNumber));
        if (salary.HasValue && salary < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(salary));

        return new Employee
        {
            PersonId = personId,
            EmployeeNumber = employeeNumber,
            EmploymentStatus = employmentStatus,
            DateOfBirth = dateOfBirth,
            PlaceOfBirth = placeOfBirth,
            SpouseName = spouseName,
            Email = email,
            NationalId = nationalId,
            Tin = tin,
            PensionNumber = pensionNumber,
            HireDate = hireDate,
            PositionId = positionId,
            JobGradeId = jobGradeId,
            Salary = salary,
            BranchId = branchId
        };
    }

    public void Update(
        string employeeNumber,
        EmploymentStatus employmentStatus,
        DateTime? dateOfBirth,
        string? placeOfBirth,
        string? spouseName,
        string? email,
        string? nationalId,
        string? tin,
        string? pensionNumber,
        DateTime? hireDate,
        Guid? positionId,
        Guid? jobGradeId,
        decimal? salary,
        Guid? branchId)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new ArgumentException("Employee number cannot be empty.", nameof(employeeNumber));
        if (salary.HasValue && salary < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(salary));

        EmployeeNumber = employeeNumber;
        EmploymentStatus = employmentStatus;
        DateOfBirth = dateOfBirth;
        PlaceOfBirth = placeOfBirth;
        SpouseName = spouseName;
        Email = email;
        NationalId = nationalId;
        Tin = tin;
        PensionNumber = pensionNumber;
        HireDate = hireDate;
        PositionId = positionId;
        JobGradeId = jobGradeId;
        Salary = salary;
        BranchId = branchId;
        base.Update();
    }

    /// <summary>Sets the stored photo file name after a successful upload (HC015/HC023).</summary>
    public void SetPhoto(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Photo file name cannot be empty.", nameof(fileName));
        PhotoUrl = fileName;
        base.Update();
    }

    /// <summary>
    /// Final-settlement automation of the termination module: the employment ends and the
    /// occupied position is decoupled so it can be reopened (vacancy is recomputed by the caller).
    /// The branch is kept for historical reporting.
    /// </summary>
    public void Terminate()
    {
        EmploymentStatus = EmploymentStatus.Terminated;
        PositionId = null;
        base.Update();
    }

    /// <summary>
    /// Applies an executed personnel movement (transfer / promotion / demotion) to the placement
    /// fields. When the position changes, the branch always follows the new position (isolation
    /// is transitive); grade and salary only change when the movement specifies them.
    /// </summary>
    public void ApplyMovement(bool changePosition, Guid? positionId, Guid? branchId, Guid? jobGradeId, decimal? salary)
    {
        if (salary is < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(salary));

        if (changePosition)
        {
            PositionId = positionId;
            BranchId = branchId;
        }
        if (jobGradeId.HasValue) JobGradeId = jobGradeId;
        if (salary.HasValue) Salary = salary;
        base.Update();
    }
}
