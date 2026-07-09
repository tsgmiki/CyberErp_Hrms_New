using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Recruitment;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>Hiring Need Assessment (HC077–HC083): justified, budgeted, workflow-approved hiring needs.</summary>
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
        IHireCandidate hireHandler) : BaseController
    {
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

    /// <summary>Application pipeline (HC098–HC099): candidate × requisition with stage machine + log.</summary>
    public class JobApplicationController(
        ICreateJobApplication createHandler,
        IGetJobApplicationById getByIdHandler,
        IGetAllJobApplications getAllHandler,
        IMoveJobApplicationStage moveHandler,
        IScoreJobApplication scoreHandler,
        IGetApplicationRanking rankingHandler) : BaseController
    {
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
    }
}
