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
    public interface IGenerateSettlementDocument
    {
        Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid settlementId);
    }

    /// <summary>
    /// Renders a document template against ONE final settlement (HC218): the worksheet lines as an
    /// HTML table ({{LinesTable}}) plus totals, dates and the leaver's identity. Same
    /// {{Placeholder}} merge + assembly as the other generators.
    /// </summary>
    public partial class GenerateSettlementDocument(
        IRepository<DocumentTemplate> templates,
        IRepository<TerminationSettlement> settlements,
        IRepository<SettlementLine> lines,
        IRepository<EmployeeTermination> terminations,
        IRepository<Employee> employees,
        IGetCompanyLogo getLogo) : IGenerateSettlementDocument
    {
        [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
        private static partial Regex TokenRegex();

        public async Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid settlementId)
        {
            var template = await templates.GetAll().FirstOrDefaultAsync(t => t.Id == templateId)
                ?? throw new NotFoundException(nameof(DocumentTemplate), templateId.ToString());
            var settlement = await settlements.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == settlementId)
                ?? throw new NotFoundException(nameof(TerminationSettlement), settlementId.ToString());
            var termination = await terminations.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == settlement.TerminationId)
                ?? throw new NotFoundException(nameof(EmployeeTermination), settlement.TerminationId.ToString());
            var employee = await employees.GetAll()
                .Where(e => e.Id == termination.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    FullName = e.Person != null
                        ? (e.Person.FirstName + " " + (e.Person.FatherName ?? "") + " " + e.Person.GrandFatherName).Trim()
                        : e.EmployeeNumber
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), termination.EmployeeId.ToString());

            var worksheet = await lines.GetAll().AsNoTracking()
                .Where(l => l.TerminationSettlementId == settlementId)
                .OrderBy(l => l.SortOrder).ToListAsync();
            var earnings = worksheet.Where(l => l.Kind == SettlementLineKind.Earning).Sum(l => l.Amount);
            var deductions = worksheet.Where(l => l.Kind == SettlementLineKind.Deduction).Sum(l => l.Amount);

            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            void Add(string key, string? value) => tokens[key] = WebUtility.HtmlEncode(value ?? string.Empty);

            Add("FullName", employee.FullName);
            Add("EmployeeNumber", employee.EmployeeNumber);
            Add("TerminationType", termination.TerminationType.ToString());
            Add("LastWorkingDate", termination.LastWorkingDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            Add("TotalEarnings", earnings.ToString("N2", CultureInfo.InvariantCulture));
            Add("TotalDeductions", deductions.ToString("N2", CultureInfo.InvariantCulture));
            Add("NetAmount", (earnings - deductions).ToString("N2", CultureInfo.InvariantCulture));
            Add("SettlementStatus", settlement.Status.ToString());
            Add("PaidReference", settlement.PaidReference);
            Add("Today", DateTime.Now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));

            // The worksheet as an HTML table (raw token — not encoded).
            var sb = new StringBuilder();
            sb.Append("<table style=\"width:100%;border-collapse:collapse;font-size:13px;\">")
              .Append("<tr><th style=\"border:1px solid #ccc;padding:6px 8px;background:#f2f2f2;text-align:left;\">Item</th>")
              .Append("<th style=\"border:1px solid #ccc;padding:6px 8px;background:#f2f2f2;text-align:right;width:25%;\">Amount</th></tr>");
            foreach (var line in worksheet)
            {
                var amount = line.Kind == SettlementLineKind.Deduction ? -line.Amount : line.Amount;
                sb.Append("<tr><td style=\"border:1px solid #ccc;padding:6px 8px;\">")
                  .Append(WebUtility.HtmlEncode(line.Label))
                  .Append(line.Kind == SettlementLineKind.Deduction ? " (deduction)" : "")
                  .Append("</td><td style=\"border:1px solid #ccc;padding:6px 8px;text-align:right;\">")
                  .Append(amount.ToString("N2", CultureInfo.InvariantCulture)).Append("</td></tr>");
            }
            sb.Append("<tr><td style=\"border:1px solid #ccc;padding:6px 8px;font-weight:bold;\">NET SETTLEMENT</td>")
              .Append("<td style=\"border:1px solid #ccc;padding:6px 8px;text-align:right;font-weight:bold;\">")
              .Append((earnings - deductions).ToString("N2", CultureInfo.InvariantCulture)).Append("</td></tr></table>");
            tokens["LinesTable"] = sb.ToString();

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
