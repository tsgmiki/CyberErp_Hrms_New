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

        // Get modules with operations
        var modules = await _moduleRepository.GetAll()
            .Include(m => m.Operations)
            .ToListAsync(ct);

        var result = modules
            .Select(m => new GetModuleWithOperationResult
            {
                Id = m.Id,
                Name = m.Name ?? string.Empty,
                SubSystem = m.SubSystem ?? string.Empty,
                Operations = (m.Operations ?? new List<Operation>())
                    .Select(op =>
                    {
                        var permission = rolePermissions.FirstOrDefault(rp => rp.OperationId == op.Id);
                        return new OperationRecord
                        {
                            Id = op.Id,
                            Name = op.Name ?? string.Empty,
                            Link = op.Link ?? string.Empty,
                            Icon = op.Icon ?? string.Empty,
                            CanAdd = permission?.CanAdd ?? false,
                            CanEdit = permission?.CanEdit ?? false,
                            CanDelete = permission?.CanDelete ?? false,
                            CanApprove = permission?.CanApprove ?? false,
                            CanView = permission?.CanView ?? true

                        };
                    })
                    .ToList()
            })
            .Where(m => m.Operations.Any())
            .ToList();

        return result;
    }
}