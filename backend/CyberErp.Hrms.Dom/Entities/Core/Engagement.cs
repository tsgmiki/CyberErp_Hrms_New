using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Management pipeline for an employee suggestion (HC203).</summary>
public enum SuggestionStatus
{
    New = 0,
    UnderReview = 1,
    Actioned = 2,
    Closed = 3
}

/// <summary>
/// An employee suggestion / idea / feedback to management (HC203). Anonymous submissions (HC207)
/// carry NO employee link and have their CreatedBy stamp cleared; the entity is intentionally NOT
/// IAuditable so the audit trail cannot deanonymize a submitter either.
/// </summary>
public class Suggestion : BaseEntity, IAggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsAnonymous { get; private set; }
    /// <summary>Null for anonymous submissions — by design there is nothing to join back to.</summary>
    public Guid? EmployeeId { get; private set; }
    public SuggestionStatus Status { get; private set; } = SuggestionStatus.New;
    public string? ManagementResponse { get; private set; }
    public DateTime SubmittedOn { get; private set; }
    public DateTime? RespondedOn { get; private set; }

    private Suggestion() : base() { }

    public static Suggestion Create(string title, string body, bool isAnonymous, Guid? employeeId, DateTime submittedOn)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("The suggestion body is required.", nameof(body));
        if (isAnonymous && employeeId.HasValue)
            throw new ArgumentException("An anonymous suggestion cannot carry an employee.", nameof(employeeId));
        if (!isAnonymous && !employeeId.HasValue)
            throw new ArgumentException("A named suggestion needs its employee.", nameof(employeeId));
        return new Suggestion
        {
            Title = title,
            Body = body,
            IsAnonymous = isAnonymous,
            EmployeeId = employeeId,
            SubmittedOn = submittedOn
        };
    }

    /// <summary>Management review (HC203): move the pipeline and record the response.</summary>
    public void Respond(SuggestionStatus status, string? managementResponse, DateTime respondedOn)
    {
        if (status == SuggestionStatus.New)
            throw new ArgumentException("Responding moves the suggestion beyond New.", nameof(status));
        Status = status;
        ManagementResponse = managementResponse;
        RespondedOn = respondedOn;
        base.Update();
    }
}

/// <summary>Resolution pipeline of a grievance (HC205).</summary>
public enum GrievanceStatus
{
    Submitted = 0,
    UnderReview = 1,
    Resolved = 2,
    Closed = 3
}

public enum GrievanceSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// An escalated issue / grievance (HC205) tracked from submission to resolution: severity,
/// confidentiality, an assigned handler and a progress note trail. Visible only to the grievant,
/// the assignee and HR — managers get no subtree view (grievances are often about them).
/// </summary>
public class Grievance : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public GrievanceSeverity Severity { get; private set; } = GrievanceSeverity.Medium;
    public bool IsConfidential { get; private set; } = true;
    public GrievanceStatus Status { get; private set; } = GrievanceStatus.Submitted;
    public Guid? AssignedToEmployeeId { get; private set; }
    public string? Resolution { get; private set; }
    public DateTime SubmittedOn { get; private set; }
    public DateTime? ResolvedOn { get; private set; }

    private readonly List<GrievanceNote> _notes = [];
    public IReadOnlyCollection<GrievanceNote> Notes => _notes;

    private Grievance() : base() { }

    public static Grievance Create(Guid employeeId, string category, string subject, string details,
        GrievanceSeverity severity, bool isConfidential, DateTime submittedOn)
    {
        Guard(employeeId, category, subject, details);
        return new Grievance
        {
            EmployeeId = employeeId,
            Category = category,
            Subject = subject,
            Details = details,
            Severity = severity,
            IsConfidential = isConfidential,
            SubmittedOn = submittedOn
        };
    }

    public void Assign(Guid assigneeEmployeeId)
    {
        EnsureOpen();
        AssignedToEmployeeId = assigneeEmployeeId;
        if (Status == GrievanceStatus.Submitted) Status = GrievanceStatus.UnderReview;
        base.Update();
    }

    public void Resolve(string resolution, DateTime resolvedOn)
    {
        if (string.IsNullOrWhiteSpace(resolution))
            throw new ArgumentException("A resolution summary is required.", nameof(resolution));
        EnsureOpen();
        Status = GrievanceStatus.Resolved;
        Resolution = resolution;
        ResolvedOn = resolvedOn;
        base.Update();
    }

    public void Close()
    {
        if (Status != GrievanceStatus.Resolved)
            throw new InvalidOperationException($"Only a resolved grievance can be closed (current: {Status}).");
        Status = GrievanceStatus.Closed;
        base.Update();
    }

    private void EnsureOpen()
    {
        if (Status is GrievanceStatus.Resolved or GrievanceStatus.Closed)
            throw new InvalidOperationException($"The grievance is already {Status}.");
    }

    private static void Guard(Guid employeeId, string category, string subject, string details)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("A category is required.", nameof(category));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("A subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(details))
            throw new ArgumentException("Details are required.", nameof(details));
    }
}

/// <summary>One progress note on a grievance's resolution trail (HC205).</summary>
public class GrievanceNote : BaseEntity
{
    public Guid GrievanceId { get; private set; }
    public Guid AuthorEmployeeId { get; private set; }
    public string Note { get; private set; } = string.Empty;

    private GrievanceNote() : base() { }

    public static GrievanceNote Create(Guid grievanceId, Guid authorEmployeeId, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("A note cannot be empty.", nameof(note));
        return new GrievanceNote
        {
            GrievanceId = grievanceId,
            AuthorEmployeeId = authorEmployeeId,
            Note = note
        };
    }
}

/// <summary>Who an announcement reaches (HC206).</summary>
public enum AnnouncementAudience
{
    All = 0,
    Branch = 1,
    Unit = 2
}

/// <summary>
/// An organizational announcement (HC206): news, events and updates, published for a window and
/// optionally TARGETED at one branch or one organization unit (a unit reaches its whole subtree).
/// </summary>
public class Announcement : BaseEntity, IAggregateRoot, IAuditable
{
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public AnnouncementAudience Audience { get; private set; } = AnnouncementAudience.All;
    public Guid? BranchId { get; private set; }
    public Guid? OrganizationUnitId { get; private set; }
    public DateTime PublishFrom { get; private set; }
    public DateTime? PublishUntil { get; private set; }
    public bool IsPinned { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Announcement() : base() { }

    public static Announcement Create(string title, string body, AnnouncementAudience audience,
        Guid? branchId, Guid? organizationUnitId, DateTime publishFrom, DateTime? publishUntil,
        bool isPinned = false, bool isActive = true)
    {
        Guard(title, body, audience, branchId, organizationUnitId, publishFrom, publishUntil);
        return new Announcement
        {
            Title = title,
            Body = body,
            Audience = audience,
            BranchId = audience == AnnouncementAudience.Branch ? branchId : null,
            OrganizationUnitId = audience == AnnouncementAudience.Unit ? organizationUnitId : null,
            PublishFrom = publishFrom,
            PublishUntil = publishUntil,
            IsPinned = isPinned,
            IsActive = isActive
        };
    }

    public void Update(string title, string body, AnnouncementAudience audience,
        Guid? branchId, Guid? organizationUnitId, DateTime publishFrom, DateTime? publishUntil,
        bool isPinned, bool isActive)
    {
        Guard(title, body, audience, branchId, organizationUnitId, publishFrom, publishUntil);
        Title = title;
        Body = body;
        Audience = audience;
        BranchId = audience == AnnouncementAudience.Branch ? branchId : null;
        OrganizationUnitId = audience == AnnouncementAudience.Unit ? organizationUnitId : null;
        PublishFrom = publishFrom;
        PublishUntil = publishUntil;
        IsPinned = isPinned;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string title, string body, AnnouncementAudience audience,
        Guid? branchId, Guid? organizationUnitId, DateTime publishFrom, DateTime? publishUntil)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("The announcement body is required.", nameof(body));
        if (audience == AnnouncementAudience.Branch && !branchId.HasValue)
            throw new ArgumentException("A branch-targeted announcement needs its branch.", nameof(branchId));
        if (audience == AnnouncementAudience.Unit && !organizationUnitId.HasValue)
            throw new ArgumentException("A unit-targeted announcement needs its unit.", nameof(organizationUnitId));
        if (publishUntil.HasValue && publishUntil.Value < publishFrom)
            throw new ArgumentException("The publish window cannot end before it starts.", nameof(publishUntil));
    }
}
