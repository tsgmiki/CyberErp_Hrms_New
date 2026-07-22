using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Engagement;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Employee Engagement (§3.9.1) — Phase E1: suggestions, grievances, announcements.

    /// <summary>Suggestions / ideas / feedback to management (HC203), anonymous supported (HC207).</summary>
    public class SuggestionController(
        ISubmitSuggestion submitHandler,
        IRespondSuggestion respondHandler,
        IDeleteSuggestion deleteHandler,
        IGetAllSuggestions getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<SuggestionDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitSuggestionDto dto) => Ok(new { id = await submitHandler.SubmitAsync(dto) });

        /// <summary>HR moves the pipeline (UnderReview / Actioned / Closed) and records the response.</summary>
        [HttpPut("respond")]
        public async Task<IActionResult> Respond([FromBody] RespondSuggestionDto dto)
        { await respondHandler.RespondAsync(dto); return Ok(new { message = "Response recorded" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Grievances (HC205): submit, assign, note trail, resolve, close.</summary>
    public class GrievanceController(
        ISubmitGrievance submitHandler,
        IAssignGrievance assignHandler,
        IResolveGrievance resolveHandler,
        ICloseGrievance closeHandler,
        IAddGrievanceNote noteHandler,
        IGetGrievanceById getByIdHandler,
        IGetAllGrievances getAllHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<GrievanceDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<GrievanceDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitGrievanceDto dto) => Ok(new { id = await submitHandler.SubmitAsync(dto) });

        [HttpPost("{id:guid}/assign/{assigneeEmployeeId:guid}")]
        public async Task<IActionResult> Assign(Guid id, Guid assigneeEmployeeId)
        { await assignHandler.AssignAsync(id, assigneeEmployeeId); return Ok(new { message = "Assigned" }); }

        [HttpPost("{id:guid}/resolve")]
        public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveGrievanceDto dto)
        { await resolveHandler.ResolveAsync(id, dto); return Ok(new { message = "Resolved" }); }

        [HttpPost("{id:guid}/close")]
        public async Task<IActionResult> Close(Guid id) { await closeHandler.CloseAsync(id); return Ok(new { message = "Closed" }); }

        [HttpPost("{id:guid}/notes")]
        public async Task<IActionResult> AddNote(Guid id, [FromBody] GrievanceNoteCreateDto dto)
        { await noteHandler.AddAsync(id, dto); return Ok(new { message = "Note added" }); }
    }

    /// <summary>Surveys, questionnaires and quick polls (HC204); anonymous supported (HC207).</summary>
    public class SurveyController(
        ISaveSurvey saveHandler,
        IOpenSurvey openHandler,
        ICloseSurvey closeHandler,
        IDeleteSurvey deleteHandler,
        IGetAllSurveys getAllHandler,
        IGetSurveyFeed feedHandler,
        IGetSurveyById getByIdHandler,
        ISubmitSurveyResponse submitHandler,
        IGetSurveyResults resultsHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<SurveyDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        /// <summary>Open surveys the caller can take now, with their completion flag.</summary>
        [HttpGet("feed")]
        public Task<PaginatedResponse<SurveyDto>> Feed([FromQuery] GetAllRequest request) => feedHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<SurveyDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        /// <summary>Aggregated results — per-question averages, counts and text answers (admin).</summary>
        [HttpGet("{id:guid}/results")]
        public Task<SurveyResultsDto> Results(Guid id) => resultsHandler.GetAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveSurveyDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveSurveyDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPost("{id:guid}/open")]
        public async Task<IActionResult> Open(Guid id) { await openHandler.OpenAsync(id); return Ok(new { message = "Survey opened" }); }

        [HttpPost("{id:guid}/close")]
        public async Task<IActionResult> Close(Guid id) { await closeHandler.CloseAsync(id); return Ok(new { message = "Survey closed" }); }

        [HttpPost("{id:guid}/respond")]
        public async Task<IActionResult> Respond(Guid id, [FromBody] SubmitSurveyResponseDto dto)
        { await submitHandler.SubmitAsync(id, dto); return Ok(new { message = "Response recorded" }); }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Announcements (HC206): admin-managed, employees read their targeted feed.</summary>
    public class AnnouncementController(
        ISaveAnnouncement saveHandler,
        IDeleteAnnouncement deleteHandler,
        IGetAllAnnouncements getAllHandler,
        IGetAnnouncementFeed feedHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<AnnouncementDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        /// <summary>The employee's feed: published, in-window, aimed at them (all/branch/unit-subtree).</summary>
        [HttpGet("feed")]
        public Task<PaginatedResponse<AnnouncementDto>> Feed([FromQuery] GetAllRequest request) => feedHandler.GetAsync(request);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveAnnouncementDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveAnnouncementDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }
}
