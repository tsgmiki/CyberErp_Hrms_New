# memory.md — Project Memory

> **Living document.** Overarching goals, architectural decisions, and current application state.
> Kept current via the `pre-commit` hook (see `.githooks/pre-commit`). Companions: `handoff.md`
> (granular changes / resume context) and `logic.md` (system logic, workflows, entity relationships).
> For build/run commands and stack conventions, see `CLAUDE.md` (authoritative for the stack).

---

## 1. Product goal

**CyberErp HRMS** — a multi-tenant SaaS Human Resource Management system built to enterprise-ERP
standard (comparable to SAP SuccessFactors / Microsoft Dynamics HR). Requirements are driven by
`HR Management.docx` (NBE Terms of Reference, requirement codes **HC001–HC052+**). Ethiopian context:
dual Gregorian/Ethiopian calendar, Ethiopian fiscal year, Ethiopian Labour Proclamation leave rules.

Delivery is **module-by-module, in verifiable vertical slices** — each module ships backend +
migration + frontend, verified end-to-end against the live DB before moving on.

## 2. Stack (see `CLAUDE.md` for full detail)

| Layer | Tech |
|---|---|
| Backend | ASP.NET Core **.NET 10**, Clean Architecture, solution `backend/CyberErp.Hrms.slnx` |
| ORM / DB | EF Core 10 + **SQL Server** (NOT Postgres — copilot-instructions.md is stale) |
| Multi-tenancy | Finbuckle.MultiTenant (`HybridTenantStrategy` + `DatabaseTenantStore`), single shared DB |
| Auth | Cookie/session (`BaseController` → `[Authorize(AuthenticationSchemes="Cookies")]`) |
| Frontend | **React 19 + Vite**, TypeScript, Tailwind v4, TanStack Query, i18next (en/am) |
| Dates | NodaTime `Instant` for audit timestamps; **`DateTime` for business dates** (no `DateOnly`) |

## 3. Architecture decisions (do not violate without cause)

1. **Clean Architecture, one direction:** `Dom` ← `App` ← `Inf`, `Api` = composition root.
   `Dom` has no external deps; `App` holds vertical slices; `Inf` = EF/repositories; `Api` = thin controllers.
2. **Vertical slices, no MediatR.** Each operation = `I{Operation}` interface + `{Operation}` handler
   (primary-constructor DI). FluentValidation. Wiring is **manual** in `App/DependencyInjection.cs`
   (handlers) and `Inf/DependencyInjection.cs` (repos; generic `IRepository<>` is open-generic registered).
3. **DDD-ish entities:** `BaseEntity` (Guid Id, string TenantId, NodaTime Instant CreatedAt/UpdatedAt,
   `byte[] RowVersion` concurrency token, domain events). Private setters; static `Create(...)` +
   instance `Update(...)` factory methods calling `base.Update()`. Marker interfaces `IAggregateRoot`,
   `IAuditable`, `IBranchScoped`, `ITenantEntity`.
4. **Table naming:** business tables `hrms_<Name>` in schema `Core`; lookups `lup<Name>`; some
   core/config tables `core<Name>` / `Core.<Name>`. Enums stored **as strings** (`.HasConversion<string>()`).
   Any enum a DTO exchanges **by name** needs `[JsonConverter(typeof(JsonStringEnumConverter))]` on the
   enum type (System.Text.Json reads numbers-from-strings but NOT enum names by default).
5. **Generic workflow engine** for ALL approvals (movements, disciplinary, termination, leave).
   New workflow-backed process = one `IWorkflowEntityHandler` + a seeded `WorkflowDefinition`, zero
   engine changes. See `logic.md`.
6. **Ledger over counter** for anything balance-like (leave balances): an append-only transaction
   ledger backs a fast-read summary; every credit/debit is a traceable, reversible row.
7. **Adopt-existing-tables pattern:** when a table pre-exists (created outside EF, e.g. `Core.FiscalYear`),
   map it in a config + hand-strip the scaffolded `CreateTable` from the migration (keep only the delta).
8. **Frontend is templated:** `src/template/` (`useEntityCrudModule`, `EntityModuleShell`,
   `EntityListShell`, `useEntityList`, `createPagedQuery`, `createEntityGetById`, `createSaveService`,
   `createDeleteService`). Build new admin modules from it — do not hand-roll CRUD.

## 4. Current application state (as of this doc's last update)

**Environment:** DB **`CERP`** on `CLOUDX-SICS2\SQLEXPRESS` (SQL Server). API runs at
`http://localhost:5241` (or IIS Express 44363 in Visual Studio). Login: **`hoadmin` / `Passw0rd!`**,
tenant `aadb4e82-2075-48ca-a93c-5cdac93a59b2` ("Head Office", head-office = global visibility).

**Implemented modules (all verified E2E):**
- **Org Structure (§3.1):** OrganizationUnit, Position, PositionClass, JobGrade, JobCategory, WorkLocation; tree + org chart.
- **Multi-Branch:** Branch, branch-level isolation, head-office visibility, audit-trail interceptor.
- **Employee (§3.2, HC015–029):** Employee master + Education/Experience/Dependents/Documents,
  custom-field engine, tabbed profile UI. **Person split:** personal identity in `Core.CorePerson`;
  `Employee.PersonId` FK; Education/Experience/Family re-FK'd to CorePerson. **Employment terms:**
  EmploymentNature (Permanent/Contract), ContractPeriod, IsProbation + ProbationEndDate (conditional,
  required-when), denormalized IsTerminated. **Dashboard analytics:** Employees-on-Probation +
  Upcoming-Retirements widgets (retirement = DOB + 60y, sargable filter).
- **Document Templates (HC022):** `{{placeholder}}` merge engine, TipTap editor, generate/print.
- **Personnel Actions:** Transfer / Promotion / Demotion (EmployeeMovement) + Disciplinary Measures.
- **Workflow Engine:** generic definitions/steps/approvers/instances/action-log; tracking UI + dashboard.
- **Termination & Clearance:** voluntary/involuntary; Manager→HRBP→Dept Head; IT/Store/Finance clearance.
- **Roles/Permissions:** Role/UserRole + Module/Operation/RolePermission (adopted template tables);
  User admin CRUD.
- **Salary Scale:** JobGrade trimmed to Name/NameA/Code; `lupStep` (Step, no UI) + `coreSalaryScale`;
  salary grid filtered by JobGrade. **PositionClass now links to a SalaryScale** (grade+step+exact
  salary), not a JobGrade; added Minimum/Maximum Age + Weekly Working Hours.
- **Attendance & Leave (HC030–052), phased:**
  - *Phase 1:* LeaveType, Holiday, `IWorkingCalendar` (working-days excl. weekends/holidays).
  - *Phase 2:* LeaveBalance (ledger) + LeaveRequest on the workflow engine (submit→approve→deduct,
    cancel→reverse, auto-approve path).
  - *Fiscal-year refactor:* leave balances/requests rekeyed from calendar `Year` → `Core.FiscalYear`;
    `AnnualLeaveSetting` accrual policy; `ILeaveAccrualService` (service-length entitlement +
    idempotent generation + year-end rollover with carry-forward/expiry); probation + FY-boundary guards.
  - *Annual Leave Ledger:* menu to preview + Calculate (generate) entitlements per setting.

**DB migrations (chronological, in `backend/CyberErp.Hrms.Inf/Migrations`):**
`InitialCreate` → `JobGradeRefactorAndSalaryScale` → `PositionClassSalaryScaleAndAgeFields` →
`AddLeaveSetup` → `AddLeaveRequestsAndBalances` → `IntegrateFiscalYearLeave` → `AddEmployeeEmploymentTerms`.

**Not yet built:** Attendance Phase 3 (shifts, capture, daily processing, timesheet), Phase 4
(overtime, regularization, permissions, attendance policy, reports, payroll hand-off), leave encashment.

## 5. Known environment quirks (bite every session — see `handoff.md` for detail)

- EF migrations history lives in **`dbo.__EFMigrationsHistory`** (not `Core.`); `dotnet ef database update`
  works on CERP but **rebuild after `migrations add`** before applying (or the new migration isn't in the DLL).
- Kill running API (`CyberErp.Hrms.Api.exe` / stray `dotnet.exe`) before `dotnet build`.
- The repo is at **1 commit ("Initial commit")** with a very large uncommitted working tree.
