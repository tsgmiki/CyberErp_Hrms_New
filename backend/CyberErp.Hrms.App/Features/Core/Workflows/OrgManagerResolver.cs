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

        /// <summary>
        /// Warm the employee→unit lookup for a whole batch in ONE query. Call before resolving managers
        /// for many employees (e.g. every row of an approval inbox); without it each employee costs a
        /// round-trip of its own.
        /// </summary>
        Task PreloadEmployeeUnitsAsync(IEnumerable<Guid> employeeIds);

        /// <summary>
        /// Everyone positioned in a unit this employee manages, including descendant units — the INVERSE
        /// of the manager climb. Lets a caller narrow "requests I might approve" to a SQL predicate
        /// instead of resolving a manager per row. A superset of the strict chain-of-command answer, so
        /// use it only to pre-filter candidates that <see cref="ResolveImmediateManagerAsync"/> (via
        /// EvaluateAsync) then confirms. Empty when the employee manages nothing.
        /// </summary>
        Task<HashSet<Guid>> EmployeesInMyManagedUnitsAsync(Guid employeeId);

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

        // ---- The org snapshot the climb runs against -----------------------------------------
        // The climb used to issue TWO queries per level (managers here? then who is my parent?) for
        // EVERY requester — and because the cache key carries the requester (self-exclusion), two
        // people in the same unit could not share a climb. An approval inbox over 2,000 running
        // instances spent ~3,800 queries and 5s resolving the same handful of units over and over.
        //
        // The inputs are tiny and stable within a request: the unit tree, and the MANAGERIAL employees
        // (a small subset). Load both once, then every climb is pure in-memory — 3 queries per request
        // regardless of how many instances are evaluated. Loaded lazily, so a request that never
        // resolves a manager pays nothing.
        private sealed record ManagerRow(Guid EmployeeId, string Name, List<Guid> UserIds);
        private Dictionary<Guid, Guid?>? _unitParent;              // unit -> parent
        private Dictionary<Guid, List<ManagerRow>>? _managersByUnit;

        private async Task EnsureOrgSnapshotAsync()
        {
            if (_unitParent is not null) return;

            _unitParent = await units.GetAll().AsNoTracking()
                .Select(u => new { u.Id, u.ParentId })
                .ToDictionaryAsync(x => x.Id, x => x.ParentId);

            // Same predicate the per-level query used, minus the requester exclusion — that is applied
            // in memory per climb, which is exactly what made the old cache un-shareable.
            var managers = await employees.GetAll().AsNoTracking()
                .Where(e => e.IsManagerial
                    && e.EmploymentStatus != EmploymentStatus.Terminated
                    && e.EmploymentStatus != EmploymentStatus.Suspended
                    && e.Position != null)
                .Select(e => new
                {
                    e.Id,
                    UnitId = e.Position!.OrganizationUnitId,
                    Name = e.Person != null
                        ? (e.Person.FirstName + " " + e.Person.GrandFatherName).Trim()
                        : e.EmployeeNumber
                })
                .ToListAsync();

            var managerIds = managers.Select(m => m.Id).Distinct().ToList();
            var userIdsByEmployee = managerIds.Count == 0
                ? []
                : (await users.GetAll().AsNoTracking()
                        .Where(u => u.EmployeeId != null && managerIds.Contains(u.EmployeeId.Value))
                        .Select(u => new { u.Id, EmployeeId = u.EmployeeId!.Value })
                        .ToListAsync())
                    .GroupBy(x => x.EmployeeId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            _managersByUnit = managers
                .GroupBy(m => m.UnitId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new ManagerRow(
                            m.Id, m.Name, userIdsByEmployee.GetValueOrDefault(m.Id) ?? []))
                          .ToList());
        }

        /// <summary>
        /// Resolve the org unit of many employees in ONE query. A caller that is about to resolve
        /// managers for a batch (the approval inbox, over every running instance) should call this
        /// first, otherwise each employee costs its own round-trip.
        /// </summary>
        public async Task PreloadEmployeeUnitsAsync(IEnumerable<Guid> employeeIds)
        {
            var missing = employeeIds.Where(id => id != Guid.Empty && !_employeeUnit.ContainsKey(id))
                .Distinct().ToList();
            if (missing.Count == 0) return;

            var rows = await employees.GetAll().AsNoTracking()
                .Where(e => missing.Contains(e.Id))
                .Select(e => new { e.Id, UnitId = e.Position != null ? (Guid?)e.Position.OrganizationUnitId : null })
                .ToListAsync();
            foreach (var r in rows) _employeeUnit[r.Id] = r.UnitId;
            // Employees the query did not return (deleted/inaccessible) are cached as "no unit" so a
            // later lookup does not re-query them one at a time.
            foreach (var id in missing) _employeeUnit.TryAdd(id, null);
        }

        private readonly Dictionary<Guid, Guid?> _employeeUnit = [];

        private async Task<Guid?> EmployeeUnitAsync(Guid employeeId)
        {
            if (_employeeUnit.TryGetValue(employeeId, out var cachedUnit)) return cachedUnit;
            var unit = await employees.GetAll().AsNoTracking()
                .Where(e => e.Id == employeeId)
                .Select(e => e.Position != null ? (Guid?)e.Position.OrganizationUnitId : null)
                .FirstOrDefaultAsync();
            _employeeUnit[employeeId] = unit;
            return unit;
        }

        public async Task<HashSet<Guid>> EmployeesInMyManagedUnitsAsync(Guid employeeId)
        {
            await EnsureOrgSnapshotAsync();

            // Units where this employee is one of the managerial staff — the roots of what they manage.
            var roots = _managersByUnit!
                .Where(kv => kv.Value.Any(m => m.EmployeeId == employeeId))
                .Select(kv => kv.Key)
                .ToHashSet();
            if (roots.Count == 0) return [];

            // Everything beneath those roots. Walks the parent map downward via a reverse index, with a
            // visited set so a cyclic parent chain cannot loop forever (same guard as the climb).
            var childrenOf = _unitParent!
                .Where(kv => kv.Value.HasValue)
                .GroupBy(kv => kv.Value!.Value)
                .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

            var scope = new HashSet<Guid>(roots);
            var queue = new Queue<Guid>(roots);
            while (queue.Count > 0)
            {
                var unit = queue.Dequeue();
                if (!childrenOf.TryGetValue(unit, out var kids)) continue;
                foreach (var child in kids)
                    if (scope.Add(child)) queue.Enqueue(child);
            }

            var scoped = await employees.GetAll().AsNoTracking()
                .Where(e => e.Position != null && scope.Contains(e.Position.OrganizationUnitId))
                .Select(e => e.Id)
                .ToListAsync();
            return scoped.ToHashSet();
        }

        public async Task<ResolvedManager?> ResolveImmediateManagerAsync(Guid requesterEmployeeId)
        {
            if (_immediateCache.TryGetValue(requesterEmployeeId, out var cached)) return cached;

            // The requester's org unit is derived from their assigned Position (not stored on Employee).
            var unitId = await EmployeeUnitAsync(requesterEmployeeId);
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
            await EnsureOrgSnapshotAsync();

            var visited = new HashSet<Guid>();
            Guid? unitId = startUnitId;

            // Identical walk to before — start unit, then parent, grandparent … stopping at the first
            // unit with an eligible manager, at the root, or on a cycle. Only the data source changed:
            // it reads the pre-loaded snapshot instead of querying at each level.
            while (unitId.HasValue && visited.Add(unitId.Value))
            {
                var managers = _managersByUnit!.TryGetValue(unitId.Value, out var rows)
                    // Self-exclusion applied HERE rather than in SQL, so the per-unit data is shared by
                    // every requester in that unit instead of being re-fetched for each of them.
                    ? rows.Where(m => m.EmployeeId != requesterEmployeeId).ToList()
                    : [];

                if (managers.Count > 0)
                {
                    return new ResolvedManager(
                        managers.Select(m => m.EmployeeId).ToList(),
                        string.Join(", ", managers.Select(m => m.Name)),
                        // Managers act through their login account(s) (User.EmployeeId link).
                        managers.SelectMany(m => m.UserIds).Distinct().ToList());
                }

                // No managerial employee positioned in this unit — escalate: parent → grandparent → …
                // An unknown unit id is treated as "no parent", matching the old FirstOrDefault → null.
                if (!_unitParent!.TryGetValue(unitId.Value, out var parent)) return null;
                unitId = parent;
            }

            return null; // reached the root (or a cycle) without finding an eligible manager
        }
    }
}
