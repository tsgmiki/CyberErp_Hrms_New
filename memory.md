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
10. **Subsystem→portal integration is CONFIGURATION, not code — 2026-08-05.** A subsystem (Finance,
    Payroll, PSMS, PM) reaches the Home portal through five surfaces, four of which need zero edits to
    any shared page or component: registration (`coreSubsystem`/`coreModule`/`coreOperation` rows),
    notifications (`dbo.coreNotification` row or `POST /Notification`), the approvals inbox (one entry
    in Home's `config/approvalSources.ts`), My Requests (`config/requestSources.ts`), and dashboard
    widgets (`config/dashboardLayout.tsx`). Subsystem APIs are likewise an env map
    (`VITE_SUBSYSTEM_APIS`) — the portal signs into each at login and `apiFor(code)` returns a bound
    client. Do not hardcode a subsystem anywhere in the portal. Full guide, contracts and worked
    example: **`Home/docs/subsystem-integration.md`**.
11. **Shared login UI, SEPARATE sign-in — decided 2026-08-05.** Each subsystem keeps its own login
    page, route and authentication against its own API; only the *presentation* is shared (HRMS's
    `authLayout` is a deliberate copy of Home's, with the brand supplied per-app by the
    `BrandPrefix`/`BrandAccent` translations). A redirect-based single sign-on (HRMS handing off to
    Home's `/login` with a `returnUrl`) was built and **explicitly rejected by the user** — do not
    rebuild it. Keep the two `authLayout.tsx` files in step when either changes.

## 4. Current application state (as of this doc's last update)

**⚠️ CERP holds ONLY migrated NVI production data (2026-08-10).** The database was reduced to the
seven tables copied from `CERPNVI` — 490 employees, 1356 persons, 1162 positions, 814 position
classes, 121 org units, 38 job grades — all re-tenanted to Head Office
`aadb4e82-2075-48ca-a93c-5cdac93a59b2`. **125 other Hrms/Core tables were emptied**, including every
`WorkflowDefinition`, so governed processes reject submissions until the chains are reconfigured. All
490 employees got an account (`FirstName` + father's initial, password `password`). Every migrated row
carries `CreatedBy = 'migrate:CERPNVI:<sourceId>'` — the only link back to the source. Restore point:
`CERP_before-purge-and-retenant-20260810-154842.bak`. Full detail + two open security findings in
`handoff.md` 00EG and §2.

**Annual leave is NOT a `LeaveType` (2026-08-10).** Its policy is the per-fiscal-year
`AnnualLeaveSetting`; balance rows are identified by `LeaveTypeId IS NULL` and `LeaveType` covers only
the other leave kinds. See `logic.md` §3.1.1 — that section is the rule, not a summary.

**App-shell height (2026-08-10).** `DashboardLayout` pins the shell to `h-screen` (NOT `min-h-screen`)
and gives the content column + `<main>` `min-h-0`. Every list/tree/grid in both SPAs is written as
`min-h-0 flex-1 overflow-auto` inside an `h-full` panel and depends on that definite height to scroll
inside itself; with a mere minimum, `main` grew to its content and the WINDOW scrolled instead. Scroll
bounds come from this chain — do not add hard-coded panel heights.

**Table naming (2026-08-08, APPLIED to CERP): every table lives in its MODULE schema.**
`Hrms.Achievement`, `Core.Module`, `Core.Step`, `Core.Person`, `Core.SalaryScale`, `Core.Notification`
— the `dbo.hrmsX` / `coreX` / `lupX` prefixes are gone. The 10 unprefixed `Core.*` tables (User, Role,
Tenant, RolePermission, …), HangFire, and both `__EFMigrationsHistory` tables are untouched. The 28
report procedures are now `Hrms.Report_X` (was `Core.hrms_Report_X`).
**Renaming tables again? Four things bite, in this order:**
1. EF emits no `GO` between `Sql()` operations, so every `CREATE PROCEDURE` collides in one batch —
   and that also makes `--idempotent` unusable, because a GO cannot sit inside its `BEGIN…END`.
2. Procedure names are also stored as DATA (the report registry), in bare AND `[bracketed]` form.
3. Some names are built by CONCATENATION (`SchemaPrefix + "hrms_X"`), so no search for the full
   string can find them — only running the app does.
4. Hand-written SQL exists outside the EF mappings. Grep the WHOLE solution for BOTH `dbo.hrmsX` and
   `[dbo].[hrmsX]`, then exercise the raw-SQL endpoints (the dashboard) — EF-mapped endpoints passing
   proves nothing about them.

**Routing (2026-08-08): entity records live in the URL.** 88 modules are generated from the registry
in `routes/entityRoutes.tsx` as `/entity` (list) · `/entity/new` · `/entity/{guid}` (edit) — including
`employee`, `organizationUnit` and `position`, where the record is in the URL but the org-tree
selection stays local state. A module is migrated by swapping `useEntityCrudModule()` →
`useEntityRouteModule("/x")` (identical return shape), adding one registry line, and deleting its flat
route. 40 flat routes remain for screens with no single-record concept (dashboards, read-only lists,
wizards, modal-detail screens) — that split is deliberate, not unfinished work. `useEntityCrudModule`
is still required by embedded sections that have no route of their own.
Four rules that are easy to break:
- `new` must ride the SAME `:id` slot as the guid. A static `path="new"` sibling leaves
  `useParams().id` undefined there, so the module cannot tell `/x/new` from the list.
- **Both children must render at the SAME tree depth.** The GUID guard belongs on the shared parent
  route, not in an extra wrapper around `:id` — a depth mismatch makes React remount the module when
  moving list ↔ form, wiping every piece of local state held alongside the record (the Salary Scale
  grade filter, the org-tree selection, the Positions unit preset). Nothing type-checks against this;
  it broke Salary Scale registration entirely and was only caught in the browser.
- Anything matching a URL to a `coreOperation.Link` must use `utils/routeMatch.ts` (longest **segment**
  match). Exact matching ungates nested record URLs; `String.includes` makes `/loanType` inherit
  `/loan`'s grants.
- `createEntityGetById` swallows 404s, so a stale deep link can leave the id empty and turn an update
  into a **duplicate-creating POST**. Guarded centrally in `createSaveService` + per-form fallbacks;
  those fallbacks must yield `undefined`, never `""`, or .NET Guid binding breaks creates.

**Salary revisions support `Step` basis and a `Performance` type (2026-08-08)** — fractional step
increments interpolated against the salary scale, plus score-banded awards; see `logic.md` §9.
`lupStep.Ordinal` is the only valid key for step arithmetic (codes are free text and differ per
tenant); its backfill is an inference worth verifying per tenant. A step revision never reduces pay.
Performance **bands are data, not constants** — appraisal scores are scored against a per-tenant
rating scale (live ones run 1-5, 1-3 and 0-130), so 90/70 thresholds silently match nobody on a 1-5
scale; the simulation reports the observed score range so that is visible before applying.

**Salary increments are governed by a per-tenant policy (2026-08-09)** — `Hrms.SalaryIncrementPolicy`,
one active row, edited on **Compensation → Increment Rules** (`/salaryIncrementPolicy`, its own
permission). Four rules: minimum service months, active-disciplinary exclusion, first-year proration,
and grade-ceiling promotion. See `logic.md` §9.1–9.4. Three things worth carrying forward:
- **Excluded employees get no LINE, not a zero line** — `Apply` walks the lines, so a line pays.
- **The Home portal keeps its OWN copy of the disciplinary/leave screens** that post to the HRMS API
  (`Home/frontend/src/components/admin/...`). A change to one of those HRMS screens is only half done
  until the Home mirror is updated — that is how `AffectsSalaryIncrement` shipped incomplete.
- **`DisciplinaryMeasure.AffectsSalaryIncrement` defaults TRUE** (opt-OUT), unlike the opt-in
  `AffectsPromotion`/`AffectsReward` — the increment block predates the flag, so defaulting it off
  would have silently started paying people mid-discipline. All three flags are independent.
- **Proration scales the increase, never the salary**, so it means the same on every basis.
- **Promotion sequences grades BY PAY**, because `JobGrade` has no level field and grade codes do not
  track pay in live data — code order would promote people into a pay cut. Revisit if a level is added.
Terminated **and retired** employees are excluded both when the population is built AND again at apply
time, since a revision spans days or weeks. `Retired` needs its own check — there is no `IsRetired`
flag, only the status. Active/Probation/OnLeave/Suspended stay in: none of those stop pay.

**Annual leave has a return-confirmation stage (2026-08-09).** An approved request is not done when
the dates pass: the employee confirms their return, and an early or late return routes back through a
SEPARATE workflow (`AnnualLeave.Return`) before the ledger moves. Three rules to preserve — the ledger
only ever moves on an APPROVED decision (so a rejected adjustment needs no reversal); days taken are
recomputed through `IWorkingCalendar`, never by arithmetic, and a late return's overrun sits OUTSIDE
the approved detail rows so it must be counted separately; and a late return is an extension on the
same request, keeping one history thread. See `logic.md` §3.4.1.

**Detail views are pages, not popups.** Salary Revision set the pattern: selecting a row navigates to
`/x/{guid}` and swaps the page to a full `EntityListShell` grid with the shell's standard Back arrow.
Two traps found building it: a double-submit guard must live in a `useRef` (state is captured per
render, so two clicks in one frame both fire), and a detail view must handle its record disappearing
or it spins on `<Loading/>` forever.

**Tests:** `CyberErp.Hrms.Tests` (xUnit) is the solution's first and currently only test project —
**128 tests**, all under `Compensation/`: salary-step interpolation and the no-cut policy, performance
bands, the DTO validator, the three eligibility rules, proration, grade-ceiling promotion, line
persistence, and the still-employed predicate. Everything else in this repo is still verified by live
API/browser runs. New test files here are mutation-checked (break the rule, confirm a test fails)
before being trusted.

**Environment:** DB **`CERP`** on `CLOUDX-SICS2\SQLEXPRESS` (SQL Server). API runs at
`http://localhost:5241` (or IIS Express 44363 in Visual Studio). Login: **`hoadmin` / `Passw0rd!`**,
tenant `aadb4e82-2075-48ca-a93c-5cdac93a59b2` ("Head Office", head-office = global visibility).

**Head-office visibility (corrected 2026-08-05):** `IsHeadOffice` is derived at login from the linked
employee's **`Branch.IsHeadOffice` flag** — NOT from "the employee has no branch", which was the old
rule and silently scoped every user of the real head-office branch to their own org-unit subtree. That
one flag drives both `Repository.ApplyBranchFilter` (head office bypasses branch isolation) and
`PerformanceVisibilityService.IsAdminAsync` (unrestricted vs manager-subtree). It is written into a
cookie at sign-in, so **the fix only takes effect on the user's next login**.

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
  DOB + 60y, sargable filter). **Home dashboard rebuilt 2026-08-05:** one aggregated
  `GET /Dashboard/summary` (a single Dapper `QueryMultipleAsync` round trip replacing 12 queries,
  incl. four `GetAll?take=1` calls made only to read `.total`), six lazy-loaded + `memo()`'d widgets
  behind their own Suspense boundaries with dimension-matched zero-CLS skeletons, and a chart-led,
  high-density presentation built from the app's own data-table language (uppercase column headers,
  column-aligned rows) plus dependency-free SVG donut/bar charts driven by the cached summary.
  Trend sparklines and headcount-by-department were deliberately NOT built — that data is not
  fetched anywhere, and inventing it on a decision-making screen is not acceptable.
- **Document Templates (HC022):** `{{placeholder}}` merge engine, TipTap editor, generate/print.
- **Personnel Actions:** Transfer / Promotion / Demotion (EmployeeMovement) + Disciplinary Measures.
- **Workflow Engine:** generic definitions/steps/approvers/instances/action-log; tracking UI + dashboard.
  ⚠️ **Open steps (2026-08-05):** a step with NO configured approvers means "anyone may act", and both
  the portal alert and the `Workflow/my-approvals` inbox now resolve that audience the SAME way —
  users whose roles hold `CanApprove` on the `/workflow` operation. Previously both derived from the
  (empty) approver rows, so such a request alerted nobody AND appeared in nobody's inbox: invisible,
  silent, undecidable from Home. The seeded default chains ship with open steps, which is why Hiring
  Requests and Other Leave were affected. The fallback is a safety net — configure real approvers for
  proper routing; doing so takes precedence automatically.
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

**Built since this section was last rewritten (2026-07-16 → 2026-07-22, committed 2026-07-22 — see
`handoff.md` §1 item 000 for the full list):** Performance Management (§3.6), Career Development
(§3.7 incl. Talent Review→Succession bridge + Succession Plan approval workflow), Employee Transfer +
Reward & Recognition (§3.7.3–4), Training (§3.8), Engagement + Disciplinary cases (§3.9),
Compensation/Medical/Insurance/Loan/Trip (§3.10, Payroll §3.10.6 excluded per user), strict RBAC +
dynamic navigation, standard report catalog (13 SP-driven reports), Role Permissions rebuild,
10k-user performance pass.

**Not yet built:** Attendance Phase 3 (shifts, capture, daily processing, timesheet), Phase 4
(overtime, regularization, permissions, attendance policy, reports, payroll hand-off), leave
encashment, Payroll (§3.10.6 — explicitly excluded per user).

**Platform expansion (2026-07-29):** a standalone **CyberERP "Home" master portal** now lives at
`D:\Workspace\CyberErp\Home` (own Dom/App/Inf/Api on port 5015 + React SPA on 5175, own repo; zero
HRMS code references — it shares only the CERP platform tables and owns `dbo.coreNotification`).
Architecture is **Home = the sole entry portal**: users log into Home, see only permission-granted
subsystems, and launch HRMS in a NEW tab auto-signed-in (dual cookie login); HRMS offers no path back
(its subsystem picker excludes code `HOME`). HRMS is the **central administration console** for every
subsystem: `coreSubsystem.Url` (migration `AddSubsystemUrl`) + cascading Subsystem→Module filters on
Role Permissions / Menu Operations / Menu Modules / Operation form. Home hosts exactly two request
operations (Annual Leave, Other Leave) + Workflow Tracking, whose screens call the HRMS API directly.

**Portal hardened for multi-subsystem use (2026-08-05, Home repo `main`).** The portal is no longer
HRMS-shaped: (a) subsystem APIs come from a `VITE_SUBSYSTEM_APIS` env map — login fans out to every
one in parallel and `apiFor(code)` returns a client bound to it, while `api` still targets the default
(HRMS) so existing call sites are untouched; (b) notification **broadcasts** (`userId: null`) used to
be stored and seen by NOBODY — reads match strictly on UserId — so they are now fanned out one row per
tenant user (⚠️ `Core.User` has NO tenant query filter, so that fan-out scopes by tenant by hand);
(c) `POST /Notification` accepts the `sourceEntityType`/`sourceEntityId` correlation key and a
`POST /Notification/resolve` clears every recipient's copy; (d) **service-key auth** lets a background
job raise alerts with no user session — each credential is scoped to ONE (subsystem, tenant) and the
tenant is derived FROM THE KEY, and the principal carries no user-id claim so it can write but never
read. Keys are env-only (`ServiceClients__<name>__{Subsystem,TenantId,Key}`); `appsettings.json`
deliberately has no such section.

**Dashboard two-flow performance batch (2026-08-12, both repos — handoff 00ER, logic §2.11).** The
felt delay on "login → dashboard" and "grid action → record" was mostly **CORS preflights**: both SPAs
sent `Content-Type: application/json` on bodyless GETs, so half of all dashboard requests were
uncached OPTIONS round-trips (18 of 36 → 2 of 20 after; the header is now body-only, and both APIs
send `SetPreflightMaxAge(24h)`). Also landed: the `GetMyApprovals` SQL pre-filter (346 ms → 31 ms at
5k instances, id-sets diffed identical), the cached single-query `DatabaseTenantStore` (was 2
`Core.Tenant` queries on EVERY request), the Home identity-probe warm-up from `AuthContext` (kills the
feed waterfall), and idle-time route-chunk prefetch for the dashboard grids (6 scripts on the click
path → 0). ⚠️ Playwright page events DO NOT surface preflights — capture via CDP; and a JIT-cold API
answers ~10× slower than warm, so warm up before trusting a measurement.

**Entity forms must invalidate their OWN detail key on save (2026-08-12 — handoff 00ES, logic §10.1).**
Every form invalidated the plural list key but not `["<entity>", id]`, and the client sets
`staleTime: 30_000` — so re-opening a record within 30 s showed the PRE-SAVE copy with no refetch
(grid fresh, form stale, fixed only by a full page reload). Swept **57 forms**; 7 others already did
it correctly in the targeted `["x", formState.id]` form. ⚠️ Derive the key from the form's own
`useQuery`, never from the folder name — `formBuilder`'s key is `dynamicForm`, and a folder-name
sweep produces both false positives and keys that match nothing. Same session: branch reassignment on
`OrganizationUnit` used to be discarded SILENTLY with a 200 for non-head-office callers (see §10.2).

**`IsAdmin` = an HR PERMISSION (fixed 2026-08-13 — logic.md §11, handoff 00EY).** It USED to
short-circuit on `IsHeadOffice()`, true for every employee in this single-branch tenant, so every
*"if IsAdmin show everything"* check applied to ordinary staff — that is what leaked the whole
organisation's Other Leave. `IsAdminAsync` now checks the `/employee` menu permission
(Administrator + HR Admin only). Effective visibility after: HR 345 employees, manager 2–5 (their
subtree), employee 1 (self). ⚠️ **Only survivable because every employee now holds a role
(`assign-employee-role.sql`) — never port it to an environment where that has not run.** For new
gates still prefer the explicit tool: menu permission, a `/mine` endpoint, or the resolved approver.
**AUDITED 2026-08-13 (handoff 00EV, logic §11.5): 143 no-op checks in 60 files** — 16 cross-employee
guards, 54 HR-only gates, 73 query-scoping sites, with the exposure measured against a live API.
31 pure HR/master-data controllers were gated (safe: 403-for-roleless is correct there);
`EmployeeController` deliberately excluded because it carries `Employee/me`.
**BLOCKER CLEARED 2026-08-13 (handoff 00EW, logic §11.6):** 480 employee accounts held NO role at all
— they worked only because the bug granted them everything. `backend/scripts/assign-employee-role.sql`
(idempotent; **run on every other environment**) fills the ordinary role's missing self-service grants
and assigns it — 480 assigned, 0 roleless employees left, sidebar 34 links vs an admin's 144.
⚠️ The operation catalog has DUPLICATE rows per link (150/132), so permission audits must aggregate by
`Link`. **Category A (16 cross-employee guards) FIXED 2026-08-13** (handoff 00EX, logic §11.7): each
"owner OR HR" guard now checks the HR-side register permission via `HrScreens` — `/loan` vs
`/myLoans`, `/trip` vs `/myTrips` etc. ⚠️ grievances use `/employee` because BOTH sides hold
`/grievance`; always check the two sides differ before reusing a link. **Still open: category C (73
scoping sites), the 54 category-B gates, and repointing `IsAdminAsync` off `IsHeadOffice()`.**

**Leave + salary-revision workflow rules (2026-08-13).** Salary revision now goes
Draft → PendingApproval → **Approved → Submitted** → Applied (the author may only *send for
approval*), with a `PerformanceHistory` row per transition. Other Leave supports **attachments**
(`Hrms.OtherLeaveAttachment`), approvers get `/review` endpoints on both entities, and approval mails
the requester. ⚠️ Resolve "is this the approver" with `ResolveApproverUserIdsAsync`, NOT
`EvaluateAsync` — the latter returns true for everyone on an OPEN step.

**SRMS platform layer, phase 1 (2026-08-13 — logic.md §12, handoff 00EU).** `cybererp_srms` is a
DIFFERENT product, not a newer CERP: its 326 operations share **zero** links with CERP's 150, and its
data is an empty template. Its *architecture* is what is worth taking. Phase 1 added seven additive
tables to `Core` (`Organization`, `OrganizationSubscription`, `SubscriptionPlanModule`,
`TenantSubscriptionAddOn`, `LoginTrail`, `Setting`, `UserPreference`) via `AddSrmsPlatformLayer` —
applied to CERP, no alters or drops, auth untouched — and wired `LoginTrail` into sign-in, which is
the system's FIRST login audit. **`CompanyProfile` CONSOLIDATED into `Organization` 2026-08-13**
(handoff 0101, logic §12.11, migration `ConsolidateCompanyProfileIntoOrganization`): `Hrms.CompanyProfile`
dropped, Organization owns the letterhead (`CompanyName`→`LegalName`, `ContactAddress`→`Address`,
`ContactPhone`→`PhoneNumber`, `ContactEmail`→`Email`, `LogoContent`→`Logo`). ⚠️ **Organization was
INVISIBLE to the repository** — it sits above the tenant and its row has an empty `TenantId`, so the
filter matched nothing; it had to join `IsGlobalEntity`. The profile had ZERO rows so the letterhead
rendered empty; it now resolves the real data that was already in Organization. Wire contract
unchanged (`CompanyProfileDto` keeps its names). **`Core.Setting` SMTP columns REPOINTED 2026-08-13**
(handoff 0102, logic §12.12, no migration): they are the source of truth now — nothing had read OR
written them before (no handler/controller/screen), so they were inert. ⚠️ Resolved **in-request**
(`ISmtpSettingsResolver`), never inside `EmailDispatchJob` — that job has no tenant context and would
silently fall back to config. ⚠️ **The password never enters the job payload — Hangfire PERSISTS job
arguments**; it is read from config inside the send, which is why the table has no password column.
⚠️ `Setting` was invisible (empty `TenantId`) and had to join `IsGlobalEntity`, same as `Organization`.
⚠️ The seeded row held `smtp.cyber.com` and would have redirected live mail →
`clear-seeded-smtp-placeholders.sql`. Added `GET/PUT /Setting` + `POST /Setting/test-email`, gated on
`setting` (no role holds it yet). **Mail DOES work** — `Email:Password` comes from user-secrets, not
appsettings. **Remaining platform tables ALIGNED with SRMS 2026-08-14** (handoff 0103, logic §12.13,
migrations `AlignPlatformTablesWithSrms` + `AlignAssignedByAndSettingUpdatedAt`): 22 shared tables,
13 differed → **7 columns left**. Closed Tenant(+OrganizationId FK/TenantTypeId/3 overrides),
Subsystem(+6 cols), SubscriptionPlan(+Code), Organization(19 diffs), UserPreference, TenantRole/
TenantOperation widths, AssignedBy→Guid, Setting.UpdatedAt→NOT NULL. ⚠️ Traps: OrganizationId FK over
an empty-Guid default; `AssignedBy` string→Guid **fails** on `'seed-tenant-authorization'` (TRY_CAST
null it); `UpdatedAt` NOT NULL scaffolds a `0001-01-01` default. ⚠️ **The last 7 CANNOT be fixed** —
`TenantId` (6 tables) and `User.CreatedAt` are **BaseEntity properties on 202 tables**: TenantId is
the Finbuckle DISCRIMINATOR STRING here vs a Guid FK in SRMS (CERP models that as `OwningTenantId`).
**RE-KEYED 2026-08-14** (handoff 0104, logic §12.14, migrations `TenantIdToUniqueidentifier` +
`MatchSrmsTenantIdExceptions`): 201 columns → uniqueidentifier; shared surface now differs by ONE
column. ⚠️ **Done with a VALUE CONVERTER, not by retyping** — CLR property stays `string`, global
converter in `OnModelCreating` makes the COLUMN a Guid, so **no entity/repository/handler changed**.
Safe because every query use is simple equality; `IsNullOrEmpty` checks run in memory; `""` ↔
`Guid.Empty`. ⚠️ Traps: `Type.GetProperty("TenantId")` throws *Ambiguous match* (TenantSubscription
shadows it with its own Guid) → use `entityType.FindProperty`; EF scaffolds 400 AlterColumn with **NO
index handling** while **141 indexes + PK_NumberSequence** depend on the column → hand-written
discovery SQL in ONE `XACT_ABORT` transaction; blanks must become the empty GUID first. ⚠️ **SRMS is
itself inconsistent** — nvarchar on `LoginTrail`/`UserPreference` — and we deliberately match that.
⚠️ Home needs the same converter or its query filters won't translate. **`User.CreatedAt` FIXED IN
SRMS 2026-08-14** (handoff 0105, logic §12.15) — it was **drift, not design**: SRMS's own BaseEntity,
snapshot and initial migration all say NOT NULL, and no migration ever made it nullable. ⚠️ Applied as
`srms-fix-user-createdat-notnull.sql` because **SRMS's `dotnet ef` is broken** by a pre-existing model
error (`TenantOperation.OperationId` has no CLR property); ⚠️ **the SRMS tree is NOT a git repo** so a
copy lives in `backend/scripts/`. **Shared surface now has ZERO SRMS→CERP differences.** ⚠️ But that
diff is ONE-DIRECTIONAL — the reverse shows **19 columns CERP has that SRMS lacks** (TenantId ×9 from
BaseEntity, OwningTenantId ×4, Setting audit cols, Subsystem.Url/SortOrder): supersets, not
mismatches. **Dropping those extras: STAGE 1 of 4 DONE 2026-08-14** (handoff 0106, logic §12.16,
migration `DropOwningTenantIdUseTenantId`): `OwningTenantId` ×4 removed — provably redundant after the
re-key (**0 mismatches across 695 rows**); FKs added in **raw SQL** since EF can't model a
relationship on a value-converted property. ⚠️ Remaining 14: `UserRole.TenantId` **carries which
tenant an assignment was made in** (the projector derives every membership from it — move creation to
the write site first); `Subsystem` **has HOME/HRMS duplicated per tenant** so going global needs dedup
+ repointing 4 tables, and SortOrder(0–5)→DisplayOrder(all 0); `Setting` audit trio; 6 mechanical.
⚠️ **PROCESS TRAP:** `git commit … | tail -1 && git push` **masks a rejected commit** — `tail` exits 0
so `&&` proceeds and prints success. The pre-commit doc hook rejected a commit this way and the DB was
briefly ahead of the code. **Verify with `git log`, not a printed message.**
**The 7 identity MODULES removed from HRMS 2026-08-14** (handoff 0107, logic §12.17) — Users, Roles,
User Roles, Role Permissions, SubSystems, Menu Modules, Menu Operations. **No migration, no schema
change:** SRMS points at the SAME `CERP` database, so only the MANAGEMENT SURFACE goes (screens,
write endpoints, CRUD handlers, `SeedDefaultMenu`, and the 7 menu entries via
`remove-identity-menu-operations.sql`). **The tables stay** — login reads `Core.User`, the sidebar
reads `Module`/`Operation`, gates read `TenantRolePermission`, and Home reads all of them directly.
⚠️ **A deleted menu operation makes its `[RequirePermission]` key PERMANENTLY UNGRANTABLE** — the
service matches required links against granted ones, so `user`/`role` gates would have been 403 for
everyone forever, silently emptying the approver pickers; re-gated on the consuming screens
(`workflowDefinition`/`clearanceDepartment`/`reports`). **Grep the link before deleting an operation.**
⚠️ **The navigation READS must survive a cleanup like this:** `GET Operation` feeds
`permissionGate.tsx`'s catalog and the gate reads "not in catalog" as "not gated", so removing it
would leave EVERY route unguarded. ⚠️ Permission changes are no longer instant (the handlers that
called `InvalidateAll()` are gone) — SRMS grants land after the **60s TTL**.
⚠️ **Only tenant `aadb4e82` (NVI) has authorization data at all** — 168 TenantOperations, 570 grants.
Tenant `demo` has ZERO, so signing in as `demo` gives an empty sidebar and 403 everywhere. Data, not
a bug.
**Menus fully data-driven in BOTH SPAs 2026-08-14** (handoff 0108, logic §12.18) — the Home question
"why is the menu static?" found three real things: (a) **`SeedHomeMenu.cs`** declared the portal menu
as a compiled C# array and WROTE IT INTO `Core.Operation` via `POST Portal/seed-defaults` — a second
source of truth that overwrote the first; deleted with its endpoint + `Portal:SubsystemUrls` config;
(b) launcher/landing **tiles in BOTH apps** resolved icons via `getModuleIcon(name)`, a
PSMS-template name→icon table matching almost nothing → now `Core.Subsystem.Icon` (mapped on Home's
entity for the first time, exposed on both DTOs, resolved via `lucideIconMap`) + `seed-subsystem-
icons.sql` because the column was **NULL on every row**; (c) `Inbox`/`Bell`/`MessageSquareQuote` were
configured on live rows but missing from both `lucideIconMap`s → silently rendered as circles.
⚠️ **`lucideIconMap` is the one place an icon degrades with NO error** — check it before suspecting
the data. ⚠️ Wiring a column to the UI is half the job when the column was never populated.
Also deleted the PSMS-template dead menu layer from both SPAs (`menu/icons/`, `getModuleIcon`,
`buildSidebarNavigation` — computed every render, never consumed — `menuTypes`, `modules`/
`moduleDetail`/`menuItem`, `quickAdd`, 4 unreachable sidebar subcomponents, `constants/subSystem.ts`).
**SRMS re-alignment STAGE 1 DONE 2026-08-15** (handoff 0109, logic §12.19, migration
`OperationModuleForeignKey`) — ⚠️ **the user CHANGED SRMS**: `Operation.ModuleId` now really FKs to
`Core.Module` and a `Core.TenantModule` table exists, so the 2026-08-13 self-referencing hierarchy is
**superseded**. Repoint cost **ZERO data change** because that migration had copied the 24 modules in
**using their own Ids** (144/144 children already valid) — the invariant paid off in reverse.
⚠️ Constraint names + CASCADE copied from SRMS **verbatim**: `FK_Operation_Module_ModuleId` actually
constrains **SubSystemId** (misnomer in SRMS) and cascades, so deleting a subsystem now deletes its
menu (CERP used Restrict). Fix in SRMS first or they diverge. Non-breaking: sidebar still 12 groups/
34 screens. ⚠️ **REMAINING blocker: `TenantOperation.OperationId`** — SRMS tenant copies are
STANDALONE (0/220 share a template Id, no template column) but BOTH apps use OperationId as the
stable UI id AND as the join between `permissionGate`'s global catalog and tenant-row grants;
dropping it is a permission-layer redesign, not a schema tweak. Also pending: Module (−TenantId,
SortOrder→DisplayOrder, +Filter/+IsActive, narrow to 200, Icon NOT NULL — 1 blank row), drop the 24
group rows (they hold **0 grants**), new `Core.TenantModule`, datetime2(7)→(3), column order. **Phase 2 STEP 1 DONE
2026-08-13** (handoff 00EZ, logic §12.3): the six tenant-scoped auth tables exist and are MIRRORED
1:1 from CERP's own data (`seed-tenant-authorization.sql`), acceptance test **MATCH** — 70,852 grant
rows both sides, 0 lost, 0 gained. **Nothing reads them yet, so behaviour is unchanged.** ⚠️ Traps:
the live model is already tenant-scoped via the discriminator so do NOT cross join Role × Tenant
(506 users → 1500 memberships), and `SELECT DISTINCT NEWID()` never dedupes. **Phase 2 STEP 2 — THE
FLIP — DONE 2026-08-13** (handoff 00FA, logic §12.4): the permission gate and the sidebar feed now
read `TenantUser → TenantUserRole → TenantRolePermission → TenantOperation`; both runtime queries
verified MATCH against the old ones across the whole population
(`verify-tenant-auth-readers.sql`). ⚠️ The admin screens still EDIT the global tables, so every write
path calls `ITenantAuthorizationProjector.SyncAsync()` — without it a permission save updates a table
nobody reads. ⚠️ `User`/`Operation`/`Role` deletes must clear the tenant rows INLINE (NoAction /
Restrict fail; `Role` is SetNull, which succeeds and leaves an invisible role still granting
permissions). ⚠️ Found while testing, unfixed: `Subsystem`/`Module`/`Operation` controllers have **no
`[RequirePermission]`** — any authenticated user can edit the menu. **FIXED 2026-08-13** (handoff
00FE, logic §12.8): gates added to the MUTATING ACTIONS only (Create/Update/Delete + seed-defaults),
links `subsystem`/`module`/`operation`. ⚠️ **Do NOT gate these controllers at class level** — `GET
Operation` is what `permissionGate.tsx` builds its catalogSet from, and an empty catalog reads as "no
route is gated", so every route would fall through UNGATED; gating `Module/WithOperations` would leave
everyone with no sidebar. **Remaining controllers swept 2026-08-13** (handoff 00FF, logic §12.9): 25
gated in THREE patterns — controller-level 1 link (15), controller-level 2 links where an HR and a
self-service screen share it (TrainingEnrollment/Certificate, Survey, EmployeeTermination;
`HasAnyAsync` is an OR), and **writes-only** for reference data whose GETs feed dropdowns app-wide
(Position 12 screens, OrganizationUnit 12, Lookup every combobox, Step, CompanyAsset, DynamicForm).
⚠️ `learningCommunity`/`recognitionWall`/`myPoints`/`myTraining` are EMPLOYEE links, so gating on them
is safe. Left open by design: Auth, Dashboard, Search, Employee(+children), leave, Guarantee,
ProfileChangeRequest, Exit*, Suggestion/Grievance/Announcement, Workflow, EmployeeMovement/
DisciplinaryMeasure, RewardNomination, TrainingNeed. **TenantId DROPPED from User/Role/Operation
2026-08-13** (handoff 0100, logic §12.10, migration `DropTenantIdFromUserRoleOperation`) — SRMS model
COMPLETE, the three tables match exactly. ⚠️ **THE TRAP:** login derived the session tenant from
`user.TenantId` and set the cookie everything resolves against; unmapped it reads `""` → NO tenant →
**every tenant-filtered query returns nothing** (empty sidebar, 0 employees, blank portal) while login
still returns 200 and no log shows an error. Both apps now resolve the tenant from `TenantUser`
membership; Home needs `IgnoreQueryFilters()` there (no tenant exists yet at login). ⚠️ The migration
backfills memberships BEFORE dropping — TenantId IS the membership and 6 users (incl. live headoffice
`dagmawi`) had none. ⚠️ `Repository<T>.IsGlobalEntity` skips the three, so lists scope themselves via
TenantUser/TenantRole, `SaveUser` creates the membership, and the projector UPDATES instances only
(creation moved to SaveRole/CreateOperationHandler/SeedDefaultMenu). **Core.User/Role/Operation ALIGNED
with cybererp_srms 2026-08-13** (handoff 00FB, logic §12.5, migration `AlignCoreTablesWithSrms`):
`User.Password`→**`PasswordHash`**+9 cols, `Role.Code` NOT NULL+3 cols, `Operation.SortOrder`→
**`DisplayOrder`**+`SubSystemId`(FK)+`IsActive`. ⚠️ **`TenantId` KEPT** (SRMS has none) — dropping it
would unscope every User/Role query. ⚠️ Forced departures: NormalizedEmail index is **filtered** (489
of 506 users have no e-mail); `Operation.ModuleId` still → `Core.Module` because **SRMS has no Module
table** and its FK names don't match their columns; `IX_Role_Code` not unique (`TenantId` is
nvarchar(max), can't be indexed). ⚠️ **EF's scaffold was not runnable** — 3 backfills had to be
interleaved before the unique index, the FK and the NOT NULL. ⚠️ **Home shares this DB** and reads the
password column — deploy both repos together. **Core.Operation became the menu TREE 2026-08-13**
(handoff 00FC, logic §12.6, migration `OperationParentChildHierarchy`): `ModuleId IS NULL` = PARENT
(menu group), else the parent it hangs off; the 24 Modules were copied in as parents → **174 = 24+150**.
⚠️ **Parents REUSE their Module's Id**, which is why no child needed repointing — invariant maintained
by both seeders. ⚠️ `Core.Module` **must stay** (SubscriptionPlanModule + TenantSubscriptionAddOn FKs);
it just isn't what navigation reads. ⚠️ Self-FK is **NoAction** (SQL Server forbids a cascading
self-reference) so deleting a group with children is refused in the handler. ⚠️ **Trap I hit:**
`TenantOperation.ModuleId` holds the **template** id — matching it against the tenant copy's own `Id`
returns an EMPTY menu; join on `OperationId`. **Core.RolePermission RETIRED 2026-08-13** (handoff
00FD, logic §12.7, migration `RetireCoreRolePermission`): `TenantRolePermission` is now the ONLY grant
table and the Role Permissions screen writes it DIRECTLY (wire contract unchanged — global ids are
resolved to tenant instances in the handler). Proved redundant first: 70,852 effective grants each
side, 0 lost/gained. ⚠️ **`SyncPermissionsAsync` is DELETED, not disabled** — with no template behind
it, its revocation sweep would delete every hand-edited grant. ⚠️ `CanExport` is never set on create
and **preserved** on edit (no field on the screen). Verify scripts that compared the two models are
gone; use `verify-tenant-authorization.sql` (dangling refs / cross-tenant leakage / tree integrity).

## 5. Known environment quirks (bite every session — see `handoff.md` for detail)

- EF migrations history lives in **`dbo.__EFMigrationsHistory`** (not `Core.`); `dotnet ef database update`
  works on CERP but **rebuild after `migrations add`** before applying (or the new migration isn't in the DLL).
- Kill running API (`CyberErp.Hrms.Api.exe` / stray `dotnet.exe`) before `dotnet build`.
- **Repo state (2026-08-05):** the buildout was merged to `main` via **PR #2** and the old
  `feature/hrms-buildout` branch deleted; work continues on **`feature/hrms-buildout-2`**, with `main`
  as the integration branch. (This line previously said "1 commit + a large uncommitted tree" — long
  obsolete.) `gh` CLI is **not installed** on this machine; GitHub API calls authenticate with the
  token already in the Git credential manager.
- ⚠️ **Frontend colour utilities are HAND-WRITTEN, not Tailwind-generated.** Both SPAs define their
  palette in `src/config/theme.css` as literal classes (`bg-primary/10`, `border-success/20`, …).
  A step that is not in that file — `bg-secondary/40`, `bg-border`, `hover:border-primary/30`,
  `divide-border/60` — compiles to **nothing** and renders transparent, silently. Use a class that
  exists, or an arbitrary value bound to the CSS variable (`bg-[var(--secondary)]`), which Tailwind
  always generates. Related: `.text-foreground` maps to `--text` (slate-900), NOT `--foreground`
  (slate-600) — the variable names mislead. Tailwind conflicts also resolve by CSS order, so
  appending `border-0` after `border` does not reliably win; omit the class instead.
