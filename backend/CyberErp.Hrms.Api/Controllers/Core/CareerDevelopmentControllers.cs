using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.CareerDevelopment;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Career Development §3.7.A — Succession Planning (HC148–HC160).

    /// <summary>Critical positions flagged for succession (HC151).</summary>
    public class CriticalPositionController(
        ISaveCriticalPosition saveHandler,
        IDeleteCriticalPosition deleteHandler,
        IGetCriticalPositionById getByIdHandler,
        IGetAllCriticalPositions getAllHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<CriticalPositionDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<CriticalPositionDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveCriticalPositionDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveCriticalPositionDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Talent-review / calibration sessions + the 9-box grid (HC148–HC150).</summary>
    public class TalentReviewController(
        ISaveTalentReview saveHandler,
        IDeleteTalentReview deleteHandler,
        IGetTalentReviewById getByIdHandler,
        IGetAllTalentReviews getAllHandler,
        IGetTalentReviewNineBox nineBoxHandler,
        IIdentifyHiPos identifyHiPosHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<TalentReviewDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<TalentReviewDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpGet("{id:guid}/nine-box")] public Task<NineBoxDto> NineBox(Guid id) => nineBoxHandler.GetAsync(id);
        /// <summary>HC148 — auto-flag the top-box (High × High) assessments as high-potentials.</summary>
        [HttpPost("{id:guid}/identify-hipos")] public Task<IdentifyHiPosResultDto> IdentifyHiPos(Guid id) => identifyHiPosHandler.IdentifyAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveTalentReviewDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveTalentReviewDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Per-employee 9-box placements with multi-rater inputs (HC148, HC149). Filter by ?parentId=talentReviewId.</summary>
    public class TalentAssessmentController(
        ISaveTalentAssessment saveHandler,
        IDeleteTalentAssessment deleteHandler,
        IGetTalentAssessmentById getByIdHandler,
        IGetAllTalentAssessments getAllHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<TalentAssessmentDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<TalentAssessmentDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveTalentAssessmentDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveTalentAssessmentDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Succession plans per critical position + the succession chart (HC152, HC157, HC159). Filter by ?parentId=criticalPositionId.</summary>
    public class SuccessionPlanController(
        ISaveSuccessionPlan saveHandler,
        IDeleteSuccessionPlan deleteHandler,
        IGetSuccessionPlanById getByIdHandler,
        IGetAllSuccessionPlans getAllHandler,
        IGetSuccessionChart chartHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<SuccessionPlanDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<SuccessionPlanDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpGet("{id:guid}/chart")] public Task<SuccessionChartDto> Chart(Guid id) => chartHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveSuccessionPlanDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveSuccessionPlanDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Ranked successors + development actions / knowledge transfer + competency gap (HC153–HC156, HC160). Filter by ?parentId=successionPlanId.</summary>
    public class SuccessionCandidateController(
        ISaveSuccessionCandidate saveHandler,
        IDeleteSuccessionCandidate deleteHandler,
        IGetSuccessionCandidateById getByIdHandler,
        IGetAllSuccessionCandidates getAllHandler,
        IGetSuccessionCandidateGap gapHandler,
        IComputeSuccessionCandidateReadiness readinessHandler,
        IGetSuccessionCandidateProfile profileHandler,
        ICreateDevelopmentPlanFromGap createPlanHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<SuccessionCandidateDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<SuccessionCandidateDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpGet("{id:guid}/gap")] public Task<CompetencyGapDto> Gap(Guid id) => gapHandler.GetAsync(id);
        /// <summary>HC153 — recompute the successor's readiness from performance + competency data.</summary>
        [HttpPost("{id:guid}/compute-readiness")] public Task<ReadinessComputationDto> ComputeReadiness(Guid id) => readinessHandler.ComputeAsync(id);
        /// <summary>HC158 — holistic candidate view (readiness + performance summary + gap).</summary>
        [HttpGet("{id:guid}/profile")] public Task<SuccessionCandidateProfileDto> Profile(Guid id) => profileHandler.GetAsync(id);
        /// <summary>HC155/HC156 — turn the successor's competency gap into a structured Individual Development Plan.</summary>
        [HttpPost("{id:guid}/create-development-plan")] public Task<CreateDevelopmentPlanResultDto> CreatePlan(Guid id) => createPlanHandler.FromSuccessionCandidateAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveSuccessionCandidateDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveSuccessionCandidateDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }
}
