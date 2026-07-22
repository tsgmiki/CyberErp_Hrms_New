using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Recruitment;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Hiring Need Assessment (HC077–HC083): justified, budgeted, workflow-approved hiring needs.</summary>
    [RequirePermission("hiringRequest", "jobRequisition")]
    public class HiringRequestController(
        ISaveHiringRequest saveHandler,
        IGetHiringRequestById getByIdHandler,
        IGetAllHiringRequests getAllHandler,
        IDeleteHiringRequest deleteHandler,
        ISubmitHiringRequest submitHandler,
        ICloseHiringRequest closeHandler,
        IGetRecruitmentBudgetMonitor budgetHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<HiringRequestDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>Per-unit recruitment budget/headcount monitor (HC083).</summary>
        [HttpGet("budget-monitor")]
        public Task<List<RecruitmentBudgetRowDto>> GetBudgetMonitor()
            => budgetHandler.GetAsync();

        [HttpGet("{id:guid}")]
        public Task<HiringRequestDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveHiringRequestDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveHiringRequestDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        /// <summary>Submits for approval — validated against the establishment first (HC082).</summary>
        [HttpPost("{id:guid}/submit")]
        public async Task<IActionResult> Submit(Guid id)
        {
            await submitHandler.SubmitAsync(id);
            return Ok(new { message = "Hiring request submitted" });
        }

        [HttpPost("{id:guid}/close")]
        public async Task<IActionResult> Close(Guid id)
        {
            await closeHandler.CloseAsync(id);
            return Ok(new { message = "Hiring request closed" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }

    /// <summary>Job requisitions (HC084–HC088, HC091): approvable vacancies raised from approved hiring needs.</summary>
    [RequirePermission("jobRequisition", "hiringRequest", "jobApplication", "candidate")]
    public class JobRequisitionController(
        ISaveJobRequisition saveHandler,
        IGetJobRequisitionById getByIdHandler,
        IGetAllJobRequisitions getAllHandler,
        IDeleteJobRequisition deleteHandler,
        ISubmitJobRequisition submitHandler,
        ISetRequisitionPosting setPostingHandler,
        IGenerateRequisitionPosting generatePostingHandler,
        IPostJobRequisition postHandler,
        ICloseJobRequisition closeHandler,
        ICancelJobRequisition cancelHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<JobRequisitionDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<JobRequisitionDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        /// <summary>Standard advertisement text generated from the requisition details (HC091).</summary>
        [HttpGet("{id:guid}/generate-posting")]
        public Task<string> GeneratePosting(Guid id)
            => generatePostingHandler.GenerateAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveJobRequisitionDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveJobRequisitionDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpPost("{id:guid}/submit")]
        public async Task<IActionResult> Submit(Guid id)
        {
            await submitHandler.SubmitAsync(id);
            return Ok(new { message = "Requisition submitted" });
        }

        /// <summary>Saves the posting channel, text and window (HC088/HC091).</summary>
        [HttpPut("posting")]
        public async Task<IActionResult> SetPosting([FromBody] SetPostingDto dto)
        {
            await setPostingHandler.SetAsync(dto);
            return Ok(new { message = "Posting updated" });
        }

        /// <summary>Publishes the approved requisition to its channel(s) (HC088).</summary>
        [HttpPost("{id:guid}/post")]
        public async Task<IActionResult> Post(Guid id)
        {
            await postHandler.PostAsync(id);
            return Ok(new { message = "Requisition posted" });
        }

        [HttpPost("{id:guid}/close")]
        public async Task<IActionResult> Close(Guid id)
        {
            await closeHandler.CloseAsync(id);
            return Ok(new { message = "Requisition closed" });
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await cancelHandler.CancelAsync(id);
            return Ok(new { message = "Requisition cancelled" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }

    /// <summary>Centralized candidate database (HC092–HC097) + internal matching (HC089–HC090).</summary>
    [RequirePermission("candidate", "jobApplication", "talentPool", "jobRequisition")]
    public class CandidateController(
        ISaveCandidate saveHandler,
        IGetCandidateById getByIdHandler,
        IGetAllCandidates getAllHandler,
        IDeleteCandidate deleteHandler,
        ISetCandidateTalentPool talentPoolHandler,
        IAnonymizeCandidate anonymizeHandler,
        IUploadCandidateResume uploadResumeHandler,
        IGetCandidateResume getResumeHandler,
        IMatchCandidates matchHandler,
        IUploadCandidateDocument uploadDocumentHandler,
        IGetCandidateDocuments getDocumentsHandler,
        IDownloadCandidateDocument downloadDocumentHandler,
        IDeleteCandidateDocument deleteDocumentHandler,
        IHireCandidate hireHandler,
        IGetCandidateEducations getEducationsHandler,
        ISaveCandidateEducation saveEducationHandler,
        IDeleteCandidateEducation deleteEducationHandler,
        IGetCandidateExperiences getExperiencesHandler,
        ISaveCandidateExperience saveExperienceHandler,
        IDeleteCandidateExperience deleteExperienceHandler,
        IUploadCandidateBackgroundDocument uploadBackgroundDocumentHandler,
        IGetCandidateBackgroundDocuments getBackgroundDocumentsHandler,
        IDownloadCandidateBackgroundDocument downloadBackgroundDocumentHandler,
        IDeleteCandidateBackgroundDocument deleteBackgroundDocumentHandler) : BaseController
    {
        // ---- Structured background (education / experience) -------------------------
        // These write the SAME person-owned rows the employee profile uses, so at hire the
        // new employee sees them automatically (shared PersonId). Read-only for internal
        // candidates — their records are maintained on the employee master.

        [HttpGet("{id:guid}/education")]
        public Task<List<CandidateEducationDto>> GetEducations(Guid id)
            => getEducationsHandler.GetAsync(id);

        [HttpPost("{id:guid}/education")]
        public async Task<IActionResult> SaveEducation(Guid id, [FromBody] SaveCandidateEducationDto dto)
        {
            dto.CandidateId = id;
            var educationId = await saveEducationHandler.SaveAsync(dto);
            return Ok(new { message = "Saved successfully", id = educationId });
        }

        [HttpDelete("education/{educationId:guid}")]
        public async Task<IActionResult> DeleteEducation(Guid educationId)
        {
            await deleteEducationHandler.DeleteAsync(educationId);
            return Ok(new { message = "Deleted successfully" });
        }

        [HttpGet("{id:guid}/experience")]
        public Task<List<CandidateExperienceDto>> GetExperiences(Guid id)
            => getExperiencesHandler.GetAsync(id);

        [HttpPost("{id:guid}/experience")]
        public async Task<IActionResult> SaveExperience(Guid id, [FromBody] SaveCandidateExperienceDto dto)
        {
            dto.CandidateId = id;
            var experienceId = await saveExperienceHandler.SaveAsync(dto);
            return Ok(new { message = "Saved successfully", id = experienceId });
        }

        [HttpDelete("experience/{experienceId:guid}")]
        public async Task<IActionResult> DeleteExperience(Guid experienceId)
        {
            await deleteExperienceHandler.DeleteAsync(experienceId);
            return Ok(new { message = "Deleted successfully" });
        }

        /// <summary>Files attached to one education/experience row (they follow the row to the employee at hire).</summary>
        [HttpGet("{id:guid}/background-documents")]
        public Task<List<CyberErp.Hrms.App.Features.Core.Employees.EmployeeDocumentDto>> GetBackgroundDocuments(
            Guid id, [FromQuery] string ownerType, [FromQuery] Guid ownerId)
            => getBackgroundDocumentsHandler.GetAsync(id, ownerType, ownerId);

        [HttpPost("{id:guid}/background-documents")]
        [RequestSizeLimit(11 * 1024 * 1024)]
        public async Task<IActionResult> UploadBackgroundDocument(
            Guid id, [FromForm] string ownerType, [FromForm] Guid ownerId, IFormFile file)
        {
            if (file is null) return BadRequest(new { message = "No file uploaded." });
            await using var stream = file.OpenReadStream();
            var documentId = await uploadBackgroundDocumentHandler.UploadAsync(
                id, ownerType, ownerId, stream, file.FileName, file.ContentType, file.Length);
            return Ok(new { message = "Document uploaded", id = documentId });
        }

        [HttpGet("background-documents/{documentId:guid}/download")]
        public async Task<IActionResult> DownloadBackgroundDocument(Guid documentId)
        {
            var (content, contentType, fileName) = await downloadBackgroundDocumentHandler.GetAsync(documentId);
            return File(content, contentType, fileName);
        }

        [HttpDelete("background-documents/{documentId:guid}")]
        public async Task<IActionResult> DeleteBackgroundDocument(Guid documentId)
        {
            await deleteBackgroundDocumentHandler.DeleteAsync(documentId);
            return Ok(new { message = "Document deleted" });
        }

        /// <summary>The candidate's attached files (credentials + mandatory compliance set).</summary>
        [HttpGet("{id:guid}/documents")]
        public Task<List<CandidateDocumentDto>> GetDocuments(Guid id)
            => getDocumentsHandler.GetAsync(id);

        /// <summary>Attaches a typed file (education certificate, ID, guarantor form, contract…).</summary>
        [HttpPost("{id:guid}/documents")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UploadDocument(Guid id, [FromForm] string documentType, IFormFile file)
        {
            if (file is null) return BadRequest(new { message = "No file uploaded." });
            await using var stream = file.OpenReadStream();
            var documentId = await uploadDocumentHandler.UploadAsync(
                id, documentType, stream, file.FileName, file.ContentType, file.Length);
            return Ok(new { message = "Document uploaded", id = documentId });
        }

        [HttpGet("documents/{documentId:guid}")]
        public async Task<IActionResult> DownloadDocument(Guid documentId)
        {
            var (content, contentType, fileName) = await downloadDocumentHandler.GetAsync(documentId);
            return File(content, contentType, fileName);
        }

        [HttpDelete("documents/{documentId:guid}")]
        public async Task<IActionResult> DeleteDocument(Guid documentId)
        {
            await deleteDocumentHandler.DeleteAsync(documentId);
            return Ok(new { message = "Document deleted" });
        }

        /// <summary>
        /// Hires the candidate: an employee is created on the candidate's EXISTING person record,
        /// all documents migrate to the employee history, and probation tracking starts when set.
        /// Blocked until the mandatory compliance documents are complete.
        /// </summary>
        [HttpPost("{id:guid}/hire")]
        public async Task<Guid> Hire(Guid id, [FromBody] HireCandidateDto dto)
        {
            dto.Id = id;
            return await hireHandler.HireAsync(dto);
        }

        [HttpGet]
        public Task<PaginatedResponse<CandidateDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>Ranked skills/experience matching of candidates to a vacancy (HC090).</summary>
        [HttpGet("match")]
        public Task<List<CandidateMatchDto>> Match([FromQuery] Guid requisitionId)
            => matchHandler.GetAsync(requisitionId);

        [HttpGet("{id:guid}")]
        public Task<CandidateDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveCandidateDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveCandidateDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        /// <summary>Flags / unflags the candidate for the internal talent pool (HC089).</summary>
        [HttpPut("talent-pool")]
        public async Task<IActionResult> SetTalentPool([FromBody] SetTalentPoolDto dto)
        {
            await talentPoolHandler.SetAsync(dto);
            return Ok(new { message = "Talent pool updated" });
        }

        /// <summary>Retention-policy anonymization — scrubs PII, keeps the anonymous history (HC097).</summary>
        [HttpPost("{id:guid}/anonymize")]
        public async Task<IActionResult> Anonymize(Guid id)
        {
            await anonymizeHandler.AnonymizeAsync(id);
            return Ok(new { message = "Candidate anonymized" });
        }

        [HttpPost("{id:guid}/resume")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UploadResume(Guid id, IFormFile file)
        {
            if (file is null) return BadRequest(new { message = "No file uploaded." });
            await using var stream = file.OpenReadStream();
            await uploadResumeHandler.UploadAsync(id, stream, file.FileName, file.Length);
            return Ok(new { message = "Resume uploaded" });
        }

        [HttpGet("{id:guid}/resume")]
        public async Task<IActionResult> GetResume(Guid id)
        {
            var (content, contentType, fileName) = await getResumeHandler.GetAsync(id);
            return File(content, contentType, fileName);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }

    /// <summary>Interview scheduling, panels and scored feedback (HC101–HC109).</summary>
    [RequirePermission("jobApplication", "jobRequisition")]
    public class InterviewController(
        ISaveInterview saveHandler,
        IGetInterviews getHandler,
        ISetInterviewStatus statusHandler,
        ISubmitInterviewFeedback feedbackHandler,
        IGetInterviewConsolidated consolidatedHandler,
        IDeleteInterview deleteHandler) : BaseController
    {
        /// <summary>All rounds of one application (panels + feedback + averages).</summary>
        [HttpGet]
        public Task<List<InterviewDto>> Get([FromQuery] Guid applicationId)
            => getHandler.GetAsync(applicationId);

        /// <summary>The consolidated evaluation report across all rounds (HC109).</summary>
        [HttpGet("consolidated")]
        public Task<InterviewConsolidatedDto> GetConsolidated([FromQuery] Guid applicationId)
            => consolidatedHandler.GetAsync(applicationId);

        /// <summary>Schedules a round (create) or reschedules/repanels a pending one (update).</summary>
        [HttpPost]
        public Task<Guid> Save([FromBody] SaveInterviewDto dto)
            => saveHandler.SaveAsync(dto);

        /// <summary>Complete | Cancel | NoShow.</summary>
        [HttpPut("status")]
        public async Task<IActionResult> SetStatus([FromBody] SetInterviewStatusDto dto)
        {
            await statusHandler.SetAsync(dto);
            return Ok(new { message = "Interview updated" });
        }

        /// <summary>One panelist's per-criterion scores (attendance auto-marks Attended).</summary>
        [HttpPut("feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] SubmitInterviewFeedbackDto dto)
        {
            await feedbackHandler.SubmitAsync(dto);
            return Ok(new { message = "Feedback recorded" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }

    /// <summary>Formal offers: approval workflow, letter, response tracking (HC111–HC114).</summary>
    [RequirePermission("jobApplication", "jobRequisition")]
    public class JobOfferController(
        ISaveJobOffer saveHandler,
        IGetJobOffers getHandler,
        IGetOfferDefaults defaultsHandler,
        ISubmitJobOffer submitHandler,
        ISendJobOffer sendHandler,
        IRespondJobOffer respondHandler,
        IWithdrawJobOffer withdrawHandler,
        IGenerateOfferLetter letterHandler,
        IDeleteJobOffer deleteHandler) : BaseController
    {
        /// <summary>The offers of one application (newest first; lazy-expires lapsed ones).</summary>
        [HttpGet]
        public Task<List<JobOfferDto>> Get([FromQuery] Guid applicationId)
            => getHandler.GetAsync(applicationId);

        /// <summary>
        /// Vacancy-derived defaults for a new offer: position salary scale + amount, and the
        /// hiring manager resolved from the unit hierarchy (unit → parent units).
        /// </summary>
        [HttpGet("defaults")]
        public Task<OfferDefaultsDto> GetDefaults([FromQuery] Guid applicationId)
            => defaultsHandler.GetAsync(applicationId);

        /// <summary>Standard offer-letter text assembled server-side (HC111).</summary>
        [HttpGet("{id:guid}/generate-letter")]
        public Task<string> GenerateLetter(Guid id)
            => letterHandler.GenerateAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveJobOfferDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveJobOfferDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        /// <summary>Draft → approval workflow (HC112); approves directly when none is configured.</summary>
        [HttpPost("{id:guid}/submit")]
        public async Task<IActionResult> Submit(Guid id)
        {
            await submitHandler.SubmitAsync(id);
            return Ok(new { message = "Offer submitted" });
        }

        /// <summary>Approved → Sent; the application moves to OfferPending (logged) and the
        /// letter is e-mailed to the candidate as a PDF (retry path for failed auto-delivery).</summary>
        [HttpPost("{id:guid}/send")]
        public async Task<IActionResult> Send(Guid id)
        {
            var emailed = await sendHandler.SendAsync(id);
            return Ok(new
            {
                message = emailed
                    ? "Offer sent — the PDF letter is queued for e-mail delivery to the candidate."
                    : "Offer marked sent, but no e-mail was queued (candidate has no address or the mailer is disabled) — deliver the letter manually."
            });
        }

        /// <summary>Records the candidate's response: Accept | Decline (HC114).</summary>
        [HttpPut("respond")]
        public async Task<IActionResult> Respond([FromBody] RespondJobOfferDto dto)
        {
            await respondHandler.RespondAsync(dto);
            return Ok(new { message = "Response recorded" });
        }

        [HttpPost("{id:guid}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid id, [FromBody] Dictionary<string, string?>? body)
        {
            await withdrawHandler.WithdrawAsync(id, body?.GetValueOrDefault("note"));
            return Ok(new { message = "Offer withdrawn" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }

    /// <summary>Application pipeline (HC098–HC099): candidate × requisition with stage machine + log.</summary>
    [RequirePermission("jobApplication", "jobRequisition", "candidate")]
    public class JobApplicationController(
        ICreateJobApplication createHandler,
        IGetJobApplicationById getByIdHandler,
        IGetAllJobApplications getAllHandler,
        IMoveJobApplicationStage moveHandler,
        IBulkMoveApplicationStage bulkMoveHandler,
        IScoreJobApplication scoreHandler,
        IGetApplicationRanking rankingHandler,
        IGetHireQueue hireQueueHandler,
        IGetEvaluatorContext evaluatorContextHandler,
        IAdoptInterviewScores adoptHandler) : BaseController
    {
        /// <summary>The current user's evaluator scope (assigned criteria/requisitions), for the UI.</summary>
        [HttpGet("evaluator-context")]
        public Task<EvaluatorContextDto> GetEvaluatorContext()
            => evaluatorContextHandler.GetAsync();

        /// <summary>Copies the consolidated per-criterion interview averages into the score sheet.</summary>
        [HttpPost("{id:guid}/adopt-interview-scores")]
        public async Task<IActionResult> AdoptInterviewScores(Guid id)
        {
            var adopted = await adoptHandler.AdoptAsync(id);
            return Ok(new { message = $"{adopted} interview criterion average(s) adopted into the ranking" });
        }

        /// <summary>The "Hire Employee" queue — fully qualified, ranked applicants (+ waitlist).</summary>
        [HttpGet("hire-queue")]
        public Task<List<HireQueueRowDto>> GetHireQueue()
            => hireQueueHandler.GetAsync();

        /// <summary>Paged pipeline (?status=Stage, ?parentId=requisitionId, ?categoryId=candidateId).</summary>
        [HttpGet]
        public Task<PaginatedResponse<JobApplicationDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>Auto-calculated weighted ranking of a vacancy's applicants.</summary>
        [HttpGet("ranking")]
        public Task<List<ApplicationRankingRowDto>> GetRanking([FromQuery] Guid requisitionId)
            => rankingHandler.GetAsync(requisitionId);

        /// <summary>Records evaluator scores per criterion; the total recomputes automatically.</summary>
        [HttpPut("scores")]
        public async Task<IActionResult> Score([FromBody] ScoreApplicationDto dto)
        {
            await scoreHandler.ScoreAsync(dto);
            return Ok(new { message = "Scores recorded" });
        }

        [HttpGet("{id:guid}")]
        public Task<JobApplicationDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateJobApplicationDto dto)
            => createHandler.CreateAsync(dto);

        /// <summary>Moves the application through the pipeline, logging the transition (HC098/HC099).</summary>
        [HttpPut("stage")]
        public async Task<IActionResult> MoveStage([FromBody] MoveApplicationStageDto dto)
        {
            await moveHandler.MoveAsync(dto);
            return Ok(new { message = "Application stage updated" });
        }

        /// <summary>Mass stage move — movable applications move; the rest report back with reasons.</summary>
        [HttpPut("stage/bulk")]
        public Task<BulkMoveResultDto> BulkMoveStage([FromBody] BulkMoveApplicationStageDto dto)
            => bulkMoveHandler.MoveAsync(dto);
    }
}
