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
9. **One database, one migration pipeline — reaffirmed 2026-07-10** after reviewing (and rejecting
   as-is) an externally-proposed standalone `RecruitmentModule` DB: separate DBs make cross-module
   FKs impossible (comments ≠ constraints) and drop tenancy; BIGINT keys, numbering triggers, and
   bespoke approval/pipeline tables were rejected (the generic workflow engine + stage log stay the
   only approval/stage mechanisms). Adopted from it for Phase 2: interview trio + offer entity
   shapes, DB range CHECKs, `(TenantId, Status)` composite indexes, and a per-tenant atomic counter
   to replace race-prone `count+1` numbering before the public portal. Full decision record:
   `logic.md` §7.1.

## 4. Current application state (as of this doc's last update)

**Environment:** DB **`CERP`** on `CLOUDX-SICS2\SQLEXPRESS` (SQL Server). API runs at
`http://localhost:5241` (or IIS Express 44363 in Visual Studio). Login: **`hoadmin` / `Passw0rd!`**,
tenant `aadb4e82-2075-48ca-a93c-5cdac93a59b2` ("Head Office", head-office = global visibility).

**Cross-cutting services:** `IEmailService` (Email config section: Enabled switch, SMTP relay or
`PickupDirectory` .eml delivery for dev/test; attachments supported; never throws; authenticated
relays like Gmail send AS the login mailbox with the branded address as Reply-To). **E-mail is
dispatched in the background via Hangfire** (`QueuedEmailService` enqueues → `EmailDispatchJob`
sends via `SmtpEmailService` with 5 retries; compose stays in-request so jobs are tenant-free;
SQL storage auto-created in CERP schema `HangFire`; ops dashboard `/hangfire`, cookie-authed).
Consumers: interview lifecycle e-mails (`IInterviewNotifier`) and **offer auto-delivery**
(`IOfferDelivery`: final approval → offer letter rendered as PDF via `IPdfService`/QuestPDF and
queued for e-mail; queued = offer marks Sent + OfferPending automatically, no-address/disabled =
stays Approved, manual Send retries). The offer **PDF is a customizable
template**: `CompanyProfile` (letterhead: name/address/phone/e-mail + logo) + `OfferLetterTemplate`
(tokenized body + signatory), merged by `IOfferLetterComposer` ({{CandidateName}}/{{Position}}/
{{Salary}}/… 10 tokens) — configured under *Recruitment → Offer Letter Template*. Offer acceptance
advances the application to the new **OfferAccepted** stage (offer-driven, hire-ready). Offers also
carry vacancy-derived defaults (position pay point + hiring manager via unit→parent hierarchy) and
the applications list exposes per-row `HireEligibility`/`Rank` — the Offer button only activates
for eligible applicants.

**User ↔ Employee link:** the FK lives on **`User.EmployeeId`** (nullable, set on the user form's
"Linked Employee" dropdown). `User.BranchId`/`IsHeadOffice` columns were REMOVED — branch scope +
head-office visibility are derived at login from the linked employee's branch (no employee / no
branch = head office; tenant owner has no employee → head office). `CurrentUserService` still reads
the branch/head-office cookies (unchanged) that `LoginRepository` now sets from the derivation.

**Recruitment scoring/hire controls:** an assigned criterion evaluator (resolved via
`User.EmployeeId` — the login MUST be linked to the employee on the User form) is strictly scoped:
(1) they SEE only their assigned requisitions' applicants (applications list is server-filtered),
(2) they may score ONLY their assigned criteria (direct scoring + interview-score adoption, others
400), (3) the score sheet shows only their criteria + an "Evaluator view" chip. HR/unlinked users
are unconstrained. Scores lock once the applicant is **Selected or beyond**. The hire conversion
**auto-populates Position & Salary** from the offer/requisition (explicit values override).

**Performance (measured, 2k-applicant vacancy):** list eligibility is set-based
(`RankingShared.ComputeEligibilityAsync`, no full-ranking hydration per page), rank assignment is
O(N log N), hot reads use `AsNoTracking` (⚠ `Repository.GetAll()` tracks by default), hire-queue
docs batched, indexes on JobApplication(TenantId,AppliedAt) + JobOffer(ApplicationId,CreatedAt),
Brotli/gzip response compression, FE React Query staleTime 30 s. List page: 1.1–3 s → 0.13–0.19 s.

**Implemented modules (all verified E2E):**
- **Org Structure (§3.1):** OrganizationUnit, Position, PositionClass, JobGrade, JobCategory, WorkLocation; tree + org chart.
- **Multi-Branch:** Branch, branch-level isolation, head-office visibility, audit-trail interceptor.
- **Employee (§3.2, HC015–029):** Employee master + Education/Experience/Dependents/Documents,
  custom-field engine, tabbed profile UI. **Person split:** personal identity in `Core.CorePerson`;
  `Employee.PersonId` FK; Education/Experience/Family re-FK'd to CorePerson. **Employment terms:**
  EmploymentNature (Permanent/Contract), ContractPeriod, IsProbation + ProbationEndDate (conditional,
  required-when), denormalized IsTerminated. **Pay point:** links to a SalaryScale (`SalaryScaleId`,
  grade+step+amount); **`Employee.JobGradeId` was dropped — the grade is DERIVED via `SalaryScale.JobGradeId`.**
  The Job Grade dropdown survives frontend-only as a **filter** for the scale list; scale auto-fills the
  editable salary. **Dashboard analytics:** Employees-on-Probation + Upcoming-Retirements widgets (retirement =
  DOB + 60y, sargable filter).
- **Document Templates (HC022):** `{{placeholder}}` merge engine, TipTap editor, generate/print.
- **Personnel Actions:** Transfer / Promotion / Demotion (EmployeeMovement) + Disciplinary Measures.
- **Workflow Engine:** generic definitions/steps/approvers/instances/action-log; tracking UI + dashboard.
- **Termination & Clearance:** voluntary/involuntary; Manager→HRBP→Dept Head approval. **Clearance is
  dynamic:** admin-configured `ClearanceDepartment`s (+ per-department User/Role approvers, any one
  authorized user clears; open when none) drive the checklist — built-in IT/Store/Finance only as
  fallback. **Approvers clear from a conditionally-shown Dashboard "Clearance" tab** (queue =
  `my-clearances`, approver-only); the employee's termination-tab checklist is read-only. **Settlement
  gate:** HR finalizes only after all *assigned* approvers clear (blocked halts; open/no-approver
  items auto-clear on settle). **Termination List** menu: terminated employees (excluded from the main
  employee list), complete history modal, and official-document generation (termination merge tokens).
  **Reinstatement:** reverse a settled termination from the Termination List — settlement snapshots the
  vacated position (`VacatedPositionId`), reinstate restores it (or forces a vacant-position pick when
  filled), employee returns to Active. **Clearance certificate:** `{{ClearanceTable}}` merge tokens +
  a seeded "Clearance Certificate" starter template, printed via the existing Generate Document flow.
- **Roles/Permissions:** Role/UserRole + Module/Operation/RolePermission (adopted template tables);
  User admin CRUD.
- **Salary Scale:** JobGrade trimmed to Name/NameA/Code; `lupStep` (Step, no UI) + `coreSalaryScale`;
  salary grid filtered by JobGrade. **PositionClass now links to a SalaryScale** (grade+step+exact
  salary), not a JobGrade; added Minimum/Maximum Age + Weekly Working Hours.
- **Workforce Planning (HC053–076):** versioned, scenario-tagged plans (`hrms_WorkforcePlan` 1─<
  `hrms_WorkforcePlanLine`) anchored to the live establishment (populate from Position seats;
  authorized/filled/vacant per unit × role); demand/supply/separations per line + planning-level
  employment types (incl. Intern/Consultant, Employee untouched); costing from the salary scale with
  budget-threshold escalation gate; seeded Directorate→HR→Finance→Executive approval; version chains
  (approve vN → auto-archive vN−1); retirement suggestions (DOB+60y); scenario comparison; Establishment
  Overview page; approved-demand feed for recruitment (module 3.5 hook). Deferred: structured competency
  model (HC061–063 deep) + requisition consumption.
- **Recruitment & Talent Acquisition (HC077–117), phased:**
  - *Phase 1 (DONE):* HiringRequest (need assessment, establishment-gated submit, Directorate→HR→
    Finance workflow, budget monitor) → JobRequisition (+screening criteria; only from approved
    requests; PositionClass defaults; posting generate/publish Internal/External/Both) → Candidate
    (consent-mandatory, resumes, talent pool, anonymization, skills matching) → JobApplication
    (unique pair, stage machine w/ interview bypass, append-only stage log, screening scores).
  - *Candidate lifecycle (DONE):* per-criterion **evaluators** (Employee/ExternalPerson/Organization)
    scoring 0–100 → auto weighted totals + vacancy **ranking**; `Candidate.PersonId` → CorePerson at
    save (internal candidates reuse the employee's person); typed **candidate documents** w/ the
    mandatory compliance set (ID/Guarantor/Medical/signed offer-or-contract) gating hire; **hire
    conversion** = employee on the SAME person + automatic document migration (EmployeeDocument
    owner Recruitment) + application→Hired + probation tracking; **Talent Pool** page (history +
    apply-to-vacancy).
  - *Candidate structured background (DONE, no migration):* candidate education/work history now
    writes the **same person-owned `hrms_EmployeeEducation`/`hrms_EmployeeExperience` rows the employee
    profile uses** (both keyed on **PersonId**). Because hire creates the Employee on the candidate's
    same PersonId, the data hands off **automatically — zero copy** (`CandidateBackgroundHandlers.cs`,
    `Candidate/{id}/education|experience`). Internal candidates are **read-only** (employee master is
    authoritative). Form UI: **Applicant Type** toggle (Internal → employee picker + locked identity
    prefilled from `GET Employee/{id}`; External → Source Channel) replaces the confusing `Source`
    field; Education/Experience textareas dropped (columns kept), structured `ChildManager` sections added.
  - *Phase 2 (DONE — interviews & offers, migration `AddRecruitmentInterviewsOffers`):* interview
    rounds (multiple per application, no stage-gate) with panels (employees or named externals,
    lead flag, attendance) and per-criterion 0–100 feedback → consolidated report (HC101–HC109);
    first round auto-advances the pipeline to Interview. Formal offers (HC111–HC114): `OFR-####`
    from the new race-safe per-tenant `hrms_NumberSequence` counter; Draft→approval workflow
    (`JobOffer`)→Sent→Accepted/Declined/lazy-Expired/Withdrawn; one ACTIVE offer per application
    (filtered unique index); HC113 scale-deviation requires justification; the offer drives the
    pipeline (send→OfferPending, decline/withdraw/expiry→release to Selected) and gates hire
    (newest offer must be Accepted; hire stamps the offer with the employee). DB CHECKs per §7.1.
  - *Weighted criteria & ranked hiring (DONE, migration `AddCriterionStageScope`):* criteria are
    **percentages that must total exactly 100%** (domain+validator+popup-grid UI), optionally
    scoped per recruitment level (`AppliesAtStage`), weights inherited by score sheets and the
    interview consolidated report (WeightedAverage). Ranking assigns 1st/2nd/3rd + a top-N
    **hire-eligibility window** (N = positions − hired); the rest are **waitlisted** and slide up
    automatically when a higher-ranked candidate's offer is declined/expired; `HireCandidate`
    enforces the window (criteria-less vacancies keep legacy behavior). New **Hire Employee menu**
    (`/hireEmployee`, hire-queue endpoint) — the hire conversion moved there from the candidate form;
    it lists strictly the qualified/ranked applicants with per-row CanHire/BlockedReason.
    **Score-button rule:** global ("All Steps") criteria keep the score action on every pipeline
    step; level-scoped criteria surface it ONLY at their level — server-computed
    (`ScoreableCriteriaCount` on the application DTOs), and the score sheet filters to the same
    subset. **Multiple evaluators per criterion** (migration `AddCriterionEvaluators`):
    `hrms_CriterionEvaluator` child rows (employee SET NULL + name snapshot, or named
    external person/organization; no duplicates per criterion); the criteria popup is a
    card-per-criterion designer with an evaluator chip panel, weight progress bar and
    Distribute-Evenly. See `logic.md` §7.0.
  - *End-to-end review hardening (2026-07-10):* **nothing strands** — vacancy fill auto-closes the
    requisition + dispositions the runner-ups; close/cancel dispositions open applications and
    withdraws live offers; hire withdraws the employee's other applications; anonymize withdraws
    the pipeline before the scrub (`PipelineDisposition` helper). **Offers are rank-gated** like
    hires; **manual screening scores rejected** on criteria-scored vacancies (one source of truth);
    **interview results adopt into the ranking** in one click (adopt-interview-scores);
    **domain guards → 409** (never 500, never retried); **HRQ/REQ/CND numbering** moved to the
    atomic counter (seeded from existing maxima). Details: `logic.md` §7 "Pipeline lifecycle rules".
  - *Phase 3 (todo):* background verification (HC110), public career portal (HC093), onboarding
    checklist (HC115–117 beyond hire conversion); email notifications, resume parsing (HC094),
    job-board feeds (HC092).
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
`AddLeaveSetup` → `AddLeaveRequestsAndBalances` → `IntegrateFiscalYearLeave` → `AddEmployeeEmploymentTerms`
→ `AddEmployeeSalaryScale` → `RemoveEmployeeJobGradeId` → `AddDynamicClearanceConfig`
→ `AddTerminationReinstatement` → `AddWorkforcePlanning` → `AddRecruitmentPhase1`
→ `AddRecruitmentCandidateLifecycle` → `AddRecruitmentInterviewsOffers` → `AddCriterionStageScope`
→ `AddCriterionEvaluators` → `SeedRecruitmentNumberSequences` (data-only).

**Not yet built:** Attendance Phase 3 (shifts, capture, daily processing, timesheet), Phase 4
(overtime, regularization, permissions, attendance policy, reports, payroll hand-off), leave encashment.

## 5. Known environment quirks (bite every session — see `handoff.md` for detail)

- EF migrations history lives in **`dbo.__EFMigrationsHistory`** (not `Core.`); `dotnet ef database update`
  works on CERP but **rebuild after `migrations add`** before applying (or the new migration isn't in the DLL).
- Kill running API (`CyberErp.Hrms.Api.exe` / stray `dotnet.exe`) before `dotnet build`.
- The repo is at **1 commit ("Initial commit")** with a very large uncommitted working tree.
