using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
