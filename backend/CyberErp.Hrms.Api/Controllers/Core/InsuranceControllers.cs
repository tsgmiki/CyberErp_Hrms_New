using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Insurance;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // §3.10.3 Insurance Management — I1: policies + premium schedule (HC247/HC250 hand-off).

    /// <summary>Employer group insurance policies and their premium payment schedule (HC247/HC250).</summary>
    public class InsurancePolicyController(
        ISaveInsurancePolicy saveHandler,
        IGetAllInsurancePolicies getAllHandler,
        IGetInsurancePolicyById getByIdHandler,
        IDeleteInsurancePolicy deleteHandler,
        IGeneratePremiumSchedule generateHandler,
        IAddPremiumSchedule addScheduleHandler,
        IRemovePremiumSchedule removeScheduleHandler,
        IMarkInsurancePremiumPaid payHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<InsurancePolicyDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<InsurancePolicyDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveInsurancePolicyDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveInsurancePolicyDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }

        [HttpPost("{id:guid}/generate-schedule")]
        public async Task<IActionResult> GenerateSchedule(Guid id) { await generateHandler.GenerateAsync(id); return Ok(new { message = "Premium schedule generated" }); }

        [HttpPost("schedule")]
        public async Task<IActionResult> AddSchedule([FromBody] AddPremiumScheduleDto dto) => Ok(new { id = await addScheduleHandler.AddAsync(dto) });

        [HttpDelete("schedule/{scheduleId:guid}")]
        public async Task<IActionResult> RemoveSchedule(Guid scheduleId) { await removeScheduleHandler.RemoveAsync(scheduleId); return Ok(new { message = "Removed" }); }

        [HttpPost("schedule/{scheduleId:guid}/pay")]
        public async Task<IActionResult> Pay(Guid scheduleId, [FromBody] MarkPremiumPaidDto? dto) { await payHandler.MarkPaidAsync(scheduleId, dto?.Reference); return Ok(new { message = "Recorded as paid" }); }
    }

    /// <summary>Insurance coverage claims lifecycle (HC248/HC249).</summary>
    public class InsuranceClaimController(
        ISubmitInsuranceClaim submitHandler,
        IGetInsuranceClaims getAllHandler,
        IGetInsuranceClaimById getByIdHandler,
        IDownloadInsuranceClaimAttachment downloadHandler,
        IApproveInsuranceClaim approveHandler,
        IRejectInsuranceClaim rejectHandler,
        IMarkInsuranceClaimPaid payHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<InsuranceClaimDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<InsuranceClaimDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpGet("attachments/{attachmentId:guid}")]
        public async Task<IActionResult> Download(Guid attachmentId)
        {
            var f = await downloadHandler.GetAsync(attachmentId);
            return File(f.Content, f.ContentType, f.FileName);
        }

        [HttpPost] public async Task<IActionResult> Submit([FromBody] SubmitInsuranceClaimDto dto) => Ok(new { id = await submitHandler.SubmitAsync(dto) });

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveInsuranceClaimDto dto) { await approveHandler.ApproveAsync(id, dto ?? new()); return Ok(new { message = "Approved" }); }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectInsuranceClaimDto dto) { await rejectHandler.RejectAsync(id, dto.Reason); return Ok(new { message = "Rejected" }); }

        [HttpPost("{id:guid}/pay")]
        public async Task<IActionResult> Pay(Guid id, [FromBody] PayInsuranceClaimDto? dto) { await payHandler.MarkPaidAsync(id, dto?.Reference); return Ok(new { message = "Recorded as paid" }); }
    }
}
