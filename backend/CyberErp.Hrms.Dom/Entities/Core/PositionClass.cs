using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A reusable job/position definition (HC004-HC005): title, pay point (salary scale), category,
/// headcount and eligibility requirements. A <see cref="Position"/> places a class at a specific
/// organization unit. Shared across branches (not branch-scoped). Reporting lines are class → class
/// via <see cref="ReportsToPositionClassId"/>. The pay grade is derived from the linked
/// <see cref="SalaryScale"/> (which carries grade + step + exact salary).
/// </summary>
public class PositionClass : BaseEntity, IAggregateRoot, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public int AllocatedHeadcount { get; private set; }
    public string? MinQualifications { get; private set; }
    public int? MinExperienceYears { get; private set; }
    public string? Skills { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public int? MinimumAge { get; private set; }
    public int? MaximumAge { get; private set; }
    public decimal? WeeklyWorkingHours { get; private set; }

    public Guid SalaryScaleId { get; private set; }
    private SalaryScale? _salaryScale;
    public SalaryScale? SalaryScale => _salaryScale;

    public Guid JobCategoryId { get; private set; }
    private JobCategory? _jobCategory;
    public JobCategory? JobCategory => _jobCategory;

    public Guid? WorkLocationId { get; private set; }
    private WorkLocation? _workLocation;
    public WorkLocation? WorkLocation => _workLocation;

    // Self-referencing reporting line (class reports to class)
    public Guid? ReportsToPositionClassId { get; private set; }
    private PositionClass? _reportsToPositionClass;
    public PositionClass? ReportsToPositionClass => _reportsToPositionClass;

    private PositionClass() : base() { }

    public static PositionClass Create(
        string code,
        string title,
        Guid salaryScaleId,
        Guid jobCategoryId,
        int allocatedHeadcount = 1,
        Guid? reportsToPositionClassId = null,
        Guid? workLocationId = null,
        string? minQualifications = null,
        int? minExperienceYears = null,
        string? skills = null,
        string? description = null,
        bool isActive = true,
        int? minimumAge = null,
        int? maximumAge = null,
        decimal? weeklyWorkingHours = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Position class code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Position class title cannot be empty.", nameof(title));
        if (salaryScaleId == Guid.Empty)
            throw new ArgumentException("A position class must have a salary scale.", nameof(salaryScaleId));
        if (jobCategoryId == Guid.Empty)
            throw new ArgumentException("A position class must have a job category.", nameof(jobCategoryId));
        if (allocatedHeadcount < 0)
            throw new ArgumentException("Allocated headcount cannot be negative.", nameof(allocatedHeadcount));
        ValidateAgeRange(minimumAge, maximumAge);

        return new PositionClass
        {
            Code = code,
            Title = title,
            SalaryScaleId = salaryScaleId,
            JobCategoryId = jobCategoryId,
            AllocatedHeadcount = allocatedHeadcount,
            ReportsToPositionClassId = reportsToPositionClassId,
            WorkLocationId = workLocationId,
            MinQualifications = minQualifications,
            MinExperienceYears = minExperienceYears,
            Skills = skills,
            Description = description,
            IsActive = isActive,
            MinimumAge = minimumAge,
            MaximumAge = maximumAge,
            WeeklyWorkingHours = weeklyWorkingHours
        };
    }

    public void Update(
        string code,
        string title,
        Guid salaryScaleId,
        Guid jobCategoryId,
        int allocatedHeadcount,
        Guid? reportsToPositionClassId,
        Guid? workLocationId,
        string? minQualifications,
        int? minExperienceYears,
        string? skills,
        string? description,
        bool isActive,
        int? minimumAge = null,
        int? maximumAge = null,
        decimal? weeklyWorkingHours = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Position class code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Position class title cannot be empty.", nameof(title));
        if (salaryScaleId == Guid.Empty)
            throw new ArgumentException("A position class must have a salary scale.", nameof(salaryScaleId));
        if (jobCategoryId == Guid.Empty)
            throw new ArgumentException("A position class must have a job category.", nameof(jobCategoryId));
        if (reportsToPositionClassId.HasValue && reportsToPositionClassId.Value == Id)
            throw new ArgumentException("A position class cannot report to itself.", nameof(reportsToPositionClassId));
        if (allocatedHeadcount < 0)
            throw new ArgumentException("Allocated headcount cannot be negative.", nameof(allocatedHeadcount));
        ValidateAgeRange(minimumAge, maximumAge);

        Code = code;
        Title = title;
        SalaryScaleId = salaryScaleId;
        JobCategoryId = jobCategoryId;
        AllocatedHeadcount = allocatedHeadcount;
        ReportsToPositionClassId = reportsToPositionClassId;
        WorkLocationId = workLocationId;
        MinQualifications = minQualifications;
        MinExperienceYears = minExperienceYears;
        Skills = skills;
        Description = description;
        IsActive = isActive;
        MinimumAge = minimumAge;
        MaximumAge = maximumAge;
        WeeklyWorkingHours = weeklyWorkingHours;
        base.Update();
    }

    private static void ValidateAgeRange(int? minimumAge, int? maximumAge)
    {
        if (minimumAge.HasValue && minimumAge.Value < 0)
            throw new ArgumentException("Minimum age cannot be negative.", nameof(minimumAge));
        if (maximumAge.HasValue && maximumAge.Value < 0)
            throw new ArgumentException("Maximum age cannot be negative.", nameof(maximumAge));
        if (minimumAge.HasValue && maximumAge.HasValue && minimumAge.Value > maximumAge.Value)
            throw new ArgumentException("Minimum age cannot be greater than maximum age.", nameof(minimumAge));
    }
}
