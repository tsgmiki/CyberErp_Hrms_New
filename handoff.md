# handoff.md — Session Handoff

> **Living document.** The latest granular changes, outstanding tasks, and the exact context needed to
> resume seamlessly next session. Update this **every working session** (enforced by `.githooks/pre-commit`).
> Big-picture state lives in `memory.md`; system logic in `logic.md`.

---

## 0. ⚠️ Repository state — READ FIRST

- On branch **`feature/hrms-buildout`** (branched off `main`). Commits: `6779d11 Initial commit` →
  `c4aabc2` (the big build-out: Salary Scale, PositionClass→SalaryScale, User CRUD, the whole
  Attendance & Leave subsystem + fiscal year + ledger + the docs/hook system) → `e2b0f25` (employee
  employment terms + dashboard workforce analytics) → `9dacdca` (grade derived from salary scale,
  `Employee.JobGradeId` dropped + dashboard redesign) → `d7058db` (Termination List + document
  generation + dynamic clearance config + approver-driven Dashboard clearance queue + settlement gate;
  migration `AddDynamicClearanceConfig`).
- **Uncommitted:** Employee Reinstatement + Clearance Document generation (§1 item 1, migration
  `AddTerminationReinstatement`, applied). Prior work through `709ece0` is committed + pushed.
- Commit/push only when the user explicitly asks. The pre-commit hook prompts you to confirm
  `memory.md` / `handoff.md` / `logic.md` are updated when a commit changes code without them
  (bypass: `SKIP_DOC_CHECK=1` or `git commit --no-verify`). `App_Data/employee-photos/` is gitignored.

## 1. Most recent changes (latest first)

1. **Employee Reinstatement + Clearance Document** (migration `AddTerminationReinstatement`, applied;
   E2E-verified on a disposable tenant, then purged):
   - **Reinstatement:** settlement now snapshots the vacated position
     (`MarkSettled(vacatedPositionId)` → `EmployeeTermination.VacatedPositionId`, no FK) so it can be
     restored. `GET EmployeeTermination/reinstatement-info?employeeId=` reports the previous position +
     availability (+ occupant); `POST EmployeeTermination/reinstate {employeeId,positionId}` validates
     the target is vacant (else 400), `Employee.Reinstate` → Active + placement restored (branch/dept
     follow the position), stamps `ReinstatedAt`; the employee leaves the Termination List. UI: a
     **Reinstate** action on the Termination List opens `ReinstateModal` — preselects the previous
     position when available, else forces a vacant-position pick (`getAllPosition({isVacant:true})`).
     New slice `EmployeeReinstatementHandlers.cs`. **NOTE:** `GetReinstatementInfo` materializes name
     parts then joins in memory — EF can't translate `string.Join` in a projection (hit + fixed in E2E).
   - **Clearance document:** new **Clearance** merge tokens (`{{ClearanceTable}}` raw-HTML checklist,
     `{{ClearanceStatus}}`, `{{ClearanceDate}}`) in `GenerateEmployeeDocument`; `DocumentTemplateType.
     ClearanceCertificate` (string enum, no migration); idempotent `SeedDefaultDocumentTemplates`
     (`POST DocumentTemplate/seed-defaults` + "Seed default templates" button) ships a turnkey
     "Clearance Certificate" template. Generated from the Termination List's existing Generate
     Document action. See `logic.md` §1. **Uncommitted.**
   - **UI fixes (follow-up):** the reinstate vacant-position selector is now the searchable
     `DropDownField` (`take:10`, server-side `searchText` over all vacant positions) instead of a
     plain `<select>`; and the template editor's "Load sample" now has a `ClearanceCertificate` entry
     in `constants/documentTemplates.ts` (it was a silent no-op for that type before). **Uncommitted.**
2. **Dashboard Clearance tab + approver-driven clearance + settlement gate** (no migration — reuses
   `AddDynamicClearanceConfig` schema; verified E2E on a disposable tenant, then purged):
   - **Dashboard "Clearance" tab** (`dashboard.tsx`) next to Upcoming Retirements,
     **conditionally rendered** only when `GET EmployeeTermination/my-clearances` returns
     `isApprover:true`. Lists the approver's outstanding items (specific user/role assignments;
     open departments excluded). **Modern layout:** identity + two prominent **Clear / Block**
     buttons per row; the remark is captured in a **decision modal** (large textarea + Confirm),
     not an inline textbox. Invalidates `myClearances` + `employeeTerminations` on decide (modal
     stays open on error). Backend `GetMyClearances` (+ DI + controller route).
   - **Termination tab checklist is now read-only** (`terminationSection.tsx` `ClearanceRow`
     stripped of the note input + Clear/Block/Reset buttons + Action column; shows note as text) —
     clearance decisions moved entirely to the Dashboard tab.
   - **Settlement gate** (`FinalizeEmployeeTermination`): blocks on any Blocked item; requires every
     clearance whose department has ≥1 approver to be Cleared ("Awaiting: …"); auto-clears remaining
     open (no-approver) items with a `system` note so finalize isn't dead-ended. E2E: queue scoped to
     the user's dept only; finalize 400 (awaiting IT+Finance) → clear IT → 400 (awaiting Finance) →
     role-grant + clear Finance → finalize 200 with Store (open) auto-cleared; blocked-item finalize
     400. See `logic.md` §1 (clearance subsections). **Uncommitted.**
2. **Termination List + document generation + dynamic clearance config** (migration
   `AddDynamicClearanceConfig`, **applied**; verified E2E on a disposable tenant, then purged):
   - **Terminated separation:** `GetAllEmployees` excludes terminated rows unless
     `status=Terminated` is requested. New **Termination List** menu (`/terminationList`, Personnel
     group; sidebar `UserX` icon): `GET EmployeeTermination/terminated` (paged, latest case via
     correlated subquery, settled preferred). Row actions: **History** modal (termination cases +
     read-only clearance detail + movements + disciplinary record) and **Generate Document**
     (reuses `GenerateDocumentModal`). New tokens in the merge engine, group "Termination":
     TerminationType/Date/NoticeDate, LastWorkingDate, TerminationReason.
   - **Dynamic clearance:** new `hrms_ClearanceDepartment` (+`hrms_ClearanceDepartmentApprover`,
     User|Role like workflow steps, display names resolved server-side) + admin UI
     `/clearanceDepartment` (System group, `ClipboardCheck` icon; approver-chip form mirrors the
     workflow definition designer). `BeginClearanceAsync` builds the checklist from active
     departments (fallback: built-in IT/Store/Finance when none configured);
     `hrms_TerminationClearance.DepartmentId` (SET NULL on department delete) links each item.
     Enforcement in `UpdateTerminationClearance`: **any one** authorized user (listed user OR
     holder of a listed role) clears; others get 400 listing authorized names. DTO exposes
     `CanDecide`/`ApproverNames` (batch-computed); `terminationSection` disables decision buttons
     and shows approver names per row. E2E verified: configured checklist replaces defaults,
     unauthorized 400 → user-approver 200 → role-grant then 200, finalize → employee left the main
     list and appeared in the Termination List; letter generated with all termination tokens.
2. **Removed `Employee.JobGradeId` — grade now DERIVED from the salary scale** (migration
   `RemoveEmployeeJobGradeId`, **applied**: DropForeignKey `FK_hrms_Employee_hrms_JobGrade_JobGradeId` +
   DropIndex `IX_hrms_Employee_JobGradeId` + DropColumn `JobGradeId` on `hrms_Employee`). Follows the
   earlier pay-point work (migration `AddEmployeeSalaryScale` added `Employee.SalaryScaleId` FK): the grade
   is redundant on the employee because it's reachable via `SalaryScale.JobGradeId`.
   - **Backend:** dropped `JobGradeId` from `Employee` entity/config + `CreateEmployeeDto`; removed the
     `JobGrade` repo injection and the grade-existence / scale-belongs-to-grade checks in
     `EnsureReferencesExistAsync` (now validates position + scale only); the read projection **derives**
     `JobGradeId`/`JobGradeName` from `SalaryScale.JobGrade`. `EmployeeMovement` keeps its own
     `From/ToJobGradeId` history; From-snapshot sourced from the scale; `ApplyMovement` no longer sets a
     grade (signature dropped the grade param). `DeleteJobGrade` dropped the direct employee check (scale
     guard covers it). Backend builds clean; migration applied to `CERP`; grade-derivation verified via
     the LEFT-JOIN the projection compiles to (scale → grade "01", amount 11000).
   - **Frontend:** Job Grade dropdown **kept as a filter only** (relabelled "Job Grade (filter)") — it
     narrows the Salary Scale list (`getAllSalaryScale({jobGradeId})`) but `saveEmployee` strips
     `jobGradeId`/`jobGradeName`/`salaryScaleStep`/`salaryScaleAmount` from the payload. Picking a scale
     still auto-fills the **editable** Salary. `tsc -b`/`vite build` pass.
   - ⚠️ **Behavioral note:** a movement that records a grade change is history only; to change an
     employee's (derived) grade you must reassign the salary scale. **Uncommitted.**
2. **Dashboard redesign (presentation only, `frontend/src/pages/home/dashboard.tsx`)**: replaced the
   7-block stacked layout (gradient hero + separate sections) with an ERP-style hierarchy — quiet
   header row, one 6-tile KPI strip (incl. actionable On-Probation / Retiring-Soon counts), then a
   2/3 + 1/3 work area: "Approvals & Workflows" card + a **tabbed Workforce Watchlist** (Probation |
   Upcoming Retirements) on the left, compact Recent Activity + Quick Access on the right. Reusable
   inline building blocks (`KpiTile` w/ tone, `Card`, `DaysBadge`, `EmptyRow`); same queries/services,
   theme tokens only, no new libraries. **Uncommitted.**
2. **Employee employment terms + dashboard analytics** (migration `AddEmployeeEmploymentTerms`):
   added `EmploymentNature` (Permanent/Contract), `ContractPeriod` (int months), `IsProbation`,
   `ProbationEndDate`, and denormalized `IsTerminated` (set by `Terminate()`; existing Terminated rows
   backfilled). DTOs/validators/projection updated; conditional-required rules (Contract→period,
   Probation→end date) in FluentValidation + zod + the form. Employee form shows the fields with
   conditional rendering (probation Yes/No dropdown coerced to a real bool in `saveEmployee`). Two new
   dashboard widgets: **Employees on Probation** (`GET Employee/on-probation`) and **Upcoming Retirements**
   (`GET Employee/upcoming-retirements`, DOB+60y, sargable filter). New indexes `(EmploymentStatus,
   IsProbation)` and `DateOfBirth`. See `logic.md` §4–5. **Note:** retirement age 60 is a constant in
   `GetUpcomingRetirements` (not yet config); `IsTerminated` is redundant with `EmploymentStatus.Terminated`
   but was explicitly requested — kept in sync via `Terminate()`.
2. **Documentation & state-tracking system**: created `memory.md`, `handoff.md`, `logic.md`
   at repo root + a tracked `.githooks/pre-commit` hook (activated via `git config core.hooksPath .githooks`).
2. **Annual Leave Ledger** (`/annualLeaveLedger`): new menu — pick an `AnnualLeaveSetting`, preview each
   active employee's service-based calculated entitlement, click **Calculate** to persist (idempotent).
   Backend: `AnnualLeaveLedgerHandlers.cs`, `AnnualLeaveLedgerController` (`GET ?settingId`, `POST calculate`).
   No new tables — a view/action over `LeaveBalance` + `ILeaveAccrualService`.
3. **Fiscal-year leave refactor** (migration `IntegrateFiscalYearLeave`): balances/ledger/requests rekeyed
   `int Year` → `Guid FiscalYearId` (FK to the user-created `Core.FiscalYear`, adopted not recreated);
   new `hrms_AnnualLeaveSetting` accrual policy; `Employee.IsManagerial`; `ILeaveAccrualService`
   (entitlement calc + `GenerateEntitlementsAsync` + `RolloverAsync`); `IFiscalYearResolver`; probation +
   FY-boundary guards in the submit pipeline.
4. **Leave Phase 2:** LeaveBalance ledger + LeaveRequest workflow integration (`LeaveRequestWorkflowHandler`,
   seeded "Leave Approval" definition, cancel/reverse, auto-approve).
5. **Leave Phase 1:** LeaveType, Holiday, `IWorkingCalendar`.
6. Earlier this stream: Salary Scale module, PositionClass→SalaryScale + age/hours fields, User admin CRUD,
   JobGrade alphanumeric validation + Salary-Scale dropdown theme fix, `createSaveService` numeric coercion.

## 2. Outstanding tasks / backlog

- **Attendance Phase 3:** WorkSchedule/Shift, EmployeeShiftAssignment, attendance capture (check-in/out),
  daily processing (present/late/absent honoring approved leave), timesheet. Make the **weekend definition
  shift/policy-driven** — currently hardcoded Sat/Sun in `WorkingCalendar`.
- **Attendance Phase 4:** overtime + regularization + permissions (all workflow-backed), attendance policy,
  reports/dashboards, payroll hand-off interface (`IPayrollAttendanceInputs`: LWOP days + OT hours).
- **Leave encashment** (legacy `hrmsAnnualLeavePayment*`): deferred — money-side, needs salary integration.
- **`Employee.IsManagerial`** is settable via API/DB but **not yet on the employee master UI**.
- Consider moving `dbo.__EFMigrationsHistory` into the `Core` schema so plain `dotnet ef database update`
  stops replaying from scratch on the *old* `CyberErp` DB (CERP is unaffected).
- Optional: annual-leave ledger currently lists *all* active employees (with a 0-entitlement preview for
  post-FY hires); add a filter if only >0-entitlement rows are wanted.

## 3. How to run & verify (exact)

```bash
# from backend/
dotnet build CyberErp.Hrms.slnx                 # kill CyberErp.Hrms.Api.exe / dotnet.exe first
dotnet run --project CyberErp.Hrms.Api --urls "http://localhost:5241"   # Swagger in Development
# migrations (migrations live in Inf, startup is Api):
dotnet ef migrations add <Name> -p CyberErp.Hrms.Inf -s CyberErp.Hrms.Api
dotnet build ...                                 # ⚠ REBUILD so the new migration is in the DLL
dotnet ef database update -p CyberErp.Hrms.Inf -s CyberErp.Hrms.Api
# frontend/
npm run dev        # Vite;  npm run build = tsc -b && vite build (typecheck gates the build)
```
- **DB:** `CERP` on `CLOUDX-SICS2\SQLEXPRESS` (Windows auth). Query via
  `sqlcmd -S "CLOUDX-SICS2\SQLEXPRESS" -d CERP -E -C`.
- **Login for API tests:** `POST /api/v1/Auth/login/cookie {"userName":"hoadmin","password":"Passw0rd!"}`
  (saved cookie jar during dev; tenant `aadb4e82`).
- **Root URL `/` returns 404 by design** — real endpoints are `/api/v1/...` (401 without auth) and `/swagger`.

## 4. Gotchas that will bite (hard-won)

- **Rebuild after `dotnet ef migrations add`** before `database update` (else "No migrations were found").
- **Migration history table is `dbo.__EFMigrationsHistory`**, but `HasDefaultSchema("Core")` — fine on
  fresh CERP; on the abandoned `CyberErp` DB it makes `database update` replay everything ("SubscriptionPlan
  already exists"): there, apply DDL via `sqlcmd` + insert the history row manually.
- **String enums:** add `[JsonConverter(typeof(JsonStringEnumConverter))]` to any enum a DTO takes/returns
  by name (accrual method, gender eligibility, holiday type, leave status, day part, txn type …).
- **`createSaveService` numeric fields:** pass `integerFields`/`numberFields` so a stray decimal in an int
  field doesn't fail JSON binding (nulls the whole DTO → "The dto field is required").
- **`IRepository.AddAsync` stamps TenantId on the aggregate ROOT only** — stamp aggregate *children*
  manually; and don't `UpdateAsync` a freshly-`AddAsync`'d entity (marks it Modified → concurrency error).
- **Test fixtures in CERP were SQL-inserted** (minimal columns): employees EMP-001 Aster Bekele (F, 7y),
  EMP-002 Dawit Haile (M, managerial), EMP-003 Chaltu Gemeda (F, new hire), EMP-004 Meron Tadesse (F, 4y).
  Fiscal years FY 2018/2019 EC; an AnnualLeaveSetting for FY19+AL; Ethiopian holidays. Not created via the
  Employee UI, so they lack position/branch.

## 5. Doc-maintenance checklist (run before committing)

- [ ] `memory.md` — new module / architectural decision / state change recorded?
- [ ] `handoff.md` — moved completed items out of §2, added new changes to §1, refreshed gotchas?
- [ ] `logic.md` — new workflow, approval chain, or entity relationship documented?
