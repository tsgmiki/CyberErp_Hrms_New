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
    public interface IGenerateTerminationDocument
    {
        Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid terminationId);
    }

    /// <summary>
    /// Renders a document template against ONE exit case — the resignation-acceptance or
    /// termination-notice letter (HC211): exit type, notice/last-working dates, reason and the
    /// employee's (possibly vacated) placement. Reuses the same {{Placeholder}} merge +
    /// header/body/footer assembly as the other generators.
    /// </summary>
    public partial class GenerateTerminationDocument(
        IRepository<DocumentTemplate> templates,
        IRepository<EmployeeTermination> terminations,
        IRepository<Employee> employees,
        IRepository<Position> positions,
        IGetCompanyLogo getLogo) : IGenerateTerminationDocument
    {
        [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
        private static partial Regex TokenRegex();

        public async Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid terminationId)
        {
            var template = await templates.GetAll().FirstOrDefaultAsync(t => t.Id == templateId)
                ?? throw new NotFoundException(nameof(DocumentTemplate), templateId.ToString());
            var termination = await terminations.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == terminationId)
                ?? throw new NotFoundException(nameof(EmployeeTermination), terminationId.ToString());
            // Tenant/branch-scoped employee read — 404 when the caller cannot see the employee.
            var employee = await employees.GetAll()
                .Where(e => e.Id == termination.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    e.PositionId,
                    FullName = e.Person != null
                        ? (e.Person.FirstName + " " + (e.Person.FatherName ?? "") + " " + e.Person.GrandFatherName).Trim()
                        : e.EmployeeNumber
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), termination.EmployeeId.ToString());

            // A settled case snapshots the vacated seat; a live case reads the current one.
            var positionId = termination.VacatedPositionId ?? employee.PositionId;
            var placement = positionId.HasValue
                ? await positions.GetAllWithoutTenantFilter()
                    .Where(p => p.Id == positionId.Value)
                    .Select(p => new
                    {
                        Name = p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : ""),
                        UnitName = p.OrganizationUnit != null ? p.OrganizationUnit.Name : null
                    }).FirstOrDefaultAsync()
                : null;

            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            void Add(string key, string? value) => tokens[key] = WebUtility.HtmlEncode(value ?? string.Empty);

            Add("FullName", employee.FullName);
            Add("EmployeeNumber", employee.EmployeeNumber);
            Add("TerminationType", termination.TerminationType.ToString());
            Add("NoticeDate", termination.NoticeDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            Add("LastWorkingDate", termination.LastWorkingDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            Add("Reason", termination.Reason);
            Add("Status", termination.Status.ToString());
            Add("Position", placement?.Name);
            Add("Unit", placement?.UnitName);
            Add("Today", DateTime.Now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));

            var logoUri = await TryGetLogoDataUriAsync();
            tokens["LogoUrl"] = logoUri ?? string.Empty;
            tokens["Logo"] = logoUri is null ? string.Empty : $"<img src=\"{logoUri}\" alt=\"logo\" style=\"max-height:64px;\"/>";

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
