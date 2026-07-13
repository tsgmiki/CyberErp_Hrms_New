using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.DynamicForms;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /// <summary>User-defined dynamic form (custom tab) definitions — the "Form Builder".</summary>
    public class DynamicFormController(IDynamicFormService service) : BaseController
    {
        /// <summary>Paged list of forms for a module (admin Form Builder).</summary>
        [HttpGet]
        public Task<PaginatedResponse<DynamicFormDto>> GetAll([FromQuery] GetAllRequest request)
            => service.GetAllFormsAsync(request);

        /// <summary>Active forms + their fields for a module — drives dynamic tab rendering.</summary>
        [HttpGet("active")]
        public Task<List<DynamicFormDto>> GetActive([FromQuery] string module)
            => service.GetActiveFormsAsync(module);

        [HttpGet("{id:guid}")]
        public Task<DynamicFormDto> GetById(Guid id)
            => service.GetFormByIdAsync(id);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveDynamicFormDto dto)
            => Ok(new { id = await service.SaveFormAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveDynamicFormDto dto)
            => Ok(new { id = await service.SaveFormAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await service.DeleteFormAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }

    /// <summary>Data rows of a dynamic form for a specific owner (e.g. an employee).</summary>
    public class DynamicFormRecordController(IDynamicFormService service) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<DynamicFormRecordDto>> Get(
            [FromQuery] Guid formId, [FromQuery] string ownerType, [FromQuery] Guid ownerId, [FromQuery] GetAllRequest request)
            => service.GetRecordsAsync(formId, ownerType, ownerId, request);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveDynamicFormRecordDto dto)
            => Ok(new { id = await service.SaveRecordAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveDynamicFormRecordDto dto)
            => Ok(new { id = await service.SaveRecordAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await service.DeleteRecordAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
