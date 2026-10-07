using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Attendance (HC042): a registered source of attendance events — a biometric terminal, a device
/// that posts to us, or a file export.
/// </summary>
/// <remarks>
/// <para>⚠️ <see cref="Protocol"/> IS A STRING, deliberately, not an enum. It is the key an
/// <c>IAttendanceDeviceAdapter</c> claims through <c>Supports(protocol)</c> — the same extension
/// shape the workflow engine uses for its 28 entity types. Making it an enum would mean a new
/// machine could not be supported without editing the domain and running a migration, which is
/// exactly the future-proofing this module was asked for.</para>
///
/// <para>⚠️ <see cref="LastSyncError"/> is kept BESIDE <see cref="LastSyncAt"/> rather than replacing
/// it. A terminal that silently stopped answering three weeks ago looks identical to a healthy one
/// if a failure only clears the timestamp, and nobody notices until payroll is short a month of
/// punches.</para>
/// </remarks>
public class AttendanceDevice : BaseEntity, IAggregateRoot, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    /// <summary>Adapter key, e.g. "Csv", "Push", later "ZKTeco". Matched by <c>Supports(protocol)</c>.</summary>
    public string Protocol { get; private set; } = string.Empty;

    /// <summary>Where it lives, for the humans who have to go and look at it.</summary>
    public string? Location { get; private set; }

    /// <summary>Host/IP or base URL for a pull adapter. Null for file imports and push-only devices.</summary>
    public string? Endpoint { get; private set; }
    public int? Port { get; private set; }
    public string? SerialNumber { get; private set; }

    /// <summary>
    /// Shared secret a PUSHING device presents on the ingest endpoint.
    /// ⚠️ Write-only from the API's point of view — handlers never project it into a DTO.
    /// </summary>
    public string? IngestKey { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>Last time a sync COMPLETED, successfully or not.</summary>
    public DateTime? LastSyncAt { get; private set; }
    /// <summary>Why the last sync failed, or null when it succeeded.</summary>
    public string? LastSyncError { get; private set; }
    /// <summary>Punches accepted by the last sync.</summary>
    public int LastSyncPunchCount { get; private set; }

    private AttendanceDevice() : base() { }

    public static AttendanceDevice Create(
        string code, string name, string protocol, string? location = null,
        string? endpoint = null, int? port = null, string? serialNumber = null,
        string? ingestKey = null, bool isActive = true)
    {
        Guard(code, name, protocol, port);
        return new AttendanceDevice
        {
            Code = code.Trim(),
            Name = name.Trim(),
            Protocol = protocol.Trim(),
            Location = location,
            Endpoint = endpoint,
            Port = port,
            SerialNumber = serialNumber,
            IngestKey = ingestKey,
            IsActive = isActive
        };
    }

    public void Update(
        string code, string name, string protocol, string? location,
        string? endpoint, int? port, string? serialNumber, bool isActive)
    {
        Guard(code, name, protocol, port);
        Code = code.Trim();
        Name = name.Trim();
        Protocol = protocol.Trim();
        Location = location;
        Endpoint = endpoint;
        Port = port;
        SerialNumber = serialNumber;
        IsActive = isActive;
        base.Update();
    }

    /// <summary>⚠️ Separate from <see cref="Update"/> so a routine edit cannot blank the key by omission.</summary>
    public void SetIngestKey(string? ingestKey)
    {
        IngestKey = string.IsNullOrWhiteSpace(ingestKey) ? null : ingestKey.Trim();
        base.Update();
    }

    public void RecordSync(int punchCount, string? error = null)
    {
        LastSyncAt = DateTime.UtcNow;
        LastSyncPunchCount = punchCount;
        LastSyncError = string.IsNullOrWhiteSpace(error) ? null : error;
        base.Update();
    }

    private static void Guard(string code, string name, string protocol, int? port)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Device code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(protocol))
            throw new ArgumentException("A device must declare a protocol so an adapter can claim it.", nameof(protocol));
        if (port is < 1 or > 65535)
            throw new ArgumentException("Port must be between 1 and 65535.", nameof(port));
    }
}
