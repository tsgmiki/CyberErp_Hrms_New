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

/// <summary>Nature of the employment contract. Stored as a string (see EmployeeConfiguration).</summary>
public enum EmploymentNature
{
    Permanent = 0,
    Contract = 1
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

    // Employment terms (belong strictly to the employment record, not the shared person)
    public EmploymentNature EmploymentNature { get; private set; } = EmploymentNature.Permanent;
    /// <summary>Contract length in months — required for a Contract nature, null for Permanent.</summary>
    public int? ContractPeriod { get; private set; }
    public bool IsProbation { get; private set; }
    public DateTime? ProbationEndDate { get; private set; }
    /// <summary>Denormalized termination flag (false by default; set true when the employee is terminated).</summary>
    public bool IsTerminated { get; private set; }
    /// <summary>
    /// Pay point: the salary scale (grade + step + amount) the employee is placed on. The employee's
    /// job grade is DERIVED from this scale (SalaryScale.JobGrade) rather than stored separately.
    /// </summary>
    public Guid? SalaryScaleId { get; private set; }
    private SalaryScale? _salaryScale;
    public SalaryScale? SalaryScale => _salaryScale;
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
        decimal? salary = null,
        Guid? branchId = null,
        EmploymentNature employmentNature = EmploymentNature.Permanent,
        int? contractPeriod = null,
        bool isProbation = false,
        DateTime? probationEndDate = null,
        Guid? salaryScaleId = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("A person record is required.", nameof(personId));
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new ArgumentException("Employee number cannot be empty.", nameof(employeeNumber));
        if (salary.HasValue && salary < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(salary));
        ValidateEmploymentTerms(employmentNature, contractPeriod, isProbation, probationEndDate);

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
            SalaryScaleId = salaryScaleId,
            Salary = salary,
            BranchId = branchId,
            EmploymentNature = employmentNature,
            ContractPeriod = employmentNature == EmploymentNature.Contract ? contractPeriod : null,
            IsProbation = isProbation,
            ProbationEndDate = isProbation ? probationEndDate : null
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
        decimal? salary,
        Guid? branchId,
        EmploymentNature employmentNature = EmploymentNature.Permanent,
        int? contractPeriod = null,
        bool isProbation = false,
        DateTime? probationEndDate = null,
        Guid? salaryScaleId = null)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new ArgumentException("Employee number cannot be empty.", nameof(employeeNumber));
        if (salary.HasValue && salary < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(salary));
        ValidateEmploymentTerms(employmentNature, contractPeriod, isProbation, probationEndDate);

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
        SalaryScaleId = salaryScaleId;
        Salary = salary;
        BranchId = branchId;
        EmploymentNature = employmentNature;
        ContractPeriod = employmentNature == EmploymentNature.Contract ? contractPeriod : null;
        IsProbation = isProbation;
        ProbationEndDate = isProbation ? probationEndDate : null;
        base.Update();
    }

    /// <summary>Enforces the conditional invariants: contract period for Contract, end date for probation.</summary>
    private static void ValidateEmploymentTerms(EmploymentNature nature, int? contractPeriod, bool isProbation, DateTime? probationEndDate)
    {
        if (nature == EmploymentNature.Contract && (!contractPeriod.HasValue || contractPeriod.Value <= 0))
            throw new ArgumentException("A contract employee must have a positive contract period.", nameof(contractPeriod));
        if (isProbation && !probationEndDate.HasValue)
            throw new ArgumentException("A probation end date is required when the employee is on probation.", nameof(probationEndDate));
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
        IsTerminated = true;
        IsProbation = false;
        PositionId = null;
        base.Update();
    }

    /// <summary>
    /// Reverses a termination: the employee returns to Active on a (vacant) position — their previous
    /// one when still open, otherwise a newly chosen one. The branch follows the position (isolation is
    /// transitive), and the organization unit/department is derived from it. Salary and pay point are
    /// preserved from before the termination.
    /// </summary>
    public void Reinstate(Guid positionId, Guid? branchId)
    {
        if (!IsTerminated && EmploymentStatus != EmploymentStatus.Terminated)
            throw new InvalidOperationException("Only a terminated employee can be reinstated.");
        if (positionId == Guid.Empty)
            throw new ArgumentException("A position is required to reinstate an employee.", nameof(positionId));

        EmploymentStatus = EmploymentStatus.Active;
        IsTerminated = false;
        PositionId = positionId;
        BranchId = branchId;
        base.Update();
    }

    /// <summary>
    /// Applies an executed personnel movement (transfer / promotion / demotion) to the placement
    /// fields. When the position changes, the branch always follows the new position (isolation is
    /// transitive); salary changes only when the movement specifies it. The job grade is no longer
    /// stored on the employee (it derives from the salary scale), so a grade change is recorded on the
    /// movement for history but is not applied here — reassign the salary scale to change the grade.
    /// </summary>
    public void ApplyMovement(bool changePosition, Guid? positionId, Guid? branchId, decimal? salary)
    {
        if (salary is < 0)
            throw new ArgumentException("Salary cannot be negative.", nameof(salary));

        if (changePosition)
        {
            PositionId = positionId;
            BranchId = branchId;
        }
        if (salary.HasValue) Salary = salary;
        base.Update();
    }
}
