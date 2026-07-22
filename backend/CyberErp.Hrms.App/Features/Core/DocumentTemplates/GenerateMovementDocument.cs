using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates.DTOs;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.DocumentTemplates
{
    public interface IGenerateMovementDocument
    {
        Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid movementId);
    }

    /// <summary>
    /// Renders a document template against ONE personnel movement — the formal transfer notice
    /// (HC174): new role, unit/location, effective (start) date and the REPORTING LINE (the target
    /// unit's manager, resolved from the org structure). Reuses the same {{Placeholder}} merge +
    /// header/body/footer assembly as <see cref="GenerateEmployeeDocument"/>.
    /// </summary>
    public partial class GenerateMovementDocument(
        IRepository<DocumentTemplate> templates,
        IRepository<EmployeeMovement> movements,
        IRepository<Employee> employees,
        IRepository<Position> positions,
        IRepository<Branch> branches,
        IOrgManagerResolver managerResolver,
        IGetCompanyLogo getLogo) : IGenerateMovementDocument
    {
        [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
        private static partial Regex TokenRegex();

        public async Task<GeneratedDocumentDto> GenerateAsync(Guid templateId, Guid movementId)
        {
            var template = await templates.GetAll().FirstOrDefaultAsync(t => t.Id == templateId)
                ?? throw new NotFoundException(nameof(DocumentTemplate), templateId.ToString());
            var movement = await movements.GetAll().AsNoTracking().FirstOrDefaultAsync(m => m.Id == movementId)
                ?? throw new NotFoundException(nameof(EmployeeMovement), movementId.ToString());
            // Tenant/branch-scoped employee read — 404 when the caller cannot see the employee.
            var employee = await employees.GetAll()
                .Where(e => e.Id == movement.EmployeeId)
                .Select(e => new
                {
                    e.EmployeeNumber,
                    FullName = e.Person != null
                        ? (e.Person.FirstName + " " + (e.Person.FatherName ?? "") + " " + e.Person.GrandFatherName).Trim()
                        : e.EmployeeNumber
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), movement.EmployeeId.ToString());

            var tokens = await BuildTokensAsync(movement, employee.FullName, employee.EmployeeNumber);

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

        private async Task<Dictionary<string, string>> BuildTokensAsync(EmployeeMovement m, string fullName, string employeeNumber)
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            void Add(string key, string? value) => tokens[key] = WebUtility.HtmlEncode(value ?? string.Empty);

            // Position display names by id from the unfiltered set (historical snapshots stay readable).
            var posSet = positions.GetAllWithoutTenantFilter();
            var from = m.FromPositionId.HasValue
                ? await posSet.Where(p => p.Id == m.FromPositionId.Value)
                    .Select(p => new
                    {
                        Name = p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : ""),
                        UnitId = (Guid?)p.OrganizationUnitId,
                        UnitName = p.OrganizationUnit != null ? p.OrganizationUnit.Name : null,
                        p.BranchId
                    }).FirstOrDefaultAsync()
                : null;
            var to = m.ToPositionId.HasValue
                ? await posSet.Where(p => p.Id == m.ToPositionId.Value)
                    .Select(p => new
                    {
                        Name = p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : ""),
                        UnitId = (Guid?)p.OrganizationUnitId,
                        UnitName = p.OrganizationUnit != null ? p.OrganizationUnit.Name : null,
                        p.BranchId
                    }).FirstOrDefaultAsync()
                : null;
            var branchSet = branches.GetAllWithoutTenantFilter();
            var fromBranch = from?.BranchId is Guid fb ? await branchSet.Where(b => b.Id == fb).Select(b => b.Name).FirstOrDefaultAsync() : null;
            var toBranch = to?.BranchId is Guid tb ? await branchSet.Where(b => b.Id == tb).Select(b => b.Name).FirstOrDefaultAsync() : null;

            // Reporting line (HC174): the manager of the TARGET unit (org-tree climb, excluding the employee).
            string? reportingLine = null;
            if (to?.UnitId is Guid unitId)
                reportingLine = (await managerResolver.ResolveUnitManagerAsync(unitId, m.EmployeeId))?.Name;

            Add("FullName", fullName);
            Add("EmployeeNumber", employeeNumber);
            Add("MovementType", m.MovementType.ToString());
            Add("TransferKind", m.TransferKind?.ToString());
            Add("EffectiveDate", m.EffectiveDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
            Add("FromPosition", from?.Name);
            Add("FromUnit", from?.UnitName);
            Add("FromBranch", fromBranch);
            Add("NewPosition", to?.Name);
            Add("NewUnit", to?.UnitName);
            Add("NewBranch", toBranch);
            Add("ReportingLine", string.IsNullOrWhiteSpace(reportingLine) ? "To be confirmed" : reportingLine);
            Add("Reason", m.Reason);
            Add("Status", m.Status.ToString());
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
