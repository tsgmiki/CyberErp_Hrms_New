using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Guarantees
{
    // ---- DTOs ---------------------------------------------------------------
    public class EmployeeGuaranteeDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        /// <summary>A value of the global "GuaranteeType" lookup category (stored by name).</summary>
        public string Type { get; set; } = string.Empty;
        public string ExternalOrganization { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string? BeneficiaryRelationship { get; set; }
        public string? ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = nameof(GuaranteeCommitmentStatus.Active);
        public string? Remarks { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public string? ReleaseNote { get; set; }
    }

    /// <summary>Status is workflow-owned and deliberately absent (HC307).</summary>
    public class SaveEmployeeGuaranteeDto
    {
        public Guid? Id { get; set; }
        /// <summary>Optional for self-service — non-admin callers are pinned to their own employee.</summary>
        public Guid? EmployeeId { get; set; }
        /// <summary>A value of the global "GuaranteeType" lookup category (stored by name).</summary>
        public string Type { get; set; } = string.Empty;
        public string ExternalOrganization { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string? BeneficiaryRelationship { get; set; }
        public string? ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Remarks { get; set; }
    }

    public class ReleaseEmployeeGuaranteeDto
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
    }

    /// <summary>HC307 — headline chips for the interactive guarantee dashboard.</summary>
    public class GuaranteeDashboardDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int PendingApproval { get; set; }
        public int Released { get; set; }
        public int Rejected { get; set; }
        public decimal ActiveAmount { get; set; }
        /// <summary>Active commitments whose end date falls within the next 60 days.</summary>
        public int ExpiringSoon { get; set; }
    }

    // ---- Validation (HC306 — guided, validated forms) ------------------------
    public class SaveEmployeeGuaranteeDtoValidator : AbstractValidator<SaveEmployeeGuaranteeDto>
    {
        public SaveEmployeeGuaranteeDtoValidator()
        {
            // Lookup-driven (global "GuaranteeType" category) — any configured value, by name.
            RuleFor(x => x.Type).NotEmpty().MaximumLength(100)
                .WithMessage("The guarantee type is required.");
            RuleFor(x => x.ExternalOrganization).NotEmpty().MaximumLength(200)
                .WithMessage("The external organization is required.");
            RuleFor(x => x.BeneficiaryName).NotEmpty().MaximumLength(200)
                .WithMessage("The beneficiary (guaranteed person) is required.");
            RuleFor(x => x.BeneficiaryRelationship).MaximumLength(100);
            RuleFor(x => x.ReferenceNumber).MaximumLength(100);
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("The committed amount must be positive.");
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
                .When(x => x.EndDate.HasValue)
                .WithMessage("The end date must be after the start date.");
            RuleFor(x => x.Remarks).MaximumLength(2000);
        }
    }

    // ---- Interfaces ----------------------------------------------------------
    public interface ISaveEmployeeGuarantee { Task<Guid> SaveAsync(SaveEmployeeGuaranteeDto dto); }
    public interface IDeleteEmployeeGuarantee { Task DeleteAsync(Guid id); }
    public interface IGetEmployeeGuaranteeById { Task<EmployeeGuaranteeDto> GetAsync(Guid id); }
    public interface IGetAllEmployeeGuarantees
    {
        Task<PaginatedResponse<EmployeeGuaranteeDto>> GetAsync(GetAllRequest request);
        /// <summary>The caller's OWN commitments (self-service list) — even for admin-scoped users.</summary>
        Task<PaginatedResponse<EmployeeGuaranteeDto>> GetMineAsync(GetAllRequest request);
    }
    public interface IReleaseEmployeeGuarantee { Task ReleaseAsync(ReleaseEmployeeGuaranteeDto dto); }
    public interface IGetGuaranteeDashboard { Task<GuaranteeDashboardDto> GetAsync(); }

    // ---- Handlers ------------------------------------------------------------
    /// <summary>
    /// HC305/HC306 — employees record their own commitments (self-service), HR records anyone's.
    /// With an active approval chain the commitment parks as PendingApproval until the workflow
    /// decides (HC307); without one the module operates directly (engine philosophy).
    /// </summary>
    public class SaveEmployeeGuarantee(
        IRepository<EmployeeGuarantee> repository,
        IRepository<Employee> employeeRepository,
        IRepository<WorkflowDefinition> workflowDefinitions,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        IPerformanceVisibilityService visibility,
        IEndpointPermissionService permissions,
        IValidator<SaveEmployeeGuaranteeDto> validator,
        ILogger<SaveEmployeeGuarantee> logger) : ISaveEmployeeGuarantee
    {
        public async Task<Guid> SaveAsync(SaveEmployeeGuaranteeDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Employees may only file/amend their OWN commitments; HR (admin scope) manages all.
            var scope = await visibility.GetScopeAsync();
            Guid employeeId;
            if (scope.IsAdmin)
                employeeId = dto.EmployeeId ?? scope.EmployeeId
                    ?? throw new ValidationException(nameof(dto.EmployeeId), "An employee is required.");
            else
            {
                employeeId = scope.EmployeeId
                    ?? throw new ValidationException("scope", "Your account is not linked to an employee record.");
                if (dto.EmployeeId.HasValue && dto.EmployeeId.Value != employeeId)
                    throw new ValidationException(nameof(dto.EmployeeId), "You can only record your own guarantee commitments.");
            }
            if (!await employeeRepository.GetAll().AnyAsync(e => e.Id == employeeId))
                throw new NotFoundException(nameof(Employee), employeeId.ToString());

            var type = dto.Type.Trim();

            // NBE-procedure guard: one live commitment per employee + organization + beneficiary.
            var duplicate = await repository.GetAll().AnyAsync(g =>
                g.EmployeeId == employeeId
                && g.ExternalOrganization == dto.ExternalOrganization.Trim()
                && g.BeneficiaryName == dto.BeneficiaryName.Trim()
                && (g.Status == GuaranteeCommitmentStatus.Active || g.Status == GuaranteeCommitmentStatus.PendingApproval)
                && g.Id != (dto.Id ?? Guid.Empty));
            if (duplicate)
                throw new ValidationException(nameof(dto.BeneficiaryName),
                    "An active commitment for this beneficiary at this organization already exists.");

            var workflowActive = await workflowDefinitions.GetAll()
                .AnyAsync(d => d.EntityType == WorkflowEntityTypes.EmployeeGuarantee && d.IsActive);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                // No amendments while an approval is in flight.
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.EmployeeGuarantee, dto.Id.Value);

                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(EmployeeGuarantee), dto.Id.Value.ToString());
                if (!await permissions.HasAnyAsync(HrScreens.GuaranteeRegister) && entity.EmployeeId != employeeId)
                    throw new ValidationException("scope", "You can only amend your own guarantee commitments.");
                // Handler-level precheck: the domain guard would throw ArgumentException (→ 500).
                if (entity.Status == GuaranteeCommitmentStatus.Released)
                    throw new ValidationException(nameof(dto.Id), "A released commitment can no longer be amended.");

                // Approval outcomes are workflow-owned: amending a Rejected/parked commitment RESUBMITS it.
                var resubmit = workflowActive && entity.Status
                    is GuaranteeCommitmentStatus.Rejected or GuaranteeCommitmentStatus.PendingApproval;
                if (resubmit)
                    await workflowService.EnsureStartableAsync(WorkflowEntityTypes.EmployeeGuarantee, entity.EmployeeId);

                entity.Update(type, dto.ExternalOrganization, dto.BeneficiaryName, dto.BeneficiaryRelationship,
                    dto.ReferenceNumber, dto.Amount, dto.StartDate, dto.EndDate, dto.Remarks);
                if (resubmit) entity.MarkPendingApproval();
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();

                if (resubmit)
                    await workflowService.StartIfDefinedAsync(
                        WorkflowEntityTypes.EmployeeGuarantee, entity.Id, entity.EmployeeId,
                        await SummaryAsync(entity.EmployeeId, dto) + " (resubmitted)");
                return entity.Id;
            }

            if (workflowActive)
                // Fail BEFORE persisting when the chain could never complete (unresolvable approvers).
                await workflowService.EnsureStartableAsync(WorkflowEntityTypes.EmployeeGuarantee, employeeId);

            var created = EmployeeGuarantee.Create(employeeId, type, dto.ExternalOrganization, dto.BeneficiaryName,
                dto.BeneficiaryRelationship, dto.ReferenceNumber, dto.Amount, dto.StartDate, dto.EndDate, dto.Remarks);
            if (workflowActive) created.MarkPendingApproval();
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();

            if (workflowActive)
                await workflowService.StartIfDefinedAsync(
                    WorkflowEntityTypes.EmployeeGuarantee, created.Id, employeeId, await SummaryAsync(employeeId, dto));

            logger.LogInformation("Created EmployeeGuarantee {Id} for Employee {EmployeeId}{Workflow}",
                created.Id, employeeId, workflowActive ? " — submitted for approval" : string.Empty);
            return created.Id;
        }

        private async Task<string> SummaryAsync(Guid employeeId, SaveEmployeeGuaranteeDto dto)
        {
            var name = await employeeRepository.GetAll()
                .Where(e => e.Id == employeeId && e.Person != null)
                .Select(e => e.Person!.FirstName + " " + e.Person!.GrandFatherName)
                .FirstOrDefaultAsync();
            return $"Guarantee — {name} → {dto.BeneficiaryName.Trim()} @ {dto.ExternalOrganization.Trim()} — {dto.Amount:N2}";
        }
    }

    public class DeleteEmployeeGuarantee(
        IRepository<EmployeeGuarantee> repository,
        IPerformanceVisibilityService visibility,
        IEndpointPermissionService permissions,
        IWorkflowGate workflowGate,
        ILogger<DeleteEmployeeGuarantee> logger) : IDeleteEmployeeGuarantee
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.EmployeeGuarantee, id);
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeGuarantee), id.ToString());

            var scope = await visibility.GetScopeAsync();
            if (!await permissions.HasAnyAsync(HrScreens.GuaranteeRegister) && entity.EmployeeId != (scope.EmployeeId ?? Guid.Empty))
                throw new ValidationException("scope", "You can only remove your own guarantee commitments.");
            if (entity.Status == GuaranteeCommitmentStatus.Released)
                throw new ValidationException(nameof(id), "A released commitment is part of the audit trail and cannot be deleted.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeGuarantee {Id}", id);
        }
    }

    public class GetEmployeeGuaranteeById(
        IRepository<EmployeeGuarantee> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetEmployeeGuaranteeById
    {
        public async Task<EmployeeGuaranteeDto> GetAsync(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            var query = repository.GetAll().AsNoTracking().Where(g => g.Id == id);
            if (!scope.IsAdmin)
                query = query.Where(g => g.EmployeeId == (scope.EmployeeId ?? Guid.Empty)); // own only

            var dto = await GuaranteeShared.Project(query, employeeRepository).FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(EmployeeGuarantee), id.ToString());
            return dto;
        }
    }

    public class GetAllEmployeeGuarantees(
        IRepository<EmployeeGuarantee> repository,
        IRepository<Employee> employeeRepository,
        IPerformanceVisibilityService visibility) : IGetAllEmployeeGuarantees
    {
        public Task<PaginatedResponse<EmployeeGuaranteeDto>> GetAsync(GetAllRequest request) => QueryAsync(request, mine: false);
        public Task<PaginatedResponse<EmployeeGuaranteeDto>> GetMineAsync(GetAllRequest request) => QueryAsync(request, mine: true);

        private async Task<PaginatedResponse<EmployeeGuaranteeDto>> QueryAsync(GetAllRequest request, bool mine)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();
            var scope = await visibility.GetScopeAsync();
            if (mine || !scope.IsAdmin)
                query = query.Where(g => g.EmployeeId == (scope.EmployeeId ?? Guid.Empty)); // own only

            if (request.EmployeeId.HasValue)
                query = query.Where(g => g.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<GuaranteeCommitmentStatus>(request.Status, true, out var st))
                query = query.Where(g => g.Status == st);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(g => g.ExternalOrganization.Contains(term)
                    || g.BeneficiaryName.Contains(term)
                    || (g.ReferenceNumber != null && g.ReferenceNumber.Contains(term)));
            }

            var total = await query.CountAsync();
            var data = await GuaranteeShared.Project(
                    query.OrderByDescending(g => g.StartDate).Skip(skip).Take(take), employeeRepository)
                .ToListAsync();

            return new PaginatedResponse<EmployeeGuaranteeDto> { Total = total, Data = data };
        }
    }

    /// <summary>HR discharges an active commitment once the external obligation ends (HC305).</summary>
    public class ReleaseEmployeeGuarantee(
        IRepository<EmployeeGuarantee> repository,
        IPerformanceVisibilityService visibility,
        ILogger<ReleaseEmployeeGuarantee> logger) : IReleaseEmployeeGuarantee
    {
        public async Task ReleaseAsync(ReleaseEmployeeGuaranteeDto dto)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                throw new ValidationException("scope", "Only HR can release a guarantee commitment.");

            var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new NotFoundException(nameof(EmployeeGuarantee), dto.Id.ToString());
            if (entity.Status != GuaranteeCommitmentStatus.Active)
                throw new ValidationException(nameof(dto.Id), "Only an active commitment can be released.");

            entity.Release(dto.Note, DateTime.UtcNow.Date);
            repository.UpdateAsync(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Released EmployeeGuarantee {Id}", dto.Id);
        }
    }

    /// <summary>HC307 — dashboard chips; non-admin callers see their own slice only.</summary>
    public class GetGuaranteeDashboard(
        IRepository<EmployeeGuarantee> repository,
        IPerformanceVisibilityService visibility) : IGetGuaranteeDashboard
    {
        public async Task<GuaranteeDashboardDto> GetAsync()
        {
            var query = repository.GetAll().AsNoTracking();
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                query = query.Where(g => g.EmployeeId == (scope.EmployeeId ?? Guid.Empty));

            var soon = DateTime.UtcNow.Date.AddDays(60);
            var today = DateTime.UtcNow.Date;
            var stats = await query
                .GroupBy(_ => 1)
                .Select(g => new GuaranteeDashboardDto
                {
                    Total = g.Count(),
                    Active = g.Count(x => x.Status == GuaranteeCommitmentStatus.Active),
                    PendingApproval = g.Count(x => x.Status == GuaranteeCommitmentStatus.PendingApproval),
                    Released = g.Count(x => x.Status == GuaranteeCommitmentStatus.Released),
                    Rejected = g.Count(x => x.Status == GuaranteeCommitmentStatus.Rejected),
                    ActiveAmount = g.Where(x => x.Status == GuaranteeCommitmentStatus.Active).Sum(x => (decimal?)x.Amount) ?? 0,
                    ExpiringSoon = g.Count(x => x.Status == GuaranteeCommitmentStatus.Active
                        && x.EndDate != null && x.EndDate >= today && x.EndDate <= soon)
                })
                .FirstOrDefaultAsync();
            return stats ?? new GuaranteeDashboardDto();
        }
    }

    /// <summary>Applies the workflow outcome (HC307): approval puts the commitment in force.</summary>
    public class EmployeeGuaranteeWorkflowHandler(
        IRepository<EmployeeGuarantee> repository,
        ILogger<EmployeeGuaranteeWorkflowHandler> logger) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.EmployeeGuarantee, StringComparison.OrdinalIgnoreCase);

        public async Task OnApprovedAsync(string entityType, Guid entityId)
        {
            var g = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (g is null) return; // deleted mid-flight — nothing to apply
            g.ApproveViaWorkflow();
            repository.UpdateAsync(g);
            await repository.SaveChangesAsync();
            logger.LogInformation("EmployeeGuarantee {Id} approved via workflow — now Active", entityId);
        }

        public async Task OnRejectedAsync(string entityType, Guid entityId)
        {
            var g = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == entityId);
            if (g is null) return;
            g.RejectViaWorkflow();
            repository.UpdateAsync(g);
            await repository.SaveChangesAsync();
            logger.LogInformation("EmployeeGuarantee {Id} rejected via workflow", entityId);
        }
    }

    // ---- Shared projection (batch employee names — no N+1) --------------------
    internal static class GuaranteeShared
    {
        internal static IQueryable<EmployeeGuaranteeDto> Project(
            IQueryable<EmployeeGuarantee> query, IRepository<Employee> employeeRepository) =>
            from g in query
            join e in employeeRepository.GetAll().AsNoTracking() on g.EmployeeId equals e.Id into emp
            from e in emp.DefaultIfEmpty()
            select new EmployeeGuaranteeDto
            {
                Id = g.Id,
                EmployeeId = g.EmployeeId,
                EmployeeName = e != null && e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : null,
                EmployeeNumber = e != null ? e.EmployeeNumber : null,
                Type = g.Type,
                ExternalOrganization = g.ExternalOrganization,
                BeneficiaryName = g.BeneficiaryName,
                BeneficiaryRelationship = g.BeneficiaryRelationship,
                ReferenceNumber = g.ReferenceNumber,
                Amount = g.Amount,
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                Status = g.Status.ToString(),
                Remarks = g.Remarks,
                ReleasedDate = g.ReleasedDate,
                ReleaseNote = g.ReleaseNote
            };
    }
}
