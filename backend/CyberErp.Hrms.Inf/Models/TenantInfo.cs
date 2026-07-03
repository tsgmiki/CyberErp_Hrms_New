using Finbuckle.MultiTenant.Abstractions;

namespace CyberErp.Hrms.Inf.Models;

/// <summary>
/// Custom tenant info for Finbuckle.MultiTenant, including subscription metadata.
/// </summary>
public class AppTenantInfo : TenantInfo
{
    public string? ConnectionString { get; set; }
    public string? Theme { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsSubscriptionValid()
    {
        if (!IsActive)
            return false;

        if (SubscriptionEndDate.HasValue && SubscriptionEndDate.Value < DateTime.UtcNow)
            return false;

        return true;
    }
}
