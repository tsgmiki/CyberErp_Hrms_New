using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    public class WorkflowApproverDto
    {
        /// <summary>"User" or "Role".</summary>
        public string ApproverType { get; set; } = "User";
        public Guid ApproverId { get; set; }
        /// <summary>Resolved server-side (user full name / role name).</summary>
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
                    a.RuleFor(x => x.ApproverId).NotEmpty();
                    a.RuleFor(x => x.ApproverType).NotEmpty()
                        .Must(v => string.Equals(v, "User", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(v, "Role", StringComparison.OrdinalIgnoreCase))
                        .WithMessage("ApproverType must be User or Role.");
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
