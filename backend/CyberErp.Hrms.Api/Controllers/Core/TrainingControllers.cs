using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Training;
using CyberErp.Hrms.App.Features.Core.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Training & Development (§3.8, HC187–HC202) — Phase TD1: catalog + training needs.

    /// <summary>Training / education program categories (HC191).</summary>
    public class TrainingCategoryController(
        ISaveTrainingCategory saveHandler,
        IDeleteTrainingCategory deleteHandler,
        IGetTrainingCategoryById getByIdHandler,
        IGetAllTrainingCategories getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TrainingCategoryDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<TrainingCategoryDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveTrainingCategoryDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveTrainingCategoryDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>The course catalog / training directory (HC191/HC196; external providers per HC194).</summary>
    public class TrainingCourseController(
        ISaveTrainingCourse saveHandler,
        IDeleteTrainingCourse deleteHandler,
        IGetTrainingCourseById getByIdHandler,
        IGetAllTrainingCourses getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TrainingCourseDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<TrainingCourseDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveTrainingCourseDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveTrainingCourseDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>
    /// Training needs (HC187) routed through the per-type approval chain (HC188/HC201). Direct
    /// approve/reject serve the no-workflow mode; with a running instance decisions come from the
    /// My Approvals inbox.
    /// </summary>
    public class TrainingNeedController(
        ISaveTrainingNeed saveHandler,
        IDeleteTrainingNeed deleteHandler,
        ICancelTrainingNeed cancelHandler,
        IGetTrainingNeedById getByIdHandler,
        IGetAllTrainingNeeds getAllHandler,
        IGetTrainingNeedSuggestions suggestionsHandler,
        ITrainingNeedDecision decisionHandler,
        IPerformanceVisibilityService visibility,
        IWorkflowGate workflowGate) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TrainingNeedDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<TrainingNeedDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>HC189 — performance-driven suggestions (competency gaps, weak results, active goals).</summary>
        [HttpGet("suggestions")]
        public Task<List<TrainingNeedSuggestionDto>> Suggestions([FromQuery] Guid employeeId) =>
            suggestionsHandler.GetAsync(employeeId);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveTrainingNeedDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveTrainingNeedDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                return BadRequest(new { message = "Only HR administrators can approve training needs directly." });
            await workflowGate.EnsureNoRunningAsync("TrainingNeed", id);
            await decisionHandler.ApproveAsync(id);
            return Ok(new { message = "Training need approved" });
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var scope = await visibility.GetScopeAsync();
            if (!scope.IsAdmin)
                return BadRequest(new { message = "Only HR administrators can reject training needs directly." });
            await workflowGate.EnsureNoRunningAsync("TrainingNeed", id);
            await decisionHandler.RejectAsync(id);
            return Ok(new { message = "Training need rejected" });
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id) { await cancelHandler.CancelAsync(id); return Ok(new { message = "Training need cancelled" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Scheduled deliveries of catalog courses (HC197) — admin-managed, company-visible.</summary>
    public class TrainingSessionController(
        ISaveTrainingSession saveHandler,
        ICreateTrainingSessionSeries seriesHandler,
        IRescheduleTrainingSession rescheduleHandler,
        ICompleteTrainingSession completeHandler,
        ICancelTrainingSession cancelHandler,
        IDeleteTrainingSession deleteHandler,
        IGetTrainingSessionById getByIdHandler,
        IGetAllTrainingSessions getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TrainingSessionDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<TrainingSessionDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveTrainingSessionDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveTrainingSessionDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        /// <summary>HC197 — materialize a bounded recurring series (weekly/monthly).</summary>
        [HttpPost("series")]
        public async Task<IActionResult> CreateSeries([FromBody] CreateSessionSeriesDto dto) => Ok(new { ids = await seriesHandler.CreateAsync(dto) });

        [HttpPost("{id:guid}/reschedule")]
        public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleSessionDto dto)
        { await rescheduleHandler.RescheduleAsync(id, dto); return Ok(new { message = "Session rescheduled" }); }

        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id) { await completeHandler.CompleteAsync(id); return Ok(new { message = "Session completed" }); }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id) { await cancelHandler.CancelAsync(id); return Ok(new { message = "Session cancelled" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Participation tracking (HC198) + effectiveness feedback (HC199).</summary>
    public class TrainingEnrollmentController(
        IEnrollTraining enrollHandler,
        IRecordTrainingParticipation participationHandler,
        ISubmitTrainingFeedback feedbackHandler,
        IWithdrawTrainingEnrollment withdrawHandler,
        IDeleteTrainingEnrollment deleteHandler,
        IGetAllTrainingEnrollments getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TrainingEnrollmentDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollTrainingDto dto) => Ok(new { id = await enrollHandler.EnrollAsync(dto) });

        /// <summary>Attendance, completion state and assessment score — HR / the employee's manager.</summary>
        [HttpPut("participation")]
        public async Task<IActionResult> RecordParticipation([FromBody] RecordParticipationDto dto)
        { await participationHandler.RecordAsync(dto); return Ok(new { message = "Participation recorded" }); }

        /// <summary>HC199 — participant-only effectiveness feedback.</summary>
        [HttpPost("{id:guid}/feedback")]
        public async Task<IActionResult> Feedback(Guid id, [FromBody] TrainingFeedbackDto dto)
        { await feedbackHandler.SubmitAsync(id, dto); return Ok(new { message = "Feedback recorded" }); }

        [HttpPost("{id:guid}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid id) { await withdrawHandler.WithdrawAsync(id); return Ok(new { message = "Enrollment withdrawn" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Structured learning paths aligned to career progression (HC193).</summary>
    public class LearningPathController(
        ISaveLearningPath saveHandler,
        IDeleteLearningPath deleteHandler,
        IGetLearningPathById getByIdHandler,
        IGetAllLearningPaths getAllHandler,
        IGetLearningPathProgress progressHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<LearningPathDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<LearningPathDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>An employee's completion progress along the path (visibility-gated).</summary>
        [HttpGet("{id:guid}/progress")]
        public Task<LearningPathProgressDto> Progress(Guid id, [FromQuery] Guid employeeId) =>
            progressHandler.GetAsync(id, employeeId);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveLearningPathDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveLearningPathDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Certification management (HC200): issue, renew, expiry tracking.</summary>
    public class TrainingCertificateController(
        IIssueTrainingCertificate issueHandler,
        ISaveTrainingCertificate saveHandler,
        IRenewTrainingCertificate renewHandler,
        IDeleteTrainingCertificate deleteHandler,
        IGetAllTrainingCertificates getAllHandler,
        IGetExpiringTrainingCertificates expiringHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TrainingCertificateDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        /// <summary>Renewal tracking — certificates lapsing within the window, soonest first.</summary>
        [HttpGet("expiring")]
        public Task<List<TrainingCertificateDto>> Expiring([FromQuery] int days = 90) => expiringHandler.GetAsync(days);

        /// <summary>Issues the certificate for a COMPLETED enrollment (idempotent per enrollment).</summary>
        [HttpPost("issue")]
        public async Task<IActionResult> Issue([FromBody] IssueCertificateDto dto) => Ok(new { id = await issueHandler.IssueAsync(dto) });

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveTrainingCertificateDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveTrainingCertificateDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPost("{id:guid}/renew")]
        public async Task<IActionResult> Renew(Guid id, [FromBody] RenewCertificateDto dto)
        { await renewHandler.RenewAsync(id, dto); return Ok(new { message = "Certificate renewed" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>CPD credits/hours rollup (HC200) — own record by default, scope-gated otherwise.</summary>
    public class TrainingCpdController(IGetCpdSummary cpdHandler) : BaseController
    {
        [HttpGet]
        public Task<CpdSummaryDto> Get([FromQuery] Guid? employeeId, [FromQuery] int? year) =>
            cpdHandler.GetAsync(employeeId, year);
    }

    /// <summary>Provider-payment hand-off for finance (HC202) — admin-only.</summary>
    public class TrainingProviderPaymentController(
        IGetAllProviderPayments getAllHandler,
        IMarkProviderPaymentPaid payHandler,
        IExportProviderPayments exportHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TrainingProviderPaymentDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpPost("{id:guid}/mark-paid")]
        public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkProviderPaymentPaidDto dto)
        { await payHandler.PayAsync(id, dto); return Ok(new { message = "Marked as paid" }); }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] GetAllRequest request)
        {
            var csv = await exportHandler.ExportCsvAsync(request);
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "training-provider-payments.csv");
        }
    }

    /// <summary>
    /// Learning communities + discussion threads (HC198/HC199): any employee founds/joins; reading is
    /// open, posting needs membership; founders moderate.
    /// </summary>
    public class LearningCommunityController(
        ISaveLearningCommunity saveHandler,
        IDeleteLearningCommunity deleteHandler,
        IJoinLearningCommunity joinHandler,
        ILeaveLearningCommunity leaveHandler,
        IGetAllLearningCommunities getAllHandler,
        IGetCommunityPosts postsHandler,
        ICreateCommunityPost createPostHandler,
        IDeleteCommunityPost deletePostHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<LearningCommunityDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveLearningCommunityDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveLearningCommunityDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPost("{id:guid}/join")]
        public async Task<IActionResult> Join(Guid id) { await joinHandler.JoinAsync(id); return Ok(new { message = "Joined" }); }

        [HttpPost("{id:guid}/leave")]
        public async Task<IActionResult> Leave(Guid id) { await leaveHandler.LeaveAsync(id); return Ok(new { message = "Left the community" }); }

        [HttpGet("{id:guid}/posts")]
        public Task<PaginatedResponse<CommunityPostDto>> Posts(Guid id, [FromQuery] GetAllRequest request) =>
            postsHandler.GetAsync(id, request);

        [HttpPost("{id:guid}/posts")]
        public async Task<IActionResult> CreatePost(Guid id, [FromBody] CreateCommunityPostDto dto) =>
            Ok(new { id = await createPostHandler.CreateAsync(id, dto) });

        [HttpDelete("posts/{postId:guid}")]
        public async Task<IActionResult> DeletePost(Guid postId) { await deletePostHandler.DeleteAsync(postId); return Ok(new { message = "Post deleted" }); }

        /// <summary>HC207-b — toggles the caller's reaction on a post; returns the new state.</summary>
        [HttpPost("posts/{postId:guid}/react")]
        public async Task<IActionResult> React(Guid postId, [FromServices] IReactToCommunityPost reactHandler) =>
            Ok(new { liked = await reactHandler.ToggleAsync(postId) });

        /// <summary>HC208-b — forum engagement analytics (HR).</summary>
        [HttpGet("analytics")]
        public Task<CommunityAnalyticsDto> Analytics([FromServices] IGetCommunityAnalytics analyticsHandler) =>
            analyticsHandler.GetAsync();

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Training budgets + utilization (HC190) — admin-only.</summary>
    public class TrainingBudgetController(
        ISaveTrainingBudget saveHandler,
        IDeleteTrainingBudget deleteHandler,
        IGetAllTrainingBudgets getAllHandler,
        IGetTrainingBudgetUtilization utilizationHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<TrainingBudgetDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("utilization")]
        public Task<TrainingBudgetUtilizationDto> Utilization([FromQuery] int fiscalYear, [FromQuery] Guid? organizationUnitId) =>
            utilizationHandler.GetAsync(fiscalYear, organizationUnitId);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveTrainingBudgetDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveTrainingBudgetDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }
}
