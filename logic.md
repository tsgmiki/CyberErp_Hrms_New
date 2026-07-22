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
  step** lists them specifically. Open steps (no approvers) are excluded from personal inboxes — they
  remain actionable from `/workflow`. The Dashboard tab (next to Upcoming Retirements/Clearance) has
  prominent Approve/Reject buttons opening a comment modal (**reason required to reject**); decisions
  call the standard `Workflow/{id}/approve|reject`. This is *the* approver entry point — without it,
  assigned approvers had no cue that work was waiting (actioning only existed on the tracking page).

**Seeded defaults** (`SeedDefaultWorkflows`, `POST /api/v1/WorkflowDefinition/seed-defaults`, idempotent):
Transfer / Promotion / Demotion / Disciplinary (Supervisor Review → HR Approval), Termination
(Manager → HRBP → Dept Head), **Leave Approval (Supervisor Review → HR Approval)**.

### Dynamic clearance configuration (offboarding — mirrors the workflow approver pattern)
`hrms_ClearanceDepartment` (Name, Description = checklist requirement text, SortOrder, IsActive) +
child `hrms_ClearanceDepartmentApprover` (ApproverType User|Role — reuses `WorkflowApproverType` —,
ApproverId, server-resolved DisplayName). Admin UI `/clearanceDepartment` (System group), controller
`ClearanceDepartmentController`, slices in `Features/Core/ClearanceDepartments/`.
- **Checklist build:** `TerminationShared.BeginClearanceAsync` reads *active* departments (ordered
  SortOrder, Name) and stamps each `hrms_TerminationClearance` row with `DepartmentId`; when **none**
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

## 3. Leave logic (Annual Leave — the flagship)

### 3.1 Fiscal-year anchoring
Leave is keyed to **`Core.FiscalYear`** (Ethiopian fiscal calendar), not calendar year. A request is
charged to the *open* fiscal year containing its **start date** (`IFiscalYearResolver.ResolveForDateAsync`);
a request may **not straddle two fiscal years** (submit one per year). Closed years reject new activity.

### 3.2 Entitlement calculation (`ILeaveAccrualService.CalculateEntitlement`)
Driven by `Employee.HireDate` + `Employee.IsManagerial` + the `AnnualLeaveSetting` for the FY/leave type:
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

The employment record (`hrms_Employee`) carries terms that belong strictly to employment (not the
shared `CorePerson`): `EmploymentNature` (Permanent | Contract, string-stored enum), `ContractPeriod`
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
The employee links to a **salary scale** (`coreSalaryScale`) via nullable `Employee.SalaryScaleId`
— the specific grade+step+amount pay point. **`Employee.JobGradeId` was dropped** (migration
`RemoveEmployeeJobGradeId`: DropForeignKey/DropIndex/DropColumn on `hrms_Employee`): the grade is
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

Two tables: `hrms_WorkforcePlan` (aggregate) 1─< `hrms_WorkforcePlanLine`. Slices in
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

- **`hrms_HiringRequest`** (HC077–HC083): directorate + role + headcount + planning-level employment
  type + justification/requirements/timeline + `EstimatedBudget` + optional `WorkforcePlanId` link
  snapshot (no FK, HC081). Status Draft→Submitted→Approved/Rejected→Closed. **Submit gate (HC082):
  requested positions ≤ currently vacant seats** for the unit × role (a Position row = one seat);
  then workflow `HiringRequest` (seeded Directorate Head → HR → Finance, HC078); no definition →
  direct approval. `GET HiringRequest/budget-monitor` = per-unit approved/submitted totals (HC083).
- **`hrms_JobRequisition`** (HC084–HC088, HC091, HC095) 1─< `hrms_RequisitionScreeningCriterion`:
  **creatable only from an APPROVED hiring request (HC080)**; role details (Title/Description/
  Qualifications/Experience/Skills/SalaryScale) default from the request's PositionClass, editable;
  Σ requisitioned positions per request ≤ the request's approved count. Status Draft→PendingApproval
  →Approved→Posted→Closed (+Rejected editable, +Cancelled). Workflow `JobRequisition` (seeded HR →
  Approving Authority, HC085). Posting: channel Internal/External/Both (HC088), `GET
  {id}/generate-posting` builds the standard advertisement from the details (HC091, stored text
  editable), `PUT posting` + `POST {id}/post` (requires text) / close / cancel.
- **`hrms_Candidate`** (HC089–HC090, HC092–HC097): centralized applicant master; Source
  External/Internal/JobBoard/SocialMedia/Referral/WalkIn (HC092); internal candidates link an
  employee (FK SET NULL); structured Education/Experience/Skills summaries + YearsOfExperience
  (resume *parsing* is the HC094 integration hook on top of these fields); resume file upload
  (PDF/DOC/DOCX ≤5MB, photo-storage pattern, `Storage:CandidateResumePath` ??
  App_Data/candidate-resumes, gitignored); **consent mandatory at create** (`ConsentGiven` +
  `ConsentAt`, HC097); `POST {id}/anonymize` scrubs all PII + deletes the resume file irreversibly,
  keeps anonymous history; talent-pool flag + notes (HC089). `GET Candidate/match?requisitionId=` —
  ranked matching (HC090): 60% skills-token overlap + 25 experience-met + 10 talent-pool + 5
  internal; list filter via ?status= Archived|TalentPool|{Source}.
- **`hrms_JobApplication`** (HC098–HC099) 1─< `hrms_JobApplicationStageLog`: unique candidate ×
  requisition; applications accepted only on Approved/Posted requisitions; stage machine Received→
  Screening→Shortlisted→Interview→Selected (+OfferPending/Hired reserved for the offer stage;
  Rejected/Withdrawn/Hired terminal-immutable); **the interview stage is not forced — transitions
  may bypass it (HC102)**; screening score/remarks recorded with moves (HC095/HC099); every
  transition appends an ActedBy/ActedAt log row (tenant-stamped + explicitly Added — the
  aggregate-child gotcha). `?parentId=` scopes the list to one requisition's pipeline.
- **Evaluator scoring & ranking** (migration `AddRecruitmentCandidateLifecycle`): each screening
  criterion can be assigned an evaluator — **internal Employee (FK SET NULL, name snapshotted
  server-side), ExternalPerson, or Organization** (`CriterionEvaluatorType` + name). Evaluators score
  applicants per criterion 0–100 (`hrms_ApplicationCriterionScore`, unique per application×criterion,
  weight snapshot); the application's total **auto-recomputes as Σ(score×weight)/Σ(weight)**.
  `PUT JobApplication/scores` (upsert sheet), `GET JobApplication/ranking?requisitionId=` — ordered
  ranking with per-criterion breakdown + a FailsMandatory flag (mandatory criterion < 50). UI: score
  sheet modal (live total preview) + Ranking modal on the requisition.
- **CorePerson integration & hire conversion:** `Candidate.PersonId` — a CorePerson row is created
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
  education/work history is captured in the **same `hrms_EmployeeEducation` / `hrms_EmployeeExperience`
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
  file attachments in the SAME `hrms_EmployeeDocument` table the employee profile reads (OwnerType +
  OwnerId = row id) via `Candidate/{id}/background-documents` (+ download/delete by document id) — so
  they are on the employee's profile at hire automatically. `EmployeeDocument.EmployeeId` (no FK) anchors
  to the CANDIDATE id until hire; `HireCandidate` re-anchors those rows to the new employee via
  `EmployeeDocument.AssignEmployee()`. Deleting a row cascades its attachments. The candidate form is a
  **tabbed profile** like the employee's (Applicant Details | Education | Experience, tab bar above the
  persistent header) with an applicant-type **switch** (unchecked = External, checked = Internal).
- **Candidate documents & automated migration:** `hrms_CandidateDocument` (typed, binary inline like
  EmployeeDocument, ≤5MB) — upload/list/download/delete under `Candidate/{id}/documents`. **At hire,
  every document (plus the disk-stored resume) migrates automatically** to `hrms_EmployeeDocument`
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
  entity shapes adopted from the §7.1 review: `hrms_Interview` (round ordinal — multiple rounds are
  first-class, NO unique stage-gate; window CHECK end>start; Scheduled→Completed/Cancelled/NoShow)
  1─< `hrms_InterviewPanelist` (EmployeeId? SET NULL + name snapshot, or free-text external
  panelist; lead flag; attendance Pending→Confirmed/Attended/Missed) 1─< `hrms_InterviewFeedback`
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
- **Offers (Phase 2, HC111–HC114)** — `hrms_JobOffer`: tenant-scoped `OFR-####` numbering from the
  NEW race-safe `hrms_NumberSequence` counter (§7.1 adoption #5: atomic UPDATE…OUTPUT via
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
  employee's branch — a user tied to a branch employee is scoped to that branch; a user with no
  employee (or an employee without a branch — e.g. the tenant owner) has global / head-office
  visibility. `LoginRepository` computes this and still writes the `BranchId`/`IsHeadOffice`
  cookies the rest of the app reads, so branch isolation is unchanged downstream.
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
  counter (`hrms_NumberSequence`); existing tenants' counters were seeded from their current max
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
  ANY number of evaluators via the child table **`hrms_CriterionEvaluator`** (criterion 1─<
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
  5. **Indexes:** `hrms_JobApplication (TenantId, AppliedAt)` (the list's tenant-filtered
     `ORDER BY AppliedAt DESC`) and `hrms_JobOffer (ApplicationId, CreatedAt)` (latest-offer
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
1. **Separate database**: SQL Server FKs cannot cross databases → every link to `CorePerson`/
   `hrms_Employee` becomes a comment, not a constraint (and its `PersonID BIGINT` cannot even
   type-match our `uniqueidentifier` PKs). One DB, one EF migration pipeline stays the rule.
2. **No TenantId**: the design is single-tenant; CERP is Finbuckle multi-tenant in a shared DB.
   Every recruitment table keeps `TenantId` — non-negotiable.
3. **BIGINT keys**: rejected for consistency with `BaseEntity`/Guid across the whole product.
4. **Bespoke `RequisitionApproval`**: the generic workflow engine remains the ONLY approval
   mechanism (its `UNIQUE(RequisitionID, ApprovalLevel)` would also break resubmission loops).
5. **`PipelineStep`/`ApplicationProgress` stage-gate**: `UNIQUE(ApplicationID, StepID)` forbids
   re-entering a stage (second interview round, re-screening) — our stage machine + append-only
   `hrms_JobApplicationStageLog` already satisfy HC098–HC102 without that defect.
6. **Numbering triggers**: EF Core 7+ `OUTPUT`-clause conflicts (`.HasTrigger()` burden), and the
   INSTEAD-OF trigger silently drops any column not re-listed in it. Also the script's
   `GenerateRequisitionNumber()` UDF is invalid SQL — `NEXT VALUE FOR` is illegal in scalar UDFs
   (verified: **Msg 11719**). Numbering stays app-layer and tenant-scoped.
7. **Soft-delete + ON DELETE CASCADE together**: contradictory; we keep RESTRICT + archival status
   semantics (and respect SQL Server's multiple-cascade-path limits — cf. `HiredEmployeeId` no-FK).
8. **`UNIQUE(PersonID, PostingID)`**: one pipeline per person per VACANCY is the rule — uniqueness
   stays requisition-scoped (`hrms_JobApplication` unique (CandidateId, RequisitionId)).

**Adopted (into our conventions, Phase 2 targets):**
1. **Interview trio shape** — `hrms_Interview` (schedule/format/status) 1─< `hrms_InterviewPanelist`
   (lead flag, attendance) 1─< `hrms_InterviewFeedback` (per-criterion score+comments, FK'ing
   `hrms_RequisitionScreeningCriterion` — NOT a free-text criterion name) for HC101–HC109.
2. **Offer entity shape** — `hrms_JobOffer` (tenant-scoped number, Draft→Sent→Accepted/Declined/
   Withdrawn/Expired, expiry + response tracking, hiring manager, `HiredEmployeeId?` handoff feeding
   the existing person-based `HireCandidate`); salary validated against the salary scale (HC113).
3. **DB-level range CHECK constraints** as defense-in-depth on new Phase 2 tables (interview
   end > start; feedback score 0–100; offer salary > 0) — FluentValidation guards only the API
   path. (An expiry-vs-start CHECK was considered and dropped: the response deadline legitimately
   precedes the employment start date.)
4. **`(TenantId, Status)`-leading composite indexes** on hot recruitment tables
   (`hrms_JobRequisition`, `hrms_JobApplication` + (RequisitionId, Stage), `hrms_Candidate`) in the
   Phase 2 migration — the tenant filter leads every query, so EF's per-FK indexes alone don't cover.
5. **Numbering race fix before the public portal (HC093)**: `count+1` numbering is race-prone under
   concurrent creates (unique index turns the race into an error today). Replace with a per-tenant
   counter row updated atomically (`UPDATE … SET Value += 1 OUTPUT inserted.Value`) + retry.

**Deferred, not rejected:** a separate `JobPosting` table (multiple channel-specific postings per
requisition, per-channel windows/URLs) — revisit with job-board feeds (HC092, Phase 3); today
HC088's Internal/External/Both posting on the requisition satisfies requirements.

## 8. Database entity relationships (key foreign keys)

```
CorePerson 1─┐
             └─< hrms_Employee >── PositionId → hrms_Position ── PositionClassId → hrms_PositionClass
                    │  SalaryScaleId → coreSalaryScale (pay point; grade DERIVED via scale, not stored)
                    │                                       hrms_PositionClass ── SalaryScaleId → coreSalaryScale
                    │  BranchId   → hrms_Branch             coreSalaryScale ── JobGradeId → hrms_JobGrade
                    │                                                        └─ StepId     → lupStep
                    ├─< hrms_EmployeeEducation / Experience / Dependent / Document  (→ CorePerson)
                    ├─< hrms_EmployeeMovement / DisciplinaryMeasure / EmployeeTermination
                    └─< hrms_LeaveRequest / hrms_LeaveBalance / hrms_LeaveBalanceTransaction

Leave:  hrms_LeaveRequest ── EmployeeId → hrms_Employee, LeaveTypeId → hrms_LeaveType, FiscalYearId → Core.FiscalYear
        hrms_LeaveBalance ── (Employee, LeaveType, FiscalYear) UNIQUE
        hrms_AnnualLeaveSetting ── (FiscalYear, LeaveType) UNIQUE  [accrual policy]
        hrms_Holiday (standalone; feeds IWorkingCalendar)

Workflow: WorkflowDefinition 1─< WorkflowStep 1─< WorkflowStepApprover
          WorkflowInstance (EntityType+EntityId → any governed record) 1─< WorkflowActionLog

Clearance: hrms_ClearanceDepartment 1─< hrms_ClearanceDepartmentApprover (User|Role)
           hrms_EmployeeTermination 1─< hrms_TerminationClearance ── DepartmentId? → hrms_ClearanceDepartment (SET NULL)
           hrms_EmployeeTermination.VacatedPositionId? (snapshot, no FK) + ReinstatedAt? (reinstatement)

Planning:  hrms_WorkforcePlan ── StartFiscalYearId → Core.FiscalYear, OrganizationUnitId? → hrms_OrganizationUnit
           hrms_WorkforcePlan 1─< hrms_WorkforcePlanLine ── OrganizationUnitId → hrms_OrganizationUnit,
                                                            PositionClassId → hrms_PositionClass
           hrms_WorkforcePlan.RootPlanId? (version-chain key, no FK)

Recruit:   hrms_HiringRequest ── OrganizationUnitId, PositionClassId (Restrict); WorkforcePlanId? (no FK)
           hrms_JobRequisition ── HiringRequestId (Restrict), OrganizationUnitId, PositionClassId,
                                  WorkLocationId?, SalaryScaleId? ── 1─< hrms_RequisitionScreeningCriterion
                                  ── 1─< hrms_CriterionEvaluator (EmployeeId? → hrms_Employee SET NULL)
           hrms_Candidate ── InternalEmployeeId? → hrms_Employee (SET NULL), PersonId? → CorePerson (Restrict),
                             HiredEmployeeId? (no FK — cascade-path limit) ── 1─< hrms_CandidateDocument (Cascade)
           hrms_JobApplication ── CandidateId, RequisitionId (Restrict; unique pair)
                                  1─< hrms_JobApplicationStageLog, 1─< hrms_ApplicationCriterionScore
           hrms_Interview ── ApplicationId (Cascade) ── 1─< hrms_InterviewPanelist (EmployeeId? SET NULL)
                                                        ── 1─< hrms_InterviewFeedback (criterion loose)
           hrms_JobOffer ── ApplicationId (Restrict), HiringManagerEmployeeId? (SET NULL),
                            SalaryScaleId? (Restrict), HiredEmployeeId? (no FK);
                            unique ACTIVE offer per application (filtered index)
           hrms_NumberSequence ── PK (TenantId, Key) — atomic per-tenant counters (no BaseEntity)

Auth/tenancy: Tenant 1─< User 1─< UserRole >── Role 1─< RolePermission >── Operation >── Module
              Every hrms_/Core entity carries TenantId (Finbuckle [MultiTenant] filter).
```

**Relationship conventions:** FKs use `OnDelete(Restrict)` except intra-aggregate children (`Cascade`);
self-references (OrgUnit.ParentId, Position.ReportsTo, PositionClass.ReportsTo) are `Restrict` with
cycle-prevention in the update handlers. `UserRole` maps `RoleId`/`UserId` as **plain scalar columns**
(the DB enforces its own FKs) to avoid duplicate shadow FKs.
