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
    // instead of a hardcoded frontend array. TenantRolePermission rows drive per-role visibility.

    /*
     * ⚠️ WHY THE GATES ARE ON THE ACTIONS AND NOT THE CONTROLLERS (2026-08-13).
     *
     * These three used to carry no [RequirePermission] at all, so ANY authenticated user could
     * create, rename or delete menu entries. Gating the whole controller looks like the obvious fix
     * and is wrong: the READS here are infrastructure every signed-in user depends on.
     *
     *   GET Module/WithOperations  -> the sidebar feed itself
     *   GET Module, GET Subsystem  -> useMenuModules, the landing page, the menu filters
     *   GET Operation              -> permissionGate.tsx builds its catalogSet from this, and
     *                                 globalSearch.tsx filters results with it
     *
     * The last one is the trap. PermissionGate treats "not in the catalog" as "not a gated page", so
     * a 403 on this read would empty the catalog and every route would fall through UNGATED — the
     * fix would open a bigger hole than the one it closed.
     *
     * The reads expose menu metadata (names, links, icons, order), not anyone's data, and
     * WithOperations is already filtered to the caller's own grants. So: writes are gated, reads are
     * not, and that is deliberate.
     */

    /// <summary>Master list of ERP subsystems (Core.Subsystem); modules reference one by name.</summary>
    public class SubsystemController(
        ISaveSubsystem saveHandler,
        IGetAllSubsystems getAllHandler,
        IDeleteSubsystem deleteHandler) : BaseController
    {
        /// <summary>Open read: the module/operation forms and the menu filters populate from this.</summary>
        [HttpGet]
        public Task<PaginatedResponse<SubsystemDto>> GetAll([FromQuery] GetAllRequest request) => getAllHandler.GetAsync(request);

        [HttpPost]
        [RequirePermission("subsystem")]
        public async Task<IActionResult> Create([FromBody] SubsystemDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpPut]
        [RequirePermission("subsystem")]
        public async Task<IActionResult> Update([FromBody] SubsystemDto dto) => Ok(new { id = await saveHandler.SaveAsync(dto) });

        [HttpDelete("{id:guid}")]
        [RequirePermission("subsystem")]
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

        /// <summary>
        /// The navigation feed: the caller's modules + operations with role permissions applied.
        /// Deliberately UNGATED — it IS the sidebar, and it already returns only what the caller may
        /// see. Gating it would leave every user with no menu at all.
        /// </summary>
        [HttpGet("WithOperations")]
        public Task<IEnumerable<GetModuleWithOperationResult>> WithOperations([FromQuery] GetModuleWithOperationsRequest request) =>
            withOperationsHandler.Handle(request);

        [HttpGet("{id:guid}")]
        public Task<GetModuleDto?> GetById(Guid id) => getByIdHandler.Handle(new GetModuleByIdRequest(id));

        [HttpPost]
        [RequirePermission("module")]
        public async Task<IActionResult> Create([FromBody] CreateModuleRequest request) =>
            Ok(await createHandler.Handle(request));

        [HttpPut]
        [RequirePermission("module")]
        public async Task<IActionResult> Update([FromBody] UpdateModuleRequest request) =>
            Ok(await updateHandler.Handle(request));

        [HttpDelete("{id:guid}")]
        [RequirePermission("module")]
        public async Task<IActionResult> Delete(Guid id)
        { await deleteHandler.Handle(new DeleteModuleRequest(id)); return Ok(new { message = "Deleted successfully" }); }

        /// <summary>
        /// Seeds the default HRMS menu (subsystem, modules, operations) for the current tenant.
        /// Rewrites the whole navigation tree, so it is gated the same as editing it by hand.
        /// </summary>
        [HttpPost("seed-defaults")]
        [RequirePermission("module")]
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
        /// <summary>
        /// Open read — and it must stay open. <c>permissionGate.tsx</c> builds its catalog of gated
        /// routes from this, and a 403 here would empty that catalog, which the gate reads as "no
        /// route is gated" and lets everything through.
        /// </summary>
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
        [RequirePermission("operation")]
        public async Task<IActionResult> Create([FromBody] CreateOperationRequest request) =>
            Ok(await createHandler.Handle(request));

        [HttpPut]
        [RequirePermission("operation")]
        public async Task<IActionResult> Update([FromBody] UpdateOperationRequest request) =>
            Ok(await updateHandler.Handle(request));

        [HttpDelete("{id:guid}")]
        [RequirePermission("operation")]
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
