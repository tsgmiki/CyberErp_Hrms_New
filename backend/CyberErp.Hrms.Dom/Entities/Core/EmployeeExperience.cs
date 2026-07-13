using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Employment-history entry with a previous employer (HC018): role, organization, duration.</summary>
public class EmployeeExperience : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid PersonId { get; private set; }
    public string Organization { get; private set; } = string.Empty;
    public string JobTitle { get; private set; } = string.Empty;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Responsibilities { get; private set; }
    /// <summary>True for a prior job at ANOTHER employer (manually entered); false for an
    /// internal role auto-registered from an employee movement.</summary>
    public bool IsExternal { get; private set; }
    /// <summary>True when the role was at a governmental organization (affects experience credit rules).</summary>
    public bool IsGovernmental { get; private set; }

    private EmployeeExperience() : base() { }

    public static EmployeeExperience Create(
        Guid personId,
        string organization,
        string jobTitle,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? responsibilities = null,
        bool isExternal = false,
        bool isGovernmental = false)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person is required.", nameof(personId));
        if (string.IsNullOrWhiteSpace(organization))
            throw new ArgumentException("Organization cannot be empty.", nameof(organization));
        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new ArgumentException("Job title cannot be empty.", nameof(jobTitle));
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

        return new EmployeeExperience
        {
            PersonId = personId,
            Organization = organization,
            JobTitle = jobTitle,
            StartDate = startDate,
            EndDate = endDate,
            Responsibilities = responsibilities,
            IsExternal = isExternal,
            IsGovernmental = isGovernmental
        };
    }

    public void Update(
        string organization,
        string jobTitle,
        DateTime? startDate,
        DateTime? endDate,
        string? responsibilities,
        bool isExternal,
        bool isGovernmental)
    {
        if (string.IsNullOrWhiteSpace(organization))
            throw new ArgumentException("Organization cannot be empty.", nameof(organization));
        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new ArgumentException("Job title cannot be empty.", nameof(jobTitle));
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

        Organization = organization;
        JobTitle = jobTitle;
        StartDate = startDate;
        EndDate = endDate;
        Responsibilities = responsibilities;
        IsExternal = isExternal;
        IsGovernmental = isGovernmental;
        base.Update();
    }
}
