using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// The legal entity a deployment belongs to — ported from the SRMS platform schema.
///
/// <para>Sits ABOVE the tenant: one organization may hold several tenants, which is why it carries no
/// <c>TenantId</c> filter of its own. It is the richer successor to <see cref="CompanyProfile"/>,
/// adding the identity a real ERP needs — registration and tax numbers, currency, timezone, locale,
/// fiscal-year start, industry and regulatory identifiers — alongside the contact and letterhead
/// fields the profile already had.</para>
///
/// <para>⚠️ <see cref="CompanyProfile"/> still exists and still feeds the offer letter and report
/// letterhead. The two OVERLAP deliberately for now: this change is additive by design, so nothing
/// that renders today can break. Consolidating the profile into this entity is the follow-up, and it
/// is a code change (letters, report headers, exports), not a data one.</para>
/// </summary>
public class Organization : BaseEntity, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string LegalName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    // ---- Contact -----------------------------------------------------------
    public string? Address { get; private set; }
    public string? PostalAddress { get; private set; }
    public string? PostalCode { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public string? City { get; private set; }
    public string? Region { get; private set; }
    public string? Country { get; private set; }

    // ---- Primary contact person -------------------------------------------
    public string? PrimaryContactName { get; private set; }
    public string? PrimaryContactTitle { get; private set; }
    public string? PrimaryContactEmail { get; private set; }
    public string? PrimaryContactPhone { get; private set; }

    // ---- Legal / regulatory identity --------------------------------------
    public string? RegistrationNumber { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? TINNumber { get; private set; }
    public string? RegulatoryIdentifiers { get; private set; }
    public string? Industry { get; private set; }
    public string? OrganizationType { get; private set; }

    // ---- Localisation ------------------------------------------------------
    /// <summary>ISO 4217, e.g. "ETB". Fixed width in the source schema (nchar).</summary>
    public string Currency { get; private set; } = string.Empty;
    public string Timezone { get; private set; } = string.Empty;
    public string Locale { get; private set; } = string.Empty;
    public string DefaultLanguage { get; private set; } = string.Empty;
    public string DateFormat { get; private set; } = string.Empty;
    /// <summary>1–12. The fiscal year's opening month; 0/null means "not configured".</summary>
    public int FiscalYearStartMonth { get; private set; }

    // ---- Letterhead --------------------------------------------------------
    public byte[]? Logo { get; private set; }
    public string? LogoContentType { get; private set; }

    public string? DataRetentionPolicy { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Organization() : base() { }

    public static Organization Create(string code, string legalName, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Organization code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException("Legal name is required.", nameof(legalName));
        return new Organization
        {
            Code = code.Trim(),
            LegalName = legalName.Trim(),
            DisplayName = displayName?.Trim() ?? string.Empty,
        };
    }

    public void UpdateIdentity(string legalName, string? displayName, string? registrationNumber,
        string? taxNumber, string? tinNumber, string? industry, string? organizationType)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException("Legal name is required.", nameof(legalName));
        LegalName = legalName.Trim();
        DisplayName = displayName?.Trim() ?? string.Empty;
        RegistrationNumber = registrationNumber?.Trim();
        TaxNumber = taxNumber?.Trim();
        TINNumber = tinNumber?.Trim();
        Industry = industry?.Trim();
        OrganizationType = organizationType?.Trim();
        base.Update();
    }

    public void UpdateContact(string? address, string? postalAddress, string? postalCode,
        string? phoneNumber, string? email, string? website, string? city, string? region, string? country)
    {
        Address = address?.Trim();
        PostalAddress = postalAddress?.Trim();
        PostalCode = postalCode?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Email = email?.Trim();
        Website = website?.Trim();
        City = city?.Trim();
        Region = region?.Trim();
        Country = country?.Trim();
        base.Update();
    }

    public void UpdateLocalisation(string? currency, string? timezone, string? locale,
        string? defaultLanguage, string? dateFormat, int fiscalYearStartMonth)
    {
        if (fiscalYearStartMonth is < 0 or > 12)
            throw new ArgumentException("Fiscal year start month must be between 1 and 12.", nameof(fiscalYearStartMonth));
        Currency = currency?.Trim() ?? string.Empty;
        Timezone = timezone?.Trim() ?? string.Empty;
        Locale = locale?.Trim() ?? string.Empty;
        DefaultLanguage = defaultLanguage?.Trim() ?? string.Empty;
        DateFormat = dateFormat?.Trim() ?? string.Empty;
        FiscalYearStartMonth = fiscalYearStartMonth;
        base.Update();
    }

    public void SetLogo(byte[]? logo, string? contentType)
    {
        Logo = logo;
        LogoContentType = logo is null ? null : contentType;
        base.Update();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        base.Update();
    }

    /// <summary>
    /// The letterhead subset that CompanyProfile used to own — the four fields generated
    /// correspondence prints in its header. Kept as one method because that is exactly what the
    /// company-profile screen posts; the wider identity/contact setters above are for the full form.
    ///
    /// <para><paramref name="companyName"/> maps to <see cref="LegalName"/>, which is REQUIRED here
    /// though it was optional on the profile — so a blank one leaves the existing name alone rather
    /// than putting the row into a state <see cref="Create"/> would have rejected.</para>
    /// </summary>
    public void SetLetterhead(string? companyName, string? address, string? phone, string? email)
    {
        if (!string.IsNullOrWhiteSpace(companyName))
            LegalName = companyName.Trim();

        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        base.Update();
    }
}
