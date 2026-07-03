using CyberErp.Hrms.Dom.Entities;
using CyberErp.Hrms.Dom.Events;
using NodaTime;

namespace CyberErp.Hrms.Dom.Entities.Core;

public class TenantSubscription : BaseEntity, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public string Status { get; private set; } = string.Empty; // Active, Trial, Expired, Cancelled
    public decimal AmountPaid { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? TransactionId { get; private set; }
    public DateTime? LastPaymentDate { get; private set; }
    public DateTime? NextBillingDate { get; private set; }
    public bool AutoRenew { get; private set; } = true;

    // Navigation properties
    private Tenant? _tenant;
    public Tenant? Tenant => _tenant;

    private SubscriptionPlan? _subscriptionPlan;
    public SubscriptionPlan? SubscriptionPlan => _subscriptionPlan;

    private TenantSubscription() : base() { }

    public static TenantSubscription Create(
        Guid tenantId,
        Guid subscriptionPlanId,
        DateTime startDate,
        DateTime? endDate = null,
        decimal amountPaid = 0,
        string? paymentMethod = null,
        string? transactionId = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        if (subscriptionPlanId == Guid.Empty)
            throw new ArgumentException("Subscription plan ID cannot be empty.", nameof(subscriptionPlanId));

        return new TenantSubscription
        {
            TenantId = tenantId,
            SubscriptionPlanId = subscriptionPlanId,
            StartDate = startDate,
            EndDate = endDate,
            Status = endDate.HasValue && endDate.Value > startDate ? "Active" : "Trial",
            AmountPaid = amountPaid,
            PaymentMethod = paymentMethod,
            TransactionId = transactionId,
            AutoRenew = true
        };
    }

    public void Update(
        Guid subscriptionPlanId,
        DateTime startDate,
        DateTime? endDate,
        DateTime? trialEndDate,
        string status,
        decimal amountPaid,
        string? paymentMethod,
        string? transactionId,
        DateTime? lastPaymentDate,
        DateTime? nextBillingDate,
        bool autoRenew)
    {
        if (subscriptionPlanId == Guid.Empty)
            throw new ArgumentException("Subscription plan ID cannot be empty.", nameof(subscriptionPlanId));

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be empty.", nameof(status));

        SubscriptionPlanId = subscriptionPlanId;
        StartDate = startDate;
        EndDate = endDate;
        TrialEndDate = trialEndDate;
        Status = status;
        AmountPaid = amountPaid;
        PaymentMethod = paymentMethod;
        TransactionId = transactionId;
        LastPaymentDate = lastPaymentDate;
        NextBillingDate = nextBillingDate;
        AutoRenew = autoRenew;
        base.Update();
    }

    public void UpdatePlan(Guid subscriptionPlanId, decimal newPrice)
    {
        SubscriptionPlanId = subscriptionPlanId;
        AmountPaid = newPrice;
        base.Update();
    }

    public void SetTrialPeriod(int trialDays)
    {
        TrialEndDate = StartDate.AddDays(trialDays);
        Status = "Trial";
        base.Update();
    }

    public void Activate()
    {
        Status = "Active";
        base.Update();
    }

    public void Cancel()
    {
        Status = "Cancelled";
        AutoRenew = false;
        base.Update();
    }

    public void Expire()
    {
        Status = "Expired";
        base.Update();
    }

    public void RecordPayment(decimal amount, string? paymentMethod, string? transactionId)
    {
        AmountPaid = amount;
        PaymentMethod = paymentMethod;
        TransactionId = transactionId;
        LastPaymentDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
        
        // Calculate next billing date based on billing cycle
        if (EndDate.HasValue)
        {
            NextBillingDate = EndDate.Value;
        }

        Status = "Active";
        base.Update();
    }

    public void SetAutoRenew(bool autoRenew)
    {
        AutoRenew = autoRenew;
        base.Update();
    }

    public void ExtendSubscription(DateTime newEndDate)
    {
        EndDate = newEndDate;
        if (Status == "Expired")
        {
            Status = "Active";
        }
        base.Update();
    }

    public bool IsActive()
    {
        return Status == "Active" || Status == "Trial";
    }

    public bool IsTrial()
    {
        return Status == "Trial" && TrialEndDate.HasValue && TrialEndDate.Value > SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
    }

    public bool IsExpired()
    {
        return EndDate.HasValue && EndDate.Value < SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
    }
}
