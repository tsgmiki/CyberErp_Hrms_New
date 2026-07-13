using CyberErp.Hrms.App.Features.Core.Employees;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class EmployeeEducationController(
        ISaveEmployeeEducation saveHandler,
        IDeleteEmployeeEducation deleteHandler,
        IGetEmployeeEducations getHandler) : BaseController
    {
        [HttpGet]
        public Task<List<EmployeeEducationDto>> GetByEmployee([FromQuery] Guid employeeId)
            => getHandler.GetAsync(employeeId);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveEmployeeEducationDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeEducationDto dto)
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
    }

    public class EmployeeExperienceController(
        ISaveEmployeeExperience saveHandler,
        IDeleteEmployeeExperience deleteHandler,
        IGetEmployeeExperiences getHandler) : BaseController
    {
        [HttpGet]
        public Task<List<EmployeeExperienceDto>> GetByEmployee([FromQuery] Guid employeeId)
            => getHandler.GetAsync(employeeId);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveEmployeeExperienceDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeExperienceDto dto)
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
    }

    public class EmployeeDocumentController(
        IUploadEmployeeDocument uploadHandler,
        IGetEmployeeDocuments getHandler,
        IDownloadEmployeeDocument downloadHandler,
        IDeleteEmployeeDocument deleteHandler) : BaseController
    {
        /// <summary>Documents attached to an education/experience record.</summary>
        [HttpGet]
        public Task<List<EmployeeDocumentDto>> GetByOwner([FromQuery] string ownerType, [FromQuery] Guid ownerId, [FromQuery] string? ownerField)
            => getHandler.GetAsync(ownerType, ownerId, ownerField);

        /// <summary>Attach a file (PDF/Office/image/text, max 10 MB) to an education/experience record
        /// (or a dynamic-form Attachment field via <c>ownerField</c>).</summary>
        [HttpPost]
        public async Task<IActionResult> Upload(
            [FromForm] Guid employeeId, [FromForm] string ownerType, [FromForm] Guid ownerId, [FromForm] string? ownerField, IFormFile file)
        {
            if (file is null) return BadRequest(new { message = "No file provided." });
            await using var stream = file.OpenReadStream();
            var id = await uploadHandler.UploadAsync(employeeId, ownerType, ownerId, ownerField, stream, file.FileName, file.ContentType, file.Length);
            return Ok(new { id, message = "Document uploaded" });
        }

        /// <summary>Downloads a document as a file attachment.</summary>
        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id)
        {
            var (content, contentType, fileName) = await downloadHandler.GetAsync(id);
            return File(content, contentType, fileName);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await deleteHandler.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }
    }

    public class EmployeeDependentController(
        ISaveEmployeeDependent saveHandler,
        IDeleteEmployeeDependent deleteHandler,
        IGetEmployeeDependents getHandler) : BaseController
    {
        [HttpGet]
        public Task<List<EmployeeDependentDto>> GetByEmployee([FromQuery] Guid employeeId)
            => getHandler.GetAsync(employeeId);

        [HttpPost]
        public Task<Guid> Create([FromBody] SaveEmployeeDependentDto dto)
            => saveHandler.SaveAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveEmployeeDependentDto dto)
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
    }
}
