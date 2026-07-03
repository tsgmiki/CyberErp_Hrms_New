using CyberErp.Hrms.Dom.Entities;
using CyberErp.Hrms.Dom.Events;

namespace CyberErp.Hrms.Dom.Entities.Core;

public class SubscriptionPlan : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string BillingCycle { get; private set; } = string.Empty; // Monthly, Yearly, etc.
    public int MaxUsers { get; private set; }
    public int MaxStorageGB { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int TrialDays { get; private set; }
    public string? Features { get; private set; } // JSON string of features

    private SubscriptionPlan() : base() { }

    public static SubscriptionPlan Create(
        string name,
        string description,
        decimal price,
        string billingCycle,
        int maxUsers,
        int maxStorageGB,
        int trialDays = 0,
        string? features = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(billingCycle))
            throw new ArgumentException("Billing cycle cannot be empty.", nameof(billingCycle));

        return new SubscriptionPlan
        {
            Name = name,
            Description = description,
            Price = price,
            BillingCycle = billingCycle,
            MaxUsers = maxUsers,
            MaxStorageGB = maxStorageGB,
            IsActive = true,
            TrialDays = trialDays,
            Features = features
        };
    }

    public void Update(
        string? name = null,
        string? description = null,
        decimal? price = null,
        string? billingCycle = null,
        int? maxUsers = null,
        int? maxStorageGB = null,
        bool? isActive = null,
        int? trialDays = null,
        string? features = null)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Plan name cannot be empty.", nameof(name));
            Name = name;
        }

        if (description != null)
            Description = description;

        if (price.HasValue)
            Price = price.Value;

        if (billingCycle != null)
        {
            if (string.IsNullOrWhiteSpace(billingCycle))
                throw new ArgumentException("Billing cycle cannot be empty.", nameof(billingCycle));
            BillingCycle = billingCycle;
        }

        if (maxUsers.HasValue)
            MaxUsers = maxUsers.Value;

        if (maxStorageGB.HasValue)
            MaxStorageGB = maxStorageGB.Value;

        if (isActive.HasValue)
            IsActive = isActive.Value;

        if (trialDays.HasValue)
            TrialDays = trialDays.Value;

        if (features != null)
            Features = features;

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
}
