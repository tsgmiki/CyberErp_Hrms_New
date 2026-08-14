using CyberErp.Hrms.Dom.Entities;
using CyberErp.Hrms.Dom.Events;

namespace CyberErp.Hrms.Dom.Entities.Core;

public class Tenant : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Identifier { get; private set; } = string.Empty;
    public string? ConnectionString { get; private set; }
    public string? Theme { get; private set; }
    public string? Address { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? SubscriptionStartDate { get; private set; }
    public DateTime? SubscriptionEndDate { get; private set; }

    // ---- SRMS platform alignment (2026-08-14, logic.md §12.13) -------------
    // ⚠️ Do not confuse OrganizationId with BaseEntity.TenantId. The latter is the Finbuckle
    // DISCRIMINATOR STRING carried by all 202 tables; this is a real foreign key to the legal
    // entity that owns the tenant. One organization may hold several tenants.
    public Guid OrganizationId { get; private set; }

    /// <summary>Optional classification (trial, internal, customer…). No table backs it yet.</summary>
    public Guid? TenantTypeId { get; private set; }

    // Per-tenant overrides of the organization's localisation. Null means "inherit".
    public string? CurrencyOverride { get; private set; }
    public string? LocaleOverride { get; private set; }
    public string? TimezoneOverride { get; private set; }

    private Tenant() : base() { }

    /// <summary>Attaches the tenant to its owning organization.</summary>
    public void SetOrganization(Guid organizationId)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization is required.", nameof(organizationId));
        OrganizationId = organizationId;
        base.Update();
    }

    public void SetTenantType(Guid? tenantTypeId)
    {
        TenantTypeId = tenantTypeId;
        base.Update();
    }

    /// <summary>Sets (or clears, when null) the localisation overrides for this tenant.</summary>
    public void SetLocalisationOverrides(string? currency, string? locale, string? timezone)
    {
        CurrencyOverride = string.IsNullOrWhiteSpace(currency) ? null : currency.Trim();
        LocaleOverride = string.IsNullOrWhiteSpace(locale) ? null : locale.Trim();
        TimezoneOverride = string.IsNullOrWhiteSpace(timezone) ? null : timezone.Trim();
        base.Update();
    }

    public static Tenant Create(
        string name,
        string identifier,
        string? connectionString = null,
        string? theme = null,
        string? address = null,
        string? phoneNumber = null,
        string? email = null,
        DateTime? subscriptionStartDate = null,
        DateTime? subscriptionEndDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Tenant identifier cannot be empty.", nameof(identifier));

        return new Tenant
        {
            Name = name,
            Identifier = identifier,
            ConnectionString = connectionString,
            Theme = theme,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email,
            IsActive = true,
            SubscriptionStartDate = subscriptionStartDate,
            SubscriptionEndDate = subscriptionEndDate
        };
    }

    public void Update(
        string? name = null,
        string? identifier = null,
        string? connectionString = null,
        string? theme = null,
        string? address = null,
        string? phoneNumber = null,
        string? email = null,
        bool? isActive = null,
        DateTime? subscriptionStartDate = null,
        DateTime? subscriptionEndDate = null)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tenant name cannot be empty.", nameof(name));
            Name = name;
        }

        if (identifier != null)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Tenant identifier cannot be empty.", nameof(identifier));
            Identifier = identifier;
        }

        if (connectionString != null)
            ConnectionString = connectionString;

        if (theme != null)
            Theme = theme;

        if (address != null)
            Address = address;

        if (phoneNumber != null)
            PhoneNumber = phoneNumber;

        if (email != null)
            Email = email;

        if (isActive.HasValue)
            IsActive = isActive.Value;

        if (subscriptionStartDate.HasValue)
            SubscriptionStartDate = subscriptionStartDate;

        if (subscriptionEndDate.HasValue)
            SubscriptionEndDate = subscriptionEndDate;

        base.Update();
    }

    public void Activate()
    {
        IsActive = true;
        base.Update();
    }

    public void Deactivate()
    {
        IsActive = false;
        base.Update();
    }

    public void UpdateSubscription(DateTime? startDate, DateTime? endDate)
    {
        SubscriptionStartDate = startDate;
        SubscriptionEndDate = endDate;
        base.Update();
    }
}
