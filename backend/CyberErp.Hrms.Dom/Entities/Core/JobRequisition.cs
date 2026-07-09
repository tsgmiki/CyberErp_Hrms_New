using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Requisition lifecycle (HC086).</summary>
public enum RequisitionStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Posted = 3,       // published to the selected channel(s) (HC088)
    Closed = 4,
    Cancelled = 5,
    Rejected = 6      // workflow outcome — editable and resubmittable
}

/// <summary>Where a requisition is advertised (HC088).</summary>
public enum PostingChannel
{
    Internal = 0,     // internal job market — current employees only
    External = 1,     // public career portal
    Both = 2
}

/// <summary>
/// Job requisition (HC084–HC088, HC091): the concrete, approvable vacancy raised from an APPROVED
/// hiring request (HC080 gate). Role details default from the position class (title, grade,
/// qualifications, experience, skills — HC084) and stay editable; approval routes through the
/// generic workflow engine (entity type "JobRequisition", HC085); posting publishes to the
/// internal and/or external market with an auto-generated, customizable posting text (HC088/HC091).
/// </summary>
public class JobRequisition : BaseEntity, IAggregateRoot, IAuditable
{
    public string RequisitionNumber { get; private set; } = string.Empty;
    /// <summary>The approved hiring need this requisition executes (HC080).</summary>
    public Guid HiringRequestId { get; private set; }
    public Guid OrganizationUnitId { get; private set; }
    public Guid PositionClassId { get; private set; }
    public Guid? WorkLocationId { get; private set; }
    public int NumberOfPositions { get; private set; } = 1;
    public PlannedEmploymentType EmploymentType { get; private set; }

    // Role details (defaulted from the position class, editable — HC084)
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? MinQualifications { get; private set; }
    public int? MinExperienceYears { get; private set; }
    public string? Skills { get; private set; }
    /// <summary>Compensation reference for the vacancy (offer validation in the offer stage).</summary>
    public Guid? SalaryScaleId { get; private set; }

    // Posting (HC088/HC091)
    public PostingChannel PostingChannel { get; private set; } = PostingChannel.Internal;
    /// <summary>Advertisement text — auto-generated from the requisition details, customizable.</summary>
    public string? PostingText { get; private set; }
    public DateTime? OpenFrom { get; private set; }
    public DateTime? OpenUntil { get; private set; }

    public RequisitionStatus Status { get; private set; } = RequisitionStatus.Draft;
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? PostedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private readonly List<RequisitionScreeningCriterion> _screeningCriteria = [];
    /// <summary>HR-defined screening criteria applied to this vacancy's applicants (HC095).</summary>
    public IReadOnlyCollection<RequisitionScreeningCriterion> ScreeningCriteria => _screeningCriteria;

    private JobRequisition() : base() { }

    public static JobRequisition Create(
        string requisitionNumber,
        Guid hiringRequestId,
        Guid organizationUnitId,
        Guid positionClassId,
        int numberOfPositions,
        PlannedEmploymentType employmentType,
        string title,
        string? description = null,
        string? minQualifications = null,
        int? minExperienceYears = null,
        string? skills = null,
        Guid? workLocationId = null,
        Guid? salaryScaleId = null)
    {
        Guard(requisitionNumber, hiringRequestId, organizationUnitId, positionClassId, numberOfPositions, title);
        return new JobRequisition
        {
            RequisitionNumber = requisitionNumber,
            HiringRequestId = hiringRequestId,
            OrganizationUnitId = organizationUnitId,
            PositionClassId = positionClassId,
            NumberOfPositions = numberOfPositions,
            EmploymentType = employmentType,
            Title = title,
            Description = description,
            MinQualifications = minQualifications,
            MinExperienceYears = minExperienceYears,
            Skills = skills,
            WorkLocationId = workLocationId,
            SalaryScaleId = salaryScaleId
        };
    }

    /// <summary>Corrections while editable (Draft or Rejected).</summary>
    public void Update(
        int numberOfPositions,
        PlannedEmploymentType employmentType,
        string title,
        string? description,
        string? minQualifications,
        int? minExperienceYears,
        string? skills,
        Guid? workLocationId,
        Guid? salaryScaleId)
    {
        EnsureEditable();
        if (numberOfPositions < 1)
            throw new ArgumentException("At least one position is required.", nameof(numberOfPositions));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("The requisition title cannot be empty.", nameof(title));
        NumberOfPositions = numberOfPositions;
        EmploymentType = employmentType;
        Title = title;
        Description = description;
        MinQualifications = minQualifications;
        MinExperienceYears = minExperienceYears;
        Skills = skills;
        WorkLocationId = workLocationId;
        SalaryScaleId = salaryScaleId;
        base.Update();
    }

    /// <summary>Replaces the screening criteria (HC095) — only while editable.</summary>
    public void SetScreeningCriteria(IEnumerable<ScreeningCriterionSpec> criteria)
    {
        EnsureEditable();
        _screeningCriteria.Clear();
        foreach (var spec in criteria)
            _screeningCriteria.Add(RequisitionScreeningCriterion.Create(Id, spec));
        base.Update();
    }

    /// <summary>Sets the advertisement (channel, text, window). Editable until the requisition closes.</summary>
    public void SetPosting(PostingChannel channel, string? postingText, DateTime? openFrom, DateTime? openUntil)
    {
        if (Status is RequisitionStatus.Closed or RequisitionStatus.Cancelled)
            throw new InvalidOperationException($"A {Status} requisition's posting can no longer change.");
        if (openFrom.HasValue && openUntil.HasValue && openUntil < openFrom)
            throw new ArgumentException("The posting close date cannot be before its open date.", nameof(openUntil));
        PostingChannel = channel;
        PostingText = postingText;
        OpenFrom = openFrom;
        OpenUntil = openUntil;
        base.Update();
    }

    public void Submit()
    {
        EnsureEditable();
        Status = RequisitionStatus.PendingApproval;
        SubmittedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Approve()
    {
        if (Status != RequisitionStatus.PendingApproval)
            throw new InvalidOperationException($"Only a pending requisition can be approved (current: {Status}).");
        Status = RequisitionStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Reject()
    {
        if (Status != RequisitionStatus.PendingApproval)
            throw new InvalidOperationException($"Only a pending requisition can be rejected (current: {Status}).");
        Status = RequisitionStatus.Rejected;
        base.Update();
    }

    /// <summary>Publishes the approved requisition to its posting channel(s) (HC088).</summary>
    public void Post()
    {
        if (Status != RequisitionStatus.Approved)
            throw new InvalidOperationException($"Only an approved requisition can be posted (current: {Status}).");
        if (string.IsNullOrWhiteSpace(PostingText))
            throw new InvalidOperationException("Generate or write the posting text before publishing.");
        Status = RequisitionStatus.Posted;
        PostedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Close()
    {
        if (Status is not (RequisitionStatus.Approved or RequisitionStatus.Posted))
            throw new InvalidOperationException($"Only an approved or posted requisition can be closed (current: {Status}).");
        Status = RequisitionStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Cancel()
    {
        if (Status is RequisitionStatus.Closed or RequisitionStatus.Cancelled or RequisitionStatus.Posted)
            throw new InvalidOperationException($"A {Status} requisition cannot be cancelled — close it instead.");
        Status = RequisitionStatus.Cancelled;
        base.Update();
    }

    private void EnsureEditable()
    {
        if (Status is not (RequisitionStatus.Draft or RequisitionStatus.Rejected))
            throw new InvalidOperationException($"A {Status} requisition can no longer be edited.");
    }

    private static void Guard(string number, Guid requestId, Guid unitId, Guid classId, int positions, string title)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Requisition number cannot be empty.", nameof(number));
        if (requestId == Guid.Empty)
            throw new ArgumentException("A requisition must execute an approved hiring request.", nameof(requestId));
        if (unitId == Guid.Empty)
            throw new ArgumentException("An organization unit is required.", nameof(unitId));
        if (classId == Guid.Empty)
            throw new ArgumentException("A role (position class) is required.", nameof(classId));
        if (positions < 1)
            throw new ArgumentException("At least one position is required.", nameof(positions));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("The requisition title cannot be empty.", nameof(title));
    }
}

/// <summary>Who evaluates a screening criterion: a staff member, an external individual, or a body.</summary>
public enum CriterionEvaluatorType
{
    None = 0,            // scored by HR / unassigned
    Employee = 1,        // an internal employee
    ExternalPerson = 2,  // a named external individual
    Organization = 3     // an external organization / institution
}

/// <summary>Input spec for one screening criterion (see <see cref="JobRequisition.SetScreeningCriteria"/>).</summary>
public record ScreeningCriterionSpec(
    string Name,
    bool IsMandatory,
    int Weight,
    CriterionEvaluatorType EvaluatorType = CriterionEvaluatorType.None,
    Guid? EvaluatorEmployeeId = null,
    string? EvaluatorName = null);

/// <summary>
/// One HR-defined screening criterion of a requisition (HC095), optionally assigned to a specific
/// evaluator — an internal employee, an external person, or an organization — who scores the
/// applicants on it; the weighted criterion scores roll up into the application's total.
/// </summary>
public class RequisitionScreeningCriterion : BaseEntity
{
    public Guid RequisitionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>A candidate failing a mandatory criterion is screened out.</summary>
    public bool IsMandatory { get; private set; }
    /// <summary>Relative weight for the total-score calculation.</summary>
    public int Weight { get; private set; } = 1;
    public CriterionEvaluatorType EvaluatorType { get; private set; } = CriterionEvaluatorType.None;
    /// <summary>The assigned internal evaluator (when <see cref="EvaluatorType"/> is Employee).</summary>
    public Guid? EvaluatorEmployeeId { get; private set; }
    /// <summary>Display name of an external person / organization evaluator (or a resolved employee snapshot).</summary>
    public string? EvaluatorName { get; private set; }

    private RequisitionScreeningCriterion() : base() { }

    public static RequisitionScreeningCriterion Create(Guid requisitionId, ScreeningCriterionSpec spec)
    {
        if (string.IsNullOrWhiteSpace(spec.Name))
            throw new ArgumentException("Criterion name cannot be empty.", nameof(spec));
        if (spec.Weight < 1)
            throw new ArgumentException("Criterion weight must be at least 1.", nameof(spec));
        if (spec.EvaluatorType == CriterionEvaluatorType.Employee && !spec.EvaluatorEmployeeId.HasValue)
            throw new ArgumentException("An employee evaluator needs the employee reference.", nameof(spec));
        if (spec.EvaluatorType is CriterionEvaluatorType.ExternalPerson or CriterionEvaluatorType.Organization
            && string.IsNullOrWhiteSpace(spec.EvaluatorName))
            throw new ArgumentException("An external evaluator needs a name.", nameof(spec));

        return new RequisitionScreeningCriterion
        {
            RequisitionId = requisitionId,
            Name = spec.Name,
            IsMandatory = spec.IsMandatory,
            Weight = spec.Weight,
            EvaluatorType = spec.EvaluatorType,
            EvaluatorEmployeeId = spec.EvaluatorType == CriterionEvaluatorType.Employee ? spec.EvaluatorEmployeeId : null,
            EvaluatorName = spec.EvaluatorName
        };
    }
}
