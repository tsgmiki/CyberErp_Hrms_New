using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Workflow tracking + approvals (instances of the generic engine).</summary>
    public class WorkflowController(
        IGetAllWorkflowInstances getAllHandler,
        IGetMyApprovals myApprovalsHandler,
        IGetWorkflowStats statsHandler,
        IGetWorkflowActions actionsHandler,
        IWorkflowService workflowService) : BaseController
    {
        /// <summary>Paged workflow runs (filter: ?status=Running|Approved|Rejected, ?searchText=).</summary>
        [HttpGet]
        public Task<PaginatedResponse<WorkflowInstanceDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>The current user's approval inbox (Dashboard "Approvals" tab, approver-only).</summary>
        [HttpGet("my-approvals")]
        public Task<MyApprovalsDto> GetMyApprovals()
            => myApprovalsHandler.GetAsync();

        /// <summary>Counts by status for the dashboard tracking panel.</summary>
        [HttpGet("stats")]
        public Task<WorkflowStatsDto> GetStats()
            => statsHandler.GetAsync();

        /// <summary>Full decision history of one workflow run.</summary>
        [HttpGet("{id:guid}/actions")]
        public Task<List<WorkflowActionDto>> GetActions(Guid id)
            => actionsHandler.GetAsync(id);

        /// <summary>Approves the current step; the final approval applies the module action.</summary>
        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] WorkflowDecisionDto dto)
        {
            await workflowService.ApproveAsync(id, dto?.Comment);
            return Ok(new { message = "Approved" });
        }

        /// <summary>Rejects the workflow; the module's rejection outcome is applied.</summary>
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] WorkflowDecisionDto dto)
        {
            await workflowService.RejectAsync(id, dto?.Comment);
            return Ok(new { message = "Rejected" });
        }
    }

    /// <summary>Admin configuration of approval chains per HR process.</summary>
    public class WorkflowDefinitionController(
        ISaveWorkflowDefinition saveHandler,
        IGetAllWorkflowDefinitions getAllHandler,
        IGetWorkflowDefinitionById getByIdHandler,
        IDeleteWorkflowDefinition deleteHandler,
        ISeedDefaultWorkflows seedHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<WorkflowDefinitionDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<WorkflowDefinitionDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveWorkflowDefinitionDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveWorkflowDefinitionDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }

        /// <summary>Creates the default approval chains for Transfer / Promotion / Demotion / Discipline.</summary>
        [HttpPost("seed-defaults")]
        public async Task<IActionResult> SeedDefaults()
        {
            var created = await seedHandler.SeedAsync();
            return Ok(new { created, message = created > 0 ? $"{created} default workflows created" : "Defaults already exist" });
        }
    }
}
