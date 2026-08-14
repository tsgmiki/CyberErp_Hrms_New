using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Handlers;
using CyberErp.Hrms.App.Features.Core.Modules;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.App.Features.Core.Modules.GetAll;
using CyberErp.Hrms.App.Features.Core.Modules.GetById;
using CyberErp.Hrms.App.Features.Core.Modules.GetOperations;
using CyberErp.Hrms.App.Features.Core.Operations.DTOs;
using CyberErp.Hrms.App.Features.Core.Operations.GetAll;
using CyberErp.Hrms.App.Features.Core.Operations.GetById;
using CyberErp.Hrms.App.Features.Core.Subsystems;
using Microsoft.AspNetCore.Mvc;

namespace CyberErp.Hrms.Api.Controllers.Core
{
    /*
     * Dynamic navigation — the sidebar is read from Core.Subsystem / Module / Operation rather than a
     * hardcoded frontend array, with TenantRolePermission driving per-role visibility.
     *
     * ⚠️ READ-ONLY SINCE 2026-08-14. Subsystems, Menu Modules, Menu Operations and Role Permissions
     * are MANAGED BY SRMS now, which runs against this same CERP database — the HRMS screens for them
     * were removed (handoff 0107). Every create/update/delete action went with them, including
     * Module/seed-defaults, which rewrote the whole tree.
     *
     * ⚠️ THE READS MUST STAY. They are not management; they are infrastructure every signed-in user
     * depends on:
     *
     *   GET Module/WithOperations  -> the sidebar feed itself
     *   GET Module, GET Subsystem  -> useMenuModules, the landing page, the menu filters
     *   GET Operation              -> permissionGate.tsx builds its catalogSet from this, and
     *                                 globalSearch.tsx filters results with it
     *
     * That last one is the trap worth remembering: PermissionGate treats "not in the catalog" as "not
     * a gated page", so losing this read would empty the catalog and let EVERY route through
     * unguarded. Removing the reads alongside the writes would have been a security regression, not a
     * cleanup.
     *
     * Do not add write actions back here. Menu changes are a request to SRMS, not a second writer
     * against a shared table.
     */

    /// <summary>Master list of ERP subsystems (Core.Subsystem). Read-only; SRMS owns the catalogue.</summary>
    public class SubsystemController(IGetAllSubsystems getAllHandler) : BaseController
    {
        /// <summary>Open read: the menu filters and the operation pickers populate from this.</summary>
        [HttpGet]
        public Task<PaginatedResponse<SubsystemDto>> GetAll([FromQuery] GetAllRequest request)
            => getAllHandler.GetAsync(request);
    }

    /// <summary>Menu modules (Core.Module) — the collapsible sidebar groups. Read-only.</summary>
    public class ModuleController(
        IFeatureHandler<GetAllModulesRequest, PaginatedResponse<GetModuleDto>> getAllHandler,
        IFeatureHandler<GetModuleByIdRequest, GetModuleDto?> getByIdHandler,
        IFeatureHandler<GetModuleWithOperationsRequest, IEnumerable<GetModuleWithOperationResult>> withOperationsHandler)
        : BaseController
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
    }

    /// <summary>Menu operations (Core.Operation) — the sidebar links. Read-only.</summary>
    public class OperationController(
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
    }
}
