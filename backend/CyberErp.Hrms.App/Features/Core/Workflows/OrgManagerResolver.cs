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

        /// <summary>The requester's second-level manager(s) — the immediate manager's own manager (two-hop
        /// climb). Null when there is no immediate manager, or none above them.</summary>
        Task<ResolvedManager?> ResolveSecondLevelManagerAsync(Guid requesterEmployeeId);

        /// <summary>The effective manager(s) anchored at a specific unit (e.g. "Finance Head"), same climb rules.</summary>
        Task<ResolvedManager?> ResolveUnitManagerAsync(Guid organizationUnitId, Guid? requesterEmployeeId);

        /// <summary>The employee's org unit (via their Position), or null when unplaced — for diagnostics.</summary>
        Task<(Guid Id, string Name)?> GetEmployeeUnitAsync(Guid employeeId);

        /// <summary>True when the employee holds a managerial position (<see cref="Employee.IsManagerial"/>).
        /// Lets the workflow engine tell "no manager above me because I'm at the top of the chain" (a CEO —
        /// bypass the manager step) from "no manager anywhere" (an org-data gap — surface the error).</summary>
        Task<bool> IsManagerialAsync(Guid employeeId);
    }

    public class OrgManagerResolver(
        IRepository<OrganizationUnit> units,
        IRepository<Employee> employees,
        IRepository<User> users) : IOrgManagerResolver
    {
        // Per-request memoization. This resolver is Scoped (one instance per request) and the org
        // structure is stable within a request, so caching resolution results is correct and never
        // leaks across requests. This collapses the multiplicative resolver calls the workflow inbox /
        // tracking handlers make (one+ per row, each an org-tree climb) to one lookup per distinct key.
        private readonly Dictionary<Guid, bool> _managerialCache = [];
        private readonly Dictionary<Guid, ResolvedManager?> _immediateCache = [];
        private readonly Dictionary<(Guid Unit, Guid Requester), ResolvedManager?> _climbCache = [];

        public async Task<ResolvedManager?> ResolveImmediateManagerAsync(Guid requesterEmployeeId)
        {
            if (_immediateCache.TryGetValue(requesterEmployeeId, out var cached)) return cached;

            // The requester's org unit is derived from their assigned Position (not stored on Employee).
            var unitId = await employees.GetAll()
                .Where(e => e.Id == requesterEmployeeId)
                .Select(e => e.Position != null ? (Guid?)e.Position.OrganizationUnitId : null)
                .FirstOrDefaultAsync();
            // unplaced employee (null unit) → nothing to traverse.
            var result = unitId is null ? null : await ClimbAsync(unitId.Value, requesterEmployeeId);
            _immediateCache[requesterEmployeeId] = result;
            return result;
        }

        public async Task<ResolvedManager?> ResolveSecondLevelManagerAsync(Guid requesterEmployeeId)
        {
            // First hop: the requester's immediate manager(s). Second hop: resolve the manager's own
            // manager (anchored on that manager's employee id, which also excludes them from their own set).
            var direct = await ResolveImmediateManagerAsync(requesterEmployeeId);
            if (direct is null || direct.EmployeeIds.Count == 0) return null;

            var employeeIds = new List<Guid>();
            var names = new List<string>();
            var userIds = new List<Guid>();
            foreach (var managerEmpId in direct.EmployeeIds)
            {
                var second = await ResolveImmediateManagerAsync(managerEmpId);
                if (second is null) continue;
                foreach (var id in second.EmployeeIds) if (!employeeIds.Contains(id)) employeeIds.Add(id);
                foreach (var uid in second.UserIds) if (!userIds.Contains(uid)) userIds.Add(uid);
                if (!string.IsNullOrWhiteSpace(second.Name)) names.Add(second.Name);
            }
            return employeeIds.Count == 0 ? null : new ResolvedManager(employeeIds, string.Join(", ", names.Distinct()), userIds);
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

        public async Task<bool> IsManagerialAsync(Guid employeeId)
        {
            if (_managerialCache.TryGetValue(employeeId, out var cached)) return cached;
            var result = await employees.GetAll().AnyAsync(e => e.Id == employeeId && e.IsManagerial);
            _managerialCache[employeeId] = result;
            return result;
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
            // The climb result depends on both the start unit AND the requester (self-exclusion), so
            // the cache key carries both.
            var key = (startUnitId, requesterEmployeeId ?? Guid.Empty);
            if (_climbCache.TryGetValue(key, out var cached)) return cached;
            var result = await ClimbCoreAsync(startUnitId, requesterEmployeeId);
            _climbCache[key] = result;
            return result;
        }

        private async Task<ResolvedManager?> ClimbCoreAsync(Guid startUnitId, Guid? requesterEmployeeId)
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
