using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    public interface IGetAllWorkflowInstances { Task<PaginatedResponse<WorkflowInstanceDto>> GetAsync(GetAllRequest request); }
    public interface IGetMyApprovals { Task<MyApprovalsDto> GetAsync(); }
    public interface IGetWorkflowStats { Task<WorkflowStatsDto> GetAsync(); }
    public interface IGetWorkflowActions { Task<List<WorkflowActionDto>> GetAsync(Guid instanceId); }
    public interface ISaveWorkflowDefinition { Task<Guid> SaveAsync(SaveWorkflowDefinitionDto dto); }
    public interface IGetAllWorkflowDefinitions { Task<PaginatedResponse<WorkflowDefinitionDto>> GetAsync(GetAllRequest request); }
    public interface IGetWorkflowDefinitionById { Task<WorkflowDefinitionDto> GetAsync(Guid id); }
    public interface IDeleteWorkflowDefinition { Task DeleteAsync(Guid id); }
    public interface ISeedDefaultWorkflows { Task<int> SeedAsync(); }

    // ---- Instances (tracking) --------------------------------------------------

    public class GetAllWorkflowInstances(
        IRepository<WorkflowInstance> repository,
        IRepository<WorkflowDefinition> definitions,
        IWorkflowApproverAuth approverAuth,
        Common.Services.ICurrentUserService currentUser) : IGetAllWorkflowInstances
    {
        public async Task<PaginatedResponse<WorkflowInstanceDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<WorkflowInstanceStatus>(request.Status, true, out var status))
                query = query.Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Summary.Contains(term) || x.EntityType.Contains(term));
            }

            var total = await query.CountAsync();

            var rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip).Take(take)
                .Select(x => new
                {
                    x.Id,
                    x.DefinitionId,
                    DefinitionName = definitions.GetAllWithoutTenantFilter()
                        .Where(d => d.Id == x.DefinitionId).Select(d => d.Name).FirstOrDefault(),
                    x.EntityType,
                    x.EntityId,
                    x.EmployeeId,
                    x.Summary,
                    x.Status,
                    x.CurrentStepOrder,
                    x.CurrentStepName,
                    x.TotalSteps,
                    x.RequestedBy,
                    x.CreatedAt,
                    x.CompletedAt
                })
                .ToListAsync();

            // Batch step-authorization for the page: one approver fetch across the running rows'
            // definitions + one role lookup for the current user.
            var runningDefIds = rows.Where(x => x.Status == WorkflowInstanceStatus.Running)
                .Select(x => x.DefinitionId).Distinct().ToList();
            var approverRows = await definitions.GetAllWithoutTenantFilter()
                .Where(d => runningDefIds.Contains(d.Id))
                .SelectMany(d => d.Steps, (d, s) => new { d.Id, Step = s })
                .SelectMany(x => x.Step.Approvers, (x, a) => new
                {
                    DefinitionId = x.Id,
                    x.Step.StepOrder,
                    a.ApproverType,
                    a.ApproverId,
                    a.DisplayName
                })
                .ToListAsync();
            var userId = currentUser.GetCurrentUserId();
            var roleIds = approverRows.Count > 0
                ? await approverAuth.GetCurrentUserRoleIdsAsync()
                : new HashSet<Guid>();

            var data = new List<WorkflowInstanceDto>(rows.Count);
            foreach (var x in rows)
            {
                var stepApprovers = approverRows
                    .Where(a => a.DefinitionId == x.DefinitionId && a.StepOrder == x.CurrentStepOrder)
                    .ToList();

                bool canDecide;
                List<string> approverNames;
                var hasDynamic = stepApprovers.Any(a =>
                    a.ApproverType is WorkflowApproverType.ImmediateManager or WorkflowApproverType.UnitManager
                        or WorkflowApproverType.SecondLevelManager or WorkflowApproverType.Subject);
                if (hasDynamic && x.Status == WorkflowInstanceStatus.Running)
                {
                    // Dynamic approvers resolve per requester (org-tree climb) — evaluate the row fully.
                    (canDecide, approverNames) = await approverAuth.EvaluateAsync(x.DefinitionId, x.CurrentStepOrder, x.EmployeeId);
                }
                else
                {
                    canDecide = x.Status == WorkflowInstanceStatus.Running &&
                        (stepApprovers.Count == 0 ||
                         stepApprovers.Any(a =>
                             (a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId) ||
                             (a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId))));
                    approverNames = stepApprovers.Select(a => a.DisplayName).ToList();
                }

                data.Add(new WorkflowInstanceDto
                {
                    Id = x.Id,
                    DefinitionName = x.DefinitionName ?? x.EntityType,
                    EntityType = x.EntityType,
                    EntityId = x.EntityId,
                    EmployeeId = x.EmployeeId,
                    Summary = x.Summary,
                    Status = x.Status.ToString(),
                    CurrentStepOrder = x.CurrentStepOrder,
                    CurrentStepName = x.CurrentStepName,
                    TotalSteps = x.TotalSteps,
                    RequestedBy = x.RequestedBy,
                    RequestedAt = x.CreatedAt.ToDateTimeUtc(),
                    CompletedAt = x.CompletedAt,
                    CanDecide = canDecide,
                    CurrentStepApprovers = approverNames
                });
            }

            return new PaginatedResponse<WorkflowInstanceDto> { Total = total, Data = data };
        }
    }

    /// <summary>
    /// The current user's approval inbox (Dashboard "Approvals" tab): every Running instance whose
    /// CURRENT step lists them as a <b>specific</b> approver — directly, or through one of their
    /// roles. Open steps (no configured approvers) are excluded: anyone *may* act on them, so they
    /// belong to no one's personal queue (they stay actionable from the Workflow Tracking page).
    /// Also reports whether the user is an assigned approver at all, for conditional rendering.
    /// </summary>
    public class GetMyApprovals(
        IRepository<WorkflowInstance> repository,
        IRepository<WorkflowDefinition> definitions,
        IRepository<Employee> employees,
        IRepository<User> users,
        IWorkflowApproverAuth approverAuth,
        Common.Services.ICurrentUserService currentUser) : IGetMyApprovals
    {
        public async Task<MyApprovalsDto> GetAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return new MyApprovalsDto { IsApprover = false };

            var roleIds = await approverAuth.GetCurrentUserRoleIdsAsync();

            // Is the user a specific approver anywhere on an ACTIVE definition? (tab visibility)
            var isApprover = await definitions.GetAll()
                .Where(d => d.IsActive)
                .SelectMany(d => d.Steps)
                .SelectMany(s => s.Approvers)
                .AnyAsync(a =>
                    (a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId.Value) ||
                    (a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId)));

            // A linked employee is a potential DYNAMIC approver: a managerial employee resolves for
            // Immediate/Second-Level/Unit Manager steps, and ANY employee can be the Subject of a step that
            // routes to the request's subject (e.g. an appraisal self-assessment / final signature).
            if (!isApprover)
            {
                var myEmployeeId = await users.GetAll()
                    .Where(u => u.Id == userId.Value)
                    .Select(u => u.EmployeeId)
                    .FirstOrDefaultAsync();
                if (myEmployeeId.HasValue)
                {
                    var isManagerial = await employees.GetAll().AnyAsync(e => e.Id == myEmployeeId.Value && e.IsManagerial);
                    var subjectRoutingExists = await definitions.GetAll()
                        .Where(d => d.IsActive)
                        .SelectMany(d => d.Steps)
                        .SelectMany(s => s.Approvers)
                        .AnyAsync(a => a.ApproverType == WorkflowApproverType.Subject);
                    isApprover = isManagerial || subjectRoutingExists;
                }
            }
            if (!isApprover) return new MyApprovalsDto { IsApprover = false };

            // Batch the running instances' current-step approvers (same shape as the tracking list).
            var running = await repository.GetAll()
                .Where(x => x.Status == WorkflowInstanceStatus.Running)
                .Select(x => new
                {
                    x.Id, x.DefinitionId, x.EntityType, x.EntityId, x.EmployeeId, x.Summary,
                    x.CurrentStepOrder, x.CurrentStepName, x.TotalSteps,
                    x.RequestedBy, x.CreatedAt
                })
                .ToListAsync();
            if (running.Count == 0) return new MyApprovalsDto { IsApprover = true };

            var defIds = running.Select(x => x.DefinitionId).Distinct().ToList();
            var approverRows = await definitions.GetAllWithoutTenantFilter()
                .Where(d => defIds.Contains(d.Id))
                .SelectMany(d => d.Steps, (d, s) => new { d.Id, Step = s })
                .SelectMany(x => x.Step.Approvers, (x, a) => new
                {
                    DefinitionId = x.Id,
                    x.Step.StepOrder,
                    a.ApproverType,
                    a.ApproverId
                })
                .ToListAsync();

            var items = new List<MyApprovalItemDto>();
            foreach (var x in running.OrderBy(x => x.CreatedAt))
            {
                var stepApprovers = approverRows
                    .Where(a => a.DefinitionId == x.DefinitionId && a.StepOrder == x.CurrentStepOrder)
                    .ToList();

                // Static match first; dynamic (manager / subject) approvers need the per-requester resolution.
                var mine = stepApprovers.Any(a =>
                    (a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId.Value) ||
                    (a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId)));
                if (!mine && stepApprovers.Any(a =>
                        a.ApproverType is WorkflowApproverType.ImmediateManager or WorkflowApproverType.UnitManager
                            or WorkflowApproverType.SecondLevelManager or WorkflowApproverType.Subject))
                {
                    (mine, _) = await approverAuth.EvaluateAsync(x.DefinitionId, x.CurrentStepOrder, x.EmployeeId);
                }
                if (!mine) continue;

                items.Add(new MyApprovalItemDto
                {
                    InstanceId = x.Id,
                    EntityId = x.EntityId,
                    Summary = x.Summary,
                    EntityType = x.EntityType,
                    CurrentStepOrder = x.CurrentStepOrder,
                    CurrentStepName = x.CurrentStepName,
                    TotalSteps = x.TotalSteps,
                    RequestedBy = x.RequestedBy,
                    RequestedAt = x.CreatedAt.ToDateTimeUtc()
                });
            }

            return new MyApprovalsDto { IsApprover = true, Items = items };
        }
    }

    public class GetWorkflowStats(IRepository<WorkflowInstance> repository) : IGetWorkflowStats
    {
        public async Task<WorkflowStatsDto> GetAsync()
        {
            var counts = await repository.GetAll()
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return new WorkflowStatsDto
            {
                Running = counts.FirstOrDefault(c => c.Status == WorkflowInstanceStatus.Running)?.Count ?? 0,
                Approved = counts.FirstOrDefault(c => c.Status == WorkflowInstanceStatus.Approved)?.Count ?? 0,
                Rejected = counts.FirstOrDefault(c => c.Status == WorkflowInstanceStatus.Rejected)?.Count ?? 0
            };
        }
    }

    public class GetWorkflowActions(
        IRepository<WorkflowActionLog> repository,
        IRepository<WorkflowInstance> instances) : IGetWorkflowActions
    {
        public async Task<List<WorkflowActionDto>> GetAsync(Guid instanceId)
        {
            if (!await instances.GetAll().AnyAsync(i => i.Id == instanceId))
                throw new NotFoundException(nameof(WorkflowInstance), instanceId.ToString());

            var rows = await repository.GetAll()
                .Where(a => a.InstanceId == instanceId)
                .OrderBy(a => a.ActedAt)
                .ToListAsync();

            return rows.Select(a => new WorkflowActionDto
            {
                StepOrder = a.StepOrder,
                StepName = a.StepName,
                Action = a.Action.ToString(),
                Comment = a.Comment,
                ActedBy = a.ActedBy,
                ActedAt = a.ActedAt
            }).ToList();
        }
    }

    // ---- Definitions (admin configuration) --------------------------------------

    internal static class WorkflowDefinitionShared
    {
        internal static WorkflowDefinitionDto ToDto(WorkflowDefinition d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            EntityType = d.EntityType,
            Description = d.Description,
            IsActive = d.IsActive,
            Steps = d.Steps.OrderBy(s => s.StepOrder)
                .Select(s => new WorkflowStepDto
                {
                    StepOrder = s.StepOrder,
                    Name = s.Name,
                    ApproverRole = s.ApproverRole,
                    Approvers = s.Approvers.Select(a => new WorkflowApproverDto
                    {
                        ApproverType = a.ApproverType.ToString(),
                        ApproverId = a.ApproverId,
                        DisplayName = a.DisplayName
                    }).ToList()
                })
                .ToList()
        };
    }

    public class SaveWorkflowDefinition(
        IRepository<WorkflowDefinition> repository,
        IRepository<WorkflowStep> stepRepository,
        IRepository<Dom.Entities.Core.User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<OrganizationUnit> orgUnitRepository,
        IValidator<SaveWorkflowDefinitionDto> validator,
        ILogger<SaveWorkflowDefinition> logger) : ISaveWorkflowDefinition
    {
        public async Task<Guid> SaveAsync(SaveWorkflowDefinitionDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // At most one ACTIVE definition per process — the engine picks it deterministically.
            if (dto.IsActive && await repository.GetAll().AnyAsync(d =>
                    d.EntityType == dto.EntityType && d.IsActive && d.Id != dto.Id))
                throw new ValidationException("entityType",
                    $"An active workflow for '{dto.EntityType}' already exists. Deactivate it first.");

            var steps = await BuildStepSpecsAsync(dto);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll()
                        .Include(d => d.Steps).ThenInclude(s => s.Approvers)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(WorkflowDefinition), dto.Id.Value.ToString());
                entity.Update(dto.Name, dto.EntityType, dto.Description, dto.IsActive);
                entity.SetSteps(steps);
                StampStepTenant(entity);
                // Replacement steps are new rows: mark them Added explicitly, otherwise
                // context.Update(root) treats the app-generated keys as existing (Modified).
                foreach (var step in entity.Steps)
                    await stepRepository.AddAsync(step);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated WorkflowDefinition {Id}", entity.Id);
                return entity.Id;
            }

            var created = WorkflowDefinition.Create(dto.Name, dto.EntityType, dto.Description, dto.IsActive);
            created.SetSteps(steps);
            await repository.AddAsync(created);   // stamps the root's TenantId
            StampStepTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created WorkflowDefinition {Id} ({EntityType})", created.Id, created.EntityType);
            return created.Id;
        }

        /// <summary>
        /// Resolves approver display names server-side (never trusting client text) and validates
        /// that every referenced user / role / org unit exists in the tenant. Dynamic approvers
        /// (ImmediateManager / UnitManager) store no principal — they resolve per-request at
        /// decision time through the org-structure traversal.
        /// </summary>
        private async Task<List<WorkflowStepSpec>> BuildStepSpecsAsync(SaveWorkflowDefinitionDto dto)
        {
            static WorkflowApproverType ParseType(string value) =>
                Enum.TryParse<WorkflowApproverType>(value, true, out var t)
                    ? t
                    : throw new ValidationException("approverType", $"Unknown approver type '{value}'.");

            var typed = dto.Steps.SelectMany(s => s.Approvers)
                .Select(a => (Type: ParseType(a.ApproverType), a.ApproverId)).ToList();
            var userIds = typed.Where(a => a.Type == WorkflowApproverType.User).Select(a => a.ApproverId).Distinct().ToList();
            var roleIds = typed.Where(a => a.Type == WorkflowApproverType.Role).Select(a => a.ApproverId).Distinct().ToList();
            var unitIds = typed.Where(a => a.Type == WorkflowApproverType.UnitManager).Select(a => a.ApproverId).Distinct().ToList();

            var users = await userRepository.GetAll()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);
            var roles = await roleRepository.GetAll()
                .Where(r => roleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name);
            var unitNames = await orgUnitRepository.GetAll()
                .Where(u => unitIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            var specs = new List<WorkflowStepSpec>();
            foreach (var step in dto.Steps)
            {
                var approvers = new List<WorkflowApproverSpec>();
                foreach (var a in step.Approvers)
                {
                    var type = ParseType(a.ApproverType);
                    var spec = type switch
                    {
                        WorkflowApproverType.User => new WorkflowApproverSpec(type, a.ApproverId,
                            users.TryGetValue(a.ApproverId, out var userName)
                                ? userName
                                : throw new NotFoundException("User", a.ApproverId.ToString())),
                        WorkflowApproverType.Role => new WorkflowApproverSpec(type, a.ApproverId,
                            roles.TryGetValue(a.ApproverId, out var roleName)
                                ? roleName
                                : throw new NotFoundException("Role", a.ApproverId.ToString())),
                        WorkflowApproverType.UnitManager => new WorkflowApproverSpec(type, a.ApproverId,
                            unitNames.TryGetValue(a.ApproverId, out var unitName)
                                ? $"Manager of {unitName}"
                                : throw new NotFoundException("OrganizationUnit", a.ApproverId.ToString())),
                        WorkflowApproverType.ImmediateManager =>
                            new WorkflowApproverSpec(type, Guid.Empty, "Immediate Manager"),
                        WorkflowApproverType.SecondLevelManager =>
                            new WorkflowApproverSpec(type, Guid.Empty, "Second-Level Manager"),
                        WorkflowApproverType.Subject =>
                            new WorkflowApproverSpec(type, Guid.Empty, "Subject (the employee)"),
                        _ => throw new ValidationException("approverType", $"Unknown approver type '{a.ApproverType}'.")
                    };
                    approvers.Add(spec);
                }
                specs.Add(new WorkflowStepSpec(step.Name, step.ApproverRole, approvers));
            }
            return specs;
        }

        /// <summary>The repository stamps only aggregate roots — cascade-inserted children copy it here.</summary>
        internal static void StampStepTenant(WorkflowDefinition definition)
        {
            foreach (var step in definition.Steps)
            {
                if (string.IsNullOrEmpty(step.TenantId))
                    step.TenantId = definition.TenantId;
                foreach (var approver in step.Approvers)
                    if (string.IsNullOrEmpty(approver.TenantId))
                        approver.TenantId = definition.TenantId;
            }
        }
    }

    public class GetAllWorkflowDefinitions(IRepository<WorkflowDefinition> repository) : IGetAllWorkflowDefinitions
    {
        public async Task<PaginatedResponse<WorkflowDefinitionDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.EntityType.Contains(term));
            }

            var total = await query.CountAsync();
            var rows = await query
                .Include(d => d.Steps).ThenInclude(s => s.Approvers)
                .OrderBy(x => x.EntityType)
                .Skip(skip).Take(take)
                .ToListAsync();

            return new PaginatedResponse<WorkflowDefinitionDto>
            {
                Total = total,
                Data = rows.Select(WorkflowDefinitionShared.ToDto).ToList()
            };
        }
    }

    public class GetWorkflowDefinitionById(IRepository<WorkflowDefinition> repository) : IGetWorkflowDefinitionById
    {
        public async Task<WorkflowDefinitionDto> GetAsync(Guid id)
        {
            var entity = await repository.GetAll()
                    .Include(d => d.Steps).ThenInclude(s => s.Approvers)
                    .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException(nameof(WorkflowDefinition), id.ToString());
            return WorkflowDefinitionShared.ToDto(entity);
        }
    }

    public class DeleteWorkflowDefinition(
        IRepository<WorkflowDefinition> repository,
        IRepository<WorkflowInstance> instances,
        ILogger<DeleteWorkflowDefinition> logger) : IDeleteWorkflowDefinition
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(WorkflowDefinition), id.ToString());

            if (await instances.GetAll().AnyAsync(i => i.DefinitionId == id))
                throw new ValidationException(nameof(id),
                    "Workflow runs reference this definition. Deactivate it instead of deleting.");

            repository.Delete(entity);
            await repository.SaveChangesAsync();
            logger.LogInformation("Deleted WorkflowDefinition {Id}", id);
        }
    }

    /// <summary>Creates the default approval chains for the initial HR processes (idempotent per tenant).</summary>
    public class SeedDefaultWorkflows(
        IRepository<WorkflowDefinition> repository,
        ILogger<SeedDefaultWorkflows> logger) : ISeedDefaultWorkflows
    {
        private static readonly (string EntityType, string Name, (string, string?)[] Steps)[] Defaults =
        [
            (WorkflowEntityTypes.Transfer, "Transfer Approval",
                [("Supervisor Review", null), ("HR Approval", null)]),
            (WorkflowEntityTypes.Promotion, "Promotion Approval",
                [("Supervisor Review", null), ("HR Approval", null)]),
            (WorkflowEntityTypes.Demotion, "Demotion Approval",
                [("Supervisor Review", null), ("HR Approval", null)]),
            (WorkflowEntityTypes.DisciplinaryMeasure, "Disciplinary Case Approval",
                [("Supervisor Review", null), ("HR Approval", null)]),
            (WorkflowEntityTypes.Termination, "Termination Approval",
                [("Manager Review", "Manager"), ("HRBP Review", "HRBP"), ("Department Head Approval", "Department Head")]),
            (WorkflowEntityTypes.LeaveRequest, "Leave Approval",
                [("Supervisor Review", null), ("HR Approval", null)]),
            (WorkflowEntityTypes.WorkforcePlan, "Workforce Plan Approval",
                [("Directorate Review", "Directorate Head"), ("HR Review", "HR"), ("Finance Review", "Finance"), ("Executive Approval", "Executive")]),
            (WorkflowEntityTypes.HiringRequest, "Hiring Need Approval",
                [("Directorate Head Review", "Directorate Head"), ("HR Review", "HR"), ("Finance Review", "Finance")]),
            (WorkflowEntityTypes.JobRequisition, "Requisition Approval",
                [("HR Review", "HR"), ("Approving Authority", null)]),
            (WorkflowEntityTypes.JobOffer, "Offer Approval",
                [("HR Review", "HR"), ("Approving Authority", null)]),
            (WorkflowEntityTypes.CareerPathChangeRequest, "Career Path Change Approval",
                [("Manager Review", null), ("HR Approval", null)]),
            // HC160 — a succession plan for a critical role is endorsed by the manager then HR before it goes live.
            (WorkflowEntityTypes.SuccessionPlan, "Succession Plan Approval",
                [("Manager Review", null), ("HR Approval", null)]),
            // HC149 — a talent-review / calibration session is endorsed before assessment begins.
            (WorkflowEntityTypes.TalentReview, "Talent Review Approval",
                [("Manager Review", null), ("HR Approval", null)]),
            (WorkflowEntityTypes.RewardNomination, "Reward Nomination Approval",
                [("Manager Review", null), ("HR Approval", null)]),
            // HC228 — a salary revision affects payroll cost; HR proposes, Finance + Executive sign off.
            (WorkflowEntityTypes.SalaryRevision, "Salary Revision Approval",
                [("HR Review", "HR"), ("Finance Review", "Finance"), ("Executive Approval", "Executive")]),
            // HC242 — a medical claim is reviewed by HR then Finance before reimbursement.
            (WorkflowEntityTypes.MedicalClaim, "Medical Claim Approval",
                [("HR Review", "HR"), ("Finance Review", "Finance")]),
            // HC249 — an insurance coverage claim is reviewed by HR then Finance before the insurer payout.
            (WorkflowEntityTypes.InsuranceClaim, "Insurance Claim Approval",
                [("HR Review", "HR"), ("Finance Review", "Finance")]),
            // HC251/HC259 — a staff loan is endorsed by HR, then Finance and Executive before disbursement.
            (WorkflowEntityTypes.EmployeeLoan, "Employee Loan Approval",
                [("HR Review", "HR"), ("Finance Review", "Finance"), ("Executive Approval", "Executive")]),
            // HC261 — per-scope trip chains; an international trip carries an extra Executive sign-off.
            (WorkflowEntityTypes.TripLocal, "Local Trip Approval",
                [("HR Approval", "HR")]),
            (WorkflowEntityTypes.TripInternational, "International Trip Approval",
                [("HR Review", "HR"), ("Executive Approval", "Executive")]),
            // HC188/HC201 — per-type training chains; the costlier Abroad type carries an extra step.
            (WorkflowEntityTypes.TrainingNeedLocal, "Local Training Approval",
                [("Manager Review", null), ("HR Approval", null)]),
            (WorkflowEntityTypes.TrainingNeedAbroad, "Abroad Training Approval",
                [("Manager Review", null), ("HR Approval", null), ("Executive Approval", "Executive")]),
        ];

        public async Task<int> SeedAsync()
        {
            var existing = await repository.GetAll().Select(d => d.EntityType).ToListAsync();
            var created = 0;

            foreach (var (entityType, name, steps) in Defaults)
            {
                if (existing.Contains(entityType)) continue;
                var definition = WorkflowDefinition.Create(name, entityType, "Default approval chain");
                definition.SetSteps(steps);
                await repository.AddAsync(definition);
                SaveWorkflowDefinition.StampStepTenant(definition);
                created++;
            }

            // Appraisal routing carries real approver TYPES (not open steps): the subject employee acts on their
            // own self-assessment + final signature, the manager / second-level manager on their reviews. Built
            // by the Performance module (shared with the generate-time auto-ensure) so the definition is identical.
            if (!existing.Contains(WorkflowEntityTypes.Appraisal))
            {
                var appr = Performance.AppraisalWorkflowService.BuildDefaultDefinition();
                await repository.AddAsync(appr);
                SaveWorkflowDefinition.StampStepTenant(appr);
                created++;
            }

            if (created > 0)
            {
                await repository.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} default workflow definitions", created);
            }
            return created;
        }
    }
}
