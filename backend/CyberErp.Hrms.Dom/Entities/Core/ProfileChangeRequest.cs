using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// How an approved profile change is applied. Identity scalars are written straight to the
/// employee/person record on approval; structural changes (pay, placement, education, experience)
/// are acknowledged and fulfilled by HR through the module that owns their side-effects.
/// </summary>
public enum ProfileChangeKind
{
    /// <summary>Simple identity/contact scalar — auto-applied to Person/Employee on approval.</summary>
    IdentityField = 0,
    /// <summary>Has side-effects (salary, position, education, experience) — HR fulfils it via the proper module.</summary>
    Structural = 1
}

/// <summary>Lifecycle of a profile change request.</summary>
public enum ProfileChangeStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// An employee's self-service request to change a RESTRICTED profile field they cannot edit
/// directly (name, birthdate, education, experience, salary, position, …). HR reviews it on the
/// dashboard and approves/rejects. On approval an <see cref="ProfileChangeKind.IdentityField"/>
/// request is applied to the record automatically; a <see cref="ProfileChangeKind.Structural"/>
/// one is acknowledged for HR to fulfil through the owning module. Ticket-style (not an approval
/// chain).
/// </summary>
public class ProfileChangeRequest : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    /// <summary>Stable key of the requested field (see the App-layer allowlist), e.g. "FirstName", "Salary".</summary>
    public string FieldKey { get; private set; } = string.Empty;
    /// <summary>Human label snapshot for display, e.g. "First Name".</summary>
    public string FieldLabel { get; private set; } = string.Empty;
    public ProfileChangeKind Kind { get; private set; }
    /// <summary>The value at submission time (snapshot), for HR context.</summary>
    public string? CurrentValue { get; private set; }
    /// <summary>The requested new value (identity fields) or a description of the desired change (structural).</summary>
    public string RequestedValue { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public ProfileChangeStatus Status { get; private set; } = ProfileChangeStatus.Pending;
    /// <summary>HR's decision note.</summary>
    public string? Resolution { get; private set; }
    /// <summary>True when an approved identity change was written to the record automatically.</summary>
    public bool AutoApplied { get; private set; }
    public DateTime SubmittedOn { get; private set; }
    public DateTime? ResolvedOn { get; private set; }
    public string? ResolvedBy { get; private set; }

    private ProfileChangeRequest() : base() { }

    public static ProfileChangeRequest Create(Guid employeeId, string fieldKey, string fieldLabel,
        ProfileChangeKind kind, string? currentValue, string requestedValue, string? reason, DateTime submittedOn)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(fieldKey)) throw new ArgumentException("A field is required.", nameof(fieldKey));
        if (string.IsNullOrWhiteSpace(requestedValue))
            throw new ArgumentException("The requested value cannot be empty.", nameof(requestedValue));
        return new ProfileChangeRequest
        {
            EmployeeId = employeeId,
            FieldKey = fieldKey.Trim(),
            FieldLabel = fieldLabel.Trim(),
            Kind = kind,
            CurrentValue = currentValue,
            RequestedValue = requestedValue.Trim(),
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            Status = ProfileChangeStatus.Pending,
            SubmittedOn = submittedOn
        };
    }

    public void Approve(string? resolution, bool autoApplied, string? by, DateTime resolvedOn)
    {
        if (Status != ProfileChangeStatus.Pending)
            throw new InvalidOperationException($"A {Status} request can no longer be decided.");
        Status = ProfileChangeStatus.Approved;
        Resolution = string.IsNullOrWhiteSpace(resolution) ? null : resolution.Trim();
        AutoApplied = autoApplied;
        ResolvedBy = by;
        ResolvedOn = resolvedOn;
        base.Update();
    }

    public void Reject(string resolution, string? by, DateTime resolvedOn)
    {
        if (Status != ProfileChangeStatus.Pending)
            throw new InvalidOperationException($"A {Status} request can no longer be decided.");
        if (string.IsNullOrWhiteSpace(resolution))
            throw new ArgumentException("A reason is required to reject a request.", nameof(resolution));
        Status = ProfileChangeStatus.Rejected;
        Resolution = resolution.Trim();
        ResolvedBy = by;
        ResolvedOn = resolvedOn;
        base.Update();
    }
}
