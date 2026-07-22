using CyberErp.Hrms.App.Features.Core.Employees;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Exit / Separation (§3.9.2) — Phase X3: exit interviews + final settlements.

    /// <summary>Exit interviews (HC219): configurable questionnaire, snapshot per case, self-service answers.</summary>
    public class ExitInterviewController(
        IGetExitQuestionnaire getQuestionnaireHandler,
        ISaveExitQuestionnaire saveQuestionnaireHandler,
        ILaunchExitInterview launchHandler,
        ISubmitExitInterview submitHandler,
        IGetExitInterview getHandler) : BaseController
    {
        /// <summary>The tenant's questionnaire configuration (HR).</summary>
        [HttpGet("questionnaire")]
        public Task<ExitQuestionnaireDto> GetQuestionnaire() => getQuestionnaireHandler.GetAsync();

        [HttpPut("questionnaire")]
        public async Task<IActionResult> SaveQuestionnaire([FromBody] ExitQuestionnaireDto dto)
        { await saveQuestionnaireHandler.SaveAsync(dto); return Ok(new { message = "Questionnaire saved" }); }

        /// <summary>Launches (or returns) the case's interview with a questionnaire snapshot.</summary>
        [HttpPost("launch/{terminationId:guid}")]
        public async Task<IActionResult> Launch(Guid terminationId) => Ok(new { id = await launchHandler.LaunchAsync(terminationId) });

        /// <summary>The leaver answers (self-service) or HR records the conversation.</summary>
        [HttpPost("{id:guid}/submit")]
        public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitExitInterviewDto dto)
        { await submitHandler.SubmitAsync(id, dto); return Ok(new { message = "Interview recorded" }); }

        /// <summary>The case's interview (null when not launched) — leaver, scope or HR.</summary>
        [HttpGet("{terminationId:guid}")]
        public Task<ExitInterviewDto?> Get(Guid terminationId) => getHandler.GetAsync(terminationId);
    }

    /// <summary>
    /// Final settlements (HC216/HC217/HC218): auto-suggested worksheet, HR edits while Draft,
    /// approval locks, payment records the payroll/finance hand-off.
    /// </summary>
    public class TerminationSettlementController(
        IBuildTerminationSettlement buildHandler,
        IUpdateSettlementLines updateLinesHandler,
        IApproveTerminationSettlement approveHandler,
        IMarkTerminationSettlementPaid payHandler,
        IGetTerminationSettlement getHandler) : BaseController
    {
        /// <summary>Creates the worksheet with auto-suggested lines (idempotent per case).</summary>
        [HttpPost("build/{terminationId:guid}")]
        public async Task<IActionResult> Build(Guid terminationId) => Ok(new { id = await buildHandler.BuildAsync(terminationId) });

        /// <summary>The case's settlement (null when not built) — leaver, scope or HR.</summary>
        [HttpGet("{terminationId:guid}")]
        public Task<TerminationSettlementDto?> Get(Guid terminationId) => getHandler.GetAsync(terminationId);

        [HttpPut("{settlementId:guid}/lines")]
        public async Task<IActionResult> UpdateLines(Guid settlementId, [FromBody] UpdateSettlementLinesDto dto)
        { await updateLinesHandler.UpdateAsync(settlementId, dto); return Ok(new { message = "Worksheet updated" }); }

        [HttpPost("{settlementId:guid}/approve")]
        public async Task<IActionResult> Approve(Guid settlementId)
        { await approveHandler.ApproveAsync(settlementId); return Ok(new { message = "Settlement approved" }); }

        [HttpPost("{settlementId:guid}/mark-paid")]
        public async Task<IActionResult> MarkPaid(Guid settlementId, [FromBody] MarkSettlementPaidDto dto)
        { await payHandler.PayAsync(settlementId, dto); return Ok(new { message = "Settlement paid" }); }
    }
}
