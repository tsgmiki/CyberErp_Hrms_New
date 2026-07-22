using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Kind of HR document a template produces. Purely a categorization for the admin — the body
/// is free-form HTML, so any layout is possible regardless of the type chosen.
/// </summary>
public enum DocumentTemplateType
{
    EmploymentLetter = 0,
    ExperienceLetter = 1,
    IdCard = 2,
    Other = 3,
    ClearanceCertificate = 4,
    AnnualLeaveRequest = 5,
    /// <summary>Formal transfer notice (HC174): new role, location, start date and reporting line.</summary>
    TransferNotice = 6,
    /// <summary>Digital certificate for a completed training (HC200).</summary>
    TrainingCertificate = 7,
    /// <summary>Exit letters (HC211): resignation acceptance / termination notice.</summary>
    TerminationNotice = 8,
    /// <summary>Final settlement letter (HC218) with the worksheet lines.</summary>
    SettlementLetter = 9
}

/// <summary>
/// Admin-configured, reusable document template (HC022 correspondence). The <see cref="Body"/>
/// holds HTML with <c>{{Placeholder}}</c> merge tokens that are resolved against an employee's
/// master data at generation time, so HR configures a template once and prints it for any employee.
/// Templates are organization-wide configuration (not branch-scoped); generation is still limited
/// to employees the caller can see.
/// </summary>
public class DocumentTemplate : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public DocumentTemplateType DocumentType { get; private set; }
    /// <summary>Optional HTML letterhead rendered above the body (supports {{Logo}} and merge tokens).</summary>
    public string? HeaderHtml { get; private set; }
    /// <summary>HTML body with {{Placeholder}} merge tokens.</summary>
    public string Body { get; private set; } = string.Empty;
    /// <summary>Optional HTML footer rendered below the body (supports merge tokens).</summary>
    public string? FooterHtml { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private DocumentTemplate() : base() { }

    public static DocumentTemplate Create(
        string name,
        DocumentTemplateType documentType,
        string body,
        string? headerHtml = null,
        string? footerHtml = null,
        string? description = null,
        bool isActive = true)
    {
        Guard(name, body);
        return new DocumentTemplate
        {
            Name = name,
            DocumentType = documentType,
            HeaderHtml = headerHtml,
            Body = body,
            FooterHtml = footerHtml,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(
        string name,
        DocumentTemplateType documentType,
        string body,
        string? headerHtml,
        string? footerHtml,
        string? description,
        bool isActive)
    {
        Guard(name, body);
        Name = name;
        DocumentType = documentType;
        HeaderHtml = headerHtml;
        Body = body;
        FooterHtml = footerHtml;
        Description = description;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name, string body)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Template body cannot be empty.", nameof(body));
    }
}
