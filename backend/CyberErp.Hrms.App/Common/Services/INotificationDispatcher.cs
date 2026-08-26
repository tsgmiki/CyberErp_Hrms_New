namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// Everything an event hands the dispatcher. The caller supplies FACTS; the administrator's
    /// template decides what is said and who hears it.
    /// </summary>
    /// <param name="EventKey">Catalogue key, from <c>NotificationEvents</c>.</param>
    /// <param name="Tokens">
    /// Merge values for the template, keyed WITHOUT braces ("EmployeeName" → "Abebe Kebede").
    /// A token the template references but the event does not supply merges to empty, which is the
    /// safe failure: a slightly thin sentence rather than a literal "{{EmployeeName}}" in a message
    /// to staff.
    /// </param>
    /// <param name="RequesterEmployeeId">
    /// The employee the record is about. Resolves the Requester and RequesterManager rules.
    /// </param>
    /// <param name="WorkflowDefinitionId">Set when the event fired inside a workflow — selects step-scoped templates.</param>
    /// <param name="StepOrder">The step the event fired on, for step-scoped templates and CurrentApprover.</param>
    /// <param name="EntityType">Governed entity type, for the portal alert's deep link.</param>
    /// <param name="EntityId">Governed entity id, for the portal alert's deep link.</param>
    /// <param name="SubjectAddresses">
    /// Addresses the event is inherently addressed to, for the EventSubject rule — a candidate, an
    /// external party, anyone who is not an employee and so cannot be resolved from the org data.
    /// </param>
    public sealed record NotificationContext(
        string EventKey,
        IReadOnlyDictionary<string, string?> Tokens,
        Guid? RequesterEmployeeId = null,
        Guid? WorkflowDefinitionId = null,
        int? StepOrder = null,
        string? EntityType = null,
        Guid? EntityId = null,
        IReadOnlyList<string>? SubjectAddresses = null);

    /// <summary>
    /// Sends the administrator-defined notification for an event: picks the template, merges the
    /// tokens, resolves the recipient rules to addresses, and delivers.
    ///
    /// <para>NEVER throws, and never blocks the business operation that raised the event — same
    /// contract as <see cref="IEmailService"/>. A missing template is not an error: it means the
    /// client has not asked to be told about that event, and nothing is sent.</para>
    /// </summary>
    public interface INotificationDispatcher
    {
        /// <summary>Dispatches every active template registered for the event. Returns how many messages were sent.</summary>
        Task<int> DispatchAsync(NotificationContext context);
    }
}
