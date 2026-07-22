using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.DocumentTemplates
{
    public interface IGenerateTrainingCertificate
    {
        Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid certificateId);
    }

    /// <summary>
    /// Renders a document template against ONE training certificate (HC200) — the digital certificate
    /// for a completed program: course, certificate number, issue/expiry dates, assessment score and
    /// CPD hours. Reuses the same {{Placeholder}} merge + header/body/footer assembly as the other
    /// document generators.
    /// </summary>
    public partial class GenerateTrainingCertificate(
        IRepository<DocumentTemplate> templates,
        IRepository<EmployeeTrainingCertificate> certificates,
        IRepository<Employee> employees,
        IRepository<TrainingCourse> courses,
        IRepository<TrainingEnrollment> enrollments,
        IRepository<TrainingSession> sessions,
        IGetCompanyLogo getLogo) : IGenerateTrainingCertificate
    {
        [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
        private static partial Regex TokenRegex();

        public async Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid certificateId)
        {
            var template = await templates.GetAll().FirstOrDefaultAsync(t => t.Id == templateId)
                ?? throw new NotFoundException(nameof(DocumentTemplate), templateId.ToString());
            var certificate = await certificates.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == certificateId)
                ?? throw new NotFoundException(nameof(EmployeeTrainingCertificate), certificateId.ToString());
            // Tenant/branch-scoped employee read — 404 when the caller cannot see the employee.
            var employee = await employees.GetAll()
                .Where(e => e.Id == certificate.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    FullName = e.Person != null
                        ? (e.Person.FirstName + " " + (e.Person.FatherName ?? "") + " " + e.Person.GrandFatherName).Trim()
                        : e.EmployeeNumber
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), certificate.EmployeeId.ToString());

            var tokens = await BuildTokensAsync(certificate, employee.FullName, employee.EmployeeNumber);

            var html = Assemble(
                Merge(template.HeaderHtml, tokens),
                Merge(template.Body, tokens),
                Merge(template.FooterHtml, tokens));

            return new GeneratedDocumentDto { Title = $"{template.Name} - {employee.FullName}", Html = html };
        }

        private static string Merge(string? section, Dictionary<string, string> tokens) =>
            string.IsNullOrEmpty(section)
                ? string.Empty
                : TokenRegex().Replace(section, m => tokens.TryGetValue(m.Groups[1].Value, out var v) ? v : string.Empty);

        private static string Assemble(string header, string body, string footer)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(header))
                sb.Append("<div class=\"doc-header\" style=\"margin-bottom:16px;\">").Append(header).Append("</div>");
            sb.Append("<div class=\"doc-body\">").Append(body).Append("</div>");
            if (!string.IsNullOrWhiteSpace(footer))
                sb.Append("<div class=\"doc-footer\" style=\"margin-top:24px;padding-top:8px;border-top:1px solid #ccc;font-size:12px;color:#555;\">")
                  .Append(footer).Append("</div>");
            return sb.ToString();
        }

        private async Task<Dictionary<string, string>> BuildTokensAsync(
            EmployeeTrainingCertificate c, string fullName, string employeeNumber)
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            void Add(string key, string? value) => tokens[key] = WebUtility.HtmlEncode(value ?? string.Empty);

            var course = c.TrainingCourseId.HasValue
                ? await courses.GetAll().AsNoTracking().Where(x => x.Id == c.TrainingCourseId.Value)
                    .Select(x => new { x.Name, x.CpdHours, x.DurationHours }).FirstOrDefaultAsync()
                : null;

            decimal? score = null;
            string? trainer = null, provider = null;
            if (c.TrainingEnrollmentId.HasValue)
            {
                var detail = await enrollments.GetAll().AsNoTracking()
                    .Where(e => e.Id == c.TrainingEnrollmentId.Value)
                    .Join(sessions.GetAll(), e => e.TrainingSessionId, s => s.Id,
                        (e, s) => new { e.AssessmentScore, s.TrainerName, s.ProviderName })
                    .FirstOrDefaultAsync();
                score = detail?.AssessmentScore;
                trainer = detail?.TrainerName;
                provider = detail?.ProviderName;
            }

            Add("FullName", fullName);
            Add("EmployeeNumber", employeeNumber);
            Add("CertificateTitle", c.Title);
            Add("CertificateNo", c.CertificateNo);
            Add("CourseName", course?.Name ?? c.Title);
            Add("DurationHours", course?.DurationHours?.ToString("0.#", CultureInfo.InvariantCulture));
            Add("CpdHours", course?.CpdHours.ToString("0.#", CultureInfo.InvariantCulture));
            Add("Score", score?.ToString("0.#", CultureInfo.InvariantCulture));
            Add("Trainer", trainer);
            Add("Provider", provider);
            Add("IssuedOn", c.IssuedOn.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            Add("ExpiresOn", c.ExpiresOn?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "No expiry");
            Add("Today", DateTime.Now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));

            var logoUri = await TryGetLogoDataUriAsync();
            tokens["LogoUrl"] = logoUri ?? string.Empty;
            tokens["Logo"] = logoUri is null ? string.Empty : $"<img src=\"{logoUri}\" alt=\"logo\" style=\"max-height:64px;\"/>";

            return tokens;
        }

        private async Task<string?> TryGetLogoDataUriAsync()
        {
            try
            {
                var (content, contentType) = await getLogo.GetAsync();
                return $"data:{contentType};base64,{Convert.ToBase64String(content)}";
            }
            catch (NotFoundException)
            {
                return null;
            }
        }
    }
}
