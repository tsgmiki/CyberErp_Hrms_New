namespace CyberErp.Hrms.Dom.Entities.Core;

/*
 * User-defined notifications (2026-08-19).
 *
 * Before this, every automated e-mail was compiled in three separate ways: the SUBJECT and BODY were
 * built with string interpolation at 29 call sites, the RECIPIENT was whoever that notifier decided
 * to resolve, and WHICH events notified at all was whichever notifiers happened to exist. Changing a
 * word, or telling the client's HR team about an approval, meant a code change and a deploy.
 *
 * Three tables replace that:
 *
 *   NotificationEvent      the CATALOGUE of moments the system can notify on. Seeded and read-only —
 *                          an admin cannot invent an event the code never raises. Each row publishes
 *                          the merge tokens available to templates written against it.
 *   NotificationTemplate   the admin's subject + body for one event, optionally narrowed to one
 *                          workflow step, so "leave approved by HR" can read differently from
 *                          "leave approved by the line manager".
 *   NotificationRecipient  WHO receives it — a list of rules, not an address. "the requester",
 *                          "whoever is approving this step", "everyone in the Finance role",
 *                          "all employees", or a literal address.
 */

/// <summary>Delivery channel a template drives. Portal alerts and e-mail share one recipient model.</summary>
public enum NotificationChannel
{
    Email,
    Portal,
    Both,
}

/// <summary>How a resolved recipient is addressed on the message.</summary>
public enum RecipientDelivery
{
    To,
    Cc,
    Bcc,
}

/// <summary>
/// WHAT a recipient rule resolves to. The rule stores intent, not addresses, so a template keeps
/// working when people change role, manager or team — which an address list cannot do.
/// </summary>
public enum RecipientKind
{
    /// <summary>The employee the record is about — who raised the leave, the appraisee, the claimant.</summary>
    Requester,

    /// <summary>Whoever is assigned to decide the workflow step this event fired on.</summary>
    CurrentApprover,

    /// <summary>The requester's line manager, from the org structure.</summary>
    RequesterManager,

    /// <summary>Everyone holding a named role (TargetId = TenantRole).</summary>
    Role,

    /// <summary>Everyone in a named organization unit (TargetId = OrganizationUnit).</summary>
    OrganizationUnit,

    /// <summary>One named employee (TargetId = Employee).</summary>
    Employee,

    /// <summary>
    /// Every active employee with an address. ⚠️ The client asked for this explicitly; it is also the
    /// rule most likely to be regretted, so the dispatcher caps and logs it.
    /// </summary>
    AllEmployees,

    /// <summary>A literal address, for distribution lists and external parties.</summary>
    Address,
}

/// <summary>
/// A moment the system can notify on. SEEDED, never user-created: the key must match what the code
/// actually raises, so an admin picks from this list rather than typing an event that never fires.
/// </summary>
public class NotificationEvent : BaseEntity
{
    /// <summary>Stable key the code dispatches on, e.g. "Leave.Submitted". Never renamed.</summary>
    public string EventKey { get; private set; } = string.Empty;

    /// <summary>What an administrator sees in the picker.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Grouping for the picker — "Leave", "Recruitment", "Performance".</summary>
    public string Category { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    /// <summary>
    /// Comma-separated merge tokens this event supplies, e.g.
    /// "EmployeeName,LeaveType,TotalDays,StartDate,EndDate". The editor renders these as a palette,
    /// so an admin never has to guess what is available — a token the event does not publish would
    /// merge to blank.
    /// </summary>
    public string Tokens { get; private set; } = string.Empty;

    /// <summary>True when the event is raised from inside a workflow, so step scoping applies.</summary>
    public bool IsWorkflowEvent { get; private set; }

    private NotificationEvent() : base() { }

    public static NotificationEvent Create(string eventKey, string name, string category,
        string tokens, string? description = null, bool isWorkflowEvent = false)
    {
        if (string.IsNullOrWhiteSpace(eventKey))
            throw new ArgumentException("Event key is required.", nameof(eventKey));
        return new NotificationEvent
        {
            EventKey = eventKey.Trim(),
            Name = string.IsNullOrWhiteSpace(name) ? eventKey.Trim() : name.Trim(),
            Category = category?.Trim() ?? string.Empty,
            Tokens = tokens?.Trim() ?? string.Empty,
            Description = description,
            IsWorkflowEvent = isWorkflowEvent,
        };
    }

    public void Update(string name, string category, string tokens, string? description)
    {
        Name = name?.Trim() ?? Name;
        Category = category?.Trim() ?? Category;
        Tokens = tokens?.Trim() ?? Tokens;
        Description = description;
    }
}

/// <summary>
/// The subject and body an administrator wrote for one event.
///
/// <para>Optionally narrowed to a single workflow step: with <see cref="WorkflowDefinitionId"/> and
/// <see cref="StepOrder"/> set, the template applies only when the event fires on that step. The
/// dispatcher prefers the most specific match, so a general "leave approved" template can coexist
/// with a different one for the HR step.</para>
/// </summary>
public class NotificationTemplate : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid NotificationEventId { get; private set; }

    /// <summary>Denormalised so the dispatcher can select a template without joining the catalogue.</summary>
    public string EventKey { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    /// <summary>Subject line, with {{Token}} merge fields.</summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>HTML body, with {{Token}} merge fields. Same merge syntax as DocumentTemplate.</summary>
    public string Body { get; private set; } = string.Empty;

    public NotificationChannel Channel { get; private set; } = NotificationChannel.Email;

    /// <summary>Narrows the template to one workflow definition; null = every workflow.</summary>
    public Guid? WorkflowDefinitionId { get; private set; }

    /// <summary>Narrows the template to one step of that workflow; null = every step.</summary>
    public int? StepOrder { get; private set; }

    public bool IsActive { get; private set; } = true;

    private NotificationTemplate() : base() { }

    public static NotificationTemplate Create(Guid notificationEventId, string eventKey, string name,
        string subject, string body, NotificationChannel channel = NotificationChannel.Email,
        Guid? workflowDefinitionId = null, int? stepOrder = null, bool isActive = true)
    {
        Guard(eventKey, subject, body);
        return new NotificationTemplate
        {
            NotificationEventId = notificationEventId,
            EventKey = eventKey.Trim(),
            Name = string.IsNullOrWhiteSpace(name) ? eventKey.Trim() : name.Trim(),
            Subject = subject.Trim(),
            Body = body,
            Channel = channel,
            WorkflowDefinitionId = workflowDefinitionId,
            StepOrder = stepOrder,
            IsActive = isActive,
        };
    }

    public void Update(string name, string subject, string body, NotificationChannel channel,
        Guid? workflowDefinitionId, int? stepOrder, bool isActive)
    {
        Guard(EventKey, subject, body);
        Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
        Subject = subject.Trim();
        Body = body;
        Channel = channel;
        WorkflowDefinitionId = workflowDefinitionId;
        StepOrder = stepOrder;
        IsActive = isActive;
    }

    private static void Guard(string eventKey, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(eventKey))
            throw new ArgumentException("Event is required.", nameof(eventKey));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.", nameof(body));
    }
}

/// <summary>
/// One recipient RULE on a template. A template carries several; the dispatcher resolves each to
/// addresses at send time and de-duplicates across them.
/// </summary>
public class NotificationRecipient : BaseEntity
{
    public Guid NotificationTemplateId { get; private set; }

    public RecipientKind Kind { get; private set; }

    /// <summary>
    /// The role / org unit / employee this rule points at. Null for the kinds that need no target
    /// (Requester, CurrentApprover, RequesterManager, AllEmployees).
    /// </summary>
    public Guid? TargetId { get; private set; }

    /// <summary>The literal address, for <see cref="RecipientKind.Address"/>.</summary>
    public string? Address { get; private set; }

    public RecipientDelivery Delivery { get; private set; } = RecipientDelivery.To;

    public bool IsActive { get; private set; } = true;

    private NotificationRecipient() : base() { }

    public static NotificationRecipient Create(Guid templateId, RecipientKind kind,
        Guid? targetId = null, string? address = null,
        RecipientDelivery delivery = RecipientDelivery.To, bool isActive = true)
    {
        if (kind is RecipientKind.Role or RecipientKind.OrganizationUnit or RecipientKind.Employee
            && (targetId is null || targetId == Guid.Empty))
            throw new ArgumentException($"{kind} needs a target.", nameof(targetId));
        if (kind == RecipientKind.Address && string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("An address rule needs an address.", nameof(address));

        return new NotificationRecipient
        {
            NotificationTemplateId = templateId,
            Kind = kind,
            TargetId = targetId,
            Address = address?.Trim(),
            Delivery = delivery,
            IsActive = isActive,
        };
    }
}
