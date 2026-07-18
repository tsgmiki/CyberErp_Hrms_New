using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Performance
{
    /// <summary>
    /// Workflow extension point for the <see cref="WorkflowEntityTypes.Appraisal"/> entity type. The appraisal
    /// drives its own routing instance in lockstep through its rich per-stage actions (score / sign / complete),
    /// so by the time the instance reaches a terminal outcome the appraisal has already applied it (Completed +
    /// its own downstream notifications). These callbacks are therefore no-ops — the handler exists so the engine
    /// recognises "Appraisal" as a workflow-enabled entity type and the instance can complete cleanly.
    /// </summary>
    public class AppraisalWorkflowHandler(ILogger<AppraisalWorkflowHandler> logger) : IWorkflowEntityHandler
    {
        public bool Supports(string entityType) =>
            string.Equals(entityType, WorkflowEntityTypes.Appraisal, StringComparison.OrdinalIgnoreCase);

        public Task OnApprovedAsync(string entityType, Guid entityId)
        {
            logger.LogInformation("Appraisal {Id} routing instance completed (Approved).", entityId);
            return Task.CompletedTask;
        }

        public Task OnRejectedAsync(string entityType, Guid entityId)
        {
            logger.LogInformation("Appraisal {Id} routing instance closed (Rejected).", entityId);
            return Task.CompletedTask;
        }
    }
}
