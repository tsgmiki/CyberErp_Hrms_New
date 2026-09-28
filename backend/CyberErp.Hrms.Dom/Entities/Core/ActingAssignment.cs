using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of an <see cref="ActingAssignment"/>.</summary>
public enum ActingAssignmentStatus
{
    /// <summary>Raised by a qualifying delegation, awaiting sign-off. No pay has changed.</summary>
    PendingApproval = 0,
    /// <summary>Approved and in force — the deputy is being paid the acting rate.</summary>
    Active = 1,
    /// <summary>Concluded normally; pay reverted and the period recorded as experience.</summary>
    Concluded = 2,
    /// <summary>Refused at approval. No pay ever changed.</summary>
    Rejected = 3,
    /// <summary>The delegation behind it was withdrawn before, or during, the assignment.</summary>
    Cancelled = 4
}

/// <summary>
/// A deputy paid at the rate of the post they are covering, for the length of a long delegation.
/// </summary>
/// <remarks>
/// <para>Raised automatically when a delegation runs longer than the tenant's configured threshold
/// (<see cref="DelegationPolicy.ActingCompensationMinDays"/>). Standing in for an afternoon is a
/// favour; standing in for a quarter is a job, and the organisation pays for the job.</para>
///
/// <para>⚠️ BOTH SALARIES ARE SNAPSHOTS, taken when the assignment is raised. The acting rate comes
/// from the POST (its PositionClass's salary scale), not from whatever its current holder happens
/// to earn — so a delegator paid above scale does not hand their personal history to a deputy. The
/// original is snapshotted because it is what the deputy reverts to, and reading it back off the
/// employee at conclusion would return the ACTING rate, quietly making the uplift permanent.</para>
///
/// <para>⚠️ Nothing here changes pay by itself. The assignment is raised as
/// <see cref="ActingAssignmentStatus.PendingApproval"/> and only <see cref="Activate"/> — called
/// by the workflow handler once somebody signs — touches the employee. A delegation can be created
/// self-service by a department head; a pay rise must not be.</para>
/// </remarks>
public class ActingAssignment : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>The delegation that raised this.</summary>
    public Guid DelegationId { get; private set; }
    /// <summary>The deputy — the person standing in and being paid for it.</summary>
    public Guid EmployeeId { get; private set; }
    /// <summary>The approver whose post is being covered.</summary>
    public Guid CoveringForEmployeeId { get; private set; }
    /// <summary>The post itself, when the covered approver holds one.</summary>
    public Guid? PositionId { get; private set; }
    /// <summary>Post title at the time, so the record reads without joins years later.</summary>
    public string PositionTitle { get; private set; } = string.Empty;

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    /// <summary>The post's scale rate — what the deputy is paid while acting.</summary>
    public decimal ActingSalary { get; private set; }
    /// <summary>The deputy's own salary, to revert to. Null when they had none recorded.</summary>
    public decimal? OriginalSalary { get; private set; }

    public ActingAssignmentStatus Status { get; private set; } = ActingAssignmentStatus.PendingApproval;
    public DateTime? ActivatedAt { get; private set; }
    public DateTime? ConcludedAt { get; private set; }
    /// <summary>Set once the period has been written to the employee's experience record.</summary>
    public bool ExperienceRecorded { get; private set; }
    public string? Notes { get; private set; }

    private ActingAssignment() : base() { }

    public static ActingAssignment Create(
        Guid delegationId, Guid employeeId, Guid coveringForEmployeeId, Guid? positionId,
        string positionTitle, DateTime startDate, DateTime endDate,
        decimal actingSalary, decimal? originalSalary, string? notes = null)
    {
        if (delegationId == Guid.Empty)
            throw new ArgumentException("The delegation is required.", nameof(delegationId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("The deputy is required.", nameof(employeeId));
        if (endDate.Date < startDate.Date)
            throw new ArgumentException("An assignment cannot end before it starts.", nameof(endDate));
        if (actingSalary < 0)
            throw new ArgumentException("An acting salary cannot be negative.", nameof(actingSalary));

        return new ActingAssignment
        {
            DelegationId = delegationId,
            EmployeeId = employeeId,
            CoveringForEmployeeId = coveringForEmployeeId,
            PositionId = positionId,
            PositionTitle = positionTitle,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            ActingSalary = actingSalary,
            OriginalSalary = originalSalary,
            Notes = notes
        };
    }

    /// <summary>Approved — the deputy starts being paid the acting rate.</summary>
    public void Activate()
    {
        if (Status != ActingAssignmentStatus.PendingApproval)
            throw new InvalidOperationException($"Only a pending assignment can be activated (current: {Status}).");
        Status = ActingAssignmentStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        base.Update();
    }

    public void Reject(string? reason)
    {
        if (Status != ActingAssignmentStatus.PendingApproval)
            throw new InvalidOperationException($"Only a pending assignment can be rejected (current: {Status}).");
        Status = ActingAssignmentStatus.Rejected;
        Notes = reason ?? Notes;
        base.Update();
    }

    /// <summary>
    /// The assignment has run its course: pay reverts, and the period becomes experience.
    /// </summary>
    public void Conclude(bool experienceRecorded)
    {
        if (Status != ActingAssignmentStatus.Active)
            throw new InvalidOperationException($"Only an active assignment can be concluded (current: {Status}).");
        Status = ActingAssignmentStatus.Concluded;
        ConcludedAt = DateTime.UtcNow;
        ExperienceRecorded = experienceRecorded;
        base.Update();
    }

    /// <summary>
    /// The delegation behind it was withdrawn.
    /// </summary>
    /// <remarks>
    /// ⚠️ Reports whether pay had actually STARTED, because the caller's next step differs: an
    /// assignment cancelled while still pending never touched the employee and needs no unwinding,
    /// while one cancelled mid-flight has a salary to put back.
    /// </remarks>
    public bool Cancel(string? reason)
    {
        var wasActive = Status == ActingAssignmentStatus.Active;
        if (Status is ActingAssignmentStatus.Concluded or ActingAssignmentStatus.Rejected
            or ActingAssignmentStatus.Cancelled)
            return false;

        Status = ActingAssignmentStatus.Cancelled;
        ConcludedAt = DateTime.UtcNow;
        Notes = reason ?? Notes;
        base.Update();
        return wasActive;
    }

    /// <summary>
    /// Re-snapshot what the deputy is actually on, at approval time.
    /// </summary>
    /// <remarks>
    /// ⚠️ Approval can land days or weeks after the assignment was raised. An ordinary increment
    /// in between would otherwise be erased when this assignment eventually reverts, because the
    /// revert restores the figure captured at RAISE time. Re-taking it at activation means the
    /// deputy goes back to their real salary, not to a stale one.
    /// </remarks>
    public void CaptureOriginalSalary(decimal? salary)
    {
        if (Status != ActingAssignmentStatus.PendingApproval)
            throw new InvalidOperationException("The original salary can only be captured before activation.");
        OriginalSalary = salary;
    }

    /// <summary>Marks the experience row as written, when it is created after conclusion.</summary>
    public void MarkExperienceRecorded()
    {
        if (ExperienceRecorded) return;
        ExperienceRecorded = true;
        base.Update();
    }

    /// <summary>Whole days the assignment covers, inclusive — the figure the experience row carries.</summary>
    public int DurationDays => (int)(EndDate.Date - StartDate.Date).TotalDays + 1;
}
