namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A portal alert row in <c>Core.Notification</c> — the table the Home portal OWNS and reads
/// (its bell / dashboard). HRMS is a WRITER of this cross-subsystem contract: it raises an alert
/// by inserting a row. This is deliberately NOT a <see cref="BaseEntity"/> — the shared table has
/// its own shape (plain <see cref="DateTime"/> timestamps, no RowVersion / UpdatedAt), so it is
/// mapped standalone and EXCLUDED from HRMS migrations (Home owns the schema). <see cref="UserId"/>
/// null = tenant-wide broadcast; <see cref="SourceEntityType"/>/<see cref="SourceEntityId"/> let the
/// raising subsystem mark its own alerts read once the underlying record is decided.
/// </summary>
public class CoreNotification
{
    public Guid Id { get; private set; }
    public string TenantId { get; set; } = string.Empty;
    /// <summary>Recipient user (Core.User.Id); null = broadcast to every user of the tenant.</summary>
    public Guid? UserId { get; private set; }
    /// <summary>Raising application's subsystem code (e.g. "HRMS") — shown as the bell's source chip.</summary>
    public string SourceSubsystem { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Body { get; private set; }
    /// <summary>Absolute or subsystem-relative link the notification navigates to when clicked.</summary>
    public string? LinkUrl { get; private set; }
    /// <summary>Info | Warning | Action — drives the bell item's tone.</summary>
    public string Severity { get; private set; } = "Info";
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }
    /// <summary>Back-reference to the record that raised this alert (e.g. "WorkflowInstance").</summary>
    public string? SourceEntityType { get; private set; }
    public Guid? SourceEntityId { get; private set; }

    private CoreNotification() { }

    public static CoreNotification Create(
        Guid? userId, string sourceSubsystem, string title,
        string? body = null, string? linkUrl = null, string severity = "Info",
        string? sourceEntityType = null, Guid? sourceEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(sourceSubsystem))
            throw new ArgumentException("The source subsystem is required.", nameof(sourceSubsystem));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A title is required.", nameof(title));
        if (severity is not ("Info" or "Warning" or "Action"))
            throw new ArgumentException("Severity must be Info, Warning or Action.", nameof(severity));

        return new CoreNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SourceSubsystem = sourceSubsystem.Trim(),
            // The shared columns cap at 300 (title) / 2000 (body) — clamp so an insert never throws.
            Title = title.Trim().Length > 300 ? title.Trim()[..300] : title.Trim(),
            Body = body is { Length: > 2000 } ? body[..2000] : body,
            LinkUrl = linkUrl,
            Severity = severity,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow,
            SourceEntityType = sourceEntityType,
            SourceEntityId = sourceEntityId
        };
    }

    public void MarkRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAtUtc = DateTime.UtcNow;
    }
}
