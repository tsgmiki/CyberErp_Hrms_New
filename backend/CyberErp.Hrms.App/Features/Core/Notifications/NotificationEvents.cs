namespace CyberErp.Hrms.App.Features.Core.Notifications
{
    /// <summary>
    /// The catalogue of moments the code can notify on.
    ///
    /// <para>These keys are a CONTRACT: an administrator's template is stored against one, so a key
    /// that is renamed silently orphans the client's configuration. Add keys freely; never rename or
    /// delete one that has shipped.</para>
    ///
    /// <para>Each entry lists the merge tokens the raising code supplies. The template editor renders
    /// them as a palette, so an admin picks tokens rather than guessing — a token the event does not
    /// publish merges to empty.</para>
    /// </summary>
    public static class NotificationEvents
    {
        // ---- Leave ---------------------------------------------------------------------------
        public const string LeaveSubmitted = "Leave.Submitted";
        public const string LeaveApproved = "Leave.Approved";
        public const string LeaveRejected = "Leave.Rejected";

        // ---- Personnel movement (transfer / promotion / redeployment) -------------------------
        public const string MovementSubmitted = "Movement.Submitted";
        public const string MovementApproved = "Movement.Approved";
        public const string MovementExecuted = "Movement.Executed";
        public const string MovementCancelled = "Movement.Cancelled";

        // ---- Exit / termination ---------------------------------------------------------------
        public const string ExitSubmitted = "Exit.Submitted";
        public const string ExitApproved = "Exit.Approved";
        public const string ExitSettled = "Exit.Settled";
        public const string ExitCancelled = "Exit.Cancelled";

        // ---- Disciplinary ---------------------------------------------------------------------
        public const string DisciplinarySubmitted = "Disciplinary.Submitted";
        public const string DisciplinaryApproved = "Disciplinary.Approved";
        public const string DisciplinaryCancelled = "Disciplinary.Cancelled";

        // ---- Recruitment (CANDIDATE-facing: use the EventSubject recipient rule) ---------------
        public const string InterviewScheduled = "Interview.Scheduled";
        public const string InterviewRescheduled = "Interview.Rescheduled";
        public const string InterviewCancelled = "Interview.Cancelled";

        // ---- Trip ------------------------------------------------------------------------------
        public const string TripSettlementOverdue = "Trip.SettlementOverdue";

        /// <summary>
        /// The seed set. Applied idempotently by <c>SeedNotificationEvents</c> — an existing row is
        /// refreshed (name / tokens can improve) but never duplicated, and rows are never deleted,
        /// because a template may point at one.
        /// </summary>
        public static readonly IReadOnlyList<NotificationEventSeed> All =
        [
            new(LeaveSubmitted, "Leave request submitted", "Leave",
                "EmployeeName,EmployeeNumber,LeaveType,TotalDays,StartDate,EndDate,RequestDate,ApproverName,StepName",
                "Raised when an employee submits a leave request and it enters the approval workflow.",
                IsWorkflowEvent: true),

            new(LeaveApproved, "Leave request approved", "Leave",
                "EmployeeName,EmployeeNumber,LeaveType,TotalDays,StartDate,EndDate,RequestDate,ApproverName,StepName",
                "Raised when a leave request completes its approval workflow.",
                IsWorkflowEvent: true),

            new(LeaveRejected, "Leave request rejected", "Leave",
                "EmployeeName,EmployeeNumber,LeaveType,TotalDays,StartDate,EndDate,RequestDate,ApproverName,StepName,Reason",
                "Raised when a leave request is refused at any approval step.",
                IsWorkflowEvent: true),

            // ---- Personnel movement -----------------------------------------------------------
            new(MovementSubmitted, "Personnel movement submitted", "Movement",
                "EmployeeName,EmployeeNumber,MovementType,EffectiveDate,Reason",
                "Raised when a transfer / promotion / redeployment is submitted and routed for approval.",
                IsWorkflowEvent: true),

            new(MovementApproved, "Personnel movement approved", "Movement",
                "EmployeeName,EmployeeNumber,MovementType,EffectiveDate,Reason",
                "Raised when a personnel movement completes its approval workflow. It is applied on the effective date.",
                IsWorkflowEvent: true),

            new(MovementExecuted, "Personnel movement executed", "Movement",
                "EmployeeName,EmployeeNumber,MovementType,EffectiveDate,Reason",
                "Raised when a movement reaches its effective date and the organizational records are updated."),

            new(MovementCancelled, "Personnel movement cancelled", "Movement",
                "EmployeeName,EmployeeNumber,MovementType,EffectiveDate,Reason",
                "Raised when a movement is cancelled or rejected and the process terminates.",
                IsWorkflowEvent: true),

            // ---- Exit -------------------------------------------------------------------------
            new(ExitSubmitted, "Exit case submitted", "Exit",
                "EmployeeName,EmployeeNumber,TerminationType,LastWorkingDate,Reason",
                "Raised when an exit case is submitted and routed for approval.",
                IsWorkflowEvent: true),

            new(ExitApproved, "Exit approved - clearance opened", "Exit",
                "EmployeeName,EmployeeNumber,TerminationType,LastWorkingDate,Reason",
                "Raised when an exit case is approved and the departmental clearance checklist opens.",
                IsWorkflowEvent: true),

            new(ExitSettled, "Exit settled", "Exit",
                "EmployeeName,EmployeeNumber,TerminationType,LastWorkingDate,Reason",
                "Raised when an exit case is settled and the employment record becomes inactive."),

            new(ExitCancelled, "Exit cancelled", "Exit",
                "EmployeeName,EmployeeNumber,TerminationType,LastWorkingDate,Reason",
                "Raised when an exit case is cancelled or rejected and the process terminates.",
                IsWorkflowEvent: true),

            // ---- Disciplinary -----------------------------------------------------------------
            new(DisciplinarySubmitted, "Disciplinary case raised", "Disciplinary",
                "EmployeeName,EmployeeNumber,ViolationType,MeasureType,ViolationDate,EffectiveDate",
                "Raised when a disciplinary case is opened for an employee and routed for review.",
                IsWorkflowEvent: true),

            new(DisciplinaryApproved, "Disciplinary case confirmed", "Disciplinary",
                "EmployeeName,EmployeeNumber,ViolationType,MeasureType,ViolationDate,EffectiveDate",
                "Raised when a disciplinary case is reviewed and confirmed.",
                IsWorkflowEvent: true),

            new(DisciplinaryCancelled, "Disciplinary case cancelled", "Disciplinary",
                "EmployeeName,EmployeeNumber,ViolationType,MeasureType,ViolationDate,EffectiveDate",
                "Raised when a disciplinary case is cancelled or voided.",
                IsWorkflowEvent: true),

            // ---- Recruitment ------------------------------------------------------------------
            // ⚠️ These are addressed to a CANDIDATE, who is not an employee and cannot be resolved
            // from org data. Templates for them need an EventSubject recipient rule, or the
            // candidate never receives their own interview invitation.
            new(InterviewScheduled, "Interview scheduled", "Recruitment",
                "CandidateName,VacancyTitle,InterviewDate,StartTime,EndTime,Mode,Location",
                "Raised when an interview is booked with a candidate. Addressed to the candidate (EventSubject)."),

            new(InterviewRescheduled, "Interview rescheduled", "Recruitment",
                "CandidateName,VacancyTitle,InterviewDate,StartTime,EndTime,Mode,Location,PreviousDate,PreviousTime",
                "Raised when a booked interview moves to a new slot. Addressed to the candidate (EventSubject)."),

            new(InterviewCancelled, "Interview cancelled", "Recruitment",
                "CandidateName,VacancyTitle,InterviewDate,StartTime,EndTime,Mode,Location",
                "Raised when a booked interview is cancelled. Addressed to the candidate (EventSubject)."),

            // ---- Trip -------------------------------------------------------------------------
            new(TripSettlementOverdue, "Travel advance overdue for settlement", "Trip",
                "EmployeeName,EmployeeNumber,TripNumber,AdvanceAmount,Currency,DueDate",
                "Raised by the daily reminder job for every travel advance past its settlement deadline."),
        ];
    }

    /// <summary>One catalogue row, as declared in code and seeded into the database.</summary>
    public sealed record NotificationEventSeed(
        string EventKey,
        string Name,
        string Category,
        string Tokens,
        string? Description = null,
        bool IsWorkflowEvent = false);
}
