using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Compensation;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // §3.10.1 Compensation & Benefit — Phase CB1: allowance catalogue + per-employee allowances.

    /// <summary>Allowance/earning catalogue (HC226): transport, housing, meal, etc.</summary>
    public class AllowanceTypeController(
        ISaveAllowanceType saveHandler,
        IGetAllAllowanceTypes getAllHandler,
        IGetAllowanceTypeById getByIdHandler,
        IDeleteAllowanceType deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AllowanceTypeDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AllowanceTypeDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveAllowanceTypeDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveAllowanceTypeDto dto)
        { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Per-employee allowances + the resolved compensation summary (HC226/HC233).</summary>
    public class EmployeeAllowanceController(
        ISaveEmployeeAllowance saveHandler,
        IGetEmployeeAllowances getHandler,
        IGetCompensationSummary summaryHandler,
        IDeleteEmployeeAllowance deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<List<EmployeeAllowanceDto>> GetByEmployee([FromQuery] Guid employeeId) => getHandler.GetAsync(employeeId);

        /// <summary>HC226/HC233 — resolved compensation snapshot (base + active allowances, taxable split).</summary>
        [HttpGet("summary")]
        public Task<CompensationSummaryDto> GetSummary([FromQuery] Guid employeeId) => summaryHandler.GetAsync(employeeId);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveEmployeeAllowanceDto dto) => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeAllowanceDto dto)
        { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Salary revision planning + scenario simulation (HC228).</summary>
    [RequirePermission("salaryRevision")]
    public class SalaryRevisionController(
        ISimulateSalaryRevision simulateHandler,
        ISaveSalaryRevision saveHandler,
        IGetSalaryRevisionById getByIdHandler,
        IGetAllSalaryRevisions getAllHandler,
        ISetSalaryRevisionLine setLineHandler,
        ISubmitSalaryRevision submitHandler,
        IApproveSalaryRevision approveHandler,
        IApplySalaryRevision applyHandler,
        IDeleteSalaryRevision deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<SalaryRevisionDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<SalaryRevisionDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>HC228 — stateless scenario simulation: try a rate over a target set without persisting.</summary>
        [HttpPost("simulate")]
        public Task<SalarySimulationDto> Simulate([FromBody] SimulateSalaryRevisionDto dto) => simulateHandler.SimulateAsync(dto);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveSalaryRevisionDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveSalaryRevisionDto dto)
        { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }

        /// <summary>Override the computed proposal for one employee (Draft only).</summary>
        [HttpPut("lines/{lineId:guid}")]
        public async Task<IActionResult> SetLine(Guid lineId, [FromBody] SetSalaryRevisionLineRequest body)
        { await setLineHandler.SetAsync(lineId, body.ProposedSalary); return Ok(new { message = "Line updated" }); }

        [HttpPost("{id:guid}/submit")]
        public async Task<IActionResult> Submit(Guid id)
        { await submitHandler.SubmitAsync(id); return Ok(new { message = "Submitted for approval" }); }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        { await approveHandler.ApproveAsync(id); return Ok(new { message = "Approved" }); }

        [HttpPost("{id:guid}/apply")]
        public async Task<IActionResult> Apply(Guid id)
        { await applyHandler.ApplyAsync(id); return Ok(new { message = "Applied — employee salaries updated" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    public class SetSalaryRevisionLineRequest
    {
        public decimal ProposedSalary { get; set; }
    }

    // §3.10.1 CB3 — benefit plans, enrollment, tax config, deductions preview.

    /// <summary>Benefit plan catalogue (HC230): health, life, pension, etc.</summary>
    public class BenefitPlanController(
        ISaveBenefitPlan saveHandler,
        IGetAllBenefitPlans getAllHandler,
        IGetBenefitPlanById getByIdHandler,
        IDeleteBenefitPlan deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<BenefitPlanDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<BenefitPlanDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveBenefitPlanDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveBenefitPlanDto dto)
        { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Employee benefit enrollment (HC230): self-enroll during the open window, HR anytime.</summary>
    public class BenefitEnrollmentController(
        IEnrollBenefit enrollHandler,
        IWaiveBenefit waiveHandler,
        ITerminateBenefit terminateHandler,
        IGetEmployeeBenefits getHandler) : BaseController
    {
        [HttpGet]
        public Task<List<BenefitEnrollmentDto>> GetByEmployee([FromQuery] Guid employeeId) => getHandler.GetAsync(employeeId);

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollBenefitDto dto) => Ok(new { id = await enrollHandler.EnrollAsync(dto) });

        [HttpPost("{id:guid}/waive")]
        public async Task<IActionResult> Waive(Guid id, [FromBody] EnrollmentActionRequest? body)
        { await waiveHandler.WaiveAsync(id, body?.Remark); return Ok(new { message = "Waived" }); }

        [HttpPost("{id:guid}/terminate")]
        public async Task<IActionResult> Terminate(Guid id, [FromBody] TerminateEnrollmentRequest body)
        { await terminateHandler.TerminateAsync(id, body.CoverageEnd, body.Remark); return Ok(new { message = "Terminated" }); }
    }

    /// <summary>Progressive income-tax table + the deductions/contributions preview (HC231/HC232).</summary>
    public class TaxBracketController(
        ISaveTaxBracket saveHandler,
        IGetAllTaxBrackets getAllHandler,
        IGetTaxBracketById getByIdHandler,
        IDeleteTaxBracket deleteHandler,
        IGetPayrollDeductions deductionsHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TaxBracketDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<TaxBracketDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveTaxBracketDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveTaxBracketDto dto)
        { await saveHandler.SaveAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }

        /// <summary>HC232 — automated deductions preview: taxable gross → income tax + benefit contributions → net.</summary>
        [HttpGet("deductions")]
        public Task<PayrollDeductionsDto> GetDeductions([FromQuery] Guid employeeId) => deductionsHandler.GetAsync(employeeId);
    }

    public class EnrollmentActionRequest { public string? Remark { get; set; } }
    public class TerminateEnrollmentRequest { public DateTime CoverageEnd { get; set; } public string? Remark { get; set; } }

    // §3.10.1 CB4 — employee self-service: My Compensation + change/discrepancy requests.

    /// <summary>Employee self-service compensation views + requests (HC233/HC234).</summary>
    public class MyCompensationController(
        IGetMyCompensation myCompHandler,
        ISubmitCompensationRequest submitHandler,
        IGetCompensationRequests getRequestsHandler,
        IResolveCompensationRequest resolveHandler) : BaseController
    {
        /// <summary>HC233 — the signed-in employee's consolidated compensation (pay, allowances, benefits, deductions).</summary>
        [HttpGet]
        public Task<MyCompensationDto> GetMine() => myCompHandler.GetAsync();

        /// <summary>HC234 — raise a benefit-change or payroll-discrepancy request.</summary>
        [HttpPost("requests")]
        public async Task<IActionResult> Submit([FromBody] SubmitCompensationRequestDto dto) => Ok(new { id = await submitHandler.SubmitAsync(dto) });

        /// <summary>Role-scoped request list (employee own / manager subtree / HR all).</summary>
        [HttpGet("requests")]
        public Task<PaginatedResponse<CompensationRequestDto>> GetRequests([FromQuery] GetAllRequest request) => getRequestsHandler.GetAsync(request);

        [HttpPost("requests/{id:guid}/resolve")]
        public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveCompensationRequestDto dto)
        { await resolveHandler.ResolveAsync(id, dto); return Ok(new { message = "Request updated" }); }
    }
}
