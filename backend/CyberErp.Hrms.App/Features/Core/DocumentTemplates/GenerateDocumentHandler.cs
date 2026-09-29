using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CyberErp.Hrms.App.Common;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates.DTOs;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.App.Features.Core.Employees.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.DocumentTemplates
{
    public class MergeFieldDto
    {
        public string Token { get; set; } = string.Empty;   // e.g. {{FullName}}
        public string Label { get; set; } = string.Empty;   // human description
        public string Group { get; set; } = string.Empty;   // palette grouping
    }

    public interface IGenerateEmployeeDocument
    {
        Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid employeeId);
    }

    public interface IGetDocumentMergeFields
    {
        Task<List<MergeFieldDto>> GetAsync();
    }

    /// <summary>
    /// Resolves a template's <c>{{Placeholder}}</c> tokens against one employee's master data and
    /// returns print-ready HTML (HC022). Employee values are HTML-encoded before substitution so
    /// data can never inject markup into the admin-authored template; the photo tokens are emitted
    /// raw as a self-contained data-URI image so the printed document needs no network access.
    /// </summary>
    public partial class GenerateEmployeeDocument(
        IRepository<DocumentTemplate> templates,
        IRepository<EmployeeTermination> terminations,
        IRepository<EmployeeMovement> movements,
        IRepository<Position> positions,
        IGetEmployeeById getEmployee,
        IGetEmployeePhoto getPhoto,
        IGetCompanyLogo getLogo) : IGenerateEmployeeDocument
    {
        [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
        private static partial Regex TokenRegex();

        public async Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid employeeId)
        {
            var template = await templates.GetAll().FirstOrDefaultAsync(t => t.Id == templateId)
                ?? throw new NotFoundException(nameof(DocumentTemplate), templateId.ToString());

            // Branch-filtered read — a branch admin cannot generate for another branch's employee.
            var employee = await getEmployee.GetAsync(employeeId);

            var tokens = await BuildTokensAsync(employee);

            // Header, body and footer are all merged, then assembled into one letterhead document.
            var html = Assemble(
                Merge(template.HeaderHtml, tokens),
                Merge(template.Body, tokens),
                Merge(template.FooterHtml, tokens));

            return new GeneratedDocumentDto
            {
                Title = $"{template.Name} - {employee.FullName}",
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

        private async Task<Dictionary<string, string>> BuildTokensAsync(EmployeeDto e)
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            void Add(string key, string? value) => tokens[key] = WebUtility.HtmlEncode(value ?? string.Empty);

            Add("EmployeeNumber", e.EmployeeNumber);
            Add("FirstName", e.FirstName);
            Add("FatherName", e.FatherName);
            Add("GrandFatherName", e.GrandFatherName);
            Add("FirstNameA", e.FirstNameA);
            Add("FatherNameA", e.FatherNameA);
            Add("GrandFatherNameA", e.GrandFatherNameA);
            // Legacy aliases — templates written before the person split keep merging.
            Add("MiddleName", e.FatherName);
            Add("LastName", e.GrandFatherName);
            Add("FullName", e.FullName);
            Add("Gender", e.Gender);
            Add("MaritalStatus", e.MaritalStatus);
            Add("EmploymentStatus", e.EmploymentStatus);
            Add("SpouseName", e.SpouseName);
            Add("DateOfBirth", FormatDate(e.DateOfBirth));
            Add("PlaceOfBirth", e.PlaceOfBirth);
            Add("LocationName", e.LocationName);
            Add("Address", e.LocationName); // legacy alias
            Add("PhoneNumber", e.PhoneNumber);
            Add("Email", e.Email);
            Add("NationalId", e.NationalId);
            Add("Tin", e.Tin);
            Add("PensionNumber", e.PensionNumber);
            Add("HireDate", FormatDate(e.HireDate));
            Add("Position", e.PositionClassTitle ?? e.PositionCode);
            Add("PositionTitle", e.PositionClassTitle);
            Add("PositionCode", e.PositionCode);
            Add("OrganizationUnit", e.OrganizationUnitName);
            Add("Branch", e.BranchName);
            Add("JobGrade", e.JobGradeName);
            Add("Salary", e.Salary?.ToString("N2", CultureInfo.InvariantCulture));
            Add("Today", DateTime.Now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            Add("TodayEC", EthiopianDate.Format(DateTime.Now));

            // Amharic header tokens. The full name is assembled from the *A parts rather than
            // stored, exactly as {{FullName}} is; it comes out blank when none are recorded.
            var fullNameA = string.Join(" ",
                new[] { e.FirstNameA, e.FatherNameA, e.GrandFatherNameA }
                    .Where(p => !string.IsNullOrWhiteSpace(p)));
            Add("FullNameA", fullNameA);

            // ---- Service history (experience letter) -------------------------------------------
            await AddServiceHistoryTokensAsync(e, tokens, Add);

            // Termination tokens (Experience / Termination letters) — from the employee's latest
            // case, preferring the settled one; all blank when no case exists.
            var termination = await terminations.GetAll()
                .Where(x => x.EmployeeId == e.Id)
                .OrderByDescending(x => x.Status == TerminationStatus.Settled)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new { x.TerminationType, x.NoticeDate, x.LastWorkingDate, x.SettledAt, x.Reason })
                .FirstOrDefaultAsync();
            Add("TerminationType", termination?.TerminationType.ToString());
            Add("TerminationNoticeDate", FormatDate(termination?.NoticeDate));
            Add("LastWorkingDate", FormatDate(termination?.LastWorkingDate));
            Add("TerminationDate", FormatDate(termination?.SettledAt ?? termination?.LastWorkingDate));
            Add("TerminationReason", termination?.Reason);

            // Clearance-certificate tokens — from the latest SETTLED termination's checklist.
            var clearanceCase = await terminations.GetAll()
                .Where(x => x.EmployeeId == e.Id && x.Status == TerminationStatus.Settled)
                .OrderByDescending(x => x.SettledAt)
                .Select(x => new
                {
                    x.SettledAt,
                    Clearances = x.Clearances
                        .OrderBy(c => c.Department)
                        .Select(c => new { c.Department, c.Description, c.Status, c.ClearedBy, c.ClearedAt })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            Add("ClearanceDate", FormatDate(clearanceCase?.SettledAt));
            Add("ClearanceStatus",
                clearanceCase != null && clearanceCase.Clearances.Count > 0
                    && clearanceCase.Clearances.All(c => c.Status == ClearanceStatus.Cleared)
                    ? "Fully Cleared" : "—");

            // {{ClearanceTable}} is system-built HTML (cell *values* are still encoded), emitted raw
            // like the photo/logo tokens so the checklist renders as a real table in the document.
            if (clearanceCase is null || clearanceCase.Clearances.Count == 0)
            {
                tokens["ClearanceTable"] = string.Empty;
            }
            else
            {
                const string th = "border:1px solid #999;padding:6px 8px;text-align:left;background:#f2f2f2;";
                const string td = "border:1px solid #ccc;padding:6px 8px;text-align:left;";
                var table = new StringBuilder();
                table.Append("<table style=\"width:100%;border-collapse:collapse;font-size:13px;\">")
                    .Append("<thead><tr>")
                    .Append($"<th style=\"{th}\">Department</th>")
                    .Append($"<th style=\"{th}\">Requirement</th>")
                    .Append($"<th style=\"{th}\">Status</th>")
                    .Append($"<th style=\"{th}\">Cleared By</th>")
                    .Append($"<th style=\"{th}\">Date</th>")
                    .Append("</tr></thead><tbody>");
                foreach (var c in clearanceCase.Clearances)
                    table.Append("<tr>")
                        .Append($"<td style=\"{td}\">{WebUtility.HtmlEncode(c.Department)}</td>")
                        .Append($"<td style=\"{td}\">{WebUtility.HtmlEncode(c.Description)}</td>")
                        .Append($"<td style=\"{td}\">{c.Status}</td>")
                        .Append($"<td style=\"{td}\">{WebUtility.HtmlEncode(c.ClearedBy ?? string.Empty)}</td>")
                        .Append($"<td style=\"{td}\">{FormatDate(c.ClearedAt)}</td>")
                        .Append("</tr>");
                table.Append("</tbody></table>");
                tokens["ClearanceTable"] = table.ToString();
            }

            // Custom fields (HC021) by definition name — master tokens win on any name clash.
            foreach (var (name, value) in e.CustomFields)
                if (!tokens.ContainsKey(name)) Add(name, value);

            // Photo tokens emitted raw (a self-contained data URI), never HTML-encoded.
            var dataUri = await TryGetPhotoDataUriAsync(e.Id);
            tokens["PhotoUrl"] = dataUri ?? string.Empty;
            tokens["Photo"] = dataUri is null
                ? string.Empty
                : $"<img src=\"{dataUri}\" alt=\"photo\" style=\"width:100px;height:120px;object-fit:cover;\"/>";

            // Company logo tokens (also raw data URIs) — blank when no logo has been uploaded.
            var logoUri = await TryGetLogoDataUriAsync();
            tokens["LogoUrl"] = logoUri ?? string.Empty;
            tokens["Logo"] = logoUri is null
                ? string.Empty
                : $"<img src=\"{logoUri}\" alt=\"logo\" style=\"max-height:64px;\"/>";

            return tokens;
        }

        /// <summary>
        /// The employee's post history at this organisation, bilingual, for an experience letter.
        /// </summary>
        /// <remarks>
        /// <para>⚠️ ONLY EXECUTED MOVEMENTS COUNT. A Pending or Approved movement is a decision, not
        /// a fact — a future-dated promotion has not happened, and a letter that states it is
        /// attesting to something untrue. Cancelled ones never happened at all.</para>
        ///
        /// <para>⚠️ This is service AT THIS ORGANISATION, derived from <c>HireDate</c> plus
        /// movements — NOT <c>EmployeeExperience</c>, which holds prior employers. A "To Whom It May
        /// Concern" letter attests to what this employer can vouch for; restating a candidate's
        /// self-reported history elsewhere over this organisation's signature is a different claim
        /// entirely.</para>
        ///
        /// <para>⚠️ The Amharic column falls back to the English title when no <c>TitleA</c> is
        /// recorded (813 of 814 position classes, at the time of writing). A blank cell would make
        /// the letter unusable today; an untranslated one is merely untranslated.</para>
        /// </remarks>
        private async Task AddServiceHistoryTokensAsync(
            EmployeeDto e, Dictionary<string, string> tokens, Action<string, string?> add)
        {
            var rows = await movements.GetAll().AsNoTracking()
                .Where(m => m.EmployeeId == e.Id
                            && m.Status == MovementStatus.Completed
                            && m.ToPositionId != null)
                .OrderBy(m => m.EffectiveDate)
                .Select(m => new { m.EffectiveDate, m.FromPositionId, m.ToPositionId })
                .ToListAsync();

            // Every position referenced by the history, resolved to its class titles in one read.
            var positionIds = rows.SelectMany(r => new[] { r.FromPositionId, r.ToPositionId })
                .Where(id => id.HasValue).Select(id => id!.Value)
                .Concat(e.PositionId.HasValue ? [e.PositionId.Value] : [])
                .Distinct().ToList();

            var titles = positionIds.Count == 0
                ? []
                : await positions.GetAll().AsNoTracking()
                    .Where(p => positionIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.PositionClass!.Title, p.PositionClass.TitleA })
                    .ToListAsync();

            string? TitleOf(Guid? id) =>
                id is Guid g ? titles.FirstOrDefault(t => t.Id == g)?.Title : null;
            string? TitleAOf(Guid? id) =>
                id is Guid g ? titles.FirstOrDefault(t => t.Id == g)?.TitleA : null;

            add("PositionA", TitleAOf(e.PositionId) ?? e.PositionClassTitle);
            add("HireDateEC", e.HireDate.HasValue ? EthiopianDate.Format(e.HireDate.Value) : null);

            // The post held at hire: the first movement's "from" side is the record of it. Without
            // any movement the employee has only ever held their current post.
            var hiredAs = rows.Count > 0 ? TitleOf(rows[0].FromPositionId) : e.PositionClassTitle;
            var hiredAsA = rows.Count > 0 ? TitleAOf(rows[0].FromPositionId) : TitleAOf(e.PositionId);

            var lastWorkingDate = await terminations.GetAll().AsNoTracking()
                .Where(x => x.EmployeeId == e.Id && x.Status == TerminationStatus.Settled)
                .OrderByDescending(x => x.SettledAt)
                .Select(x => (DateTime?)x.LastWorkingDate)
                .FirstOrDefaultAsync();

            var history = ServiceHistory.Build(
                e.HireDate,
                hiredAs ?? e.PositionClassTitle,
                hiredAsA,
                rows.Select(r => new PositionChange(
                    r.EffectiveDate,
                    TitleOf(r.ToPositionId) ?? string.Empty,
                    TitleAOf(r.ToPositionId))),
                lastWorkingDate);

            add("ServiceFrom", history.Count > 0 ? FormatDate(history[0].From) : null);
            add("ServiceFromEC", history.Count > 0 ? EthiopianDate.Format(history[0].From) : null);
            add("ServiceTo", lastWorkingDate.HasValue ? FormatDate(lastWorkingDate) : "to date");

            // System-built HTML, emitted raw like {{ClearanceTable}} — cell VALUES are still encoded.
            tokens["ServiceHistoryTable"] = RenderServiceHistory(history);
        }

        /// <summary>
        /// The bilingual history table: one row per post, English left, Amharic right.
        /// </summary>
        /// <remarks>
        /// ⚠️ One row per POST rather than two independent lists side by side. Two lists would drift
        /// apart the moment one language wrapped onto a second line, and a reader comparing them
        /// would be matching the wrong post to the wrong dates.
        /// </remarks>
        private static string RenderServiceHistory(List<ServicePeriod> history)
        {
            if (history.Count == 0) return string.Empty;

            const string th = "border:1px solid #999;padding:6px 10px;text-align:left;background:#f2f2f2;width:50%;";
            const string td = "border:1px solid #ccc;padding:6px 10px;text-align:left;vertical-align:top;width:50%;";
            const string dates = "color:#444;font-size:12px;";

            var sb = new StringBuilder();
            sb.Append("<table style=\"width:100%;border-collapse:collapse;font-size:13px;table-layout:fixed;\">")
              .Append("<thead><tr>")
              .Append($"<th style=\"{th}\">Position &amp; Period</th>")
              .Append($"<th style=\"{th}\">የሥራ መደብና ጊዜ</th>")
              .Append("</tr></thead><tbody>");

            foreach (var p in history)
            {
                var title = WebUtility.HtmlEncode(p.Title);
                // Falls back to the English title — see the note on AddServiceHistoryTokensAsync.
                var titleA = WebUtility.HtmlEncode(
                    string.IsNullOrWhiteSpace(p.TitleAmharic) ? p.Title : p.TitleAmharic);

                var rangeEn = $"{FormatDate(p.From)} – {(p.To.HasValue ? FormatDate(p.To) : "to date")}";
                var rangeAm = $"{EthiopianDate.Format(p.From)} – "
                    + (p.To.HasValue ? EthiopianDate.Format(p.To.Value) : "እስከ አሁን");

                sb.Append("<tr>")
                  .Append($"<td style=\"{td}\"><strong>{title}</strong><br/><span style=\"{dates}\">{rangeEn}</span></td>")
                  .Append($"<td style=\"{td}\"><strong>{titleA}</strong><br/><span style=\"{dates}\">{rangeAm}</span></td>")
                  .Append("</tr>");
            }

            sb.Append("</tbody></table>");
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
                return null;   // no company logo configured — leave logo tokens blank
            }
        }

        private async Task<string?> TryGetPhotoDataUriAsync(Guid employeeId)
        {
            try
            {
                var (content, contentType) = await getPhoto.GetAsync(employeeId);
                return $"data:{contentType};base64,{Convert.ToBase64String(content)}";
            }
            catch (NotFoundException)
            {
                return null;   // employee has no photo — leave photo tokens blank
            }
        }

        private static string FormatDate(DateTime? value) =>
            value.HasValue ? value.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : string.Empty;
    }

    /// <summary>Catalog of tokens available to a template author, incl. dynamic custom fields.</summary>
    public class GetDocumentMergeFields(IRepository<EmployeeFieldDefinition> fieldDefinitions) : IGetDocumentMergeFields
    {
        private static readonly MergeFieldDto[] Standard =
        [
            new() { Token = "{{FullName}}", Label = "Full name", Group = "Employee" },
            new() { Token = "{{FirstName}}", Label = "First name", Group = "Employee" },
            new() { Token = "{{FatherName}}", Label = "Father name", Group = "Employee" },
            new() { Token = "{{GrandFatherName}}", Label = "Grandfather name", Group = "Employee" },
            new() { Token = "{{FirstNameA}}", Label = "First name (Amharic)", Group = "Employee" },
            new() { Token = "{{FatherNameA}}", Label = "Father name (Amharic)", Group = "Employee" },
            new() { Token = "{{GrandFatherNameA}}", Label = "Grandfather name (Amharic)", Group = "Employee" },
            new() { Token = "{{EmployeeNumber}}", Label = "Employee number", Group = "Employee" },
            new() { Token = "{{Gender}}", Label = "Gender", Group = "Employee" },
            new() { Token = "{{MaritalStatus}}", Label = "Marital status", Group = "Employee" },
            new() { Token = "{{DateOfBirth}}", Label = "Date of birth", Group = "Employee" },
            new() { Token = "{{PlaceOfBirth}}", Label = "Place of birth", Group = "Employee" },
            new() { Token = "{{SpouseName}}", Label = "Spouse name", Group = "Employee" },
            new() { Token = "{{LocationName}}", Label = "Location / address", Group = "Contact" },
            new() { Token = "{{PhoneNumber}}", Label = "Phone number", Group = "Contact" },
            new() { Token = "{{Email}}", Label = "Email", Group = "Contact" },
            new() { Token = "{{NationalId}}", Label = "National ID", Group = "Identification" },
            new() { Token = "{{Tin}}", Label = "TIN", Group = "Identification" },
            new() { Token = "{{PensionNumber}}", Label = "Pension number", Group = "Identification" },
            new() { Token = "{{Position}}", Label = "Position title", Group = "Placement" },
            new() { Token = "{{PositionCode}}", Label = "Position code", Group = "Placement" },
            new() { Token = "{{OrganizationUnit}}", Label = "Organization unit", Group = "Placement" },
            new() { Token = "{{Branch}}", Label = "Branch", Group = "Placement" },
            new() { Token = "{{JobGrade}}", Label = "Job grade", Group = "Placement" },
            new() { Token = "{{Salary}}", Label = "Salary", Group = "Placement" },
            new() { Token = "{{EmploymentStatus}}", Label = "Employment status", Group = "Placement" },
            new() { Token = "{{HireDate}}", Label = "Hire date", Group = "Placement" },
            new() { Token = "{{TerminationType}}", Label = "Termination type", Group = "Termination" },
            new() { Token = "{{TerminationDate}}", Label = "Termination (settlement) date", Group = "Termination" },
            new() { Token = "{{LastWorkingDate}}", Label = "Last working date", Group = "Termination" },
            new() { Token = "{{TerminationNoticeDate}}", Label = "Notice date", Group = "Termination" },
            new() { Token = "{{TerminationReason}}", Label = "Termination reason", Group = "Termination" },
            new() { Token = "{{ClearanceTable}}", Label = "Clearance checklist (table)", Group = "Clearance" },
            new() { Token = "{{ClearanceStatus}}", Label = "Overall clearance status", Group = "Clearance" },
            new() { Token = "{{ClearanceDate}}", Label = "Clearance (settlement) date", Group = "Clearance" },
            // Annual-leave request tokens (resolved only for an Annual Leave Request template).
            new() { Token = "{{EmployeeName}}", Label = "Employee name", Group = "Annual Leave" },
            new() { Token = "{{RequestDate}}", Label = "Request date", Group = "Annual Leave" },
            new() { Token = "{{FiscalYear}}", Label = "Ledger fiscal year", Group = "Annual Leave" },
            new() { Token = "{{Ledger}}", Label = "Selected ledger (fiscal year)", Group = "Annual Leave" },
            new() { Token = "{{LedgerAvailable}}", Label = "Ledger available balance", Group = "Annual Leave" },
            new() { Token = "{{Remark}}", Label = "Request remark", Group = "Annual Leave" },
            new() { Token = "{{TotalLeaveDays}}", Label = "Grand total leave days", Group = "Annual Leave" },
            new() { Token = "{{LeaveDetailsTable}}", Label = "Leave lines (table)", Group = "Annual Leave" },
            // Experience letter (HC022): service at THIS organisation, bilingual.
            new() { Token = "{{ServiceHistoryTable}}", Label = "Position history, English + Amharic (table)", Group = "Experience" },
            new() { Token = "{{ServiceFrom}}", Label = "Service start date", Group = "Experience" },
            new() { Token = "{{ServiceFromEC}}", Label = "Service start date (Ethiopian)", Group = "Experience" },
            new() { Token = "{{ServiceTo}}", Label = "Service end date, or ‘to date’", Group = "Experience" },
            new() { Token = "{{FullNameA}}", Label = "Full name (Amharic)", Group = "Experience" },
            new() { Token = "{{PositionA}}", Label = "Position title (Amharic)", Group = "Experience" },
            new() { Token = "{{HireDateEC}}", Label = "Hire date (Ethiopian)", Group = "Experience" },
            new() { Token = "{{TodayEC}}", Label = "Today’s date (Ethiopian)", Group = "Experience" },
            new() { Token = "{{Today}}", Label = "Today's date", Group = "Document" },
            new() { Token = "{{Photo}}", Label = "Photo (image)", Group = "Document" },
            new() { Token = "{{PhotoUrl}}", Label = "Photo URL (for <img src>)", Group = "Document" },
            new() { Token = "{{Logo}}", Label = "Company logo (image)", Group = "Document" },
            new() { Token = "{{LogoUrl}}", Label = "Company logo URL (for <img src>)", Group = "Document" },
        ];

        public async Task<List<MergeFieldDto>> GetAsync()
        {
            var fields = new List<MergeFieldDto>(Standard);

            var custom = await fieldDefinitions.GetAll()
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder)
                .Select(d => new MergeFieldDto
                {
                    Token = "{{" + d.Name + "}}",
                    Label = d.Label,
                    Group = "Custom fields"
                })
                .ToListAsync();

            fields.AddRange(custom);
            return fields;
        }
    }
}
