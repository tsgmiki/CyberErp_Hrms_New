using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace CyberErp.Hrms.App.Features.Core.Workflows
{
    /// <summary>
    /// A dynamically-resolved approver set: the managerial employee(s) of the matched unit plus
    /// their login account(s). <see cref="Name"/> is display-ready (multiple managers joined).
    /// </summary>
    public record ResolvedManager(IReadOnlyList<Guid> EmployeeIds, string Name, IReadOnlyList<Guid> UserIds);

    /// <summary>
    /// Resolves DYNAMIC workflow approvers from the organizational structure at decision time.
    /// The manager of a unit is RELATIONAL — no direct unit→manager link exists: it is any employee
    /// with <see cref="Employee.IsManagerial"/> = true whose <see cref="Position"/> belongs to that
    /// unit (Employee → PositionId → Position.OrganizationUnitId). "Immediate Manager" starts at the
    /// requester's own unit (via their Position); "Unit Manager" starts at a configured unit. Both
    /// climb <see cref="OrganizationUnit.ParentId"/> recursively: a unit with no managerial employee
    /// — or where the requester is the only one (no self-approval) — escalates to its parent unit,
    /// then the grandparent, until eligible managers are found or the tree root is passed.
    /// </summary>
    public interface IOrgManagerResolver
    {
        /// <summary>The requester's effective manager(s), or null when none exist up the chain.</summary>
        Task<ResolvedManager?> ResolveImmediateManagerAsync(Guid requesterEmployeeId);

        /// <summary>The effective manager(s) anchored at a specific unit (e.g. "Finance Head"), same climb rules.</summary>
        Task<ResolvedManager?> ResolveUnitManagerAsync(Guid organizationUnitId, Guid? requesterEmployeeId);

        /// <summary>The employee's org unit (via their Position), or null when unplaced — for diagnostics.</summary>
        Task<(Guid Id, string Name)?> GetEmployeeUnitAsync(Guid employeeId);
    }

    public class OrgManagerResolver(
        IRepository<OrganizationUnit> units,
        IRepository<Employee> employees,
        IRepository<User> users) : IOrgManagerResolver
    {
        public async Task<ResolvedManager?> ResolveImmediateManagerAsync(Guid requesterEmployeeId)
        {
            // The requester's org unit is derived from their assigned Position (not stored on Employee).
            var unitId = await employees.GetAll()
                .Where(e => e.Id == requesterEmployeeId)
                .Select(e => e.Position != null ? (Guid?)e.Position.OrganizationUnitId : null)
                .FirstOrDefaultAsync();
            if (unitId is null) return null; // unplaced employee — nothing to traverse

            return await ClimbAsync(unitId.Value, requesterEmployeeId);
        }

        public Task<ResolvedManager?> ResolveUnitManagerAsync(Guid organizationUnitId, Guid? requesterEmployeeId) =>
            ClimbAsync(organizationUnitId, requesterEmployeeId);

        public async Task<(Guid Id, string Name)?> GetEmployeeUnitAsync(Guid employeeId)
        {
            var unit = await employees.GetAll()
                .Where(e => e.Id == employeeId && e.Position != null)
                .Select(e => new { e.Position!.OrganizationUnitId, e.Position.OrganizationUnit!.Name })
                .FirstOrDefaultAsync();
            return unit is null ? null : (unit.OrganizationUnitId, unit.Name);
        }

        /// <summary>
        /// The recursive chain-of-command traversal. From <paramref name="startUnitId"/> upward:
        ///   1. Find the unit's managers via the relational join —
        ///      hrms_Employee (IsManagerial = 1, active) → PositionId → hrms_Position.OrganizationUnitId = unit —
        ///      excluding the requester (no self-approval).
        ///   2. Any found → resolved (all are eligible approvers).
        ///   3. Otherwise escalate to the parent unit and repeat (grandparent, etc.).
        ///   4. Stop past the root — or on a cycle (visited-set guard) — and return null.
        /// </summary>
        private async Task<ResolvedManager?> ClimbAsync(Guid startUnitId, Guid? requesterEmployeeId)
        {
            var visited = new HashSet<Guid>();
            Guid? unitId = startUnitId;

            while (unitId.HasValue && visited.Add(unitId.Value))
            {
                var managers = await employees.GetAll()
                    .Where(e => e.IsManagerial
                        && e.Id != requesterEmployeeId
                        && e.EmploymentStatus != EmploymentStatus.Terminated
                        && e.EmploymentStatus != EmploymentStatus.Suspended
                        && e.Position != null
                        && e.Position.OrganizationUnitId == unitId.Value)
                    .Select(e => new
                    {
                        e.Id,
                        Name = e.Person != null
                            ? (e.Person.FirstName + " " + e.Person.GrandFatherName).Trim()
                            : e.EmployeeNumber
                    })
                    .ToListAsync();

                if (managers.Count > 0)
                {
                    var managerIds = managers.Select(m => m.Id).ToList();
                    // Managers act through their login account(s) (User.EmployeeId link).
                    var userIds = await users.GetAll()
                        .Where(u => u.EmployeeId != null && managerIds.Contains(u.EmployeeId.Value))
                        .Select(u => u.Id)
                        .ToListAsync();
                    return new ResolvedManager(
                        managerIds,
                        string.Join(", ", managers.Select(m => m.Name)),
                        userIds);
                }

                // No managerial employee positioned in this unit — escalate: parent → grandparent → …
                var parentId = await units.GetAll()
                    .Where(u => u.Id == unitId.Value)
                    .Select(u => new { u.ParentId })
                    .FirstOrDefaultAsync();
                if (parentId is null) return null;
                unitId = parentId.ParentId;
            }

            return null; // reached the root (or a cycle) without finding an eligible manager
        }
    }
}
