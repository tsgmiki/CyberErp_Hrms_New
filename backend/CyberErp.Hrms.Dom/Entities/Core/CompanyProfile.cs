using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Company-wide branding for the tenant (one row per tenant). Holds the company logo used by
/// document templates via the <c>{{Logo}}</c> merge token, plus the letterhead identity (name,
/// contact address/phone/e-mail) that generated correspondence — e.g. the offer letter PDF —
/// prints in its header. The logo is stored inline as bytes so generated documents can embed it
/// as a self-contained data URI (no external fetch).
/// </summary>
public class CompanyProfile : BaseEntity, IAggregateRoot, IAuditable
{
    public byte[]? LogoContent { get; private set; }
    public string? LogoContentType { get; private set; }

    /// <summary>Legal/display company name shown on letterhead.</summary>
    public string? CompanyName { get; private set; }
    /// <summary>Postal/physical contact address (multi-line allowed).</summary>
    public string? ContactAddress { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? ContactEmail { get; private set; }

    private CompanyProfile() : base() { }

    public static CompanyProfile Create() => new();

    public void SetLogo(byte[] content, string contentType)
    {
        if (content is null || content.Length == 0)
            throw new ArgumentException("Logo content cannot be empty.", nameof(content));
        LogoContent = content;
        LogoContentType = contentType;
        base.Update();
    }

    public void ClearLogo()
    {
        LogoContent = null;
        LogoContentType = null;
        base.Update();
    }

    /// <summary>Updates the letterhead identity fields (all optional).</summary>
    public void SetIdentity(string? companyName, string? contactAddress, string? contactPhone, string? contactEmail)
    {
        CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim();
        ContactAddress = string.IsNullOrWhiteSpace(contactAddress) ? null : contactAddress.Trim();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
        base.Update();
    }
}
