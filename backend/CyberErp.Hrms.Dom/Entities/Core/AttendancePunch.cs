using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PunchDirection
{
    /// <summary>The device did not say. Direction is inferred when the day is processed.</summary>
    Unknown = 0,
    In = 1,
    Out = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PunchSource
{
    /// <summary>Pulled from, or pushed by, a registered device.</summary>
    Device = 0,
    /// <summary>Keyed in by an administrator.</summary>
    Manual = 1,
    /// <summary>Loaded from a file a machine exported.</summary>
    Import = 2
}

/// <summary>
/// Attendance (HC042): one raw clock event, exactly as it was reported.
/// </summary>
/// <remarks>
/// <para>⚠️ THIS ROW IS THE EVIDENCE AND IS NEVER EDITED. Every correction belongs on the derived
/// <c>AttendanceDay</c>, which carries its own override audit. If an administrator could rewrite a
/// punch, the record of what the machine actually saw would be gone, and an attendance dispute
/// would have nothing to appeal to.</para>
///
/// <para>⚠️ <see cref="EmployeeId"/> IS NULLABLE. A punch whose device id maps to nobody is kept
/// UNRESOLVED, not dropped: somebody really did stand at that door, and discarding the event
/// because the enrolment is missing destroys the only proof they were there. It is surfaced for an
/// administrator to attach to an employee.</para>
///
/// <para>⚠️ Every ingest path — pull, push, file, manual — funnels through this one table, so
/// deduplication and resolution have a single implementation rather than one per transport.</para>
/// </remarks>
public class AttendancePunch : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>Resolved employee, or null while the punch is unattributed.</summary>
    public Guid? EmployeeId { get; private set; }

    /// <summary>Null for a manual entry — there was no machine.</summary>
    public Guid? AttendanceDeviceId { get; private set; }

    /// <summary>The identifier the device reported, kept verbatim even once resolved.</summary>
    public string? DeviceUserId { get; private set; }

    public DateTime PunchedAt { get; private set; }
    public PunchDirection Direction { get; private set; } = PunchDirection.Unknown;
    public PunchSource Source { get; private set; } = PunchSource.Device;

    /// <summary>
    /// The device's own record id, when it has one.
    /// ⚠️ This is what makes re-reading a terminal safe: the same log pulled twice yields the same
    /// external ids, so the second pull inserts nothing instead of doubling everybody's day.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>The untouched payload, for working out what a machine actually sent.</summary>
    public string? RawPayload { get; private set; }

    /// <summary>Why the punch could not be attributed, when it could not.</summary>
    public string? UnresolvedReason { get; private set; }

    public Employee? Employee { get; private set; }
    public AttendanceDevice? AttendanceDevice { get; private set; }

    private AttendancePunch() : base() { }

    public static AttendancePunch Create(
        DateTime punchedAt,
        Guid? employeeId = null,
        Guid? deviceId = null,
        string? deviceUserId = null,
        PunchDirection direction = PunchDirection.Unknown,
        PunchSource source = PunchSource.Device,
        string? externalId = null,
        string? rawPayload = null,
        string? unresolvedReason = null)
    {
        if (punchedAt == default)
            throw new ArgumentException("A punch needs a timestamp.", nameof(punchedAt));
        if (employeeId is null && string.IsNullOrWhiteSpace(deviceUserId))
            throw new ArgumentException(
                "A punch must name somebody — an employee, or the device id to resolve later.", nameof(employeeId));
        if (source == PunchSource.Manual && employeeId is null)
            throw new ArgumentException("A manual punch must name the employee.", nameof(employeeId));

        return new AttendancePunch
        {
            EmployeeId = employeeId,
            AttendanceDeviceId = deviceId,
            DeviceUserId = string.IsNullOrWhiteSpace(deviceUserId) ? null : deviceUserId.Trim(),
            PunchedAt = punchedAt,
            Direction = direction,
            Source = source,
            ExternalId = string.IsNullOrWhiteSpace(externalId) ? null : externalId.Trim(),
            RawPayload = rawPayload,
            UnresolvedReason = employeeId is null
                ? (unresolvedReason ?? "No enrolment matches this device id.")
                : null
        };
    }

    /// <summary>
    /// Attaches an unresolved punch to an employee.
    /// ⚠️ The ONLY mutation this entity allows, and it only ever fills a blank — it cannot move a
    /// punch from one employee to another, which would be a rewrite of the evidence.
    /// </summary>
    public void ResolveTo(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (EmployeeId is not null)
            throw new InvalidOperationException("This punch is already attributed and cannot be reassigned.");
        EmployeeId = employeeId;
        UnresolvedReason = null;
        base.Update();
    }
}
