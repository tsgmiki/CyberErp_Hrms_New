using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Performance;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Performance Management (HC118–HC147) — Phase A configuration endpoints.

    /// <summary>Rating scales / scoring frameworks (HC138) with their level bands.</summary>
    public class RatingScaleController(
        ISaveRatingScale saveHandler,
        IDeleteRatingScale deleteHandler,
        IGetRatingScaleById getByIdHandler,
        IGetAllRatingScales getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<RatingScaleDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<RatingScaleDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveRatingScaleDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveRatingScaleDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Configurable competency categories (HC125).</summary>
    public class CompetencyCategoryController(
        ICreateCompetencyCategory createHandler,
        IUpdateCompetencyCategory updateHandler,
        IDeleteCompetencyCategory deleteHandler,
        IGetCompetencyCategoryById getByIdHandler,
        IGetAllCompetencyCategories getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<CompetencyCategoryDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<CompetencyCategoryDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateCompetencyCategoryDto dto) => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCompetencyCategoryDto dto) { await updateHandler.UpdateAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Competency library (HC123–HC125).</summary>
    public class CompetencyController(
        ICreateCompetency createHandler,
        IUpdateCompetency updateHandler,
        IDeleteCompetency deleteHandler,
        IGetCompetencyById getByIdHandler,
        IGetAllCompetencies getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<CompetencyDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<CompetencyDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateCompetencyDto dto) => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCompetencyDto dto) { await updateHandler.UpdateAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Competencies assigned to a position with weights (HC123/HC124).</summary>
    public class PositionCompetencyController(
        IGetPositionCompetencies getHandler,
        ISavePositionCompetencies saveHandler) : BaseController
    {
        [HttpGet]
        public Task<List<PositionCompetencyDto>> Get([FromQuery] Guid positionId) => getHandler.GetAsync(positionId);

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SavePositionCompetenciesDto dto) { await saveHandler.SaveAsync(dto); return Ok(new { message = "Saved successfully" }); }
    }

    /// <summary>Configurable appraisal cycles (HC126–HC128).</summary>
    public class ReviewCycleController(
        ISaveReviewCycle saveHandler,
        IDeleteReviewCycle deleteHandler,
        IGetReviewCycleById getByIdHandler,
        IGetAllReviewCycles getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<ReviewCycleDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<ReviewCycleDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveReviewCycleDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveReviewCycleDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Organizational objectives — cascade hierarchy that goals align to (HC118/HC120/HC122).</summary>
    public class OrganizationalObjectiveController(
        ISaveOrganizationalObjective saveHandler,
        IDeleteOrganizationalObjective deleteHandler,
        IGetOrganizationalObjectiveById getByIdHandler,
        IGetAllOrganizationalObjectives getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<OrganizationalObjectiveDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<OrganizationalObjectiveDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveOrganizationalObjectiveDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveOrganizationalObjectiveDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Individual employee goals with SMART criteria + action plans (HC119–HC122).</summary>
    public class EmployeeGoalController(
        ISaveEmployeeGoal saveHandler,
        IDeleteEmployeeGoal deleteHandler,
        IGetEmployeeGoalById getByIdHandler,
        IGetAllEmployeeGoals getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<EmployeeGoalDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<EmployeeGoalDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveEmployeeGoalDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeGoalDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Scored appraisals — generate, self/manager scoring, stage flow, completion (HC127/HC138).</summary>
    public class AppraisalController(
        IGenerateAppraisal generateHandler,
        ISaveAppraisalScores saveScoresHandler,
        ISubmitAppraisalSelfAssessment submitHandler,
        ICompleteAppraisal completeHandler,
        IDeleteAppraisal deleteHandler,
        IGetAppraisalById getByIdHandler,
        IGetAllAppraisals getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AppraisalDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AppraisalDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateAppraisalDto dto) => Ok(new { id = await generateHandler.GenerateAsync(dto) });

        [HttpPut("score")]
        public async Task<IActionResult> Score([FromBody] SaveAppraisalScoresDto dto) { await saveScoresHandler.SaveAsync(dto); return Ok(new { message = "Saved successfully" }); }

        [HttpPost("{id:guid}/submit-self")]
        public async Task<IActionResult> SubmitSelf(Guid id) { await submitHandler.SubmitAsync(id); return Ok(new { message = "Submitted for manager review" }); }

        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id) { await completeHandler.CompleteAsync(id); return Ok(new { message = "Appraisal completed" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Employee acknowledgment / signing of completed appraisals (HC142–HC143, HC146).</summary>
    public class AppraisalSignatureController(
        IAcknowledgeAppraisal acknowledgeHandler,
        IManagerSignAppraisal managerSignHandler) : BaseController
    {
        [HttpPost("acknowledge")]
        public async Task<IActionResult> Acknowledge([FromBody] SignAppraisalDto dto) { await acknowledgeHandler.AcknowledgeAsync(dto); return Ok(new { message = "Acknowledged" }); }

        [HttpPost("manager-sign")]
        public async Task<IActionResult> ManagerSign([FromBody] SignAppraisalDto dto) { await managerSignHandler.SignAsync(dto); return Ok(new { message = "Signed" }); }
    }

    /// <summary>Appraisal appeals and their HR/management review (HC143–HC144).</summary>
    public class AppraisalAppealController(
        ISubmitAppraisalAppeal submitHandler,
        IStartAppraisalAppealReview startHandler,
        IResolveAppraisalAppeal resolveHandler,
        IGetAppraisalAppealById getByIdHandler,
        IGetAllAppraisalAppeals getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AppraisalAppealDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AppraisalAppealDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitAppraisalAppealDto dto) => Ok(new { id = await submitHandler.SubmitAsync(dto) });

        [HttpPost("{id:guid}/start-review")]
        public async Task<IActionResult> StartReview(Guid id) { await startHandler.StartAsync(id); return Ok(new { message = "Under review" }); }

        [HttpPost("resolve")]
        public async Task<IActionResult> Resolve([FromBody] ResolveAppraisalAppealDto dto) { await resolveHandler.ResolveAsync(dto); return Ok(new { message = "Appeal resolved" }); }
    }

    /// <summary>Peer assessment of appraisals (HC127).</summary>
    public class AppraisalPeerController(
        IInviteAppraisalPeers inviteHandler,
        ISubmitAppraisalPeerReview submitHandler,
        IRemoveAppraisalPeerReview removeHandler,
        IGetAppraisalPeerReviews getHandler) : BaseController
    {
        [HttpGet]
        public Task<List<AppraisalPeerReviewDto>> Get([FromQuery] Guid appraisalId) => getHandler.GetAsync(appraisalId);

        [HttpPost("invite")]
        public async Task<IActionResult> Invite([FromBody] InviteAppraisalPeersDto dto) { await inviteHandler.InviteAsync(dto); return Ok(new { message = "Peers invited" }); }

        [HttpPut("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitAppraisalPeerReviewDto dto) { await submitHandler.SubmitAsync(dto); return Ok(new { message = "Peer review submitted" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Remove(Guid id) { await removeHandler.RemoveAsync(id); return Ok(new { message = "Removed successfully" }); }
    }

    /// <summary>Calibration &amp; moderation sessions (HC128–HC129).</summary>
    public class CalibrationSessionController(
        ICreateCalibrationSession createHandler,
        ISaveCalibrationItem saveItemHandler,
        IFinalizeCalibrationSession finalizeHandler,
        IDeleteCalibrationSession deleteHandler,
        IGetCalibrationSessionById getByIdHandler,
        IGetAllCalibrationSessions getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<CalibrationSessionDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<CalibrationSessionDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCalibrationSessionDto dto) => Ok(new { id = await createHandler.CreateAsync(dto) });

        [HttpPut("item")]
        public async Task<IActionResult> SaveItem([FromBody] SaveCalibrationItemDto dto) { await saveItemHandler.SaveAsync(dto); return Ok(new { message = "Saved successfully" }); }

        [HttpPost("{id:guid}/finalize")]
        public async Task<IActionResult> Finalize(Guid id) { await finalizeHandler.FinalizeAsync(id); return Ok(new { message = "Calibration finalized" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Manager / HR performance dashboard (HC134).</summary>
    public class PerformanceDashboardController(IGetPerformanceDashboard getHandler) : BaseController
    {
        [HttpGet]
        public Task<PerformanceDashboardDto> Get([FromQuery] GetAllRequest request) => getHandler.GetAsync(request);
    }

    /// <summary>Unified per-employee performance summary for cross-module use (HC147).</summary>
    public class EmployeePerformanceSummaryController(IGetEmployeePerformanceSummary getHandler) : BaseController
    {
        [HttpGet]
        public Task<EmployeePerformanceSummaryDto> Get([FromQuery] Guid employeeId) => getHandler.GetAsync(employeeId);
    }

    /// <summary>Append-only performance version history / audit trail (HC132).</summary>
    public class PerformanceHistoryController(IGetPerformanceHistory getHandler) : BaseController
    {
        [HttpGet]
        public Task<List<PerformanceHistoryDto>> Get([FromQuery] string entityType, [FromQuery] Guid entityId) => getHandler.GetAsync(entityType, entityId);
    }

    /// <summary>Individual Development Plans (HC130–HC131).</summary>
    public class DevelopmentPlanController(
        ISaveDevelopmentPlan saveHandler,
        IDeleteDevelopmentPlan deleteHandler,
        IGetDevelopmentPlanById getByIdHandler,
        IGetAllDevelopmentPlans getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<DevelopmentPlanDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<DevelopmentPlanDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveDevelopmentPlanDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveDevelopmentPlanDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Performance Improvement Plans (HC135).</summary>
    public class ImprovementPlanController(
        ISaveImprovementPlan saveHandler,
        IRecordImprovementPlanOutcome outcomeHandler,
        IDeleteImprovementPlan deleteHandler,
        IGetImprovementPlanById getByIdHandler,
        IGetAllImprovementPlans getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<ImprovementPlanDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<ImprovementPlanDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveImprovementPlanDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveImprovementPlanDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPost("outcome")]
        public async Task<IActionResult> RecordOutcome([FromBody] RecordImprovementPlanOutcomeDto dto) { await outcomeHandler.RecordAsync(dto); return Ok(new { message = "Outcome recorded" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Employee achievements / milestones log (HC139–HC140).</summary>
    public class AchievementController(
        ISaveAchievement saveHandler,
        IDeleteAchievement deleteHandler,
        IGetAchievementById getByIdHandler,
        IGetAllAchievements getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AchievementDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AchievementDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveAchievementDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveAchievementDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Recognition badge / award catalog (HC141).</summary>
    public class RecognitionBadgeController(
        ICreateRecognitionBadge createHandler,
        IUpdateRecognitionBadge updateHandler,
        IDeleteRecognitionBadge deleteHandler,
        IGetRecognitionBadgeById getByIdHandler,
        IGetAllRecognitionBadges getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<RecognitionBadgeDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<RecognitionBadgeDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateRecognitionBadgeDto dto) => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRecognitionBadgeDto dto) { await updateHandler.UpdateAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Recognitions granted to employees — the recognition board (HC141).</summary>
    public class RecognitionController(
        ISaveEmployeeRecognition saveHandler,
        IDeleteEmployeeRecognition deleteHandler,
        IGetEmployeeRecognitionById getByIdHandler,
        IGetAllEmployeeRecognitions getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<EmployeeRecognitionDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<EmployeeRecognitionDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveEmployeeRecognitionDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeRecognitionDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Appraisal form templates — goals/competencies weight split (HC138).</summary>
    public class AppraisalTemplateController(
        ICreateAppraisalTemplate createHandler,
        IUpdateAppraisalTemplate updateHandler,
        IDeleteAppraisalTemplate deleteHandler,
        IGetAppraisalTemplateById getByIdHandler,
        IGetAllAppraisalTemplates getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AppraisalTemplateDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<AppraisalTemplateDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateAppraisalTemplateDto dto) => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAppraisalTemplateDto dto) { await updateHandler.UpdateAsync(dto); return Ok(new { message = "Updated successfully" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }
}
