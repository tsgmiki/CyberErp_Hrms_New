using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// The tenant's customizable offer-letter template (one row per tenant). HR authors the letter
/// <see cref="Body"/> once with <c>{{Placeholder}}</c> merge tokens (candidate name, position,
/// salary, dates, company name…) and it is rendered — over the company letterhead (logo / name /
/// contact address from <see cref="CompanyProfile"/>) — into the offer-letter PDF that is attached
/// to the candidate's e-mail when the offer is approved (HC111). The signatory block closes the
/// letter. Editing never breaks in-flight offers; each offer captures its own merged text.
/// </summary>
public class OfferLetterTemplate : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>Letter body with {{Placeholder}} merge tokens (plain text, blank lines = paragraphs).</summary>
    public string Body { get; private set; } = string.Empty;
    /// <summary>Name of the person who signs the letter (e.g. HR Manager's name).</summary>
    public string? SignatoryName { get; private set; }
    /// <summary>Title of the signatory (e.g. "Head of Human Resources").</summary>
    public string? SignatoryTitle { get; private set; }

    private OfferLetterTemplate() : base() { }

    public static OfferLetterTemplate Create() => new() { Body = DefaultBody };

    public void Update(string body, string? signatoryName, string? signatoryTitle)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("The offer-letter body cannot be empty.", nameof(body));
        Body = body;
        SignatoryName = string.IsNullOrWhiteSpace(signatoryName) ? null : signatoryName.Trim();
        SignatoryTitle = string.IsNullOrWhiteSpace(signatoryTitle) ? null : signatoryTitle.Trim();
        base.Update();
    }

    /// <summary>The out-of-the-box letter used until HR customizes it — every token is populated.</summary>
    public const string DefaultBody =
        """
        Dear {{CandidateName}},

        Following your application and the completion of our selection process, we are pleased to
        offer you the position of {{Position}} at {{CompanyName}}.

        Terms of the offer:
          • Department / Unit: {{UnitName}}
          • Gross monthly salary: {{Salary}} ETB
          • Proposed start date: {{StartDate}}

        This offer remains valid until {{ExpiryDate}}. Please confirm your acceptance in writing
        before that date; the offer lapses automatically afterwards.

        We look forward to welcoming you to the team.
        """;
}
