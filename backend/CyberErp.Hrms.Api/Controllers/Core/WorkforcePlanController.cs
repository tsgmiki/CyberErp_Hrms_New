using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.WorkforcePlans;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Workforce Planning (HC053–HC076): versioned, scenario-tagged headcount and cost plans
    /// anchored to the approved establishment, with budget control, workflow approval and the
    /// approved-demand feed consumed by recruitment.
    /// </summary>
    public class WorkforcePlanController(
        ISaveWorkforcePlan saveHandler,
        IGetWorkforcePlanById getByIdHandler,
        IGetAllWorkforcePlans getAllHandler,
        IDeleteWorkforcePlan deleteHandler,
        ISubmitWorkforcePlan submitHandler,
        ICreateWorkforcePlanVersion versionHandler,
        IGetEstablishmentOverview establishmentHandler,
        IPopulateWorkforcePlan populateHandler,
        ISuggestPlanSeparations suggestHandler,
        IGetWorkforcePlanSummary summaryHandler,
        ICompareWorkforcePlans compareHandler,
        IGetApprovedDemand approvedDemandHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<WorkforcePlanDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>Live establishment: authorized / filled / vacant per unit × role (HC056).</summary>
        [HttpGet("establishment")]
        public Task<List<EstablishmentRowDto>> GetEstablishment([FromQuery] Guid? organizationUnitId)
            => establishmentHandler.GetAsync(organizationUnitId);

        /// <summary>Outstanding hiring demand of approved plans — the recruitment feed (HC075).</summary>
        [HttpGet("approved-demand")]
        public Task<List<ApprovedDemandRowDto>> GetApprovedDemand()
            => approvedDemandHandler.GetAsync();

        /// <summary>Side-by-side scenario comparison of 2–5 plans (HC068).</summary>
        [HttpGet("compare")]
        public Task<List<WorkforcePlanComparisonDto>> Compare([FromQuery] string ids)
            => compareHandler.GetAsync(
                (ids ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(Guid.Parse).ToList());

        [HttpGet("{id:guid}")]
        public Task<WorkforcePlanDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        /// <summary>Budget position + time-phased aggregates of one plan (HC069/HC073).</summary>
        [HttpGet("{id:guid}/summary")]
        public Task<WorkforcePlanSummaryDto> GetSummary(Guid id)
            => summaryHandler.GetAsync(id);

        /// <summary>Suggested per-line retirement counts within the plan horizon (HC060).</summary>
        [HttpGet("{id:guid}/suggest-separations")]
        public Task<List<SeparationSuggestionDto>> SuggestSeparations(Guid id)
            => suggestHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveWorkforcePlanDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveWorkforcePlanDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        /// <summary>Rebuilds the plan grid from the live establishment in the plan's scope (HC055).</summary>
        [HttpPost("{id:guid}/populate")]
        public async Task<IActionResult> Populate(Guid id)
        {
            var count = await populateHandler.PopulateAsync(id);
            return Ok(new { message = $"Populated {count} line(s) from the establishment." });
        }

        /// <summary>Submits for approval; over-threshold cost requires an escalation justification (HC066).</summary>
        [HttpPost("{id:guid}/submit")]
        public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitWorkforcePlanDto dto)
        {
            dto.Id = id;
            await submitHandler.SubmitAsync(dto);
            return Ok(new { message = "Workforce plan submitted" });
        }

        /// <summary>Creates the next editable version of a non-draft plan (HC071).</summary>
        [HttpPost("{id:guid}/new-version")]
        public async Task<Guid> NewVersion(Guid id)
            => await versionHandler.CreateAsync(id);

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
