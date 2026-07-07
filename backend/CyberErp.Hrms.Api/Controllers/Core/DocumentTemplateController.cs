using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates;
using CyberErp.Hrms.App.Features.Core.DocumentTemplates.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class DocumentTemplateController(
        ICreateDocumentTemplate createHandler,
        IUpdateDocumentTemplate updateHandler,
        IDeleteDocumentTemplate deleteHandler,
        IGetDocumentTemplateById getByIdHandler,
        IGetAllDocumentTemplates getAllHandler,
        IGetDocumentMergeFields mergeFieldsHandler,
        IGenerateEmployeeDocument generateHandler,
        IUploadCompanyLogo uploadLogoHandler,
        IGetCompanyLogo getLogoHandler,
        IGetCompanyLogoInfo getLogoInfoHandler,
        IDeleteCompanyLogo deleteLogoHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<DocumentTemplateDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        /// <summary>Catalog of merge tokens (incl. dynamic custom fields) for the template editor palette.</summary>
        [HttpGet("merge-fields")]
        public Task<List<MergeFieldDto>> GetMergeFields()
            => mergeFieldsHandler.GetAsync();

        // ---- Company logo (used by the {{Logo}} merge token; one per tenant) ----------------

        /// <summary>Whether a company logo is configured (for the template editor's logo panel).</summary>
        [HttpGet("logo/info")]
        public Task<CompanyLogoInfo> GetLogoInfo()
            => getLogoInfoHandler.GetAsync();

        /// <summary>Streams the current company logo.</summary>
        [HttpGet("logo")]
        public async Task<IActionResult> GetLogo()
        {
            var (content, contentType) = await getLogoHandler.GetAsync();
            return File(content, contentType);
        }

        /// <summary>Uploads / replaces the company logo (JPG, PNG, WEBP or GIF, max 2 MB).</summary>
        [HttpPost("logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file is null) return BadRequest(new { message = "No file provided." });
            await using var stream = file.OpenReadStream();
            await uploadLogoHandler.UploadAsync(stream, file.FileName, file.Length);
            return Ok(new { message = "Logo uploaded" });
        }

        /// <summary>Removes the company logo.</summary>
        [HttpDelete("logo")]
        public async Task<IActionResult> DeleteLogo()
        {
            await deleteLogoHandler.DeleteAsync();
            return Ok(new { message = "Logo removed" });
        }

        [HttpGet("{id:guid}")]
        public Task<DocumentTemplateDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        /// <summary>Render this template's merged, print-ready HTML for a specific employee.</summary>
        [HttpGet("{id:guid}/generate/{employeeId:guid}")]
        public Task<GeneratedDocumentDto> Generate(Guid id, Guid employeeId)
            => generateHandler.GenerateAsync(id, employeeId);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateDocumentTemplateDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateDocumentTemplateDto dto)
        {
            await updateHandler.UpdateAsync(dto);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
