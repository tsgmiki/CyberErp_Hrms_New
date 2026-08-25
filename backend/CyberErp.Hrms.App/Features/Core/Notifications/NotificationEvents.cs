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
