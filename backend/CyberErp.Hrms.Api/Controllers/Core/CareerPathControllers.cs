using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.CareerDevelopment;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Career Development §3.7.B — Career Path (HC161–HC169).

    /// <summary>Career path definitions (HC161) + visualisation &amp; utilisation analytics (HC166).</summary>
    public class CareerPathController(
        ISaveCareerPath saveHandler,
        IDeleteCareerPath deleteHandler,
        IGetCareerPathById getByIdHandler,
        IGetAllCareerPaths getAllHandler,
        IVisualizeCareerPath visualizeHandler,
        IGetCareerPathUtilization utilizationHandler,
        ISuggestCareerPaths suggestHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<CareerPathDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<CareerPathDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        /// <summary>HC164/HC166 — the ordered step ladder, optionally overlaid with an employee's progress + competency gaps.</summary>
        [HttpGet("{id:guid}/visualize")] public Task<CareerPathVisualizeDto> Visualize(Guid id, [FromQuery] Guid? employeeId) => visualizeHandler.GetAsync(id, employeeId);
        /// <summary>HC166 — assignment counts, status breakdown and average progress per path.</summary>
        [HttpGet("analytics/utilization")] public Task<List<CareerPathUtilizationDto>> Utilization() => utilizationHandler.GetAsync();
        /// <summary>HC163 — suggest career paths for an employee, ranked by competency match.</summary>
        [HttpGet("suggestions")] public Task<List<CareerPathSuggestionDto>> Suggestions([FromQuery] Guid employeeId) => suggestHandler.SuggestAsync(employeeId);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveCareerPathDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveCareerPathDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Employee 360 development profile (HC158) — the Performance ↔ Career Development bridge.</summary>
    public class EmployeeDevelopmentController(IGetEmployeeDevelopmentProfile profileHandler) : BaseController
    {
        /// <summary>Holistic view: performance snapshot + career-path progress + succession candidacy + next-step gap + mentorships.</summary>
        [HttpGet("{employeeId:guid}/profile")] public Task<EmployeeDevelopmentProfileDto> Profile(Guid employeeId) => profileHandler.GetAsync(employeeId);
    }

    /// <summary>Steps of a career path + their required competencies (HC162). Filter by ?parentId=careerPathId.</summary>
    public class CareerPathStepController(
        ISaveCareerPathStep saveHandler,
        IDeleteCareerPathStep deleteHandler,
        IGetCareerPathStepById getByIdHandler,
        IGetAllCareerPathSteps getAllHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<CareerPathStepDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<CareerPathStepDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveCareerPathStepDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveCareerPathStepDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Employee career-path assignments + per-step progress (HC163, HC165). Filter by ?parentId=careerPathId or ?employeeId=.</summary>
    public class EmployeeCareerPathController(
        ISaveEmployeeCareerPath saveHandler,
        IDeleteEmployeeCareerPath deleteHandler,
        IGetEmployeeCareerPathById getByIdHandler,
        IGetAllEmployeeCareerPaths getAllHandler,
        IGetCareerPathRecommendations recommendationsHandler,
        ICreateDevelopmentGoals createGoalsHandler,
        ICreateDevelopmentPlanFromGap createPlanHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<EmployeeCareerPathDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<EmployeeCareerPathDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        /// <summary>HC164 — competencies to develop for the employee's next career step.</summary>
        [HttpGet("{id:guid}/recommendations")] public Task<DevelopmentRecommendationDto> Recommendations(Guid id) => recommendationsHandler.GetAsync(id);
        /// <summary>HC167 — materialise the gap as development goals on the performance engine, aligned to an org objective.</summary>
        [HttpPost("{id:guid}/create-goals")] public Task<CreateDevelopmentGoalsResultDto> CreateGoals(Guid id, [FromQuery] Guid? reviewCycleId, [FromQuery] Guid? organizationalObjectiveId) => createGoalsHandler.CreateAsync(id, reviewCycleId, organizationalObjectiveId);
        /// <summary>HC130 — turn the gap into a structured Individual Development Plan on the performance engine.</summary>
        [HttpPost("{id:guid}/create-development-plan")] public Task<CreateDevelopmentPlanResultDto> CreatePlan(Guid id) => createPlanHandler.FromCareerPathAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveEmployeeCareerPathDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveEmployeeCareerPathDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Mentor↔mentee pairings (HC168). Filter by ?parentId=menteeEmployeeId or ?employeeId=.</summary>
    public class MentorshipController(
        ISaveMentorship saveHandler,
        IDeleteMentorship deleteHandler,
        IGetMentorshipById getByIdHandler,
        IGetAllMentorships getAllHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<MentorshipDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<MentorshipDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveMentorshipDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveMentorshipDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Career-path change requests with a light approval flow (HC169). Filter by ?employeeId= / ?status=.</summary>
    public class CareerPathChangeRequestController(
        ISaveCareerPathChangeRequest saveHandler,
        IDeleteCareerPathChangeRequest deleteHandler,
        IGetCareerPathChangeRequestById getByIdHandler,
        IGetAllCareerPathChangeRequests getAllHandler,
        ISubmitCareerPathChangeRequest submitHandler,
        IApproveCareerPathChangeRequest approveHandler,
        IRejectCareerPathChangeRequest rejectHandler) : BaseController
    {
        [HttpGet] public Task<PaginatedResponse<CareerPathChangeRequestDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);
        [HttpGet("{id:guid}")] public Task<CareerPathChangeRequestDto> GetById(Guid id) => getByIdHandler.GetAsync(id);
        /// <summary>HC169 — submit a draft request for managerial review.</summary>
        [HttpPost("{id:guid}/submit")] public async Task<IActionResult> Submit(Guid id) { await submitHandler.SubmitAsync(id); return Ok(new { message = "Submitted" }); }
        /// <summary>HC169 — approve a submitted request (assigns the employee to the requested path).</summary>
        [HttpPost("{id:guid}/approve")] public async Task<IActionResult> Approve(Guid id, [FromBody] DecideCareerPathChangeRequestDto dto) { await approveHandler.ApproveAsync(id, dto); return Ok(new { message = "Approved" }); }
        /// <summary>HC169 — reject a submitted request with a note.</summary>
        [HttpPost("{id:guid}/reject")] public async Task<IActionResult> Reject(Guid id, [FromBody] DecideCareerPathChangeRequestDto dto) { await rejectHandler.RejectAsync(id, dto); return Ok(new { message = "Rejected" }); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] SaveCareerPathChangeRequestDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpPut] public async Task<IActionResult> Update([FromBody] SaveCareerPathChangeRequestDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });
        [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }
}
