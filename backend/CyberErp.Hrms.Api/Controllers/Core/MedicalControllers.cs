using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Medical;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // §3.10.2 Medical Benefit Management — MB1: providers, plans, service contracts.

    /// <summary>Approved medical service providers (HC238).</summary>
    public class MedicalProviderController(
        ISaveMedicalProvider saveHandler,
        IGetAllMedicalProviders getAllHandler,
        IGetMedicalProviderById getByIdHandler,
        IDeleteMedicalProvider deleteHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<MedicalProviderDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<MedicalProviderDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveMedicalProviderDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveMedicalProviderDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Medical coverage plans (HC235).</summary>
    public class MedicalPlanController(
        ISaveMedicalPlan saveHandler,
        IGetAllMedicalPlans getAllHandler,
        IGetMedicalPlanById getByIdHandler,
        IDeleteMedicalPlan deleteHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<MedicalPlanDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<MedicalPlanDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveMedicalPlanDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveMedicalPlanDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Credit medical service contracts with providers (HC236).</summary>
    public class MedicalContractController(
        ISaveMedicalContract saveHandler,
        IGetAllMedicalContracts getAllHandler,
        IGetMedicalContractById getByIdHandler,
        IDeleteMedicalContract deleteHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<MedicalContractDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<MedicalContractDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveMedicalContractDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveMedicalContractDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Medical enrollment + beneficiaries (HC235/HC237).</summary>
    public class MedicalEnrollmentController(
        ISaveMedicalEnrollment saveHandler,
        IGetEmployeeMedicalEnrollments getHandler,
        IGetMyMedicalEnrollments getMineHandler,
        ISetMedicalEnrollmentStatus statusHandler,
        IAddMedicalBeneficiary addBeneficiaryHandler,
        IRemoveMedicalBeneficiary removeBeneficiaryHandler,
        IDeleteMedicalEnrollment deleteHandler) : BaseController
    {
        [HttpGet] public Task<List<MedicalEnrollmentDto>> GetByEmployee([FromQuery] Guid employeeId) => getHandler.GetAsync(employeeId);

        /// <summary>The signed-in employee's own enrollments (self-service claim entry).</summary>
        [HttpGet("mine")] public Task<List<MedicalEnrollmentDto>> GetMine() => getMineHandler.GetAsync();
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveMedicalEnrollmentDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveMedicalEnrollmentDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpPost("{id:guid}/status/{status}")]
        public async Task<IActionResult> SetStatus(Guid id, string status, [FromBody] MedicalEnrollmentActionDto? body)
        { await statusHandler.SetAsync(id, status, body?.CoverageEnd); return Ok(new { message = "Coverage updated" }); }

        [HttpPost("beneficiaries")]
        public async Task<IActionResult> AddBeneficiary([FromBody] AddBeneficiaryDto dto) => Ok(new { id = await addBeneficiaryHandler.AddAsync(dto) });

        [HttpDelete("beneficiaries/{beneficiaryId:guid}")]
        public async Task<IActionResult> RemoveBeneficiary(Guid beneficiaryId) { await removeBeneficiaryHandler.RemoveAsync(beneficiaryId); return Ok(new { message = "Beneficiary removed" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Medical claims lifecycle + expense reports (HC239–246).</summary>
    public class MedicalClaimController(
        ISubmitMedicalClaim submitHandler,
        IGetMedicalClaims getAllHandler,
        IGetMedicalClaimById getByIdHandler,
        IDownloadMedicalClaimAttachment downloadHandler,
        IApproveMedicalClaim approveHandler,
        IRejectMedicalClaim rejectHandler,
        IMarkMedicalClaimPaid payHandler,
        IGetMedicalExpenseReport reportHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<MedicalClaimDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<MedicalClaimDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>HC246 — approved/paid medical expense report grouped by beneficiary category.</summary>
        [HttpGet("expense-report")]
        public Task<MedicalExpenseReportDto> Report([FromQuery] string? fromDate, [FromQuery] string? toDate) => reportHandler.GetAsync(fromDate, toDate);

        [HttpGet("attachments/{attachmentId:guid}")]
        public async Task<IActionResult> Download(Guid attachmentId)
        {
            var f = await downloadHandler.GetAsync(attachmentId);
            return File(f.Content, f.ContentType, f.FileName);
        }

        [HttpPost] public async Task<IActionResult> Submit([FromBody] SubmitMedicalClaimDto dto) => Ok(new { id = await submitHandler.SubmitAsync(dto) });

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveMedicalClaimDto dto) { await approveHandler.ApproveAsync(id, dto ?? new()); return Ok(new { message = "Approved" }); }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectMedicalClaimDto dto) { await rejectHandler.RejectAsync(id, dto.Reason); return Ok(new { message = "Rejected" }); }

        [HttpPost("{id:guid}/pay")]
        public async Task<IActionResult> Pay(Guid id, [FromBody] PayMedicalClaimDto? dto) { await payHandler.MarkPaidAsync(id, dto?.Reference); return Ok(new { message = "Recorded as paid" }); }
    }
}
