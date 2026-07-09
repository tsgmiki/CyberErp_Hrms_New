using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a hiring-need assessment (HC077–HC080).</summary>
public enum HiringRequestStatus
{
    Draft = 0,
    Submitted = 1,   // routed through the approval workflow (Directorate → HR → Finance)
    Approved = 2,    // recruitment may start: requisitions can be raised from it (HC080)
    Rejected = 3,    // back to the requester — editable and resubmittable
    Closed = 4       // consumed / withdrawn
}

/// <summary>
/// Hiring Need Assessment (HC077–HC083): a directorate's request to hire, with justification,
/// role requirements, timeline and budget. Routed through the generic workflow engine (entity type
/// "HiringRequest", seeded chain Directorate Head → HR → Finance, HC078). Submission is validated
/// against the live establishment (vacant seats, HC082); an approved request is the mandatory gate
/// for raising a job requisition (HC080) and can reference the workforce plan it fulfils (HC081).
/// </summary>
public class HiringRequest : BaseEntity, IAggregateRoot, IAuditable
{
    public string RequestNumber { get; private set; } = string.Empty;
    /// <summary>The requesting directorate / organizational unit.</summary>
    public Guid OrganizationUnitId { get; private set; }
    /// <summary>The role (job definition) being requested.</summary>
    public Guid PositionClassId { get; private set; }
    public int NumberOfPositions { get; private set; } = 1;
    public PlannedEmploymentType EmploymentType { get; private set; }
    public string Justification { get; private set; } = string.Empty;
    /// <summary>Job requirements captured with the need (defaulted from the position class).</summary>
    public string? JobRequirements { get; private set; }
    public DateTime? ExpectedStartDate { get; private set; }
    public string? TimelineRemarks { get; private set; }
    /// <summary>Estimated recruitment/compensation budget for monitoring (HC083).</summary>
    public decimal EstimatedBudget { get; private set; }
    /// <summary>Workforce plan this need fulfils (HC081). A link snapshot — no FK, plans evolve.</summary>
    public Guid? WorkforcePlanId { get; private set; }
    public HiringRequestStatus Status { get; private set; } = HiringRequestStatus.Draft;
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    private HiringRequest() : base() { }

    public static HiringRequest Create(
        string requestNumber,
        Guid organizationUnitId,
        Guid positionClassId,
        int numberOfPositions,
        PlannedEmploymentType employmentType,
        string justification,
        string? jobRequirements = null,
        DateTime? expectedStartDate = null,
        string? timelineRemarks = null,
        decimal estimatedBudget = 0,
        Guid? workforcePlanId = null)
    {
        Guard(requestNumber, organizationUnitId, positionClassId, numberOfPositions, justification, estimatedBudget);
        return new HiringRequest
        {
            RequestNumber = requestNumber,
            OrganizationUnitId = organizationUnitId,
            PositionClassId = positionClassId,
            NumberOfPositions = numberOfPositions,
            EmploymentType = employmentType,
            Justification = justification,
            JobRequirements = jobRequirements,
            ExpectedStartDate = expectedStartDate,
            TimelineRemarks = timelineRemarks,
            EstimatedBudget = estimatedBudget,
            WorkforcePlanId = workforcePlanId
        };
    }

    /// <summary>Corrections while the request is editable (Draft or Rejected).</summary>
    public void Update(
        Guid organizationUnitId,
        Guid positionClassId,
        int numberOfPositions,
        PlannedEmploymentType employmentType,
        string justification,
        string? jobRequirements,
        DateTime? expectedStartDate,
        string? timelineRemarks,
        decimal estimatedBudget,
        Guid? workforcePlanId)
    {
        EnsureEditable();
        Guard(RequestNumber, organizationUnitId, positionClassId, numberOfPositions, justification, estimatedBudget);
        OrganizationUnitId = organizationUnitId;
        PositionClassId = positionClassId;
        NumberOfPositions = numberOfPositions;
        EmploymentType = employmentType;
        Justification = justification;
        JobRequirements = jobRequirements;
        ExpectedStartDate = expectedStartDate;
        TimelineRemarks = timelineRemarks;
        EstimatedBudget = estimatedBudget;
        WorkforcePlanId = workforcePlanId;
        base.Update();
    }

    public void Submit()
    {
        EnsureEditable();
        Status = HiringRequestStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Workflow outcome: recruitment may start — requisitions can now be raised (HC080).</summary>
    public void Approve()
    {
        if (Status != HiringRequestStatus.Submitted)
            throw new InvalidOperationException($"Only a submitted hiring request can be approved (current: {Status}).");
        Status = HiringRequestStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        base.Update();
    }

    /// <summary>Workflow outcome: back to the requester — still editable and resubmittable.</summary>
    public void Reject()
    {
        if (Status != HiringRequestStatus.Submitted)
            throw new InvalidOperationException($"Only a submitted hiring request can be rejected (current: {Status}).");
        Status = HiringRequestStatus.Rejected;
        base.Update();
    }

    public void Close()
    {
        if (Status is HiringRequestStatus.Closed)
            throw new InvalidOperationException("The hiring request is already closed.");
        Status = HiringRequestStatus.Closed;
        base.Update();
    }

    private void EnsureEditable()
    {
        if (Status is not (HiringRequestStatus.Draft or HiringRequestStatus.Rejected))
            throw new InvalidOperationException($"A {Status} hiring request can no longer be edited.");
    }

    private static void Guard(string requestNumber, Guid unitId, Guid classId, int positions, string justification, decimal budget)
    {
        if (string.IsNullOrWhiteSpace(requestNumber))
            throw new ArgumentException("Request number cannot be empty.", nameof(requestNumber));
        if (unitId == Guid.Empty)
            throw new ArgumentException("The requesting organization unit is required.", nameof(unitId));
        if (classId == Guid.Empty)
            throw new ArgumentException("A role (position class) is required.", nameof(classId));
        if (positions < 1)
            throw new ArgumentException("At least one position must be requested.", nameof(positions));
        if (string.IsNullOrWhiteSpace(justification))
            throw new ArgumentException("A justification is required for a hiring need.", nameof(justification));
        if (budget < 0)
            throw new ArgumentException("The estimated budget cannot be negative.", nameof(budget));
    }
}
