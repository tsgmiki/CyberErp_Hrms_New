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

**Step 2 is the flip** — see §12.4.

### 12.4 Phase 2 step 2 — the flip: the runtime now READS the tenant-scoped tables

The two readers moved:

| Reader | Now resolves through |
|---|---|
| `EndpointPermissionService.LoadGrantedLinksAsync` (the `[RequirePermission]` gate) | `TenantUser`(Active) → `TenantUserRole` → `TenantRolePermission`(CanView) → `TenantOperation`(IsActive) |
| `GetModuleWithOperationsRepository` (the sidebar feed) | the same chain; `Name`/`Link`/`Icon`/order come from the tenant's OWN copy |

Two capabilities the old global join could not express are now live: a user belonging to several
tenants sees only the tenant they signed in to, and `IsActive = 0` genuinely revokes a screen rather
than merely hiding its menu entry. `OperationRecord.Id` still reports the TEMPLATE id, because that
is what the role-permission screen sends back.

#### ⚠️ Why a projector, and why every admin write calls it

The runtime READS the tenant tables; the admin screens still EDIT the global ones. Without something
in between, saving a permission would update a table nobody reads and **appear to do nothing**.
`ITenantAuthorizationProjector.SyncAsync()` closes that gap and is called from every write path:
role save/delete, user-role save/delete, role-permission save/delete, operation create/update/delete,
and `SeedDefaultMenu`.

It is a FULL reconcile, not a surgical per-row update. One tenant is ~150 operations, 8 roles and
~600 grants — trivial for an action a human performs a few times a day — and in exchange it is
*self-healing*: a write path that forgets to call it is corrected by the next sync. What it cannot
see is a change made directly in SQL; that needs a manual (idempotent) run of
`seed-tenant-authorization.sql`.

It deliberately does **not** touch anything the global model cannot express: grants and assignments
belonging to a role with no `SourceTemplateId` (a bespoke tenant role) are skipped by the revocation
sweep, and `TenantRole.SyncFromTemplate` is a no-op once `IsCustomized` is set. Otherwise the next
admin save would silently strip local customisation.

#### ⚠️ Three FK traps — deletes that must clean up BEFORE the save, not via the projector

The projector runs *after* `SaveChangesAsync`, so anything the database would reject has to be
handled inline:

| Delete | FK | Consequence if left to the projector |
|---|---|---|
| `User` | `TenantUser → User` is **NoAction** | the delete FAILS outright |
| `Operation` | `TenantOperation → Operation` is **Restrict** | the delete FAILS outright |
| `Role` | `TenantRole → Role` is **SetNull** | worse — it SUCCEEDS, blanking `SourceTemplateId`. A null source reads as a *bespoke* role, which the projector never touches, so the role lives on invisibly, still granting its permissions |

`DeleteUser`, `DeleteOperationHandler` and `DeleteRole` therefore each clear the tenant rows (and
their dependent grants) explicitly first.

#### Cache invalidation

The granted-link set is cached 60s per user. Cache keys now carry a **generation number** because
`IMemoryCache` cannot enumerate or clear by prefix; `InvalidateAll()` bumps it, orphaning every entry
at once, and the projector calls it whenever it writes. The TTL now only bounds changes made behind
the application's back.

#### Verification

`backend/scripts/verify-tenant-auth-readers.sql` (read-only) transcribes both runtime queries,
predicate for predicate, and compares old against new across the whole population:

```
memberships_not_active 0 | operations_hidden 0 | users_with_a_role_but_no_membership 0
old_gate_rows 15369 | new_gate_rows 15369 | gate_lost 0 | gate_gained 0  -> MATCH
old_menu_rows 17409 | new_menu_rows 17409 | menu_lost 0 | menu_gained 0  -> MATCH
users_whose_sidebar_size_differs 0
```

The first line matters most: those three counts are the ONLY way the readers can diverge while the
model test still passes, since `Status` and `IsActive` have no counterpart in the old model.

Live: `hoadmin` (UserRole) got 34 links — matching the old model exactly — 403 on `RolePermission`
and `Role`, 200 on `OtherLeave`, 401 unauthenticated. The projector was exercised with a **throwaway
operation**: create → tenant copy appeared; rename → copy followed (`/zzFlipProbe2`, order 8888);
delete → both rows gone and all six baseline counts back to 150 / 8 / 598 / 500 / 503.

### 12.5 Aligning Core.User / Core.Role / Core.Operation with the SRMS schema

Migration `AlignCoreTablesWithSrms`. The three tables now carry **exactly** SRMS's column set — verified
by diffing `INFORMATION_SCHEMA.COLUMNS` between the two databases in both directions — with **one
deliberate extra: `TenantId`**.

| Table | Change |
|---|---|
| `User` | `Password` → **`PasswordHash`**; + `AccountStatus`, `FailedLoginAttempts`, `LockoutEndUtc`, `TwoFactorEnabled`, `ProfilePicture`(+`ContentType`), `IsPlatformAdministrator`, `NormalizedEmail`, `NormalizedUserName` |
| `Role` | `Code` → `nvarchar(80) NOT NULL`; `Name` 200→100; + `Description`, `IsPlatformRole`, `IsActive` |
| `Operation` | `SortOrder` → **`DisplayOrder`**; + `SubSystemId` (FK), `IsActive`; four columns narrowed to the SRMS caps |

#### ⚠️ Why TenantId was KEPT

SRMS has no such column: there, a user is a global identity and tenancy lives entirely in `TenantUser`.
`Repository<T>` filters *every* query on `TenantId`, so dropping it would stop scoping users, roles and
operations and the User/Role screens would show all tenants at once. That is a separate, deliberate
change requiring those screens to be refactored onto `TenantUser`/`TenantRole` — not a side effect of a
column alignment.

#### ⚠️ Three departures from SRMS, each forced by real data or a real dependency

1. **`IX_User_NormalizedEmail` is FILTERED** (`WHERE NormalizedEmail <> ''`), not a plain `UNIQUE`.
   **489 of 506 accounts have no e-mail address on file** — a plain unique index cannot be created
   because they all collide on `''`. SRMS gets away with it on 3 users.
2. **`Operation.ModuleId` still points at `Core.Module`.** SRMS has **no Module table**: its operations
   nest into each other, and the column it calls `ModuleId` actually carries an FK back to
   `Operation.Id` — a renamed `ParentOperationId` whose constraint name was never updated (the one
   named `FK_Operation_Module_ModuleId` in fact sits on `SubSystemId`). CERP groups 150 operations
   under 24 modules and the sidebar depends on it. `SubSystemId` was added as a real FK to
   `Core.Subsystem`, denormalised from the module — the same value `TenantOperation` already stores.
3. **`IX_Role_Code` is NOT unique**, unlike SRMS's `IX_StandardRoleTemplate_Code`. Two tenants may each
   define an "Administrator", and the index cannot be scoped to `(TenantId, Code)` because `TenantId`
   is `nvarchar(max)`, which SQL Server will not index. Uniqueness is enforced **per tenant in
   `SaveRole`**, the way the role name already was.

#### ⚠️ The scaffolded migration was not runnable as generated

EF orders operations by dependency, not by data, so three would have failed or corrupted:

| Scaffolded | Why it breaks | Fix |
|---|---|---|
| `UNIQUE IX_User_NormalizedUserName` | created while all 506 rows hold `''` — every one collides | backfill `UPPER(LTRIM(RTRIM(...)))` first |
| FK `Operation → Subsystem` | `SubSystemId` defaults to an empty Guid, which matches no subsystem | backfill from `Module.SubsystemId` first |
| `Role.Code` → `NOT NULL DEFAULT ''` | silently blanks the 5 roles that had no code | backfill `UPPER(REPLACE(Name,' ','-'))` first |

#### ⚠️ The Home portal shares this database

`D:\Workspace\CyberErp\Home` maps the same three tables (`Navigation.cs`) and reads the password column
directly in `LoginUser.cs` / `ChangePassword.cs`. Its entity, its portal feed and `SeedHomeMenu` were
updated in the same pass — **the two repos must be deployed together**, the same trap as
`Core.Notification`. All shared tables are `ExcludeFromMigrations` there, so Home will not fight the
schema.

#### What did NOT change

The **API/DTO wire contract still says `sortOrder`**, mapped to the entity's `DisplayOrder`. `Module`
still has a genuine `SortOrder`, so renaming only the operation half would have made two adjacent
screens inconsistent for no schema benefit — and it keeps both SPAs working untouched.
`Core.RolePermission` (598 rows) and `Core.Module` (24 rows) **do not exist in SRMS** at all;
RolePermission is already superseded at runtime by `TenantRolePermission` (§12.4), but removing either
is its own decision, not part of a column alignment.

#### Verification

```
column-set diff vs SRMS:  0 missing, 0 type/length mismatches,
                          3 extras (the deliberate TenantId columns)
backfills: 506 hashes intact, 0 blank normalised usernames, 0 blank role codes,
           150 operations resolved, 0 subsystem mismatches
readers:   gate MATCH 15369=15369, menu MATCH 17409=17409, 0 users differing
model:     MATCH 70852=70852, 0 lost, 0 gained
live:      HRMS login 200 + 34 links; Home login 200 + portal feed correct; 0 errors in either log
projector: throwaway operation created (SubSystemId resolved from module, tenant copy order 7777)
           and deleted cleanly
```

### 12.6 Core.Operation becomes the menu TREE (parent–child, SRMS topology)

Migration `OperationParentChildHierarchy`. `Core.Operation` is now self-referencing:

```
ModuleId IS NULL      -> a PARENT: a menu group (the row a Module used to be)
ModuleId IS NOT NULL  -> a CHILD:  a screen, hanging off the parent with that Id
```

The column keeps its name — that is what SRMS calls it. The 24 `Core.Module` rows were copied in as
those parents, giving **174 = 24 + 150**.

#### ⚠️ Each parent REUSES ITS MODULE'S Id

Verified beforehand: zero collisions with existing operation ids. This is what made the migration
cheap — the 150 children already held those values in `ModuleId`, so **not one needed repointing**.
It also establishes the invariant the entity and both seeders rely on: **a parent operation and its
module share an Id.**

`Core.Module` is NOT dropped and must not be: `SubscriptionPlanModule` and `TenantSubscriptionAddOn`
have foreign keys into it. It simply stops being what navigation reads. `Module.Operations` is gone.

#### ⚠️ The scaffold added the self-FK before the parents existed

EF drops the old FK, widens the column, then adds the self-reference — but adds it while all 150
children still point at keys not yet in the table, so the constraint cannot be created. The copy has
to happen in between. `Down()` also has to delete the parent rows (and their tenant copies, which are
`Restrict`) before `ModuleId` can be `NOT NULL` again.

`NoAction`, not `Cascade`: SQL Server rejects a cascading self-referencing foreign key outright.
`DeleteOperationHandler` therefore refuses to delete a group that still has children, with a message
naming the count rather than an FK error.

#### ⚠️ The bug this introduced, and how it showed up

The first sidebar build returned **an empty menu**. `TenantOperation.ModuleId` is copied straight from
the template, so it names a **`Core.Operation`** — but the grouping matched it against the *tenant
copy's own* `Id`, which never joins. Fixed by matching on `OperationId`. Worth remembering: the
tenant tables carry template ids in their parent link, not tenant-row ids.

#### What reads the hierarchy now

| Reader | Change |
|---|---|
| `GetModuleWithOperationsRepository` (sidebar) | groups are `TenantOperation` rows with a null `ModuleId`; one query returns the whole tree |
| `GetAllOperationsRepository` | filters on the row's own `SubSystemId`; orders group-then-children |
| `GetOperationByIdHandler`, `RolePermissionHandlers` | "Module" is the parent's name, blank when the row IS a group |
| `CreateOperationHandler` | a null `ModuleId` creates a GROUP (requires `SubsystemId`); a child takes its parent's subsystem, and a screen is rejected as a parent |
| `SeedDefaultMenu` | creates the group as an operation **and** a `Core.Module` sharing its Id |
| Home `GetMySubsystems` | self-joins to the parent instead of `Core.Module` |

Home's `Core.Module` join would still have *worked* — via the shared-Id invariant — but only for
groups predating the change: a group created through the HRMS screen writes no module row, and every
screen under it would have vanished from the portal.

The **wire contract is unchanged** — the feed still calls the outer objects "modules" — so neither SPA
needed a change. `OperationDto.ModuleId` is now nullable.

#### Verification

```
tree        174 operations = 24 parents + 150 children
            0 children without a parent | 0 parents disagreeing with their module
            0 parents carrying a link (a group must grant nothing)
projection  TenantOperation 174 = 24 groups + 150 screens
readers     gate MATCH 15369 | menu MATCH 17409 | 0 users differing
live        HRMS 34 links under 12 groups (identical to before)
            Home 2 subsystems / 12 modules / 34 operations
round-trip  group + child created via the API, delete-with-children refused
            with a plain message, then both deleted — baseline back to 174/24/598
```

### 12.7 Core.RolePermission retired

Migration `RetireCoreRolePermission`. `Core.TenantRolePermission` is now the **only** grant table.

The old table had had no reader since the phase-2 flip (§12.4); this change removes the last thing
writing to it. The Role Permissions screen now writes the tenant table **directly**, so a save is
live the moment it commits instead of going through a projection.

**Proved redundant immediately before the drop** — every user's effective
`(link, CanView, CanAdd, CanEdit, CanDelete, CanApprove)` compared across both models in both
directions: **70,852 rows each side, 0 lost, 0 gained**. The migration also carries a `THROW` guard
that refuses to drop while `TenantRolePermission` is empty, because that would destroy the only copy.

#### What moved

| | Now |
|---|---|
| `SaveRolePermissions` | writes `TenantRolePermission`; resolves the screen's global `RoleId`/`OperationId` to this tenant's instances, so **the wire contract is unchanged** |
| `GetAllRolePermissions` | reads the tenant tables, reports TEMPLATE ids so the save call still round-trips |
| `DeleteRolePermission` | deletes the tenant grant |
| `WorkflowApproverAuth` | open-step approvers resolve through the tenant chain, still returning template role ids (it is compared against `Core.UserRole`) |
| Home `GetMySubsystems` | walks `TenantUser → TenantUserRole → TenantRolePermission → TenantOperation` |

`CanExport` has no field on the screen, so a **new** grant never sets it and an **edit preserves**
whatever was there — silently clearing a privilege the UI cannot display would be worse than either.

#### ⚠️ The projector no longer projects permissions

`SyncPermissionsAsync` is **deleted, not disabled**. With no template table behind it, its revocation
sweep would treat every hand-edited grant as orphaned and delete the lot. Do not reinstate it.
`Role`, `Operation` and `UserRole` are still projected.

#### Scripts

`seed-tenant-authorization-verify.sql` and `verify-tenant-auth-readers.sql` are **deleted** — both
existed to compare against `Core.RolePermission` and could now only fail with "invalid object name".
Replaced by `verify-tenant-authorization.sql`, which checks the model's own consistency: dangling
references, cross-tenant leakage (a grant whose role and operation belong to different tenants — the
two FKs are independent, so nothing else asserts this), and menu-tree integrity.
`seed-tenant-authorization.sql` no longer seeds permissions, and `assign-employee-role.sql` writes
tenant grants.

#### Verification

```
drop        Core.RolePermission gone | Role 8, UserRole 503, Operation 174 all kept
grants      598 preserved | effective grant rows 70852, unchanged
integrity   0 dangling refs | 0 cross-tenant grants | 0 orphan screens | 0 groups with a link
live        HRMS 34 links | Home 2 subsystems / 12 modules / 34 ops | 0 errors in either log
round-trip  the rewritten screen exercised end to end against the live API: GET returned 149 rows
            with template ids and parent-group names; POST flipped one grant on, then back off;
            CanExport untouched; baseline restored to 598 grants / 15369 viewable pairs
```

### 12.8 The ungated navigation controllers, closed

`SubsystemController`, `ModuleController` and `OperationController` carried **no
`[RequirePermission]` at all**, so any authenticated user could create, rename or delete menu
entries. Found while testing the phase-2 flip — that is how a throwaway operation was created under a
non-admin account (§12.4).

#### ⚠️ Gating the CONTROLLERS would have been worse than the hole

The obvious fix is a class-level attribute. It is wrong, because the **reads** here are infrastructure
every signed-in user depends on:

| Read | Consumer |
|---|---|
| `GET Module/WithOperations` | the sidebar feed itself |
| `GET Module`, `GET Subsystem` | `useMenuModules`, the landing page, the menu filters |
| `GET Operation` | `permissionGate.tsx` builds its **catalogSet**, `globalSearch.tsx` filters results |

The last is the trap. `PermissionGate` treats "not in the catalog" as "not a gated page", so a 403 on
that read would empty the catalog and **every route would fall through ungated** — the fix would open
a strictly bigger hole than the one it closed. Gating `WithOperations` would simply leave everyone
with no menu.

So the attributes go on the **mutating actions only** — `Create`, `Update`, `Delete` on all three, plus
`Module/seed-defaults`, which rewrites the whole tree and is gated like editing it by hand. The reads
expose menu metadata (names, links, icons, order), never anyone's data, and `WithOperations` is
already filtered to the caller's own grants.

Links used: `subsystem`, `module`, `operation`. Administrator and HR Admin already hold `CanView` on
all three, so no role lost anything.

#### Verification

Both directions, live:

```
non-admin (UserRole)   GET Operation/Module/WithOperations/Subsystem  200 200 200 200
                       POST Operation / Module / Subsystem / seed-defaults  403 403 403 403
                       sidebar 34 links, unchanged

with the permission    POST Operation 200, throwaway created and deleted
                       (granted temporarily, then restored to 000000)
baseline               174 operations | 598 grants | 0 leftover probe rows
```

### 12.9 The remaining ungated controllers

Follow-on from §12.8, across the rest of `Controllers/Core`. **Three patterns**, chosen per
controller from what actually calls it — a blanket sweep would have broken self-service for the 490
employee accounts.

**1. Controller-level, one link** (the only consumer is the screen being gated) — 15:
`AnnualLeaveLedger`, `TrainingCategory`, `TrainingCourse`, `TrainingSession`, `TrainingBudget`,
`TrainingProviderPayment`, `LearningPath`, `LearningCommunity`, `AwardCategory`,
`RecognitionProgram`, `RewardDisbursement`, `RecognitionWall`, `RewardPoints`(→`myPoints`),
`TrainingCpd`(→`myTraining`), `WorkflowDefinition`.

> `learningCommunity`, `recognitionWall`, `myPoints` and `myTraining` are **employee** links —
> `assign-employee-role.sql` grants them — so gating on them keeps self-service working.

**2. Controller-level, TWO links** (an HR screen and a self-service screen share the controller) — 4:

| Controller | Links | Why |
|---|---|---|
| `TrainingEnrollment` | `trainingSession`, `myTraining` | HR's participants modal **and** My Training |
| `TrainingCertificate` | `trainingCertificate`, `myTraining` | same |
| `Survey` | `survey`, `surveyTake` | employees respond via `/surveyTake` |
| `EmployeeTermination` | `terminationList`, `myExit` | employees see their own exit |

`HasAnyAsync` is an OR, so either link admits.

**3. Writes only** (the GETs are reference data consumed app-wide) — 6 files:
`Position` (12 screens use it as a dropdown), `OrganizationUnit` (12), `Lookup` (every form's
comboboxes), `Step`, `CompanyAsset` (also read by `/myExit`), `DynamicForm` (+`DynamicFormRecord`,
read by the profile tabs). Gating those GETs would 403 a dropdown for anyone lacking that screen.

#### Deliberately left open

| | Why |
|---|---|
| `Auth` | `[AllowAnonymous]` — sign-in |
| `Dashboard`, `Search` | every user's landing page; the palette is already permission-filtered internally |
| `Employee`, `EmployeeChild*` | scoped by `IPerformanceVisibilityService`; also serve `/myProfile` |
| `LeaveRequest`, `AnnualLeave`, `LeaveBalance` | employee self-service |
| `Guarantee`, `ProfileChangeRequest`, `ExitInterview`, `TerminationSettlement` | self-service |
| `Suggestion`, `Grievance`, `Announcement` | anonymous-safe self-service |
| `Workflow` | `/workflow` is My Approvals / My Submissions |
| `EmployeeMovement`, `DisciplinaryMeasure` | employee profile tabs |
| `RewardNomination`, `TrainingNeed` | employees nominate and raise needs |

These have their own guards or are self-service by design; a link gate would be wrong, not merely
redundant.

#### Verification

```
self-service (employee links)  TrainingEnrollment 200 | TrainingCertificate 200
                              TrainingCpd 400 (validation, so the GATE PASSED) 
                              LearningCommunity 200 | RecognitionWall 200
admin-only                    TrainingCategory | TrainingCourse | AwardCategory
                              WorkflowDefinition | AnnualLeaveLedger      all 403
reference GETs                Position | OrganizationUnit | Step          all 200
reference WRITES              Position | Step | OrganizationUnit | Lookup all 403
sidebar                       34 links, unchanged
```

### 12.10 TenantId dropped from User / Role / Operation — the SRMS model completed

Migration `DropTenantIdFromUserRoleOperation`. A user is a global identity, a role a global template
and an operation a global menu entry; tenancy lives entirely in `TenantUser` / `TenantRole` /
`TenantOperation`. The three tables now match SRMS **exactly**, with no extra columns.

#### ⚠️ The bug this caused, in BOTH apps

Login derived the session's tenant from `user.TenantId` and set the cookie every later request
resolves against. With the column unmapped it read as `""` — so there was no tenant, and **every
tenant-filtered query in the system returned nothing**: empty sidebar, zero employees, blank portal.
Login itself still returned 200, and neither log showed an error.

Both now resolve the tenant from **membership** (`TenantUser`, default first, then any active one),
which is also what makes multi-tenant users possible. Home additionally needs `IgnoreQueryFilters()`
there: `TenantUser` carries the same tenant filter as everything else, and at login there is no
tenant yet — that is precisely what the query is working out.

An account with no membership is now refused at sign-in with a plain message, rather than being
dropped into an application where nothing exists.

#### ⚠️ The migration had to carry the membership across FIRST

`TenantId` **is** the membership for these rows; once dropped there is no way to recover which tenant
a user belonged to. The seed had built `TenantUser` from `UserRole`, so it only covered users holding
a **role** — six accounts did not, and one (`dagmawi`) is a live headoffice user. The migration
backfills memberships for every user, and instances for every role, before dropping anything.
Result: 506 users → 506 memberships, 0 without.

#### What had to be scoped by hand

`Repository<T>` skips these three now (`IsGlobalEntity`), so anything listing them must scope itself:

| | Scope |
|---|---|
| `GetAllUsers` | through `TenantUser` — otherwise 506 rows instead of 500 |
| `GetAllRoles` | through `TenantRole` — otherwise 8 instead of 5 |
| `SaveRole` duplicate name/code | through `TenantRole`; `GetAll()` no longer scopes them |
| `SaveUserRole` user/role checks | existence no longer proves ownership — membership does |
| `SaveUser` | creates the `TenantUser` **membership**, or the account belongs nowhere |
| `SaveUser` uniqueness | now GLOBAL, matching the unique indexes the database actually has |

#### ⚠️ The projector no longer instantiates templates

With `Role` and `Operation` global, `templates` spans every tenant — instantiating them all would
hand this tenant every other tenant's roles. `SyncRolesAsync` and `SyncOperationsAsync` **update
existing instances only**. Creation moved to where it can be attributed: `SaveRole`,
`CreateOperationHandler` and `SeedDefaultMenu` each create their own tenant copy.

#### Verification

```
schema      0 TenantId columns left on the three | column sets now match SRMS exactly
backfill    506 users -> 506 memberships, 0 without | headoffice 500, demo 1, candbg 5
scoping     Users list 500 (not 506) | Roles list 5 (not 8)
live        HRMS 3 identities: login 200, 34 links, employee counts unchanged
            Home: login 200, 2 subsystems / 12 modules / 34 operations
            gate: RolePermission 403, OtherLeave 200, Position GET 200 / POST 403
integrity   0 dangling | 0 cross-tenant | 0 orphan screens | 15369 viewable pairs, unchanged
```

### 12.11 CompanyProfile consolidated into Organization

Migration `ConsolidateCompanyProfileIntoOrganization`. `Hrms.CompanyProfile` is gone;
`Core.Organization` owns the letterhead.

`Organization` was added as an additive layer in the SRMS phase-1 change and **had no reader at all**,
while the profile fed the company logo, the offer letter and the movement letters. The overlap was
always meant to end this way round — the profile's four fields are a strict subset of what
Organization carries.

| CompanyProfile | Organization |
|---|---|
| `CompanyName` | `LegalName` |
| `ContactAddress` | `Address` |
| `ContactPhone` | `PhoneNumber` |
| `ContactEmail` | `Email` |
| `LogoContent` / `LogoContentType` | `Logo` / `LogoContentType` |

#### ⚠️ Organization was invisible to the repository

It sits ABOVE the tenant — one organization may hold several — and the row that exists carries an
**empty `TenantId`**, so `Repository<T>`'s filter matched nothing and the whole table read as absent.
Adding it to `IsGlobalEntity` is what makes this work at all; without it the consolidation would have
swapped a table with no rows for a table nobody could see.

The practical effect is an improvement, not just a tidy-up: the profile had **zero rows**, so the
letterhead rendered empty. It now resolves the real data that was sitting unused in Organization —
`Cybersoft`, `Menelik II Avenue`, `cyber@cyber.com`, and a 13,905-byte PNG logo.

#### The wire contract is unchanged

`CompanyProfileDto` keeps its field names (`companyName`, `contactAddress`, …) and maps to
Organization's, so the company-profile screen and its service needed no change.

`Organization.SetLetterhead` exists for exactly the subset that screen posts. It leaves `LegalName`
alone when the posted name is blank — that field is REQUIRED here though it was optional on the
profile, so a blank one must not put the row into a state `Create` would have rejected.

#### ⚠️ The migration copies before dropping, even though there was nothing to copy

This database has **zero** profile rows, so the drop was free. Another environment may not be, and a
migration that only works against the database it was written on is not a migration. It fills only
the fields Organization has not already got, so a real organization record is never overwritten by a
thinner profile, and it creates an organization for any tenant that had a profile but none.

`OfferLetterTemplateConfiguration` shared the deleted configuration file and moved to its own.

#### Verification

```
schema      Hrms.CompanyProfile gone | Core.Organization 1 row
letterhead  GET OfferLetterTemplate/company -> 200
            {"companyName":"Cybersoft","contactAddress":"Menelik II Avenue",
             "contactEmail":"cyber@cyber.com","hasLogo":true}
logo        GET DocumentTemplate/logo/info -> {"hasLogo":true,"contentType":"image/png"}
            GET DocumentTemplate/logo      -> 200, 13905 bytes, image/png
live        login 200 | sidebar 34 links | 598 grants, baseline restored
```

### 12.12 SMTP settings: Core.Setting is now the source of truth

No migration — this is a code change plus one data correction.

#### What was actually wrong

`Core.Setting` has held `SmtpHost` / `SmtpPort` / `SmtpUser` / `SmtpUseTls` all along, and **nothing
in the application read or wrote them**. There was no handler, no controller and no screen; the row
that exists was seeded. `SmtpEmailService` went straight to the `Email` configuration section, so the
stored values were inert.

#### ⚠️ Why the settings are resolved IN-REQUEST

`EmailDispatchJob` says it plainly:

> the job itself touches NO tenant-scoped data … background jobs have no request, hence no Finbuckle
> tenant context

`Core.Setting` is tenant-scoped. Resolving it *inside* the job would have quietly returned nothing
and fallen back to configuration — the very bug being fixed, in the one place nobody would look. So
`QueuedEmailService` resolves the relay in-request, exactly as it already materialises the payload,
and passes it into the job.

#### ⚠️ The password never travels that path

**Hangfire persists job arguments.** A password in the job payload would be written to disk in clear
text and kept for the life of the job history. So `SmtpSettings` carries Host/Port/User/TLS only, and
the credential is read from configuration *inside* the send. That is also why `Core.Setting` has no
password column, and why `IEmailConfiguration` exposes `HasPassword` but never the value.

The old 4-argument `EmailDispatchJob.SendAsync` is **kept**: Hangfire resolves a job by method
signature, so removing it would strand every message already queued at deploy time.

#### ⚠️ Setting was invisible, exactly like Organization

Its single row carries an **empty `TenantId`**, so the repository's filter excluded it and the first
working build still returned the configured host. `Setting` had to join `IsGlobalEntity` — it holds
deployment-level operations (relay, backup schedule, password and session policy) as one row.

#### ⚠️ The seeded row would have redirected live mail

Once visible, the stored values won — and they were `smtp.cyber.com` / `noreply@cybererp.com`, seed
data nobody had verified, overriding a **working** relay. `scripts/clear-seeded-smtp-placeholders.sql`
blanks just those two fields (only where they still hold the seeded values), so configuration remains
the fallback until an administrator sets a relay deliberately. Fallback is **field by field**: a
tenant that sets only a host still inherits the configured port.

#### What was added to make this real

A settings API, because settings that cannot be edited are not settings: `GET`/`PUT /api/v1/Setting`
and `POST /api/v1/Setting/test-email`. The test endpoint reports which host and user were *actually*
resolved — the stored value and the configured fallback are easy to confuse — and refuses up front
when mail is disabled, no host is configured, or a user is set with no password, rather than letting
the send fail invisibly in a background job.

Gated on `setting`, and a "Settings" entry was added to `SeedDefaultMenu`'s System group. No role
holds that link yet, so the endpoints are unreachable until one is granted deliberately.

#### Verification

```
before   GET /Setting -> smtp.gmail.com   (config; stored value ignored)
after    GET /Setting -> smtp.cyber.com   (stored value now wins)
cleared  GET /Setting -> smtp.gmail.com   (config fallback restored)
                         autoBackup still true from the DB -> field-by-field confirmed

gate     /Setting 403 without the link | 200 with it
live     sidebar 34 links | 598 grants | 175 operations (the new Settings entry)
```

### 12.13 Aligning the remaining platform tables with SRMS

Migrations `AlignPlatformTablesWithSrms` and `AlignAssignedByAndSettingUpdatedAt`. A full
column-by-column diff of all **22 shared tables** found **13 differing**; these close all but two
root causes, taking it to **7 columns on 7 tables**.

#### Closed

| Table | Change |
|---|---|
| `Tenant` | +`OrganizationId` (FK to Core.Organization), `TenantTypeId`, `CurrencyOverride`, `LocaleOverride`, `TimezoneOverride` |
| `Subsystem` | +`Abbreviation`, `Icon`, `Description`, `DisplayOrder`, `IsActive`, `LandingPath`; `Name` 200→100 |
| `SubscriptionPlan` | +`Code` |
| `Organization` | all 19 length/nullability differences |
| `UserPreference` | 5 columns → NOT NULL and narrower (table is empty, so free) |
| `TenantRole` / `TenantOperation` | `Code`, `Name`, `Description`, `Link` widths |
| `TenantUserRole` | `AssignedBy` nvarchar(200) → uniqueidentifier |
| `Setting` | `UpdatedAt` → NOT NULL |

No truncation anywhere: the longest value in any narrowed column was 24 characters against caps of
80–500.

#### ⚠️ Three traps in the scaffolded migrations

1. **`Tenant.OrganizationId`** was added NOT NULL defaulting to an empty Guid, then given an FK to
   `Core.Organization` — which nothing satisfies. Backfilled from the single organization first; on a
   fresh database `Core.Tenant` is empty so it is a no-op, and if tenants somehow exist with no
   organization one is created rather than failing the migration.
2. **`AssignedBy` string → Guid.** SQL Server cannot cast `'seed-tenant-authorization'` to a
   uniqueidentifier, so the ALTER fails outright. The column only ever held provenance markers, never
   a user id, so anything not already a Guid is dropped to null via `TRY_CAST` rather than invented.
3. **`Setting.UpdatedAt` → NOT NULL** scaffolded a default of `0001-01-01`, which would stamp every
   existing row with a date that never happened. Seeded from `CreatedAt` instead.

#### ⚠️ What CANNOT be aligned, and why

Both remaining differences are **`BaseEntity` properties**, so they cannot be changed for a few
tables while the rest of the model shares them.

**`TenantId` — `uniqueidentifier` in SRMS, `nvarchar(max)` here (6 tables).** These are not the same
concept. In CERP `TenantId` is the **Finbuckle discriminator string**, declared once on `BaseEntity`,
carried by **202 tables**, and used by `Repository<T>.ApplyTenantFilter` on every query. SRMS uses the
name for a Guid foreign key to the tenant — which CERP models separately and deliberately as
`OwningTenantId` (§12.1). Matching would mean re-keying multi-tenancy across the whole application:
202 tables, the repository filter, the Finbuckle wiring and every seeded discriminator value. That is
a re-architecture, not a column alignment.

**`User.CreatedAt` — nullable in SRMS, NOT NULL here.** `BaseEntity.CreatedAt` is a non-nullable
`Instant` on all 202 tables. EF cannot make it optional for one entity while the CLR property is
non-nullable, and doing it globally would drop a guarantee every audited row currently has. SRMS being
looser here is not worth adopting.

#### Verification

```
diff       22 shared tables | 13 differing -> 7 columns, all BaseEntity-rooted
data       3 tenants | 506 users | 503 user-roles | 598 grants | 175 operations | 7 subsystems
live       HRMS login 200, 34 links, employee counts unchanged
           Home  login 200, 2 subsystems / 12 modules / 34 operations
           gate  RolePermission 403 | OtherLeave 200 | Position 200
           0 errors in either log
```

### 12.14 TenantId re-keyed to uniqueidentifier

Migrations `TenantIdToUniqueidentifier` and `MatchSrmsTenantIdExceptions`. **201 columns converted.**
The shared surface with SRMS is now identical but for **one** column.

#### ⚠️ A value converter, NOT a retyped property

The CLR property stays a `string`; a global converter in `OnModelCreating` maps it to `Guid`, so the
**column** is `uniqueidentifier` while **no entity, no repository filter and no handler changed**.

That was the whole insight. `TenantId` is declared once on `BaseEntity` and carried by 202 tables, and
it is also the Finbuckle discriminator — whose own `ITenantInfo.Id` is a **string**, so retyping to
`Guid` would have needed a conversion at that boundary anyway, on top of touching the repository
filter, ~20 aggregate-child propagations, Home's `IHasTenant` and every SQL script. **The column type
is what had to match; the CLR type never did.**

It is safe because every query use of `TenantId` is a **simple equality** — verified across both
repos: the repository filter, Home's nine query filters, `PortalNotifier`, `LoginRepository`. The
`string.IsNullOrEmpty(x.TenantId)` checks scattered through the aggregate handlers all run **in
memory**, so the converter never sees them.

`""` ↔ `Guid.Empty` round-trips. Nineteen rows legitimately carry a blank TenantId (the global lookup
tables, Organization, Setting) and `Guid.Parse` would throw on every one.

#### ⚠️ Four traps, three of which stopped the migration dead

1. **`Type.GetProperty("TenantId")` throws `Ambiguous match found`.** `TenantSubscription` declares
   its own `Guid TenantId` — a real FK — which *shadows* BaseEntity's string one. Ask EF what it
   MAPPED (`entityType.FindProperty`), not reflection; that column was already correct.
2. **EF scaffolded 400 `AlterColumn` calls and NO index handling**, but **141 indexes include
   TenantId** and SQL Server refuses to alter a column an index depends on. The migration is
   hand-written as discovery-driven SQL: capture index definitions, drop, convert, rebuild.
3. **A PRIMARY KEY also blocked it** — `PK_NumberSequence`. Key constraints are not `DROP INDEX`-able
   and were missed by the first attempt, which failed on exactly that. Captured and rebuilt as
   constraints. (Checked: no foreign key references them.)
4. **Blank values cannot implicitly convert.** Set to the empty GUID first.

Everything runs in **one transaction with `XACT_ABORT`** — and that earned its keep: the first attempt
failed on the primary key and rolled back cleanly, leaving all 201 columns untouched rather than the
schema half-converted.

#### ⚠️ SRMS is internally inconsistent, and we now match the inconsistency

SRMS types `TenantId` as `uniqueidentifier` on seven tables and **nvarchar on `LoginTrail` and
`UserPreference`**. The blanket conversion left CERP *more consistent than the thing it is supposed to
match*, so `MatchSrmsTenantIdExceptions` reverts those two. **The oddity is SRMS's.** If it is ever
tidied up there, delete that migration and the exclusion in `HrmsDbContext` rather than working around
them.

#### The last remaining difference

`User.CreatedAt` — nullable in SRMS, NOT NULL here. `BaseEntity.CreatedAt` is a non-nullable `Instant`
on 202 tables; EF cannot make it optional for one entity, and doing it globally would drop a guarantee
every audited row has. SRMS being looser is not worth adopting.

#### Verification

```
schema     202 TenantId columns -> uniqueidentifier, then 2 reverted to match SRMS
           141 indexes + 1 primary key rebuilt
diff       22 shared tables -> 1 remaining column (User.CreatedAt)
data       506 users | 490 employees | 598 grants | 175 operations | 60 login-trail rows
live       HRMS  3 identities: login 200, 34 links, employee counts unchanged
                 gate RolePermission 403 | OtherLeave 200 | Position 200
                 write path: throwaway operation created, TenantId stamped as a real Guid, deleted
           Home  login 200, 2 subsystems / 12 modules / 34 operations, notifications 200
           0 errors in either log
```

### 12.15 The last difference, fixed in SRMS rather than CERP

`User.CreatedAt` was nullable in `cybererp_srms` and NOT NULL in CERP — the one column left after
§12.14. Fixed **in SRMS**, because investigating showed the stricter side was right.

#### It was drift, not a design decision

SRMS's own model says the column is required, in two independent places:

- `BaseEntity.CreatedAt` is a non-nullable NodaTime `Instant`, assigned in the constructor
- `SrmsDbContextModelSnapshot` declares `b.Property<DateTime>("CreatedAt")` — non-nullable — and the
  initial migration created it `nullable: false`

**No migration in that project ever made it nullable**, and 20 of its 23 `CreatedAt` columns are
already NOT NULL. The database had drifted away from its own model outside of migrations. So CERP was
not diverging from SRMS; it was matching what SRMS intends.

#### ⚠️ Applied as a script, because SRMS's EF tooling is broken

`dotnet ef migrations add` fails in that project before reaching anything related to this:

```
The property 'OperationId' cannot be added to the type
'CyberErp.Srms.Dom.Entities.Core.TenantOperation (Dictionary<string, object>)' because no property
type was specified and there is no corresponding CLR property or field.
```

A pre-existing model error, unrelated, which blocks the tooling entirely. Hand-forging snapshot files
against a model that will not load would be worse than a script, so the fix is
`fix-user-createdat-notnull.sql` — guarded, re-runnable, and it **refuses rather than inventing
timestamps** if any row is NULL (a fabricated creation date is worse than a missing one, because
afterwards it is indistinguishable from a real one). It belongs in a migration once their model error
is resolved.

⚠️ **The SRMS tree is not a git repository**, so a copy lives at
`backend/scripts/srms-fix-user-createdat-notnull.sql` — otherwise the only record of this would be an
untracked file on one machine.

#### ⚠️ Two more tables carry the same drift, deliberately left

`Core.LookUpCategory` and `Core.LookUpCategoryList` are also nullable in SRMS. They were not the ask
and are not part of the CERP comparison, so they were left alone; the one-liners are noted at the
bottom of the script.

#### The shared surface is now identical — with one honest qualification

**Every column SRMS has, CERP now has with the same type and nullability.** Zero differences.

Diffing the OTHER direction shows **19 columns CERP has that SRMS does not**, and these are supersets
rather than mismatches:

| | Why |
|---|---|
| `TenantId` ×9 | CERP's `BaseEntity` adds it universally; SRMS carries it only where a table is genuinely tenant-scoped |
| `OwningTenantId` ×4 | CERP's separate Guid FK — SRMS overloads `TenantId` for that job (§12.1) |
| `CreatedBy` / `UpdatedBy` / `RowVersion` on `Setting` | `BaseEntity` audit columns |
| `Subsystem.Url`, `Subsystem.SortOrder` | CERP features — `Url` is what the Home launcher deep-links to |
| `TenantSubscriptionAddOn.SubscribedTenantId` | CERP's model |

Worth stating plainly because the earlier reports in this series diffed **one direction only**
(SRMS → CERP) and described the result as a single remaining difference. That was true of that
direction and is now zero, but it was never the whole picture.

### 12.16 Dropping the columns CERP has that SRMS does not

§12.15 established the shared surface matches in one direction, and that CERP carries **19 columns
SRMS does not**. Removing them is being done in stages, because they are not one kind of thing.

#### Stage 1 (done): OwningTenantId ×4 — the provably redundant ones

`TenantRole`, `TenantOperation`, `TenantUser`, `TenantSubSystem`.

They existed for a reason that expired. When those tables were created `TenantId` was an nvarchar
discriminator, so a real Guid FK to `Core.Tenant` needed somewhere else to live — hence a
deliberately differently-named column (§12.1). The re-key (§12.14) made `TenantId` a uniqueidentifier
holding the same value, and the two have been duplicates since: **zero mismatches across all 695
rows**. SRMS uses `TenantId` for this job.

⚠️ The FKs are added in **raw SQL**, not the EF model: EF cannot model a relationship on a
value-converted property. Three are added — exactly the three SRMS constrains (`TenantOperation`,
`TenantRole`, `TenantUser`); `TenantSubSystem` has none there and gets none here.

⚠️ Removing the column from the seed script's `TenantSubSystem` insert left the **SELECT list
misaligned** — it would have written the tenant id into `SubSystemId`. The nvarchar casts those
scripts used to join on `TenantId` are gone too, now that the column is a Guid.

#### Remaining, and why each is its own piece

| | Columns | Why not yet |
|---|---|---|
| `UserRole.TenantId` | 1 | ⚠️ **It carries information nothing else holds** — which tenant an assignment was made in. The projector derives every `TenantUser` membership from it, so going global leaves that derivation unable to tell one tenant's assignments from another's. Needs creation moved to the write site first, as Role and Operation got in §12.14. |
| `Subsystem` `TenantId` / `SortOrder` / `Url` | 3 | ⚠️ **HOME and HRMS are duplicated per tenant** (7 rows, 5 codes). Going global surfaces duplicates in the launcher, so it needs deduplication and repointing of `Module`, `Operation`, `TenantOperation` and `TenantSubSystem`. `SortOrder` also has real values (0–5) while `DisplayOrder` is all 0, so one must be migrated into the other. |
| `Setting` audit trio + `TenantId` | 4 | Needs an explicit `BaseEntity` exclusion for that entity. |
| The rest | 6 | `Organization`, `OrganizationSubscription`, `SubscriptionPlan`, `SubscriptionPlanModule`, `Tenant`, `TenantRolePermission` — all either 0 rows or already in `IsGlobalEntity`; mechanical. |

#### ⚠️ A process failure worth recording

The Stage 1 commit was reported as pushed when it **had not been committed at all**. The pre-commit
hook rejected it for not updating these docs, and the shell chain
`git commit … 2>&1 | tail -1 && git push` masked the failure — `tail` exits 0, so `&&` proceeded and
printed a success message. The migration was already applied, so the database was briefly ahead of
committed code.

**Check that the commit exists (`git log`), not that the command printed something.** Piping a
command through `tail`/`head` discards its exit status.

### 12.17 Removing the seven identity modules from HRMS

SRMS manages users, roles and the menu catalogue now, so HRMS stops offering its own screens for
them: **Users, Roles, User Roles, Role Permissions, SubSystems, Menu Modules, Menu Operations**.

#### What "remove the module" means here, and what it deliberately does not mean

Two facts set the scope. **SRMS's connection string points at `CERP` — the same database.** And HRMS
logs in against `Core.User`, renders its sidebar from `Core.Module`/`Operation`, and gates every
`[RequirePermission]` on `TenantRolePermission`.

So the tables stay and the **management surface** goes: the screens, the write endpoints, the CRUD
handlers, and the menu entries that led to them. Dropping the tables instead would break login here,
in Home, and in SRMS itself.

| Layer | Removed | Kept, and why |
|---|---|---|
| Frontend | 7 component modules, 7 pages, every `save`/`delete` service, `module/seedDefaults`, the route entries, and `common/menuFilters` (its only consumers were these screens) | `user/getAll`, `role/getAll` (approver pickers), `subsystem/getAll`, `module/{get,getAll,getAllWithOperation}`, `operation/{get,getAll,getAllByRole}` |
| API | Every create/update/delete action, `Module/seed-defaults`, and the `UserRole` and `RolePermission` controllers outright | `GET Module/WithOperations`, `GET Module`, `GET Subsystem`, `GET Operation`, plus one `GET` each on `User` and `Role` |
| App | `SaveRole`/`DeleteRole`, `SaveUserRole`/`DeleteUserRole`, `SaveUser`/`DeleteUser`, `SaveRolePermissions`/`DeleteRolePermission`, `SaveSubsystem`/`DeleteSubsystem`, the Module and Operation Create/Update/Delete slices, and `SeedDefaultMenu` | `GetAllRoles`, `GetAllUsers`, `GetAllSubsystems`, and the Module/Operation read handlers |
| Database | The 7 `Core.Operation` rows, their 7 `TenantOperation` copies and 28 `TenantRolePermission` grants (`backend/scripts/remove-identity-menu-operations.sql`) | Every table. `Core.User`, `Role`, `UserRole`, `SubSystem`, `Module`, `Operation` are untouched |

#### ⚠️ The reads are infrastructure — removing them would have been a security regression

`GET Operation` is the trap. `permissionGate.tsx` builds its catalog of gated routes from it, and
treats "not in the catalog" as "not a gated page". Take that read away and the catalog is empty, so
**every route falls through unguarded** — a bigger hole than the one being closed. `Module/
WithOperations` is the sidebar feed itself. Both stay open, and both only ever return menu metadata
already filtered to the caller.

#### ⚠️ Deleting a menu operation makes its permission key permanently ungrantable

`UserController` and `RoleController` were gated `[RequirePermission("user", "userRole")]` and
`[RequirePermission("role", …)]`. Those operations no longer exist, and
`EndpointPermissionService` resolves a required link by matching it against the caller's **granted
operation links** — so a key with no operation behind it can never be granted by anyone. The gates
would have returned **403 to every user, forever**, silently emptying the approver pickers on
workflow definitions, clearance departments and the report viewer.

The fix is to name the screens that actually consume the data:

```csharp
[RequirePermission("workflowDefinition", "clearanceDepartment", "reports")]
public class RoleController(IGetAllRoles getAllHandler) : BaseController
```

Verified against the data: all three links exist and carry `CanView` for Administrator and HR Admin,
whereas `user` and `role` now match zero rows anywhere.

**The general rule: before deleting a menu operation, grep for its link in `[RequirePermission]`.**
A gate outlives the screen it was named after, and it fails closed and silently.

#### One more consequence: permission changes are no longer instant

`SaveRolePermissions` and `DeleteRolePermission` called `IEndpointPermissionService.InvalidateAll()`,
so an admin's own save took effect immediately. Nothing in HRMS busts that cache any more, so a grant
made in SRMS appears here when the **60-second TTL** expires. That is what the TTL was always for
(§ the service's own comment: it bounds changes made behind the application's back).

### 12.18 The menu is data — removing the last places it wasn't

The question that started this was about the **Home portal**: why is the menu static? The sidebar
turned out to be fully dynamic already. Three other things were not.

#### 1. A compiled menu that wrote itself into the database

`SeedHomeMenu.cs` declared the portal's whole menu as a C# array — three groups, ~25 screens with
their links and icons — and `POST Portal/seed-defaults` inserted it into `Core.Operation`.

That is worse than a hardcoded menu, because it is a **second source of truth that writes to the
first**. A screen renamed, re-ordered, re-iconed or deleted in the database could be re-created by
the next seed run, and the menu could not be changed at all without a deployment. Deleted — along
with the endpoint, the DI registration, and the `Portal:SubsystemUrls` options binding whose only
job was to backfill `Core.Subsystem.Url` from `appsettings.json`.

HRMS's equivalent (`SeedDefaultMenu` + `Module/seed-defaults`) went in §12.17. Neither app writes the
navigation tables now; SRMS owns the catalogue.

#### 2. Launcher tiles that looked up their icon by name

Both SPAs resolved subsystem-tile icons through `getModuleIcon(subsystem.name)` — a PSMS-template
lookup table of `Purchases`, `Inventory`, `Container`, `Expense`… It matched almost nothing real, so
HOME, HRMS, PSMS and SRMS every one of them drew a neutral circle, and there was **no way to
configure the icon at all**.

`Core.Subsystem.Icon` already existed (HRMS owns the migration for it). What was missing:

| | Fix |
|---|---|
| Home's `Subsystem` entity had no `Icon` property | added — the table is `ExcludeFromMigrations` there, so it maps by convention |
| Neither `PortalSubsystemDto` nor HRMS's `SubsystemDto` exposed it | added to both |
| The tiles resolved by name | now `resolveNavIcon(row.icon)`, the same resolver the sidebar uses |
| **The column was NULL on every row** | `Home/backend/scripts/seed-subsystem-icons.sql` (idempotent — only fills blanks, so a hand-picked icon is never overwritten) |

That last row matters: without the seed, making the tiles data-driven would have turned *every* tile
into a circle. Wiring a column to the UI is only half the job when the column has never been written.

#### 3. ⚠️ `lucideIconMap` is where an icon degrades silently

`resolveNavIcon` falls back to `Circle` for any name it doesn't know, and nothing logs. Three real
lucide names — **`Inbox`** (the portal's main "My Requests" group), **`Bell`** and
**`MessageSquareQuote`** — were configured on live rows but absent from *both* apps' maps, so they
had always rendered blank.

**If a newly configured icon shows up as a circle, check the map before suspecting the data.** The
check is cheap: diff the icon names in the menu feed against the keys of `LUCIDE_ICONS`.

#### What was deleted, and why it read as "static"

Both SPAs still carried the PSMS template's menu layer, wired to nothing: `menu/icons/`,
`getModuleIcon`, `buildSidebarNavigation` (its result was computed in `useMenuModules` on every
render and **never consumed** — the sidebar builds its own groups), `menuTypes`,
`modules`/`moduleDetail`/`menuItem`, `quickAdd` with 18 hardcoded links, four unreachable sidebar
subcomponents, and Home's `constants/subSystem.ts` (`Administration, Purchases, Sales, Inventory,
Fianance, Report` — typo included).

None of it rendered. All of it made the codebase *read* as though the menu were hardcoded, which is
exactly the impression that prompted the question.

### 12.19 Following SRMS back to a real Module foreign key

SRMS was **changed by the user**: `Core.Operation.ModuleId` now genuinely constrains to
`Core.Module`, and a `Core.TenantModule` table exists. The 2026-08-13 self-referencing hierarchy
(§12.10 era) was built because SRMS looked self-referencing *then* — the entity comment even recorded
that reading. It is superseded.

#### Stage 1 (done): the foreign key, at zero data cost

The repoint needed **no data migration at all**, and that is not luck. The 2026-08-13 migration
copied the 24 modules into `Operation` **using their own Ids**, precisely so the existing children
would not need repointing. That invariant now pays off in the opposite direction: every child's
`ModuleId` was already a valid `Core.Module.Id`. Verified before applying — **144 of 144 present, 0
missing**, and all 24 group rows share an Id with a module.

⚠️ **Both constraint names are SRMS's, verbatim.** `FK_NavigationOperation_Module_ModuleId` is the
ModuleId one; `FK_Operation_Module_ModuleId` constrains **`SubSystemId`** — a misnomer left in SRMS by
a rename. Its `CASCADE` is SRMS's as well, so deleting a subsystem now deletes its menu, where CERP
deliberately used `Restrict`. Both were copied because the requirement is *identical structure*; both
are things to change in SRMS first if they should change at all.

The 24 group rows still exist with a null `ModuleId`, so nothing that reads the menu changed: login
200, sidebar 12 groups / 34 screens, `Operation` list 168.

#### What is still different, and why the rest is not mechanical

| Object | Delta | Note |
|---|---|---|
| `Core.Module` | −`TenantId`, `SortOrder`→`DisplayOrder`, +`Filter`, +`IsActive`, `Name`/`Icon` 400→200, `Icon` NOT NULL | safe: all 24 rows are one tenant, no name exceeds 200, **1 row has a blank Icon** and needs a value first |
| `Core.Operation` | drop the 24 group rows, `ModuleId` NOT NULL | the groups hold **zero grants**, so removing them costs no permissions — but the tenant-side reads must move to modules in the same change |
| `Core.TenantModule` | **does not exist in CERP** | a new tenant-scoped table the projector has to populate |
| `Core.TenantOperation` | −`OperationId`, `ModuleId` NOT NULL → `TenantModule` | ⚠️ see below |
| both | `UpdatedAt` datetime2(7)→(3), column order | order needs a table rebuild; cosmetic but part of "identical" |

#### Stage 2a (done): Core.Module, and a 409 that was not a conflict

`Core.Module` lost `TenantId`, renamed `SortOrder` to `DisplayOrder`, gained `Filter` and `IsActive`,
narrowed `Name`/`Icon` to `nvarchar(100)` with `Icon` NOT NULL, took SRMS's `SubSystemId` spelling
(via `HasColumnName`, so the C# property is untouched), and both tables moved `UpdatedAt` from
`datetime2(7)` to `(3)`.

**`Core.Module` and `Core.Operation` now diff to zero** against cybererp_srms on name, type, length
and nullability. Each narrowing was checked against the data first: 24 modules all in one tenant (so
none of the deduplication `Subsystem` will need), longest name 29 characters, and the one blank
`Icon` holding `''` rather than NULL — so NOT NULL applied without touching a row.

⚠️ **Dropping a TenantId breaks every read of that entity until it joins `IsGlobalEntity`.**
`GET Module` started returning **409 — "The LINQ expression … could not be translated"**, because
`Repository<T>` filters on `e.TenantId` and the member is now unmapped. The fix is one entry in the
skip-list; the trap is the symptom. A **409 on a plain GET** reads like an optimistic-concurrency
conflict, so the instinct is to look at `RowVersion`, which is nowhere near the problem. Any entity
whose `TenantId` gets `Ignore()`d needs that entry in the same commit.

#### Stage 2b (done): groups move to their own table, on both sides

`Core.TenantModule` now exists, and the menu group is a row there rather than a `TenantOperation`
with a null `ModuleId`. Both sets of group rows are gone, so **every row in `Operation` and
`TenantOperation` is a screen** — as in SRMS.

The data migration is inside the migration, and it leans on the same trick as Stage 1: each group
row **keeps its own Id** when it becomes a `TenantModule`, so nothing else needed re-keying. Only the
144 screens moved, from naming the template module to naming the tenant's group row.

```
1. group rows (ModuleId NULL)  -> Core.TenantModule, same Id, ModuleId = its OperationId
2. screens: ModuleId = TEMPLATE module id  ->  the tenant's TenantModule.Id
3. DELETE the group rows from TenantOperation
4. DELETE the 24 group rows from Core.Operation   (must follow 3 — FK OperationId)
5. THROW if any null ModuleId survives, before the NOT NULL lands
```

Step 5 matters more than it looks: without it a surviving orphan would be silently converted to an
empty-Guid FK by the `NOT NULL` alter, and the failure would surface much later as a missing menu.

Verified after applying: TenantModule 24, TenantOperation 144, Operation 144, **0** bad `ModuleId` in
either table, and **570 grants intact with 0 orphaned**.

**The projector had to learn a translation.** `SyncModulesAsync` runs *before* operations (a screen
cannot be projected before its group exists) and, unlike roles and operations, it **creates** rows —
it can, because the set it needs is derived, not guessed: a tenant that holds a screen must hold that
screen's group. `SyncOperationsAsync` then maps the template's `ModuleId` through
`TenantModule.ModuleId → TenantModule.Id` rather than copying it straight across, which is the whole
point: template ids and tenant ids are different namespaces now.

⚠️ **Home reads these tables directly, so the two repos deploy together.** It got the `TenantModule`
entity, the DbContext mapping, and a rewritten join in `GetMySubsystems`. Both feeds still report
TEMPLATE ids on the wire, so neither SPA needed a change — HRMS still renders 12 groups / 34 screens
and the portal still renders HOME(21) / HRMS(13).

#### What still differs from SRMS, and why each is deliberate

| Difference | Why |
|---|---|
| `TenantOperation.OperationId` | the template link. Your call (2026-08-15) to keep it: both apps use it as the stable UI id and as the join between `permissionGate`'s global catalog and tenant grants |
| `TenantModule.ModuleId` | the same link for groups — added for consistency, and the projector needs it to know which template a copy came from |
| `Operation.ModuleId` NOT NULL vs nullable | CERP is the **stricter** side and can hold anything SRMS can. Matching exactly would force the CLR property to `Guid?`, because EF refuses to map a nullable column onto a non-nullable Guid, reintroducing null handling for no gain |
| column ORDER | cosmetic; needs a full table rebuild |

#### Stage 2c (done): TenantOperation, and a correction

SRMS was restructured again between stages: it **normalised** `TenantId`, `SubSystemId` and the
template link `OperationId` off `TenantOperation`. A screen's tenant and subsystem are its module's.
All three are dropped here and the table now **diffs to zero**.

⚠️ **A tenant table with no tenant column.** `TenantOperation` is listed in
`Repository.IsGlobalEntity` purely because the filter would otherwise reference an unmapped member —
it is NOT global data. `GetAll()` returns every tenant's rows, and correctness now depends on the
caller:

| Caller | How it stays scoped |
|---|---|
| sidebar feed | joins this tenant's `TenantModule` ids (changed) |
| projector | same, and its orphan sweep would otherwise delete other tenants' screens (changed) |
| `EndpointPermissionService` | joins FROM `TenantRolePermission` by primary key — already correct |
| `WorkflowApproverAuth` | same shape — already correct |
| Home's portal feed | joins `TenantModule`, which keeps its filter (changed) |

⚠️ **The projector re-keys on (module, link).** `OperationId` was how a copy knew its template;
without it the natural key does the job — verified 0 duplicate `(ModuleId, Link)` pairs across the
144 rows, and link is what every permission check already matches on.

#### Stage 2d (done): zero differences, and what the earlier check missed

⚠️ **The Stage 2a "diff to zero" was measured on the wrong set.** It compared column name, type,
length and nullability — but not DEFAULT CONSTRAINTS. Adding them surfaced nine more differences, one
of them substantive: SRMS had also dropped `Core.Operation.SubSystemId`, the same normalisation it
applied to the tenant tables.

| Change | Note |
|---|---|
| `Operation.SubSystemId` dropped | its FK went too — the misnamed `FK_Operation_Module_ModuleId`, which constrained that column and cascaded. Both problems solved by the column not existing. The three readers take the subsystem from `Module` |
| `Operation.ModuleId` → nullable | the CLR property is `Guid?` now. I backed out of this once because EF refuses to map a nullable column to a non-nullable Guid; with SubSystemId gone the surface was small enough to just do it |
| `TenantModule.ModuleId` dropped | the last template link, now that `OperationId` has gone. The projector keys groups on **(SubSystemId, Name)** — verified unique, all 24 rows resolve |
| 6 stray defaults removed | EF added them via `HasDefaultValue`; SRMS has none |
| `Operation.IsActive` default respelled | `CONVERT([bit],(1))` → `((1))`. Same value, different catalog text |

⚠️ **EF will not drop a default constraint it never declared**, so the leftovers need name-agnostic
raw SQL — the same lesson as the raw-SQL FK that blocked Stage 2c.

**Result: zero differences** across `Module`, `Operation`, `TenantModule` and `TenantOperation` on
name, type, size, nullability and default. Only ordinal ORDER still differs, which needs a rebuild.

#### ⚠️ Mapped read-models drift silently

Home's build stayed green while two of its entities mapped columns that no longer existed
(`Operation.SubSystemId`, `TenantModule.ModuleId`). Nothing catches that at compile time — the portal
feed simply returned **500 "Invalid column name"** at runtime. When this repo drops a column, grep the
Home entities and RUN the feed; a green build proves nothing.

#### ⚠️ A correction to what §12.19 said about `OperationId`

When this column came up as a "blocker" I gave two reasons to keep it. One was **wrong**: I said it
was the join between `permissionGate`'s global catalog and the tenant-row grants. It is not.
`permissionGate` builds both `catalogSet` and `grantedSet` from **links**, and so do
`formPermissions`, `gridAction` and `useListPermissions`. The id travels into
`UserPermissionModel` and is never matched on — it is a React key.

The only real dependency was the projector, and re-keying it on link took a dozen lines. The column
was therefore much cheaper to drop than the earlier note claimed, and the decision to keep it was
made on partly false information.

#### ⚠️ The original blocker: `TenantOperation.OperationId`

SRMS's tenant copies are **standalone** — verified: **0 of 220** `TenantOperation` rows share an Id
with any `Operation`, and there is no template column. A tenant's menu, once copied, has no link back.

CERP's copies do carry `OperationId`, and both applications depend on it:

- the sidebar and portal feeds report **the template id** as each item's id, because that is the
  stable identifier the UI and the role-permission screen work against;
- `permissionGate.tsx` builds its catalog from the **global** `GET Operation` while grants live on
  **tenant** rows — `OperationId` is the join between the two.

Dropping it means re-establishing that link some other way (matching on `Link`, or making the tenant
copy's Id equal the template's — which cannot survive a second tenant). That is an architectural
change to the permission layer of two applications, not a schema tweak, so it is a decision rather
than a mechanical step.

### 12.20 The database-wide schema audit

All 30 tables SRMS has exist in CERP. Comparing **every column** on name, type, size, nullability and
default found **65 differences**; `TenantRole` and the timestamp work took it to **50**.

`TenantRole` was a plain rename: `SourceTemplateId` is SRMS's `RoleId`. Mapped with
`HasColumnName` rather than renamed in C#, because "RoleId" on a table whose rows *are* roles reads
like the primary key.

#### ⚠️ The blanket timestamp fix that had to be rolled back

The convention gave every **nullable** timestamp `datetime2(7)` and every non-nullable one `(3)` —
an accident of nullability rather than a design, and the cause of 14 differences. Changing it to
`(3)` everywhere looked like the one-line root-cause fix.

It was not. **It fixed 16 columns and broke 13**, because SRMS keeps `UpdatedAt` at `(7)` on Person,
SalaryScale, Step, SubscriptionPlan, Tenant, TenantSubscription and UserPreference — a net gain of
three across a **594-column migration**. Rolled back and replaced with an explicit list of the 17
entities SRMS actually has at `(3)`.

**The lesson: verify the shape of the target before generalising about it.** SRMS is internally
inconsistent here; the ugly list mirrors its migration history because there is no rule to mirror.

#### The remaining 50, and why they are not a to-do list

| Group | Count | Verdict |
|---|---|---|
| ~~Default constraints~~ | ~~28~~ | ✅ **DONE** — see below |
| `TenantId` **absent** in SRMS (10 tables incl. UserRole, TenantRolePermission, TenantUserRole, Setting, Subsystem) | 10 | ⚠️ **Load-bearing.** Each removes tenant isolation from a table the runtime filters on. `TenantOperation` took a full stage to do safely — every one of these needs the same treatment |
| `TenantId` **typed nvarchar** in SRMS (FiscalYear, Notification, Person, SalaryScale, Step) | 5 | ⚠️ **Would go backwards.** CERP re-keyed these to `uniqueidentifier` in §12.14. SRMS simply has not caught up |
| `Subsystem.Url` + `SortOrder` absent in SRMS | 2 | ⚠️ **Would break the Home launcher**, which deep-links through `Url` — built this session |
| `Setting` audit columns + `TenantSubscriptionAddOn.SubscribedTenantId` | 5 | The known BaseEntity superset |
| `User.CreatedAt` nullable in SRMS | 1 | ⚠️ **SRMS regressed.** I made it NOT NULL on 2026-08-14 (§12.15). Fix there, not here |

So "identical" is not simply achievable in one direction any more: **CERP is ahead of SRMS on some
columns and behind on others**, and three of the differences exist because CERP has features SRMS
does not.

#### The defaults (done) — and three EF could not express

28 constraints, in four kinds: 8 CERP-only ones removed, 9 of SRMS's added, 6 changed to SRMS's real
values (`'en'`, `'dd/MM/yyyy'`, `'/'`, `'1,234.56'`, `'system'`, `'Africa/Nairobi'`), 5 respelled.
That took the total from **50 to 23**.

⚠️ **Three had to be hand-written SQL**, each for a different reason, and each worth recognising
again:

| Column | Why EF could not do it |
|---|---|
| `Role.Code` | carried an `(N'')` default from an **older migration the model never declared**. EF neither knows about it nor drops it — the same class of problem as the raw-SQL FK in §12.19 |
| `Subsystem.Code` | pure spelling: EF always emits `N''` for a string default, SRMS stores `''` |
| `TenantRolePermission.CanExport` | the opposite — `HasDefaultValue(false)` produces **nothing**, because `false` is the CLR default and EF optimises the constraint away |

A hand-authored migration also needs an explicit `[Migration("id")]` attribute, or EF does not
discover it at all.

**These defaults are decorative.** EF supplies every value on insert, so they only ever apply to raw
SQL. They were aligned because the requirement is an identical catalog, not because behaviour
depended on them — worth knowing before spending effort defending them.

#### `Core.Tenant` and the safe platform drops (done) — 23 → 18

`Core.Tenant`'s only difference was a `TenantId` column, and it was always meaningless: **the row IS
the tenant.** `Core.Tenant` has been in `IsGlobalEntity` since the beginning, so nothing ever stamped
or filtered it, and all three rows held the empty Guid. Gone.

Three more went with it — `SubscriptionPlan`, `SubscriptionPlanModule` and `OrganizationSubscription`
are **platform data, not tenant data** (a plan belongs to the product, not to a customer) and all
three are **empty**, so there was nothing to lose. ⚠️ The latter two had to join `IsGlobalEntity` in
the *same* change, or every read of them would have failed with the 409 "could not be translated"
that cost a debugging round on `Module`.

#### ⚠️ The audit had a hole: columns are not a schema

Everything above compared **columns** — name, type, size, nullability, default. It never compared
**foreign keys**. So `FK_Tenant_LookUpCategoryList`, which SRMS has and CERP did not, was invisible
to every "remaining differences" count reported in §12.20 up to that point.

`Core.Tenant.TenantTypeId` now references `Core.LookUpCategoryList` under SRMS's constraint name,
`Restrict` on delete. All three rows hold NULL and NULLs are exempt, so it applied without touching
data. Running the comparison properly: **foreign keys diff to zero across all 30 shared tables, in
both directions.** Indexes are still uncompared — the same hole, one level down.

#### ⚠️⚠️ RETRACTION: the "foreign keys diff to zero" claim was false

§12.20 twice reported **0 foreign-key differences**. Both were wrong, and for the same reason as the
index harness — one level more subtle.

The query concatenated `fk.name` with `fk.delete_referential_action_desc`. Those two columns carry
**different collations** (`Latin1_General_CI_AS_KS_WS` and `SQL_Latin1_General_CP1_CI_AS`), so SQL
Server refused the `+` with *"Cannot resolve collation conflict"*. The query returned nothing but an
error, `Where-Object { $_ -match '~' }` filtered the error text away, and two empty dictionaries
compared as identical.

**That is the second false zero this session from the same shape.** A comparison that returns nothing
is indistinguishable from one that passes, whether the cause is an empty result or an error. Fixed
with `COLLATE DATABASE_DEFAULT` on every concatenated system column, and **every comparison now
prints and asserts its load count before reporting a difference count.**

The real number was **13**, now **9**:

| Fixed | What |
|---|---|
| `TenantRolePermission` FK name | SRMS calls it `FK_TenantRolePermission_Operation_OperationId` — naming a column, `OperationId`, that exists on neither side. A leftover from when the table referenced `Core.Operation` directly. Renamed to match |
| `TenantOperation` FK name | likewise `FK_TenantNavigationOperation_TenantModule_ModuleId` |
| `LoginTrail.UserId → User` | added, SET NULL — a deleted account leaves its audit trail behind with the link cleared. 84 rows, 0 orphans |
| `TenantModule.TenantId → Tenant` | added in **raw SQL**: EF cannot model a relationship on the value-converted `TenantId` (§12.14), the same constraint that forced raw SQL in §12.16 |

#### The 9 foreign-key differences that remain, and why

| Difference | Verdict |
|---|---|
| `SalaryScale.JobGradeId → JobGrade`, `User.EmployeeId → Employee` | **CERP-only, keep.** SRMS has no JobGrade or Employee table — these are HRMS domain integrity |
| `TenantSubSystem.SubSystemId`, `TenantSubscriptionAddOn.ModuleId` / `.SubscribedTenantId` | **CERP-only, keep.** Real integrity on tables SRMS models more thinly |
| `SubscriptionPlanModule.ModuleId` → `Module` (CERP) vs → `SubSystem` (SRMS) | ⚠️ **Do not match.** SRMS points a column named ModuleId at **SubSystem** — an artifact of its SubSystem-was-once-Module rename. CERP's target is the correct one |
| `OrganizationSubscription.OrganizationId`, `UserRole.RoleId`: CASCADE here, NO_ACTION there | ⚠️ **Behavioural, not cosmetic.** Changing them alters what a delete does. Needs a decision, not a sweep |

#### ⚠️ The index comparison, and a harness that lied

The first index comparison reported **0 differences**. It was wrong. The query built its column list
with `FOR XML PATH`, returned nothing, and the `Where-Object { $_ -match '~' }` filter quietly
swallowed the empty result — leaving two empty dictionaries, which compare as identical.

**A comparison that returns nothing looks exactly like a comparison that passes.** The fix is one
line: print the row counts and assert they are non-zero before trusting a zero. `Core.Tenant` alone
disproved it — CERP had three indexes there, SRMS had one different one.

The real answer was **69 differences**, in three groups:

| Group | Count | Action |
|---|---|---|
| Primary-key NAMES | 6 | renamed to SRMS's: `PK_NavigationModule`, `PK_StandardRoleTemplate`, `PK_SystemSetting`, `PK_TenantNavigationModule`, `PK_TenantModuleEntitlement`, and `PK_Module` for **`Core.Subsystem`** (a leftover from when SRMS's SubSystem entity was called Module) |
| Alternate keys SRMS declares | 4 | added — each is trivially unique, leading with the primary key column |
| CERP-only indexes | 53 | **KEPT** — performance and uniqueness indexes (`IX_User_UserName` from the performance pass, the notification indexes, unique business keys like `IX_Tenant_Identifier`). Dropping them regresses performance and integrity |

⚠️ **Renaming a primary key needs `sp_rename`, not EF.** EF scaffolds a rename as drop-then-add, and
SQL Server refuses to drop a key that foreign keys reference — *"The constraint 'PK_TenantModule' is
being referenced by table 'TenantOperation'"*. `sp_rename` changes the name in place and leaves every
dependant intact, which is what a rename actually means.

⚠️ **And the order matters:** `Core.Module`'s PK had to be renamed away before `Core.Subsystem` could
take the freed `PK_Module` name.

**Result: every index and key SRMS has now exists in CERP with an identical definition** — 0 missing,
0 mismatched. Columns, foreign keys, indexes and keys have all now been compared.

#### ⚠️ CERP has TWO lookup systems, and EF picked the wrong one

Mapping that foreign key through EF scaffolded a constraint to **`Hrms.LookUpCategoryList`** — the
wrong table — and it would have applied silently.

| Table | What it is | Referenced by |
|---|---|---|
| `Core.LookUpCategory` / `Core.LookUpCategoryList` | mirrors the **SRMS platform** schema | `Tenant.TenantTypeId` (now) |
| `Hrms.LookUpCategory` / `Hrms.LookUpCategoryList` | the **HRMS domain** lookups — education level, field of study | the `LookupCategoryList` **entity** maps this one |

Both exist, both are currently empty, and before this change **neither was referenced by any foreign
key at all**. A tenant TYPE is platform data, so the constraint belongs on the Core pair — which EF
cannot express while the entity maps the Hrms pair, hence raw SQL.

**The general point: when two tables share a name across schemas, read the scaffolded migration
before applying it.** EF resolves the entity, not the intent.

#### ⚠️ Two "extras" that are not extras

Not everything CERP has and SRMS lacks is surplus. Two were deliberately left:

- **`TenantSubscriptionAddOn.SubscribedTenantId`** — a real foreign key to `Core.Tenant`, recording
  which tenant holds the add-on. The table is empty, so dropping it was *safe*; it would still have
  been wrong. SRMS lacking it is a gap there.
- **`Organization.TenantId`** — unlike `Tenant`'s, its single row holds a real value. Dropping it
  discards data, which is a decision rather than a cleanup.

The distinction that matters: a column is only surplus when it is both absent upstream **and**
carrying nothing here.

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

### 12.21 Core.Subsystem: dropping the last two CERP-only columns

The table still carried `SortOrder` and `Url`, plus a unique `IX_Subsystem_Name`. SRMS has none of
the three, so all three went. `Core.Subsystem` is now column-for-column and index-for-index
identical to SRMS.

**`SortOrder` was a duplicate.** SRMS's ordering column is `DisplayOrder`, which CERP also had —
two columns doing one job. When the pair was first noticed (§12.16) `DisplayOrder` was all zeros
while `SortOrder` held 0–5, so dropping the wrong one would have scrambled the launcher. That is no
longer true: `DisplayOrder` is now curated (1,1,2,3,3,5,7), so the drop needed no data migration.
Everything that ordered by `SortOrder` now orders by `DisplayOrder`.

**`Url` was in the wrong place entirely, and that is the point.** It held each subsystem
application's address — `http://localhost:5174` and friends. A database column cannot express it,
because these rows are SHARED across every environment while the address DIFFERS by environment:
dev, staging and production each need their own value, and one shared row has only one. So the four
addresses moved into environment configuration, `VITE_SUBSYSTEM_APPS`, a JSON `{code: appUrl}` map
keyed by `Core.Subsystem.Code` and matched case-insensitively. Both SPAs read the same variable
name; the Home portal already had the identical pattern one function above it for subsystem *APIs*
(`VITE_SUBSYSTEM_APIS`) — this is its sibling for subsystem *apps*: one is where a subsystem's
server is, the other where its SPA is.

The wire contract did not change. `getMySubsystems` stitches `url` back onto each subsystem from
the registry as the response is mapped, so the launcher tiles, sidebar hrefs and
`openExternalSubsystem` all still see the shape they always saw and needed no edits. **That is the
single place the swap happens** — resolve addresses through `appUrlFor(code)`, never expect a
server-supplied `url`.

#### The index that was deliberate, and is now gone

`IX_Subsystem_Name` (unique) was kept on purpose at §12.16, because losing it lets a second row
claim a name the launcher matches on — the duplicate-`HOME` bug that needed
`dedup-subsystem-rows.sql`. Complete parity means it goes anyway. The exposure is smaller than it
was: that duplicate came from PER-TENANT subsystem rows, and `TenantId` is gone (§12.16); HRMS's
Subsystem module is read-only since §12.17, so SRMS owns writes here. A duplicate can now only
arrive by manual insert. **Nothing in the database prevents one.**

#### ⚠️ `--no-build` makes `dotnet ef` lie about pending changes

`database update --no-build` failed with *"The model for context 'HrmsDbContext' has pending
changes. Add a new migration"* — immediately after adding exactly that migration. Scaffolding a
probe migration then re-emitted the SAME three operations, which looks like the known
tools-9.0.5-vs-runtime-10.0.8 snapshot drift and invites the documented DriftProbe workaround.

It was neither. `--no-build` means the compiled assembly still holds the OLD snapshot: `migrations
add` writes the `.cs` files, but nothing recompiles them, so `database update` compares the live
model against a stale compiled snapshot and reports phantom drift — and the probe, comparing
against the same stale snapshot, reproduces the same diff and "confirms" the false conclusion.

The tell that separates the two: `migrations remove` named the PREVIOUS migration as the last one,
proving EF could not see the new files at all. **Drop `--no-build` and rebuild before `database
update`.** Reach for the DriftProbe workaround only after a real build has ruled this out —
otherwise a probe migration gets committed to fix a problem that does not exist.

### 12.22 Edit Profile in the Home portal, ported from SRMS

The portal's "Change Password" menu item and its `/password` page are gone, replaced by SRMS's
`UserAccountDialog` — a three-tab self-service dialog (Profile / Preferences / Security) opened from
the header account menu. **Password change was not removed**: it moved into the Security tab, which
is where SRMS has it, and still posts to the portal's existing `auth/change-password`.

#### What was deliberately NOT ported

SRMS's dialog is three components wearing one name. Besides "my profile" it is also the **create a
user** form and the **edit someone else's account** form (`createMode`, `targetUser`,
`platformMode`) — which is why its endpoints are `/User/{id}/profile`, `/User/{id}/preferences`, and
why it carries a role picker, an employee combobox and account-status switches.

The portal has no user administration; SRMS owns that. Porting the id-addressed shape would have put
**every signed-in user one URL edit away from reading and rewriting anyone else's profile**, behind
UI that would itself be dead. So every portal endpoint is a `me` route taking the account from the
auth cookie, and `AccountController` takes no user id at all. If admin editing is ever wanted here,
it needs its own permission-gated slice — not a widened parameter.

Everything belonging to the self-service path was kept: avatar cropping to a 512×512 JPEG, picture
changes STAGED until Save, the dirty-state discard guard, live theme switching, and the security
activity feed.

#### No migration — and two runtime-only traps

Every column already existed on `Core.User` (`ProfilePicture`, `ProfilePictureContentType`,
`AccountStatus`, `TwoFactorEnabled`, `FailedLoginAttempts`, `LockoutEndUtc`), plus all nine of
`Core.UserPreference` and the whole of `Core.LoginTrail`. The portal had simply never mapped them.
Both traps below build perfectly cleanly and fail only when the row is actually written:

1. **`UserPreference.TenantId` is `nvarchar(900)`, not `uniqueidentifier`.** It was never part of
   the §12.14 re-key. `HomeDbContext` ends with a blanket loop converting EVERY string `TenantId` to
   `uniqueidentifier`; left alone it sends a Guid parameter at an nvarchar column. The entity is
   explicitly skipped there. **Check the COLUMN before trusting "every TenantId is a Guid now".**
2. **`UserPreference.RowVersion` is `varbinary(8) NOT NULL`** and manually managed (not a SQL
   rowversion the server fills in). The first insert failed with *"Cannot insert the value NULL into
   column 'RowVersion'"*. `SharedAudited` exists for exactly this, so the entity extends it. **Any
   shared table the portal INSERTS into needs that base class; read-only ones (LoginTrail) do not.**

#### Smaller decisions worth keeping

- **The avatar is a STREAM, not JSON.** `GET Account/profile-picture` returns the image with
  `Cache-Control: private, max-age=300`; the profile response carries only a URL. Inlining base64
  would put the whole picture in every profile read. The SPA appends `?v=<timestamp>` after an
  upload so only the changed avatar is refetched.
- **The upload uses raw `fetch`.** The shared client sets `Content-Type: application/json` whenever
  there is a body, which overrides the multipart boundary the browser must generate.
- **`AuthContext` gained `syncUser`** so the header reflects a renamed account immediately — the
  shell reads the name and email from context, not from a re-fetch.
- **"Last successful login" is the newest SUCCESS**, not the newest row: a failed attempt after a
  good one must not be reported as the last sign-in.

### 12.23 Why Edit Profile could not save, and could not be cancelled

Three independent defects, each of which alone looked like "the feature is broken". Reproduced in
a real browser (Chrome over CDP) before and after fixing.

#### 1. Email was required, but almost nobody has one

`UpdateMyProfile` demanded an email, on the client AND the server. **489 of the 506 accounts in
this database have none** — and that is deliberate, not dirty data: `IX_User_NormalizedEmail` is
UNIQUE but **FILTERED to `NormalizedEmail <> ''`**, so the schema explicitly exempts blanks.
Those users could not save anything, not even a phone number, without inventing an address.

Even had the client allowed it, the server would still have refused: the uniqueness check compared
empty strings, so all 489 blanks collided with one another. Email is now optional, its format is
checked only when supplied, and the uniqueness check skips blanks.

**The lesson: read what the INDEX says before deciding a field is mandatory.** A filtered unique
index is a statement about optionality, and it disagreed with the code.

The same handler never maintained `NormalizedUserName` / `NormalizedEmail`, which are what those
unique indexes actually enforce. A rename left them stale — the row's identity and its uniqueness
guarantee drifting apart. Both are now written alongside the values they mirror.

#### 2. ⚠️ The top layer: why Cancel did nothing, forever

`components/ui/modal.tsx` opens with **`<dialog>.showModal()`**, which places it in the browser's
**top layer** and makes everything outside it **inert**. `DialogModal` — and therefore `confirm()` —
was a plain `position: fixed; z-index: 100` portal into `document.body`.

Top-layer content paints above ALL normal content regardless of z-index, and inertness means
clicks never arrive. So the discard prompt rendered *behind* the modal's own backdrop and could not
be clicked: `await confirm(...)` never settled, Cancel hung, and the only escape was a page reload.

`DialogModal` is now a native `<dialog>` too — dialogs stack in open order, so a confirm raised
from inside a modal sits above it. **A z-index, however large, cannot beat the top layer.**

#### 3. The same trap made every failure silent

`Toaster` portals into the normal layer as well, so *every* toast raised while a modal was open —
including the save error naming the exact problem — was hidden behind the backdrop. That is why
this presented as "nothing happens" rather than as an error.

The toast stack is now a `popover="manual"` element: it reaches the top layer **without** making
the page inert or taking focus, which is what a toast requires and what a second `<dialog>` could
not provide. The dialog additionally renders the failure INLINE in its footer, so feedback never
depends on the toast layer alone.

#### Smaller things found while verifying

- **Cancel is never disabled.** It was `disabled={busy}`, so a hung or slow save also locked the
  only way out. The discard guard now closes rather than stranding the user if it ever fails.
- **Auto-close on success is guarded**, so the dialog's own `close` event cannot re-open the
  discard prompt on the way out.
- **`InputField` hardcoded `layout="horizontal"`** into FieldShell, which pins the label BESIDE the
  control for non-full-width fields. A form mixing widths therefore had labels above on some rows
  and beside on others. It now forwards `layout` as `SelectField` already did.
- **`sqlcmd` needs `SET QUOTED_IDENTIFIER ON`** to update `Core.User` at all — the filtered index
  requires it. EF sets it by default, so this bites only hand-run SQL.

#### The browser harness

Diagnosing this by inspection failed twice; the answer came from driving a real browser. Node 22
has a global `WebSocket`, so Chrome can be driven over the DevTools Protocol with **no npm
packages** — launch with `--remote-debugging-port`, connect to the page target, then
`Runtime.evaluate` / `Input.dispatchMouseEvent` / `Page.captureScreenshot`. Screenshots come back
as base64 PNGs that can be read directly. Worth rebuilding whenever a UI bug resists inspection —
note that `element.click()` does not open the header dropdowns; a real dispatched mouse event does.

### 12.24 The header avatar, and the blob that rode along with it

The account button top-right shows the profile picture when the account has one, initials when it
does not, and changes the moment a picture is saved or removed.

**The shell has to KNOW whether a picture exists.** It cannot be inferred by rendering an `<img>`
and waiting for it to fail — that shows a broken image or an empty circle first, on every load, for
the ~99% of accounts with no picture. So the flag rides on the session: both the sign-in payload
and **`Auth/me`, the session probe**, return `profilePictureUrl` (the relative route, or null).
Putting it only on sign-in is not enough; without it on the probe, a reload flashes initials before
swapping to the image.

`AuthContext` turns that into an absolute, cache-busted URL in **one** place, so every consumer can
simply render it or not. **The `?v=` is load-bearing:** the picture endpoint answers
`private, max-age=300`, so a freshly saved image would keep serving the old one for five minutes
without a new version each time.

Instant update comes from the dialog pushing the new URL — or `null` on removal — into the session
through `syncUser` as part of the save. It is sent **only when the picture was actually touched**
(`undefined` means "leave it alone"), so an unrelated profile save cannot clobber it.

The `<img onError>` fallback to initials is a real path, not decoration: the URL is minted from a
session flag, so it goes stale if the picture is removed in another tab, and a broken image icon in
the header is worse than initials.

#### ⚠️ Mapping a varbinary(max) column has a cost everywhere it is materialised

`Core.User.ProfilePicture` is `varbinary(max)`. Once §12.22 mapped it onto the entity, **every**
`users.GetAll()` that materialised a `User` started dragging the image with it — including **login**,
which loaded each candidate's avatar purely to compare a password hash. Nothing failed; it just got
quietly more expensive on the hottest path in the system.

Login, `GetMyProfile` and `GetMyAccountProfile` now project the columns they actually use, reducing
the blob to `HasProfilePicture = u.ProfilePicture != null && u.ProfilePicture.Length > 0`.

**Rule: do not materialise `Core.User` — project it.** The same applies to any entity that gains a
blob: mapping it is not free, and the cost lands on readers that never asked for it.

The avatar's route is now the shared constant `AccountRoutes.ProfilePicture`, so the sign-in
payload, the profile reads and the upload response cannot drift apart — the SPA keys "has a
picture" off exactly that value.

### 12.25 Preferences that are read, not just written

Preferences had been saveable for some time, but nothing read them back. Theme came from
`localStorage`, language was pinned by `i18n.init({ lng: "en" })`, and the remaining seven were
written and never consulted. A user could set a preference, see it work for as long as the dialog
was open, and watch the app return to the system defaults on the next load.

`PreferencesContext` (one per SPA) is now the single place that reads a user's row and applies it:

- **Dynamic read** — loaded once the session is known, keyed to the signed-in user.
- **Default fallback** — no row, or an unreachable API, leaves the defaults in place. An absent row
  is the NORMAL state for anyone who has never opened the dialog, not an error.
- **Instant application** — the dialog saves THROUGH the context, so every consumer re-renders at
  once and nothing reverts.

**One row, two subsystems.** `Core.UserPreference` is shared. Home OWNS editing it (Edit Profile);
HRMS only READS it, through a new `GET UserPreference/mine`. A second editor for one shared row is
how validation drifts apart, so the HRMS slice is deliberately read-only.

#### What is actually applied, and what is not

| Preference | Status |
|---|---|
| `theme`, `language` | **Applied** in both SPAs — each has one control point. `<html lang>` is set too, so the applied language is observable and assistive tech sees it. |
| `landingPage` | Honoured by the post-login redirect (Home). |
| `inAppNotifications` | Hides the bell **and stops its 60-second poll** — a preference that leaves the poll running is only half honoured. |
| `dateFormat`, `numberFormat`, `timeZone` | Exposed as `formatDate` / `formatNumber`. **Screens that format values themselves are NOT retrofitted** — there is no chokepoint (the shared `dateFormater.ts` hardcodes its patterns and has 3 callers), so use these helpers or the preference cannot reach that screen. |
| `emailNotifications`, `approvalNotifications` | Stored only — they are instructions to the senders (backend), not to the UI. |

#### ⚠️ Two React traps this hit, both in the provider itself

1. **A ref guard that deadlocks under StrictMode.** The load effect skipped work when the user id
   was unchanged, to avoid a duplicate fetch. StrictMode runs effects twice: pass 1 armed the guard
   and started the fetch, cleanup marked it cancelled, pass 2 returned early on the guard — and the
   cancelled pass never set `loaded`. Anything awaiting `loaded` waited forever; the login redirect
   did exactly that, so **sign-in stopped navigating at all**. The `cancelled` flag already prevents
   a stale response being applied, which is the only thing the guard was really needed for. Use the
   ref to identify WHICH load is current, never to skip an effect run.
2. **A callback dependency that re-triggers itself.** `apply` closed over `ThemeContext.setTheme`,
   which is recreated on each ThemeProvider render — and `apply` is what changes the theme. With
   `apply` in the dependency array, applying a preference re-ran the fetch that applied it. It is
   held in a ref so the effect keys on the user alone.

**And a rule for anything on the sign-in path: cap the wait.** The login redirect briefly waits for
preferences so it can honour `landingPage`, but falls through to `/` on a timer. Sign-in must never
depend on a secondary fetch succeeding.

#### A projection that did not project

While verifying, the API log showed `GetMyProfile` still emitting
`SELECT …, [u].[ProfilePicture]` despite §12.24's projection. `HasProfilePicture =
u.ProfilePicture != null && u.ProfilePicture.Length > 0` cannot be translated, so EF fetched the
whole `varbinary(max)` column to evaluate `.Length` client-side — the exact cost the projection
existed to avoid. `!= null` alone translates cleanly, and an empty array is never stored because the
upload handler rejects a zero-length file. **Read the generated SQL; a projection is not proof.**

### 12.26 Why HRMS still ignored the preferences: two endpoints, two user shapes

The provider from §12.25 was mounted and its endpoint worked, yet HRMS kept showing the defaults.
The cause was neither: **HRMS's two auth endpoints disagree about the shape of a user.**

| Endpoint | Returns |
|---|---|
| `auth/login` | `id`, `fullName`, `email`, `userName`, … |
| `auth/loginStatus` (the session probe) | **`userId`**, **`name`**, `email`, `tenantId`, `isAuthenticated` |

Both were cast straight to the SPA's `User` type, which declares `id` and `fullName`. So on any
session **restored from the cookie** — every deep-link in from the Home portal, and every reload
without `sessionStorage` — `user.id` and `user.fullName` were `undefined`. `PreferencesContext`
loads when `user.id` appears, so it never loaded.

`AuthContext.normalizeUser` now maps either shape onto the declared one, which fixes three things
that were all the same defect:

- **Preferences** load on a cookie-restored session.
- **The header** showed "User" instead of the person's name (it reads `user.fullName`).
- **`useFormLayoutPreference`** had already grown a local `?? user?.userName ?? "anon"` workaround,
  so every restored session shared ONE `"anon"` layout key. It keys per user now — a saved layout
  looks reset once, because the key changes.

#### ⚠️ The test that hid the bug

The §12.25 verification signed in through the **HRMS login form**, which calls `login()` with the
well-shaped `auth/login` payload and writes it to `sessionStorage`. `AuthContext` seeds its state
from `sessionStorage`, so `user.id` was present and everything worked — including after a reload.
The bug only appears when `sessionStorage` is empty and identity comes from the **probe alone**.

**Test the path the user actually takes.** For HRMS that means: mint the cookie WITHOUT the login
form (as the portal's dual sign-in does), clear `sessionStorage` *and* the `localStorage` theme
cache, then load cold. Signing in through the form tests a path that repairs the very data the bug
depends on.

#### Known gap, deliberately left

`loginStatus` reports `Name` as the **username** and `TenantId` as **null**, even though
`auth/login/cookie` appears to add `FullName`, `Email` and `TenantId` claims — so the cookie
principal is not carrying the full claim set, while `UserId` and `Name` come through. The header
therefore greets the user by username. Cosmetic, pre-existing, and unrelated to preferences; noted
in `GetCurrentUserRepository` rather than half-fixed.

### 12.27 Making the Edit Profile dialog open fast — and measuring first

Reported as slow to load. **Measured before touching anything**, which changed what was worth doing:

| | |
|---|---|
| `Account/profile` warm | 3–5 ms |
| `Account/account-profile` warm | 4–5 ms |
| Click → fields populated | **74 ms cold, ~40 ms warm** |

So the server was never the bottleneck on this machine, and most of the 40 ms is React render, not
network. A slower environment (debugger attached, verbose EF console logging) inflates it, but the
dialog was also doing more work per open than it needs — and that cost is paid per user, per open,
on a surface reached from the header of every page.

#### What changed

1. **One request instead of two.** `Account/profile` and `Account/account-profile` were fired in
   parallel on every open and **each re-read `Core.User`**. `GET Account/overview` returns both
   halves from ONE read of that row plus one read of the activity trail.
2. **Cached for 60 s** (React Query `fetchQuery` + `staleTime`), so re-opening inside the window
   costs nothing.
3. **Prefetched when the account MENU opens** — pointer-enter, focus or click. That menu is the only
   route to the dialog, and the human pause between the two clicks is enough for the request to
   land. `prefetchQuery` no-ops while the value is fresh, so opening the menu repeatedly does not
   hammer the API. Saving invalidates the key with `refetchType: "none"` so the next open refetches
   without firing a request nobody asked for.

**Result on the same measurement: ZERO requests when the dialog opens** (the prefetch has already
warmed it), 43 ms cold / 39 ms warm. The request count is the meaningful improvement; on a slow link
or slow API it is the difference of a whole round trip.

#### ⚠️ The index that matters when this table grows

Every "recent security activity" read is `WHERE UserId = @x ORDER BY Date DESC` + `TOP(n)`. The only
index was `IX_LoginTrail_UserId` — UserId alone — so SQL Server seeks the user and then **SORTS
every row they have ever accumulated** to take the newest few. `Core.LoginTrail` gains a row per
sign-in ATTEMPT and is never trimmed, so that sort grows without bound per user.

Added `IX_LoginTrail_UserId_Date` — `(UserId, Date DESC)`, migration
`LoginTrailUserIdDateIndex` (HRMS owns this table).

**Verified with the actual plan, not by assumption.** At the current 122 rows the optimiser still
picks a Clustered Index Scan + Sort — correct, because a scan beats seek-plus-lookups on a tiny
table. Forcing the index shows the intended shape:

```
|--Top(TOP EXPRESSION:((20)))
     |--Index Seek(... IX_LoginTrail_UserId_Date ... ORDERED FORWARD)
     |--Clustered Index Seek(... LOOKUP ORDERED FORWARD)
```

**Top → Seek → Lookup, with no Sort.** It switches to this on its own once a user has enough history
to make the seek cheaper. Deliberately NOT covering: including `UserAgent` (nvarchar 2000) and
`FailureReason` (1000) would nearly duplicate the table to save 20 key lookups per read.

**The habit worth keeping: measure before optimising, and read the plan before claiming an index
helps.** Both steps changed the work here — the first ruled out the server, the second showed the
index is dormant at current volumes and proved it does the right thing at scale.

### 12.28 Core.User: AccountStatus as a bit, PhoneNumber nullable

Two schema changes to a table THREE applications share, applied 2026-08-18 (migration
`UserAccountStatusBitAndNullablePhone`).

| Column | Before | After |
|---|---|---|
| `AccountStatus` | `nvarchar(20) NOT NULL`, default `'Active'` | `bit NOT NULL`, default `1` |
| `PhoneNumber` | `nvarchar(50) NOT NULL` | `nvarchar(50) NULL` |

Data was safe on both counts: all 506 rows were `'Active'` (so nothing was lost collapsing four
states to two), and PhoneNumber had no NULLs but 489 blanks, so nullable matches how it is actually
used.

#### ⚠️ THIS BREAKS SRMS, DELIBERATELY

`cybererp_srms` runs against **this same CERP database** (its connection string is `Database=CERP`),
maps the column as `public string AccountStatus`, and **gates sign-in** on
`user.AccountStatus == "Active"` in `AuthenticationServiceExtensions`. A `bit` cannot be read into a
`string`, so SRMS cannot authenticate against CERP until it is updated to match. This was raised
with evidence before applying and the change was made on that understanding — it is a deliberate
divergence from the SRMS parity established in §12.13–§12.21, not an oversight.

#### The four states, and where "Locked" went

`UserAccountStatuses` (Active | Suspended | Locked | Invited) is removed. Nothing ever set anything
but Active, and — importantly — a **temporary sign-in block was never really this column**: that is
`LockoutEndUtc`, which survives untouched along with `FailedLoginAttempts`. So the dialog's
"Sign-in lock" indicator now reads a new `IsLockedOut` flag computed **server-side** as
`LockoutEndUtc > UtcNow`. That is better than what it replaced: the browser has no business
comparing a UTC instant to the workstation clock, and computing it during render tripped the
`no impure function during render` lint rule.

`PhoneNumber` becomes `string?` on both entities, but the DTO boundary coalesces to `""`, so the
wire contract is unchanged and SRMS — which reads this column into a non-nullable `string` — does
not meet a NULL from anything we write.

#### ⚠️ Two migration traps, both hit

1. **EF's scaffolded `AlterColumn<bool>` cannot work.** SQL Server will not convert `'Active'` to
   `bit`; the ALTER fails on the first row. The column must be REBUILT: drop the (server-named)
   default constraint, add a `bit` column, `UPDATE … CASE WHEN AccountStatus = 'Active' THEN 1 ELSE
   0 END`, drop the old column, `sp_rename` the new one into place, re-add the default.
2. **That rebuild cannot be ONE `Sql()` call.** SQL Server compiles a whole batch before executing
   any of it, so an `UPDATE` naming a column added by an `ALTER` in the same batch fails to parse:
   *Invalid column name 'AccountStatus_bit'*. The usual separator is `GO`, but `GO` is a client
   directive and EF rejects it. **Each `migrationBuilder.Sql()` IS its own batch — that is the
   separator.** Split the rebuild across several calls.

   (The first attempt failed exactly this way; EF's transaction rolled the whole migration back
   cleanly, leaving no temp column behind — verified before retrying.)

Verified after applying: schema and default as intended, all 506 rows = 1; Home returns
`"accountStatus": true` and `"isLockedOut": false`; profile save 200; a genuine NULL phone reads
back as `""` without error; HRMS sign-in 200; the dialog renders Status "Active" / Sign-in lock
"Unlocked".

### 12.29 One sign-in experience across all three subsystems

The SRMS login was a self-contained page — a small bordered card on a plain background, with its own
brand block. It now presents the same shell as HRMS and the Home portal: branded gradient backdrop
with dot grid and outlined geometry, product mark top-left, one elevated card carrying the accent
bar, in-card mark, "Sign in" heading, the form, a divided footer note, and a slim legal footer.

Structure mirrors the siblings too: a reusable `components/auth/AuthLayout.tsx` that the page
composes, exactly as `authLayout.tsx` + `pages/auth/login/page.tsx` do in HRMS and Home.

**HRMS and Home were already identical** — verified, not assumed: a naive `diff` reported the whole
file changed, but that was line endings. `diff --strip-trailing-cr` showed the only real differences
are one comment block and the footer version string. So the target design was unambiguous.

#### ⚠️ It could not be a file copy, and copying would have failed silently

HRMS and Home define their palette as ready-to-use colours (`--primary: #0a4fa3`) with hand-written
utility classes. SRMS is shadcn-style: `--primary: 224 71% 33%`, an **HSL triplet** that is only
valid inside `hsl()`. Pasting their

```
linear-gradient(165deg, var(--primary) 0%, …)
```

into SRMS yields invalid CSS — no gradient, no error, just a flat background. Every colour in the
SRMS shell therefore goes through `hsl(var(--…))`, and the dark end of the gradient is produced with
`color-mix` because SRMS has no `--primary-hover`.

Each app keeps its own palette and brand accent, so the SRMS card reads "Cyber**SRMS**" against its
own primary while the portal reads "Cyber**Home**" — same design, correct identity.

#### Behaviour deliberately preserved

The redesign changed the frame, not the form. SRMS's caps-lock hint, show/hide toggle, per-field
validation on submit and sonner failure toast are untouched. Field presentation was aligned to the
siblings (required asterisks, "User Name" wording, lock icon on the submit button) because that is
the visible part of the consistency being asked for.

**⚠️ Three SRMS trees exist on this machine** (`SRMS-main/SRMS-main`, `CYBER_ERP_SRMS/SRMS-main`,
`CYBER_ERP_SRMS1`). Only the first was changed — it is the one the running dev server on :8080
serves, confirmed because the edits appeared there by HMR. **It is not a git repository**, so those
changes exist on disk only and are not version-controlled.

### 12.30 The portal keys on Abbreviation, not Code

The Home portal identified subsystems by `Core.Subsystem.Code`. That column is not dependable as a
key — the same catalogue holds `HOME`, `002`, `srms` and `Finance`, and it is re-typed by hand.

| Name | Code | Abbreviation |
|---|---|---|
| Self Service Management Sysem | `HOME` | **SSMS** |
| Security and Admin Management System | `002` | **SAMS** |
| SRMS | `srms` | **SRMS** |
| Finance | `Finance` | **IFMS** |

The working copy showed exactly what that instability costs: the SPA was matching `"003"` while the
row still said `HOME`. Nothing errors — `resolveOperationHref` simply stops recognising the portal's
own subsystem, so every Home-local link becomes an external deep-link and the sidebar empties.
Abbreviation is the curated short name, and everything now keys on it.

- **API**: `PortalSubsystemDto.Code` → `Abbreviation`, so the wire field is `abbreviation`.
  Confirmed in the published swagger schema, which no longer carries `code`.
- **Fallback**: the column is NULLABLE, so the projection falls back to `Code` when it is blank. An
  un-keyed row would drop out of the launcher entirely, which is worse than an odd key.
- **Frontend**: `PortalSubsystemModel.code` → `abbreviation`, and every consumer follows — the
  app-URL and API registries, `apiFor` / `baseUrlFor` / `appUrlFor`, `resolveOperationHref`,
  `openExternalSubsystem`, the launcher, dashboard widgets, the menu hook, the sign-in failure toast.
  `DEFAULT_SUBSYSTEM_CODE` → `DEFAULT_SUBSYSTEM_ABBREVIATION`.
- **Environment**: `VITE_SUBSYSTEM_APPS` / `VITE_SUBSYSTEM_APIS` are keyed by abbreviation, so the
  app map reads `SSMS` / `HRMS` / `SRMS` / `SAMS`.

#### ⚠️ The duplicated constant was the actual bug

The portal's own identity was a literal copy-pasted into **three** components plus a fourth inline
check in the menu hook. That is how they drifted: one was updated to `"003"` and the others were
not. It is now `HOME_SUBSYSTEM_ABBREVIATION`, defined once in `config/subsystemApis` and imported.
**Import it; never re-declare it** — a second copy is a future outage, not a style preference.

Scope: the Home portal only. HRMS's own landing page still reads its `Subsystem` feed's `code`,
which is untouched and independent.

### 12.31 HRMS keys subsystems on Abbreviation too

The mirror of §12.30, applied to HRMS's own subsystem feed so both SPAs identify subsystems the
same way.

- **API**: `SubsystemDto.Code` → `Abbreviation` (wire field `abbreviation`), projected from
  `Core.Subsystem.Abbreviation` with a `Code` fallback because the column is nullable. Verified in
  the running API's swagger schema: `["id","name","abbreviation","icon","displayOrder"]`.
- **Search** matches Abbreviation *and* still Code, so a search for what someone remembers from the
  old field keeps working.
- **Frontend**: `SubsystemModel.code` → `abbreviation`; the landing page's HOME exclusion and
  `appUrlFor` both key on it; `VITE_SUBSYSTEM_APPS` re-keyed to abbreviations.
- The Home-identity literal is now `HOME_SUBSYSTEM_ABBREVIATION` in `config/appConfig`, defined
  once — the same fix as the portal, for the same reason.

#### ⚠️ Scope: the SUBSYSTEM identifier only

`Code` is not one concept in HRMS. **33 DTOs expose a `Code`** — Branch, JobGrade, JobCategory,
Position, PositionClass, OrganizationUnit, LeaveType, AllowanceType, Lookup, CareerPath and more.
Those are *business codes* on `Hrms.*` tables: they map to real `Code` columns, those tables have no
`Abbreviation` column at all, and a Position's code is an identifier rather than an abbreviation.
Renaming them would mean a migration across ~33 tables plus every CRUD screen and Zod schema that
references `code`, and would change meaning, not just naming.

Only the subsystem identifier was changed, because that is the one with an `Abbreviation` column
already holding the better value — and it is what makes HRMS and the portal agree.
