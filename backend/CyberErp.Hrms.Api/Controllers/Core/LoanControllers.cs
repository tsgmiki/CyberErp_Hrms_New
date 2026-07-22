using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Loans;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // §3.10.4 Employee Loan Management — L1: loan types + requests + guarantors + schedule + workflow.

    /// <summary>Configurable loan products with limits and interest (HC251).</summary>
    public class LoanTypeController(
        ISaveLoanType saveHandler,
        IGetAllLoanTypes getAllHandler,
        IGetLoanTypeById getByIdHandler,
        IDeleteLoanType deleteHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<LoanTypeDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<LoanTypeDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveLoanTypeDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveLoanTypeDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Employee loan requests + lifecycle (HC252/HC253).</summary>
    public class LoanController(
        IRequestLoan requestHandler,
        IGetLoans getAllHandler,
        IGetLoanById getByIdHandler,
        IApproveLoan approveHandler,
        IRejectLoan rejectHandler,
        ICancelLoan cancelHandler,
        IDisburseLoan disburseHandler,
        IRecordLoanRepayment repayHandler,
        IIncrementLoanInstallment incrementHandler,
        IGiveLoanConsent consentHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<LoanDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<LoanDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Request([FromBody] RequestLoanDto dto) => Ok(new { id = await requestHandler.RequestAsync(dto) });

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLoanDto? dto) { await approveHandler.ApproveAsync(id, dto?.Note); return Ok(new { message = "Approved" }); }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLoanDto dto) { await rejectHandler.RejectAsync(id, dto.Reason); return Ok(new { message = "Rejected" }); }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id) { await cancelHandler.CancelAsync(id); return Ok(new { message = "Cancelled" }); }

        /// <summary>HC256/HC259 — disburse (finance/CBS hand-off) and activate the loan.</summary>
        [HttpPost("{id:guid}/disburse")]
        public async Task<IActionResult> Disburse(Guid id, [FromBody] DisburseLoanDto? dto) { await disburseHandler.DisburseAsync(id, dto?.Reference); return Ok(new { message = "Disbursed" }); }

        /// <summary>HC251/HC254 — record a repayment against the loan ledger.</summary>
        [HttpPost("{id:guid}/repay")]
        public async Task<IActionResult> Repay(Guid id, [FromBody] RecordLoanRepaymentDto dto) { await repayHandler.RecordAsync(id, dto.Amount); return Ok(new { message = "Repayment recorded" }); }

        /// <summary>HC255 — raise the monthly installment (reschedules the remaining installments).</summary>
        [HttpPost("{id:guid}/increment-installment")]
        public async Task<IActionResult> Increment(Guid id, [FromBody] IncrementInstallmentDto dto) { await incrementHandler.IncrementAsync(id, dto.NewMonthlyInstallment); return Ok(new { message = "Installment increased" }); }

        /// <summary>HC257 — the borrower's online service-commitment consent.</summary>
        [HttpPost("{id:guid}/consent")]
        public async Task<IActionResult> Consent(Guid id) { await consentHandler.ConsentAsync(id); return Ok(new { message = "Consent recorded" }); }
    }
}
