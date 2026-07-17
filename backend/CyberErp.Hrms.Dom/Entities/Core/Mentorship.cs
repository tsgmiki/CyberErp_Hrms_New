using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>What a mentorship supports — a career path (HC168), a succession candidate (HC156), or general.</summary>
public enum MentorshipContext
{
    General = 0,
    CareerPath = 1,
    Succession = 2
}

public enum MentorshipStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

/// <summary>
/// A mentor↔mentee pairing (HC168, and reused for succession development HC156). Optionally scoped to a
/// context record (<see cref="RefId"/> — e.g. an <see cref="EmployeeCareerPath"/> or
/// <see cref="SuccessionCandidate"/>).
/// </summary>
public class Mentorship : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid MentorEmployeeId { get; private set; }
    public Guid MenteeEmployeeId { get; private set; }
    public MentorshipContext Context { get; private set; } = MentorshipContext.General;
    public Guid? RefId { get; private set; }
    public MentorshipStatus Status { get; private set; } = MentorshipStatus.Active;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Notes { get; private set; }

    private Employee? _mentor;
    public Employee? Mentor => _mentor;
    private Employee? _mentee;
    public Employee? Mentee => _mentee;

    private Mentorship() : base() { }

    public static Mentorship Create(Guid mentorEmployeeId, Guid menteeEmployeeId, MentorshipContext context, Guid? refId, MentorshipStatus status, DateTime? startDate, DateTime? endDate, string? notes)
    {
        if (mentorEmployeeId == Guid.Empty) throw new ArgumentException("A mentor is required.", nameof(mentorEmployeeId));
        if (menteeEmployeeId == Guid.Empty) throw new ArgumentException("A mentee is required.", nameof(menteeEmployeeId));
        if (mentorEmployeeId == menteeEmployeeId) throw new ArgumentException("A mentor and mentee must be different employees.");
        return new Mentorship
        {
            MentorEmployeeId = mentorEmployeeId,
            MenteeEmployeeId = menteeEmployeeId,
            Context = context,
            RefId = refId,
            Status = status,
            StartDate = startDate ?? DateTime.UtcNow,
            EndDate = endDate,
            Notes = notes,
        };
    }

    public void Update(Guid mentorEmployeeId, Guid menteeEmployeeId, MentorshipContext context, Guid? refId, MentorshipStatus status, DateTime? startDate, DateTime? endDate, string? notes)
    {
        if (mentorEmployeeId == menteeEmployeeId) throw new ArgumentException("A mentor and mentee must be different employees.");
        MentorEmployeeId = mentorEmployeeId;
        MenteeEmployeeId = menteeEmployeeId;
        Context = context;
        RefId = refId;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
        Notes = notes;
        base.Update();
    }
}
