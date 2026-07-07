using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.App.Features.Core.Employees.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    public class EmployeeController(
        ICreateEmployee createHandler,
        IUpdateEmployee updateHandler,
        IDeleteEmployee deleteHandler,
        IGetEmployeeById getByIdHandler,
        IGetAllEmployees getAllHandler,
        IUploadEmployeePhoto uploadPhotoHandler,
        IGetEmployeePhoto getPhotoHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<EmployeeDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);

        [HttpGet("{id:guid}")]
        public Task<EmployeeDto> GetById(Guid id)
            => getByIdHandler.GetAsync(id);

        [HttpPost]
        public Task<Guid> Create([FromBody] CreateEmployeeDto dto)
            => createHandler.CreateAsync(dto);

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEmployeeDto dto)
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

        /// <summary>Uploads / replaces the employee photo (JPG, PNG or WEBP, max 2 MB).</summary>
        [HttpPost("{id:guid}/photo")]
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file)
        {
            if (file is null) return BadRequest(new { message = "No file provided." });
            await using var stream = file.OpenReadStream();
            await uploadPhotoHandler.UploadAsync(id, stream, file.FileName, file.Length);
            return Ok(new { message = "Photo uploaded" });
        }

        /// <summary>Streams the employee photo.</summary>
        [HttpGet("{id:guid}/photo")]
        public async Task<IActionResult> GetPhoto(Guid id)
        {
            var (content, contentType) = await getPhotoHandler.GetAsync(id);
            return File(content, contentType);
        }
    }
}
