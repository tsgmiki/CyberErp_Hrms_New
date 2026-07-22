using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates.DTOs;
using CyberErp.Hrms.App.Features.Core.Leaves;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.DocumentTemplates
{
    public interface IGenerateAnnualLeaveDocument
    {
        Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid annualLeaveId);
    }

    /// <summary>
    /// Renders a document template against ONE annual-leave request (header + detail), reusing the
    /// same {{Placeholder}} merge + header/body/footer assembly as <see cref="GenerateEmployeeDocument"/>.
    /// Header fields (employee, request date, ledger, remark, grand total) are HTML-encoded scalars; the
    /// detail rows are emitted as a raw, system-built <c>{{LeaveDetailsTable}}</c> (cell values still
    /// encoded) — exactly like the clearance-checklist table — so they render as a real table in print.
    /// </summary>
    public partial class GenerateAnnualLeaveDocument(
        IRepository<DocumentTemplate> templates,
        IGetAnnualLeaveById getAnnualLeave,
        IGetCompanyLogo getLogo) : IGenerateAnnualLeaveDocument
    {
        [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
        private static partial Regex TokenRegex();

        public async Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid annualLeaveId)
        {
            var template = await templates.GetAll().FirstOrDefaultAsync(t => t.Id == templateId)
                ?? throw new NotFoundException(nameof(DocumentTemplate), templateId.ToString());

            // Branch/tenant-scoped read via the same handler the grid uses; 404 if not visible.
            var request = await getAnnualLeave.GetAsync(annualLeaveId);

            var tokens = await BuildTokensAsync(request);

            var html = Assemble(
                Merge(template.HeaderHtml, tokens),
                Merge(template.Body, tokens),
                Merge(template.FooterHtml, tokens));

            return new GeneratedDocumentDto
            {
                Title = $"{template.Name} - {request.EmployeeName}",
                Html = html
            };
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

        private async Task<Dictionary<string, string>> BuildTokensAsync(AnnualLeaveHeaderDto r)
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            void Add(string key, string? value) => tokens[key] = WebUtility.HtmlEncode(value ?? string.Empty);

            // Header (hrms_AnnualLeaveHeader)
            Add("EmployeeName", r.EmployeeName);
            Add("EmployeeNumber", r.EmployeeNumber);
            Add("RequestDate", FormatDate(r.RequestDate));
            Add("FiscalYear", r.FiscalYearName);
            Add("Ledger", r.FiscalYearName);               // the ledger is identified by its fiscal year
            Add("LedgerAvailable", r.LedgerAvailable.ToString("0.##", CultureInfo.InvariantCulture));
            Add("Remark", r.Remark);
            Add("Status", r.Status);
            Add("TotalLeaveDays", r.TotalLeaveDays.ToString("0.##", CultureInfo.InvariantCulture));
            Add("GrandTotal", r.TotalLeaveDays.ToString("0.##", CultureInfo.InvariantCulture)); // alias
            Add("LineCount", r.Details.Count.ToString(CultureInfo.InvariantCulture));
            Add("Today", DateTime.Now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));

            // {{LeaveDetailsTable}} — raw system-built table (values encoded), like {{ClearanceTable}}.
            tokens["LeaveDetailsTable"] = BuildDetailTable(r);

            // Company logo (raw data URI), blank when none uploaded.
            var logoUri = await TryGetLogoDataUriAsync();
            tokens["LogoUrl"] = logoUri ?? string.Empty;
            tokens["Logo"] = logoUri is null
                ? string.Empty
                : $"<img src=\"{logoUri}\" alt=\"logo\" style=\"max-height:64px;\"/>";

            return tokens;
        }

        private static string BuildDetailTable(AnnualLeaveHeaderDto r)
        {
            if (r.Details.Count == 0) return string.Empty;

            const string th = "border:1px solid #999;padding:6px 8px;text-align:left;background:#f2f2f2;";
            const string td = "border:1px solid #ccc;padding:6px 8px;text-align:left;";
            const string tdr = "border:1px solid #ccc;padding:6px 8px;text-align:right;";

            var table = new StringBuilder();
            table.Append("<table style=\"width:100%;border-collapse:collapse;font-size:13px;\">")
                .Append("<thead><tr>")
                .Append($"<th style=\"{th}\">#</th>")
                .Append($"<th style=\"{th}\">Leave Usage</th>")
                .Append($"<th style=\"{th}\">Start Date</th>")
                .Append($"<th style=\"{th}\">End Date</th>")
                .Append($"<th style=\"{th}text-align:right;\">Leave Days</th>")
                .Append("</tr></thead><tbody>");

            var i = 1;
            foreach (var d in r.Details)
            {
                table.Append("<tr>")
                    .Append($"<td style=\"{td}\">{i++}</td>")
                    .Append($"<td style=\"{td}\">{WebUtility.HtmlEncode(UsageLabel(d))}</td>")
                    .Append($"<td style=\"{td}\">{FormatDate(d.StartDate)}</td>")
                    .Append($"<td style=\"{td}\">{FormatDate(d.EndDate)}</td>")
                    .Append($"<td style=\"{tdr}\">{d.LeaveDays.ToString("0.##", CultureInfo.InvariantCulture)}</td>")
                    .Append("</tr>");
            }

            table.Append("</tbody><tfoot><tr>")
                .Append($"<td style=\"{td}font-weight:bold;\" colspan=\"4\">Grand Total</td>")
                .Append($"<td style=\"{tdr}font-weight:bold;\">{r.TotalLeaveDays.ToString("0.##", CultureInfo.InvariantCulture)}</td>")
                .Append("</tr></tfoot></table>");
            return table.ToString();
        }

        /// <summary>"Full Day" or "Half Day (Morning/Afternoon)" for a detail row.</summary>
        private static string UsageLabel(AnnualLeaveDetailDto d)
        {
            var usage = d.LeaveUsage == nameof(AnnualLeaveUsage.HalfDay) ? "Half Day" : "Full Day";
            return string.IsNullOrEmpty(d.HalfDayPart) ? usage : $"{usage} ({d.HalfDayPart})";
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

        private static string FormatDate(DateTime? value) =>
            value.HasValue ? value.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : string.Empty;
    }
}
