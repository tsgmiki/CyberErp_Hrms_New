using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Who delivers a session.</summary>
public enum TrainerType
{
    Internal = 0,
    External = 1
}

/// <summary>Lifecycle of a scheduled session (HC197).</summary>
public enum TrainingSessionStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2
}

/// <summary>A participant's state on a session (HC198).</summary>
public enum TrainingEnrollmentStatus
{
    Enrolled = 0,
    Completed = 1,
    NoShow = 2,
    Withdrawn = 3
}

/// <summary>
/// A scheduled delivery of a catalog course (HC197): dates, venue, trainer, capacity and the cost
/// lines that feed budget tracking (HC190 — trainer / materials / venue). Recurring deliveries are
/// materialized as a bounded series of sessions. An online/hybrid session carries its meeting URL.
/// </summary>
public class TrainingSession : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TrainingCourseId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string? Venue { get; private set; }
    public TrainerType TrainerType { get; private set; } = TrainerType.Internal;
    public string? TrainerName { get; private set; }
    /// <summary>External delivery organisation — feeds the provider-payment hand-off (HC202, TD3).</summary>
    public string? ProviderName { get; private set; }
    public string? MeetingUrl { get; private set; }
    /// <summary>Null = unlimited.</summary>
    public int? MaxParticipants { get; private set; }
    public TrainingSessionStatus Status { get; private set; } = TrainingSessionStatus.Scheduled;
    public decimal? TrainerCost { get; private set; }
    public decimal? MaterialsCost { get; private set; }
    public decimal? VenueCost { get; private set; }
    public string? Notes { get; private set; }

    private TrainingSession() : base() { }

    public static TrainingSession Create(Guid trainingCourseId, DateTime startDate, DateTime endDate,
        string? venue, TrainerType trainerType, string? trainerName, string? providerName, string? meetingUrl,
        int? maxParticipants, decimal? trainerCost, decimal? materialsCost, decimal? venueCost, string? notes)
    {
        Guard(trainingCourseId, startDate, endDate, maxParticipants, trainerCost, materialsCost, venueCost);
        return new TrainingSession
        {
            TrainingCourseId = trainingCourseId,
            StartDate = startDate,
            EndDate = endDate,
            Venue = venue,
            TrainerType = trainerType,
            TrainerName = trainerName,
            ProviderName = providerName,
            MeetingUrl = meetingUrl,
            MaxParticipants = maxParticipants,
            TrainerCost = trainerCost,
            MaterialsCost = materialsCost,
            VenueCost = venueCost,
            Notes = notes
        };
    }

    /// <summary>Editable only while scheduled.</summary>
    public void Update(Guid trainingCourseId, DateTime startDate, DateTime endDate,
        string? venue, TrainerType trainerType, string? trainerName, string? providerName, string? meetingUrl,
        int? maxParticipants, decimal? trainerCost, decimal? materialsCost, decimal? venueCost, string? notes)
    {
        Guard(trainingCourseId, startDate, endDate, maxParticipants, trainerCost, materialsCost, venueCost);
        EnsureScheduled();
        TrainingCourseId = trainingCourseId;
        StartDate = startDate;
        EndDate = endDate;
        Venue = venue;
        TrainerType = trainerType;
        TrainerName = trainerName;
        ProviderName = providerName;
        MeetingUrl = meetingUrl;
        MaxParticipants = maxParticipants;
        TrainerCost = trainerCost;
        MaterialsCost = materialsCost;
        VenueCost = venueCost;
        Notes = notes;
        base.Update();
    }

    /// <summary>HC197 — reschedule a planned session.</summary>
    public void Reschedule(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date cannot precede the start date.", nameof(endDate));
        EnsureScheduled();
        StartDate = startDate;
        EndDate = endDate;
        base.Update();
    }

    public void MarkCompleted()
    {
        EnsureScheduled();
        Status = TrainingSessionStatus.Completed;
        base.Update();
    }

    public void Cancel()
    {
        EnsureScheduled();
        Status = TrainingSessionStatus.Cancelled;
        base.Update();
    }

    /// <summary>Cost lines the session contributes to budget utilization (HC190).</summary>
    public decimal TotalCost => (TrainerCost ?? 0) + (MaterialsCost ?? 0) + (VenueCost ?? 0);

    private void EnsureScheduled()
    {
        if (Status != TrainingSessionStatus.Scheduled)
            throw new InvalidOperationException($"Only a scheduled session can change (current: {Status}).");
    }

    private static void Guard(Guid trainingCourseId, DateTime startDate, DateTime endDate,
        int? maxParticipants, decimal? trainerCost, decimal? materialsCost, decimal? venueCost)
    {
        if (trainingCourseId == Guid.Empty)
            throw new ArgumentException("A course is required.", nameof(trainingCourseId));
        if (endDate < startDate)
            throw new ArgumentException("End date cannot precede the start date.", nameof(endDate));
        if (maxParticipants is < 1)
            throw new ArgumentException("Capacity must be at least 1.", nameof(maxParticipants));
        if (trainerCost is < 0 || materialsCost is < 0 || venueCost is < 0)
            throw new ArgumentException("Costs cannot be negative.", nameof(trainerCost));
    }
}

/// <summary>
/// One employee's participation in a session (HC198): attendance, completion, the post-training
/// assessment score, and the effectiveness feedback (HC199 — rating + comments by the participant).
/// Optionally linked to the approved <see cref="TrainingNeed"/> it fulfils (HC188 closes the loop).
/// </summary>
public class TrainingEnrollment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TrainingSessionId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid? TrainingNeedId { get; private set; }
    public TrainingEnrollmentStatus Status { get; private set; } = TrainingEnrollmentStatus.Enrolled;
    /// <summary>0–100 (HC198 attendance tracking).</summary>
    public decimal? AttendancePercent { get; private set; }
    /// <summary>Post-training assessment result, 0–100 (HC198).</summary>
    public decimal? AssessmentScore { get; private set; }
    public DateTime? CompletedOn { get; private set; }
    /// <summary>Participant feedback 1–5 (HC199).</summary>
    public int? FeedbackRating { get; private set; }
    public string? FeedbackComments { get; private set; }

    private TrainingEnrollment() : base() { }

    public static TrainingEnrollment Create(Guid trainingSessionId, Guid employeeId, Guid? trainingNeedId)
    {
        if (trainingSessionId == Guid.Empty)
            throw new ArgumentException("A session is required.", nameof(trainingSessionId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        return new TrainingEnrollment
        {
            TrainingSessionId = trainingSessionId,
            EmployeeId = employeeId,
            TrainingNeedId = trainingNeedId
        };
    }

    /// <summary>HC198 — manager/HR records attendance, completion state and the assessment result.</summary>
    public void RecordParticipation(TrainingEnrollmentStatus status, decimal? attendancePercent,
        decimal? assessmentScore, DateTime? completedOn)
    {
        if (Status == TrainingEnrollmentStatus.Withdrawn)
            throw new InvalidOperationException("A withdrawn enrollment cannot be updated.");
        if (attendancePercent is < 0 or > 100)
            throw new ArgumentException("Attendance must be between 0 and 100.", nameof(attendancePercent));
        if (assessmentScore is < 0 or > 100)
            throw new ArgumentException("Assessment score must be between 0 and 100.", nameof(assessmentScore));
        if (status == TrainingEnrollmentStatus.Completed && completedOn is null)
            throw new ArgumentException("A completion date is required to complete an enrollment.", nameof(completedOn));

        Status = status;
        AttendancePercent = attendancePercent;
        AssessmentScore = assessmentScore;
        CompletedOn = status == TrainingEnrollmentStatus.Completed ? completedOn : null;
        base.Update();
    }

    /// <summary>HC199 — the participant's own effectiveness feedback.</summary>
    public void SubmitFeedback(int rating, string? comments)
    {
        if (rating is < 1 or > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));
        FeedbackRating = rating;
        FeedbackComments = comments;
        base.Update();
    }

    public void Withdraw()
    {
        if (Status != TrainingEnrollmentStatus.Enrolled)
            throw new InvalidOperationException($"Only an active enrollment can be withdrawn (current: {Status}).");
        Status = TrainingEnrollmentStatus.Withdrawn;
        base.Update();
    }

    /// <summary>Re-joins after a withdrawal — the unique (session, employee) row is reused.</summary>
    public void Rejoin()
    {
        if (Status != TrainingEnrollmentStatus.Withdrawn)
            throw new InvalidOperationException($"Only a withdrawn enrollment can re-join (current: {Status}).");
        Status = TrainingEnrollmentStatus.Enrolled;
        AttendancePercent = null;
        AssessmentScore = null;
        CompletedOn = null;
        base.Update();
    }
}

/// <summary>
/// A training budget envelope (HC190) for one fiscal year, org-wide (null unit) or per organization
/// unit. Utilization = actual session costs + committed estimates of approved-but-unfulfilled needs.
/// </summary>
public class TrainingBudget : BaseEntity, IAggregateRoot, IAuditable
{
    public int FiscalYear { get; private set; }
    public Guid? OrganizationUnitId { get; private set; }
    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }

    private TrainingBudget() : base() { }

    public static TrainingBudget Create(int fiscalYear, Guid? organizationUnitId, decimal amount, string? notes)
    {
        Guard(fiscalYear, amount);
        return new TrainingBudget
        {
            FiscalYear = fiscalYear,
            OrganizationUnitId = organizationUnitId,
            Amount = amount,
            Notes = notes
        };
    }

    public void Update(int fiscalYear, Guid? organizationUnitId, decimal amount, string? notes)
    {
        Guard(fiscalYear, amount);
        FiscalYear = fiscalYear;
        OrganizationUnitId = organizationUnitId;
        Amount = amount;
        Notes = notes;
        base.Update();
    }

    private static void Guard(int fiscalYear, decimal amount)
    {
        if (fiscalYear is < 2000 or > 2100)
            throw new ArgumentException("Fiscal year is out of range.", nameof(fiscalYear));
        if (amount < 0)
            throw new ArgumentException("Budget amount cannot be negative.", nameof(amount));
    }
}
