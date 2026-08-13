using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a subscription or add-on. Stored as text, matching the SRMS platform schema.</summary>
public static class SubscriptionStatuses
{
    public const string Trial = "Trial";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
}

/// <summary>
/// What an <see cref="Organization"/> is paying for — ported from the SRMS platform schema.
///
/// <para>Distinct from the existing <c>TenantSubscription</c>, which attaches a plan to a single
/// TENANT. Billing is an organization-level concern: one organization can run several tenants on one
/// agreement, and this is the row that agreement lives on.</para>
/// </summary>
public class OrganizationSubscription : BaseEntity, IAggregateRoot
{
    public Guid OrganizationId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    public string Status { get; private set; } = SubscriptionStatuses.Trial;
    public string Currency { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? NextBillingDate { get; private set; }
    public bool AutoRenew { get; private set; }

    private OrganizationSubscription() : base() { }

    public static OrganizationSubscription Create(Guid organizationId, Guid subscriptionPlanId,
        string status, string currency, DateTime startDate, DateTime? endDate = null,
        DateTime? nextBillingDate = null, bool autoRenew = true)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization is required.", nameof(organizationId));
        if (subscriptionPlanId == Guid.Empty)
            throw new ArgumentException("Subscription plan is required.", nameof(subscriptionPlanId));
        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));
        return new OrganizationSubscription
        {
            OrganizationId = organizationId,
            SubscriptionPlanId = subscriptionPlanId,
            Status = string.IsNullOrWhiteSpace(status) ? SubscriptionStatuses.Trial : status.Trim(),
            Currency = currency?.Trim() ?? string.Empty,
            StartDate = startDate.Date,
            EndDate = endDate?.Date,
            NextBillingDate = nextBillingDate?.Date,
            AutoRenew = autoRenew,
        };
    }

    public void SetStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required.", nameof(status));
        Status = status.Trim();
        base.Update();
    }
}

/// <summary>
/// Which modules a subscription plan includes — ported from the SRMS platform schema.
///
/// <para>The plan-to-module map is what makes a plan mean something: without it a "Standard" plan is
/// a price with no contents. Paired with <see cref="TenantSubscriptionAddOn"/>, which grants a single
/// module beyond whatever the plan covers.</para>
/// </summary>
public class SubscriptionPlanModule : BaseEntity
{
    public Guid SubscriptionPlanId { get; private set; }
    public Guid ModuleId { get; private set; }

    private SubscriptionPlanModule() : base() { }

    public static SubscriptionPlanModule Create(Guid subscriptionPlanId, Guid moduleId)
    {
        if (subscriptionPlanId == Guid.Empty)
            throw new ArgumentException("Subscription plan is required.", nameof(subscriptionPlanId));
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module is required.", nameof(moduleId));
        return new SubscriptionPlanModule { SubscriptionPlanId = subscriptionPlanId, ModuleId = moduleId };
    }
}

/// <summary>
/// A single module bought for one tenant OUTSIDE its plan — ported from the SRMS platform schema.
///
/// <para><c>TenantId</c> here is a real foreign key to <c>Core.Tenant</c>, deliberately separate from
/// <see cref="BaseEntity.TenantId"/> (the string discriminator Finbuckle filters on). The add-on is a
/// platform-level billing record ABOUT a tenant, not a row owned BY one — a distinction that matters
/// because platform staff must be able to read every tenant's add-ons.</para>
/// </summary>
public class TenantSubscriptionAddOn : BaseEntity
{
    public Guid SubscribedTenantId { get; private set; }
    public Guid ModuleId { get; private set; }
    public string Status { get; private set; } = SubscriptionStatuses.Active;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    private TenantSubscriptionAddOn() : base() { }

    public static TenantSubscriptionAddOn Create(Guid subscribedTenantId, Guid moduleId, string status,
        DateTime startDate, decimal amount, string currency, DateTime? endDate = null)
    {
        if (subscribedTenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(subscribedTenantId));
        if (moduleId == Guid.Empty)
            throw new ArgumentException("Module is required.", nameof(moduleId));
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        return new TenantSubscriptionAddOn
        {
            SubscribedTenantId = subscribedTenantId,
            ModuleId = moduleId,
            Status = string.IsNullOrWhiteSpace(status) ? SubscriptionStatuses.Active : status.Trim(),
            StartDate = startDate.Date,
            EndDate = endDate?.Date,
            Amount = amount,
            Currency = currency?.Trim() ?? string.Empty,
        };
    }
}
