using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

public enum InterviewFormat
{
    InPerson = 0,
    Video = 1,
    Phone = 2,
    Panel = 3,
    TechnicalTest = 4
}

public enum InterviewStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2,
    NoShow = 3
}

public enum PanelistAttendance
{
    Pending = 0,
    Confirmed = 1,
    Attended = 2,
    Missed = 3
}

/// <summary>
/// One interview round for a job application (HC101–HC108). An application may hold any number of
/// rounds — deliberately NO unique (application, round-type) gate, so second rounds and re-screens
/// are first-class (logic.md §7.1 rejection #5). Panelists and their per-criterion feedback hang
/// off the round; the consolidated report (HC109) is computed on read.
/// </summary>
public class Interview : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid ApplicationId { get; private set; }
    /// <summary>1-based round ordinal for display (multiple rounds allowed).</summary>
    public int Round { get; private set; }
    public DateTime ScheduledStart { get; private set; }
    public DateTime ScheduledEnd { get; private set; }
    public InterviewFormat Format { get; private set; }
    public InterviewStatus Status { get; private set; } = InterviewStatus.Scheduled;
    public string? Location { get; private set; }
    public string? MeetingLink { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<InterviewPanelist> _panelists = [];
    public IReadOnlyCollection<InterviewPanelist> Panelists => _panelists;

    private Interview() : base() { }

    public static Interview Create(
        Guid applicationId, int round, DateTime scheduledStart, DateTime scheduledEnd,
        InterviewFormat format, string? location, string? meetingLink, string? notes)
    {
        if (applicationId == Guid.Empty)
            throw new ArgumentException("An application is required.", nameof(applicationId));
        EnsureWindow(scheduledStart, scheduledEnd);

        return new Interview
        {
            ApplicationId = applicationId,
            Round = round < 1 ? 1 : round,
            ScheduledStart = scheduledStart,
            ScheduledEnd = scheduledEnd,
            Format = format,
            Location = location,
            MeetingLink = meetingLink,
            Notes = notes
        };
    }

    /// <summary>Reschedules / edits the logistics of a not-yet-held round.</summary>
    public void Reschedule(DateTime scheduledStart, DateTime scheduledEnd,
        InterviewFormat format, string? location, string? meetingLink, string? notes)
    {
        if (Status != InterviewStatus.Scheduled)
            throw new InvalidOperationException($"A {Status} interview can no longer be rescheduled.");
        EnsureWindow(scheduledStart, scheduledEnd);

        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
        Format = format;
        Location = location;
        MeetingLink = meetingLink;
        Notes = notes;
        base.Update();
    }

    public void Complete(string? notes)
    {
        if (Status != InterviewStatus.Scheduled)
            throw new InvalidOperationException($"A {Status} interview cannot be completed.");
        Status = InterviewStatus.Completed;
        if (!string.IsNullOrWhiteSpace(notes)) Notes = notes;
        base.Update();
    }

    public void Cancel(string? reason)
    {
        if (Status != InterviewStatus.Scheduled)
            throw new InvalidOperationException($"A {Status} interview cannot be cancelled.");
        Status = InterviewStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason)) Notes = reason;
        base.Update();
    }

    public void MarkNoShow()
    {
        if (Status != InterviewStatus.Scheduled)
            throw new InvalidOperationException($"A {Status} interview cannot be marked no-show.");
        Status = InterviewStatus.NoShow;
        base.Update();
    }

    /// <summary>Replaces the panel of a not-yet-held round (add/edit panelists wholesale).</summary>
    public void SetPanel(IEnumerable<InterviewPanelist> panelists)
    {
        if (Status != InterviewStatus.Scheduled)
            throw new InvalidOperationException($"The panel of a {Status} interview can no longer change.");
        _panelists.Clear();
        _panelists.AddRange(panelists);
        base.Update();
    }

    private static void EnsureWindow(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("The interview must end after it starts.", nameof(end));
    }
}

/// <summary>One panel member of an interview round (HC104) — lead flag + attendance tracking.</summary>
public class InterviewPanelist : BaseEntity
{
    public Guid InterviewId { get; private set; }
    /// <summary>The internal employee on the panel (SET NULL on employee deletion — name survives).</summary>
    public Guid? EmployeeId { get; private set; }
    /// <summary>Server-resolved name snapshot (survives employee deletion).</summary>
    public string PanelistName { get; private set; } = string.Empty;
    public bool IsLead { get; private set; }
    public PanelistAttendance Attendance { get; private set; } = PanelistAttendance.Pending;

    private readonly List<InterviewFeedback> _feedback = [];
    public IReadOnlyCollection<InterviewFeedback> Feedback => _feedback;

    private InterviewPanelist() : base() { }

    public static InterviewPanelist Create(Guid interviewId, Guid? employeeId, string panelistName, bool isLead)
    {
        if (string.IsNullOrWhiteSpace(panelistName))
            throw new ArgumentException("A panelist name is required.", nameof(panelistName));

        return new InterviewPanelist
        {
            InterviewId = interviewId,
            EmployeeId = employeeId,
            PanelistName = panelistName.Trim(),
            IsLead = isLead
        };
    }

    public void SetAttendance(PanelistAttendance attendance)
    {
        Attendance = attendance;
        base.Update();
    }

    /// <summary>
    /// Upserts this panelist's score for one criterion (HC106). CriterionId is kept loose (no FK)
    /// like ApplicationCriterionScore — criteria are replaced wholesale on requisition edits, so
    /// the row snapshots the criterion name for display resilience. A null criterion = the
    /// panelist's overall/general feedback entry.
    /// </summary>
    public void RecordFeedback(Guid? criterionId, string? criterionName, decimal score, string? comments)
    {
        if (score is < 0 or > 100)
            throw new ArgumentException("A feedback score must be between 0 and 100.", nameof(score));

        var existing = criterionId.HasValue
            ? _feedback.FirstOrDefault(f => f.CriterionId == criterionId)
            : _feedback.FirstOrDefault(f => f.CriterionId == null);
        if (existing is null)
            _feedback.Add(InterviewFeedback.Create(Id, criterionId, criterionName, score, comments));
        else
            existing.Revise(score, comments);
        base.Update();
    }
}

/// <summary>One panelist's scored assessment of one criterion (0–100, HC106/HC109).</summary>
public class InterviewFeedback : BaseEntity
{
    public Guid PanelistId { get; private set; }
    /// <summary>Loose reference to hrms_RequisitionScreeningCriterion (null = overall feedback).</summary>
    public Guid? CriterionId { get; private set; }
    /// <summary>Criterion-name snapshot at feedback time.</summary>
    public string? CriterionName { get; private set; }
    /// <summary>0–100 (same scale as the screening engine).</summary>
    public decimal Score { get; private set; }
    public string? Comments { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    private InterviewFeedback() : base() { }

    public static InterviewFeedback Create(Guid panelistId, Guid? criterionId, string? criterionName, decimal score, string? comments)
    {
        return new InterviewFeedback
        {
            PanelistId = panelistId,
            CriterionId = criterionId,
            CriterionName = criterionName,
            Score = score,
            Comments = comments,
            SubmittedAt = DateTime.UtcNow
        };
    }

    public void Revise(decimal score, string? comments)
    {
        if (score is < 0 or > 100)
            throw new ArgumentException("A feedback score must be between 0 and 100.", nameof(score));
        Score = score;
        Comments = comments;
        SubmittedAt = DateTime.UtcNow;
        base.Update();
    }
}
