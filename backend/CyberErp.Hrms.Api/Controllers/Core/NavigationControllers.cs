using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Features.Core.Modules;
using CyberErp.Hrms.App.Features.Core.Modules.Create;
using CyberErp.Hrms.App.Features.Core.Modules.Delete;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.App.Features.Core.Modules.GetAll;
using CyberErp.Hrms.App.Features.Core.Modules.GetById;
using CyberErp.Hrms.App.Features.Core.Modules.GetOperations;
using CyberErp.Hrms.App.Features.Core.Operations.Create;
using CyberErp.Hrms.App.Features.Core.Operations.Delete;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.App.Features.Core.Operations.GetAll;
using CyberErp.Hrms.App.Features.Core.Operations.GetById;
using CyberErp.Hrms.App.Features.Core.Operations.Update;
using CyberErp.Hrms.App.Features.Core.Roles;
using CyberErp.Hrms.App.Features.Core.Subsystems;
using Microsoft.AspNetCore.Mvc;
using UpdateModuleRequest = CyberErp.Hrms.App.Features.Core.Modules.Update.UpdateModuleRequest;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    // Dynamic navigation — the sidebar menu is read from coreSubsystem / Module / Operation
    // instead of a hardcoded frontend array. RolePermission rows drive per-role visibility.

    /// <summary>Master list of ERP subsystems (dbo.coreSubsystem); modules reference one by name.</summary>
    public class SubsystemController(
        ISaveSubsystem saveHandler,
        IGetAllSubsystems getAllHandler,
        IDeleteSubsystem deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<SubsystemDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubsystemDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SubsystemDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Menu modules (Core.Module) — the collapsible sidebar groups.</summary>
    public class ModuleController(
        IFeatureHandler<CreateModuleRequest, ModuleResult> createHandler,
        IFeatureHandler<UpdateModuleRequest, ModuleResult> updateHandler,
        IFeatureHandler<DeleteModuleRequest, ModuleResult?> deleteHandler,
        IFeatureHandler<GetAllModulesRequest, PaginatedResponse<GetModuleDto>> getAllHandler,
        IFeatureHandler<GetModuleByIdRequest, GetModuleDto?> getByIdHandler,
        IFeatureHandler<GetModuleWithOperationsRequest, IEnumerable<GetModuleWithOperationResult>> withOperationsHandler,
        ISeedDefaultMenu seedHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<GetModuleDto>> GetAll([FromQuery] GetAllModulesRequest request) =>
            getAllHandler.Handle(request);

        /// <summary>The navigation feed: the caller's modules + operations with role permissions applied.</summary>
        [HttpGet("WithOperations")]
        public Task<IEnumerable<GetModuleWithOperationResult>> WithOperations([FromQuery] GetModuleWithOperationsRequest request) =>
            withOperationsHandler.Handle(request);

        [HttpGet("{id:guid}")]
        public Task<GetModuleDto?> GetById(Guid id) => getByIdHandler.Handle(new GetModuleByIdRequest(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateModuleRequest request) =>
            Ok(await createHandler.Handle(request));

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateModuleRequest request) =>
            Ok(await updateHandler.Handle(request));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.Handle(new DeleteModuleRequest(id)); return Ok(new { message = "Deleted successfully" }); }

        /// <summary>Seeds the default HRMS menu (subsystem, modules, operations) for the current tenant.</summary>
        [HttpPost("seed-defaults")]
        public async Task<IActionResult> SeedDefaults()
        {
            var created = await seedHandler.SeedAsync();
            return Ok(new { created, message = created > 0 ? $"{created} navigation entries created" : "Menu already seeded" });
        }
    }

    /// <summary>Menu operations (Core.Operation) — the sidebar links under each module.</summary>
    public class OperationController(
        IFeatureHandler<CreateOperationRequest, OperationResult> createHandler,
        IFeatureHandler<UpdateOperationRequest, OperationResult> updateHandler,
        IFeatureHandler<DeleteOperationRequest, OperationResult?> deleteHandler,
        IFeatureHandler<GetAllOperationsRequest, PaginatedResponse<OperationDto>> getAllHandler,
        IFeatureHandler<GetOperationByIdRequest, OperationDto?> getByIdHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<OperationDto>> GetAll([FromQuery] GetAllOperationsRequest request) =>
            getAllHandler.Handle(request);

        /// <summary>Alias kept for the role-permission screen's service contract.</summary>
        [HttpGet("ByRole")]
        public Task<PaginatedResponse<OperationDto>> ByRole([FromQuery] GetAllOperationsRequest request) =>
            getAllHandler.Handle(request);

        [HttpGet("{id:guid}")]
        public Task<OperationDto?> GetById(Guid id) => getByIdHandler.Handle(new GetOperationByIdRequest(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOperationRequest request) =>
            Ok(await createHandler.Handle(request));

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateOperationRequest request) =>
            Ok(await updateHandler.Handle(request));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.Handle(new DeleteOperationRequest(id)); return Ok(new { message = "Deleted successfully" }); }
    }

    /// <summary>Per-role operation permissions (Core.RolePermission) — drives menu visibility.</summary>
    [RequirePermission("rolePermission")]
    public class RolePermissionController(
        ISaveRolePermissions saveHandler,
        IGetAllRolePermissions getAllHandler,
        IDeleteRolePermission deleteHandler) : BaseController
    {
        [HttpGet]
        public Task<PaginatedResponse<RolePermissionDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        /// <summary>Bulk upsert — carries one role's whole permission grid.</summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveRolePermissionsDto dto) =>
            Ok(new { saved = await saveHandler.SaveAsync(dto), message = "Permissions saved" });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.DeleteAsync(id); return Ok(new { message = "Deleted successfully" }); }
    }
}
