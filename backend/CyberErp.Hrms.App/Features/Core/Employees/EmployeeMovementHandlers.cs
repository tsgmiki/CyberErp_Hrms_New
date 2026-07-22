using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.EmployeeFields;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Employees
{
    // ---- DTOs ---------------------------------------------------------------
    public class EmployeeMovementDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }

        public Guid? FromPositionId { get; set; }
        public string? FromPositionName { get; set; }
        public Guid? FromSalaryScaleId { get; set; }
        public string? FromSalaryScaleName { get; set; }
        public decimal? FromSalary { get; set; }
        public string? FromBranchName { get; set; }

        public Guid? ToPositionId { get; set; }
        public string? ToPositionName { get; set; }
        public Guid? ToSalaryScaleId { get; set; }
        public string? ToSalaryScaleName { get; set; }
        public decimal? ToSalary { get; set; }
        public string? ToBranchName { get; set; }

        public string? Reason { get; set; }
        public string? Remark { get; set; }
        public DateTime? ExecutedAt { get; set; }
        // Transfer-request details (HC170/171/173).
        public string? TransferKind { get; set; }
        public Guid? RequestedByEmployeeId { get; set; }
        public string? RequestedByName { get; set; }
        public decimal? RelocationExpense { get; set; }
        /// <summary>Set on the standalone paged list (per-employee history resolves it from context).</summary>
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        /// <summary>Values of this form's dynamic custom fields (HC021), keyed by field name.</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveEmployeeMovementDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string MovementType { get; set; } = nameof(Dom.Entities.Core.MovementType.Transfer);
        public DateTime EffectiveDate { get; set; }
        public Guid? ToPositionId { get; set; }
        public Guid? ToSalaryScaleId { get; set; }
        public decimal? ToSalary { get; set; }
        public string? Reason { get; set; }
        public string? Remark { get; set; }
        /// <summary>What the transfer changes: Role | Department | Location (HC171, Transfer only).</summary>
        public string? TransferKind { get; set; }
        /// <summary>Estimated relocation cost for budget-impact tracking (HC173, Transfer only).</summary>
        public decimal? RelocationExpense { get; set; }
        /// <summary>Submitted values for this form's dynamic custom fields (HC021).</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveEmployeeMovementDtoValidator : AbstractValidator<SaveEmployeeMovementDto>
    {
        private static bool IsTransfer(SaveEmployeeMovementDto x) =>
            string.Equals(x.MovementType, nameof(Dom.Entities.Core.MovementType.Transfer), StringComparison.OrdinalIgnoreCase);

        public SaveEmployeeMovementDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.EffectiveDate).NotEmpty().WithMessage("Effective date is required.");
            RuleFor(x => x.MovementType).NotEmpty()
                .Must(v => Enum.TryParse<MovementType>(v, true, out _))
                .WithMessage("MovementType must be one of: Transfer, Promotion, Demotion.");
            RuleFor(x => x.ToPositionId).NotEmpty()
                .When(IsTransfer)
                .WithMessage("A transfer requires a target position.");
            // Salary rule: a Transfer may not change the salary or pay scale.
            RuleFor(x => x.ToSalary).Empty().When(IsTransfer)
                .WithMessage("A transfer cannot change the salary — record a Promotion or Demotion for pay changes.");
            RuleFor(x => x.ToSalaryScaleId).Empty().When(IsTransfer)
                .WithMessage("A transfer cannot change the salary scale — record a Promotion or Demotion for pay changes.");
            RuleFor(x => x)
                .Must(x => x.ToPositionId.HasValue || x.ToSalaryScaleId.HasValue || x.ToSalary.HasValue)
                .When(x => !IsTransfer(x))
                .WithMessage("A promotion/demotion must change at least the position, salary scale or salary.")
                .OverridePropertyName("toSalaryScaleId");
            RuleFor(x => x.ToSalary).GreaterThanOrEqualTo(0).When(x => x.ToSalary.HasValue);
            // Transfer-request details (HC171/HC173) apply to transfers only.
            RuleFor(x => x.TransferKind)
                .Must(v => Enum.TryParse<TransferKind>(v, true, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.TransferKind))
                .WithMessage("TransferKind must be one of: Role, Department, Location.");
            RuleFor(x => x.TransferKind).Empty().When(x => !IsTransfer(x))
                .WithMessage("Transfer kind applies to transfers only.");
            RuleFor(x => x.RelocationExpense).Empty().When(x => !IsTransfer(x))
                .WithMessage("Relocation expense applies to transfers only.");
            RuleFor(x => x.RelocationExpense).GreaterThanOrEqualTo(0).When(x => x.RelocationExpense.HasValue);
        }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface ISaveEmployeeMovement { Task<Guid> SaveAsync(SaveEmployeeMovementDto dto); }
    public interface IGetEmployeeMovements { Task<List<EmployeeMovementDto>> GetAsync(Guid employeeId); }
    public interface IGetAllEmployeeMovements { Task<Common.DTOs.PaginatedResponse<EmployeeMovementDto>> GetAsync(Common.DTOs.GetAllRequest request); }
    public interface IGetEmployeeMovementById { Task<EmployeeMovementDto> GetAsync(Guid id); }
    public interface IExecuteEmployeeMovement { Task ExecuteAsync(Guid id); }
    /// <summary>Workflow-approval outcome: execute now, or park as Approved until the effective date (HC176).</summary>
    public interface IApproveEmployeeMovement { Task ApproveAsync(Guid id); }
    /// <summary>Scheduler entry point: executes every Approved movement whose effective date has arrived.</summary>
    public interface IExecuteDueMovements { Task<int> ExecuteAsync(); }
    public interface ICancelEmployeeMovement { Task CancelAsync(Guid id); }
    public interface IDeleteEmployeeMovement { Task DeleteAsync(Guid id); }

    internal static class MovementShared
    {
        /// <summary>The target position must exist and be open — unless the employee already holds it.</summary>
        internal static async Task EnsureTargetPositionAvailableAsync(
            IRepository<Position> positions, Guid targetPositionId, Guid? currentPositionId)
        {
            var pos = await positions.GetAll()
                .Where(p => p.Id == targetPositionId)
                .Select(p => new { p.IsVacant })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Position), targetPositionId.ToString());

            if (!pos.IsVacant && targetPositionId != currentPositionId)
                throw new ValidationException("toPositionId", "The target position is already occupied.");
        }
    }

    // ---- Save (create / correct while pending) ---------------------------------
    public class SaveEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<SalaryScale> salaryScaleRepository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        Performance.IPerformanceVisibilityService visibility,
        IDisciplinaryEligibilityService disciplineEligibility,
        IMovementNotifier notifier,
        ICustomFieldService customFields,
        IValidator<SaveEmployeeMovementDto> validator,
        ILogger<SaveEmployeeMovement> logger) : ISaveEmployeeMovement
    {
        public async Task<Guid> SaveAsync(SaveEmployeeMovementDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Initiation gates (HC170): a TRANSFER may be requested by the employee themselves, their
            // manager (unit subtree), or HR; a Promotion/Demotion is manager/HR only — never self-service.
            var scope = await visibility.GetScopeAsync();
            var isTransferType = string.Equals(dto.MovementType, nameof(MovementType.Transfer), StringComparison.OrdinalIgnoreCase);
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException(nameof(dto.EmployeeId),
                    "You can only record personnel actions for yourself or for employees in your unit.");
            if (!isTransferType && !scope.IsAdmin && scope.EmployeeId == dto.EmployeeId)
                throw new ValidationException(nameof(dto.MovementType),
                    "A promotion or demotion cannot be self-initiated — it is recorded by the manager or HR.");

            var employee = await employeeRepository.GetAll()
                .Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.PositionId, e.Salary, e.BranchId, e.EmployeeNumber,
                    // The From-snapshot freezes the employee's current pay point (salary scale).
                    FromSalaryScaleId = e.SalaryScaleId,
                    FirstName = e.Person != null ? e.Person.FirstName : string.Empty,
                    LastName = e.Person != null ? e.Person.GrandFatherName : string.Empty
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            if (dto.ToPositionId.HasValue)
                await MovementShared.EnsureTargetPositionAvailableAsync(positionRepository, dto.ToPositionId.Value, employee.PositionId);
            if (dto.ToSalaryScaleId.HasValue && !await salaryScaleRepository.GetAll().AnyAsync(s => s.Id == dto.ToSalaryScaleId.Value))
                throw new NotFoundException(nameof(SalaryScale), dto.ToSalaryScaleId.Value.ToString());

            var type = Enum.Parse<MovementType>(dto.MovementType, true);
            var transferKind = string.IsNullOrWhiteSpace(dto.TransferKind)
                ? (TransferKind?)null
                : Enum.Parse<TransferKind>(dto.TransferKind, true);

            // HC224/HC225 — an active disciplinary measure flagged AffectsPromotion blocks a promotion
            // (opt-in hard block; transfers/demotions are unaffected).
            if (type == MovementType.Promotion)
                await disciplineEligibility.EnsureEligibleForPromotionAsync(dto.EmployeeId);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.EmployeeId == dto.EmployeeId)
                    ?? throw new NotFoundException(nameof(EmployeeMovement), dto.Id.Value.ToString());
                if (entity.Status != MovementStatus.Pending)
                    throw new ValidationException("status", "Only pending movements can be edited.");
                await workflowGate.EnsureNoRunningAsync("EmployeeMovement", entity.Id);

                entity.Update(type, dto.EffectiveDate, dto.ToPositionId, dto.ToSalaryScaleId, dto.ToSalary,
                    dto.Reason, dto.Remark, transferKind, dto.RelocationExpense);
                repository.UpdateAsync(entity);
                await customFields.ApplyAsync(EmployeeFieldOwnerType.Movement, entity.Id, dto.CustomFields);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeMovement {Id}", entity.Id);
                return entity.Id;
            }

            // The From* snapshot freezes the employee's placement at recording time; RequestedBy stamps
            // the initiator (employee self-service / manager / HR) for the audit trail (HC170).
            var created = EmployeeMovement.Create(
                dto.EmployeeId, type, dto.EffectiveDate,
                employee.PositionId, employee.FromSalaryScaleId, employee.Salary, employee.BranchId,
                dto.ToPositionId, dto.ToSalaryScaleId, dto.ToSalary,
                dto.Reason, dto.Remark,
                transferKind, scope.EmployeeId, dto.RelocationExpense);
            await repository.AddAsync(created);
            await customFields.ApplyAsync(EmployeeFieldOwnerType.Movement, created.Id, dto.CustomFields);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeMovement {Id} ({Type}) for Employee {EmployeeId}", created.Id, type, dto.EmployeeId);

            // Route the action through its approval chain when one is configured (e.g.
            // "EmployeeMovement.Transfer"); without a definition the module operates directly.
            await workflowService.StartIfDefinedAsync(
                $"EmployeeMovement.{type}", created.Id, dto.EmployeeId,
                $"{type} — {employee.FirstName} {employee.LastName} ({employee.EmployeeNumber})");

            // HC175 — best-effort stakeholder notification (never blocks; e-mail is queued/off-switchable).
            await notifier.SubmittedAsync(created.Id);

            return created.Id;
        }
    }

    // ---- Execute (apply to the employee master) ---------------------------------
    public class ExecuteEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<EmployeeExperience> experienceRepository,
        IRepository<CompanyProfile> companyRepository,
        IWorkflowGate workflowGate,
        IMovementNotifier notifier,
        ILogger<ExecuteEmployeeMovement> logger) : IExecuteEmployeeMovement
    {
        public async Task ExecuteAsync(Guid id)
        {
            // While an approval is running, execution only happens through the workflow's final
            // approval (which completes the instance before invoking this handler).
            await workflowGate.EnsureNoRunningAsync("EmployeeMovement", id);

            var movement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeMovement), id.ToString());
            if (movement.Status is not (MovementStatus.Pending or MovementStatus.Approved))
                throw new ValidationException("status", $"A {movement.Status} movement can no longer be executed.");

            // Tracked + branch-filtered: a branch admin can only execute for employees they see.
            var employee = await employeeRepository.GetAll().FirstOrDefaultAsync(e => e.Id == movement.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), movement.EmployeeId.ToString());

            var oldPositionId = employee.PositionId;
            var changePosition = movement.ToPositionId.HasValue && movement.ToPositionId != employee.PositionId;
            Guid? newBranchId = employee.BranchId;

            if (changePosition)
            {
                await MovementShared.EnsureTargetPositionAvailableAsync(positionRepository, movement.ToPositionId!.Value, employee.PositionId);

                // The branch follows the new position (same rule as direct assignment).
                newBranchId = await positionRepository.GetAll()
                    .Where(p => p.Id == movement.ToPositionId.Value)
                    .Select(p => p.BranchId)
                    .FirstOrDefaultAsync();
            }

            // Applies the target position/branch and, for a promotion/demotion, the new pay point
            // (salary scale — the grade derives from it) and salary.
            employee.ApplyMovement(changePosition, movement.ToPositionId, newBranchId, movement.ToSalary, movement.ToSalaryScaleId);
            employeeRepository.UpdateAsync(employee);

            if (changePosition)
            {
                await EmployeeShared.RecomputePositionVacancyAsync(oldPositionId, employee.Id, positionRepository, employeeRepository);
                await EmployeeShared.MarkPositionOccupiedAsync(movement.ToPositionId, positionRepository);
            }

            // Experience tracking: executing a movement automatically records the role the employee
            // is LEAVING as an INTERNAL experience entry (IsExternal = false) on their person record.
            await RegisterInternalExperienceAsync(movement, employee);

            movement.MarkExecuted(newBranchId);
            repository.UpdateAsync(movement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Executed EmployeeMovement {Id} ({Type}) for Employee {EmployeeId}",
                movement.Id, movement.MovementType, movement.EmployeeId);
            await notifier.ExecutedAsync(movement.Id);   // HC175/HC176 — best-effort
        }

        /// <summary>
        /// Auto-registers the FROM role as internal experience — organization = the company name,
        /// job title = the prior position, ending on the movement's effective date. Robust with
        /// fallbacks so it never fails the execution; the row is added to the same unit of work.
        /// </summary>
        private async Task RegisterInternalExperienceAsync(EmployeeMovement movement, Employee employee)
        {
            if (employee.PersonId == Guid.Empty) return;

            var fromTitle = movement.FromPositionId.HasValue
                ? await positionRepository.GetAllWithoutTenantFilter()
                    .Where(p => p.Id == movement.FromPositionId.Value)
                    .Select(p => p.PositionClass != null ? p.PositionClass.Title : p.Code)
                    .FirstOrDefaultAsync()
                : null;
            var companyName = await companyRepository.GetAll()
                .Select(c => c.CompanyName)
                .FirstOrDefaultAsync();
            // Start of the from-role = the previous completed movement's effective date, else hire date.
            var priorEffective = await repository.GetAll()
                .Where(m => m.EmployeeId == movement.EmployeeId && m.Status == MovementStatus.Completed
                            && m.Id != movement.Id && m.EffectiveDate <= movement.EffectiveDate)
                .OrderByDescending(m => m.EffectiveDate)
                .Select(m => (DateTime?)m.EffectiveDate)
                .FirstOrDefaultAsync();

            var experience = EmployeeExperience.Create(
                employee.PersonId,
                organization: string.IsNullOrWhiteSpace(companyName) ? "Internal" : companyName!,
                jobTitle: string.IsNullOrWhiteSpace(fromTitle) ? "Employee" : fromTitle!,
                startDate: priorEffective ?? employee.HireDate,
                endDate: movement.EffectiveDate,
                responsibilities: $"Internal {movement.MovementType} recorded on {movement.EffectiveDate:yyyy-MM-dd}.",
                isExternal: false,
                isGovernmental: false);
            await experienceRepository.AddAsync(experience);
            // Background-safe: the due-movement scheduler runs OUTSIDE a request (no tenant context), so
            // the repository can't stamp the tenant — copy it from the movement row explicitly.
            if (string.IsNullOrEmpty(experience.TenantId))
                experience.TenantId = movement.TenantId;
        }
    }

    // ---- Approve (workflow outcome: execute now or park until the effective date, HC176) ----
    public class ApproveEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IExecuteEmployeeMovement executeHandler,
        IMovementNotifier notifier,
        ILogger<ApproveEmployeeMovement> logger) : IApproveEmployeeMovement
    {
        public async Task ApproveAsync(Guid id)
        {
            var movement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeMovement), id.ToString());

            // Due today (or overdue) → apply immediately; future-dated → park as Approved and let the
            // daily scheduler execute it on the effective date (HC176: records change ON the date).
            if (movement.EffectiveDate.Date <= DateTime.UtcNow.Date)
            {
                await executeHandler.ExecuteAsync(id);
                return;
            }

            movement.MarkApproved();
            repository.UpdateAsync(movement);
            await repository.SaveChangesAsync();
            logger.LogInformation("EmployeeMovement {Id} approved — parked until its effective date {Date:yyyy-MM-dd}",
                id, movement.EffectiveDate);
            await notifier.ApprovedAsync(id);   // HC175 — best-effort
        }
    }

    // ---- Due-runner (Hangfire daily job + admin trigger) ------------------------
    /// <summary>
    /// Executes every Approved movement whose effective date has arrived. Runs as a background job
    /// with NO tenant context — the repository's tenant filter is inactive there by design, so ONE
    /// sweep covers all tenants (each row already carries its TenantId; created child rows are
    /// stamped from the movement). One indexed query on (Status, EffectiveDate); per-row isolation so
    /// a single bad movement never blocks the rest.
    /// </summary>
    public class ExecuteDueMovements(
        IRepository<EmployeeMovement> repository,
        IExecuteEmployeeMovement executeHandler,
        ILogger<ExecuteDueMovements> logger) : IExecuteDueMovements
    {
        public async Task<int> ExecuteAsync()
        {
            var today = DateTime.UtcNow.Date;
            var dueIds = await repository.GetAll()
                .Where(m => m.Status == MovementStatus.Approved && m.EffectiveDate <= today)
                .OrderBy(m => m.EffectiveDate)
                .Select(m => m.Id)
                .ToListAsync();

            var executed = 0;
            foreach (var id in dueIds)
            {
                try
                {
                    await executeHandler.ExecuteAsync(id);
                    executed++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Due-movement execution failed for {Id} — continuing with the rest.", id);
                }
            }
            if (dueIds.Count > 0)
                logger.LogInformation("Due-movement sweep: {Executed}/{Due} executed.", executed, dueIds.Count);
            return executed;
        }
    }

    // ---- Cancel -----------------------------------------------------------------
    public class CancelEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        IMovementNotifier notifier,
        ILogger<CancelEmployeeMovement> logger) : ICancelEmployeeMovement
    {
        public async Task CancelAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync("EmployeeMovement", id);

            var movement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeMovement), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, movement.EmployeeId);
            if (movement.Status is not (MovementStatus.Pending or MovementStatus.Approved))
                throw new ValidationException("status", $"A {movement.Status} movement can no longer be cancelled.");

            movement.Cancel();
            repository.UpdateAsync(movement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cancelled EmployeeMovement {Id}", id);
            await notifier.CancelledAsync(id);   // HC172 — the employee is notified on termination
        }
    }

    // ---- Delete (drafts only — executed history is immutable) --------------------
    public class DeleteEmployeeMovement(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        ICustomFieldService customFields,
        ILogger<DeleteEmployeeMovement> logger) : IDeleteEmployeeMovement
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync("EmployeeMovement", id);

            var movement = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeMovement), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, movement.EmployeeId);
            if (movement.Status == MovementStatus.Completed)
                throw new ValidationException("status", "Executed movements are part of the employee's history and cannot be deleted.");

            await customFields.DeleteForOwnerAsync(EmployeeFieldOwnerType.Movement, id);
            repository.Delete(movement);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeMovement {Id}", id);
        }
    }

    // ---- Get (movement history per employee) --------------------------------------
    public class GetEmployeeMovements(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<SalaryScale> salaryScaleRepository,
        IRepository<Branch> branchRepository,
        Performance.IPerformanceVisibilityService visibility,
        ICustomFieldService customFields) : IGetEmployeeMovements
    {
        public async Task<List<EmployeeMovementDto>> GetAsync(Guid employeeId)
        {
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, employeeId);
            // Same visibility rule as the by-id read: the employee themselves, their manager, or HR.
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException("access", "You do not have access to this employee's movements.");

            // Name lookups use the unfiltered sets: display-by-id is safe and keeps historical
            // cross-branch snapshots readable (same approach as RelatedEmployeeName).
            var positions = positionRepository.GetAllWithoutTenantFilter();
            // The scale display = "<grade> / <step>" (its pay point identity).
            var scales = salaryScaleRepository.GetAllWithoutTenantFilter()
                .Select(s => new { s.Id, Name = (s.JobGrade != null ? s.JobGrade.Name : "") + (s.Step != null ? " / " + s.Step.Name : "") });
            var branches = branchRepository.GetAllWithoutTenantFilter();

            var list = await repository.GetAll()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.EffectiveDate)
                .Select(x => new EmployeeMovementDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    MovementType = x.MovementType.ToString(),
                    Status = x.Status.ToString(),
                    EffectiveDate = x.EffectiveDate,
                    FromPositionId = x.FromPositionId,
                    FromPositionName = positions.Where(p => p.Id == x.FromPositionId)
                        .Select(p => p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : "")).FirstOrDefault(),
                    FromSalaryScaleId = x.FromSalaryScaleId,
                    FromSalaryScaleName = scales.Where(s => s.Id == x.FromSalaryScaleId).Select(s => s.Name).FirstOrDefault(),
                    FromSalary = x.FromSalary,
                    FromBranchName = branches.Where(b => b.Id == x.FromBranchId).Select(b => b.Name).FirstOrDefault(),
                    ToPositionId = x.ToPositionId,
                    ToPositionName = positions.Where(p => p.Id == x.ToPositionId)
                        .Select(p => p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : "")).FirstOrDefault(),
                    ToSalaryScaleId = x.ToSalaryScaleId,
                    ToSalaryScaleName = scales.Where(s => s.Id == x.ToSalaryScaleId).Select(s => s.Name).FirstOrDefault(),
                    ToSalary = x.ToSalary,
                    ToBranchName = branches.Where(b => b.Id == x.ToBranchId).Select(b => b.Name).FirstOrDefault(),
                    Reason = x.Reason,
                    Remark = x.Remark,
                    ExecutedAt = x.ExecutedAt,
                    TransferKind = x.TransferKind != null ? x.TransferKind.ToString() : null,
                    RequestedByEmployeeId = x.RequestedByEmployeeId,
                    RelocationExpense = x.RelocationExpense
                })
                .ToListAsync();

            var byOwner = await customFields.GetValuesForOwnersAsync(
                EmployeeFieldOwnerType.Movement, list.Select(x => x.Id).ToList());
            foreach (var item in list)
                item.CustomFields = byOwner.TryGetValue(item.Id, out var m) ? m : new();

            return list;
        }
    }

    // ---- Get by id (the Transfer Requests form) -----------------------------------
    public class GetEmployeeMovementById(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        Performance.IPerformanceVisibilityService visibility,
        ICustomFieldService customFields) : IGetEmployeeMovementById
    {
        public async Task<EmployeeMovementDto> GetAsync(Guid id)
        {
            var x = await repository.GetAll().AsNoTracking().FirstOrDefaultAsync(m => m.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeMovement), id.ToString());
            // Same visibility rule as the list: the employee themselves, their manager, or HR.
            if (!await visibility.CanAccessEmployeeAsync(x.EmployeeId))
                throw new ValidationException("access", "You do not have access to this movement.");

            var employees = employeeRepository.GetAll();
            var positions = positionRepository.GetAllWithoutTenantFilter();
            var dto = new EmployeeMovementDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                EmployeeName = await employees.Where(e => e.Id == x.EmployeeId)
                    .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefaultAsync(),
                EmployeeNumber = await employees.Where(e => e.Id == x.EmployeeId).Select(e => e.EmployeeNumber).FirstOrDefaultAsync(),
                MovementType = x.MovementType.ToString(),
                Status = x.Status.ToString(),
                EffectiveDate = x.EffectiveDate,
                FromPositionId = x.FromPositionId,
                FromPositionName = await positions.Where(p => p.Id == x.FromPositionId)
                    .Select(p => p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : "")).FirstOrDefaultAsync(),
                ToPositionId = x.ToPositionId,
                ToPositionName = await positions.Where(p => p.Id == x.ToPositionId)
                    .Select(p => p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : "")).FirstOrDefaultAsync(),
                Reason = x.Reason,
                Remark = x.Remark,
                ExecutedAt = x.ExecutedAt,
                TransferKind = x.TransferKind?.ToString(),
                RequestedByEmployeeId = x.RequestedByEmployeeId,
                RelocationExpense = x.RelocationExpense
            };
            dto.CustomFields = await customFields.GetValuesAsync(EmployeeFieldOwnerType.Movement, id);
            return dto;
        }
    }

    // ---- Get all (standalone paged list, role-scoped: admin all / manager subtree / employee own) ----
    public class GetAllEmployeeMovements(
        IRepository<EmployeeMovement> repository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        Performance.IPerformanceVisibilityService visibility) : IGetAllEmployeeMovements
    {
        public async Task<Common.DTOs.PaginatedResponse<EmployeeMovementDto>> GetAsync(Common.DTOs.GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll().AsNoTracking();

            // Role-based visibility as a single SQL predicate (same model as appraisals/goals).
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
            {
                var myEmp = scope.EmployeeId ?? Guid.Empty;
                if (scope.IsManager)
                {
                    var unitIds = scope.UnitIds;
                    var emps = employeeRepository.GetAll();
                    query = query.Where(m => m.EmployeeId == myEmp ||
                        emps.Any(e => e.Id == m.EmployeeId && e.Position != null && unitIds.Contains(e.Position.OrganizationUnitId)));
                }
                else
                {
                    query = query.Where(m => m.EmployeeId == myEmp);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.MovementType) && Enum.TryParse<MovementType>(request.MovementType, true, out var mt))
                query = query.Where(m => m.MovementType == mt);
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<MovementStatus>(request.Status, true, out var st))
                query = query.Where(m => m.Status == st);
            if (request.EmployeeId.HasValue)
                query = query.Where(m => m.EmployeeId == request.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                var emps = employeeRepository.GetAll();
                query = query.Where(m => emps.Any(e => e.Id == m.EmployeeId &&
                    (e.EmployeeNumber.Contains(term) ||
                     (e.Person != null && (e.Person.FirstName.Contains(term) || e.Person.GrandFatherName.Contains(term))))));
            }

            var total = await query.CountAsync();

            // Display names by id from unfiltered sets (historical snapshots stay readable — same as history).
            var employees = employeeRepository.GetAll();
            var positions = positionRepository.GetAllWithoutTenantFilter();
            var data = await query
                .OrderByDescending(m => m.EffectiveDate).ThenByDescending(m => m.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new EmployeeMovementDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = employees.Where(e => e.Id == x.EmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefault(),
                    EmployeeNumber = employees.Where(e => e.Id == x.EmployeeId).Select(e => e.EmployeeNumber).FirstOrDefault(),
                    MovementType = x.MovementType.ToString(),
                    Status = x.Status.ToString(),
                    EffectiveDate = x.EffectiveDate,
                    FromPositionId = x.FromPositionId,
                    FromPositionName = positions.Where(p => p.Id == x.FromPositionId)
                        .Select(p => p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : "")).FirstOrDefault(),
                    ToPositionId = x.ToPositionId,
                    ToPositionName = positions.Where(p => p.Id == x.ToPositionId)
                        .Select(p => p.Code + (p.PositionClass != null ? " — " + p.PositionClass.Title : "")).FirstOrDefault(),
                    Reason = x.Reason,
                    Remark = x.Remark,
                    ExecutedAt = x.ExecutedAt,
                    TransferKind = x.TransferKind != null ? x.TransferKind.ToString() : null,
                    RequestedByEmployeeId = x.RequestedByEmployeeId,
                    RequestedByName = employees.Where(e => e.Id == x.RequestedByEmployeeId)
                        .Select(e => e.Person != null ? e.Person.FirstName + " " + e.Person.GrandFatherName : "").FirstOrDefault(),
                    RelocationExpense = x.RelocationExpense
                })
                .ToListAsync();

            return new Common.DTOs.PaginatedResponse<EmployeeMovementDto> { Total = total, Data = data };
        }
    }
}
