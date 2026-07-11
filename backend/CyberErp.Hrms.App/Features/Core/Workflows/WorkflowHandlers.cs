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

            var data = rows.Select(x =>
            {
                var stepApprovers = approverRows
                    .Where(a => a.DefinitionId == x.DefinitionId && a.StepOrder == x.CurrentStepOrder)
                    .ToList();
                var canDecide = x.Status == WorkflowInstanceStatus.Running &&
                    (stepApprovers.Count == 0 ||
                     stepApprovers.Any(a =>
                         (a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId) ||
                         (a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId))));

                return new WorkflowInstanceDto
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
                    CurrentStepApprovers = stepApprovers.Select(a => a.DisplayName).ToList()
                };
            }).ToList();

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
            if (!isApprover) return new MyApprovalsDto { IsApprover = false };

            // Batch the running instances' current-step approvers (same shape as the tracking list).
            var running = await repository.GetAll()
                .Where(x => x.Status == WorkflowInstanceStatus.Running)
                .Select(x => new
                {
                    x.Id, x.DefinitionId, x.EntityType, x.Summary,
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

            var items = running
                .Where(x => approverRows.Any(a =>
                    a.DefinitionId == x.DefinitionId && a.StepOrder == x.CurrentStepOrder &&
                    ((a.ApproverType == WorkflowApproverType.User && a.ApproverId == userId.Value) ||
                     (a.ApproverType == WorkflowApproverType.Role && roleIds.Contains(a.ApproverId)))))
                .OrderBy(x => x.CreatedAt)
                .Select(x => new MyApprovalItemDto
                {
                    InstanceId = x.Id,
                    Summary = x.Summary,
                    EntityType = x.EntityType,
                    CurrentStepOrder = x.CurrentStepOrder,
                    CurrentStepName = x.CurrentStepName,
                    TotalSteps = x.TotalSteps,
                    RequestedBy = x.RequestedBy,
                    RequestedAt = x.CreatedAt.ToDateTimeUtc()
                })
                .ToList();

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
        /// that every referenced user / role exists in the tenant.
        /// </summary>
        private async Task<List<WorkflowStepSpec>> BuildStepSpecsAsync(SaveWorkflowDefinitionDto dto)
        {
            var userIds = dto.Steps.SelectMany(s => s.Approvers)
                .Where(a => string.Equals(a.ApproverType, "User", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.ApproverId).Distinct().ToList();
            var roleIds = dto.Steps.SelectMany(s => s.Approvers)
                .Where(a => string.Equals(a.ApproverType, "Role", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.ApproverId).Distinct().ToList();

            var users = await userRepository.GetAll()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);
            var roles = await roleRepository.GetAll()
                .Where(r => roleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name);

            var specs = new List<WorkflowStepSpec>();
            foreach (var step in dto.Steps)
            {
                var approvers = new List<WorkflowApproverSpec>();
                foreach (var a in step.Approvers)
                {
                    var isUser = string.Equals(a.ApproverType, "User", StringComparison.OrdinalIgnoreCase);
                    if (isUser && !users.TryGetValue(a.ApproverId, out var userName))
                        throw new NotFoundException("User", a.ApproverId.ToString());
                    if (!isUser && !roles.TryGetValue(a.ApproverId, out var roleName))
                        throw new NotFoundException("Role", a.ApproverId.ToString());

                    approvers.Add(new WorkflowApproverSpec(
                        isUser ? WorkflowApproverType.User : WorkflowApproverType.Role,
                        a.ApproverId,
                        isUser ? users[a.ApproverId] : roles[a.ApproverId]));
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

            if (created > 0)
            {
                await repository.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} default workflow definitions", created);
            }
            return created;
        }
    }
}
