using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a career-path change request (HC169) — a light approval flow.</summary>
public enum CareerPathChangeStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3
}

/// <summary>
/// A request by (or for) an employee to change their career path (HC169), subject to managerial review.
/// A submitted request can be approved (which reassigns the path) or rejected with a note.
/// </summary>
public class CareerPathChangeRequest : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid? CurrentCareerPathId { get; private set; }
    public Guid RequestedCareerPathId { get; private set; }
    public string? Reason { get; private set; }
    public CareerPathChangeStatus Status { get; private set; } = CareerPathChangeStatus.Draft;
    public string? DecisionNotes { get; private set; }
    public DateTime? DecidedAt { get; private set; }

    private Employee? _employee;
    public Employee? Employee => _employee;

    private CareerPathChangeRequest() : base() { }

    public static CareerPathChangeRequest Create(Guid employeeId, Guid? currentCareerPathId, Guid requestedCareerPathId, string? reason)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (requestedCareerPathId == Guid.Empty) throw new ArgumentException("A requested career path is required.", nameof(requestedCareerPathId));
        return new CareerPathChangeRequest
        {
            EmployeeId = employeeId,
            CurrentCareerPathId = currentCareerPathId,
            RequestedCareerPathId = requestedCareerPathId,
            Reason = reason,
        };
    }

    public void Update(Guid? currentCareerPathId, Guid requestedCareerPathId, string? reason)
    {
        EnsureEditable();
        CurrentCareerPathId = currentCareerPathId;
        RequestedCareerPathId = requestedCareerPathId;
        Reason = reason;
        base.Update();
    }

    public void Submit()
    {
        EnsureEditable();
        Status = CareerPathChangeStatus.Submitted;
        base.Update();
    }

    public void Approve(string? decisionNotes)
    {
        if (Status != CareerPathChangeStatus.Submitted)
            throw new ArgumentException("Only a submitted request can be approved.");
        Status = CareerPathChangeStatus.Approved;
        DecisionNotes = decisionNotes;
        DecidedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Reject(string? decisionNotes)
    {
        if (Status != CareerPathChangeStatus.Submitted)
            throw new ArgumentException("Only a submitted request can be rejected.");
        Status = CareerPathChangeStatus.Rejected;
        DecisionNotes = decisionNotes;
        DecidedAt = DateTime.UtcNow;
        base.Update();
    }

    private void EnsureEditable()
    {
        if (Status is CareerPathChangeStatus.Approved or CareerPathChangeStatus.Rejected)
            throw new ArgumentException("A decided request can no longer be modified.");
    }
}
