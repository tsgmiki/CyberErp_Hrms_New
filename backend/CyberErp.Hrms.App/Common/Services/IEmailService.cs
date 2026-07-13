namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>A file attached to an outbound e-mail (e.g. a generated offer-letter PDF).</summary>
    public sealed record EmailAttachment(string FileName, byte[] Content, string ContentType);

    /// <summary>
    /// Outbound e-mail. Implementations NEVER throw — a failed (or disabled) send returns false
    /// and logs; notification mail must never break the business operation that triggered it.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>Sends a plain-text message. Returns false when disabled, unconfigured or failed.</summary>
        Task<bool> SendAsync(string to, string subject, string body,
            IReadOnlyList<EmailAttachment>? attachments = null);
    }

    /// <summary>
    /// A letter to render as a PDF over the company letterhead: the logo + company identity in the
    /// header, the merged body, and a signatory block. Used for the offer-letter PDF (HC111).
    /// </summary>
    public sealed record OfferLetterDocument(
        byte[]? Logo,
        string? CompanyName,
        string? ContactAddress,
        string? ContactPhone,
        string? ContactEmail,
        string Title,
        string DateText,
        string Body,
        string? SignatoryName,
        string? SignatoryTitle);

    /// <summary>
    /// Renders a letter into a printable PDF document. A simple corporate-letter layout: company
    /// letterhead (logo / name / contact), title, body paragraphs, signatory block, footer.
    /// </summary>
    public interface IPdfService
    {
        byte[] RenderOfferLetter(OfferLetterDocument document);
    }
}
