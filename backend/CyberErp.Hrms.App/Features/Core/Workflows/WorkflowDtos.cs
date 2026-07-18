using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    public class WorkflowApproverDto
    {
        /// <summary>"User", "Role", "ImmediateManager" or "UnitManager".</summary>
        public string ApproverType { get; set; } = "User";
        /// <summary>User/role id, target org-unit id (UnitManager), or Guid.Empty (ImmediateManager).</summary>
        public Guid ApproverId { get; set; }
        /// <summary>Resolved server-side (user full name / role name / "Manager of {unit}").</summary>
        public string? DisplayName { get; set; }
    }

    public class WorkflowStepDto
    {
        public int StepOrder { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ApproverRole { get; set; }
        /// <summary>Authorized approvers. Empty = any authenticated user may act.</summary>
        public List<WorkflowApproverDto> Approvers { get; set; } = [];
    }

    public class WorkflowDefinitionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<WorkflowStepDto> Steps { get; set; } = [];
    }

    public class SaveWorkflowDefinitionDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        /// <summary>Ordered step names (approver role optional).</summary>
        public List<WorkflowStepDto> Steps { get; set; } = [];
    }

    /// <summary>One approval awaiting the current user's decision (Dashboard "Approvals" inbox).</summary>
    public class MyApprovalItemDto
    {
        public Guid InstanceId { get; set; }
        /// <summary>The governed record's id — lets the inbox deep-link to the owning module's screen.</summary>
        public Guid EntityId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int CurrentStepOrder { get; set; }
        public string CurrentStepName { get; set; } = string.Empty;
        public int TotalSteps { get; set; }
        public string? RequestedBy { get; set; }
        public DateTime RequestedAt { get; set; }
    }

    /// <summary>The current user's approval inbox + whether they are an assigned approver at all.</summary>
    public class MyApprovalsDto
    {
        /// <summary>True when the current user is a configured approver (directly or via a role) on
        /// any active workflow step — drives the conditional Dashboard "Approvals" tab.</summary>
        public bool IsApprover { get; set; }
        public List<MyApprovalItemDto> Items { get; set; } = [];
    }

    public class SaveWorkflowDefinitionDtoValidator : AbstractValidator<SaveWorkflowDefinitionDto>
    {
        public SaveWorkflowDefinitionDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Steps).NotEmpty().WithMessage("A workflow needs at least one step.");
            RuleForEach(x => x.Steps).ChildRules(step =>
            {
                step.RuleFor(s => s.Name).NotEmpty().MaximumLength(200)
                    .WithMessage("Every step needs a name.");
                step.RuleFor(s => s.ApproverRole).MaximumLength(200);
                step.RuleForEach(s => s.Approvers).ChildRules(a =>
                {
                    // ImmediateManager carries no principal id (resolved per-request from the org
                    // tree); User/Role need a user/role id and UnitManager the target unit id.
                    a.RuleFor(x => x.ApproverId).NotEmpty()
                        .When(x => !string.Equals(x.ApproverType, "ImmediateManager", StringComparison.OrdinalIgnoreCase));
                    a.RuleFor(x => x.ApproverType).NotEmpty()
                        .Must(v => Enum.TryParse<Dom.Entities.Core.WorkflowApproverType>(v, true, out _))
                        .WithMessage("ApproverType must be User, Role, ImmediateManager or UnitManager.");
                });
            });
        }
    }

    public class WorkflowInstanceDto
    {
        public Guid Id { get; set; }
        public string DefinitionName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CurrentStepOrder { get; set; }
        public string CurrentStepName { get; set; } = string.Empty;
        public int TotalSteps { get; set; }
        public string? RequestedBy { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        /// <summary>Whether the current user may act on the current step (running instances only).</summary>
        public bool CanDecide { get; set; }
        /// <summary>Display names of the current step's authorized approvers (empty = anyone).</summary>
        public List<string> CurrentStepApprovers { get; set; } = [];
    }

    public class WorkflowActionDto
    {
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public string? ActedBy { get; set; }
        public DateTime ActedAt { get; set; }
    }

    public class WorkflowStatsDto
    {
        public int Running { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }

    public class WorkflowDecisionDto
    {
        public string? Comment { get; set; }
    }
}
