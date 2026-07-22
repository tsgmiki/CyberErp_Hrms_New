using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
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
    public class TerminationClearanceDto
    {
        public Guid Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? ClearedBy { get; set; }
        public DateTime? ClearedAt { get; set; }
        /// <summary>Whether the current user is authorized to decide this department's clearance.</summary>
        public bool CanDecide { get; set; } = true;
        /// <summary>Configured approver display names (empty = open — anyone may clear).</summary>
        public List<string> ApproverNames { get; set; } = [];
    }

    /// <summary>One outstanding clearance item assigned to the current user (Dashboard queue).</summary>
    public class MyClearanceItemDto
    {
        public Guid ClearanceId { get; set; }
        public Guid TerminationId { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime LastWorkingDate { get; set; }
    }

    /// <summary>The current user's clearance queue plus whether they are an assigned approver at all.</summary>
    public class MyClearancesDto
    {
        /// <summary>True when the current user is a configured approver on any active department
        /// (drives the conditional Dashboard "Clearance" tab).</summary>
        public bool IsApprover { get; set; }
        public List<MyClearanceItemDto> Items { get; set; } = [];
    }

    /// <summary>One row of the Termination List: a terminated employee + their latest settled case.</summary>
    public class TerminatedEmployeeDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? HireDate { get; set; }
        // Latest termination case (null when the employee was terminated without a recorded case).
        public Guid? TerminationId { get; set; }
        public string? TerminationType { get; set; }
        public DateTime? NoticeDate { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public DateTime? SettledAt { get; set; }
        public string? Reason { get; set; }
    }

    public class EmployeeTerminationDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string TerminationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime NoticeDate { get; set; }
        public DateTime LastWorkingDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime? SettledAt { get; set; }
        public bool AwaitingWorkflow { get; set; }
        public List<TerminationClearanceDto> Clearances { get; set; } = [];
        /// <summary>Values of this form's dynamic custom fields (HC021), keyed by field name.</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class SaveEmployeeTerminationDto
    {
        public Guid? Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string TerminationType { get; set; } = nameof(Dom.Entities.Core.TerminationType.Voluntary);
        public DateTime NoticeDate { get; set; }
        public DateTime LastWorkingDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        /// <summary>Submitted values for this form's dynamic custom fields (HC021).</summary>
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class UpdateTerminationClearanceDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = nameof(ClearanceStatus.Pending);
        public string? Note { get; set; }
    }

    public class SaveEmployeeTerminationDtoValidator : AbstractValidator<SaveEmployeeTerminationDto>
    {
        public SaveEmployeeTerminationDtoValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.TerminationType).NotEmpty()
                .Must(v => Enum.TryParse<TerminationType>(v, true, out _))
                .WithMessage("TerminationType must be Voluntary or Involuntary.");
            RuleFor(x => x.NoticeDate).NotEmpty().WithMessage("Notice date is required.");
            RuleFor(x => x.LastWorkingDate).NotEmpty().WithMessage("Last working date is required.")
                .GreaterThanOrEqualTo(x => x.NoticeDate)
                .WithMessage("Last working date cannot be before the notice date.");
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Remarks).MaximumLength(2000);
        }
    }

    // ---- Interfaces -----------------------------------------------------------
    public interface ISaveEmployeeTermination { Task<Guid> SaveAsync(SaveEmployeeTerminationDto dto); }
    public interface IGetEmployeeTerminations { Task<List<EmployeeTerminationDto>> GetAsync(Guid employeeId); }
    public interface IGetTerminatedEmployees { Task<PaginatedResponse<TerminatedEmployeeDto>> GetAsync(GetAllRequest request); }
    public interface IGetMyClearances { Task<MyClearancesDto> GetAsync(); }
    public interface IUpdateTerminationClearance { Task UpdateAsync(UpdateTerminationClearanceDto dto); }
    public interface IFinalizeEmployeeTermination { Task FinalizeAsync(Guid id); }
    public interface ICancelEmployeeTermination { Task CancelAsync(Guid id); }
    public interface IDeleteEmployeeTermination { Task DeleteAsync(Guid id); }

    internal static class TerminationShared
    {
        /// <summary>Built-in fallback checklist, used only when no clearance departments are configured.</summary>
        internal static readonly (string Department, string Description, Guid? DepartmentId)[] DefaultChecklist =
        [
            ("IT", "Company assets returned (laptop, badge, system accounts revoked)", null),
            ("Store", "Company property returned (uniforms, tools, equipment)", null),
            ("Finance", "Outstanding loans, advances and payroll items settled", null),
        ];

        /// <summary>
        /// Opens the clearance phase: status transition + checklist rows built from the
        /// admin-configured active <see cref="ClearanceDepartment"/>s (falling back to the built-in
        /// defaults when none are configured). New children need two manual touches: the tenant
        /// stamp (repository stamps roots only) and an explicit Add — otherwise
        /// <c>context.Update(root)</c> would mark them Modified (their app-generated keys look
        /// "existing") and SaveChanges would fail with a concurrency exception.
        /// </summary>
        internal static async Task BeginClearanceAsync(
            EmployeeTermination termination,
            IRepository<TerminationClearance> clearanceRepository,
            IRepository<ClearanceDepartment> departmentRepository)
        {
            var configured = await departmentRepository.GetAll()
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
                .Select(d => new { d.Id, d.Name, d.Description })
                .ToListAsync();
            var checklist = configured.Count > 0
                ? configured.Select(d => (d.Name, d.Description, (Guid?)d.Id))
                : DefaultChecklist;

            var existing = termination.Clearances.Select(c => c.Id).ToHashSet();
            termination.BeginClearance(checklist);
            foreach (var item in termination.Clearances.Where(c => !existing.Contains(c.Id)))
            {
                if (string.IsNullOrEmpty(item.TenantId))
                    item.TenantId = termination.TenantId;
                await clearanceRepository.AddAsync(item);
            }
        }

        /// <summary>
        /// Clearance authorization (mirrors the workflow engine's step approvers): a department
        /// with no configured approvers is open; otherwise the user must be listed directly or
        /// hold one of the listed roles. Any single authorized user's decision clears the
        /// department. Legacy rows without a DepartmentId match a configured department by name.
        /// </summary>
        internal static async Task<(bool CanDecide, List<string> ApproverNames)> EvaluateClearanceApproverAsync(
            Guid? departmentId,
            string departmentName,
            IRepository<ClearanceDepartment> departmentRepository,
            IRepository<UserRole> userRoleRepository,
            ICurrentUserService currentUser)
        {
            var approvers = await departmentRepository.GetAll()
                .Where(d => departmentId != null ? d.Id == departmentId : (d.Name == departmentName && d.IsActive))
                .SelectMany(d => d.Approvers)
                .Select(a => new { a.ApproverType, a.ApproverId, a.DisplayName })
                .ToListAsync();

            var names = approvers.Select(a => a.DisplayName).ToList();
            if (approvers.Count == 0) return (true, names); // open department — anyone may clear

            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return (false, names);

            if (approvers.Any(a => a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId.Value))
                return (true, names);

            var roleIds = await userRoleRepository.GetAll()
                .Where(u => u.UserId == userId.Value)
                .Select(u => u.RoleId)
                .ToListAsync();
            var allowed = approvers.Any(a => a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId));
            return (allowed, names);
        }

        internal static async Task<EmployeeTermination> GetWithClearancesAsync(
            IRepository<EmployeeTermination> repository, Guid id)
        {
            return await repository.GetAll()
                    .Include(t => t.Clearances)
                    .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeTermination), id.ToString());
        }
    }

    // ---- Save (initiate / correct while initiated) -------------------------------
    public class SaveEmployeeTermination(
        IRepository<EmployeeTermination> repository,
        IRepository<TerminationClearance> clearanceRepository,
        IRepository<ClearanceDepartment> clearanceDepartmentRepository,
        IRepository<Employee> employeeRepository,
        IRepository<CompanyAsset> assetRepository,
        IRepository<TerminationAssetRecovery> recoveryRepository,
        IWorkflowService workflowService,
        IWorkflowGate workflowGate,
        Performance.IPerformanceVisibilityService visibility,
        ITerminationNotifier notifier,
        ICustomFieldService customFields,
        IValidator<SaveEmployeeTerminationDto> validator,
        ILogger<SaveEmployeeTermination> logger) : ISaveEmployeeTermination
    {
        public async Task<Guid> SaveAsync(SaveEmployeeTerminationDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            var employee = await employeeRepository.GetAll()
                .Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.EmploymentStatus, e.EmployeeNumber,
                    FirstName = e.Person != null ? e.Person.FirstName : string.Empty,
                    LastName = e.Person != null ? e.Person.GrandFatherName : string.Empty
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.ToString());

            var type = Enum.Parse<TerminationType>(dto.TerminationType, true);

            // HC209 — initiation matrix: HR records anything; a manager acts within their subtree;
            // an employee may request their OWN exit, and only a VOLUNTARY one (resignation /
            // early-retirement request) — involuntary cases belong to their manager or HR.
            var scope = await visibility.GetScopeAsync();
            if (!await visibility.CanAccessEmployeeAsync(dto.EmployeeId))
                throw new ValidationException("employeeId", "The employee is outside your scope.");
            var isSelf = scope.EmployeeId.HasValue && scope.EmployeeId.Value == dto.EmployeeId;
            if (isSelf && !scope.IsAdmin && type == TerminationType.Involuntary)
                throw new ValidationException("terminationType",
                    "You can request a voluntary exit (resignation / early retirement) — involuntary cases are recorded by your manager or HR.");

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.Termination, dto.Id.Value);

                var entity = await repository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.EmployeeId == dto.EmployeeId)
                    ?? throw new NotFoundException(nameof(EmployeeTermination), dto.Id.Value.ToString());
                if (entity.Status != TerminationStatus.Initiated)
                    throw new ValidationException("status", "Only initiated terminations can be edited.");

                entity.Update(type, dto.NoticeDate, dto.LastWorkingDate, dto.Reason, dto.Remarks);
                repository.UpdateAsync(entity);
                await customFields.ApplyAsync(EmployeeFieldOwnerType.Termination, entity.Id, dto.CustomFields);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated EmployeeTermination {Id}", entity.Id);
                return entity.Id;
            }

            if (employee.EmploymentStatus == EmploymentStatus.Terminated)
                throw new ValidationException("employeeId", "This employee is already terminated.");
            if (await repository.GetAll().AnyAsync(t =>
                    t.EmployeeId == dto.EmployeeId &&
                    (t.Status == TerminationStatus.Initiated || t.Status == TerminationStatus.ClearanceInProgress)))
                throw new ValidationException("employeeId", "An active termination case already exists for this employee.");

            var created = EmployeeTermination.Create(dto.EmployeeId, type, dto.NoticeDate, dto.LastWorkingDate, dto.Reason, dto.Remarks);
            await repository.AddAsync(created);
            await customFields.ApplyAsync(EmployeeFieldOwnerType.Termination, created.Id, dto.CustomFields);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created EmployeeTermination {Id} ({Type}) for Employee {EmployeeId}", created.Id, type, dto.EmployeeId);

            // Route through the approval chain when configured; approval opens the clearance
            // phase. Without a workflow definition the clearance checklist opens immediately.
            await workflowService.StartIfDefinedAsync(
                WorkflowEntityTypes.Termination, created.Id, dto.EmployeeId,
                $"Termination ({type}) — {employee.FirstName} {employee.LastName} ({employee.EmployeeNumber})");

            if (!await workflowGate.HasRunningAsync(WorkflowEntityTypes.Termination, created.Id))
            {
                await TerminationShared.BeginClearanceAsync(created, clearanceRepository, clearanceDepartmentRepository);
                // HC215 — the asset-recovery checklist opens with the clearance phase.
                await AssetRecoveryShared.GenerateChecklistAsync(created.Id, created.EmployeeId, created.TenantId,
                    assetRepository, recoveryRepository);
                repository.UpdateAsync(created);
                await repository.SaveChangesAsync();
            }

            // HC209/HC220 — every stakeholder receives the request (best-effort, never blocks).
            await notifier.SubmittedAsync(created.Id);

            return created.Id;
        }
    }

    // ---- Get (per employee, with checklist) ---------------------------------------
    public class GetEmployeeTerminations(
        IRepository<EmployeeTermination> repository,
        IRepository<Employee> employeeRepository,
        IRepository<ClearanceDepartment> clearanceDepartmentRepository,
        IRepository<UserRole> userRoleRepository,
        ICurrentUserService currentUser,
        Performance.IPerformanceVisibilityService visibility,
        ICustomFieldService customFields,
        IWorkflowGate workflowGate) : IGetEmployeeTerminations
    {
        public async Task<List<EmployeeTerminationDto>> GetAsync(Guid employeeId)
        {
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, employeeId);
            // Same visibility rule as the save path: the employee themselves, their manager, or HR.
            if (!await visibility.CanAccessEmployeeAsync(employeeId))
                throw new ValidationException("access", "You do not have access to this employee's termination records.");

            // Read-only DTO mapping — no change tracking for the case + clearance entities.
            var rows = await repository.GetAll()
                .AsNoTracking()
                .Include(t => t.Clearances)
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Batch clearance authorization for the page: one approver fetch across all configured
            // departments + one role lookup for the current user (mirrors the workflow list).
            var departments = await clearanceDepartmentRepository.GetAll()
                .SelectMany(d => d.Approvers, (d, a) => new
                {
                    d.Id, d.Name, d.IsActive,
                    a.ApproverType, a.ApproverId, a.DisplayName
                })
                .ToListAsync();
            var userId = currentUser.GetCurrentUserId();
            var roleIds = departments.Count > 0 && userId != null
                ? (await userRoleRepository.GetAll()
                    .Where(u => u.UserId == userId.Value)
                    .Select(u => u.RoleId)
                    .ToListAsync()).ToHashSet()
                : [];

            (bool CanDecide, List<string> Names) Evaluate(TerminationClearance c)
            {
                var approvers = departments
                    .Where(d => c.DepartmentId != null ? d.Id == c.DepartmentId : (d.Name == c.Department && d.IsActive))
                    .ToList();
                var names = approvers.Select(a => a.DisplayName).ToList();
                if (approvers.Count == 0) return (true, names); // open department — anyone may clear
                var canDecide = approvers.Any(a =>
                    (a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId) ||
                    (a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId)));
                return (canDecide, names);
            }

            // One set-based query for the page's workflow flags (was one query PER termination row).
            var initiatedIds = rows.Where(t => t.Status == TerminationStatus.Initiated).Select(t => t.Id).ToList();
            var runningIds = await workflowGate.RunningIdsAsync(WorkflowEntityTypes.Termination, initiatedIds);

            var result = new List<EmployeeTerminationDto>();
            foreach (var t in rows)
            {
                result.Add(new EmployeeTerminationDto
                {
                    Id = t.Id,
                    EmployeeId = t.EmployeeId,
                    TerminationType = t.TerminationType.ToString(),
                    Status = t.Status.ToString(),
                    NoticeDate = t.NoticeDate,
                    LastWorkingDate = t.LastWorkingDate,
                    Reason = t.Reason,
                    Remarks = t.Remarks,
                    SettledAt = t.SettledAt,
                    AwaitingWorkflow = t.Status == TerminationStatus.Initiated && runningIds.Contains(t.Id),
                    Clearances = t.Clearances
                        .OrderBy(c => c.Department)
                        .Select(c =>
                        {
                            var (canDecide, names) = Evaluate(c);
                            return new TerminationClearanceDto
                            {
                                Id = c.Id,
                                Department = c.Department,
                                Description = c.Description,
                                Status = c.Status.ToString(),
                                Note = c.Note,
                                ClearedBy = c.ClearedBy,
                                ClearedAt = c.ClearedAt,
                                CanDecide = canDecide,
                                ApproverNames = names
                            };
                        })
                        .ToList()
                });
            }

            var byOwner = await customFields.GetValuesForOwnersAsync(
                EmployeeFieldOwnerType.Termination, result.Select(x => x.Id).ToList());
            foreach (var item in result)
                item.CustomFields = byOwner.TryGetValue(item.Id, out var m) ? m : new();

            return result;
        }
    }

    // ---- Clearance updates -----------------------------------------------------
    public class UpdateTerminationClearance(
        IRepository<TerminationClearance> repository,
        IRepository<EmployeeTermination> terminationRepository,
        IRepository<Employee> employeeRepository,
        IRepository<ClearanceDepartment> clearanceDepartmentRepository,
        IRepository<UserRole> userRoleRepository,
        ICurrentUserService currentUser,
        ILogger<UpdateTerminationClearance> logger) : IUpdateTerminationClearance
    {
        public async Task UpdateAsync(UpdateTerminationClearanceDto dto)
        {
            if (!Enum.TryParse<ClearanceStatus>(dto.Status, true, out var status))
                throw new ValidationException("status", "Status must be Pending, Cleared or Blocked.");

            var item = await repository.GetAll().FirstOrDefaultAsync(c => c.Id == dto.Id)
                ?? throw new NotFoundException(nameof(TerminationClearance), dto.Id.ToString());

            var termination = await terminationRepository.GetAll().FirstOrDefaultAsync(t => t.Id == item.TerminationId)
                ?? throw new NotFoundException(nameof(EmployeeTermination), item.TerminationId.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, termination.EmployeeId);
            if (termination.Status != TerminationStatus.ClearanceInProgress)
                throw new ValidationException("status", $"Clearances can only be updated while clearance is in progress (case is {termination.Status}).");

            // Departments with configured approvers accept a decision only from an authorized user
            // (any single one of them); departments without approvers stay open.
            var (canDecide, approverNames) = await TerminationShared.EvaluateClearanceApproverAsync(
                item.DepartmentId, item.Department, clearanceDepartmentRepository, userRoleRepository, currentUser);
            if (!canDecide)
                throw new ValidationException("approver",
                    $"You are not an authorized approver for the '{item.Department}' clearance." +
                    (approverNames.Count > 0 ? $" Authorized: {string.Join(", ", approverNames)}." : string.Empty));

            item.SetStatus(status, dto.Note, currentUser.GetCurrentUserName());
            repository.UpdateAsync(item);
            await repository.SaveChangesAsync();
            logger.LogInformation("Clearance {Id} ({Department}) set to {Status}", item.Id, item.Department, status);
        }
    }

    // ---- Finalize (system automations) -------------------------------------------
    public class FinalizeEmployeeTermination(
        IRepository<EmployeeTermination> repository,
        IRepository<TerminationClearance> clearanceRepository,
        IRepository<ClearanceDepartment> clearanceDepartmentRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Position> positionRepository,
        IRepository<TerminationAssetRecovery> assetRecoveryRepository,
        ITerminationNotifier notifier,
        ILogger<FinalizeEmployeeTermination> logger) : IFinalizeEmployeeTermination
    {
        public async Task FinalizeAsync(Guid id)
        {
            var termination = await TerminationShared.GetWithClearancesAsync(repository, id);
            if (termination.Status != TerminationStatus.ClearanceInProgress)
                throw new ValidationException("status", $"Only a clearance-in-progress termination can be finalized (current: {termination.Status}).");

            // Settlement is gated on the ASSIGNED approvers: every clearance whose department has one
            // or more configured approvers must be Cleared (approvers act from their Dashboard queue).
            // A clearance can only reach Cleared through the authorization-checked update handler, so a
            // Cleared assigned item means an authorized approver signed it off.
            var deptsWithApprovers = await clearanceDepartmentRepository.GetAll()
                .Where(d => d.Approvers.Any())
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();
            var assignedIds = deptsWithApprovers.Select(d => d.Id).ToHashSet();
            var assignedNames = deptsWithApprovers.Select(d => d.Name).ToHashSet();
            bool IsAssigned(TerminationClearance c) => c.DepartmentId != null
                ? assignedIds.Contains(c.DepartmentId.Value)
                : assignedNames.Contains(c.Department);

            // A blocked item (by any approver) always halts settlement.
            var blocked = termination.Clearances
                .Where(c => c.Status == ClearanceStatus.Blocked)
                .Select(c => c.Department)
                .ToList();
            if (blocked.Count > 0)
                throw new ValidationException("clearances",
                    $"Settlement is blocked by: {string.Join(", ", blocked)}. Resolve the block first.");

            var awaitingApprovers = termination.Clearances
                .Where(c => IsAssigned(c) && c.Status != ClearanceStatus.Cleared)
                .Select(c => c.Department)
                .ToList();
            if (awaitingApprovers.Count > 0)
                throw new ValidationException("clearances",
                    $"All assigned approvers must complete their clearance before settlement. Awaiting: {string.Join(", ", awaitingApprovers)}.");

            // HC215 — settlement waits for the company property: every checklist item must be
            // recovered or explicitly waived (assets are concrete; nothing auto-clears).
            var outstandingAssets = await assetRecoveryRepository.GetAll().AsNoTracking()
                .Where(r => r.TerminationId == id && r.Status == AssetRecoveryStatus.Outstanding)
                .Select(r => r.AssetName)
                .ToListAsync();
            if (outstandingAssets.Count > 0)
                throw new ValidationException("assets",
                    $"Company assets are still outstanding: {string.Join(", ", outstandingAssets)}. Recover or waive them before settlement.");

            // Any remaining not-yet-cleared items belong to departments with NO assigned approver, so
            // there is nobody to sign them off — auto-resolve them on settlement (traceable via note).
            foreach (var c in termination.Clearances.Where(c => c.Status != ClearanceStatus.Cleared))
            {
                c.SetStatus(ClearanceStatus.Cleared, "Auto-cleared on settlement — no approver assigned.", "system");
                clearanceRepository.UpdateAsync(c);
            }

            var employee = await employeeRepository.GetAll().FirstOrDefaultAsync(e => e.Id == termination.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), termination.EmployeeId.ToString());

            // System automations: end the employment, decouple the seat and reopen it. The vacated
            // position is captured before Terminate() nulls it so reinstatement can restore it.
            var oldPositionId = employee.PositionId;
            employee.Terminate();
            employeeRepository.UpdateAsync(employee);
            await EmployeeShared.RecomputePositionVacancyAsync(oldPositionId, employee.Id, positionRepository, employeeRepository);

            termination.MarkSettled(oldPositionId);
            repository.UpdateAsync(termination);
            await repository.SaveChangesAsync();
            logger.LogInformation("Settled EmployeeTermination {Id}: employee {EmployeeId} terminated, position {PositionId} reopened",
                id, employee.Id, oldPositionId);
            await notifier.SettledAsync(id);   // HC220 — best-effort
        }
    }

    // ---- Terminated employees (Termination List) ----------------------------------

    /// <summary>
    /// Paged directory of terminated employees (excluded from the main employee list), each with
    /// their latest termination case for the Termination List grid.
    /// </summary>
    public class GetTerminatedEmployees(
        IRepository<Employee> employeeRepository,
        IRepository<EmployeeTermination> terminationRepository) : IGetTerminatedEmployees
    {
        public async Task<PaginatedResponse<TerminatedEmployeeDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = employeeRepository.GetAll()
                .Where(e => e.IsTerminated || e.EmploymentStatus == EmploymentStatus.Terminated);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x =>
                    x.EmployeeNumber.Contains(term) ||
                    (x.Person != null && (
                        x.Person.FirstName.Contains(term) ||
                        (x.Person.FatherName != null && x.Person.FatherName.Contains(term)) ||
                        x.Person.GrandFatherName.Contains(term))) ||
                    (x.Email != null && x.Email.Contains(term)));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.UpdatedAt)   // most recently terminated first
                .Skip(skip).Take(take)
                .Select(e => new
                {
                    e.Id, e.EmployeeNumber, e.PhotoUrl, e.Email, e.HireDate,
                    FirstName = e.Person != null ? e.Person.FirstName : string.Empty,
                    FatherName = e.Person != null ? e.Person.FatherName : null,
                    GrandFatherName = e.Person != null ? e.Person.GrandFatherName : string.Empty,
                    PhoneNumber = e.Person != null ? e.Person.PhoneNumber : null,
                    // Latest case: prefer the settled one; a directly-terminated employee has none.
                    Termination = terminationRepository.GetAll()
                        .Where(x => x.EmployeeId == e.Id)
                        .OrderByDescending(x => x.Status == TerminationStatus.Settled)
                        .ThenByDescending(x => x.CreatedAt)
                        .Select(x => new
                        {
                            x.Id, x.TerminationType, x.NoticeDate, x.LastWorkingDate, x.SettledAt, x.Reason
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return new PaginatedResponse<TerminatedEmployeeDto>
            {
                Total = total,
                Data = data.Select(e => new TerminatedEmployeeDto
                {
                    EmployeeId = e.Id,
                    EmployeeNumber = e.EmployeeNumber,
                    FullName = string.Join(" ",
                        new[] { e.FirstName, e.FatherName, e.GrandFatherName }.Where(p => !string.IsNullOrWhiteSpace(p))),
                    PhotoUrl = e.PhotoUrl,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    HireDate = e.HireDate,
                    TerminationId = e.Termination != null ? e.Termination.Id : null,
                    TerminationType = e.Termination != null ? e.Termination.TerminationType.ToString() : null,
                    NoticeDate = e.Termination != null ? e.Termination.NoticeDate : null,
                    LastWorkingDate = e.Termination != null ? e.Termination.LastWorkingDate : null,
                    SettledAt = e.Termination != null ? e.Termination.SettledAt : null,
                    Reason = e.Termination != null ? e.Termination.Reason : null
                }).ToList()
            };
        }
    }

    // ---- My clearances (Dashboard approver queue) ---------------------------------

    /// <summary>
    /// The current user's clearance work queue: every outstanding (not-yet-Cleared) clearance item,
    /// across in-progress termination cases, for a department where they are a **specific** approver
    /// (listed directly, or via one of their roles). Open departments (no configured approvers) are
    /// excluded — they belong to no one in particular. Also reports whether the user is an approver
    /// at all, which drives the conditional Dashboard "Clearance" tab.
    /// </summary>
    public class GetMyClearances(
        IRepository<EmployeeTermination> terminationRepository,
        IRepository<Employee> employeeRepository,
        IRepository<ClearanceDepartment> clearanceDepartmentRepository,
        IRepository<UserRole> userRoleRepository,
        ICurrentUserService currentUser) : IGetMyClearances
    {
        public async Task<MyClearancesDto> GetAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return new MyClearancesDto { IsApprover = false };

            var roleIds = (await userRoleRepository.GetAll()
                .Where(u => u.UserId == userId.Value)
                .Select(u => u.RoleId)
                .ToListAsync()).ToHashSet();

            // Active departments the current user is a *specific* approver of (user or role).
            var myDepartments = await clearanceDepartmentRepository.GetAll()
                .Where(d => d.IsActive && d.Approvers.Any(a =>
                    (a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId.Value) ||
                    (a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId))))
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();

            if (myDepartments.Count == 0) return new MyClearancesDto { IsApprover = false };

            var myDeptIds = myDepartments.Select(d => d.Id).ToHashSet();
            var myDeptNames = myDepartments.Select(d => d.Name).ToHashSet();

            // Outstanding clearance items in in-progress cases that belong to one of those departments.
            var rows = await terminationRepository.GetAll()
                .Where(t => t.Status == TerminationStatus.ClearanceInProgress)
                .SelectMany(t => t.Clearances, (t, c) => new { t.Id, t.EmployeeId, t.LastWorkingDate, Clearance = c })
                .Where(x => x.Clearance.Status != ClearanceStatus.Cleared &&
                    (x.Clearance.DepartmentId != null
                        ? myDeptIds.Contains(x.Clearance.DepartmentId.Value)
                        : myDeptNames.Contains(x.Clearance.Department)))
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeId,
                    x.LastWorkingDate,
                    ClearanceId = x.Clearance.Id,
                    x.Clearance.Department,
                    x.Clearance.Description,
                    x.Clearance.Status,
                    x.Clearance.Note,
                    // Employee display resolved through the shared person record (no direct nav).
                    EmployeeNumber = employeeRepository.GetAll()
                        .Where(e => e.Id == x.EmployeeId).Select(e => e.EmployeeNumber).FirstOrDefault(),
                    FirstName = employeeRepository.GetAll()
                        .Where(e => e.Id == x.EmployeeId).Select(e => e.Person != null ? e.Person.FirstName : null).FirstOrDefault(),
                    FatherName = employeeRepository.GetAll()
                        .Where(e => e.Id == x.EmployeeId).Select(e => e.Person != null ? e.Person.FatherName : null).FirstOrDefault(),
                    GrandFatherName = employeeRepository.GetAll()
                        .Where(e => e.Id == x.EmployeeId).Select(e => e.Person != null ? e.Person.GrandFatherName : null).FirstOrDefault()
                })
                .ToListAsync();

            return new MyClearancesDto
            {
                IsApprover = true,
                Items = rows
                    .OrderBy(r => r.LastWorkingDate)
                    .Select(r => new MyClearanceItemDto
                    {
                        ClearanceId = r.ClearanceId,
                        TerminationId = r.Id,
                        EmployeeId = r.EmployeeId,
                        EmployeeNumber = r.EmployeeNumber ?? string.Empty,
                        EmployeeName = string.Join(" ",
                            new[] { r.FirstName, r.FatherName, r.GrandFatherName }.Where(p => !string.IsNullOrWhiteSpace(p))),
                        Department = r.Department,
                        Description = r.Description,
                        Status = r.Status.ToString(),
                        Note = r.Note,
                        LastWorkingDate = r.LastWorkingDate
                    })
                    .ToList()
            };
        }
    }

    // ---- Cancel / Delete ----------------------------------------------------------
    public class CancelEmployeeTermination(
        IRepository<EmployeeTermination> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        ITerminationNotifier notifier,
        ILogger<CancelEmployeeTermination> logger) : ICancelEmployeeTermination
    {
        public async Task CancelAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.Termination, id);

            var termination = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeTermination), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, termination.EmployeeId);
            if (termination.Status is TerminationStatus.Settled or TerminationStatus.Cancelled)
                throw new ValidationException("status", $"A {termination.Status} termination can no longer be cancelled.");

            termination.Cancel();
            repository.UpdateAsync(termination);
            await repository.SaveChangesAsync();
            logger.LogInformation("Cancelled EmployeeTermination {Id}", id);
            await notifier.CancelledAsync(id);   // HC220 — best-effort
        }
    }

    public class DeleteEmployeeTermination(
        IRepository<EmployeeTermination> repository,
        IRepository<Employee> employeeRepository,
        IWorkflowGate workflowGate,
        ICustomFieldService customFields,
        ILogger<DeleteEmployeeTermination> logger) : IDeleteEmployeeTermination
    {
        public async Task DeleteAsync(Guid id)
        {
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.Termination, id);

            var termination = await repository.GetAll().FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException(nameof(EmployeeTermination), id.ToString());
            await EmployeeGuard.EnsureEmployeeVisibleAsync(employeeRepository, termination.EmployeeId);
            if (termination.Status == TerminationStatus.Settled)
                throw new ValidationException("status", "Settled terminations are part of the employee's history and cannot be deleted.");

            await customFields.DeleteForOwnerAsync(EmployeeFieldOwnerType.Termination, id);
            repository.Delete(termination);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted EmployeeTermination {Id}", id);
        }
    }
}
