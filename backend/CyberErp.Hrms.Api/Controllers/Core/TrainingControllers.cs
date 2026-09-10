using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Performance;
using CyberErp.Hrms.App.Features.Core.Training;
using CyberErp.Hrms.App.Features.Core.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Training & Development (§3.8, HC187–HC202) — Phase TD1: catalog + training needs.

    /// <summary>Training / education program categories (HC191).</summary>
    [RequirePermission("trainingCategory")]
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
    /// <summary>
    /// Which competencies a course develops — the mapping that lets a competency gap name a course.
    /// Gated with the course catalogue, since it is part of describing a course.
    /// </summary>
    [RequirePermission("trainingCourse")]
    public class CourseCompetencyController(
        IGetCourseCompetencies getHandler,
        ISetCourseCompetencies setHandler) : BaseController
    {
        [HttpGet]
        public Task<List<CourseCompetencyDto>> GetByCourse([FromQuery] Guid trainingCourseId)
            => getHandler.GetAsync(trainingCourseId);

        /// <summary>Replaces the course's whole mapping — set semantics, like LearningPath steps.</summary>
        [HttpPut]
        public async Task<IActionResult> Set([FromBody] SetCourseCompetenciesDto dto)
        {
            await setHandler.SetAsync(dto);
            return Ok(new { message = "Competencies updated" });
        }
    }

    [RequirePermission("trainingCourse")]
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
    [RequirePermission("trainingNeed")]
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
    [RequirePermission("trainingSession")]
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
    [RequirePermission("trainingSession", "myTraining")]
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

        /// <summary>
        /// Withdraw from a session. Requires <b>Add</b>, not the derived Edit.
        ///
        /// <para>⚠️ Withdrawing is the other half of enrolling — one self-service capability — but the
        /// suffix "withdraw" is not an Add token, so the verb derivation asks for Edit. Edit on THIS
        /// controller pair also means RecordParticipation and certificate issue/renew, which are HR
        /// functions, so granting staff Edit to reach this one action would open all three. Naming
        /// the privilege here is the split the derivation cannot express; the handler still restricts
        /// the row to its owner, their manager, or HR.</para>
        ///
        /// <para>The action attribute REPLACES the controller's, so both operation links are
        /// repeated — dropping them would gate this on nothing.</para>
        /// </summary>
        [HttpPost("{id:guid}/withdraw")]
        [RequirePermission("trainingSession", "myTraining", Access = PermissionAccess.Add)]
        public async Task<IActionResult> Withdraw(Guid id) { await withdrawHandler.WithdrawAsync(id); return Ok(new { message = "Enrollment withdrawn" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Structured learning paths aligned to career progression (HC193).</summary>
    [RequirePermission("learningPath")]
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
    [RequirePermission("trainingCertificate", "myTraining")]
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

    /// <summary>
    /// Course content authoring — versions and their modules. Gated with the course catalogue,
    /// since authoring content is part of describing a course.
    /// </summary>
    [RequirePermission("trainingCourse")]
    public class CourseVersionController(
        IGetCourseVersions getHandler,
        ICreateCourseVersion createHandler,
        ISetCourseVersionModules setModulesHandler,
        IPublishCourseVersion publishHandler) : BaseController
    {
        [HttpGet]
        public Task<List<CourseVersionDto>> GetByCourse([FromQuery] Guid trainingCourseId)
            => getHandler.GetAsync(trainingCourseId);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseVersionRequest request)
            => Ok(new { id = await createHandler.CreateAsync(request.TrainingCourseId, request.ChangeNote) });

        /// <summary>Replaces the draft's ordered module list — set semantics.</summary>
        [HttpPut("modules")]
        public async Task<IActionResult> SetModules([FromBody] SaveCourseVersionModulesDto dto)
        {
            await setModulesHandler.SetAsync(dto);
            return Ok(new { message = "Content updated" });
        }

        /// <summary>Makes the draft live and retires the version it replaces.</summary>
        [HttpPost("{id:guid}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            await publishHandler.PublishAsync(id);
            return Ok(new { message = "Version published" });
        }
    }

    public class CreateCourseVersionRequest
    {
        public Guid TrainingCourseId { get; set; }
        public string? ChangeNote { get; set; }
    }

    /// <summary>
    /// The learner's course player. Gated on <c>myTraining</c> — this is a learner surface — and the
    /// handlers additionally require the enrolment to be the caller's OWN.
    /// </summary>
    [RequirePermission("myTraining")]
    public class CoursePlayerController(
        IGetCoursePlayer playerHandler,
        IRecordModuleProgress progressHandler) : BaseController
    {
        [HttpGet("{trainingEnrollmentId:guid}")]
        public Task<CoursePlayerDto> Get(Guid trainingEnrollmentId)
            => playerHandler.GetAsync(trainingEnrollmentId);

        /// <summary>
        /// Records a visit and returns the refreshed player, so the caller never has to guess whether
        /// that module was the one that completed the course.
        /// </summary>
        [HttpPost("progress")]
        [RequirePermission("myTraining", Access = PermissionAccess.Add)]
        public Task<CoursePlayerDto> Progress([FromBody] RecordModuleProgressDto dto)
            => progressHandler.RecordAsync(dto);
    }

    /// <summary>
    /// The learner's own training records and their signature on them (logic §12.90).
    ///
    /// <para>⚠️ Every signing re-authenticates. The password travels in the body of a POST over the
    /// same session the caller already holds — that is the point: Part 11 wants a signing to be an
    /// act of identity rather than a click by whoever is at the keyboard.</para>
    /// </summary>
    [RequirePermission("myTraining")]
    public class MyTrainingRecordController(
        IGetMySignableRecords listHandler,
        IGetSignableRecord getHandler,
        ISignTrainingRecord signHandler) : BaseController
    {
        [HttpGet]
        public Task<List<SignableRecordDto>> GetAll() => listHandler.GetAsync();

        [HttpGet("{trainingEnrollmentId:guid}")]
        public Task<SignableRecordDto> Get(Guid trainingEnrollmentId)
            => getHandler.GetAsync(trainingEnrollmentId);

        /// <summary>The learner's own attestation that they completed the training.</summary>
        [HttpPost("sign")]
        [RequirePermission("myTraining", Access = PermissionAccess.Add)]
        public Task<SignableRecordDto> Sign([FromBody] SignRecordDto dto) => signHandler.SignAsync(dto);
    }

    /// <summary>
    /// HR's view of a training record, and the verifier's counter-signature (logic §12.90).
    /// </summary>
    [RequirePermission("learningCompliance")]
    public class TrainingRecordController(
        IGetSignableRecord getHandler,
        IVerifyTrainingRecord verifyHandler,
        IGetTrainingRecordDocument documentHandler) : BaseController
    {
        [HttpGet("{trainingEnrollmentId:guid}")]
        public Task<SignableRecordDto> Get(Guid trainingEnrollmentId)
            => getHandler.GetAsync(trainingEnrollmentId);

        /// <summary>Confirms someone else's record. A verifier cannot sign their own.</summary>
        [HttpPost("verify")]
        [RequirePermission("learningCompliance", Access = PermissionAccess.Edit)]
        public Task<SignableRecordDto> Verify([FromBody] SignRecordDto dto) => verifyHandler.VerifyAsync(dto);

        /// <summary>
        /// The human-readable record copy an inspection asks for — generated from the data, with the
        /// signature manifestations on it.
        /// </summary>
        [HttpGet("{employeeId:guid}/document")]
        public async Task<IActionResult> Document(Guid employeeId)
        {
            var (content, fileName) = await documentHandler.GetAsync(employeeId);
            return File(content, "application/pdf", fileName);
        }
    }

    /// <summary>
    /// A course's material library (logic §12.89) — the store that makes the Document module kind
    /// reachable. Gated with the course catalogue, since uploading course material is authoring.
    /// </summary>
    [RequirePermission("trainingCourse")]
    public class CourseFileController(
        IGetCourseFiles listHandler,
        IUploadCourseFile uploadHandler,
        IDeleteCourseFile deleteHandler,
        IDownloadCourseFile downloadHandler) : BaseController
    {
        [HttpGet]
        public Task<List<CourseFileDto>> GetByCourse([FromQuery] Guid trainingCourseId)
            => listHandler.GetAsync(trainingCourseId);

        /// <summary>Adds a file (PDF/Office/text/image, max 25 MB) to the course's library.</summary>
        [HttpPost]
        public async Task<IActionResult> Upload(
            [FromForm] Guid trainingCourseId, [FromForm] string? description, IFormFile file)
        {
            if (file is null) return BadRequest(new { message = "No file provided." });
            await using var stream = file.OpenReadStream();
            var id = await uploadHandler.UploadAsync(trainingCourseId, stream, file.FileName,
                file.ContentType, file.Length, description);
            return Ok(new { id, message = "File uploaded" });
        }

        /// <summary>The author's own preview of the material.</summary>
        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id)
        {
            var (content, contentType, fileName) = await downloadHandler.GetAsync(id);
            return File(content, contentType, fileName);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>
    /// The learner's copy of a course's material.
    ///
    /// <para>⚠️ A SEPARATE, READ-ONLY controller for the same reason the catalogue is one (§12.85):
    /// it is gated on <c>myTraining</c>, which ordinary staff hold, and widening the authoring
    /// controller to that permission would hand every employee upload and delete along with it. The
    /// handler additionally requires the file to be served by the PUBLISHED version of a course the
    /// caller is enrolled on.</para>
    /// </summary>
    [RequirePermission("myTraining")]
    public class CourseMaterialController(IDownloadCourseMaterial handler) : BaseController
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var (content, contentType, fileName) = await handler.GetAsync(id);
            // Inline, so a PDF opens in the player's viewer instead of landing in the downloads
            // folder — the learner is reading it as part of a course, not filing it.
            Response.Headers.ContentDisposition = $"inline; filename=\"{fileName}\"";
            return File(content, contentType);
        }
    }

    /// <summary>
    /// Mandatory-training rules and the obligations they produce (logic §12.88).
    ///
    /// <para>Its own operation rather than an extension of the course catalogue: assigning training
    /// to a population and waiving someone's obligation are compliance acts, and the people who
    /// author a course are not automatically the people who should be doing them.</para>
    /// </summary>
    [RequirePermission("learningCompliance")]
    public class LearningAssignmentController(
        IGetLearningAssignments listHandler,
        IGetLearningAssignment getHandler,
        ISaveLearningAssignment saveHandler,
        IDeleteLearningAssignment deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<LearningAssignmentDto>> GetAll([FromQuery] GetAllRequest request)
            => listHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<LearningAssignmentDto> Get(Guid id) => getHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveLearningAssignmentDto dto)
            => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveLearningAssignmentDto dto)
            => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>
    /// The compliance picture: who owes what, where the organisation stands, and whether the training
    /// is doing anything (logic §12.88).
    /// </summary>
    [RequirePermission("learningCompliance")]
    public class LearningComplianceController(
        IGetObligations obligationsHandler,
        IWaiveObligation waiveHandler,
        IGetComplianceOverview overviewHandler,
        IGetTrainingEffectiveness effectivenessHandler,
        ILearningComplianceChaser chaser) : BaseController
    {
        [HttpGet("obligations")]
        public Task<PaginatedResponse<ObligationDto>> Obligations([FromQuery] GetAllRequest request)
            => obligationsHandler.GetAsync(request);

        [HttpGet("overview")]
        public Task<ComplianceOverviewDto> Overview([FromQuery] Guid? trainingCourseId)
            => overviewHandler.GetAsync(trainingCourseId);

        [HttpGet("effectiveness")]
        public Task<List<EffectivenessRowDto>> Effectiveness([FromQuery] Guid? trainingCourseId)
            => effectivenessHandler.GetAsync(trainingCourseId);

        /// <summary>Excuses one obligation, with a reason an audit can read.</summary>
        [HttpPost("obligations/{id:guid}/waive")]
        [RequirePermission("learningCompliance", Access = PermissionAccess.Edit)]
        public async Task<IActionResult> Waive(Guid id, [FromBody] WaiveObligationRequest request)
        { await waiveHandler.WaiveAsync(id, request.Reason); return Ok(new { message = "Obligation waived" }); }

        /// <summary>
        /// Runs the sweep now instead of waiting for tonight.
        ///
        /// <para>⚠️ <c>RunAsync</c>, which carries the HR guard — this messages the whole workforce.
        /// The nightly Hangfire job calls <c>RunUnattendedAsync</c> instead, because a background job
        /// has no signed-in user to satisfy that guard (§12.73).</para>
        /// </summary>
        [HttpPost("run")]
        [RequirePermission("learningCompliance", Access = PermissionAccess.Add)]
        public async Task<IActionResult> Run() => Ok(await chaser.RunAsync());
    }

    public class WaiveObligationRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// The learner's own mandatory training. Gated on <c>myTraining</c>, and the handler answers only
    /// for the signed-in employee.
    /// </summary>
    [RequirePermission("myTraining")]
    public class MyObligationsController(IGetMyObligations handler) : BaseController
    {
        [HttpGet]
        public Task<List<ObligationDto>> Get() => handler.GetAsync();
    }

    /// <summary>
    /// Question banks — the reusable question library (logic §12.87). Gated with the course
    /// catalogue, since writing questions is course authoring.
    /// </summary>
    [RequirePermission("questionBank")]
    public class QuestionBankController(
        IGetQuestionBanks listHandler,
        IGetQuestionBank getHandler,
        ISaveQuestionBank saveHandler,
        IDeleteQuestionBank deleteHandler,
        IGetBankQuestions questionsHandler,
        ISetBankQuestions setQuestionsHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<QuestionBankDto>> GetAll([FromQuery] GetAllRequest request)
            => listHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<QuestionBankDto> Get(Guid id) => getHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveQuestionBankDto dto)
            => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveQuestionBankDto dto)
            => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }

        [HttpGet("{id:guid}/questions")]
        public Task<List<QuestionDto>> Questions(Guid id) => questionsHandler.GetAsync(id);

        /// <summary>Replaces the bank's question list — set semantics.</summary>
        [HttpPut("questions")]
        public async Task<IActionResult> SetQuestions([FromBody] SaveBankQuestionsDto dto)
        { await setQuestionsHandler.SetAsync(dto); return Ok(new { message = "Questions updated" }); }
    }

    /// <summary>
    /// The quiz on a Quiz content module (logic §12.87). Authoring, so it is gated with the course
    /// catalogue — and every write additionally refuses a version that is no longer a draft.
    /// </summary>
    [RequirePermission("trainingCourse")]
    public class AssessmentController(
        IGetAssessment getHandler,
        ISaveAssessment saveHandler,
        ISetAssessmentQuestions setQuestionsHandler,
        IImportQuestionsFromBank importHandler) : BaseController
    {
        /// <summary>Null when the module has no quiz yet — the normal state of a new Quiz module.</summary>
        [HttpGet("{contentModuleId:guid}")]
        public Task<AssessmentDto?> GetByModule(Guid contentModuleId)
            => getHandler.GetByModuleAsync(contentModuleId);

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveAssessmentDto dto)
            => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut("questions")]
        public async Task<IActionResult> SetQuestions([FromBody] SaveAssessmentQuestionsDto dto)
        { await setQuestionsHandler.SetAsync(dto); return Ok(new { message = "Questions updated" }); }

        /// <summary>Copies bank questions onto the end of the quiz.</summary>
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] ImportQuestionsDto dto)
            => Ok(new { imported = await importHandler.ImportAsync(dto), message = "Questions imported" });
    }

    /// <summary>
    /// Sitting the quiz. Gated on <c>myTraining</c> — this is a learner surface — and the handlers
    /// additionally require the enrolment to be the caller's OWN.
    ///
    /// <para>⚠️ Nothing here ever returns the answer key while the learner can still improve their
    /// score, and grading happens entirely on the server (logic §12.87).</para>
    /// </summary>
    [RequirePermission("myTraining")]
    public class AssessmentAttemptController(
        IStartAssessmentAttempt startHandler,
        IGetAssessmentAttempt getHandler,
        ISubmitAssessmentAttempt submitHandler) : BaseController
    {
        /// <summary>Starts an attempt, or resumes the one already open.</summary>
        [HttpPost("start")]
        [RequirePermission("myTraining", Access = PermissionAccess.Add)]
        public Task<AttemptDto> Start([FromBody] StartAttemptDto dto) => startHandler.StartAsync(dto);

        [HttpGet("{attemptId:guid}")]
        public Task<AttemptDto> Get(Guid attemptId) => getHandler.GetAsync(attemptId);

        /// <summary>Grades the attempt and returns the result, revealing answers only when allowed.</summary>
        [HttpPost("submit")]
        [RequirePermission("myTraining", Access = PermissionAccess.Add)]
        public Task<AttemptDto> Submit([FromBody] SubmitAttemptDto dto) => submitHandler.SubmitAsync(dto);
    }

    /// <summary>
    /// The learner's course catalogue — active courses, joinable sessions, and which of them address
    /// the caller's own competency gaps.
    ///
    /// <para>Gated on <c>myTraining</c>, which ordinary staff hold, rather than on
    /// <c>trainingCourse</c>, which they do not. It is a separate READ-ONLY controller rather than an
    /// extra link on TrainingCourseController because UserRole carries Add on myTraining — widening
    /// that controller would have handed every employee course creation and deletion along with the
    /// catalogue (logic §12.85).</para>
    /// </summary>
    [RequirePermission("myTraining")]
    public class TrainingCatalogController(IGetTrainingCatalog catalogHandler) : BaseController
    {
        [HttpGet]
        public Task<List<CatalogCourseDto>> Get() => catalogHandler.GetAsync();
    }

    /// <summary>CPD credits/hours rollup (HC200) — own record by default, scope-gated otherwise.</summary>
    [RequirePermission("myTraining")]
    public class TrainingCpdController(IGetCpdSummary cpdHandler) : BaseController
    {
        [HttpGet]
        public Task<CpdSummaryDto> Get([FromQuery] Guid? employeeId, [FromQuery] int? year) =>
            cpdHandler.GetAsync(employeeId, year);
    }

    /// <summary>Provider-payment hand-off for finance (HC202) — admin-only.</summary>
    [RequirePermission("trainingProviderPayment")]
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
    [RequirePermission("learningCommunity")]
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
    [RequirePermission("trainingBudget")]
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
