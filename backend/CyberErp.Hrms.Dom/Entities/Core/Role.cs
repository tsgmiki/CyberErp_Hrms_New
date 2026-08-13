namespace CyberErp.Hrms.Dom.Entities.Core;

/*
 * Aligned with the SRMS platform schema (2026-08-13), where this table is the STANDARD ROLE TEMPLATE
 * (its primary key there is still named PK_StandardRoleTemplate). Hence `Code` is now required and
 * unique — it is how a template is identified — and Description/IsPlatformRole/IsActive were added.
 *
 * ⚠️ `TenantId` is KEPT, unlike SRMS. See the note on User; per-tenant instances live in TenantRole.
 */
public class Role : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    /// <summary>Stable identifier for the template. Required and unique — derived from the name when omitted.</summary>
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// <summary>A platform-level role, not one a tenant administers.</summary>
    public bool IsPlatformRole { get; private set; }
    public bool IsActive { get; private set; } = true;


    private Role() : base() { }

    public static Role Create(
        string name,
        string? code = null,
        string? description = null,
        bool isPlatformRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        return new Role
        {
            Name = name,
            Code = DeriveCode(code, name),
            Description = description?.Trim() ?? string.Empty,
            IsPlatformRole = isPlatformRole,
            IsActive = true
            // TenantId, CreatedBy will be set by Repository.AddAsync()
        };
    }

    public void Update(string? name = null, string? code = null, string? description = null)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be empty.", nameof(name));
            Name = name;
        }

        // Code is required, so a blank one falls back to the name rather than clearing the column.
        if (code != null)
            Code = DeriveCode(code, Name);

        if (description != null)
            Description = description.Trim();

        base.Update();
    }

    public void SetActive(bool isActive)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        base.Update();
    }

    /// <summary>
    /// The code a caller supplied, or one derived from the name when they left it blank —
    /// upper-cased and hyphenated, e.g. "HR Officer" becomes "HR-OFFICER".
    /// </summary>
    private static string DeriveCode(string? code, string name)
    {
        if (!string.IsNullOrWhiteSpace(code)) return code.Trim();
        var derived = string.Join("-", (name ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToUpperInvariant();
        return string.IsNullOrEmpty(derived) ? "ROLE" : derived;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Name = name;
        base.Update();
    }

}

