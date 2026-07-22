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

namespace CyberErp.Hrms.App.Features.Core.Medical
{
    // ---- DTOs ---------------------------------------------------------------
    public class MedicalClaimAttachmentMetaDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public class MedicalClaimDto
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public Guid MedicalBeneficiaryId { get; set; }
        public string? BeneficiaryName { get; set; }
        public string BeneficiaryCategory { get; set; } = string.Empty;
        public Guid MedicalPlanId { get; set; }
        public string? MedicalPlanName { get; set; }
        public Guid? MedicalProviderId { get; set; }
        public string? ProviderName { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public DateTime SubmittedOn { get; set; }
        public decimal ClaimedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Diagnosis { get; set; }
        public string? Resolution { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentReference { get; set; }
        public List<MedicalClaimAttachmentMetaDto> Attachments { get; set; } = [];
    }

    public class ClaimAttachmentInput
    {
        public string FileName { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public string ContentBase64 { get; set; } = string.Empty;
    }

    public class SubmitMedicalClaimDto
    {
        public Guid MedicalEnrollmentId { get; set; }
        public Guid MedicalBeneficiaryId { get; set; }
        public Guid? MedicalProviderId { get; set; }
        /// <summary>HR may record a provider-sourced claim; employees are always the Employee source.</summary>
        public string Source { get; set; } = nameof(MedicalClaimSource.Employee);
        public DateTime ServiceDate { get; set; }
        public decimal ClaimedAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Diagnosis { get; set; }
        public List<ClaimAttachmentInput> Attachments { get; set; } = [];
    }

    public class SubmitMedicalClaimDtoValidator : AbstractValidator<SubmitMedicalClaimDto>
    {
        public SubmitMedicalClaimDtoValidator()
        {
            RuleFor(x => x.MedicalEnrollmentId).NotEmpty();
            RuleFor(x => x.MedicalBeneficiaryId).NotEmpty();
            RuleFor(x => x.ServiceDate).NotEmpty();
            RuleFor(x => x.ClaimedAmount).GreaterThan(0);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        }
    }

    public class ApproveMedicalClaimDto { public decimal? ApprovedAmount { get; set; } public string? Note { get; set; } }
    public class RejectMedicalClaimDto { public string Reason { get; set; } = string.Empty; }
    public class PayMedicalClaimDto { public string? Reference { get; set; } }

    public class MedicalExpenseRowDto
    {
        public string Category { get; set; } = string.Empty;
        public int ClaimCount { get; set; }
        public decimal TotalClaimed { get; set; }
        public decimal TotalApproved { get; set; }
    }
    public class MedicalExpenseReportDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<MedicalExpenseRowDto> Rows { get; set; } = [];
        public int TotalClaims { get; set; }
        public decimal GrandTotalClaimed { get; set; }
        public decimal GrandTotalApproved { get; set; }
    }

    public class AttachmentDownload { public string FileName { get; set; } = ""; public string ContentType { get; set; } = ""; public byte[] Content { get; set; } = []; }

    // ---- Interfaces ---------------------------------------------------------
    public interface ISubmitMedicalClaim { Task<Guid> SubmitAsync(SubmitMedicalClaimDto dto); }
    public interface IGetMedicalClaims { Task<PaginatedResponse<MedicalClaimDto>> GetAsync(GetAllRequest request); }
    public interface IGetMedicalClaimById { Task<MedicalClaimDto> GetAsync(Guid id); }
    public interface IDownloadMedicalClaimAttachment { Task<AttachmentDownload> GetAsync(Guid attachmentId); }
    public interface IApproveMedicalClaim { Task ApproveAsync(Guid id, ApproveMedicalClaimDto dto); }
    public interface IRejectMedicalClaim { Task RejectAsync(Guid id, string reason); }
    public interface IMarkMedicalClaimPaid { Task MarkPaidAsync(Guid id, string? reference); }
    public interface IGetMedicalExpenseReport { Task<MedicalExpenseReportDto> GetAsync(string? fromDate, string? toDate); }

    // ---- Coverage --------------------------------------------------------------
    internal static class MedicalCoverage
    {
        private const decimal PercentBase = 100m;

        /// <summary>
        /// Reimbursable amount = claimed × plan coverage %, capped at the beneficiary's remaining annual
        /// limit and never above the claimed amount. An explicit override (HR) replaces the % step.
        /// </summary>
        internal static async Task<decimal> ComputeAsync(IRepository<MedicalPlan> plans, IRepository<MedicalClaim> claims,
            MedicalClaim claim, decimal? overrideAmount)
        {
            var plan = await plans.GetAll().AsNoTracking().FirstOrDefaultAsync(p => p.Id == claim.MedicalPlanId);
            var pct = plan?.CoveragePercent ?? PercentBase;
            var amount = overrideAmount ?? Math.Round(claim.ClaimedAmount * pct / PercentBase, 2);

            if (plan?.AnnualCoverageLimit is decimal limit)
            {
                var year = claim.ServiceDate.Year;
                var already = await claims.GetAll().AsNoTracking()
                    .Where(c => c.MedicalBeneficiaryId == claim.MedicalBeneficiaryId && c.Id != claim.Id
                        && (c.Status == MedicalClaimStatus.Approved || c.Status == MedicalClaimStatus.Paid)
                        && c.ServiceDate.Year == year)
                    .SumAsync(c => c.ApprovedAmount ?? 0m);
                amount = Math.Min(amount, Math.Max(0m, limit - already));
            }
            return Math.Min(amount, claim.ClaimedAmount);
        }
    }

    // ---- Handlers -----------------------------------------------------------
    public class SubmitMedicalClaim(
        IRepository<MedicalClaim> repository,
        IRepository<MedicalClaimAttachment> attachmentRepository,
        IRepository<MedicalEnrollment> enrollmentRepository,
        IRepository<MedicalBeneficiary> beneficiaryRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility,
        IWorkflowService workflowService,
        INumberSequenceService numberSequence,
        IValidator<SubmitMedicalClaimDto> validator,
        ILogger<SubmitMedicalClaim> logger) : ISubmitMedicalClaim
    {
        private const int MaxAttachmentBytes = 5 * 1024 * 1024; // 5 MB per file

        public async Task<Guid> SubmitAsync(SubmitMedicalClaimDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var enrollment = await enrollmentRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(e => e.Id == dto.MedicalEnrollmentId)
                ?? throw new NotFoundException(nameof(MedicalEnrollment), dto.MedicalEnrollmentId.ToString());

            // Employees claim only against their own coverage; HR anyone. Provider-sourced is HR-only (no portal).
            var scope = await visibility.GetScopeAsync();
            var isSelf = scope.EmployeeId.HasValue && scope.EmployeeId.Value == enrollment.EmployeeId;
            if (!scope.IsAdmin && !isSelf)
                throw new ValidationException(nameof(dto.MedicalEnrollmentId), "You can only claim against your own coverage.");
            if (enrollment.Status != MedicalEnrollmentStatus.Active)
                throw new ValidationException(nameof(dto.MedicalEnrollmentId), "Coverage is not active on this enrollment.");

            var beneficiary = await beneficiaryRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == dto.MedicalBeneficiaryId && b.MedicalEnrollmentId == dto.MedicalEnrollmentId)
                ?? throw new ValidationException(nameof(dto.MedicalBeneficiaryId), "The beneficiary is not on this enrollment.");
            if (!beneficiary.IsActive)
                throw new ValidationException(nameof(dto.MedicalBeneficiaryId), "That beneficiary's coverage is inactive.");

            var source = scope.IsAdmin && Enum.TryParse<MedicalClaimSource>(dto.Source, true, out var src)
                ? src : MedicalClaimSource.Employee;

            var claimNumber = $"MC-{await numberSequence.NextAsync("MedicalClaim"):D5}";
            var created = MedicalClaim.Create(claimNumber, enrollment.EmployeeId, enrollment.Id, beneficiary.Id,
                beneficiary.Category, enrollment.MedicalPlanId, dto.MedicalProviderId, source,
                dto.ServiceDate, dto.ClaimedAmount, dto.Description, dto.Diagnosis, DateTime.UtcNow.Date);
            await repository.AddAsync(created);

            foreach (var a in dto.Attachments)
            {
                if (string.IsNullOrWhiteSpace(a.ContentBase64)) continue;
                byte[] bytes;
                try { bytes = Convert.FromBase64String(a.ContentBase64); }
                catch { throw new ValidationException("attachments", $"'{a.FileName}' is not valid base64 content."); }
                if (bytes.Length > MaxAttachmentBytes)
                    throw new ValidationException("attachments", $"'{a.FileName}' exceeds the 5 MB limit.");
                await attachmentRepository.AddAsync(MedicalClaimAttachment.Create(created.Id, a.FileName, a.ContentType, bytes));
            }

            await repository.SaveChangesAsync();

            var name = await employeeRepository.GetAll().Where(e => e.Id == enrollment.EmployeeId && e.Person != null)
                .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefaultAsync();
            await workflowService.StartIfDefinedAsync(WorkflowEntityTypes.MedicalClaim, created.Id, enrollment.EmployeeId,
                $"Medical claim {claimNumber} — {name}");

            logger.LogInformation("Submitted MedicalClaim {ClaimNumber} for Employee {EmployeeId}", claimNumber, enrollment.EmployeeId);
            return created.Id;
        }
    }

    public class GetMedicalClaims(
        IRepository<MedicalClaim> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetMedicalClaims
    {
        public async Task<PaginatedResponse<MedicalClaimDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                query = query.Where(c => c.EmployeeId == (scope.EmployeeId ?? Guid.Empty));  // medical = own only (privacy)

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<MedicalClaimStatus>(request.Status, true, out var st))
                query = query.Where(c => c.Status == st);
            if (request.EmployeeId.HasValue)
                query = query.Where(c => c.EmployeeId == request.EmployeeId.Value);
            if (DateTime.TryParse(request.FromDate, out var from))
                query = query.Where(c => c.ServiceDate >= from);
            if (DateTime.TryParse(request.ToDate, out var to))
                query = query.Where(c => c.ServiceDate <= to);

            var total = await query.CountAsync();
            var employees = employeeRepository.GetAll();
            var data = await query.OrderByDescending(c => c.SubmittedOn).ThenByDescending(c => c.CreatedAt)
                .Skip(skip).Take(take)
                .Select(c => new MedicalClaimDto
                {
                    Id = c.Id, ClaimNumber = c.ClaimNumber, EmployeeId = c.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == c.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    MedicalBeneficiaryId = c.MedicalBeneficiaryId, BeneficiaryCategory = c.BeneficiaryCategory.ToString(),
                    MedicalPlanId = c.MedicalPlanId, MedicalProviderId = c.MedicalProviderId,
                    Source = c.Source.ToString(), ServiceDate = c.ServiceDate, SubmittedOn = c.SubmittedOn,
                    ClaimedAmount = c.ClaimedAmount, ApprovedAmount = c.ApprovedAmount, Status = c.Status.ToString(),
                    Description = c.Description, PaidAt = c.PaidAt, PaymentReference = c.PaymentReference
                }).ToListAsync();

            return new PaginatedResponse<MedicalClaimDto> { Total = total, Data = data };
        }
    }

    public class GetMedicalClaimById(
        IRepository<MedicalClaim> repository,
        IRepository<MedicalClaimAttachment> attachmentRepository,
        IRepository<MedicalBeneficiary> beneficiaryRepository,
        IRepository<MedicalPlan> planRepository,
        IRepository<MedicalProvider> providerRepository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetMedicalClaimById
    {
        public async Task<MedicalClaimDto> GetAsync(Guid id)
        {
            var employees = employeeRepository.GetAll();
            var beneficiaries = beneficiaryRepository.GetAll();
            var plans = planRepository.GetAll();
            var providers = providerRepository.GetAll();
            var dto = await repository.GetAll().AsNoTracking().Where(c => c.Id == id)
                .Select(c => new MedicalClaimDto
                {
                    Id = c.Id, ClaimNumber = c.ClaimNumber, EmployeeId = c.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == c.EmployeeId && e.Person != null).Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName).FirstOrDefault(),
                    MedicalBeneficiaryId = c.MedicalBeneficiaryId,
                    BeneficiaryName = beneficiaries.Where(b => b.Id == c.MedicalBeneficiaryId).Select(b => b.FullName).FirstOrDefault(),
                    BeneficiaryCategory = c.BeneficiaryCategory.ToString(),
                    MedicalPlanId = c.MedicalPlanId, MedicalPlanName = plans.Where(p => p.Id == c.MedicalPlanId).Select(p => p.Name).FirstOrDefault(),
                    MedicalProviderId = c.MedicalProviderId, ProviderName = providers.Where(p => p.Id == c.MedicalProviderId).Select(p => p.Name).FirstOrDefault(),
                    Source = c.Source.ToString(), ServiceDate = c.ServiceDate, SubmittedOn = c.SubmittedOn,
                    ClaimedAmount = c.ClaimedAmount, ApprovedAmount = c.ApprovedAmount, Status = c.Status.ToString(),
                    Description = c.Description, Diagnosis = c.Diagnosis, Resolution = c.Resolution,
                    PaidAt = c.PaidAt, PaymentReference = c.PaymentReference
                }).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(MedicalClaim), id.ToString());

            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(id), "You do not have access to this claim.");

            dto.Attachments = await attachmentRepository.GetAll().AsNoTracking().Where(a => a.MedicalClaimId == id)
                .Select(a => new MedicalClaimAttachmentMetaDto { Id = a.Id, FileName = a.FileName, ContentType = a.ContentType, FileSize = a.FileSize })
                .ToListAsync();
            return dto;
        }
    }

    public class DownloadMedicalClaimAttachment(
        IRepository<MedicalClaimAttachment> attachmentRepository,
        IRepository<MedicalClaim> claimRepository,
        IPerformanceVisibilityService visibility) : IDownloadMedicalClaimAttachment
    {
        public async Task<AttachmentDownload> GetAsync(Guid attachmentId)
        {
            var att = await attachmentRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(a => a.Id == attachmentId)
                ?? throw new NotFoundException(nameof(MedicalClaimAttachment), attachmentId.ToString());
            var employeeId = await claimRepository.GetAll().Where(c => c.Id == att.MedicalClaimId).Select(c => c.EmployeeId).FirstOrDefaultAsync();
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException(nameof(attachmentId), "You do not have access to this attachment.");
            return new AttachmentDownload { FileName = att.FileName, ContentType = att.ContentType, Content = att.Content };
        }
    }

    public class ApproveMedicalClaim(
        IRepository<MedicalClaim> repository,
        IRepository<MedicalPlan> planRepository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IApproveMedicalClaim
    {
        public async Task ApproveAsync(Guid id, ApproveMedicalClaimDto dto)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can approve claims.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.MedicalClaim, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException(nameof(MedicalClaim), id.ToString());
            var amount = await MedicalCoverage.ComputeAsync(planRepository, repository, entity, dto.ApprovedAmount);
            entity.Approve(amount, dto.Note);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class RejectMedicalClaim(
        IRepository<MedicalClaim> repository,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : IRejectMedicalClaim
    {
        public async Task RejectAsync(Guid id, string reason)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can reject claims.");
            if (string.IsNullOrWhiteSpace(reason)) throw new ValidationException(nameof(reason), "A rejection reason is required.");
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.MedicalClaim, id);
            var entity = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException(nameof(MedicalClaim), id.ToString());
            if (entity.Status == MedicalClaimStatus.Paid)
                throw new ValidationException(nameof(id), "A paid claim cannot be rejected.");
            entity.Reject(reason);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class MarkMedicalClaimPaid(
        IRepository<MedicalClaim> repository,
        IPerformanceVisibilityService visibility) : IMarkMedicalClaimPaid
    {
        public async Task MarkPaidAsync(Guid id, string? reference)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException(nameof(id), "Only HR can record claim payment.");
            var entity = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException(nameof(MedicalClaim), id.ToString());
            if (entity.Status != MedicalClaimStatus.Approved)
                throw new ValidationException(nameof(id), "Only an approved claim can be marked paid.");
            entity.MarkPaid(DateTime.UtcNow.Date, reference);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
        }
    }

    public class GetMedicalExpenseReport(
        IRepository<MedicalClaim> repository,
        IPerformanceVisibilityService visibility) : IGetMedicalExpenseReport
    {
        public async Task<MedicalExpenseReportDto> GetAsync(string? fromDate, string? toDate)
        {
            if (!(await visibility.GetScopeAsync()).IsAdmin) throw new ValidationException("scope", "Only HR can view medical expense reports.");

            var query = repository.GetAll().AsNoTracking()
                .Where(c => c.Status == MedicalClaimStatus.Approved || c.Status == MedicalClaimStatus.Paid);
            DateTime? from = DateTime.TryParse(fromDate, out var f) ? f : null;
            DateTime? to = DateTime.TryParse(toDate, out var tt) ? tt : null;
            if (from.HasValue) query = query.Where(c => c.ServiceDate >= from.Value);
            if (to.HasValue) query = query.Where(c => c.ServiceDate <= to.Value);

            // Single grouped aggregate by beneficiary category (HC246).
            var grouped = await query
                .GroupBy(c => c.BeneficiaryCategory)
                .Select(g => new MedicalExpenseRowDto
                {
                    Category = g.Key.ToString(),
                    ClaimCount = g.Count(),
                    TotalClaimed = g.Sum(x => x.ClaimedAmount),
                    TotalApproved = g.Sum(x => x.ApprovedAmount ?? 0m)
                }).ToListAsync();

            return new MedicalExpenseReportDto
            {
                FromDate = from, ToDate = to,
                Rows = grouped.OrderBy(r => r.Category).ToList(),
                TotalClaims = grouped.Sum(r => r.ClaimCount),
                GrandTotalClaimed = grouped.Sum(r => r.TotalClaimed),
                GrandTotalApproved = grouped.Sum(r => r.TotalApproved)
            };
        }
    }
}
