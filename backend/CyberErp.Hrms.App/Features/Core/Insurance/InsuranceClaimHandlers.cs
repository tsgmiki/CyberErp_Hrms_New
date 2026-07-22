using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Insurance
{
    // ---- DTOs ---------------------------------------------------------------
    public class InsuranceClaimAttachmentMetaDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public class InsuranceClaimDto
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid InsurancePolicyId { get; set; }
        public string? PolicyNumber { get; set; }
        public string? InsurerName { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public DateTime SubmittedOn { get; set; }
        public decimal ClaimedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentReference { get; set; }
        public List<InsuranceClaimAttachmentMetaDto> Attachments { get; set; } = [];
    }

    public class InsuranceClaimAttachmentInput
    {
        public string FileName { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public string ContentBase64 { get; set; } = string.Empty;
    }

    public class SubmitInsuranceClaimDto
    {
        /// <summary>HR may claim on behalf of any employee; employees claim only for themselves.</summary>
        public Guid? EmployeeId { get; set; }
        public Guid InsurancePolicyId { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public decimal ClaimedAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<InsuranceClaimAttachmentInput> Attachments { get; set; } = [];
    }

    public class SubmitInsuranceClaimDtoValidator : AbstractValidator<SubmitInsuranceClaimDto>
    {
        public SubmitInsuranceClaimDtoValidator()
        {
            RuleFor(x => x.InsurancePolicyId).NotEmpty();
            RuleFor(x => x.ClaimType).NotEmpty().MaximumLength(100);
            RuleFor(x => x.IncidentDate).NotEmpty();
            RuleFor(x => x.ClaimedAmount).GreaterThan(0);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        }
    }

    public class ApproveInsuranceClaimDto { public decimal? ApprovedAmount { get; set; } public string? Note { get; set; } }
    public class RejectInsuranceClaimDto { public string Reason { get; set; } = string.Empty; }
    public class PayInsuranceClaimDto { public string? Reference { get; set; } }

    public class InsuranceAttachmentDownload { public string FileName { get; set; } = ""; public string ContentType { get; set; } = ""; public byte[] Content { get; set; } = []; }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISubmitInsuranceClaim { Task<Guid> SubmitAsync(SubmitInsuranceClaimDto dto); }
    public interface IGetInsuranceClaims { Task<PaginatedResponse<InsuranceClaimDto>> GetAsync(GetAllRequest request); }
    public interface IGetInsuranceClaimById { Task<InsuranceClaimDto> GetAsync(Guid id); }
    public interface IDownloadInsuranceClaimAttachment { Task<InsuranceAttachmentDownload> GetAsync(Guid attachmentId); }
    public interface IApproveInsuranceClaim { Task ApproveAsync(Guid id, ApproveInsuranceClaimDto dto); }
    public interface IRejectInsuranceClaim { Task RejectAsync(Guid id, string reason); }
    public interface IMarkInsuranceClaimPaid { Task MarkPaidAsync(Guid id, string? reference); }

    // ---- Handlers -----------------------------------------------------------
    public class SubmitInsuranceClaim(
        IRepository<InsuranceClaim> repository,
        IRepository<InsuranceClaimAttachment> attachmentRepository,
        IRepository<InsurancePolicy> policyRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        IWorkflowService workflowService,
        INumberSequenceService numberSequence,
        IValidator<SubmitInsuranceClaimDto> validator,
        ILogger<SubmitInsuranceClaim> logger) : ISubmitInsuranceClaim
    {
        private const int MaxAttachmentBytes = 5 * 1024 * 1024; // 5 MB per file

        public async Task<Guid> SubmitAsync(SubmitInsuranceClaimDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var scope = await visibility.GetScopeAsync();
            // Resolve whose claim this is: HR may set EmployeeId; employees claim only for themselves.
            Guid employeeId;
            if (scope.IsAdmin)
                employeeId = dto.EmployeeId ?? scope.EmployeeId ?? throw new ValidationException(nameof(dto.EmployeeId), "An employee is required.");
            else
            {
                employeeId = scope.EmployeeId ?? throw new ValidationException("scope", "Your account is not linked to an employee record.");
                if (dto.EmployeeId.HasValue && dto.EmployeeId.Value != employeeId)
                    throw new ValidationException(nameof(dto.EmployeeId), "You can only submit your own insurance claims.");
            }
            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == employeeId))
                throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var policy = await policyRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.InsurancePolicyId)
                ?? throw new NotFoundException(nameof(InsurancePolicy), dto.InsurancePolicyId.ToString());
            if (policy.Status != InsurancePolicyStatus.Active)
                throw new ValidationException(nameof(dto.InsurancePolicyId), "Claims can only be filed against an active policy.");

            var claimNumber = $"INS-{await numberSequence.NextAsync("InsuranceClaim"):D5}";
            var created = InsuranceClaim.Create(claimNumber, employeeId, policy.Id, dto.ClaimType,
                dto.IncidentDate, dto.ClaimedAmount, dto.Description, DateTime.UtcNow.Date);
            await repository.AddAsync(created);

            foreach (var a in dto.Attachments)
            {
                if (string.IsNullOrWhiteSpace(a.ContentBase64)) continue;
                byte[] bytes;
                try { bytes = Convert.FromBase64String(a.ContentBase64); }
                catch { throw new ValidationException("attachments", $"'{a.FileName}' is not valid base64 content."); }
                if (bytes.Length > MaxAttachmentBytes)
                    throw new ValidationException("attachments", $"'{a.FileName}' exceeds the 5 MB limit.");
                await attachmentRepository.AddAsync(InsuranceClaimAttachment.Create(created.Id, a.FileName, a.ContentType, bytes));
            }

            await repository.SaveChangesAsync();

            var name = await employeeRepository.GetAll().Where(e => e.Id == employeeId && e.Person != null)
                .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefaultAsync();
            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.InsuranceClaim, created.Id, employeeId,
                $"Insurance claim {claimNumber} — {name}");

            logger.LogInformation("Submitted InsuranceClaim {ClaimNumber} for Employee {EmployeeId}", claimNumber, employeeId);
            return created.Id;
        }
    }

    public class GetInsuranceClaims(
        IRepository<InsuranceClaim> repository,
        IRepository<Employee> employeeRepository,
        IRepository<InsurancePolicy> policyRepository,
        IPerformanceVisibilityService visibility) : IGetInsuranceClaims
    {
        public async Task<PaginatedResponse<InsuranceClaimDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                query = query.Where(c => c.EmployeeId == (scope.EmployeeId ?? Guid.Empty));  // insurance = own only (privacy)

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<InsuranceClaimStatus>(request.Status, true, out var st))
                query = query.Where(c => c.Status == st);
            if (request.EmployeeId.HasValue)
                query = query.Where(c => c.EmployeeId == request.EmployeeId.Value);
            if (DateTime.TryParse(request.FromDate, out var from))
                query = query.Where(c => c.IncidentDate >= from);
            if (DateTime.TryParse(request.ToDate, out var to))
                query = query.Where(c => c.IncidentDate <= to);

            var total = await query.CountAsync();
            var employees = employeeRepository.GetAll();
            var policies = policyRepository.GetAll();
            var data = await query.OrderByDescending(c => c.SubmittedOn).ThenByDescending(c => c.CreatedAt)
                .Skip(skip).Take(take)
                .Select(c => new InsuranceClaimDto
                {
                    Id = c.Id, ClaimNumber = c.ClaimNumber, EmployeeId = c.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == c.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    InsurancePolicyId = c.InsurancePolicyId,
                    PolicyNumber = policies.Where(p => p.Id == c.InsurancePolicyId).Select(p => p.PolicyNumber).FirstOrDefault(),
                    InsurerName = policies.Where(p => p.Id == c.InsurancePolicyId).Select(p => p.InsurerName).FirstOrDefault(),
                    ClaimType = c.ClaimType, IncidentDate = c.IncidentDate, SubmittedOn = c.SubmittedOn,
                    ClaimedAmount = c.ClaimedAmount, ApprovedAmount = c.ApprovedAmount, Status = c.Status.ToString(),
                    Description = c.Description, PaidAt = c.PaidAt, PaymentReference = c.PaymentReference
                }).ToListAsync();

            return new PaginatedResponse<InsuranceClaimDto> { Total = total, Data = data };
        }
    }

    public class GetInsuranceClaimById(
        IRepository<InsuranceClaim> repository,
        IRepository<InsuranceClaimAttachment> attachmentRepository,
        IRepository<InsurancePolicy> policyRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetInsuranceClaimById
    {
        public async Task<InsuranceClaimDto> GetAsync(Guid id)
        {
            var employees = employeeRepository.GetAll();
            var policies = policyRepository.GetAll();
            var dto = await repository.GetAll().AsNoTracking().Where(c => c.Id == id)
                .Select(c => new InsuranceClaimDto
                {
                    Id = c.Id, ClaimNumber = c.ClaimNumber, EmployeeId = c.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == c.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    InsurancePolicyId = c.InsurancePolicyId,
                    PolicyNumber = policies.Where(p => p.Id == c.InsurancePolicyId).Select(p => p.PolicyNumber).FirstOrDefault(),
                    InsurerName = policies.Where(p => p.Id == c.InsurancePolicyId).Select(p => p.InsurerName).FirstOrDefault(),
                    ClaimType = c.ClaimType, IncidentDate = c.IncidentDate, SubmittedOn = c.SubmittedOn,
                    ClaimedAmount = c.ClaimedAmount, ApprovedAmount = c.ApprovedAmount, Status = c.Status.ToString(),
                    Description = c.Description, Resolution = c.Resolution, PaidAt = c.PaidAt, PaymentReference = c.PaymentReference
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(InsuranceClaim), id.ToString());

            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(id), "You do not have access to this claim.");

            dto.Attachments = await attachmentRepository.GetAll().AsNoTracking().Where(a => a.InsuranceClaimId == id)
                .Select(a => new InsuranceClaimAttachmentMetaDto { Id = a.Id, FileName = a.FileName, ContentType = a.ContentType, FileSize = a.FileSize })
                .ToListAsync();
            return dto;
        }
    }

    public class DownloadInsuranceClaimAttachment(
        IRepository<InsuranceClaimAttachment> attachmentRepository,
        IRepository<InsuranceClaim> claimRepository,
        IPerformanceVisibilityService visibility) : IDownloadInsuranceClaimAttachment
    {
        public async Task<InsuranceAttachmentDownload> GetAsync(Guid attachmentId)
        {
            var att = await attachmentRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(a => a.Id == attachmentId)
                ?? throw new NotFoundException(nameof(InsuranceClaimAttachment), attachmentId.ToString());
            var employeeId = await claimRepository.GetAll().Where(c => c.Id == att.InsuranceClaimId).Select(c => c.EmployeeId).FirstOrDefaultAsync();
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(attachmentId), "You do not have access to this attachment.");
            return new InsuranceAttachmentDownload { FileName = att.FileName, ContentType = att.ContentType, Content = att.Content };
        }
    }

    public class ApproveInsuranceClaim(
        IRepository<InsuranceClaim> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IApproveInsuranceClaim
    {
        public async Task ApproveAsync(Guid id, ApproveInsuranceClaimDto dto)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can approve claims.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.InsuranceClaim, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException(nameof(InsuranceClaim), id.ToString());
            // Default to the full claimed amount; HR may approve a lower amount.
            var amount = dto.ApprovedAmount ?? entity.ClaimedAmount;
            entity.Approve(amount, dto.Note);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class RejectInsuranceClaim(
        IRepository<InsuranceClaim> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IRejectInsuranceClaim
    {
        public async Task RejectAsync(Guid id, string reason)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can reject claims.");
            if (string.IsNullOrWhiteSpace(reason)) throw new ValidationException(nameof(reason), "A rejection reason is required.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.InsuranceClaim, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException(nameof(InsuranceClaim), id.ToString());
            if (entity.Status == InsuranceClaimStatus.Paid)
                throw new ValidationException(nameof(id), "A paid claim cannot be rejected.");
            entity.Reject(reason);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class MarkInsuranceClaimPaid(
        IRepository<InsuranceClaim> repository,
        IPerformanceVisibilityService visibility) : IMarkInsuranceClaimPaid
    {
        public async Task MarkPaidAsync(Guid id, string? reference)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can record claim payment.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException(nameof(InsuranceClaim), id.ToString());
            if (entity.Status != InsuranceClaimStatus.Approved)
                throw new ValidationException(nameof(id), "Only an approved claim can be marked paid.");
            entity.MarkPaid(DateTime.UtcNow.Date, reference);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }
}
