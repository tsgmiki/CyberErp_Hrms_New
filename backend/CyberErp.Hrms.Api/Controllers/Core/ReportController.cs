using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Reports;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>
    /// Generic, stored-procedure-driven report engine (ported from the reference APSmart module).
    /// A report is a registry row naming its own SP; the SP returns its own column schema (result
    /// set 1) + rows (result set 2). Parameters are metadata rows; lookups come from the master
    /// procedure Core.hrms_ReportFieldValues. @TenantId/@BranchId/@UserId are injected server-side.
    /// </summary>
    public class ReportController(
        IGetReportCatalog catalogHandler,
        IGetReportSchema schemaHandler,
        IGetReportFieldValues fieldValuesHandler,
        IGenerateReport generateHandler,
        ISaveReport saveHandler,
        IDeleteReport deleteHandler,
        ISetReportActive setActiveHandler,
        IGetReportById getByIdHandler,
        IGetAllReports getAllHandler,
        ISaveReportFilter saveFilterHandler,
        IGetReportFilter getFilterHandler,
        IDeleteReportFilter deleteFilterHandler,
        IGetReportHistory historyHandler,
        ISaveReportSchedule saveScheduleHandler,
        IGetReportSchedules getSchedulesHandler,
        IGetReportScheduleDetail getScheduleDetailHandler,
        IDeleteReportSchedule deleteScheduleHandler,
        ISetReportScheduleEnabled setScheduleEnabledHandler,
        IRunReportSchedule runScheduleHandler,
        IEmailGeneratedReport emailHandler,
        ISetReportRestrictions setRestrictionsHandler) : BaseController
    {
        // ---- Scheduled e-mail delivery (Hangfire) ----------------------------

        /// <summary>Replace a report's restrict-to roles (the "Restricted Roles" popup).</summary>
        [HttpPost("restrictions")]
        public async Task<IActionResult> SetRestrictions([FromBody] SetReportRestrictionsDto dto)
        {
            await setRestrictionsHandler.SetAsync(dto);
            return Ok(new { message = "Saved" });
        }

        /// <summary>Ad-hoc e-mail of a generated report (reference SendGeneratedReportByEmail).</summary>
        [HttpPost("email")]
        public Task<ScheduleRunResultDto> Email([FromBody] EmailReportDto dto) => emailHandler.SendAsync(dto);

        [HttpPost("schedules")]
        public Task<Guid> SaveSchedule([FromBody] SaveReportScheduleDto dto) => saveScheduleHandler.SaveAsync(dto);

        [HttpGet("schedules")]
        public Task<List<ReportScheduleListDto>> Schedules([FromQuery] string reportKey) => getSchedulesHandler.GetAsync(reportKey);

        /// <summary>Full detail for editing a schedule (hydrates the schedule form).</summary>
        [HttpGet("schedules/{id:guid}")]
        public Task<ReportScheduleDetailDto> ScheduleDetail(Guid id) => getScheduleDetailHandler.GetAsync(id);

        /// <summary>Enable/disable a schedule in place (reference _x_ReportClientScheduleEnable).</summary>
        [HttpPost("schedules/{id:guid}/enable")]
        public async Task<IActionResult> EnableSchedule(Guid id, [FromQuery] bool enabled)
        {
            await setScheduleEnabledHandler.SetAsync(id, enabled);
            return Ok(new { message = enabled ? "Enabled" : "Disabled" });
        }

        [HttpDelete("schedules/{id:guid}")]
        public async Task<IActionResult> DeleteSchedule(Guid id)
        {
            await deleteScheduleHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }

        /// <summary>Runs a schedule immediately (same pipeline the recurring job uses).</summary>
        [HttpPost("schedules/{id:guid}/run-now")]
        public Task<ScheduleRunResultDto> RunNow(Guid id) => runScheduleHandler.RunAsync(id);

        // ---- Saved filter sets (catalog "children") -------------------------

        [HttpPost("filters")]
        public Task<Guid> SaveFilter([FromBody] SaveReportFilterDto dto) => saveFilterHandler.SaveAsync(dto);

        [HttpGet("filters/{id:guid}")]
        public Task<ReportFilterDto> GetFilter(Guid id) => getFilterHandler.GetAsync(id);

        [HttpDelete("filters/{id:guid}")]
        public async Task<IActionResult> DeleteFilter(Guid id)
        {
            await deleteFilterHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }

        /// <summary>Recent run history (optionally per report).</summary>
        [HttpGet("history")]
        public Task<List<ReportRunDto>> History([FromQuery] string? reportKey) => historyHandler.GetAsync(reportKey);

        // ---- Viewer -------------------------------------------------------

        /// <summary>Active reports grouped by category (the report menu).</summary>
        [HttpGet("catalog")]
        public Task<List<ReportCatalogGroupDto>> Catalog() => catalogHandler.GetAsync();

        /// <summary>A report's dynamic parameter schema (choice fields come with preloaded options).</summary>
        [HttpGet("{reportKey}/schema")]
        public Task<ReportSchemaDto> Schema(string reportKey) => schemaHandler.GetAsync(reportKey);

        /// <summary>Lookup options for one parameter (supports cascading `dependency` + `search`).</summary>
        [HttpGet("{reportKey}/field-values")]
        public Task<List<ReportLookupOptionDto>> FieldValues(string reportKey,
            [FromQuery] string field, [FromQuery] string? dependency, [FromQuery] string? search)
            => fieldValuesHandler.GetAsync(reportKey, field, dependency, search);

        /// <summary>Runs the report: dynamic columns + rows straight from the SP.</summary>
        [HttpPost("generate")]
        public Task<ReportResultDto> Generate([FromBody] GenerateReportDto dto) => generateHandler.GenerateAsync(dto);

        // ---- Admin registry -------------------------------------------------

        [HttpGet]
        public Task<PaginatedResponse<ReportDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<ReportDto> GetById(Guid id) => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveReportDto dto) => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveReportDto dto)
        {
            await saveHandler.SaveAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }

        /// <summary>Activate/deactivate a report definition (reference _x_ReportActivate).</summary>
        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id, [FromQuery] bool active)
        {
            await setActiveHandler.SetAsync(id, active);
            return Ok(new { message = active ? "Activated" : "Deactivated" });
        }
    }
}
