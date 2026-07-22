using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Trips;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // §3.10.5 Trip Management — T1: per-diem rates + travel budgets.

    /// <summary>Per-diem daily rates by job grade and trip type (HC267).</summary>
    public class PerDiemRateController(
        ISavePerDiemRate saveHandler,
        IGetAllPerDiemRates getAllHandler,
        IGetPerDiemRateById getByIdHandler,
        IDeletePerDiemRate deleteHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<PerDiemRateDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<PerDiemRateDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SavePerDiemRateDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SavePerDiemRateDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Travel budgets per organization unit and fiscal year (HC266).</summary>
    public class TripBudgetController(
        ISaveTripBudget saveHandler,
        IGetAllTripBudgets getAllHandler,
        IGetTripBudgetById getByIdHandler,
        IDeleteTripBudget deleteHandler,
        IGetTripBudgetUtilization utilizationHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<TripBudgetDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<TripBudgetDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveTripBudgetDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveTripBudgetDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }

        /// <summary>HC266 — computed budget utilization (committed advances vs allocation).</summary>
        [HttpGet("utilization")]
        public Task<TripBudgetUtilizationDto> Utilization([FromQuery] int fiscalYear, [FromQuery] Guid? organizationUnitId) => utilizationHandler.GetAsync(fiscalYear, organizationUnitId);
    }

    /// <summary>Business trip requests + lifecycle + expenses (HC260/HC261/HC262).</summary>
    public class TripRequestController(
        IRequestTrip requestHandler,
        IGetTrips getAllHandler,
        IGetTripById getByIdHandler,
        IApproveTrip approveHandler,
        IRejectTrip rejectHandler,
        ICancelTrip cancelHandler,
        ITransitionTrip transitionHandler,
        IAddTripExpense addExpenseHandler,
        IRemoveTripExpense removeExpenseHandler,
        IDisburseTripAdvance disburseHandler,
        ISettleTrip settleHandler,
        IGetTripAgingReport agingHandler,
        ITripSettlementReminder reminderHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<TripRequestDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<TripRequestDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>HC265 — aging report of outstanding trip advances.</summary>
        [HttpGet("aging-report")] public Task<TripAgingReportDto> AgingReport() => agingHandler.GetAsync();

        [HttpPost] public async Task<IActionResult> Request([FromBody] RequestTripDto dto) => Ok(new { id = await requestHandler.RequestAsync(dto) });

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveTripDto? dto) { await approveHandler.ApproveAsync(id, dto?.Note); return Ok(new { message = "Approved" }); }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectTripDto dto) { await rejectHandler.RejectAsync(id, dto.Reason); return Ok(new { message = "Rejected" }); }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id) { await cancelHandler.CancelAsync(id); return Ok(new { message = "Cancelled" }); }

        [HttpPost("{id:guid}/start")]
        public async Task<IActionResult> Start(Guid id) { await transitionHandler.StartAsync(id); return Ok(new { message = "Trip started" }); }

        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id) { await transitionHandler.CompleteAsync(id); return Ok(new { message = "Trip completed" }); }

        [HttpPost("expenses")]
        public async Task<IActionResult> AddExpense([FromBody] AddTripExpenseDto dto) => Ok(new { id = await addExpenseHandler.AddAsync(dto) });

        [HttpDelete("expenses/{expenseId:guid}")]
        public async Task<IActionResult> RemoveExpense(Guid expenseId) { await removeExpenseHandler.RemoveAsync(expenseId); return Ok(new { message = "Removed" }); }

        /// <summary>HC268 — pay the trip advance (finance/CBS hand-off).</summary>
        [HttpPost("{id:guid}/disburse-advance")]
        public async Task<IActionResult> DisburseAdvance(Guid id, [FromBody] DisburseTripAdvanceDto? dto) { await disburseHandler.DisburseAsync(id, dto?.Reference); return Ok(new { message = "Advance paid" }); }

        /// <summary>HC264 — settle the trip (advance reconciled against expenses).</summary>
        [HttpPost("{id:guid}/settle")]
        public async Task<IActionResult> Settle(Guid id, [FromBody] SettleTripDto? dto) { var net = await settleHandler.SettleAsync(id, dto?.Reference); return Ok(new { message = "Settled", net }); }

        /// <summary>HC263 — send settlement reminders for overdue advances in this tenant now.</summary>
        [HttpPost("run-settlement-reminders")]
        public async Task<IActionResult> RunReminders() => Ok(new { sent = await reminderHandler.RunAsync() });
    }
}
