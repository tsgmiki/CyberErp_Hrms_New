# handoff.md — Session Handoff

> **Living document.** The latest granular changes, outstanding tasks, and the exact context needed to
> resume seamlessly next session. Update this **every working session** (enforced by `.githooks/pre-commit`).
> Big-picture state lives in `memory.md`; system logic in `logic.md`.

---

## 0. ⚠️ Repository state — READ FIRST

- Branch **`main`**, **one commit** (`6779d11 Initial commit`). **~260 paths are uncommitted** — the
  entire recent build (Salary Scale, PositionClass→SalaryScale refactor, User CRUD, the whole
  Attendance & Leave subsystem incl. fiscal-year integration and the Annual Leave Ledger) is unstaged.
- **Nothing has been committed by the assistant** — commit/push only when the user explicitly asks.
- When you do commit: branch off `main` first if asked to preserve it; the pre-commit hook will prompt
  you to confirm `memory.md` / `handoff.md` / `logic.md` are updated (bypass: `git commit --no-verify`).

## 1. Most recent changes (latest first)

1. **Documentation & state-tracking system** (this task): created `memory.md`, `handoff.md`, `logic.md`
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
