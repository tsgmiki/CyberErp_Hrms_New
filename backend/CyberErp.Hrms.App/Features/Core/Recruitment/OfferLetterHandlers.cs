using System.Globalization;
using System.Text.RegularExpressions;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- DTOs -----------------------------------------------------------------

    public class OfferLetterTemplateDto
    {
        public string Body { get; set; } = string.Empty;
        public string? SignatoryName { get; set; }
        public string? SignatoryTitle { get; set; }
    }

    public class SaveOfferLetterTemplateDto
    {
        public string Body { get; set; } = string.Empty;
        public string? SignatoryName { get; set; }
        public string? SignatoryTitle { get; set; }
    }

    public class SaveOfferLetterTemplateDtoValidator : AbstractValidator<SaveOfferLetterTemplateDto>
    {
        public SaveOfferLetterTemplateDtoValidator()
        {
            RuleFor(x => x.Body).NotEmpty().MaximumLength(8000);
            RuleFor(x => x.SignatoryName).MaximumLength(200);
            RuleFor(x => x.SignatoryTitle).MaximumLength(200);
        }
    }

    public class CompanyProfileDto
    {
        public string? CompanyName { get; set; }
        public string? ContactAddress { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public bool HasLogo { get; set; }
    }

    public class SaveCompanyProfileDto
    {
        public string? CompanyName { get; set; }
        public string? ContactAddress { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
    }

    public class SaveCompanyProfileDtoValidator : AbstractValidator<SaveCompanyProfileDto>
    {
        public SaveCompanyProfileDtoValidator()
        {
            RuleFor(x => x.CompanyName).MaximumLength(200);
            RuleFor(x => x.ContactAddress).MaximumLength(500);
            RuleFor(x => x.ContactPhone).MaximumLength(50);
            RuleFor(x => x.ContactEmail).MaximumLength(200).EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        }
    }

    public class OfferMergeFieldDto
    {
        public string Token { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>The merged letter text plus the fully-composed letterhead PDF model for one offer.</summary>
    public record ComposedOfferLetter(string MergedBody, OfferLetterDocument Document);

    // ---- Interfaces -------------------------------------------------------------

    public interface IGetOfferLetterTemplate { Task<OfferLetterTemplateDto> GetAsync(); }
    public interface ISaveOfferLetterTemplate { Task SaveAsync(SaveOfferLetterTemplateDto dto); }
    public interface IGetCompanyProfile { Task<CompanyProfileDto> GetAsync(); }
    public interface ISaveCompanyProfile { Task SaveAsync(SaveCompanyProfileDto dto); }
    public interface IGetOfferMergeFields { List<OfferMergeFieldDto> Get(); }
    /// <summary>Renders the (possibly unsaved) template with sample data to a PDF for the editor preview.</summary>
    public interface IPreviewOfferLetter { Task<byte[]> PreviewAsync(SaveOfferLetterTemplateDto dto); }

    /// <summary>
    /// Composes the offer letter for one offer: resolves the merge tokens from the offer, its
    /// candidate, requisition and the company profile, merges them into the template body (or an
    /// explicit body override), and returns both the merged text and the letterhead PDF model.
    /// </summary>
    public interface IOfferLetterComposer
    {
        Task<ComposedOfferLetter> ComposeAsync(Guid offerId, string? bodyOverride = null);
    }

    // ---- Template + company-profile handlers ------------------------------------

    public class GetOfferLetterTemplate(IRepository<OfferLetterTemplate> repository) : IGetOfferLetterTemplate
    {
        public async Task<OfferLetterTemplateDto> GetAsync()
        {
            var template = await repository.GetAll().FirstOrDefaultAsync();
            return new OfferLetterTemplateDto
            {
                Body = template?.Body ?? OfferLetterTemplate.DefaultBody,
                SignatoryName = template?.SignatoryName,
                SignatoryTitle = template?.SignatoryTitle
            };
        }
    }

    public class SaveOfferLetterTemplate(
        IRepository<OfferLetterTemplate> repository,
        IValidator<SaveOfferLetterTemplateDto> validator) : ISaveOfferLetterTemplate
    {
        public async Task SaveAsync(SaveOfferLetterTemplateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var template = await repository.GetAll().FirstOrDefaultAsync();
            if (template is null)
            {
                template = OfferLetterTemplate.Create();
                template.Update(dto.Body, dto.SignatoryName, dto.SignatoryTitle);
                await repository.AddAsync(template);
            }
            else
            {
                template.Update(dto.Body, dto.SignatoryName, dto.SignatoryTitle);
                repository.UpdateAsync(template);
            }
            await repository.SaveChangesAsync();
        }
    }

    public class GetCompanyProfile(IRepository<CompanyProfile> repository) : IGetCompanyProfile
    {
        public async Task<CompanyProfileDto> GetAsync()
        {
            var profile = await repository.GetAll()
                .Select(p => new CompanyProfileDto
                {
                    CompanyName = p.CompanyName,
                    ContactAddress = p.ContactAddress,
                    ContactPhone = p.ContactPhone,
                    ContactEmail = p.ContactEmail,
                    HasLogo = p.LogoContent != null
                })
                .FirstOrDefaultAsync();
            return profile ?? new CompanyProfileDto();
        }
    }

    public class SaveCompanyProfile(
        IRepository<CompanyProfile> repository,
        IValidator<SaveCompanyProfileDto> validator) : ISaveCompanyProfile
    {
        public async Task SaveAsync(SaveCompanyProfileDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var profile = await repository.GetAll().FirstOrDefaultAsync();
            if (profile is null)
            {
                profile = CompanyProfile.Create();
                profile.SetIdentity(dto.CompanyName, dto.ContactAddress, dto.ContactPhone, dto.ContactEmail);
                await repository.AddAsync(profile);
            }
            else
            {
                profile.SetIdentity(dto.CompanyName, dto.ContactAddress, dto.ContactPhone, dto.ContactEmail);
                repository.UpdateAsync(profile);
            }
            await repository.SaveChangesAsync();
        }
    }

    public class GetOfferMergeFields : IGetOfferMergeFields
    {
        public List<OfferMergeFieldDto> Get() =>
        [
            new() { Token = "{{CandidateName}}", Label = "Candidate full name" },
            new() { Token = "{{Position}}", Label = "Offered position / vacancy title" },
            new() { Token = "{{Salary}}", Label = "Offered monthly salary (ETB)" },
            new() { Token = "{{StartDate}}", Label = "Proposed start date" },
            new() { Token = "{{ExpiryDate}}", Label = "Offer valid-until date" },
            new() { Token = "{{OfferNumber}}", Label = "Offer number" },
            new() { Token = "{{EmploymentType}}", Label = "Employment type" },
            new() { Token = "{{UnitName}}", Label = "Department / organization unit" },
            new() { Token = "{{CompanyName}}", Label = "Company name" },
            new() { Token = "{{Today}}", Label = "Today's date" },
        ];
    }

    /// <summary>
    /// Renders the editor's current (unsaved) template body over the real company letterhead with
    /// representative SAMPLE token values — so HR sees exactly how a generated offer letter looks
    /// before saving. Uses no offer/candidate data.
    /// </summary>
    public partial class PreviewOfferLetter(
        IRepository<CompanyProfile> companyRepository,
        IPdfService pdfService) : IPreviewOfferLetter
    {
        [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
        private static partial Regex TokenRegex();

        public async Task<byte[]> PreviewAsync(SaveOfferLetterTemplateDto dto)
        {
            var company = await companyRepository.GetAll()
                .Select(p => new { p.CompanyName, p.ContactAddress, p.ContactPhone, p.ContactEmail, p.LogoContent })
                .FirstOrDefaultAsync();

            var sample = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CandidateName"] = "Selam Bekele Lemma",
                ["Position"] = "Senior Data Analyst",
                ["Salary"] = "18,500.00",
                ["StartDate"] = "01 August 2026",
                ["ExpiryDate"] = "25 July 2026",
                ["OfferNumber"] = "OFR-0001",
                ["EmploymentType"] = "Permanent",
                ["UnitName"] = "Data & Analytics",
                ["CompanyName"] = company?.CompanyName ?? "our organization",
                ["Today"] = DateTime.Now.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
            };

            var body = string.IsNullOrWhiteSpace(dto.Body) ? OfferLetterTemplate.DefaultBody : dto.Body;
            var merged = TokenRegex().Replace(body, m => sample.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);

            var document = new OfferLetterDocument(
                Logo: company?.LogoContent,
                CompanyName: company?.CompanyName,
                ContactAddress: company?.ContactAddress,
                ContactPhone: company?.ContactPhone,
                ContactEmail: company?.ContactEmail,
                Title: "Offer of Employment — OFR-0001 (Preview)",
                DateText: sample["Today"],
                Body: merged,
                SignatoryName: dto.SignatoryName,
                SignatoryTitle: dto.SignatoryTitle);

            return pdfService.RenderOfferLetter(document);
        }
    }

    // ---- The composer -----------------------------------------------------------

    public partial class OfferLetterComposer(
        IRepository<JobOffer> offerRepository,
        IRepository<JobApplication> applicationRepository,
        IRepository<Candidate> candidateRepository,
        IRepository<JobRequisition> requisitionRepository,
        IRepository<OrganizationUnit> organizationUnitRepository,
        IRepository<CompanyProfile> companyRepository,
        IRepository<OfferLetterTemplate> templateRepository) : IOfferLetterComposer
    {
        [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
        private static partial Regex TokenRegex();

        public async Task<ComposedOfferLetter> ComposeAsync(Guid offerId, string? bodyOverride = null)
        {
            var offer = await offerRepository.GetAll().FirstOrDefaultAsync(o => o.Id == offerId)
                ?? throw new NotFoundException(nameof(JobOffer), offerId.ToString());
            var application = await applicationRepository.GetAll()
                    .FirstOrDefaultAsync(a => a.Id == offer.ApplicationId)
                ?? throw new NotFoundException(nameof(JobApplication), offer.ApplicationId.ToString());
            var candidate = await candidateRepository.GetAll()
                .Where(c => c.Id == application.CandidateId)
                .Select(c => new { c.FirstName, c.FatherName, c.GrandFatherName })
                .FirstOrDefaultAsync();
            var requisition = await requisitionRepository.GetAll()
                .Where(q => q.Id == application.RequisitionId)
                .Select(q => new { q.Title, q.EmploymentType, q.OrganizationUnitId })
                .FirstOrDefaultAsync();
            var unitName = requisition is null
                ? null
                : await organizationUnitRepository.GetAll()
                    .Where(u => u.Id == requisition.OrganizationUnitId).Select(u => u.Name).FirstOrDefaultAsync();
            var company = await companyRepository.GetAll()
                .Select(p => new { p.CompanyName, p.ContactAddress, p.ContactPhone, p.ContactEmail, p.LogoContent })
                .FirstOrDefaultAsync();
            var template = await templateRepository.GetAll()
                .Select(t => new { t.Body, t.SignatoryName, t.SignatoryTitle })
                .FirstOrDefaultAsync();

            var candidateName = candidate is null
                ? "Candidate"
                : string.Join(" ", new[] { candidate.FirstName, candidate.FatherName, candidate.GrandFatherName }
                    .Where(n => !string.IsNullOrWhiteSpace(n)));

            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CandidateName"] = candidateName,
                ["Position"] = requisition?.Title ?? "the advertised role",
                ["Salary"] = offer.Salary.ToString("N2", CultureInfo.InvariantCulture),
                ["StartDate"] = offer.ProposedStartDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
                ["ExpiryDate"] = offer.ExpiryDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
                ["OfferNumber"] = offer.OfferNumber,
                ["EmploymentType"] = requisition?.EmploymentType.ToString() ?? "Permanent",
                ["UnitName"] = unitName ?? "—",
                ["CompanyName"] = company?.CompanyName ?? "our organization",
                ["Today"] = DateTime.Now.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
            };

            var sourceBody = bodyOverride ?? template?.Body ?? OfferLetterTemplate.DefaultBody;
            var mergedBody = TokenRegex().Replace(sourceBody,
                m => tokens.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);

            var document = new OfferLetterDocument(
                Logo: company?.LogoContent,
                CompanyName: company?.CompanyName,
                ContactAddress: company?.ContactAddress,
                ContactPhone: company?.ContactPhone,
                ContactEmail: company?.ContactEmail,
                Title: $"Offer of Employment — {offer.OfferNumber}",
                DateText: tokens["Today"],
                Body: mergedBody,
                SignatoryName: template?.SignatoryName,
                SignatoryTitle: template?.SignatoryTitle);

            return new ComposedOfferLetter(mergedBody, document);
        }
    }
}
