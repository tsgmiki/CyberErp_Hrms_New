using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates.DTOs;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.DocumentTemplates
{
    public interface ICreateDocumentTemplate { Task<Guid> CreateAsync(CreateDocumentTemplateDto dto); }
    public interface IUpdateDocumentTemplate { Task UpdateAsync(UpdateDocumentTemplateDto dto); }
    public interface IDeleteDocumentTemplate { Task DeleteAsync(Guid id); }
    public interface IGetDocumentTemplateById { Task<DocumentTemplateDto> GetAsync(Guid id); }
    public interface IGetAllDocumentTemplates { Task<PaginatedResponse<DocumentTemplateDto>> GetAsync(GetAllRequest request); }
    public interface ISeedDefaultDocumentTemplates { Task<int> SeedAsync(); }

    internal static class DocumentTemplateShared
    {
        internal static readonly System.Linq.Expressions.Expression<Func<DocumentTemplate, DocumentTemplateDto>> Projection = t => new DocumentTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            DocumentType = t.DocumentType.ToString(),
            HeaderHtml = t.HeaderHtml,
            Body = t.Body,
            FooterHtml = t.FooterHtml,
            Description = t.Description,
            IsActive = t.IsActive
        };

        /// <summary>
        /// List rows WITHOUT the HTML payload: Body/Header/Footer can be tens of KB per template
        /// and the grid never renders them — the editor loads the full record by id. Body stays
        /// non-null (empty) so the DTO contract is unchanged for list consumers.
        /// </summary>
        internal static readonly System.Linq.Expressions.Expression<Func<DocumentTemplate, DocumentTemplateDto>> ListProjection = t => new DocumentTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            DocumentType = t.DocumentType.ToString(),
            Description = t.Description,
            IsActive = t.IsActive,
            Body = string.Empty
        };
    }

    // ---- Create ---------------------------------------------------------------

    public class CreateDocumentTemplate(
        IRepository<DocumentTemplate> repository,
        IValidator<CreateDocumentTemplateDto> validator,
        ILogger<CreateDocumentTemplate> logger) : ICreateDocumentTemplate
    {
        public async Task<Guid> CreateAsync(CreateDocumentTemplateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name))
                throw new DuplicateException(nameof(DocumentTemplate), nameof(dto.Name), dto.Name);

            var entity = DocumentTemplate.Create(
                dto.Name, Enum.Parse<DocumentTemplateType>(dto.DocumentType),
                dto.Body, dto.HeaderHtml, dto.FooterHtml, dto.Description, dto.IsActive);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created DocumentTemplate {Id} ({Name})", entity.Id, entity.Name);
            return entity.Id;
        }
    }

    // ---- Update ---------------------------------------------------------------

    public class UpdateDocumentTemplate(
        IRepository<DocumentTemplate> repository,
        IValidator<UpdateDocumentTemplateDto> validator,
        ILogger<UpdateDocumentTemplate> logger) : IUpdateDocumentTemplate
    {
        public async Task UpdateAsync(UpdateDocumentTemplateDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(DocumentTemplate), dto.Id.ToString());

            if (await repository.GetAll().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id))
                throw new DuplicateException(nameof(DocumentTemplate), nameof(dto.Name), dto.Name);

            entity.Update(dto.Name, Enum.Parse<DocumentTemplateType>(dto.DocumentType),
                dto.Body, dto.HeaderHtml, dto.FooterHtml, dto.Description, dto.IsActive);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Updated DocumentTemplate {Id}", entity.Id);
        }
    }

    // ---- Delete ---------------------------------------------------------------

    public class DeleteDocumentTemplate(
        IRepository<DocumentTemplate> repository,
        ILogger<DeleteDocumentTemplate> logger) : IDeleteDocumentTemplate
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(DocumentTemplate), id.ToString());
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted DocumentTemplate {Id}", id);
        }
    }

    // ---- Get by id ------------------------------------------------------------

    public class GetDocumentTemplateById(IRepository<DocumentTemplate> repository) : IGetDocumentTemplateById
    {
        public async Task<DocumentTemplateDto> GetAsync(Guid id) =>
            await repository.GetAll()
                .Where(t => t.Id == id)
                .Select(DocumentTemplateShared.Projection)
                .FirstOrDefaultAsync()
            ?? throw new NotFoundException(nameof(DocumentTemplate), id.ToString());
    }

    // ---- Get all (paged) ------------------------------------------------------

    public class GetAllDocumentTemplates(IRepository<DocumentTemplate> repository) : IGetAllDocumentTemplates
    {
        public async Task<PaginatedResponse<DocumentTemplateDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DocumentTemplateType>(request.Status, out var type))
                query = query.Where(x => x.DocumentType == type);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) ||
                    (x.Description != null && x.Description.Contains(term)));
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.Name)
                .Skip(skip).Take(take)
                .Select(DocumentTemplateShared.ListProjection)
                .ToListAsync();

            return new PaginatedResponse<DocumentTemplateDto> { Total = total, Data = data };
        }
    }

    // ---- Seed default templates (idempotent) ------------------------------------

    /// <summary>
    /// Creates ready-to-use starter templates when absent (idempotent per tenant, matched by name).
    /// Currently ships the <b>Clearance Certificate</b> that renders the offboarding checklist via the
    /// {{ClearanceTable}} merge token — so HR can print a final clearance document out of the box.
    /// </summary>
    public class SeedDefaultDocumentTemplates(
        IRepository<DocumentTemplate> repository,
        ILogger<SeedDefaultDocumentTemplates> logger) : ISeedDefaultDocumentTemplates
    {
        private const string ClearanceCertificateName = "Clearance Certificate";

        private const string ClearanceHeader =
            "<div style=\"display:flex;align-items:center;justify-content:space-between;\">" +
            "<div>{{Logo}}</div>" +
            "<div style=\"text-align:right;\"><div style=\"font-size:12px;color:#555;\">{{Branch}}</div></div>" +
            "</div>";

        private const string ClearanceBody =
            "<h2 style=\"text-align:center;margin:8px 0 16px;letter-spacing:1px;\">CLEARANCE CERTIFICATE</h2>" +
            "<p>This is to certify that <strong>{{FullName}}</strong> (Employee No. <strong>{{EmployeeNumber}}</strong>), " +
            "formerly holding the position of <strong>{{Position}}</strong>, has completed the organizational " +
            "clearance process following separation effective <strong>{{TerminationDate}}</strong> " +
            "(last working day {{LastWorkingDate}}).</p>" +
            "<p>Departmental clearance status: <strong>{{ClearanceStatus}}</strong></p>" +
            "<div style=\"margin:12px 0;\">{{ClearanceTable}}</div>" +
            "<p>Accordingly, the employee is hereby granted final clearance as of {{ClearanceDate}}.</p>";

        private const string ClearanceFooter =
            "<div style=\"display:flex;justify-content:space-between;margin-top:48px;\">" +
            "<div>_____________________________<br/>Human Resources</div>" +
            "<div>_____________________________<br/>Finance</div>" +
            "<div>_____________________________<br/>Authorized Signature</div>" +
            "</div><p style=\"margin-top:16px;font-size:12px;color:#555;\">Generated on {{Today}}.</p>";

        private const string TransferNoticeName = "Transfer Notice";

        private const string TransferNoticeBody =
            "<h2 style=\"text-align:center;margin:8px 0 16px;letter-spacing:1px;\">TRANSFER NOTICE</h2>" +
            "<p>Dear <strong>{{FullName}}</strong> (Employee No. <strong>{{EmployeeNumber}}</strong>),</p>" +
            "<p>We are pleased to confirm your <strong>{{TransferKind}}</strong> transfer, effective " +
            "<strong>{{EffectiveDate}}</strong>:</p>" +
            "<table style=\"width:100%;border-collapse:collapse;font-size:13px;margin:12px 0;\">" +
            "<tr><td style=\"border:1px solid #ccc;padding:6px 8px;background:#f2f2f2;width:30%;\">New Role</td>" +
            "<td style=\"border:1px solid #ccc;padding:6px 8px;\"><strong>{{NewPosition}}</strong></td></tr>" +
            "<tr><td style=\"border:1px solid #ccc;padding:6px 8px;background:#f2f2f2;\">Department / Unit</td>" +
            "<td style=\"border:1px solid #ccc;padding:6px 8px;\">{{NewUnit}}</td></tr>" +
            "<tr><td style=\"border:1px solid #ccc;padding:6px 8px;background:#f2f2f2;\">Location</td>" +
            "<td style=\"border:1px solid #ccc;padding:6px 8px;\">{{NewBranch}}</td></tr>" +
            "<tr><td style=\"border:1px solid #ccc;padding:6px 8px;background:#f2f2f2;\">Reporting Line</td>" +
            "<td style=\"border:1px solid #ccc;padding:6px 8px;\">{{ReportingLine}}</td></tr>" +
            "<tr><td style=\"border:1px solid #ccc;padding:6px 8px;background:#f2f2f2;\">Start Date</td>" +
            "<td style=\"border:1px solid #ccc;padding:6px 8px;\">{{EffectiveDate}}</td></tr>" +
            "</table>" +
            "<p>Your previous assignment was <strong>{{FromPosition}}</strong> ({{FromUnit}}). " +
            "Your terms of compensation remain unchanged by this transfer.</p>" +
            "<p>We wish you continued success in your new role.</p>";

        private const string TransferNoticeFooter =
            "<div style=\"display:flex;justify-content:space-between;margin-top:48px;\">" +
            "<div>_____________________________<br/>Human Resources</div>" +
            "<div>_____________________________<br/>Authorized Signature</div>" +
            "</div><p style=\"margin-top:16px;font-size:12px;color:#555;\">Generated on {{Today}}.</p>";

        private const string TrainingCertificateName = "Training Certificate";

        private const string TrainingCertificateBody =
            "<div style=\"text-align:center;border:3px double #b8860b;padding:32px 24px;\">" +
            "<h1 style=\"margin:0 0 4px;letter-spacing:3px;color:#b8860b;\">CERTIFICATE</h1>" +
            "<p style=\"margin:0 0 20px;font-size:13px;letter-spacing:2px;color:#555;\">OF COMPLETION</p>" +
            "<p style=\"margin:0;\">This certifies that</p>" +
            "<h2 style=\"margin:8px 0;\">{{FullName}}</h2>" +
            "<p style=\"margin:0 0 12px;font-size:12px;color:#555;\">Employee No. {{EmployeeNumber}}</p>" +
            "<p style=\"margin:0;\">has successfully completed</p>" +
            "<h3 style=\"margin:8px 0 16px;\">{{CourseName}}</h3>" +
            "<p style=\"margin:0 0 4px;font-size:13px;\">Assessment score: <strong>{{Score}}</strong> &nbsp;&middot;&nbsp; CPD hours: <strong>{{CpdHours}}</strong></p>" +
            "<p style=\"margin:0 0 4px;font-size:13px;\">Certificate No. <strong>{{CertificateNo}}</strong></p>" +
            "<p style=\"margin:0;font-size:13px;\">Issued {{IssuedOn}} &nbsp;&middot;&nbsp; Valid until {{ExpiresOn}}</p>" +
            "</div>";

        private const string TrainingCertificateFooter =
            "<div style=\"display:flex;justify-content:space-between;margin-top:40px;\">" +
            "<div>_____________________________<br/>Trainer: {{Trainer}}</div>" +
            "<div>_____________________________<br/>Human Resources</div>" +
            "</div><p style=\"margin-top:16px;font-size:12px;color:#555;\">Generated on {{Today}}.</p>";

        private const string ResignationAcceptanceName = "Resignation Acceptance";

        private const string ResignationAcceptanceBody =
            "<h2 style=\"text-align:center;margin:8px 0 16px;letter-spacing:1px;\">ACCEPTANCE OF RESIGNATION</h2>" +
            "<p>Dear <strong>{{FullName}}</strong> (Employee No. <strong>{{EmployeeNumber}}</strong>),</p>" +
            "<p>We acknowledge receipt of your notice dated <strong>{{NoticeDate}}</strong> and confirm the acceptance " +
            "of your resignation from the position of <strong>{{Position}}</strong> ({{Unit}}).</p>" +
            "<p>Your last working day will be <strong>{{LastWorkingDate}}</strong>. Departmental clearance and the " +
            "final settlement will proceed per company policy.</p>" +
            "<p>We thank you for your service and wish you success in your future endeavours.</p>";

        private const string TerminationNoticeName = "Termination Notice";

        private const string TerminationNoticeBody =
            "<h2 style=\"text-align:center;margin:8px 0 16px;letter-spacing:1px;\">NOTICE OF TERMINATION</h2>" +
            "<p>Dear <strong>{{FullName}}</strong> (Employee No. <strong>{{EmployeeNumber}}</strong>),</p>" +
            "<p>This letter serves as formal notice that your employment in the position of <strong>{{Position}}</strong> " +
            "({{Unit}}) will end effective <strong>{{LastWorkingDate}}</strong>.</p>" +
            "<p>Reason: {{Reason}}</p>" +
            "<p>Departmental clearance, the return of company property and the final settlement will be handled " +
            "per company policy. Please contact Human Resources with any questions.</p>";

        private const string ExitLetterFooter =
            "<div style=\"display:flex;justify-content:space-between;margin-top:48px;\">" +
            "<div>_____________________________<br/>Human Resources</div>" +
            "<div>_____________________________<br/>Authorized Signature</div>" +
            "</div><p style=\"margin-top:16px;font-size:12px;color:#555;\">Generated on {{Today}}.</p>";

        private const string SettlementLetterName = "Final Settlement Letter";

        private const string SettlementLetterBody =
            "<h2 style=\"text-align:center;margin:8px 0 16px;letter-spacing:1px;\">FINAL SETTLEMENT STATEMENT</h2>" +
            "<p>Employee: <strong>{{FullName}}</strong> (No. <strong>{{EmployeeNumber}}</strong>) &nbsp;&middot;&nbsp; " +
            "Exit type: <strong>{{TerminationType}}</strong> &nbsp;&middot;&nbsp; Last working day: <strong>{{LastWorkingDate}}</strong></p>" +
            "<div style=\"margin:12px 0;\">{{LinesTable}}</div>" +
            "<p>Total earnings: <strong>{{TotalEarnings}}</strong> &nbsp;&middot;&nbsp; Total deductions: <strong>{{TotalDeductions}}</strong> " +
            "&nbsp;&middot;&nbsp; Net settlement: <strong>{{NetAmount}}</strong></p>" +
            "<p>Status: {{SettlementStatus}} {{PaidReference}}</p>" +
            "<p>This statement represents the full and final settlement of all employment dues.</p>";

        public async Task<int> SeedAsync()
        {
            var created = 0;

            if (!await repository.GetAll().AnyAsync(t => t.Name == ClearanceCertificateName))
            {
                var template = DocumentTemplate.Create(
                    ClearanceCertificateName,
                    DocumentTemplateType.ClearanceCertificate,
                    ClearanceBody,
                    ClearanceHeader,
                    ClearanceFooter,
                    "Final clearance certificate for a terminated employee (offboarding checklist).");
                await repository.AddAsync(template);
                created++;
            }

            // HC174 — formal transfer notice: new role, location, start date and reporting line.
            if (!await repository.GetAll().AnyAsync(t => t.Name == TransferNoticeName))
            {
                var template = DocumentTemplate.Create(
                    TransferNoticeName,
                    DocumentTemplateType.TransferNotice,
                    TransferNoticeBody,
                    ClearanceHeader,   // same logo/branch letterhead
                    TransferNoticeFooter,
                    "Formal transfer notice for an employee movement (HC174).");
                await repository.AddAsync(template);
                created++;
            }

            // HC211 — exit letters: resignation acceptance + termination notice.
            if (!await repository.GetAll().AnyAsync(t => t.Name == ResignationAcceptanceName))
            {
                var template = DocumentTemplate.Create(
                    ResignationAcceptanceName,
                    DocumentTemplateType.TerminationNotice,
                    ResignationAcceptanceBody,
                    ClearanceHeader,   // same logo/branch letterhead
                    ExitLetterFooter,
                    "Formal acceptance of a voluntary exit (HC211).");
                await repository.AddAsync(template);
                created++;
            }
            if (!await repository.GetAll().AnyAsync(t => t.Name == TerminationNoticeName))
            {
                var template = DocumentTemplate.Create(
                    TerminationNoticeName,
                    DocumentTemplateType.TerminationNotice,
                    TerminationNoticeBody,
                    ClearanceHeader,
                    ExitLetterFooter,
                    "Formal notice for an involuntary exit (HC211).");
                await repository.AddAsync(template);
                created++;
            }

            // HC218 — final settlement letter with the worksheet table.
            if (!await repository.GetAll().AnyAsync(t => t.Name == SettlementLetterName))
            {
                var template = DocumentTemplate.Create(
                    SettlementLetterName,
                    DocumentTemplateType.SettlementLetter,
                    SettlementLetterBody,
                    ClearanceHeader,
                    ExitLetterFooter,
                    "Final settlement statement for an exit case (HC218).");
                await repository.AddAsync(template);
                created++;
            }

            // HC200 — digital certificate for a completed training.
            if (!await repository.GetAll().AnyAsync(t => t.Name == TrainingCertificateName))
            {
                var template = DocumentTemplate.Create(
                    TrainingCertificateName,
                    DocumentTemplateType.TrainingCertificate,
                    TrainingCertificateBody,
                    ClearanceHeader,   // same logo/branch letterhead
                    TrainingCertificateFooter,
                    "Digital certificate for a completed training program (HC200).");
                await repository.AddAsync(template);
                created++;
            }

            if (created > 0)
            {
                await repository.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} default document template(s)", created);
            }
            return created;
        }
    }
}
