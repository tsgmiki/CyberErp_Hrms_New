using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Company-wide branding for the tenant (one row per tenant). Currently holds the company logo
/// used by document templates via the <c>{{Logo}}</c> merge token. The logo is stored inline as
/// bytes so generated documents can embed it as a self-contained data URI (no external fetch).
/// </summary>
public class CompanyProfile : BaseEntity, IAggregateRoot, IAuditable
{
    public byte[]? LogoContent { get; private set; }
    public string? LogoContentType { get; private set; }

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
}
