# logic.md — Core System Logic

> **Living document.** Core system logic, the dynamic workflow engine, multi-step approval processes,
> and database entity relationships. Update when logic changes (enforced by `.githooks/pre-commit`).
> State/goals live in `memory.md`; session handoff in `handoff.md`.

---

## 1. Dynamic workflow engine (generic, module-agnostic)

The single approval mechanism for **all** processes (personnel movements, disciplinary, termination,
leave — and future overtime/regularization). Adding a workflow-backed process needs **no engine changes**.

**Entities** (schema `Core`, `hrms_` prefix):
- `WorkflowDefinition` — `Name`, **`EntityType`** key (e.g. `"EmployeeMovement.Transfer"`, `"LeaveRequest"`;
  constants in `WorkflowEntityTypes`), `IsActive`. Save-guard: one *active* definition per `EntityType`.
- `WorkflowStep` — `DefinitionId` (cascade), `StepOrder`, `Name`, `ApproverRole`.
- `WorkflowStepApprover` — `StepId` (cascade), `ApproverType` (`User`|`Role`), `ApproverId`, `DisplayName`
  (resolved server-side). Empty approvers = open step (anyone may approve).
- `WorkflowInstance` — governs one record: `EntityType` + `EntityId`, optional `EmployeeId`, precomputed
  **`Summary`** (tracking never joins module tables), `Status` (`Running`/`Approved`/`Rejected`),
  `CurrentStepOrder`/`Name`, `TotalSteps`, `RequestedBy`, `CompletedAt`.
- `WorkflowActionLog` — per-decision history (`Submitted`/`Approved`/`Rejected`, comment, ActedBy/At).

**Extension point** — `IWorkflowEntityHandler` (`WorkflowService.cs`):
```
bool Supports(string entityType);                     // exact or prefix match on the EntityType key
Task OnApprovedAsync(string entityType, Guid entityId);   // apply the approved outcome
Task OnRejectedAsync(string entityType, Guid entityId);   // apply the rejected outcome
```
Registered `services.AddScoped<IWorkflowEntityHandler, XxxHandler>()`; engine injects
`IEnumerable<IWorkflowEntityHandler>` and picks the first whose `Supports` returns true.
Handlers: `EmployeeMovementWorkflowHandler`, `DisciplinaryMeasureWorkflowHandler`,
`EmployeeTerminationWorkflowHandler`, `LeaveRequestWorkflowHandler`, `AnnualLeaveWorkflowHandler`,
`CareerPathChangeRequestWorkflowHandler`, `SuccessionPlanWorkflowHandler`, `TalentReviewWorkflowHandler`,
`CriticalPositionWorkflowHandler`,
`SalaryRevisionWorkflowHandler`, `MedicalClaimWorkflowHandler`, `InsuranceClaimWorkflowHandler`,
`LoanWorkflowHandler`, `TripRequestWorkflowHandler`, `TrainingNeedWorkflowHandler`,
`RewardNominationWorkflowHandler` (+ the appraisal flow, which drives the engine via
`AdvanceToStepAsync`).

**Succession Plan approval (HC160, 2026-07-22).** `EntityType = "SuccessionPlan"`. Statuses:
the 3 operational ones (`Active`/`OnHold`/`Closed`) plus workflow-owned `PendingApproval`/`Rejected`.
With an active definition, `SaveSuccessionPlan` forces a created plan to `PendingApproval` and starts
the instance (`employeeId: null` — a plan is position-scoped, so definitions must not use
Subject/Immediate-Manager dynamic approvers); approve → `Active`, reject → `Rejected` (still
editable — saving a Rejected plan RESUBMITS it through the chain; the requested status is ignored so
approval can't be bypassed). Update/delete are gated by `EnsureNoRunningAsync` while an instance runs.
No definition → plans save directly with the requested status (engine's opt-in philosophy).

**Talent Review approval (HC149, 2026-07-22).** `EntityType = "TalentReview"`. Mirrors the
succession-plan flow (force-pending on create, gates, resubmit-on-reject) with two differences:
(1) **approve → `InProgress`** — approval opens calibration directly; `Draft` is only reachable in
direct (no-definition) mode; (2) **`SaveTalentAssessment` rejects (400) while the review is
`PendingApproval` or `Rejected`** — 9-box calibration is the review's substance and must not
proceed under an unapproved session.

**Critical Position approval (HC151, 2026-07-22).** `EntityType = "CriticalPosition"`. Same flow,
with a dedicated `Status` column added by migration `AddCriticalPositionApprovalStatus` (default
`Active` — legacy rows stay operational). The approval state drives the operational flag:
**pending/rejected force `IsActive = false`** so active-only feeds exclude unapproved flags;
approve → `Active` + `IsActive = true`. The Save DTO carries no status — it is fully
workflow-owned. Downstream gate: `SaveSuccessionPlan` refuses (400) to anchor a plan to a
Pending/Rejected critical position — succession planning starts only on an approved flag.

**Service API** — `IWorkflowService`:
```
Task StartIfDefinedAsync(string entityType, Guid entityId, Guid? employeeId, string summary);
Task ApproveAsync(Guid instanceId, string? comment);
Task RejectAsync(Guid instanceId, string? comment);
```
- `StartIfDefinedAsync`: called by a module **after** persisting its record. If **no active definition**
  exists for the entity type → **no-op** (the module operates directly, i.e. workflow is opt-in per process).
  Skips if a `Running` instance already exists. Logs a "Submitted" action.
- `IWorkflowGate.EnsureNoRunningAsync(prefix, entityId)` → throws 400 while an approval is in flight;
  modules call it at the top of edit/execute/cancel/delete handlers.
- `IWorkflowApproverAuth`: `CanDecide` = the current user matches a step user-approver OR intersects a
  step role via `Core.UserRole`. Enforced in Approve/Reject; the tracking list batch-computes per row.
- **Approver inbox** — `GET Workflow/my-approvals` (`GetMyApprovals`) → `{ IsApprover, Items }`:
  `IsApprover` = the user is a *specific* approver (user or role) on any **active** definition's step
  (drives the conditional Dashboard **Approvals** tab); `Items` = Running instances whose **current
  step** lists them specifically. **Open steps — CHANGED 2026-08-05:** a step with no approver rows
  means "anyone may act" (`EvaluateAsync` returns `(true, [])`), and such instances are now **included**
  for users who hold `CanApprove` on the `/workflow` operation — previously they were excluded from
  every personal inbox, so a request on an open step appeared in nobody's queue. See
  *Open-step audience* below. The Dashboard tab (next to Upcoming Retirements/Clearance) has
  prominent Approve/Reject buttons opening a comment modal (**reason required to reject**); decisions
  call the standard `Workflow/{id}/approve|reject`. This is *the* approver entry point — without it,
  assigned approvers had no cue that work was waiting (actioning only existed on the tracking page).

**Seeded defaults** (`SeedDefaultWorkflows`, `POST /api/v1/WorkflowDefinition/seed-defaults`, idempotent):
Transfer / Promotion / Demotion / Disciplinary (Supervisor Review → HR Approval), Termination
(Manager → HRBP → Dept Head), **Leave Approval (Supervisor Review → HR Approval)**.
⚠️ **Seeded steps carry NO approvers** — `SetSteps` takes only `(name, description)`, so every default
chain ships as open steps. That is why open-step behaviour (below) is the norm, not an edge case.

### Portal alerts + the open-step audience (2026-08-05)

The engine raises alerts into the Home portal's `dbo.Core.Notification` via `IPortalNotifier`
(`Inf/Common/PortalNotifier.cs`), correlated by `SourceEntityType = "WorkflowInstance"` +
`SourceEntityId = instance.Id`:

| When | What happens |
|---|---|
| `StartIfDefinedAsync` | alert the entry step's audience |
| advance to next step | resolve the previous step's alerts, then alert the new step's audience |
| terminal approve/reject | resolve the instance's outstanding alerts |

All of it is **best-effort** — wrapped in try/catch and logged; an alert failure must never break the
governing operation.

**The audience rule.** Both the alert and the inbox derive the recipient set the SAME way, deliberately:

```
explicit approvers of the current step        (User / Role / Immediate|SecondLevel|Unit Manager / Subject)
  └─ if that set is EMPTY (an open step) →    users whose roles hold CanApprove on the /workflow operation
       └─ if that is ALSO empty →             log a WARNING naming the cause; never fail silently
```

`IWorkflowApproverAuth.ResolveOpenStepRecipientsAsync()` (who to alert) and `CanActOnOpenStepsAsync()`
(does it belong in *my* inbox) implement the fallback from that one rule, so **the people told about a
request are exactly the people who then find it waiting**. Deriving them separately is precisely how
the original defect arose: the system said *anyone may approve this*, then alerted nobody and showed it
to nobody — silent, with no error and no log. Hiring Requests and Other Leave were the visible victims.

The fallback is a **safety net, not routing** — configuring real approvers on a step takes precedence
automatically. Note `EnsureCanDecideAsync` still lets ANYONE decide an open step (unchanged): the new
rule bounds who is *told*, not who *may act*.

### Dynamic clearance configuration (offboarding — mirrors the workflow approver pattern)
`Hrms.ClearanceDepartment` (Name, Description = checklist requirement text, SortOrder, IsActive) +
child `Hrms.ClearanceDepartmentApprover` (ApproverType User|Role — reuses `WorkflowApproverType` —,
ApproverId, server-resolved DisplayName). Admin UI `/clearanceDepartment` (System group), controller
`ClearanceDepartmentController`, slices in `Features/Core/ClearanceDepartments/`.
- **Checklist build:** `TerminationShared.BeginClearanceAsync` reads *active* departments (ordered
  SortOrder, Name) and stamps each `Hrms.TerminationClearance` row with `DepartmentId`; when **none**
  are configured it falls back to the built-in IT/Store/Finance defaults (DepartmentId null).
  Deleting a department SET NULLs existing checklist rows (they revert to open).
- **Authorization:** `TerminationShared.EvaluateClearanceApproverAsync` — no approvers = open
  (anyone); otherwise the current user must be a listed **user** approver or hold a listed **role**
  (via `Core.UserRole`). **Any single authorized user's decision clears the department.** Enforced
  server-side in `UpdateTerminationClearance` (400 listing authorized names); surfaced per row as
  `CanDecide` + `ApproverNames` on `TerminationClearanceDto` (batch-computed in
  `GetEmployeeTerminations`, like the workflow tracking list). Legacy rows without `DepartmentId`
  match a configured department **by name** (active only).
- **Where approvers act — the Dashboard "Clearance" tab (not the termination tab).** Approvers work
  their queue from a **conditionally-rendered** Dashboard tab next to Upcoming Retirements:
  `GET EmployeeTermination/my-clearances` (`GetMyClearances`) returns `{ IsApprover, Items }` —
  `IsApprover` = the user is a *specific* approver (user or role) on any **active** department
  (drives tab visibility); `Items` = every **outstanding** (not-Cleared) clearance in in-progress
  cases for a department they specifically approve (open/no-approver departments are excluded — they
  belong to no one). Each row shows identity + two prominent **Clear / Block** buttons; clicking one
  opens a **decision modal** with a large remarks textarea + Confirm (the remark is captured there,
  not inline — modal stays open on a 400 so the approver can retry). The employee's **termination tab
  clearance checklist is read-only** (progress view only).
- **Settlement gate (`FinalizeEmployeeTermination`):** HR can settle only after **all assigned
  approvers** finish. Concretely: (1) any **Blocked** clearance halts settlement; (2) every clearance
  whose department has ≥1 configured approver must be **Cleared** (a Cleared item implies an
  authorized approver signed it — clearing is authorization-gated); (3) remaining not-Cleared items
  belong to departments with **no** approver (nobody to sign them) and are **auto-cleared on
  settlement** with a `system` note. Then `MarkSettled(vacatedPositionId)` (all-cleared invariant
  holds) + terminate + reopen position. Settlement **snapshots the vacated position** on the case
  (see reinstatement below).

### Reinstatement (reverse a settled termination)
`Employee.Terminate()` nulls `PositionId`, so `FinalizeEmployeeTermination` passes the pre-termination
`oldPositionId` into `MarkSettled`, stored as `EmployeeTermination.VacatedPositionId` (nullable Guid,
**no FK** — a snapshot like the movement position columns). Migration `AddTerminationReinstatement`
adds `VacatedPositionId` + `ReinstatedAt`.
- **`GET EmployeeTermination/reinstatement-info?employeeId=`** (`GetReinstatementInfo`): reads the
  latest settled case's `VacatedPositionId`, returns `{ PreviousPositionId, PreviousPositionTitle,
  PreviousPositionAvailable (position exists && IsVacant), PreviousPositionOccupiedBy }`. Names are
  materialized then joined in memory (EF can't translate `string.Join` in a projection).
- **`POST EmployeeTermination/reinstate`** `{ EmployeeId, PositionId }` (`ReinstateEmployee`): employee
  must be terminated; the target position must exist and be **vacant** (else 400 "select a vacant
  position"); `Employee.Reinstate(positionId, branchId)` → Active, `IsTerminated=false`, branch follows
  the position (department derives from it), salary/pay-point preserved; `MarkPositionOccupiedAsync`;
  the latest settled case is stamped `MarkReinstated()`. The employee then **leaves the Termination
  List** and returns to the main Employee List automatically.
- **UI** (`terminationList/index.tsx` `ReinstateModal`): a **Reinstate** action fetches the info; when
  the previous position is available it's preselected ("will be restored unless you pick another"),
  otherwise a warning names the occupant and a **required** vacant-position picker forces a new choice.
  The picker is the searchable `DropDownField` (`param`/`setParam`) with `take:10` + `isVacant:true` —
  it shows 10 rows but the search box pushes `searchText` to the API, so it searches **all** vacant
  positions server-side (Position GetAll filters Code/PositionClass.Title), not just the loaded 10.

### Terminated-employee separation (Termination List)
`GetAllEmployees` excludes `IsTerminated`/`EmploymentStatus.Terminated` rows **unless** the caller
explicitly filters `status=Terminated`. Terminated employees live in the **Termination List** menu
(`/terminationList`, Personnel group): `GET EmployeeTermination/terminated` pages employees
(`IsTerminated` OR status Terminated, most recently updated first) each with the **latest case**
(settled preferred) via a correlated subquery. The row's History modal shows the complete record —
termination cases + full clearance detail, personnel movements, disciplinary record (existing
per-employee GETs) — and its Documents action opens the standard `GenerateDocumentModal`.
**Termination merge tokens** in `GenerateEmployeeDocument` (group "Termination"):
`{{TerminationType}} {{TerminationDate}} {{LastWorkingDate}} {{TerminationNoticeDate}}
{{TerminationReason}}` — from the employee's latest case (settled preferred), blank when none.

### Clearance document generation
Same template + merge + `GenerateDocumentModal` + react-to-print stack. New **Clearance** merge tokens
in `GenerateEmployeeDocument`, sourced from the latest **settled** termination's checklist:
`{{ClearanceTable}}` (system-built raw-HTML `<table>` of Department/Requirement/Status/Cleared-By/Date —
cell values HTML-encoded, emitted un-encoded like `{{Photo}}`/`{{Logo}}`), `{{ClearanceStatus}}`
("Fully Cleared" when all Cleared), `{{ClearanceDate}}` (SettledAt). New enum
`DocumentTemplateType.ClearanceCertificate` (stored as string — no migration). A **turnkey starter**
template is created by the idempotent `SeedDefaultDocumentTemplates` (`POST DocumentTemplate/seed-defaults`,
"Seed default templates" button on the Document Templates list) — a "Clearance Certificate" with a
letterhead, employee block, `{{ClearanceTable}}`, and signature footer. Generated/printed from the
Termination List's existing **Generate Document** action.

## 2. Multi-step approval flow (the exact sequence)

```
Module.Save() ──persist──► StartIfDefinedAsync(type,id,emp,summary)
   │                              │
   │ (no active definition)       │ (active definition)
   ▼                              ▼
 module acts directly        WorkflowInstance(Running, step 1)
 (or auto-approves)               │  approver at step N calls ApproveAsync/RejectAsync (auth-gated)
                                  ▼
                    not last step → advance CurrentStep
                    last step + Approve → Complete(Approved) + SAVE, then handler.OnApprovedAsync
                                          (on exception → Reopen() compensation + rethrow)
                    any step + Reject  → Complete(Rejected) + handler.OnRejectedAsync
```
Ordering matters: the instance is **completed and saved before** the handler runs, so the module's own
`WorkflowGate` check passes inside the handler. Approvals surface automatically in the generic
`/workflow` tracking UI + dashboard — **no per-module approval screen**.

## 2.9 Employee data visibility (who sees whom)

One rule serves every scoped module — appraisal, employee list/options, goals, development plans:

| Scope | Sees |
|---|---|
| `IsAdmin` | every employee in the tenant |
| `IsManager` | self + every employee whose position sits in their org-unit **subtree** |
| otherwise | self only |

Resolved once per request by `IPerformanceVisibilityService.GetScopeAsync()` and applied as a **SQL
predicate**, never an in-memory filter. `CanAccessEmployeeAsync(id)` answers the same question for
single-record endpoints.

- **Manager** = `Employee.IsManagerial` **and** a position with an org unit; the subtree comes from
  `EstablishmentShared.ResolveSubtreeAsync`. Without the flag a user is self-only regardless of title.
- **Admin** = head office **or** an `HrSignOff` approver on an active Appraisal workflow definition.
- **Per-record reads follow the record's owner**, not a blanket HR gate: performance history resolves
  its subject to an employee (`ResolveOwnerEmployeeAsync`) and reuses `CanAccessEmployeeAsync`, so the
  appraisee and their management line can read their own audit trail. **Aggregate/cohort reads stay
  HR-only** — medical expense reports, calibration sessions, trip budget utilisation and aging, and
  individual peer reviews (anonymity). An unmapped entity type fails CLOSED to HR-only.
- ⚠️ **Head office is the master switch** — `IsAdminAsync` opens with
  `if (currentUser.IsHeadOffice()) return true`, so anything that wrongly marks a session head office
  silently disables all of the above. It is set at login from the employee's branch, and (2026-08-10)
  a missing branch only counts when the account has **no linked employee** at all. Before that fix,
  every employee in a tenant with no branches was an admin and the whole table above was dead code.

## 2.10 Portal alerts: where a record opens

HRMS raises portal alerts through `IPortalNotifier` into the Home-owned `Core.Notification`. Two
things decide where a click lands, and they must agree:

- **HRMS writes `LinkUrl`.** `WorkflowService.NotificationLinkFor` → `/appraisal/{EntityId}` for an
  Appraisal (module-driven: the generic approve/reject is refused for it), `/workflow` otherwise.
  `InviteAppraisalPeers` writes `/myPeerReviews`.
- **Home resolves the same mapping** in `config/recordRouting.ts` for surfaces that hold a record
  rather than a notification — the Approvals Inbox card and the Pending Approvals count, which read
  the `APPROVAL_SOURCES` registry, not the notification feed. `openPortalTarget` is the one opener
  (absolute URL → new tab, else in-app), shared with the bell.

**Alerts are correlated per RECIPIENT-ACTIONABLE record**, not per parent: a peer-review alert carries
the `AppraisalPeerReview.Id`, so `ResolveAsync` on submit clears that peer's alert alone. Correlating
on the appraisal would clear every peer's.

**Anything a user must act on that is NOT a workflow instance needs an `APPROVAL_SOURCES` entry** —
otherwise it is invisible on every approvals surface, as peer reviews were.

## 2.11 Approver resolution — the performance contract

`IOrgManagerResolver` answers "who manages this employee" by climbing the org unit tree. It is called
**once per work item** by the approval inbox, so its cost is multiplied by every running instance in
the tenant. Two rules keep that affordable:

- **Load the org snapshot once per request, climb in memory.** The unit tree and the (small) set of
  managerial employees are fetched once; the walk itself does no I/O. Never reintroduce a
  per-level query — that cost 3,795 queries / 5 s on 2,000 instances.
- **Cache per UNIT, not per requester.** Self-exclusion ("a manager cannot approve their own request")
  is applied in memory *after* the lookup. Putting it in the SQL predicate makes the cache key
  requester-specific, so nobody sharing a unit can share a climb.

Callers resolving for many employees should call `PreloadEmployeeUnitsAsync` first — one query for the
whole batch instead of one per employee.

- **The inbox pre-filters candidates in SQL** (`GetMyApprovals`). Approver rows for all Running-step
  definitions are classified once — *static* (User/Role match), *subject* (`EmployeeId == mine`), or
  *dynamic* (manager types, matched against `EmployeesInMyManagedUnitsAsync`, the inverse subtree walk
  off the same org snapshot) — and the `WorkflowInstance` query keeps only instances whose current
  step could possibly route to the caller. The dynamic predicate is a deliberate **superset** (the
  whole managed subtree); `EvaluateAsync` still makes the final per-row decision, so results are
  identical to the full scan (diffed per role at 5,002 seeded instances: 346 ms → 31 ms). Never revert
  to `.Where(Status == Running)` alone — that materialises every running instance in the tenant.

**Tenant resolution is cached** (`DatabaseTenantStore`, 5-min `IMemoryCache`, primed under both the id
and the identifier). One predicate serves both key shapes — the cookie flow carries the GUID, the
host/header flow the identifier; querying `Identifier` first then falling back to `Id` guaranteed a
wasted miss on every cookie request. Finbuckle resolves the tenant on EVERY request, so an uncached
store taxes each of them; subscription changes take effect within the TTL.

**Cross-origin reads must stay "simple" CORS requests.** Both SPAs set `Content-Type: application/json`
only when a request has a body. The value is not CORS-safelisted, so sending it on GETs preflighted
every read — half of all dashboard requests were OPTIONS round-trips (18 of 36, measured via CDP).
Both APIs also send `SetPreflightMaxAge(24h)` so the writes that legitimately preflight cache it.

**Portal grids read cached registry feeds, never their own fetch.** The Approvals Inbox and My Pending
Requests grids share `useApprovalFeeds`/`useRequestFeeds` with the dashboard cards and page/search in
memory. Handing `useEntityList` a `fetchPage` that calls the registries re-runs the entire fan-out on
every page change and every keystroke, because its query key includes `param`.

**Grid rows warm their destination before the click.** Every route is `lazy`, so opening a record used
to download + parse the screen's chunk *inside* the click (6 scripts on the path). The dashboard grids
call `prefetchRoutes` (Home `config/routePrefetch.ts` → `template/prefetchLazy.ts`) on idle for exactly
the destinations their visible rows can open; the specifiers must match `routes/index.tsx` character
for character or the browser downloads a second copy instead of priming the one `lazy` resolves. The
identity probe (`Employee/me`) is likewise warmed from `AuthContext` the moment the session is
confirmed — five request feeds cannot build their URL without it — and cleared on login/logout so a
second sign-in on the same tab cannot inherit the previous user's identity.

## 3. Leave logic (Annual Leave — the flagship)

### 3.1 Fiscal-year anchoring
Leave is keyed to **`Core.FiscalYear`** (Ethiopian fiscal calendar), not calendar year. A request is
charged to the *open* fiscal year containing its **start date** (`IFiscalYearResolver.ResolveForDateAsync`);
a request may **not straddle two fiscal years** (submit one per year). Closed years reject new activity.

### 3.1.1 Annual leave has NO `LeaveType` (2026-08-10)
**Annual leave is not a configurable leave type and never looks one up.** Its entitlement comes
entirely from the per-fiscal-year `AnnualLeaveSetting`, so there was nothing for a `LeaveType` row to
contribute — yet the ledger still had to *find* one to stamp on balance rows. It resolved "the single
active `LeaveType` with `AccrualMethod = Annual`" and threw when there were zero or several, which
took the whole annual ledger down whenever that one config was missing.

- `LeaveBalance.LeaveTypeId` and `LeaveBalanceTransaction.LeaveTypeId` are **nullable**, and
  **`NULL` means annual leave**. `LeaveType` now covers only the OTHER leave kinds — what it was for.
- The convention is stated once in `App/Features/Core/Leaves/AnnualLeave.cs`
  (`AnnualLeave.LeaveTypeId` = null, `AnnualLeave.DisplayName` = "Annual Leave"); use it instead of
  bare nulls.
- `ILeaveAccrualService.ResolveAnnualLeaveTypeIdAsync` is **gone**. Annual rows are selected with
  `b.LeaveTypeId == null`.
- The unique index `(TenantId, EmployeeId, LeaveTypeId, FiscalYearId)` is deliberately **unfiltered**
  (`HasFilter(null)`). EF's default for a nullable column is `WHERE [LeaveTypeId] IS NOT NULL`, which
  would leave annual rows unconstrained and allow duplicate annual balances; SQL Server treats NULLs
  as equal in a unique index, so the unfiltered form enforces one annual row per employee per year.
- `ILeaveBalanceService` takes `Guid? leaveTypeId` throughout. Passing `null` is safe:
  EF Core's null semantics compile `== leaveTypeId` to `[LeaveTypeId] IS NULL` (verified in the
  generated SQL) rather than SQL equality, which would never match.
- `AllowHalfDay` **moved from `LeaveType` to `AnnualLeaveSetting`** — the one policy bit the type was
  still supplying. It follows `MaxConsecutiveDays` and `CarryForwardMaxDays`, which moved earlier. The
  migration backfills it from each tenant's old annual type, so no environment changed behaviour.

### 3.2 Entitlement calculation (`ILeaveAccrualService.CalculateEntitlement`)
Driven by `Employee.HireDate` + `Employee.IsManagerial` + the `AnnualLeaveSetting` for the FY:
```
serviceMonths = whole months from HireDate to FiscalYear.StartDate
if serviceMonths < 12:                      # under one year
    monthsInYear = months served within the FY (≤ 12)
    entitled = floor( (NewEmployeeLeaveDays or BaseLeaveDays) * monthsInYear/12 * 2 ) / 2   # ½-day precision
else:
    serviceYears = serviceMonths / 12
    base  = IsManagerial ? ManagerialLeaveDays : BaseLeaveDays
    extra = floor((serviceYears - 1) / IncrementIntervalYears) * IncrementDays
    entitled = min(base + extra, MaxLeaveDays)
```
Ethiopian Labour Proclamation defaults: base **16**, +**1 day / 2 service years**, managerial **20**,
cap **35**, probation **12 months**, carry-forward expiry **2 years**.

### 3.3 Generation & the Ledger
- `GenerateEntitlementsAsync(settingId)` — **idempotent**: creates a `LeaveBalance` per active employee
  for the setting's FY+type (skips those already generated) and posts an `Entitlement` ledger transaction.
- **Annual Leave Ledger UI** (`/annualLeaveLedger`) previews the calculated entitlement per employee and
  triggers generation via the **Calculate** button (`POST /api/v1/AnnualLeaveLedger/calculate`).
- Balance model = **ledger-backed summary**: `LeaveBalance` (Entitled, CarriedForward, Adjusted, Taken;
  `Available = Entitled + CarriedForward + Adjusted − Taken`) + append-only `LeaveBalanceTransaction`
  (signed `Delta`, `BalanceAfter`, `Type` ∈ Opening/Entitlement/CarryForward/Accrual/Deduction/Reversal/
  Adjustment/Expiry, `ReferenceId`). Managed only through `ILeaveBalanceService` (never mutate directly).

### 3.4 Request lifecycle (`SubmitLeaveRequest`)
```
validate → leave type active → resolve fiscal year (start date) → FY-boundary guard
 → gender eligibility (LeaveType.GenderEligibility vs Person.Gender)
 → working days = IWorkingCalendar.CountWorkingDaysAsync(start,end, halfDay = dayPart≠Full)   # excl. weekends+holidays
 → max-consecutive-days guard → overlap guard (pending/approved) → probation guard (MinExperienceMonths)
 → balance sufficiency (skipped when AccrualMethod = None) → persist LeaveRequest(Pending)
 → if LeaveType.RequiresApproval: StartIfDefinedAsync("LeaveRequest", …)
 → if NOT gate.HasRunningAsync(...):  auto-approve + Deduct   # no approval required OR no active definition
```
- **On workflow approval** (`LeaveRequestWorkflowHandler.OnApproved`): `request.Approve()` +
  `LeaveBalanceService.DeductAsync` (posts a `Deduction`). **On reject:** `request.Reject()`, no deduction.
- **Cancel** (`CancelLeaveRequest`): gated (can't cancel mid-approval — reject via workflow instead);
  if it was Approved → `ReverseAsync` (posts a `Reversal`, credits the balance back).

### 3.4.1 Return from leave (2026-08-09)

An approved request is not finished when the dates pass — the employee confirms they are back, and
what they confirm decides whether anything moves.

| Return | Header | Ledger | Approval |
|---|---|---|---|
| **On time** | → `Closed` | untouched — it already holds exactly these days | none |
| **Early** | → `ReturnPending` → `Closed` | credited the unused days **on approval** | required, comment required |
| **Late** | → `ReturnPending` → `Closed` | debited the extra days **on approval** | required, comment required |

**The ledger only ever moves on an approved decision.** Confirming an early return credits nothing;
`AnnualLeaveReturnWorkflowHandler` is the only place a return touches the balance, so the balance is
always the sum of decisions somebody actually made. A rejected adjustment therefore needs no reversal
— it simply returns the header to `Approved` so the employee can confirm again with a corrected date.

**A late return is an EXTENSION on the same request**, not a second request: one record, one history
thread. The extra days are balance-checked at confirmation, so an approver is never asked to
rubber-stamp an extension the entitlement cannot fund.

**Days actually taken are recomputed through `IWorkingCalendar`, never derived by arithmetic.**
Returning two days early over a weekend costs nothing; the same two days midweek costs two — only the
calendar knows which. ⚠️ A late return runs PAST every approved detail row, so the overrun has to be
counted separately (`plannedEnd+1 .. actualEnd`); the first implementation looped over the detail rows
only and silently reported the approved total for every late return. A half-day row the return lands
inside keeps its 0.5 — a half day is atomic, and the calendar would re-count it as a whole day.

`TotalLeaveDays` stays the APPROVED figure forever; `ActualLeaveDays` records what was taken. Keeping
both is what lets the grid show `5 → 3` and the history compare them.

**Adjustments need their own workflow definition** (`WorkflowEntityTypes.AnnualLeaveReturn` =
`"AnnualLeave.Return"`). Confirming an early/late return without one fails with a message naming the
process to configure, rather than stranding the request in `ReturnPending` with nothing able to
approve it — the same stance `SubmitAnnualLeave` takes. On-time returns need no workflow.
The recommended shape is to MIRROR that tenant's `AnnualLeave` chain (whoever approves the leave
approves a change to it); where there is no chain to mirror, pick approvers that actually resolve —
`ImmediateManager` is useless in a tenant whose employees have no manager set.

**History (`GET /AnnualLeave/{id}/history`)** merges three sources into one ordered timeline: the
request, the workflow action log for BOTH chains (the original approval and the adjustment), and every
return confirmation. Assembled server-side because an approver judging an adjustment needs all of it
at once, and stitching it client-side would mean four round trips and four chances to show a partial
story. Rendered as a popup in both apps (`annualLeave/historyModal.tsx`).

### 3.5 Year-end rollover (`RolloverAsync(fromFiscalYearId)`)
For each source balance with remaining days: **expire** days that were already carried in once
(`min(remaining, CarriedForward)` — the 2-year law) plus any excess over `LeaveType.CarryForwardMaxDays`
(→ `Expiry` txn); **carry** the rest into the next open FY (→ `CarryForward` txn on the destination
balance); then **close** the source fiscal year.

### 3.6 Working calendar (`IWorkingCalendar`, HC040)
`CountWorkingDaysAsync` / `IsWorkingDayAsync` / `GetNonWorkingDaysAsync` exclude Saturday/Sunday
(**hardcoded — becomes shift/policy-driven in Attendance Phase 3**) and active `Holiday` rows
(recurring holidays matched by month/day). Half-day = single working day → 0.5.

## 4. Employee employment terms + conditional form logic

The employment record (`Hrms.Employee`) carries terms that belong strictly to employment (not the
shared `Core.Person`): `EmploymentNature` (Permanent | Contract, string-stored enum), `ContractPeriod`
(int, months), `IsProbation` (bool), `ProbationEndDate` (date), and a denormalized `IsTerminated`
(bool, default false — set true by `Employee.Terminate()`, which the termination final-settlement
handler already calls; also clears `IsProbation`). Existing `Terminated`-status rows were backfilled
to `IsTerminated = 1` in the migration.

**Conditional rules (enforced in 3 places — form UX, zod, and FluentValidation):**
- `EmploymentNature === "Contract"` → the **Contract Period** field renders and is **required**
  (hidden for Permanent). The domain nulls `ContractPeriod` when nature is Permanent.
- `IsProbation === true` → the **Probation End Date** field renders and is **required** (hidden when
  false). The domain nulls `ProbationEndDate` when not on probation.
- Frontend: fields are conditionally spread into the `masterForm` component array; the probation
  Yes/No dropdown carries `"true"/"false"`, coerced to a real JSON boolean in `saveEmployee`
  (System.Text.Json will not read `"true"` into a `bool`). `IsTerminated` is never sent by the form.
- Backend: `CreateEmployeeDtoValidator` uses `.When(x => x.EmploymentNature == "Contract")` /
  `.When(x => x.IsProbation)`; the entity also guards the invariants in `ValidateEmploymentTerms`.

### Salary scale (pay point) — grade is now DERIVED from the scale
The employee links to a **salary scale** (`Core.SalaryScale`) via nullable `Employee.SalaryScaleId`
— the specific grade+step+amount pay point. **`Employee.JobGradeId` was dropped** (migration
`RemoveEmployeeJobGradeId`: DropForeignKey/DropIndex/DropColumn on `Hrms.Employee`): the grade is
redundant on the employee because it is reachable through `SalaryScale.JobGradeId`. The employee's grade
is therefore **derived**, never stored.
- **Form logic (`masterForm`):** the Job Grade dropdown is a **client-side filter only** (label
  "Job Grade (filter)") — it narrows the Salary Scale dropdown via `getAllSalaryScale({ jobGradeId })`
  (`jobGradeSelectHandler` also clears any prior scale) but is never persisted. Choosing a scale
  (`salaryScaleSelectHandler`) records `salaryScaleId` and **auto-fills the Salary field with the scale
  amount, which stays editable**. `saveEmployee` strips `jobGradeId`/`jobGradeName`/`salaryScaleStep`/
  `salaryScaleAmount` from the payload (filter/display-only; `CreateEmployeeDto` has no `JobGradeId`).
  On edit, the derived `jobGradeId` on the read DTO pre-seeds the filter so the scale list is pre-narrowed.
- **Backend:** `EmployeeShared.EnsureReferencesExistAsync` validates only that the **position** and
  **salary scale** exist (the old grade existence + scale-belongs-to-grade consistency checks are gone —
  there is no grade input to reconcile). `ResolveSalaryAsync` uses the client's salary if supplied, else
  defaults to the scale amount. The read projection **derives** `JobGradeId = e.SalaryScale.JobGradeId`
  and `JobGradeName = e.SalaryScale.JobGrade.Name`, alongside `SalaryScaleId`/`SalaryScaleStep`/
  `SalaryScaleAmount`. `DeleteJobGrade` no longer checks employees directly — the salary-scale guard
  transitively protects in-use grades.
- **Movements:** `EmployeeMovement` keeps its own `From/ToJobGradeId` history columns. The **From** grade
  snapshot is now sourced from the salary scale (`e.SalaryScale.JobGradeId`), and `ApplyMovement` no
  longer sets a grade on the employee. ⚠️ A grade change recorded on a movement is history only — to
  actually change an employee's (derived) grade, reassign the salary scale.

## 5. Dashboard analytics queries (optimized)

**Aggregate summary (2026-08-05).** The dashboard's counters come from ONE endpoint,
`GET /api/v1/Dashboard/summary` (`Inf/Common/DashboardSummaryService`), replacing 12 separate queries —
four of which were full paginated `GetAll?take=1` list calls issued only to read `.total`. It is a
single Dapper **`QueryMultipleAsync`** round trip of seven statements (branch / org-unit / position /
employee counts, workflow `Running|Approved|Rejected` via `GROUP BY`, probation count, retirement
count) reusing the ambient EF connection — the same pattern as `ReportExecutor`.

⚠️ Because it bypasses the repository, **tenant + branch isolation is re-implemented in C# and must
match `Repository.ApplyBranchFilter` exactly**: `Branch` filters by its own `Id` (it is not
`IBranchScoped`); `OrganizationUnit`/`Position`/`Employee` filter by `BranchId`; `WorkflowInstance` is
tenant-only. Head office bypasses the branch predicate entirely. Supporting indexes came from migration
`AddDashboardSummaryIndexes`: `(TenantId, Status)` on `hrmsWorkflowInstance` and
`(TenantId, BranchId, EmploymentStatus)` on `hrmsEmployee`. If you add a counter, add it to the same
batch rather than a new endpoint.

The frontend consumes it through one shared `useDashboardSummary()` query key, so the KPI strip and the
charts read the same cached payload without a second request.

Two employee widgets on the dashboard, each a dedicated endpoint on `EmployeeController` returning a
lean projection (tenant/branch-scoped via `IRepository.GetAll()`):
- **Employees on Probation** — `GET /api/v1/Employee/on-probation`:
  `Where(e => e.EmploymentStatus == Active && e.IsProbation)`, backed by the composite index
  `(EmploymentStatus, IsProbation)`. `DaysRemaining` to `ProbationEndDate` computed in memory.
- **Upcoming Retirements** — `GET /api/v1/Employee/upcoming-retirements`: there is **no** stored
  retirement date; it is derived as `DateOfBirth + 60y` (statutory age). "Retires within a month"
  ⟺ `RetirementDate < today + 1mo` ⟺ **`DateOfBirth < (today + 1mo − 60y)`**. The threshold is a
  C# constant, so the filter is **SARGABLE** (plain range scan on the `DateOfBirth` index, no per-row
  `DATEADD`). `RetirementDate`/`DaysRemaining` computed in memory after materializing the small set.
  Includes already-due (negative `DaysRemaining`) rows.

## 6. Workforce Planning (HC053–HC076)

Two tables: `Hrms.WorkforcePlan` (aggregate) 1─< `Hrms.WorkforcePlanLine`. Slices in
`Features/Core/WorkforcePlans/`; controller `WorkforcePlanController`; UI `/workforcePlan` (plan
designer: header + editable lines grid + live cost tiles) and `/establishmentOverview` under the
new **Planning** sidebar group.

- **Plan** = Name, Horizon (Annual/MediumTerm/MultiYear, HC053), Scenario
  (Baseline/Growth/Contraction/Restructuring, HC067), Status
  (Draft→Submitted→Approved/Rejected→Archived), scope `OrganizationUnitId?` (null =
  organization-wide; else the unit **subtree** via an in-memory BFS — HC054),
  `StartFiscalYearId` + `PeriodCount` (1–10 consecutive fiscal-year periods — HC069), budget
  envelope (`TotalBudget`, `BudgetThresholdPercent`, `EscalationJustification`), denormalized
  `ProjectedCost`, and **version chain** `Version` + `RootPlanId` (null on v1; chain groups by
  `RootPlanId ?? Id` — HC071).
- **Line** = unit × position class × planned employment type (Permanent/Contract/Intern/Consultant —
  a planning-level enum, the Employee entity untouched, HC057) × period, carrying: establishment
  snapshot (Authorized/Filled/Vacant), demand (NewHires/Replacements/TemporaryStaff, HC058), supply
  (MobilityIn/Promotions/ActingAssignments, HC059), separations
  (Retirements/Resignations/ContractExpiries, HC060), `IsCriticalRole` + free-text
  `RequiredCompetencies` (HC061–062; structured competency model deferred to L&D, HC063), and
  per-head annual costs (salary defaulted from the role's **salary scale × 12** when 0, HC064).
  Computed (not stored): `EndHeadcount = max(0, Filled − separations + demand + supply)`,
  `HeadcountGap = max(0, End − Authorized)`, `LineCost = End × (salary+allowances+benefits)`.
  Unique index (Plan, Unit, Class, Type, Period).
- **Establishment anchoring (HC055/HC056):** a `Position` row = one authorized seat; `IsVacant`
  splits filled/vacant. `GET WorkforcePlan/establishment` groups seats per unit × role (+ grade +
  job family) with a vacancy-aging approximation (days since the vacant seat's UpdatedAt);
  `POST {id}/populate` rebuilds the plan grid from it. `GET {id}/suggest-separations` pre-fills
  retirements per unit × role from the DOB+60y sargable forecast within the horizon.
- **Budget control (HC065/HC066):** variance = budget − projected cost; submission (handler
  pre-check → 400, domain invariant as backstop) **requires an escalation justification when
  projected cost > budget × (1 + threshold%)**.
- **Approval (HC070/HC072):** `Submit` routes through the generic engine (entity type
  `WorkforcePlan`; seeded chain **Directorate Review → HR Review → Finance Review → Executive
  Approval**); no active definition → direct approval. The process is selectable in the Workflow
  Definitions designer (`workflowEntityTypeOptions` in `constants/orgStructure.ts` — ⚠️ every new
  workflow-backed module must add its entity-type key there, or the chain is not configurable
  from the UI). `WorkforcePlanWorkflowHandler.OnApproved`
  approves the plan **and auto-archives older Approved versions of the same chain** (one approved
  plan per chain); OnRejected → Rejected (still editable, resubmittable). Approved/Submitted/
  Archived plans are immutable — `POST {id}/new-version` clones into a Draft vN+1 (only one open
  draft/submitted version per chain).
- **Analytics & feeds:** `GET {id}/summary` (per-period headcount/demand/supply/separations/cost +
  budget position), `GET compare?ids=` (2–5 plans side-by-side — HC068), `GET approved-demand`
  (outstanding NewHires/Replacements/Temporary of Approved plans — the recruitment-requisition feed,
  HC075; also surfaced in the UI "Hiring Demand" modal). The plan designer shows a live
  **Period Projections** table (per-year end headcount, hiring demand, internal mobility, attrition,
  cost trend — HC069/HC073) and a per-line **Gap** column (HC062). Export: plans list rides the
  standard list-export; the Establishment Overview has its own Excel export via `exportListToExcel`
  (HC074). Deferred integration surfaces (HC076): competencies text + critical-role flags for
  L&D/succession; approved-demand for module 3.5 recruitment.

## 7. Recruitment & Talent Acquisition — Phase 1 (HC077–HC100 core)

Six tables (slices in `Features/Core/Recruitment/`, controllers in `RecruitmentControllers.cs`,
Recruitment sidebar group → `/hiringRequest` `/jobRequisition` `/candidate` `/jobApplication`).
Sequential document numbers (HRQ-/REQ-/CND-####, tenant-scoped, unique-indexed).

- **`Hrms.HiringRequest`** (HC077–HC083): directorate + role + headcount + planning-level employment
  type + justification/requirements/timeline + `EstimatedBudget` + optional `WorkforcePlanId` link
  snapshot (no FK, HC081). Status Draft→Submitted→Approved/Rejected→Closed. **Submit gate (HC082):
  requested positions ≤ currently vacant seats** for the unit × role (a Position row = one seat);
  then workflow `HiringRequest` (seeded Directorate Head → HR → Finance, HC078); no definition →
  direct approval. `GET HiringRequest/budget-monitor` = per-unit approved/submitted totals (HC083).
- **`Hrms.JobRequisition`** (HC084–HC088, HC091, HC095) 1─< `Hrms.RequisitionScreeningCriterion`:
  **creatable only from an APPROVED hiring request (HC080)**; role details (Title/Description/
  Qualifications/Experience/Skills/SalaryScale) default from the request's PositionClass, editable;
  Σ requisitioned positions per request ≤ the request's approved count. Status Draft→PendingApproval
  →Approved→Posted→Closed (+Rejected editable, +Cancelled). Workflow `JobRequisition` (seeded HR →
  Approving Authority, HC085). Posting: channel Internal/External/Both (HC088), `GET
  {id}/generate-posting` builds the standard advertisement from the details (HC091, stored text
  editable), `PUT posting` + `POST {id}/post` (requires text) / close / cancel.
- **`Hrms.Candidate`** (HC089–HC090, HC092–HC097): centralized applicant master; Source
  External/Internal/JobBoard/SocialMedia/Referral/WalkIn (HC092); internal candidates link an
  employee (FK SET NULL); structured Education/Experience/Skills summaries + YearsOfExperience
  (resume *parsing* is the HC094 integration hook on top of these fields); resume file upload
  (PDF/DOC/DOCX ≤5MB, photo-storage pattern, `Storage:CandidateResumePath` ??
  App_Data/candidate-resumes, gitignored); **consent mandatory at create** (`ConsentGiven` +
  `ConsentAt`, HC097); `POST {id}/anonymize` scrubs all PII + deletes the resume file irreversibly,
  keeps anonymous history; talent-pool flag + notes (HC089). `GET Candidate/match?requisitionId=` —
  ranked matching (HC090): 60% skills-token overlap + 25 experience-met + 10 talent-pool + 5
  internal; list filter via ?status= Archived|TalentPool|{Source}.
- **`Hrms.JobApplication`** (HC098–HC099) 1─< `Hrms.JobApplicationStageLog`: unique candidate ×
  requisition; applications accepted only on Approved/Posted requisitions; stage machine Received→
  Screening→Shortlisted→Interview→Selected (+OfferPending/Hired reserved for the offer stage;
  Rejected/Withdrawn/Hired terminal-immutable); **the interview stage is not forced — transitions
  may bypass it (HC102)**; screening score/remarks recorded with moves (HC095/HC099); every
  transition appends an ActedBy/ActedAt log row (tenant-stamped + explicitly Added — the
  aggregate-child gotcha). `?parentId=` scopes the list to one requisition's pipeline.
- **Evaluator scoring & ranking** (migration `AddRecruitmentCandidateLifecycle`): each screening
  criterion can be assigned an evaluator — **internal Employee (FK SET NULL, name snapshotted
  server-side), ExternalPerson, or Organization** (`CriterionEvaluatorType` + name). Evaluators score
  applicants per criterion 0–100 (`Hrms.ApplicationCriterionScore`, unique per application×criterion,
  weight snapshot); the application's total **auto-recomputes as Σ(score×weight)/Σ(weight)**.
  `PUT JobApplication/scores` (upsert sheet), `GET JobApplication/ranking?requisitionId=` — ordered
  ranking with per-criterion breakdown + a FailsMandatory flag (mandatory criterion < 50). UI: score
  sheet modal (live total preview) + Ranking modal on the requisition.
- **Core.Person integration & hire conversion:** `Candidate.PersonId` — a Core.Person row is created
  (or, for Internal candidates, **reused from the employee**) at candidate save (grandfather name +
  gender therefore required); saving keeps it in sync; legacy candidates backfill on next save.
  **`POST Candidate/{id}/hire`** converts to an employee **on the SAME person — zero re-entry**:
  requires an application at Selected + the compliance set (below); creates the Employee (optional
  vacant position [occupancy synced], salary from scale or explicit, Permanent/Contract w/ period,
  **probation tracking** via IsProbation + end date → status Probation); moves the application →
  Hired (logged); `Candidate.MarkHired` archives + links `HiredEmployeeId` (no FK — SQL Server
  multiple-cascade-path limit; InternalEmployeeId holds the SET NULL slot). Hired candidates can't
  be anonymized (their identity lives on as the employee). **UI (Source/Type cleanup):** the confusing
  `Source` dropdown (which mixed the internal-vs-external *type* with acquisition *channels*) is replaced
  by an **Applicant Type** segmented control — **Internal** shows an Employee picker that prefills + **locks**
  identity from `GET Employee/{id}` (source=`Internal`), **External** shows a **Source Channel** dropdown
  (External/JobBoard/SocialMedia/Referral/WalkIn) with editable identity. The stored `CandidateSource` enum
  is unchanged; the UI derives type = (source===`Internal`).
- **Structured background (education & experience) — the person IS the hand-off:** the candidate's
  education/work history is captured in the **same `Hrms.EmployeeEducation` / `Hrms.EmployeeExperience`
  tables the employee profile uses** — those rows are keyed on **`PersonId`, not `EmployeeId`**. New
  candidate-scoped handlers (`CandidateBackgroundHandlers.cs`) resolve `personId` from `Candidate.PersonId`
  and read/write those aggregates via their existing domain `Create`/`Update`. Because hire creates the
  Employee on that **same** PersonId, the rows are already the employee's — **zero migration, zero copy**
  (verified E2E: candidate.PersonId == education.PersonId == experience.PersonId). Endpoints:
  `GET/POST Candidate/{id}/education` + `…/experience`, `DELETE Candidate/education/{id}` + `…/experience/{id}`.
  **Internal candidates are read-only** here (their person's records belong to the employee master — the
  guard 400s on create/update/delete, GET still works). The free-text `EducationSummary`/`ExperienceSummary`
  columns remain (dropping = destructive) but are **removed from the form**; `SkillsSummary` stays (drives
  matching). No schema migration in this increment. **Row attachments:** education/experience rows take
  file attachments in the SAME `Hrms.EmployeeDocument` table the employee profile reads (OwnerType +
  OwnerId = row id) via `Candidate/{id}/background-documents` (+ download/delete by document id) — so
  they are on the employee's profile at hire automatically. `EmployeeDocument.EmployeeId` (no FK) anchors
  to the CANDIDATE id until hire; `HireCandidate` re-anchors those rows to the new employee via
  `EmployeeDocument.AssignEmployee()`. Deleting a row cascades its attachments. The candidate form is a
  **tabbed profile** like the employee's (Applicant Details | Education | Experience, tab bar above the
  persistent header) with an applicant-type **switch** (unchecked = External, checked = Internal).
- **Candidate documents & automated migration:** `Hrms.CandidateDocument` (typed, binary inline like
  EmployeeDocument, ≤5MB) — upload/list/download/delete under `Candidate/{id}/documents`. **At hire,
  every document (plus the disk-stored resume) migrates automatically** to `Hrms.EmployeeDocument`
  with the new owner `EmployeeDocumentOwner.Recruitment` (OwnerId = the employee id; string-stored
  enum → no migration on that table) — retrievable via the existing
  `GET EmployeeDocument?ownerType=Recruitment&ownerId={employeeId}`.
- **Mandatory documentation (compliance gate):** required set = **National ID + Guarantor Form +
  Medical Certificate + (Signed Offer Letter OR Employment Contract)**
  (`CandidateShared.MissingComplianceDocuments`). Candidate DTO exposes
  `ComplianceComplete`/`MissingComplianceDocuments`; **hire 400s listing what's missing**; the
  candidate form shows the checklist + compliance badge and disables Hire until complete.
- **Talent Pool** (`/talentPool`, Recruitment group): searchable past-applicant interface — name/
  skills search, All/TalentPool/Archived filters, per-candidate **application history**
  (`GET JobApplication?categoryId={candidateId}`), hired badge, and one-click **Apply to Vacancy**
  onto any open requisition.
- **Interviews & panels (Phase 2, HC101–HC109)** — migration `AddRecruitmentInterviewsOffers`,
  entity shapes adopted from the §7.1 review: `Hrms.Interview` (round ordinal — multiple rounds are
  first-class, NO unique stage-gate; window CHECK end>start; Scheduled→Completed/Cancelled/NoShow)
  1─< `Hrms.InterviewPanelist` (EmployeeId? SET NULL + name snapshot, or free-text external
  panelist; lead flag; attendance Pending→Confirmed/Attended/Missed) 1─< `Hrms.InterviewFeedback`
  (0–100 CHECK, per-criterion loose FK + name snapshot like ApplicationCriterionScore; null
  criterion = overall entry). **Interviews are the Interview LEVEL's activity**: scheduling (and
  rescheduling) requires the application to sit AT the Interview stage — moving it there is a
  deliberate pipeline decision, never a side effect (the earlier auto-advance was removed,
  2026-07-11). Records stay viewable from any stage; completing/cancelling an old round remains
  possible after the application moves on. **The panel PRE-FILLS from the vacancy's criteria
  evaluators** (Interview-level + global; employee evaluators → employee panelists, external
  persons/organizations → named panelists, deduplicated, first one lead) — interviewers are
  defined ONCE on the criteria and inherited, adjustable but never re-typed. Feedback submission
  auto-marks the panelist Attended. `GET Interview/consolidated?applicationId=` = HC109 report
  (per-criterion averages across rounds, per-panelist totals, overall + weighted averages;
  cancelled rounds excluded). Scheduled rounds can be rescheduled/re-panelled/deleted at the
  Interview level; held ones are record (cancel only).
- **Offers (Phase 2, HC111–HC114)** — `Hrms.JobOffer`: tenant-scoped `OFR-####` numbering from the
  NEW race-safe `Hrms.NumberSequence` counter (§7.1 adoption #5: atomic UPDATE…OUTPUT via
  `INumberSequenceService`, replaces count+1 for new numbers); lifecycle Draft → Submit →
  PendingApproval (generic workflow `JobOffer`, seeded HR → Approving Authority; auto-approves when
  no definition) → Approved → Sent → Accepted | Declined | Expired, Withdrawn from any pre-final
  state; rejection returns to Draft for resubmission. **One ACTIVE offer per application**
  (filtered unique index on Draft/PendingApproval/Approved/Sent + handler check). **HC113 salary
  validation:** offer carries `SalaryScaleId?`; a salary deviating from the scale amount requires a
  written `SalaryJustification` (400 otherwise). **The offer drives the pipeline:** Send moves the
  application → OfferPending (logged); Decline/Withdraw/lazy **Expiry** (sent offer past its date
  lapses on read) release it back to Selected. **Hire gate:** once any offer exists for the
  application, `HireCandidate` requires the newest one ACCEPTED (hire also accepts stage
  OfferPending, stamps `offer.HiredEmployeeId` — no FK, cascade-path limit). `GET
  JobOffer/{id}/generate-letter` builds the standard letter text server-side (HC111, editable
  Draft-only, frozen at send). UI: Interviews + Offers modals on the Applications pipeline.
- **Pipeline lifecycle rules (end-to-end review, 2026-07-10)** — nothing in the pipeline is ever
  STRANDED (`PipelineDisposition.CloseOutAsync`: moves active applications to a final stage with a
  logged note and withdraws their live Draft/Approved/Sent offers; PendingApproval offers stay with
  their running workflow):
  1. **Vacancy fill auto-close:** when a hire fills the LAST open position, the requisition
     auto-closes and the remaining active applicants are Rejected ("Position filled — vacancy …
     closed"). No vacancy stays open with a pipeline nobody can hire from.
  2. **Close/Cancel cleans up:** closing or cancelling a requisition dispositions its open
     applications (Rejected, reason logged) and withdraws live offers — a candidate can never
     accept an offer for a vacancy that no longer exists.
  3. **Hire withdraws siblings:** the new employee's active applications on OTHER vacancies are
     Withdrawn ("Hired on vacancy …").
  4. **Anonymize withdraws first:** the erasure right ends participation — active applications are
     Withdrawn and live offers pulled BEFORE the PII scrub (no anonymous ghost mid-pipeline).
- **One source of truth for the screening score:** on a vacancy WITH weighted criteria, the
  criterion engine owns `ScreeningScore` — manual scores on stage moves are rejected (400), and
  the Move Stage form **doesn't offer the field at all** on such vacancies (it shows the current
  auto-calculated total and keeps only the remarks input; criteria-less vacancies keep the manual
  field). UI and API tell the same story.
- **Offer-driven stage lock:** once ANY offer is in play (Draft / PendingApproval / Approved /
  Sent / Accepted), manual stage moves are blocked (400 naming the offer) — the offer drives the
  pipeline; declined/expired/withdrawn offers release the application automatically. The UI
  disables Move Stage at OfferPending with an explanatory tooltip.
- **Bulk stage moves (mass processing):** `PUT JobApplication/stage/bulk` moves many applications
  in one action with **per-item outcomes** — each application is checked against the SAME rules as
  a single move (final stages, offer-driven lock, already-there); the movable subset commits as
  one transaction, the rest are reported back with the reason and candidate name, never failing
  the batch. Bulk moves carry a shared note (logged per application) but no screening scores.
  UI: checkbox selection on the pipeline (final/offer-driven rows unselectable) → "Move N
  Selected" toolbar action → stage+note modal → moved/skipped result report.
- **Action-button sequence (Applications row, process order):** Score → Interviews → Move Stage →
  Offers → History. Interviews are ALWAYS viewable (the record outlives the decision; the modal is
  read-only for final applications); Offers are viewable from Selected onward and on final
  applications (creation gated to Selected/OfferPending in the modal AND the backend); Score hides
  entirely at final stages; History is always available.
- **Criteria authoring flow:** Apply in the criteria popup STAGES the set locally — persisting
  happens with Save Requisition; the summary card shows a "Not saved yet" badge until then (the
  Apply≠Save trap is surfaced, never silent). Button reads Define / Edit / View Criteria by
  context; the empty state offers a one-click standard template (Written Exam 50 mandatory /
  Interview 30 @Interview / Document Review 20 @Screening).
- **Offers are rank-gated like hires:** on a scored vacancy, an offer can only be created for an
  ELIGIBLE candidate (never waitlisted / unscored / mandatory-failing / offer-rejected) — the
  system never issues an offer the hire gate would refuse. **The UI mirrors the gate:** the
  applications list carries per-row `HireEligibility` + `Rank` (computed from the vacancy ranking
  for criteria-scored vacancies; null otherwise), an eligibility chip renders under the stage
  chip, and the row's Offer button is ACTIVE only for Eligible applicants (disabled with the
  specific reason — "Waitlisted at rank #N…", "Not scored…" — for everyone else; finished
  applications keep view-only access). Three applicants on a 1-position vacancy = exactly one
  active Offer button.
- **Offer defaults derive from the vacancy** (`GET JobOffer/defaults?applicationId=`): the
  position dictates the pay point (requisition `SalaryScaleId`, falling back to the position
  class's scale — label + amount returned), and the **hiring manager resolves from the unit's
  management hierarchy**: the active `IsManagerial` employee whose position sits in the vacancy's
  unit; when the unit has none, the PARENT unit answers, walking `ParentId` upwards (≤10 levels;
  `ManagerResolvedFromUnit` names the answering unit). The offer form opens pre-populated (scale
  locked to the position's pay point when one exists, salary + manager pre-filled with the
  resolution source shown); `SaveJobOffer` applies the SAME defaults server-side when a create
  omits scale/manager, so raw API calls behave identically. HC113 deviation-justification applies
  against the defaulted scale.
- **Approved offers auto-deliver as PDF e-mail:** the moment the FINAL approver approves
  (workflow `OnApprovedAsync`, or the direct auto-approve when no chain is defined), the letter
  (HR draft, or the standard HC111 letter generated and attached to the record) renders as a PDF
  (`IPdfService`/QuestPDF, A4 letter layout) and e-mails to the candidate as an attachment
  (`IOfferDelivery`; `EmailAttachment` support added to `IEmailService`). On success the offer
  marks **Sent** and the application moves to OfferPending ("approved and e-mailed") — no manual
  step. On failure (no candidate e-mail, mail outage) the offer STAYS Approved and the manual
  "Send to Candidate" button is the retry (it too e-mails the PDF; the response says whether the
  mail went out). Delivery never throws — approval always stands.
- **Offer acceptance advances the pipeline (OfferAccepted stage):** an accepted offer moves the
  application OfferPending → **OfferAccepted** (a non-terminal stage, `ApplicationStage=9`; string
  column, no migration) with a logged transition, so the list no longer reads "Offer Pending"
  after acceptance. OfferAccepted is offer-driven like OfferPending: manual/bulk moves are blocked
  (400), the Move-Stage button is disabled, and the offer record stays view-only (`New Offer` is
  replaced by an "accepted — ready to hire" note; `SaveJobOffer` also refuses a new offer at this
  stage). The hire flow and hire-queue treat OfferAccepted as hire-ready (alongside
  Selected/OfferPending); the hire conversion moves it → Hired.
- **The offer button reflects acceptance too:** an Accepted offer is a settled positive outcome,
  so no `New Offer` is offered afterwards — this fixes the bug where `New Offer` reappeared once
  Accepted left the ACTIVE set. Terminal + OfferAccepted applications keep view access to the
  offer record.
- **Customizable offer-letter PDF template (HC111):** HR configures the offer letter under
  *Recruitment → Offer Letter Template* (`OfferLetterTemplateController`). Two parts:
  (1) **company letterhead** on `CompanyProfile` (shared with document templates) — company name,
  contact address/phone/e-mail, and the logo (reuses the `DocumentTemplate/logo` upload); and
  (2) a **tokenized letter body** + signatory (`OfferLetterTemplate`, one row per tenant, default
  provided). `IOfferLetterComposer` merges `{{CandidateName}}`, `{{Position}}`, `{{Salary}}`,
  `{{StartDate}}`, `{{ExpiryDate}}`, `{{OfferNumber}}`, `{{EmploymentType}}`, `{{UnitName}}`,
  `{{CompanyName}}`, `{{Today}}` from the offer/candidate/requisition/company, and QuestPDF renders
  the letterhead (logo + identity) + merged body + signatory. One source of truth: the
  "Generate letter" button, the stored `LetterText`, and the e-mailed PDF all flow through the
  composer. The editor has a live **Preview PDF** (`POST .../preview`, sample data over the real
  letterhead). An HR-edited `LetterText` is used verbatim as the PDF body.
- **SMTP sender must be the authenticated mailbox (Gmail/365):** authenticated relays reject a
  `From` that is not the login account or a verified alias, so a branded `FromAddress` like
  `no-reply@…local` silently fails. `SmtpEmailService` now sends **as the login** when `UserName`
  is an e-mail address that differs from `FromAddress`, and keeps the branded address as
  **Reply-To**. Non-address logins (e.g. SendGrid's `apikey`) leave the configured From untouched.
- **User ↔ Employee relationship (FK owned by User):** the `User` table carries a nullable
  **`EmployeeId`** foreign key to `Employee` (SET NULL on employee deletion) — one login account
  belongs to at most one employee; set on the **user** form ("Linked Employee"). The old
  `Employee.UserId` direction was removed. `User.BranchId` and `User.IsHeadOffice` columns were
  also removed: **branch scope + head-office visibility are DERIVED at login** from the linked
  employee's branch. `LoginRepository` computes this and still writes the `BranchId`/`IsHeadOffice`
  cookies the rest of the app reads, so branch isolation is unchanged downstream.
  **CORRECTED 2026-08-05 — the rule is the BRANCH FLAG, not the absence of a branch:**
  ```
  isHeadOffice = employee has no branch  OR  employee.Branch.IsHeadOffice
  ```
  The old rule was `branchId is null` alone, which only recognised users with **no** branch (tenant
  owner / unlinked account). Because the Head Office is itself a real `Branch` row, everyone assigned
  to it got `branchId != null` → `isHeadOffice = false`, and was silently scoped to their own org-unit
  subtree. That single flag drives BOTH visibility gates, which is why the symptom looked like two
  unrelated bugs:
  - `Repository.ApplyBranchFilter` — head office bypasses branch isolation entirely; otherwise
    `Branch` filters by its own `Id` and `IBranchScoped` entities by `BranchId`.
  - `PerformanceVisibilityService.IsAdminAsync` — head office ⇒ unrestricted; otherwise a managerial
    employee sees their unit subtree and everyone else sees only themselves.
  ⚠️ The value lives in a **cookie minted at sign-in**, so a change only takes effect on the user's
  **next login**. Two coupled hardenings shipped with it: the login-time lookup reads **without** the
  repository's tenant/branch filters (they read the *previous* session's cookies, and a stale
  `BranchId` made the lookup return no row → collapsed to "no branch" → wrongly granted head office to
  the next user), re-asserting `TenantId` by hand; and logout now clears `BranchId`/`IsHeadOffice`,
  which it previously left behind.
- **Evaluator permissions (an assigned evaluator only handles their own applicants) — enforced at
  three layers:** the current user is resolved to their employee via **`User.EmployeeId`**; an
  employee assigned as a criterion evaluator ANYWHERE is a "constrained evaluator"
  (`EvaluationGuard.GetContextAsync` → employeeId + assigned criterion ids + assigned requisition
  ids). **PREREQUISITE:** the evaluator's login account must be linked to their employee (User form
  → "Linked Employee"); an unlinked account is treated as HR (unconstrained). The three layers:
  1. **Visibility (read):** `GetAllJobApplications` filters the pipeline to the constrained
     evaluator's assigned requisitions — they SEE only their own applicants. HR / unlinked see all.
  2. **Scoring (write):** `EvaluationGuard.EnsureMayScoreAsync` rejects (400) any criterion the
     evaluator is not personally assigned to — on direct scoring AND interview-score adoption
     (`AdoptInterviewScores`, previously a bypass).
  3. **UI:** `GET JobApplication/evaluator-context` returns `{ isConstrainedEvaluator,
     assignedCriterionIds, assignedRequisitionIds }`; the applications page shows an "Evaluator
     view" chip and the score sheet lists ONLY the evaluator's assigned criteria (never inviting a
     submission the backend would refuse).
  The "assigned anywhere" test means an evaluator from vacancy A cannot see or score vacancy B's
  applicants (no assignment there).
- **Score locking (evaluation concluded = frozen):** criterion scores can be entered or corrected
  only while the applicant is still being evaluated — stages Received / Screening / Shortlisted /
  Interview. Once HR moves them to **Selected** (or any later/terminal stage), the evaluation is
  complete and the score sheet is locked (400 on any score/adopt). `EvaluationGuard.EnsureEvaluatable`
  guards both direct scoring and interview-score adoption; the pipeline's Score button hides at
  Selected+ to match.
- **Hire auto-populates Position & Salary (no manual re-entry):** the hire conversion derives the
  pay point and placement instead of asking HR to re-select them — salary scale = DTO ?? the
  candidate's **offer** ?? the requisition scale; salary = DTO ?? **offer amount** ?? scale amount;
  position = DTO ?? a still-**vacant** position of the requisition's PositionClass (preferring its
  own unit). An explicit value on the request always wins (override). The Hire modal prefills the
  salary from the accepted offer and labels the position picker "Auto — from the vacancy's role".
- **Interview results adopt into the ranking (no double entry):**
  `POST JobApplication/{id}/adopt-interview-scores` copies the consolidated per-criterion interview
  averages into the application's criterion scores (weights inherited; overall impressions stay
  commentary) and recomputes the weighted total — the "Adopt into Ranking" button on the
  consolidated report.
- **Domain rule violations are 409, never 500:** `ExceptionMiddleware` maps
  `InvalidOperationException` (every domain state-machine guard) to **409 Conflict** carrying the
  domain message; it is also no longer classified transient/retryable. Handler-level
  `ValidationException` pre-checks stay 400.
- **All recruitment numbering is race-safe:** HRQ/REQ/CND joined OFR on the per-tenant atomic
  counter (`Hrms.NumberSequence`); existing tenants' counters were seeded from their current max
  (data migration `SeedRecruitmentNumberSequences`).
- **Notifications** (HC079/HC087/HC099/HC100): in-app via status chips + the Dashboard approvals
  inbox. **E-mail infrastructure now exists**: `IEmailService` (App) / `SmtpEmailService` (Inf),
  driven by the `Email` config section — `Enabled` master switch (false = logged no-op),
  Host/Port/EnableSsl/UserName/Password relay settings, FromAddress/FromName, and
  `PickupDirectory` (writes .eml files instead of network delivery — dev/test without a mail
  server). Sends NEVER throw and always run AFTER the business transaction commits — a mail
  outage or a candidate without an e-mail address is logged and skipped, the operation stands.
- **E-mail is delivered in the BACKGROUND (Hangfire, 2026-07-12):** requests never block on SMTP
  (previously up to the 15 s timeout). `IEmailService` resolves to `QueuedEmailService`, which
  runs the cheap guards in-request (no recipient / mailer disabled → `false`, nothing enqueued —
  callers keep their semantics) and otherwise enqueues an `EmailDispatchJob` with the fully
  materialized payload (to/subject/body/attachments) and returns `true` = "durably queued".
  **Compose in-request, send in background:** all tenant-scoped work (candidate lookup, letter
  merge, QuestPDF) still happens inside the request — background jobs have NO Finbuckle tenant
  context, and the payload design keeps them tenant-free. The job throws on a failed send so
  Hangfire retries (1 m/5 m/15 m/1 h/2 h, then parked as Failed on the dashboard) — delivery is now
  MORE reliable than the old one-shot attempt; a transient relay outage delays mail instead of
  losing it. Consequence for offers: `true` from `EmailOfferAsync` = queued, so an approved offer
  marks **Sent on successful enqueue** (retries make delivery durable); no-address/disabled still
  leaves it Approved for manual handling. Storage: Hangfire SQL Server tables auto-created in CERP
  under the `HangFire` schema (no EF migration); tuned options (`SlidingInvisibilityTimeout` +
  `QueuePollInterval=Zero`, `UseRecommendedIsolationLevel`, `DisableGlobalLocks`) + a small capped
  worker pool (2–4) so background processing never contends with the request path's connection
  pool. Ops dashboard at **`/hangfire`** — cookie-authenticated users only (the filter
  authenticates the `Cookies` scheme explicitly because the app's default scheme is JWT).
  **Automatic applicant e-mails (interview lifecycle):** invitation on schedule, "rescheduled"
  with old→new times when the TIME changes on a reschedule (panel-only edits are internal — no
  mail), and cancellation notice on cancel. Composed by `IInterviewNotifier` from the
  application → candidate (Email, name) + requisition title. Other e-mail hooks (HC100
  acknowledgement, offer letters) can now plug into the same service.
  The posting window (`OpenUntil`) deliberately does NOT block manual
  application entry — HR may register late/walk-in applicants; requisition status is the gate.
  **Deferred to Phase 3:** background verification (HC110), public career portal (HC093),
  onboarding checklist (HC115–116 beyond the hire conversion now in place), job-board feeds
  (HC092), resume parsing (HC094).

### 7.0 Weighted screening criteria, ranking & the Hire Employee flow (2026-07-10)

- **Criteria are percentages (migration `AddCriterionStageScope`):** each requisition criterion's
  `Weight` is a % of the final ranking score; a non-empty set must total **exactly 100%** — enforced
  in the domain (`SetScreeningCriteria`), the validator, and the popup criteria grid (live Σ badge,
  Apply disabled otherwise). Criteria may be **global or scoped to one recruitment level**
  (`AppliesAtStage?`: null = all steps; Screening / Interview / Selected). **Weights are
  INHERITED downstream:** the screening score sheet and the interview feedback sheet display them
  read-only; the interview consolidated report adds a `WeightedAverage`
  (Σ criterionAvg × weight / Σ weight) alongside the plain average. The interview feedback sheet
  shows only Interview-level + global criteria.
- **Multiple evaluators per criterion (migration `AddCriterionEvaluators`):** a criterion carries
  ANY number of evaluators via the child table **`Hrms.CriterionEvaluator`** (criterion 1─<
  evaluator; `EmployeeId?` SET NULL + server-resolved name snapshot for internal evaluators;
  free-text name for `ExternalPerson` / `Organization`). The migration was **hand-reordered** to
  copy the old single-evaluator columns into child rows BEFORE dropping them (scaffolded order
  lost data); legacy empty-type rows were purged. Rules: an evaluator row must be a concrete kind
  (no `None`); the same employee may appear once per criterion (validator 400); evaluator children
  are two levels deep in the requisition aggregate — `StampCriteriaTenant` stamps both levels
  (tenant gotcha). Zero evaluators = "scored by HR". Downstream reads (`CriterionScoreDto`
  score sheets, ranking breakdown) expose a deterministic alphabetical **joined-names** display
  string; requisition reads need `.Include(ScreeningCriteria).ThenInclude(Evaluators)`.
  **Popup UI (enterprise standard):** the criteria designer is a card-per-criterion grid — name /
  weight (with % suffix) / level / mandatory on the first row, an **evaluator chip panel** on the
  second (removable chips with kind icons + inline add row: kind → employee picker or name);
  toolbar has Add Criterion (pre-fills the unassigned weight) and **Distribute Evenly**; the footer
  shows a live weight progress bar (green =100 / amber under / red over) and gates Apply.
- **Ranking & waitlist (`RankingShared`):** `GET JobApplication/ranking?requisitionId=` assigns
  **Rank** and **HireEligibility**. Out of play = stage Rejected/Withdrawn/Hired, fails-mandatory,
  unscored, or **latest offer Declined/Expired** (`OfferRejected`) — a declined offer automatically
  slides the next scored tier into the window. `HireCandidate` enforces the gate whenever the
  vacancy has criteria: only `Eligible` candidates can be hired. Requisitions WITHOUT criteria keep
  the legacy behavior (no rank gate).
- **Large-scale performance (2026-07-12, measured on a 2,000-applicant vacancy):** the pipeline
  list dropped **1.1–3.0 s → 0.13–0.19 s**, ranking 0.44 s → 0.23 s, hire-queue 1.3 s → 0.32 s.
  What was fixed (migration `AddPerformanceIndexes` + code):
  1. **List eligibility went set-based** (`RankingShared.ComputeEligibilityAsync`): the list had
     called the FULL ranking per requisition on every page load — hydrating every applicant's
     criterion breakdown, candidate names and offers with change tracking. Now three no-tracking
     projection queries (light app rows using the stored `ScreeningScore`, mandatory-fail set,
     latest offers) for ALL page requisitions combined, then the same shared assignment logic —
     identical eligibility/rank values, none of the hydration.
  2. **Rank assignment is O(N log N)** — one sort + a walk over score tiers (competition ranks +
     co-eligible ties preserved); the per-row recount was O(N²) (~4 M comparisons at 2 k rows).
  3. **`AsNoTracking` on hot read paths** (ranking hydration, eligibility queries, hire-queue
     docs, latest offers) — `Repository.GetAll()` tracks by default; read-only lists must opt out.
  4. **Hire-queue N+1 removed** — compliance documents now load in ONE batched query per vacancy
     pool instead of one query per candidate row.
  5. **Indexes:** `Hrms.JobApplication (TenantId, AppliedAt)` (the list's tenant-filtered
     `ORDER BY AppliedAt DESC`) and `Hrms.JobOffer (ApplicationId, CreatedAt)` (latest-offer
     lookups scan all statuses; the existing ApplicationId index is filtered to active only).
  6. **Response compression** (Brotli/gzip, Fastest): ranking payload 1.28 MB → 248 KB on the wire.
  7. **Frontend React Query defaults** (`staleTime: 30 s`, `refetchOnWindowFocus: false`,
     `retry: 1`): screen navigation / tab refocus reuses cached results instead of refiring every
     list+lookup query; saves still show fresh data because handlers invalidate their query keys.
- **Tied scores — no hidden tie-break (fixed 2026-07-12):** the old logic sorted only by
  `OrderByDescending(TotalScore)` (a STABLE sort) and then assigned Rank 1,2,3 + top-N Eligible in
  list order — so equal scores were split by the **arbitrary database return order** (clustered
  `Guid` PK order), silently making one tied applicant Eligible and the rest Waitlisted. Now:
  - **Rank is standard-competition:** `Rank = 1 + (# scored candidates strictly higher)` — tied
    scores SHARE a rank (three tied at the top are all "1st", the next is "4th").
  - **Eligibility is tie-safe / co-eligible:** a candidate is `Eligible` when **fewer than the
    open-position count strictly outrank them on score** (`strictlyAhead < openSlots`). All members
    of a tie group share the same `strictlyAhead`, so a tie straddling the last slot makes **every**
    tied candidate Eligible — the engine never breaks a genuine merit tie; HR selects within the
    open positions (the fill-auto-close + hire gate still cap actual hires). Enterprise-standard:
    equal merit is treated equally and the final pick is a transparent human decision.
  - **Deterministic display order** (no arbitrary DB order): `TotalScore` desc → `AppliedAt` asc
    (earliest application) → `CandidateNumber` — decides only the row order, never eligibility.
  - Row exposes **`Tied`** + **`AppliedAt`**; the Ranking modal shows a "TIED" badge and a banner
    when tied candidates are co-eligible ("choose which to advance — the vacancy still closes at its
    open-position count").
- **Score-button visibility rule (level-aware UI):** the "Score against the requisition criteria"
  action on the Applications pipeline renders per row based on the application's CURRENT stage:
  **global criteria (`AppliesAtStage` = null / "All Steps") keep the button visible and enabled on
  every pipeline step; level-scoped criteria surface it ONLY while the application sits at that
  level.** The backend computes this — `JobApplicationDto.ScoreableCriteriaCount` (+
  `TotalCriteriaCount`) on both the list and by-id endpoints: `count(criteria where AppliesAtStage
  is null OR == current stage)`. The UI renders the button iff `scoreableCriteriaCount > 0`
  (terminal stages still disable it), and the **score sheet filters to the same subset** — scoring a
  level-scoped criterion is only possible at its level. When criteria exist but none apply at the
  current step, the sheet explains they belong to other recruitment levels. Scores accumulate
  across steps: the weighted total spans everything scored so far, whichever step recorded it.
- **Hire Employee menu (`/hireEmployee`):** `GET JobApplication/hire-queue` lists STRICTLY the
  fully qualified, ranked applicants (Eligible + Waitlisted) of open Approved/Posted vacancies,
  grouped per requisition with hired/positions counters, rank medals, compliance status, and a
  per-row `CanHire`/`BlockedReason`. The **"Hire as Employee" action moved here** from the
  candidate form (which now shows a pointer note); the hire modal (employee number, vacant
  position, nature, probation) is otherwise unchanged.

### 7.1 Recruitment DB architecture review — decisions (2026-07-10)

An externally-proposed standalone `RecruitmentModule` database (separate .mdf, `Recruitment` schema,
BIGINT identity keys, INSTEAD-OF-INSERT numbering triggers, bespoke approval + pipeline-step tables)
was reviewed and **rejected as-is**; selected ideas were adopted. Rationale (binding for Phase 2+):

**Rejected — and why:**
1. **Separate database**: SQL Server FKs cannot cross databases → every link to `Core.Person`/
   `Hrms.Employee` becomes a comment, not a constraint (and its `PersonID BIGINT` cannot even
   type-match our `uniqueidentifier` PKs). One DB, one EF migration pipeline stays the rule.
2. **No TenantId**: the design is single-tenant; CERP is Finbuckle multi-tenant in a shared DB.
   Every recruitment table keeps `TenantId` — non-negotiable.
3. **BIGINT keys**: rejected for consistency with `BaseEntity`/Guid across the whole product.
4. **Bespoke `RequisitionApproval`**: the generic workflow engine remains the ONLY approval
   mechanism (its `UNIQUE(RequisitionID, ApprovalLevel)` would also break resubmission loops).
5. **`PipelineStep`/`ApplicationProgress` stage-gate**: `UNIQUE(ApplicationID, StepID)` forbids
   re-entering a stage (second interview round, re-screening) — our stage machine + append-only
   `Hrms.JobApplicationStageLog` already satisfy HC098–HC102 without that defect.
6. **Numbering triggers**: EF Core 7+ `OUTPUT`-clause conflicts (`.HasTrigger()` burden), and the
   INSTEAD-OF trigger silently drops any column not re-listed in it. Also the script's
   `GenerateRequisitionNumber()` UDF is invalid SQL — `NEXT VALUE FOR` is illegal in scalar UDFs
   (verified: **Msg 11719**). Numbering stays app-layer and tenant-scoped.
7. **Soft-delete + ON DELETE CASCADE together**: contradictory; we keep RESTRICT + archival status
   semantics (and respect SQL Server's multiple-cascade-path limits — cf. `HiredEmployeeId` no-FK).
8. **`UNIQUE(PersonID, PostingID)`**: one pipeline per person per VACANCY is the rule — uniqueness
   stays requisition-scoped (`Hrms.JobApplication` unique (CandidateId, RequisitionId)).

**Adopted (into our conventions, Phase 2 targets):**
1. **Interview trio shape** — `Hrms.Interview` (schedule/format/status) 1─< `Hrms.InterviewPanelist`
   (lead flag, attendance) 1─< `Hrms.InterviewFeedback` (per-criterion score+comments, FK'ing
   `Hrms.RequisitionScreeningCriterion` — NOT a free-text criterion name) for HC101–HC109.
2. **Offer entity shape** — `Hrms.JobOffer` (tenant-scoped number, Draft→Sent→Accepted/Declined/
   Withdrawn/Expired, expiry + response tracking, hiring manager, `HiredEmployeeId?` handoff feeding
   the existing person-based `HireCandidate`); salary validated against the salary scale (HC113).
3. **DB-level range CHECK constraints** as defense-in-depth on new Phase 2 tables (interview
   end > start; feedback score 0–100; offer salary > 0) — FluentValidation guards only the API
   path. (An expiry-vs-start CHECK was considered and dropped: the response deadline legitimately
   precedes the employment start date.)
4. **`(TenantId, Status)`-leading composite indexes** on hot recruitment tables
   (`Hrms.JobRequisition`, `Hrms.JobApplication` + (RequisitionId, Stage), `Hrms.Candidate`) in the
   Phase 2 migration — the tenant filter leads every query, so EF's per-FK indexes alone don't cover.
5. **Numbering race fix before the public portal (HC093)**: `count+1` numbering is race-prone under
   concurrent creates (unique index turns the race into an error today). Replace with a per-tenant
   counter row updated atomically (`UPDATE … SET Value += 1 OUTPUT inserted.Value`) + retry.

**Deferred, not rejected:** a separate `JobPosting` table (multiple channel-specific postings per
requisition, per-channel windows/URLs) — revisit with job-board feeds (HC092, Phase 3); today
HC088's Internal/External/Both posting on the requisition satisfies requirements.

## 8. Database entity relationships (key foreign keys)

```
Core.Person 1─┐
             └─< Hrms.Employee >── PositionId → Hrms.Position ── PositionClassId → Hrms.PositionClass
                    │  SalaryScaleId → Core.SalaryScale (pay point; grade DERIVED via scale, not stored)
                    │                                       Hrms.PositionClass ── SalaryScaleId → Core.SalaryScale
                    │  BranchId   → Hrms.Branch             Core.SalaryScale ── JobGradeId → Hrms.JobGrade
                    │                                                        └─ StepId     → Core.Step
                    ├─< Hrms.EmployeeEducation / Experience / Dependent / Document  (→ Core.Person)
                    ├─< Hrms.EmployeeMovement / DisciplinaryMeasure / EmployeeTermination
                    └─< Hrms.LeaveRequest / Hrms.LeaveBalance / Hrms.LeaveBalanceTransaction

Leave:  Hrms.LeaveRequest ── EmployeeId → Hrms.Employee, LeaveTypeId → Hrms.LeaveType, FiscalYearId → Core.FiscalYear
        Hrms.LeaveBalance ── (Employee, LeaveType, FiscalYear) UNIQUE
        Hrms.AnnualLeaveSetting ── (FiscalYear, LeaveType) UNIQUE  [accrual policy]
        Hrms.Holiday (standalone; feeds IWorkingCalendar)

Workflow: WorkflowDefinition 1─< WorkflowStep 1─< WorkflowStepApprover
          WorkflowInstance (EntityType+EntityId → any governed record) 1─< WorkflowActionLog

Clearance: Hrms.ClearanceDepartment 1─< Hrms.ClearanceDepartmentApprover (User|Role)
           Hrms.EmployeeTermination 1─< Hrms.TerminationClearance ── DepartmentId? → Hrms.ClearanceDepartment (SET NULL)
           Hrms.EmployeeTermination.VacatedPositionId? (snapshot, no FK) + ReinstatedAt? (reinstatement)

Planning:  Hrms.WorkforcePlan ── StartFiscalYearId → Core.FiscalYear, OrganizationUnitId? → Hrms.OrganizationUnit
           Hrms.WorkforcePlan 1─< Hrms.WorkforcePlanLine ── OrganizationUnitId → Hrms.OrganizationUnit,
                                                            PositionClassId → Hrms.PositionClass
           Hrms.WorkforcePlan.RootPlanId? (version-chain key, no FK)

Recruit:   Hrms.HiringRequest ── OrganizationUnitId, PositionClassId (Restrict); WorkforcePlanId? (no FK)
           Hrms.JobRequisition ── HiringRequestId (Restrict), OrganizationUnitId, PositionClassId,
                                  WorkLocationId?, SalaryScaleId? ── 1─< Hrms.RequisitionScreeningCriterion
                                  ── 1─< Hrms.CriterionEvaluator (EmployeeId? → Hrms.Employee SET NULL)
           Hrms.Candidate ── InternalEmployeeId? → Hrms.Employee (SET NULL), PersonId? → Core.Person (Restrict),
                             HiredEmployeeId? (no FK — cascade-path limit) ── 1─< Hrms.CandidateDocument (Cascade)
           Hrms.JobApplication ── CandidateId, RequisitionId (Restrict; unique pair)
                                  1─< Hrms.JobApplicationStageLog, 1─< Hrms.ApplicationCriterionScore
           Hrms.Interview ── ApplicationId (Cascade) ── 1─< Hrms.InterviewPanelist (EmployeeId? SET NULL)
                                                        ── 1─< Hrms.InterviewFeedback (criterion loose)
           Hrms.JobOffer ── ApplicationId (Restrict), HiringManagerEmployeeId? (SET NULL),
                            SalaryScaleId? (Restrict), HiredEmployeeId? (no FK);
                            unique ACTIVE offer per application (filtered index)
           Hrms.NumberSequence ── PK (TenantId, Key) — atomic per-tenant counters (no BaseEntity)

Auth/tenancy: Tenant 1─< User 1─< UserRole >── Role 1─< RolePermission >── Operation >── Module
              Every hrms_/Core entity carries TenantId (Finbuckle [MultiTenant] filter).
```

**Relationship conventions:** FKs use `OnDelete(Restrict)` except intra-aggregate children (`Cascade`);
self-references (OrgUnit.ParentId, Position.ReportsTo, PositionClass.ReportsTo) are `Restrict` with
cycle-prevention in the update handlers. `UserRole` maps `RoleId`/`UserId` as **plain scalar columns**
(the DB enforces its own FKs) to avoid duplicate shadow FKs.

---

## 9. Salary revision — the three bases (and the step ladder)

A `SalaryRevision` proposes a new salary for every targeted employee. `Rate` means something
different per `SalaryAdjustmentBasis`, which is why the UI relabels the field:

| Basis | `Rate` means | New salary |
|---|---|---|
| `Percentage` | percent uplift | `round(current × (1 + rate/100), 2)`; rejected above 100 |
| `FixedAmount` | flat amount | `current + rate` |
| `Step` | **step increment** (fractional: 1.5, 2.5) | read/interpolated from the salary scale |

A fourth **type**, `Performance`, overrides where `Rate` comes from: see below.

### Step basis — how the salary is derived

1. Employee's grade + rung come from `Employee.SalaryScaleId → Core.SalaryScale (JobGradeId, StepId)`,
   with the rung number being **`Core.Step.Ordinal`** — never `Code`/`Name`, which are free text and
   differ per tenant (`01`/`1`–`8`/`11` vs `S1` vs `ST1`, with "Base" coded `01` and "Celling" `11`).
2. `target = currentOrdinal + increment`.
3. **Clamp** to that grade's ladder: at/above the top rung pays the ceiling (`Capped`), at/below the
   bottom pays the base. It never extrapolates past the ends — a grade has no authority to pay
   outside its own scale.
4. **Bracket + interpolate.** Ladders are GAPPED in real data (grade `01` → ordinals 2,4,5; grade
   `13` → 1,2,3,10), so the landing point is bracketed by the two nearest **defined** rungs
   (binary search), not by `floor`/`ceil`:

   ```
   fraction = (target − loOrdinal) / (hiOrdinal − loOrdinal)
   salary   = round(loSalary + (hiSalary − loSalary) × fraction, 2)
   ```

   Landing exactly on a defined rung is read directly and is NOT flagged `Interpolated`.
5. **Never cut pay.** If the scale value is below what the employee already earns (red-circled /
   off-scale / promoted ahead of a scale refresh), the salary is HELD at current and the line carries
   a `Note`. Without this, "advance everyone 1 step" hands a pay cut to every above-scale employee.
6. Employees with no grade, no step, or a grade with no scale rows keep their pay and are counted in
   the simulation's `UnresolvedCount`.

**Performance contract:** the ladders are built **once per revision run**
(`ISalaryScaleLadderFactory`), never per employee — a per-employee bracket query is an N+1 (10k
employees → 10k+ round trips). The scale is bounded by grades × steps, so one projected read
(index-only via `IX_coreSalaryScale_TenantId_JobGradeId_StepId INCLUDE (Salary)`) serves every lookup;
each employee then resolves in O(log steps) in memory. Any change here must preserve that.

Covered by `CyberErp.Hrms.Tests/Compensation/*` (35 tests) — the only unit tests in the solution.

### Performance-banded revisions (`SalaryRevisionType.Performance`)

A fourth **type** that changes where the amount comes from. The `Basis` still decides the UNIT; the
bands decide the VALUE, per employee:

```
score  ──selects──>  band (MinScore inclusive, highest match wins)
band.Value ──feeds──> the chosen basis  →  2.5 steps  |  15%  |  3000
```

1. Score = the employee's most recent **completed** appraisal (`CompletedAt` + `OverallScore`),
   optionally pinned to `TargetReviewCycleId`. Loaded for the whole population in ONE query.
2. Bands are matched highest-threshold-first on `score >= MinScore`; give the lowest band a floor of
   `0` so it acts as a catch-all.
3. The band value is then applied through the normal basis path — including the step ladder's
   interpolation and the never-cut-pay rule.

**Bands are configuration, never constants.** `Appraisal.OverallScore` is scored against a
**per-tenant `RatingScale`**; the live scales in this DB run **1-5, 1-3 and 0-130**. A hard-coded
"> 90" tier works on 0-130 and silently puts EVERY employee in the bottom band on a 1-5 scale. The
simulation therefore returns `MinObservedScore` / `MaxObservedScore` / `NoScoreCount`, and the UI
warns when a threshold sits above every score observed. Any change here must keep that signal.

Two deliberate rules:
- **No appraisal ≠ low score.** Employees without a completed appraisal are left untouched and
  counted in `NoScoreCount`, rather than being awarded the bottom band on missing data.
- **A zero band is a real decision.** "< 70 → 0%" leaves pay unchanged and is NOT flagged as a
  problem, unlike an unresolved line.

Covered by `CyberErp.Hrms.Tests/Compensation/PerformanceBandTests.cs` (24 tests).

### 9.1 Who a revision may touch (2026-08-09)

Two filters run BEFORE any amount is computed, and both matter because a leaver keeps their last
salary on the record — nothing in the pay data marks them as gone.

1. **Still employed.** `SalaryRevisionShared.StillEmployed` =
   `!IsTerminated && EmploymentStatus != Terminated && EmploymentStatus != Retired`.
   `IsTerminated` and the status are both checked because they are set independently and can disagree
   (the same pair the employee list, options and workforce analytics test); **`Retired` needs its own
   check** because there is no `IsRetired` flag behind it, and a retiree has left just as surely as a
   leaver. Applied in `TargetsAsync` **and again in `ApplySalaryRevision`** — a revision is planned,
   approved and applied over days or weeks, so anyone who leaves inside that window would otherwise be
   paid a raise on their way out.
2. **Positive base salary** (`Salary ?? ScaleSalary`), unchanged.

### 9.2 Increment eligibility policy (`Hrms.SalaryIncrementPolicy`, 2026-08-09)

One active row per tenant (same shape as `WorkWeekConfiguration`); the save endpoint UPSERTS rather
than stacking competing policies. Screen: **Compensation → Increment Rules** (`/salaryIncrementPolicy`
— menu NAMES are per-tenant data, so the live CERP tenant shows it as "Salary Increment Policy";
the ROUTE is what identifies it),
its own menu operation and permission, since deciding who qualifies for a raise is grantable
separately from planning a revision). Absent a policy the defaults are gate 0, proration ON,
disciplinary exclusion ON, promotion OFF.

| Rule | Field | Behaviour |
|---|---|---|
| Minimum service | `MinimumServiceMonths` (0–60) | Excluded below the gate. **Completed months**, so a 3-month gate means the same regardless of month lengths. No hire date ⇒ excluded, not assumed eligible. |
| Active disciplinary | `ExcludeActiveDisciplinary` | Excluded when a non-cancelled, unexpired `DisciplinaryMeasure` **flagged `AffectsSalaryIncrement`** exists. Two levels: the policy decides whether cases count at all, the flag decides which ones. |
| First-year proration | `ProrateFirstYear` | Under 12 months earns `monthsWorked/12` of the **increase**, not of the salary — so it means the same on every basis and can never cut pay. |
| Ceiling promotion | `PromoteOnGradeCeiling` | See 9.3. Defaults OFF: it changes an employee's GRADE, not just their pay. |

Rules are measured **at the revision's effective date** (tenure, and whether a case has expired by
then) — the simulate DTO must send `EffectiveDate` or the API falls back to today.

Excluded employees get **no line at all**, not a zero line: `Apply` walks the lines, so a line would
have paid them. Prorated lines persist `MonthsOfService` / `ProrationFactor` / `Note`, because the
figure has already been approved — re-deriving it later against today's policy would describe a
decision nobody made. An HR override (`SetProposed`) clears those, plus any promotion.

The policy + the disciplinary block set are loaded **once per run** (`ISalaryIncrementEligibilityFactory`),
one `Distinct()` query for the whole population — same batching contract as the ladder and the awards.

**`DisciplinaryMeasure.AffectsSalaryIncrement` defaults to TRUE**, unlike its siblings
`AffectsPromotion` / `AffectsReward`, which are opt-in. Blocking a promotion or a reward is an extra
sanction someone chooses to apply; withholding an increment was already the behaviour of every active
case before the flag existed, so defaulting it off would have quietly started paying people
mid-discipline the moment the column shipped. It is therefore an **opt-OUT** — HR unticks it to exempt
a case — and the migration backfills every existing row to `true`. The three flags are independent: a
case can block promotion and reward while still allowing an increment, and vice versa.

**Three places edit this flag, in two repos.** HRMS: the standalone Disciplinary Cases screen (JSON
save) and the employee-profile Discipline tab (FormData — see the trap below). **Home portal:
`Home/frontend/src/components/admin/disciplinaryCase/` posts to the HRMS API** with its own copy of
the form and list, so a change here needs the mirror updated or the portal silently sends the DTO
default. Home keeps this module on a FLAT route (no `:id`), unlike HRMS — reach its form via the list.

⚠️ **Frontend trap.** The employee-profile Discipline tab submits with `new FormData(form)`, and an
**unchecked checkbox is omitted from FormData entirely**; `createSaveService`'s `booleanFields` only
converts keys that are PRESENT, so an absent key falls through to the DTO default. That is harmless
for the two opt-in flags (absent = false = their default) but would make `AffectsSalaryIncrement`
impossible to untick. The handler therefore sets it explicitly (`fd.set(...)`) before saving.

### 9.3 Grade-ceiling promotion (2026-08-09)

With `PromoteOnGradeCeiling`, a step increment that overshoots the top rung moves the employee to the
next grade instead of stopping at "Capped at the grade ceiling".

- **"Next grade" is resolved BY PAY** — the cheapest grade whose ceiling exceeds theirs. `JobGrade`
  has no level/sort field, and grade CODE order does not track pay in live data (a tenant has code
  `001` paying 10,000–12,000 and `002` paying 2,501–5,529), so following codes would promote people
  into a pay CUT. Ordering by ceiling also skips grades with no scale rows for free.
- **One step buys the move; the remainder climbs the new ladder** (index-based, so gapped ladders
  work). A 1-step overshoot lands on the new grade's base.
- **One grade per revision** — a large overshoot stops at the new grade's ceiling rather than chaining.
- **A promotion that would not raise pay is refused.** Bands overlap; the engine climbs to a rung that
  actually pays more, and if none does it caps as before.
- **A prorated increment cannot buy a grade.** Promoting then scaling the money down would leave the
  employee on a rung of the new grade while paid below its base.
- `Apply` writes `PromotedToSalaryScaleId` onto the employee via `ApplyMovement(..., salaryScaleId)` —
  without that the grade never moves and the same employee is "promoted" again every revision.

`SalaryScaleLadderFactory` therefore loads **every** grade, not just the targeted one; `TargetsAsync`
has already narrowed the population, so nothing is lost.

### 9.4 Approve is the workflow's, once a workflow exists

`SubmitSalaryRevision` calls `StartIfDefinedAsync`; `ApproveSalaryRevision` then calls
`EnsureNoRunningAsync`, which **throws 400** while an instance is running. The detail DTO therefore
carries `AwaitingWorkflow` (`status == PendingApproval && HasRunningAsync`), and the grid hides its
Approve button on it — the same `AwaitingWorkflow` pattern hiring requests, job offers, requisitions
and terminations use. With no definition, direct approval remains the intended path and the button
stays. `Apply` keeps the user on the grid and refetches; `Applied` status is what removes Apply and
Delete.

Covered by `IncrementEligibilityTests`, `ProratedProposalTests`, `GradeCeilingPromotionTests`,
`SalaryRevisionLineTests` and `RevisionPopulationTests` (128 tests in total across the suite).

## 10. Editing a record: cache invalidation and branch reassignment

### 10.1 A save must invalidate the RECORD's key, not just the list

Every entity form runs a detail query keyed `["<entitySingular>", id]` while its grid uses the plural
`["<entityPlural>"]`. Saving used to invalidate only the plural, and the app's `QueryClient` sets
`staleTime: 30_000` — so re-opening the same record within 30 s served the **pre-save copy without
refetching**. The grid showed the new values while the form showed the old ones, and only a full page
reload (which builds a new `QueryClient`) cleared it. That reads as "my edit did not save".

Every save-success block therefore invalidates its own detail key as well:

```ts
queryClient.invalidateQueries({ queryKey: ["organizationUnits"] });   // the grid
queryClient.invalidateQueries({ queryKey: ["organizationUnit"] });    // the record itself
```

The bare key prefix-matches every `["organizationUnit", <id>]` entry. Prefix matching compares array
ELEMENTS, not strings, so `["loan"]` never touches `["loanType", id]` — the substring trap that
applies to route matching does not apply here. A form that already invalidates the targeted form
(`["candidate", formState.id]`) is equally correct and must not be double-patched.

> When adding an entity form, add BOTH lines. The list-only version looks like it works, because the
> grid updates; the failure only shows on the next Edit, inside the 30 s window.

### 10.2 Branch reassignment must fail out loud

`OrganizationUnit` is `IBranchScoped`. Head Office may move a unit between branches; a branch admin
may not — that is deliberate isolation, and `ICurrentUserService.IsHeadOffice()` (a **cookie**, set by
`LoginRepository.SetBranchCookies` at sign-in) decides. What was wrong is that breaking the rule was
SILENT: `UpdateOrganizationUnit` substituted the entity's existing `BranchId` and still answered
**200**, so the user was told the save succeeded and watched the field revert.

The rule now throws a 400 naming the restriction, and only when the value would actually CHANGE —
an omitted or unchanged `BranchId` is not an attempt to reassign and must never fail an ordinary
edit. Create is different and stays as it was: it PINS a branch admin's new unit to their own branch
(`GetCurrentBranchId()`), which is an assignment, not an override.

## 11. ⚠️ `IsAdmin` is NOT an authorization check in this deployment

`PerformanceVisibilityService.IsAdminAsync` short-circuits on `ICurrentUserService.IsHeadOffice()`,
and that flag is `isBranchHeadOffice` — true when the employee's branch has `IsHeadOffice = 1`. **CERP
has ONE branch and it is flagged head office, so every one of the 490 employee-linked users resolves
to `IsAdmin = true`.** Any check written as *"if IsAdmin then show everything"* therefore applies to
ordinary staff.

This is not theoretical; it has produced real defects:

- The portal's Other Leave grid listed the WHOLE organisation's leave, because `GetAllOtherLeaves`
  widens for `IsAdmin` (§11.2).
- A first cut of the salary-revision review endpoint gated on `IsAdmin` and granted the entire
  payroll to every user; it now gates on the `salaryRevision` MENU PERMISSION, which has no
  head-office bypass (`IEndpointPermissionService.HasAnyAsync`).

**When you need a real gate, use one of these instead — never `IsAdmin` alone:**

| Need | Use |
|---|---|
| "HR only" | `IEndpointPermissionService.HasAnyAsync(["<screen>"])` — role `CanView`, no bypass |
| "this employee's own data" | a dedicated `/mine` endpoint (§11.2) |
| "the person approving it" | `IWorkflowApproverAuth.ResolveApproverUserIdsAsync` (§11.3) |

### 11.5 The audit: what is actually exposed, and why it cannot simply be fixed

**143 `IsAdmin` checks across 60 files are no-ops.** Measured as an ordinary employee: a
`GET CalibrationSession` (explicitly *"Only HR can view calibration sessions"*) answered **200**, and
five HR-only POSTs — `LoanType`, `MedicalPlan`, `InsurancePolicy`, `PerDiemRate`, `BenefitPlan` —
reached FIELD VALIDATION, proving the HR gate never fired. The contrast: `POST SalaryRevision`
answered **403**, because that controller also carries `[RequirePermission]`.

Three shapes, worst first:

| | Pattern | Count | Effect | Status |
|---|---|---|---|---|
| A | `!IsAdmin && notMine → throw` | 16 | Guard unreachable — any employee could act on ANOTHER's loan / trip / guarantee | **FIXED (§11.7)** |
| B | `IsAdmin → throw "Only HR…"` | 54 | HR-only operations open to all staff | **FIXED at the root (§11.8)** |
| C | `if (!IsAdmin) narrow query` | 73 | Row scoping skipped — this is the Other Leave defect | **FIXED at the root (§11.8)** |

> ⚠️ **THE BLOCKER (now CLEARED — see §11.6): 480 of the 490 employee accounts had NO ROLE AT ALL.**
> `HasAnyAsync` needs a role carrying `CanView`, so every `[RequirePermission]` added returned 403 for
> those 480 — and fixing `IsAdminAsync` at the root had the same effect, because those users could
> only use the system BY VIRTUE OF the bug.

**What has been done:** `[RequirePermission]` added to 31 pure HR/master-data controllers, where a
403 for a roleless employee is the CORRECT answer. None of them exposes a self-service route
(`/me`, `/mine`, `/my*`) — that was checked before applying, and `EmployeeController` was deliberately
EXCLUDED because it carries `Employee/me`. Verified both directions: roleless 403 / Administrator 200
on 15 endpoints, with self-service (`Employee/me`, `OtherLeave/mine`, `AnnualLeave/mine`,
`my-balance`, `Workflow/my-approvals`, `AppraisalPeer/mine`) still 200.

Categories A and C remain OPEN: they live inside handlers, where a controller-level filter cannot
reach them.

### 11.6 Every employee now holds a role — the prerequisite is done

`backend/scripts/assign-employee-role.sql` (idempotent; run it on any other environment) does two
things: it grants the ordinary `UserRole` the employee-facing screens it was missing, then assigns
that role to every employee-linked account that had none. **480 accounts assigned; 0 roleless
employees remain**, and 495 users now hold it.

The missing grants were `/myGuarantees`, `/myInsuranceClaims`, `/myTraining`, `/notifications`,
`/workflow`, `/surveyTake`, `/recognitionWall`, `/learningCommunity`, `/appraisalAppeal`. Four
look self-service but were deliberately EXCLUDED, and should stay that way: `/employeeGuarantee` (the
HR register — employees get `/myGuarantees`), `/transferRequest` (the *Manager Requests* module),
`/exitQuestionnaire` (Personnel/HR — employees get `/myExit`) and `/compensationRequest` (employees
already hold `/myCompensation`).

The change only ever ADDS access, so it cannot lock anyone out. In practice it flipped existing
`CanView = 0` rows rather than inserting: the role already carried a row per operation, which is why
the permission-row count stayed at 598.

> ⚠️ **The catalog holds DUPLICATE operations per link** (150 rows, 132 distinct links). A link is
> granted when ANY row for it grants it, so audit queries must aggregate by `Link` — checking a
> single row reports false gaps.

Measured after: an account that was roleless reaches `Employee/me`, `OtherLeave/mine`,
`AnnualLeave/mine`, `Workflow/my-approvals`, `AppraisalPeer/mine` and the portal's loan/trip/medical
feeds (200), is refused every HR master screen (403), and its sidebar resolves to **34 links against
an Administrator's 144**. Five randomly sampled accounts behaved identically.

**This unblocks the rest**: categories A and C can now be closed, and `IsAdminAsync` can be repointed
off `IsHeadOffice()` — with the same acceptance test as phase 2, that each user's effective
permissions are unchanged where they should be.

### 11.7 "Owner OR HR" guards check a menu permission, never `IsAdmin`

All 16 category-A guards now read
`if (!await permissions.HasAnyAsync(HrScreens.X) && record.EmployeeId != mine) throw`.

`HrScreens` (App/Common/Authorization) names the link once per record type. **Each is the HR-side
REGISTER, which ordinary staff do not hold** — they hold the matching self-service screen:
`/loan` vs `/myLoans`, `/trip` vs `/myTrips`, `/employeeGuarantee` vs `/myGuarantees`,
`/trainingNeed` vs `/myTraining`.

> ⚠️ **Before reusing one of these for a new guard, check the two sides really differ.** A link BOTH
> hold cannot discriminate — which is exactly why grievances use `HrScreens.EmployeeRegister`
> (`/employee`, held only by Administrator and HR Admin): every employee holds `/grievance`, so it
> would have been useless.

Proven against real records — the three parties hit three DIFFERENT gates, which is the whole point:

```
employee B (not owner) : "You can only cancel your own loan requests."   <- ownership guard fires
owner       (employee) : "This record is awaiting workflow approval…"    <- passed ownership
HR                     : "This record is awaiting workflow approval…"    <- override intact
```

> ⚠️ `RewardNominationHandlers` also contains `if (!scope.IsAdmin && !scope.IsManager)` — an
> HR-or-manager gate, NOT an ownership guard. It was left as-is here (it belongs to category B) and is
> now fixed by §11.8 along with the rest of that category.

### 11.8 `IsAdmin` means a PERMISSION — categories B and C fixed at the root

`IsAdminAsync` no longer short-circuits on `IsHeadOffice()`. It now reads:

```csharp
if (await permissions.HasAnyAsync(HrScreens.EmployeeRegister)) return true;   // /employee
// …then the existing fallback: an explicit User/Role approver on the Appraisal HrSignOff step
```

**One line closes both remaining categories.** The 73 scoping sites and the 54 `"Only HR…"` gates were
never wrong in themselves — they ask `scope.IsAdmin`, which was simply answering "yes" for everyone.
None of those 73/54 sites was edited; they started working the moment the answer became correct.

`/employee` is held only by Administrator and HR Admin, so it says what the method always meant.
Head-office status still drives BRANCH scoping through `ICurrentUserService` — it just no longer
doubles as "this person is HR".

> ⚠️ **This depends on §11.6.** It is only survivable because every employee now holds a role: before
> that, removing the head-office short-circuit would have left 480 accounts with no permissions at
> all. Do not port this change to an environment where `assign-employee-role.sql` has not run.

**Acceptance test — effective visibility per user, same endpoints before and after:**

```
                        Emp   Appr  OthL  AnnL  Goal
BEFORE  Administrator   345    1     1     1     3
        employee        345    1     1     1     3     <- saw the whole organisation
        employee        345    1     1     1     3

AFTER   Administrator   345    1     1     1     3     <- HR unchanged
        employee          1    0     0     0     0     <- self only
        employee          1    0     1     0     0     <- keeps their OWN leave request
```

The last row is the useful signal: that employee retains exactly one other-leave row because it is
theirs, while the employee with none sees zero — per-owner scoping, not a blanket zero. The manager
tier resolves in between: two managerial employees see **2** and **5** employees (their unit
subtrees) against HR's 345.

Verified with no 500s across twelve modules, self-service unaffected, and the portal's News Feed
still working — it reads `Announcement/feed`, which is open to staff, while the admin `Announcement`
list now correctly refuses them.

### 11.1 Approval precedes submission (salary revision)

`Draft → PendingApproval → Approved → Submitted → Applied`. The author may only SEND FOR APPROVAL;
`Submit` is unreachable until an approver has approved, and `Apply` requires `Submitted`. Sending for
approval REFUSES when no active `SalaryRevision` workflow definition exists — otherwise the revision
lands in PendingApproval with nobody but its author able to approve it, which is the self-approval
hole the states exist to prevent. `Status` persists as a STRING, so adding `Submitted` needed no
migration. Every transition writes a `PerformanceHistory` row, which is what makes "who created / who
approved / who submitted" three separately attributed facts.

### 11.2 Self-service grids read a `/mine` endpoint

A personal screen must never call the role-widened list. `AnnualLeave/mine` and `OtherLeave/mine`
scope to `scope.EmployeeId` with **no** admin/manager widening, return EMPTY for an account with no
linked employee (never a broader set), and apply that scope BEFORE the `employeeId` query filter — so
passing someone else's id widens nothing. The portal's Other Leave grid is additionally pinned to
`status=Pending`: a decided request belongs in the employee-profile tab, which is the record of what
was granted.

### 11.3 Reading a record you are approving

An approver is routed a request by the WORKFLOW, which says nothing about whether they manage the
requester or hold the owning screen's permission — so deciding blind was the default. `OtherLeave`
and `SalaryRevision` both expose a `/review` endpoint granting the assigned approver read access to
the record (and, for leave, its attachment).

> ⚠️ Resolve the approver with **`ResolveApproverUserIdsAsync`, never `EvaluateAsync`**.
> `EvaluateAsync` answers "may this person DECIDE", and for an OPEN step (no configured approvers)
> that is TRUE FOR EVERYONE by design. Routing READ access through it would hand a colleague's
> medical certificate to the whole tenant the moment a step is left unconfigured — which is exactly
> how the OtherLeave chain ships. `ResolveApproverUserIdsAsync` returns an empty set for an open step.

### 11.4 Leave approval notifies the requester

Approval is the moment a request LEAVES every list the employee was watching — the approver's inbox
(the instance stops running) and the requester's pending feed (filtered to Pending). `LeaveNotifier`
therefore mails the requester on annual- and other-leave approval, AFTER the commit and never
throwing: a mail failure must not undo an approval. An employee with no address on file is logged,
not failed. Note the chain is TWO steps (Supervisor Review → HR Approval); the leave becomes
Approved only when the instance COMPLETES, so step 1 leaves that approver's queue while the request
correctly stays Pending for the requester.

## 12. The SRMS platform layer (phase 1 of 2)

`cybererp_srms` is a DIFFERENT PRODUCT, not a newer CERP: its 326 operations and CERP's 150 share
**zero** links. What is worth taking from it is its platform architecture, which is a generation
ahead of ours.

### 12.1 What landed (additive, phase 1)

Seven tables in `Core`, migration `AddSrmsPlatformLayer` — `Organization`,
`OrganizationSubscription`, `SubscriptionPlanModule`, `TenantSubscriptionAddOn`, `LoginTrail`,
`Setting`, `UserPreference`. Nothing existing was altered or dropped, so no login or permission
behaviour changed.

**Which of them are tenant-scoped matters.** `Organization` sits ABOVE the tenant (one organization
may hold several), and the plan/add-on rows are billing records *about* tenants that platform staff
must read across all of them, so none of those carry `[MultiTenant]`. `Setting` is a deployment
SINGLETON. `LoginTrail` and `UserPreference` do carry the `BaseEntity` tenant discriminator.

`LoginTrail` is wired into `LoginRepository`: success, wrong password, and unknown user name. It
stores the attempted name SEPARATELY from `UserId` (a failed attempt often has no user to point at,
and that is the case worth recording), never the submitted password, and has **no FK to `Core.User`**
— an audit row that disappears with its subject is not an audit row. Writing it can never fail a
sign-in: `RecordLoginEventAsync` swallows and logs.

> ⚠️ Two deliberate OVERLAPS, left alone so this phase stayed additive:
> `Organization` duplicates much of `CompanyProfile`, which still feeds offer letters and report
> letterheads; and `Setting`'s SMTP columns duplicate the `Email` section of `appsettings.json`,
> which is what `SmtpEmailService` actually reads. Neither has been repointed — doing so silently
> would change what letters render and where mail is sent from.

### 12.3 Phase 2 step 1 — the model exists and is MIRRORED, but nothing reads it yet

Migration `AddTenantScopedAuthorization` creates the six tables; `backend/scripts/
seed-tenant-authorization.sql` fills them FROM CERP's own data. **No reader has been switched over**,
so this changed no behaviour — verified at runtime (Administrator 345 employees / 144 sidebar links,
employee 1 / 34, manager 2 / 34, all identical to before).

**The mirror is 1:1 with the live model:**

```
TenantRole 8   TenantOperation 150   TenantRolePermission 598
TenantUser 500 TenantUserRole 503    TenantSubSystem 7
```

> ⚠️ **The existing model is ALREADY tenant-scoped** through the `TenantId` discriminator: 3 roles
> belong to `demo`, 5 to `headoffice`, and all 150 operations and 598 permissions to `headoffice`.
> So each row joins to ITS OWN tenant. **Do not cross join `Role × Tenant`** — that replicates every
> role into every tenant, and the membership insert then makes each user a member of ALL of them
> (506 users produced 1500 memberships on the first attempt, caught by the row counts).
>
> ⚠️ `SELECT DISTINCT NEWID(), …` does NOT dedupe — `NEWID()` makes every row unique. Resolve the
> distinct pairs in a subquery first, or a user holding two roles yields two membership rows.

**Acceptance test** (`seed-tenant-authorization-verify.sql`, read-only) compares EFFECTIVE
permissions per user — the distinct `(Link, CanView, CanAdd, CanEdit, CanDelete, CanApprove)` each
user reaches through any role — with a full outer join in both directions:

```
old_grant_rows 70852 | new_grant_rows 70852 | lost_in_new 0 | gained_in_new 0
users_whose_viewable_link_count_differs 0
verdict: MATCH - effective permissions identical in both models
```

`CanExport` is seeded FALSE everywhere: the old model has no such column, so granting it would invent
access nobody assigned.

**Step 2 (not started) is the flip** — pointing `IEndpointPermissionService`, login, the DB-driven
sidebar and the Role Permissions screens at the new tables. Re-run the verify script immediately
before flipping: it must still say MATCH, or the live model has drifted since the seed.

### 12.2 What phase 2 is, and its one hard rule

The tenant-scoped auth model — `TenantRole` (from a `Role` TEMPLATE, with `SourceTemplateId` and
`IsCustomized`), `TenantOperation`, `TenantRolePermission` (which adds **`CanExport`**), `TenantUser`,
`TenantUserRole`, `TenantSubSystem` (per-tenant licensing with status and dates).

**The rule: generate the new rows FROM CERP's existing data, never from SRMS's.** SRMS ships 1 role,
3 users and 6 permissions against operations that match none of our screens; CERP has 8 roles, 150
operations, 598 permissions and 506 users. The map is
`Role → TenantRole`, `Operation → TenantOperation`, `RolePermission → TenantRolePermission`,
`UserRole → TenantUser + TenantUserRole`, and the acceptance test is that each user's EFFECTIVE
permission set is identical before and after. Everything that reads the current model has to move in
lockstep: login, `PermissionAuthorizationFilter`, `IEndpointPermissionService`, the DB-driven sidebar
and the Role Permissions screens.
