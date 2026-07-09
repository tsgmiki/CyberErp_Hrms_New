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
`EmployeeTerminationWorkflowHandler`, `LeaveRequestWorkflowHandler`.

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
  be anonymized (their identity lives on as the employee).
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
- **Notifications** (HC079/HC087/HC099/HC100): in-app via status chips + the Dashboard approvals
  inbox; e-mail (incl. the configurable acknowledgement, HC100) is the documented hook — no SMTP
  infrastructure exists. **Deferred to Phase 2/3:** interviews & panels (HC101–HC109), background
  verification (HC110), offer letters/workflow (HC111–HC114), public career portal (HC093),
  onboarding checklist (HC115–116 beyond the hire conversion now in place), job-board feeds (HC092).

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
                                  (criterion.EvaluatorEmployeeId? → hrms_Employee SET NULL)
           hrms_Candidate ── InternalEmployeeId? → hrms_Employee (SET NULL), PersonId? → CorePerson (Restrict),
                             HiredEmployeeId? (no FK — cascade-path limit) ── 1─< hrms_CandidateDocument (Cascade)
           hrms_JobApplication ── CandidateId, RequisitionId (Restrict; unique pair)
                                  1─< hrms_JobApplicationStageLog, 1─< hrms_ApplicationCriterionScore

Auth/tenancy: Tenant 1─< User 1─< UserRole >── Role 1─< RolePermission >── Operation >── Module
              Every hrms_/Core entity carries TenantId (Finbuckle [MultiTenant] filter).
```

**Relationship conventions:** FKs use `OnDelete(Restrict)` except intra-aggregate children (`Cascade`);
self-references (OrgUnit.ParentId, Position.ReportsTo, PositionClass.ReportsTo) are `Restrict` with
cycle-prevention in the update handlers. `UserRole` maps `RoleId`/`UserId` as **plain scalar columns**
(the DB enforces its own FKs) to avoid duplicate shadow FKs.
