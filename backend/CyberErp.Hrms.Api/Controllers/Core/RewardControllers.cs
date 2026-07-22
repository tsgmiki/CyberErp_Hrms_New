using System.Text;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Rewards;
using CyberErp.Hrms.App.Features.Core.Workflows;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Reward & Recognition (§3.7.4, HC177–HC186).

    /// <summary>Award categories grouping badges under shared criteria (HC178).</summary>
    public class AwardCategoryController(
        ISaveAwardCategory saveHandler,
        IDeleteAwardCategory deleteHandler,
        IGetAwardCategoryById getByIdHandler,
        IGetAllAwardCategories getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AwardCategoryDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AwardCategoryDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveAwardCategoryDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveAwardCategoryDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Recurring recognition programs, e.g. Employee of the Month (HC182).</summary>
    public class RecognitionProgramController(
        ISaveRecognitionProgram saveHandler,
        IDeleteRecognitionProgram deleteHandler,
        IGetRecognitionProgramById getByIdHandler,
        IGetAllRecognitionPrograms getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<RecognitionProgramDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<RecognitionProgramDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveRecognitionProgramDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveRecognitionProgramDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>
    /// Reward nominations (HC179), routed through the generic workflow engine (HC186). The direct
    /// approve/reject endpoints serve the no-workflow mode only — with a running instance, decisions
    /// come from the My Approvals inbox and the endpoints refuse.
    /// </summary>
    public class RewardNominationController(
        ISaveRewardNomination saveHandler,
        IDeleteRewardNomination deleteHandler,
        IGetRewardNominationById getByIdHandler,
        IGetAllRewardNominations getAllHandler,
        IApproveRewardNomination approveHandler,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<RewardNominationDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<RewardNominationDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveRewardNominationDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveRewardNominationDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                return BadRequest(new { message = "Only HR administrators can approve nominations directly." });
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.RewardNomination, id);
            await approveHandler.ApproveAsync(id);
            return Ok(new { message = "Nomination approved" });
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                return BadRequest(new { message = "Only HR administrators can reject nominations directly." });
            await workflowGate.EnsureNoRunningAsync(WorkflowEntityTypes.RewardNomination, id);
            await approveHandler.RejectAsync(id);
            return Ok(new { message = "Nomination rejected" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Reward-points ledger: balance + statement, and self-service redemption (HC180).</summary>
    public class RewardPointsController(
        IGetRewardPoints getHandler,
        IRedeemRewardPoints redeemHandler,
        IGetPointsLeaderboard leaderboardHandler) : BaseController
    {
        /// <summary>Own points by default; HR/managers may pass an employee in their scope.</summary>
        [HttpGet]
        public Task<RewardPointsSummaryDto> Get([FromQuery] Guid? employeeId, [FromQuery] GetAllRequest request) =>
            getHandler.GetAsync(employeeId, request);

        /// <summary>HC209 — top point earners over the window (default 90 days), open to everyone.</summary>
        [HttpGet("leaderboard")]
        public Task<List<LeaderboardRowDto>> Leaderboard([FromQuery] int days = 90) => leaderboardHandler.GetAsync(days);

        [HttpPost("redeem")]
        public async Task<IActionResult> Redeem([FromBody] RedeemRewardPointsDto dto) =>
            Ok(new { balance = await redeemHandler.RedeemAsync(dto) });
    }

    /// <summary>Monetary reward hand-off for payroll/finance (HC185) — admin-only.</summary>
    public class RewardDisbursementController(
        IGetAllRewardDisbursements getAllHandler,
        IMarkRewardDisbursementPaid payHandler,
        IExportRewardDisbursements exportHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<RewardDisbursementDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpPost("{id:guid}/mark-paid")]
        public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkDisbursementPaidDto dto)
        {
            await payHandler.PayAsync(id, dto);
            return Ok(new { message = "Marked as paid" });
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] GetAllRequest request)
        {
            var csv = await exportHandler.ExportCsvAsync(request);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "reward-disbursements.csv");
        }
    }

    /// <summary>The company-wide public recognition feed (HC184).</summary>
    public class RecognitionWallController(IGetRecognitionWall wallHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<RecognitionWallItemDto>> Get([FromQuery] GetAllRequest request) => wallHandler.GetAsync(request);
    }
}
