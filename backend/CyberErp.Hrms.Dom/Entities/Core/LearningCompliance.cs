namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// How an assignment picks the people it applies to.
///
/// <para>Every option here is ONE join from the employee, which is what keeps the nightly
/// materialisation a set operation rather than a per-employee climb. Job grade is deliberately
/// absent: it is reached through PositionClass → SalaryScale → JobGrade, and grade is a PAY concept
/// in this product while <see cref="PositionClassAudience"/> is the job-role one — which is what
/// "everyone who does this job must hold this certificate" actually means (logic §12.88).</para>
/// </summary>
public enum AssignmentAudience
{
    /// <summary>Every active, non-terminated employee.</summary>
    Everyone = 0,
    /// <summary>Everyone positioned in one organizational unit, optionally including its descendants.</summary>
    OrganizationUnit = 1,
    /// <summary>Everyone holding a position of one class — "every Cold Chain Technician".</summary>
    PositionClassAudience = 2,
    /// <summary>Everyone at one branch/site.</summary>
    Branch = 3
}

/// <summary>
/// Where one person's obligation stands.
///
/// <para>⚠️ There is no Overdue member, and that is deliberate: overdue is
/// <c>Pending &amp;&amp; DueOn &lt; today</c>, derived wherever it is needed. A stored flag would need
/// a nightly sweep purely to keep itself honest, and would be wrong for the rest of the day whenever
/// that sweep failed.</para>
/// </summary>
public enum ObligationStatus
{
    /// <summary>Outstanding. Overdue once the due date passes.</summary>
    Pending = 0,
    /// <summary>Satisfied by a completed enrolment inside this cycle.</summary>
    Completed = 1,
    /// <summary>Excused by HR, with a reason. Counts as neither compliant nor outstanding.</summary>
    Waived = 2
}

/// <summary>
/// A standing rule: this course is REQUIRED of this population, by this deadline, repeating this
/// often.
///
/// <para>⚠️ This is the difference between training and compliance. Enrolment is one person choosing
/// one session; an assignment is an obligation the organisation places on a population and then
/// chases — and it keeps applying to people who join afterwards, which a list of enrolments cannot
/// do (logic §12.88).</para>
///
/// <para>The rule holds no people. Individual obligations are materialised from it into
/// <see cref="AssignmentObligation"/> by the nightly pass, so that who was in scope on a given day is
/// a recorded fact rather than something re-derived — and re-derived differently — later.</para>
/// </summary>
public class LearningAssignment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid TrainingCourseId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AssignmentAudience Audience { get; private set; }
    /// <summary>The unit, position class or branch. Null for <see cref="AssignmentAudience.Everyone"/>.</summary>
    public Guid? AudienceId { get; private set; }
    /// <summary>Org-unit audiences only: whether descendant units are in scope too.</summary>
    public bool IncludeSubUnits { get; private set; }
    /// <summary>Days from the cycle start to the deadline. Also the recertification window.</summary>
    public int DueWithinDays { get; private set; } = 30;
    /// <summary>
    /// A hard calendar deadline for the FIRST cycle, overriding <see cref="DueWithinDays"/> — a
    /// one-off campaign ("everyone by 31 March"). Cannot be combined with recurrence, because a
    /// repeating obligation has no single date to repeat to.
    /// </summary>
    public DateTime? FixedDueOn { get; private set; }
    /// <summary>Months between recertifications, measured from COMPLETION. Null means once only.</summary>
    public int? RecurrenceMonths { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private LearningAssignment() : base() { }

    public static LearningAssignment Create(Guid trainingCourseId, string name, AssignmentAudience audience,
        Guid? audienceId, bool includeSubUnits, int dueWithinDays, DateTime? fixedDueOn,
        int? recurrenceMonths, bool isActive, string? notes)
    {
        if (trainingCourseId == Guid.Empty)
            throw new ArgumentException("A course is required.", nameof(trainingCourseId));

        var a = new LearningAssignment { TrainingCourseId = trainingCourseId };
        a.Apply(name, audience, audienceId, includeSubUnits, dueWithinDays, fixedDueOn,
            recurrenceMonths, isActive, notes);
        return a;
    }

    public void Apply(string name, AssignmentAudience audience, Guid? audienceId, bool includeSubUnits,
        int dueWithinDays, DateTime? fixedDueOn, int? recurrenceMonths, bool isActive, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("An assignment needs a name.", nameof(name));
        if (dueWithinDays < 1)
            throw new ArgumentException("Allow at least a day to complete it.", nameof(dueWithinDays));
        if (recurrenceMonths is < 1)
            throw new ArgumentException("A recurrence must be at least one month.", nameof(recurrenceMonths));

        // A targeted audience with nothing to target would silently match nobody, and the screen
        // would look correctly configured.
        if (audience != AssignmentAudience.Everyone && (audienceId is null || audienceId == Guid.Empty))
            throw new ArgumentException($"Choose the {audience} this applies to.", nameof(audienceId));

        // Recurrence measures from completion, so there is no fixed date for cycle two to land on.
        if (fixedDueOn.HasValue && recurrenceMonths.HasValue)
            throw new ArgumentException(
                "A fixed deadline cannot repeat — use a recurrence window instead.", nameof(fixedDueOn));

        Name = name.Trim();
        Audience = audience;
        AudienceId = audience == AssignmentAudience.Everyone ? null : audienceId;
        IncludeSubUnits = audience == AssignmentAudience.OrganizationUnit && includeSubUnits;
        DueWithinDays = dueWithinDays;
        FixedDueOn = fixedDueOn?.Date;
        RecurrenceMonths = recurrenceMonths;
        IsActive = isActive;
        Notes = notes;
        base.Update();
    }

    /// <summary>The deadline for a cycle starting on <paramref name="cycleStart"/>.</summary>
    public DateTime DueDateFor(DateTime cycleStart, int cycleNumber) =>
        cycleNumber == 1 && FixedDueOn.HasValue
            ? FixedDueOn.Value
            : cycleStart.Date.AddDays(DueWithinDays);
}

/// <summary>
/// One person's copy of an assignment for one cycle — the row that is chased, reported on, and
/// eventually satisfied.
///
/// <para>⚠️ CYCLES ARE WHAT MAKE RECERTIFICATION MEAN ANYTHING. A completion only satisfies the cycle
/// it happened in: the satisfying enrolment must have completed on or after
/// <see cref="AssignedOn"/>. Without that rule, last year's certificate would silently satisfy this
/// year's obligation and nobody would ever be re-trained (logic §12.88).</para>
/// </summary>
public class AssignmentObligation : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid LearningAssignmentId { get; private set; }
    public Guid EmployeeId { get; private set; }
    /// <summary>1, 2, 3 … one per recertification round.</summary>
    public int CycleNumber { get; private set; }
    /// <summary>When this cycle started — the earliest completion that can satisfy it.</summary>
    public DateTime AssignedOn { get; private set; }
    public DateTime DueOn { get; private set; }
    public ObligationStatus Status { get; private set; } = ObligationStatus.Pending;
    public DateTime? CompletedOn { get; private set; }
    /// <summary>The enrolment that satisfied it — the evidence behind a compliant row.</summary>
    public Guid? TrainingEnrollmentId { get; private set; }
    /// <summary>Stops the nightly chase from mailing the same person every single night.</summary>
    public DateTime? LastReminderOn { get; private set; }
    /// <summary>Set once the manager has been told, so they are told once rather than nightly.</summary>
    public DateTime? EscalatedOn { get; private set; }
    public string? WaivedReason { get; private set; }

    public bool IsOverdueOn(DateTime today) => Status == ObligationStatus.Pending && DueOn.Date < today.Date;

    private AssignmentObligation() : base() { }

    public static AssignmentObligation Create(Guid learningAssignmentId, Guid employeeId, int cycleNumber,
        DateTime assignedOn, DateTime dueOn)
    {
        if (learningAssignmentId == Guid.Empty)
            throw new ArgumentException("An assignment is required.", nameof(learningAssignmentId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (cycleNumber < 1)
            throw new ArgumentException("Cycles are numbered from 1.", nameof(cycleNumber));

        return new AssignmentObligation
        {
            LearningAssignmentId = learningAssignmentId,
            EmployeeId = employeeId,
            CycleNumber = cycleNumber,
            AssignedOn = assignedOn.Date,
            DueOn = dueOn.Date
        };
    }

    /// <summary>
    /// Marks it satisfied by a specific enrolment.
    ///
    /// <para>Idempotent, because the nightly pass re-examines outstanding obligations every night and
    /// must not keep rewriting one it already closed.</para>
    /// </summary>
    public void Satisfy(Guid trainingEnrollmentId, DateTime completedOn)
    {
        if (Status == ObligationStatus.Completed) return;
        if (Status == ObligationStatus.Waived)
            throw new InvalidOperationException("A waived obligation cannot be completed.");

        Status = ObligationStatus.Completed;
        TrainingEnrollmentId = trainingEnrollmentId;
        CompletedOn = completedOn;
        base.Update();
    }

    /// <summary>
    /// Excuses the obligation, with a reason.
    ///
    /// <para>⚠️ A reason is REQUIRED. A waiver is the one way a compliance record can show someone as
    /// not needing training they were assigned, so an audit has to be able to read why — an
    /// unexplained waiver is indistinguishable from a mistake.</para>
    /// </summary>
    public void Waive(string reason)
    {
        if (Status == ObligationStatus.Completed)
            throw new InvalidOperationException("A completed obligation cannot be waived.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A waiver needs a reason.", nameof(reason));

        Status = ObligationStatus.Waived;
        WaivedReason = reason.Trim();
        base.Update();
    }

    public void MarkReminded(DateTime when)
    {
        LastReminderOn = when;
        base.Update();
    }

    public void MarkEscalated(DateTime when)
    {
        EscalatedOn = when;
        base.Update();
    }
}
