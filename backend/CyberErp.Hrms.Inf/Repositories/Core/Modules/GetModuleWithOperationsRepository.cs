using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Modules.DTOs;
using CyberErp.Hrms.App.Features.Core.Modules.GetOperations;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Repositories.Core.Modules;

public class GetModuleWithOperationsRepository(
    IRepository<Module> moduleRepository,
    IRepository<UserRole> userRoleRepository,
    IRepository<RolePermission> rolePermissionRepository,
    ILogger<GetModuleWithOperationsRepository> logger) : IGetModuleWithOperationsRepository
{
    private readonly IRepository<Module> _moduleRepository = moduleRepository;
    private readonly IRepository<UserRole> _userRoleRepository = userRoleRepository;
    private readonly IRepository<RolePermission> _rolePermissionRepository = rolePermissionRepository;
    private readonly ILogger<GetModuleWithOperationsRepository> _logger = logger;

    public async Task<IEnumerable<GetModuleWithOperationResult>> GetAsync(Guid? userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting Modules with Operations for user {UserId}", userId);

        // Get user's role IDs
        var userRoleIds = new List<Guid>();
        if (userId.HasValue)
        {
            userRoleIds = await _userRoleRepository.GetAll()
                .Where(ur => ur.UserId == userId.Value)
                .Select(ur => ur.RoleId)
                .ToListAsync(ct);
        }

        // Get role permissions for the user's roles
        var rolePermissions = await _rolePermissionRepository.GetAll()
            .Where(rp => userRoleIds.Contains(rp.RoleId))
            .ToListAsync(ct);

        // Strictly role-based (deny-by-default): an operation appears ONLY when one of the user's
        // roles grants CanView on it. No branch/head-office bypass — "admin" = a role granted the
        // permissions, NOT a user who happens to have no branch (IsHeadOffice is a branch-data flag).
        // Get modules with operations, in menu order
        var modules = await _moduleRepository.GetAll()
            .Include(m => m.Operations)
            .Include(m => m.Subsystem)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
            .ToListAsync(ct);

        var result = modules
            .Select(m => new GetModuleWithOperationResult
            {
                Id = m.Id,
                Name = m.Name ?? string.Empty,
                SubsystemId = m.SubsystemId,
                SubSystem = m.Subsystem?.Name ?? string.Empty,
                Icon = m.Icon,
                SortOrder = m.SortOrder,
                Operations = (m.Operations ?? new List<Operation>())
                    .OrderBy(op => op.SortOrder).ThenBy(op => op.Name)
                    .Select(op =>
                    {
                        var permission = rolePermissions.FirstOrDefault(rp => rp.OperationId == op.Id);
                        return new OperationRecord
                        {
                            Id = op.Id,
                            Name = op.Name ?? string.Empty,
                            Link = op.Link ?? string.Empty,
                            Icon = op.Icon ?? string.Empty,
                            SortOrder = op.SortOrder,
                            CanAdd = permission?.CanAdd ?? false,
                            CanEdit = permission?.CanEdit ?? false,
                            CanDelete = permission?.CanDelete ?? false,
                            CanApprove = permission?.CanApprove ?? false,
                            CanView = permission?.CanView ?? false
                        };
                    })
                    .Where(op => op.CanView)   // hide operations the role can't view
                    .ToList()
            })
            .Where(m => m.Operations.Any())    // drop modules left with no visible operations
            .ToList();

        return result;
    }
}