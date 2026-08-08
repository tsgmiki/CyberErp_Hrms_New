# handoff.md — Session Handoff

> **Living document.** The latest granular changes, outstanding tasks, and the exact context needed to
> resume seamlessly next session. Update this **every working session** (enforced by `.githooks/pre-commit`).
> Big-picture state lives in `memory.md`; system logic in `logic.md`.

---

## 0. ⚠️ Repository state — READ FIRST

- **CURRENT BRANCH: `feature/hrms-buildout-3`** (branched off `main` at `706b65f`, 2026-08-05).
  `main` is the integration branch — **open a PR from the current branch when a batch is ready**, then
  rotate to a fresh `feature/hrms-buildout-N`. Completed so far: **PR #2** merged the buildout
  (18 commits) and **PR #3** merged the doc sync; the `feature/hrms-buildout` and
  `feature/hrms-buildout-2` branches were deleted after merging. Historical references to them below
  are accurate for their date, but those branches no longer exist.
- Historical branch note (pre-PR #2): on branch `feature/hrms-buildout` (branched off `main`).
  Commits: `6779d11 Initial commit` →
  `c4aabc2` (the big build-out: Salary Scale, PositionClass→SalaryScale, User CRUD, the whole
  Attendance & Leave subsystem + fiscal year + ledger + the docs/hook system) → `e2b0f25` (employee
  employment terms + dashboard workforce analytics) → `9dacdca` (grade derived from salary scale,
  `Employee.JobGradeId` dropped + dashboard redesign) → `d7058db` (Termination List + document
  generation + dynamic clearance config + approver-driven Dashboard clearance queue + settlement gate;
  migration `AddDynamicClearanceConfig`) → `709ece0` (docs sync) → `2887f96` (employee reinstatement +
  clearance certificate; migration `AddTerminationReinstatement`).
- Later local commits: `346170b` (Workforce Planning) → `059f3b0` (Recruitment Phase 1 + candidate
  lifecycle, migrations `AddRecruitmentPhase1` + `AddRecruitmentCandidateLifecycle`, applied) →
  `077e531` (**Recruitment Phase 2 + enterprise hardening** — everything in §1 items 1–9:
  interviews/panels/offers, ranked hiring + Hire Employee menu, multi-evaluator criteria,
  pipeline-lifecycle hardening, level-gated interviews, reset script; migrations
  `AddRecruitmentInterviewsOffers` → `AddCriterionStageScope` → `AddCriterionEvaluators` →
  `SeedRecruitmentNumberSequences`, all applied to CERP). **Everything is pushed to origin**
  (`feature/hrms-buildout` in sync as of 2026-07-11).
- **Uncommitted:** §1 items 1–16 (Dynamic Form/Tab Builder + paging + attachment fields w/ per-field pools [BE+FE, migrations `AddDynamicForms` + `IndexDynamicFormRecordCreatedAt` + `AddEmployeeDocumentOwnerField` applied]; custom-field engine extended to all 6 child forms [BE+FE, migration `GeneralizeCustomFieldsToChildForms` applied]; Experience IsExternal visible+editable & checkbox styling matched to Employee form [BE+FE, no migration, needs API restart]; employee child-form redesign [FE only, no migration]; movement SalaryScale + experience flags w/ migration `MovementSalaryScaleAndExperienceFlags`; employee-form redesign + IsManagerial [no migration]; performance pass w/ migration `AddPerformanceIndexes`; Hangfire background e-mail [packages + auto-created HangFire schema, no EF migration]; tied-score ranking fix [no migration]; strict evaluator enforcement [no migration]; User↔Employee FK restructure w/ migration
  `ReverseUserEmployeeRelationship`; evaluator permissions + score locking + hire auto-populate w/
  migration `AddEmployeeUserLink`; offer bug-fixes + offer-letter template w/ migration
  `AddOfferLetterTemplate`; offer refinement w/ QuestPDF + auto PDF delivery; interview e-mails +
  e-mail infrastructure; bulk stage moves) + these doc updates. Migrations `AddOfferLetterTemplate`,
  `AddEmployeeUserLink`, `ReverseUserEmployeeRelationship` are applied to CERP. NOTE: `AddEmployeeUserLink`
  (which added Employee.UserId) is now effectively undone by the reverse migration — both are in
  history; a fresh DB replays add-then-drop, which is fine. Untracked: `~$ Management.docx` (Office lock file — do not commit; consider
  gitignoring `~$*`).
- **2026-07-28 commit** bundles §1 items 00FB / 00PT / 00G / 00OL / 00LR / 00LP (Form Builder multi-module +
  lookup selects; profile-tab inline-form conversion; §3.12 Employee Guarantee HC305–307; Other Leave module;
  leave-settings restructure; leave-policy fields moved LeaveType → AnnualLeaveSetting). Migrations applied to
  CERP: `AddDynamicFormFieldLookupCategory`, `AddEmployeeGuarantee`, `WidenGuaranteeTypeForLookup`,
  `AddOtherLeave`, `RestructureLeaveSettings`, `MoveLeavePolicyFieldsToSetting`. Backend build 0 errors +
  frontend `tsc -b` clean at commit time.
- **2026-07-29 commits** (no migrations): §1 item 00RH (report 3-column header + company name) and 00RM
  (recruitment internal-candidate hire routes to a promotion/transfer instead of a duplicate employee).
- **2026-07-29 later commits**: §1 item 00CA (central subsystem administration + Home-first entry flow,
  migration `AddSubsystemUrl` applied; plus the lazy-chunk auto-recovery hardening). The companion
  **Home portal** lives in its own repo at `D:\Workspace\CyberErp\Home`.
- **2026-07-30 commits** (no HRMS migration): §1 items 00WN (approval-request notifications → Home portal;
  needs the Home repo's `AddNotificationSourceRef` migration, already applied to CERP) and 00AL (annual-leave
  own-only `/mine` grid + `/my-balance` dashboard endpoints). Companion Home-repo work: strict notification
  isolation, the annual-leave grid/dashboard/subsystem-link fixes.
- **2026-08-05 commits.** HRMS `feature/hrms-buildout`: `71764c5` (§1 00DC head-office scoping),
  `bb34c5d` (00DD dashboard rework), `e213a57` (00DE open-step notifications + inbox) — no migrations,
  all pushed. Companion **Home repo** (`main`, own remote `CyberErp_Home`): `cb76844` → `56a7d8b`,
  §1 item 00DF — the subsystem integration guide plus the notification/multi-API/service-key contract
  fixes. No migration in either repo; the Home API needs a rebuild for its changes to take effect.
- Commit/push only when the user explicitly asks. The pre-commit hook prompts you to confirm
  `memory.md` / `handoff.md` / `logic.md` are updated when a commit changes code without them
  (bypass: `SKIP_DOC_CHECK=1` or `git commit --no-verify`). `App_Data/employee-photos/` is gitignored.

## 1. Most recent changes (latest first)

00DJ. **Salary revision "by step" — fractional step increments interpolated against the salary scale
    (2026-08-08, backend + frontend, MIGRATION `SalaryStepOrdinalAndStepBasis`, first unit tests).**
    `SalaryAdjustmentBasis` gains `Step = 2` alongside `Percentage`/`FixedAmount`. `Rate` is then a
    step increment (1.5, 2.5), not a percent or an amount.
    - **Blocker found first: `Step` had no numeric ordinal.** Only `Name`/`Code`, and codes are
      inconsistent per tenant — `01`(Base), `1`–`8`, `11`(Celling) in one; `S1` in another; `ST1` in a
      third. Parsing the code would rank the *ceiling* as step 11 and collide Base with step 1. Added
      `Step.Ordinal` (int ≥ 1) as the ONLY key for step arithmetic; `Step.Create/Update` and the Step
      slice/DTOs take it, list ordering switched to `Ordinal`.
    - **Backfill is an inference — review it.** The migration ranks per tenant by the number embedded
      in the code (first digit onwards), falling back to code then name, and numbers 1..N. For the
      rich tenant that produced Base=1, steps 1–8 → 2–9, Celling=10, which is right — but it is a
      reading of what those NAMES mean. Verify once per tenant:
      `SELECT Name, Code, Ordinal FROM Core.lupStep ORDER BY TenantId, Ordinal;`
      Consequence: an employee on code "3" now has ordinal **4**.
    - **Interpolation** (`SalaryStepCalculator.cs`): target = current ordinal + increment, clamped to
      the grade's [base, ceiling] (never extrapolates past the top). A fractional landing is bracketed
      by the two nearest **defined** rungs via binary search and linearly interpolated, rounded to 2dp.
      Live ladders are GAPPED (grade `01` has ordinals 2,4,5; grade `13` has 1,2,3,10), so a naive
      floor/ceil would have been wrong on real data.
    - **Performance:** the ladder is loaded ONCE per revision run, not per employee — a per-employee
      bracket query is an N+1 (10k employees → 10k+ round trips). The scale is grades × steps (19 rows
      here, low hundreds at enterprise size), so one projected read serves every lookup; each employee
      then resolves in O(log steps) in memory. Migration also rebuilds
      `IX_coreSalaryScale_TenantId_JobGradeId_StepId` with `INCLUDE (Salary)` so that read is index-only.
    - **Policy decision — a step revision NEVER cuts pay.** Live data has employees paid above their
      rung (one at 52 000 where the rung pays 45 000); "+1 step" computed 48 000, a 4 000 cut, in bulk.
      `SalaryRevisionShared.HoldPay` floors the result at current pay and annotates the line. Reversible
      in one method if red-circled staff should instead be re-based downward.
    - Simulation/line DTOs expose `CurrentStep`/`ProposedStep`/`Interpolated`/`Note` plus
      `UnresolvedCount`/`InterpolatedCount`, so a revision that legitimately moves nobody is visible
      before applying rather than after.
    - **Frontend:** basis dropdown gains "By step (salary scale)"; the `rate` field relabels per basis
      (Percent / Amount / **Step increment**) with per-basis help text; simulation panel shows the
      interpolated / not-moved counts. `InputField` already emits `step="0.0001"` for numeric inputs,
      so fractions needed no new control.
    - **First test project in the solution:** `CyberErp.Hrms.Tests` (xUnit, CPM, `/Tests/` folder in
      `.slnx`), 35 tests over the interpolation and the no-cut policy. App exposes internals via
      `InternalsVisibleTo` rather than widening its public surface. Mutation-checked: snapping
      interpolation to the lower rung fails 10 tests; allowing pay cuts fails 3.

00DI. **Global search goes straight to the record, for every entity (2026-08-08, HRMS frontend only,
    no backend change).** `globalSearch.select()` now calls `buildRecordRoute(item.route, item.id)`
    (exported from `routes/entityRoutes.tsx`), which returns `/{route}/{id}` when the module is in the
    registry and the plain list route otherwise. Providers already return the base route + record id,
    so every category — current and future — inherits direct-to-edit with no per-provider work and no
    `ISearchProvider` change. Verified: Employees → `/employee/{guid}`, Departments →
    `/organizationUnit/{guid}`. Leave Requests goes through identical code but this tenant has no data
    to exercise it.

00DH. **URL-driven `:id` routing across the whole SPA (2026-08-08, HRMS frontend only, no migration).**
    Records were React state, so nothing was linkable, Back left the app, and refresh dropped the open
    record. Now `/entity` = list · `/entity/new` = create · `/entity/{guid}` = edit, for **88 modules**
    generated from a registry.
    - `template/useEntityRouteModule(basePath)` returns the IDENTICAL shape to `useEntityCrudModule`,
      so migrating a module is one import + one hook line; `list.tsx`/`form.tsx` untouched. All 88 call
      sites were identical (`useEntityCrudModule()`, no args, barrel import), so this was a codemod.
      `useEntityCrudModule` is KEPT — `employee/{leaveRequest,otherLeave}Section.tsx` are embedded
      sections with no route of their own.
    - `routes/entityRoutes.tsx` = registry + factory; `routes/index.tsx` went 320 → ~161 lines and
      keeps 40 flat routes for screens with no single-record concept (dashboards, read-only lists,
      wizards, modal-detail screens). Added the missing `*` catch-all → `pages/home/notFound.tsx`.
    - **Route shape gotcha:** `new` rides the SAME `:id` slot rather than getting a static sibling —
      a static `path="new"` leaves `useParams().id` undefined on `/x/new`, indistinguishable from the
      list (this bit: `/branch/new` rendered the list until fixed). `EntityRecordGuard` admits `"new"`
      and rejects non-GUIDs before any API call.
    - **Employee + Organization Structure** (`employee`, `organizationUnit`, `position`) are included:
      the record goes in the URL, the org-tree selection stays local. `employee` swaps list↔profile;
      the other two open a modal over the tree. `position`'s owning unit is a hidden field fed only by
      the tree preset, so a cold `/position/new` shows the "select an organization unit" hint instead
      of saving an unparented position.
    - **Security fix shipped with it:** `PermissionGate` matched the path EXACTLY, so `/branch/{guid}`
      was not recognised as an operation and fell through UNGATED. Proved empirically by restoring the
      old matcher: a role without Branch access could open `/branch/new` and reach `/branch/{guid}`.
      Five call sites now share `utils/routeMatch.ts` (longest segment match); two of them
      (`useListPermissions`, `gridAction`) used raw `String.includes`, so `/loanType` was inheriting
      `/loan`'s grants — pre-existing bug fixed as a side effect.
    - **Data-integrity fix:** `createEntityGetById` swallows 404s → `formData` stays empty → the hidden
      `id` renders blank → `createSaveService` reads that as "no id" → **POST, creating a duplicate**
      instead of failing the update. Unreachable before; a pasted stale link makes it reachable.
      Closed centrally in `createSaveService` (recovers the id from the route, but ONLY when the
      route's base segment names that same resource, so a child form on a parent record URL is
      untouched) + 27 hand-written forms given a route-id fallback. **Watch the `|| undefined`:** an
      early pass wrote `id: meta.id || id`, which puts `id: ""` into payloads where the key was
      previously absent — 13 services keep the id in the body on create and would have sent `""` to a
      .NET Guid binder, breaking creates.

00DG. **HRMS login page now uses the SAME UI as the Home portal's (2026-08-05, HRMS frontend only,
    no migration, no auth/behaviour change).** Purely visual: both apps keep their OWN login pages,
    their own `/login` route and their own sign-in against their own API — only the presentation was
    unified.
    - Ported Home's auth shell into `components/auth/authLayout/authLayout.tsx`: gradient backdrop +
      dot grid + outlined circles, top-left product mark with the ERP tagline, one elevated centred
      card (accent bar, in-card mark, heading, form, divided footer), slim legal footer. Login page
      passes `title`/`subtitle`; register page gained a heading (it shares the layout and would
      otherwise render a headless card). `authBrand.tsx` deleted — orphaned by the replacement.
    - **The brand adapts by itself**: both apps already define `BrandPrefix`/`BrandAccent`, so the
      identical layout renders "CyberHRMS" here and "CyberHome" there. Footer uses HRMS's own
      `Cyber HRMS v1.0` key. Keep the two `authLayout.tsx` files in step when either changes.
    - Ported Home's opt-in `frameless` flag (`FormModel` + `formProvider` authMode branch) so the
      fields sit directly on the card instead of in a nested frame.
    - ⚠️ **GOTCHA that cost a round:** HRMS's existing pattern APPENDS override classes
      (`… border … ${inModal ? "border-0 p-0 shadow-none" : ""}`). That does not reliably win —
      Tailwind conflicts resolve by CSS order, NOT by the order classes appear in the attribute, so
      the frame stayed visible. Home instead OMITS the frame classes; `frameless` now does the same.
      Scoped so `inModal` keeps its exact previous string ⇒ every non-frameless form is byte-identical
      and no other screen is affected.
    - **NOT wanted, do not rebuild:** an earlier attempt in this session delegated HRMS sign-in to the
      portal (redirect to Home's `/login` + `returnUrl`, `VITE_PORTAL_URL`, an origin allow-list). The
      user rejected it and it was reverted in full — the requirement is shared UI, NOT shared sign-in.
    Verified: `tsc -b` + eslint clean; both `/login` and `/register` render with zero console errors;
    Home repo untouched.

00DF. **Home portal made genuinely pluggable for other subsystems: integration guide + four contract
    fixes (2026-08-05, HOME REPO ONLY — `main`, commits `cb76844` → `56a7d8b`; no migration, no HRMS
    change).** Asked how Finance/Payroll/PSMS/Project-Management add their requests without hardcoded
    changes. Answer: four of the five surfaces already needed ZERO core changes — writing the guide is
    what exposed the gaps, and all four are now closed.
    - **The guide: `Home/docs/subsystem-integration.md`** (527 lines; every cited path verified to
      resolve). Five integration surfaces — subsystem registration (`coreSubsystem`/`coreModule`/
      `coreOperation` rows), notifications, the approvals inbox (`config/approvalSources.ts`), My
      Requests (`config/requestSources.ts`), custom widgets (`config/dashboardLayout.tsx`) — plus the
      contract rules (assigned-only / self-scoped / best-effort-null-on-failure), a worked Finance
      expense-claim example, a phase-grouped checklist and a purpose-grouped reference table.
    - **Fix 1 — broadcasts reached NOBODY.** `CreateNotificationDto` advertised `userId: null` as a
      tenant broadcast, but the read path is strictly `n.UserId == uid` (the blanket `|| UserId == null`
      clause had been removed for isolation), so the row was stored and invisible to everyone. Now
      fanned out to one row per tenant user, returning `{id, created}`. ⚠️ `Core.User` deliberately has
      **no tenant query filter** ("login searches across tenants"), so the fan-out scopes by tenant BY
      HAND — without that it would notify every tenant on the platform.
    - **Fix 2 — the HTTP API could not set the correlation key.** `Notification.Create` didn't accept
      `SourceEntityType`/`SourceEntityId` (though entity+table had them), so API-raised alerts could
      never be auto-cleared. Both now accepted, plus `POST /Notification/resolve` clearing every
      recipient's copy. Domain `ArgumentException`s at this boundary became actionable 400s (a bad
      `severity` used to surface to a calling subsystem as an opaque 500).
    - **Fix 3 — the SPA knew ONE subsystem API.** `apiClient` read a single `VITE_API_BASE_URL` and
      login signed into exactly two backends. Now `src/config/subsystemApis.ts` builds the registry
      from `VITE_SUBSYSTEM_APIS={"HRMS":"…","Finance":"…"}`; `createApiClient(baseUrl)` +
      `apiFor("Finance")` (throws for an unconfigured code rather than silently falling back to HRMS);
      login fans out to every entry IN PARALLEL and names failures. `api` still binds to the default
      subsystem so the ~35 existing call sites are untouched, and legacy `VITE_API_BASE_URL` is still
      honoured as HRMS. `portalApiClient` now builds on the same factory (it already exported
      `portalApi` to 9 consumers — a duplicate export was nearly introduced).
    - **Fix 4 — no service-to-service auth.** Write endpoints now accept a user cookie OR a service key
      (`X-Service-Key`), so a background job can raise alerts with no human present. **Each credential
      is scoped to one (subsystem, tenant) and the tenant is derived FROM THE KEY**, so a caller cannot
      choose the tenant it writes to. Config is a NAMED credential
      (`ServiceClients__financeAcme__{Subsystem,TenantId,Key}`) supplied by environment variables —
      `appsettings.json` deliberately has no `ServiceClients` section. Boundaries: service principal is
      issued **no user-id claim**, so every read path (which filters on user id) resolves to zero rows —
      it can write only, never read; cross-subsystem impersonation → 400; wrong/incomplete key, or a
      secret reused across credentials → 401; `FixedTimeEquals` over every client; keys never logged.
    - **GOTCHAS worth keeping.** (a) **CORS is the trap**: a subsystem API missing the portal origin in
      `Cors:AllowedOrigins` looks EXACTLY like "the subsystem is down" — cost a failed test run; use a
      `Cors__AllowedOrigins__N` env override to test without editing appsettings. (b) A **tenant GUID
      contains hyphens**, which are not legal in shell env-var names — that is why the service
      credential is `ServiceClients__<name>__TenantId=<guid>` and not `ServiceClients__Finance__<guid>`.
      (c) `Core.User` has no tenant filter (see Fix 1).
    - Verified live throughout: 10/10 registry-parsing units; 9/9 browser test across three backends
      (portal + HRMS + a deliberately-dead Finance) proving the fan-out, that HRMS still returns 200 and
      routes correctly, and that only the down subsystem is named; 8/8 service-key security tests
      including a tenant-A key refused for tenant B and a tenant-B broadcast landing on exactly tenant
      B's 10 users; human-session regressions each time. All test rows deleted, DB back to baseline.

00DE. **Open workflow steps were invisible end-to-end: no portal alert AND absent from the approval
    inbox — Hiring Requests could never be approved from Home (2026-08-05, backend only, no
    migration, no API-contract change).** Reported as "a manager submits a Hiring Request from the
    home page but the approver gets no notification and cannot approve it there".
    - **Root cause — ONE condition, two symptoms.** A step with no configured approver rows is treated
      by `WorkflowApproverAuth.EvaluateAsync` as an **open step — "anyone may act"** (it returns
      `(true, [])`). But both downstream consumers keyed off the approver ROWS, which are empty:
      (a) `WorkflowService.NotifyCurrentStepApproversAsync` resolved recipients from that empty list
      and `IPortalNotifier.NotifyUsersAsync` no-ops on an empty set → **no coreNotification row ever
      written**; (b) `GetMyApprovals` matched the caller against the step's approver rows, so `mine`
      stayed false and `continue` **skipped the instance entirely** → nothing in the Home Approvals
      Inbox / Workflow Tracking to decide. Net effect: the system said *anyone can approve this* and
      then told nobody, and showed it to nobody. Silent — no error, no log.
    - **Why leave worked and hiring did not:** verified in CERP — every AnnualLeave step has 1 approver
      configured; the active HiringRequest definition has **0** (so does the active OtherLeave, which
      was silently affected too).
    - **Fix (3 files, backend only).** `WorkflowApproverAuth`: `ResolveOpenStepRecipientsAsync()` (who
      to alert) + `CanActOnOpenStepsAsync()` (does it land in MY inbox) — both resolve from ONE rule,
      roles carrying `CanApprove` on the `/workflow` operation, so the alerted set and the inbox set
      are identical by construction. `WorkflowService`: falls back to that audience, and logs a
      WARNING naming the cause when even that is empty (never silent again). `WorkflowHandlers.
      GetMyApprovals`: an open step now resolves by entitlement instead of approver-row matching, and
      counts toward `isApprover` tab visibility.
    - **NO frontend change was needed** — the Home portal is already generic: `config/approvalSources.ts`
      is a pluggable registry whose single HRMS source reads `Workflow/my-approvals`, feeding both the
      dashboard `ApprovalsInbox` widget and the `/workflow` Tracking screen (which has inline
      Approve/Reject + history). Any entity type flows through it once the endpoint returns it.
    - **Verified end-to-end** against the running API with an open-step workflow (the exact hiring
      condition): submit → notification raised → appears in `my-approvals` → approved via the same
      endpoint the portal calls (200) → instance Approved, entity handler ran, notification
      auto-resolved, inbox empty again. Test vehicle was EmployeeGuarantee because the demo tenant has
      no org units/positions (HC082 establishment gate blocks creating a hiring request there); the
      code path does not branch on entity type. All test rows deleted, DB verified clean afterwards.
      Real-world confirmation: a genuine Hiring Request (HRQ-0006) submitted while the fixed build was
      running correctly alerted `admin` + `medhanit`, the 2 users with approve rights.
    - **⚠️ This is a SAFETY NET, not routing.** The seeded chain is Directorate Head → HR → Finance but
      the live definition is a SINGLE step with no approvers, so every approve-capable user is alerted
      for every hiring request. Configuring real approvers in Workflow Definitions takes precedence
      automatically and is the proper fix. Note also `EnsureCanDecideAsync` still lets ANYONE decide an
      open step (unchanged, deliberately) — the new rule bounds who is TOLD, not who may act.

00DD. **Dashboard UI reworked as a chart-led, high-density page in the app's own table language
    (2026-08-05, frontend only, no migration, no API change).** Pure visual/JSX pass — verified by
    diff that ZERO hooks, queries, state, handlers, services or backend files changed. Took four
    attempts; recording what actually mattered so nobody re-treads it:
    - **What the earlier attempts got wrong.** (a) The dashboard's feeds were loose stacked lists in
      an invented style, while every other screen in the system (`components/common/dataTableProvider`)
      uses column-aligned tables with uppercase micro-caps headers over a `bg-muted/50` strip — so the
      page read as foreign. (b) It was far too airy. (c) It had NO data visualisation at all, which is
      the thing that most separates a Fiori/D365 dashboard from a list of cards.
    - **Now:** feeds are real tables (header strip + rows share ONE css-grid template so columns align
      exactly); KPI tiles are horizontal ~72px (was ~110px) with a semantic left accent rail; card
      headers `py-2` uppercase w/ inline icon; rows `py-2`, 12px primary / 11px secondary; canvas is
      neutral `bg-background` (was the pale-blue `bg-secondary`, which washed the page and made white
      cards vanish into it). Roughly 30% more content per screen.
    - **New files:** `components/dashboard/charts.tsx` (dependency-free SVG donut / legend / bars,
      purely presentational, no data access) and `WorkforceAnalyticsWidget.tsx` (Workflow Status donut
      + Workforce Composition bars). The widget reuses the EXISTING `useDashboardSummary()` hook —
      same queryKey, so React Query serves it from cache: **zero extra network calls.**
    - **Deliberately NOT built:** trend sparklines and headcount-by-department. Neither exists in the
      data currently fetched (no historical series; summary returns totals with no dept breakdown) and
      fabricating them on a decision-making screen is not acceptable. Dept breakdown would be one extra
      `GROUP BY` in the existing `DashboardSummaryService` Dapper batch if wanted later.
    - **⚠️ THE TRAP THAT COST TWO ROUNDS:** this app does NOT feed its palette into Tailwind's theme.
      Every colour utility (`bg-primary/10`, `border-success/20`, …) is HAND-WRITTEN in
      `frontend/src/config/theme.css`. A step that isn't in that file — `bg-secondary/40`, `bg-border`,
      `hover:border-primary/30`, `divide-border/60` — compiles to NOTHING and renders transparent,
      silently. Anything outside the hand-written set must be an arbitrary value bound to the CSS var
      (`bg-[var(--secondary)]`, `border-[color-mix(in_srgb,var(--primary)_40%,transparent)]`), which
      Tailwind always generates. Also note `.text-foreground` maps to `--text` (slate-900, high
      contrast), NOT `--foreground` (slate-600) — the variable names are misleading.
    Verified: `tsc -b` + eslint clean; Playwright at 1600/1150px with stubbed populated data 10/10
    (no overlapping cells, no horizontal overflow, no stray skeletons, donut renders real proportional
    arcs); dark mode checked. NOTE: an earlier rejected redesign is parked in `stash@{0}` — drop it.

00DC. **Head-office users were scoped to their own department subtree — `IsHeadOffice` now reads the
    branch flag (2026-08-05, no migration).** A user assigned to the branch flagged `IsHeadOffice = 1`
    (here `Corporate`) could only see their own department + child departments in the Employee module's
    org-tree list. Root cause in `Inf/Repositories/Core/LoginRepository.cs`: head-office status was
    derived as `var isHeadOffice = branchId is null;` — i.e. it only recognised users with **no branch at
    all** (tenant owner / unlinked account) and completely ignored `Branch.IsHeadOffice`. Because the
    Head Office is itself a real branch row, its staff got `branchId != null` → `isHeadOffice = false`.
    That one flag drives BOTH visibility gates: `Repository.ApplyBranchFilter` (head office bypasses
    branch isolation) and `PerformanceVisibilityService.IsAdminAsync` (starts with
    `if (currentUser.IsHeadOffice()) return true`) — returning false dropped them into the **manager**
    branch of `GetAllEmployees`, which restricts to `scope.UnitIds` = own unit + descendants. Fix:
    resolve the employee's branch flag in the same projection (`e.Branch != null && e.Branch.IsHeadOffice`)
    and use `isHeadOffice = branchId is null || isBranchHeadOffice`. Measured on CERP tenant
    `aadb4e82…`: gibril (Finance Unit) and medhanit (Human Resource Unit) went 3/10 → 10/10 visible
    employees, admin (CEO) 5/10 → 10/10.
    **Two coupled fixes in the same path (the primary fix is unreliable without them):** (a) that lookup
    ran through the repository's tenant/branch filters, which at login still read the *previous*
    session's cookies — a stale `BranchId` made it return no row, collapsing to "no branch" and silently
    granting head-office access to the next user to log in; it now uses `GetAllWithoutTenantFilter()`
    with an explicit `e.TenantId == user.TenantId` re-assertion (tenant isolation unchanged).
    (b) logout never cleared `BranchId`/`IsHeadOffice` — both cookie names added to the delete lists in
    `LogoutCookieHandler` and `LogoutUser`.
    Verified live (login A/B reading the issued cookie): employee on the head-office branch → `true`
    (was `false`); on a regular branch → `false` (still correctly scoped, not a blanket grant); no
    branch → `true` (unchanged). The A/B temporarily repointed the `demo` test employee's branch;
    baseline captured and restored, branch distribution confirmed identical afterwards (NULL 9 /
    Corporate 7). NOTE (unchanged, by design): selecting a tree node lists employees assigned **directly**
    to that unit, not its descendants — select the root "All Units" for everyone.
    NOTE: the dashboard UI redesign attempted this session was rejected by the user and reverted; it is
    parked in `stash@{0}` ("discarded dashboard UI redesign"), NOT committed. Useful finding from it:
    this app does **not** feed its palette into Tailwind's theme — every colour utility
    (`bg-primary/10`, `border-success/20`, …) is hand-written in `frontend/src/config/theme.css`, so an
    invented step like `bg-secondary/40` or `bg-border` compiles to nothing and renders transparent.

00DB. **HRMS home dashboard rebuilt: aggregated summary endpoint + lazy/memoized widget split
    (2026-08-04, migration `AddDashboardSummaryIndexes`, applied to CERP).** The dashboard
    (`pages/home/dashboard.tsx`) fired 12 separate `useQuery` calls on mount, four of which were full
    paginated `GetAll?take=1` list requests just to read `.total` for a KPI count. Replaced with **one**
    aggregated `GET /Dashboard/summary` (`App/Features/Core/Dashboard/IDashboardSummary` +
    `Inf/Common/DashboardSummaryService`) — a single Dapper `QueryMultipleAsync` round trip (7
    statements: branch/orgUnit/position/employee counts, workflow Running/Approved/Rejected via GROUP BY,
    probation count, retirement count) reusing the ambient EF connection, same pattern as
    `ReportExecutor`. Tenant+branch isolation is replicated in C# to match `Repository.ApplyBranchFilter`
    **exactly** (Branch filtered by own `Id` when branch-scoped since it isn't `IBranchScoped`;
    OrgUnit/Position/Employee filtered by `BranchId`; `WorkflowInstance` is tenant-only — it has no
    `BranchId` at all). Verified field-by-field against the old endpoints for the same tenant before
    cutover (all 9 numbers matched). Two new indexes added (neither table had a tenant-scoped index for
    these queries before): `hrmsWorkflowInstance(TenantId, Status)`, `hrmsEmployee(TenantId, BranchId,
    EmploymentStatus)` — the workflow one required an `ALTER COLUMN TenantId nvarchar(max)→nvarchar(450)`
    (EF requires bounded index-key columns); confirmed safe first (`MAX(LEN(TenantId))`=36 on live data).
    Frontend split into 6 independently `React.lazy` + `memo()`'d widgets under `components/dashboard/`
    (KpiOverviewWidget, WorkflowActivityWidget, WorkforceWatchlistWidget, ActionQueueWidget,
    RecentActivityWidget, QuickAccessWidget), each behind its own `Suspense` with a skeleton
    dimension-matched to its real content (zero CLS). The 3 decision modals (workflow approve/reject,
    clearance, profile-change) — previously all in one 1067-line component's top-level state, so any
    keystroke re-rendered the whole page — now live entirely inside `ActionQueueWidget`'s local state.
    Probation/retirement tabs (`WorkforceWatchlistWidget`) get their badge counts from the aggregate and
    fetch the row-level list **only for the active tab** (`enabled: activeTab === key`) — previously both
    lists fetched unconditionally regardless of which tab was visible. Shared hooks
    (`useDashboardSummary`, `useActionQueues`) let multiple widgets call the identical queryKey — React
    Query dedupes to one network call with zero prop-drilling. E2E via Playwright against real API+DB
    (demo tenant): 9/9 — KPI numbers correct, zero console errors, exactly one `Dashboard/summary` call
    per load (confirmed no leftover count-only calls), skeleton pulses render then fully clear, tab-switch
    does not refetch the aggregate. Measured warm response: 6ms (small dataset; every query is an index
    seek, not a scan). `tsc -b` + `eslint --quiet` clean both repos-side.

00SG. **Stale-form guard sweep (2026-07-31, no migration; uncommitted).** The user-form stale-Add fix
    (00UE) replicated across EVERY id-driven CRUD form via codemod: 33 `formData/setFormData` forms +
    13 `meta`-pattern master-detail forms (resets mirror the record-populate effect's setters, each to
    its own useState initializer) in HRMS, + 4 hosted copies in the Home repo (disciplinaryCase,
    employeeGoal, transferRequest, hiringRequest). Marker comment: "stale-form guard". Analysis:
    `EntityModuleShell` renders `showForm ? form : list` (forms UNMOUNT when hidden) and org-unit/
    position use `{showForm && …}`, so most screens were safe by construction — the guard makes the
    invariant LOCAL so future parent/mounting refactors can't reintroduce the bug. Deliberately
    skipped: `appraisal/scoring.tsx` + `calibration/workspace.tsx` (form-slot work surfaces with no
    Add/empty-id mode). Spot-checked live: Operation modal edit→close→Add opens blank. ALSO fixed all
    3 react-compiler lint ERRORS repo-wide: offerLetterTemplate `useState(Date.now())` → lazy
    initializer; rolePermission/detail memo deps → extracted `scope?.subsystemId/moduleId` locals so
    reads match deps exactly; dynamicForm/DynamicFormSection spread-in-deps useMemo → removed the
    manual memo (the React Compiler auto-memoizes with precise deps). `eslint --quiet` = 0 errors in
    BOTH repos (41 auto-fixable warnings remain in HRMS — untouched, style-level).

00UE. **User admin: Edit button + stale-Add fix (2026-07-31, no migration).** `userList.tsx` GridAction
    had `showEdit={false}` (only Delete rendered). The reported "edit form fails to populate" was NOT a
    binding bug (GET /User/{id} + controlled inputs verified fine) — the real defect: the form component
    stays MOUNTED across list↔form switches, so pressing Add after viewing a user showed the previous
    user's values; fixed with a reset-on-id-cleared effect in `userForm.tsx`. E2E 6/6. ⚠ Other CRUD
    screens sharing the always-mounted EntityModuleShell form pattern may have the same stale-Add bug.

00AL. **Annual-leave self-service endpoints — own-only grid + dashboard balance (2026-07-30, no migration).**
    The Home portal's "Annual Leave" grid showed EVERY employee's requests because `GetAllAnnualLeaves`
    grants head-office accounts admin visibility (`IsAdminAsync` → true for branch-null users) and the
    Home self-service user is head-office. FIX: `IGetAllAnnualLeaves.GetMineAsync` (refactored `GetAsync`
    → private `QueryAsync(request, mineOnly)`) ALWAYS scopes `x.EmployeeId == scope.EmployeeId` — no
    admin/manager widening, null employee → empty — exposed at **`GET /AnnualLeave/mine`**; the Home list
    now calls it and defaults status to Pending. Also new `IGetMyAnnualLeaveBalance` → **`GET /AnnualLeave/
    my-balance`** (self-scoped balance for the active `AnnualLeaveSetting`'s fiscal year; Available =
    Entitled+CarriedForward+Adjusted−Taken, or the setting's DefaultAnnualEntitlement when no LeaveBalance
    row) for the Home dashboard widget. Proven: an admin (appraisal-HrSignOff approver) saw 7 via
    `/AnnualLeave` but only their own 3 via `/mine`.
    **REVISED (later 2026-07-30): `/my-balance` rewritten to per-type, per-active-fiscal-year.**
    `GetMyAnnualLeaveBalance` is now driven by the employee's OWN LeaveBalance rows in ALL active
    Core.FiscalYear rows (one query, joined to hrmsLeaveType), returning `MyAnnualLeaveBalancesDto
    { hasData, items[] }` where each item = FY + leave type + figures + `IsAnnual`
    (AccrualMethod==Annual); policy-default synth rows only for annual types with no row. It NEVER
    calls the throwing `ResolveAnnualLeaveTypeIdAsync` (throws on 0/>1 annual-method types — that
    was a hidden my-balance 400) and no longer hides balances when a year's policy row is missing
    or the "annual" type is misconfigured. ⚠ aadb4e82 config quirk: the type NAMED "Annual Leave"
    accrues Monthly while "Casual Leave" accrues Annual — accrual flags look swapped (user to fix
    in Leave Types admin; the widget/KPI follow the flag).

00WN. **Approval-request notifications → Home portal (2026-07-30, no HRMS migration; needs Home migration
    `AddNotificationSourceRef` on CERP).** The portal bell had no PRODUCER — the workflow engine never wrote
    to `dbo.coreNotification`. Added: `CoreNotification` plain POCO (Dom) + `CoreNotificationConfiguration`
    (`ToTable("coreNotification","dbo", ExcludeFromMigrations)`) + DbSet — HRMS WRITES the Home-owned table;
    `IPortalNotifier` (App/Common/Services) + `PortalNotifier` (Inf, stamps TenantId from ITenantService,
    SourceSubsystem="HRMS", best-effort); `IWorkflowApproverAuth.ResolveApproverUserIdsAsync` (step approvers
    → distinct Core.User ids: User/Role/Subject/Immediate/SecondLevel/UnitManager); `WorkflowService` emits
    on START + each ADVANCE (`NotifyCurrentStepApproversAsync`, Severity="Action", LinkUrl="/workflow") and
    marks read on every decision (`ResolvePortalAlertsAsync` → correlated by SourceEntityType="WorkflowInstance"
    + SourceEntityId=instance.Id). Open steps (no approvers) → no row. Appraisals only notify on START
    (module-driven advance bypasses the generic engine). E2E: single + 2-step round-trip verified in SQL.

00CA. **Central subsystem administration + Home-first entry flow (2026-07-29, migration `AddSubsystemUrl`
    applied to CERP).** Companion to the new standalone Home portal repo (`D:\Workspace\CyberErp\Home`).
    (a) `Subsystem.Url` (nvarchar 400, nullable) + DTO/validator/form/list — each `dbo.coreSubsystem` row
    carries its application URL; the landing page deep-links when the URL origin differs. Hardcoded
    `constants/subSystem.ts` DELETED — subsystems are read live. (b) **Cascading Subsystem→Module filters**
    (`GetAllRequest.SubsystemId/ModuleId`; Operation GetAll/GetById project `SubsystemId`+`SubSystem`;
    natural menu ordering) wired via shared `components/common/menuFilters/subsystemModuleFilter.tsx`
    (compact DropDownFields — non-compact labels clip in `searchBarFilters`) into Role Permissions
    (visible-rows-only scoping + check-all; permission map stays FULL so filtered saves keep hidden ticks),
    Menu Operations (server-side, `moduleGroup` = "SubSystem / Module" group key), Menu Modules, and the
    Operation form (subsystem scopes the module dropdown; GetById now returns SortOrder too).
    (c) **Entry flow:** sidebar scoped to own subsystem; dead `backToModules.tsx` finally mounted in
    `sidebar/index.tsx` ("All Modules" switcher); login → `/landing` with `state:{fromLogin:true}` and the
    landing auto-forwards when exactly ONE subsystem card is visible (ref-guarded; switcher never forwards);
    the Home portal (code `HOME`) is EXCLUDED from the picker — one-way Home→HRMS flow per user decision.
    (d) CORS += `http://localhost:5175`. (e) **Lazy-chunk resilience:** `errorBoundary.componentDidCatch`
    auto-reloads once (30 s sessionStorage throttle) on "Failed to fetch dynamically imported module" —
    React caches the rejected lazy import so "Try again" can never recover it; `main.tsx` also handles
    `vite:preloadError` for production builds. Gotcha: vite 504 "Outdated Optimize Dep" = stale
    `node_modules/.vite` cache — reloads don't fix it; delete the cache and restart vite. E2E: 21/21
    (filters), 7/7+3/3 (switcher/auto-forward), 12/12 (portal entry), recovery verified.

00RM. **Recruitment: internal candidate hire → promotion/transfer, not a duplicate employee (2026-07-29,
    no migration).** Module-review fix. `HireCandidate.HireAsync` (`CandidateLifecycleHandlers.cs`) used to
    always `Employee.Create(candidate.PersonId, …)` — for an INTERNAL candidate (whose PersonId is an
    existing employee's) that minted a SECOND employee on the person, stranding their leave/appraisal/
    movement/loan/document history. Now it branches: internal → `PlaceInternalAsync` records an
    `EmployeeMovement` against the existing employee (reuses `ISaveEmployeeMovement` + auto-applies via
    `IApproveEmployeeMovement` when no chain, mirroring the offer/requisition idiom; type auto-derived
    Promotion/Transfer/Demotion from the offer pay vs current, HR-overridable via `HireCandidateDto.MovementType`);
    external → the original path + a guard refusing a 2nd employee on an already-active person, and the
    compliance-doc gate skipped for internal. Shared `MigrateCandidateDocumentsAsync` + `CloseRecruitmentPipelineAsync`
    dedup the closure. `GetHireQueue` flags `IsInternal` + skips compliance for internal; `hireEmployee` modal is
    internal-aware (Place-Internal-Employee, Personnel-Action selector, no employee-number). E2E: internal move
    verified via SQL (1 employee/person, Completed Promotion, seat swap, salary/tenure updated, app Hired);
    external still creates a new employee. **Review advice: an internal employee changing position must go through
    EmployeeMovement, never a re-registration. Other findings (hire-queue N+1, EmployeePicker gaps, dup helpers,
    workforce-plan link, succession→movement wiring) logged but NOT done — user chose this fix only.**

00RH. **Report header — company name + three-column ISO layout (2026-07-28, no migration, frontend-only).**
    The header previously conflated identity into one slot (`headerTitle ?? companyName`), so a configured
    report header title HID the company name. Now the issuing COMPANY name always heads the block, and the
    header is a three-column layout: logo LEFT, company name (bold) + report title directly underneath CENTERED,
    date/time RIGHT. `ListExportHeader` gained a distinct `headerTitle`; `result.tsx` uses a `grid-cols-[1fr_auto_1fr]`
    header; `listExport.tsx` PDF splits into left/center/right Views and Excel merges+centres the company/title
    rows with a right-aligned date row. Verified on screen + by parsing the exported .xlsx/.pdf.


00LP. **Leave-policy fields moved LeaveType → AnnualLeaveSetting (2026-07-28, migration
    `MoveLeavePolicyFieldsToSetting` APPLIED to CERP). E2E 15/15.**
    - Dropped `DefaultAnnualEntitlement`, `CarryForwardMaxDays` (null=unlimited), `MaxConsecutiveDays`
      (null=no cap) from `hrmsLeaveType` (entity/DTO/validator/FE form+list trimmed — LeaveType is now
      just the category + intrinsic flags); ADDED all three to `hrmsAnnualLeaveSetting` (the per-FY policy).
    - Business logic rewired: `LeaveBalanceService` implicit/materialized entitlement now = the request
      FY's active setting's `DefaultAnnualEntitlement` (swapped `IRepository<LeaveType>` → `<AnnualLeaveSetting>`);
      `LeaveAccrualService.RolloverAsync` carry cap = the CLOSING FY's setting's `CarryForwardMaxDays`
      (one policy cap, no longer per-type); consecutive-day guard in both `SubmitAnnualLeave` and generic
      `SubmitLeaveRequest` reads `setting?.MaxConsecutiveDays`. FE: three `num()` fields on the
      annualLeaveSetting form + save.ts number/integer field lists updated.

00LR. **Leave-settings restructure (2026-07-28, migration `RestructureLeaveSettings` APPLIED). E2E 24/24
    + browser-verified.**
    - `AnnualLeaveSetting` lost its `LeaveTypeId` → ONE annual policy per fiscal year (unique
      `(TenantId,FiscalYearId)`); ledger/accrual resolves "the annual type" via
      `ILeaveAccrualService.ResolveAnnualLeaveTypeIdAsync()` = the single active LeaveType with
      `AccrualMethod==Annual` (0/>1 → loud ValidationException).
    - `OtherLeaveSetting.Name` (free text) → `LeaveTypeId` FK (unique `(TenantId,FiscalYearId,LeaveTypeId)`;
      DTO keeps `Name` projected from `LeaveType.Name`; UI = Leave Type dropdown).
    - New `LeaveDayCounting {WorkingDays, CalendarDays}` on OtherLeaveSetting ("Holiday & Weekend Handling"):
      CalendarDays charges `(end−start).Days+1` straight through; honored in Submit, lump-sum-end, and the
      client-side preview.
    - New **Employee Profile "Other Leave" tab** (`employee/otherLeaveSection.tsx`; `OtherLeaveList` gained
      `employeeId` scoping + hides the Employee column).

00OL. **Other Leave module (2026-07-28, migration `AddOtherLeave` APPLIED). E2E 41/41.**
    - Non-annual statutory leaves (maternity/paternity/mourning…) with STATIC position-based days — never
      accrues, never touches the annual ledger. Tables `hrmsOtherLeaveSetting` (per-FY: gender All/Female/Male,
      Standard/Managerial days, IsLumpSum, IsActive), `hrmsOtherLeave` header + `hrmsOtherLeaveDetail`.
    - Balance is DERIVED (allocation − Σ Pending+Approved), active-FY-only; lump-sum = one block covering the
      exact full allocation once/FY (`GET /OtherLeave/lump-sum-end`). Workflow key `WorkflowEntityTypes.OtherLeave`
      + seeded "Other Leave Approval" (Supervisor→HR); submit REQUIRES an active definition; Cancel is
      workflow-gated. Self-locked `/otherLeave` via `/Employee/me` + `/otherLeaveSetting` under the
      Attendance & Leave menu.

00G. **§3.12 Employee Guarantee Commitment Management, HC305–307 (2026-07-27, migrations
    `AddEmployeeGuarantee` + `WidenGuaranteeTypeForLookup` APPLIED). E2E 35/35.**
    - `EmployeeGuarantee` entity (NBE external-org guarantee commitments) + workflow trio
      (`WorkflowEntityTypes.EmployeeGuarantee`, seeded chain, `EmployeeGuaranteeWorkflowHandler`) + release
      lifecycle + dashboard chips. Screens: HR register (`employeeGuarantee/`), self-service `My Guarantees`
      (`myGuarantees/`), and a **Guarantees profile tab** with a lookup-driven Guarantee Type. Menu seeded
      into existing tenants via `SeedDefaultMenu` + `/Module/seed-defaults`.

00PT. **All Employee-Profile child tabs converted to inline (non-popup) forms (2026-07-27, FE only, no
    migration).** Education/Experience/Family/Movements/Award/Certification/Discipline/Termination tabs now
    use the same grid+inline-form layout as Guarantees: `childManager.tsx` renders `EntityListShell` with a
    `formOpen/formTitle/formView/onBack` inline mode (client search/sort/page or `paging` pass-through +
    `renderActions`), replacing the modal popups. `personBackground/{education,experience}Section` +
    `employee/{family,movement,discipline,termination}Section` rewired.

00FB. **Form Builder: multi-module support + lookup-bound Select fields + tabbed Add-Field UI (2026-07-27,
    migration `AddDynamicFormFieldLookupCategory` APPLIED).** Builder no longer restricted to the Employee
    module — a `module` selector scopes custom tabs to any owner type; a `Select`-type field can bind to a
    dynamic `LookupCategoryId` (options resolved via the centralized Lookup API). "Add Field" moved into a
    tab like the rest of the UI; static-comma + combo-selector visibility fixed.

00R. **ISO report header + Report Definition header/logo config + header-aware PDF/Excel exports
    (2026-07-24, migration `AddReportHeaderTitle` APPLIED to CERP) + editor crash fix.**
    - **Report result page** (`reportViewer/result.tsx`): header strip = tenant LOGO + header
      name + report title + **generation date** chip; sequential **"No." column** stamped after
      search+sort (rides paging/grouping/exports). `ReportResultDto` gained
      `GeneratedAtUtc`/`CompanyName` (enriched in ReportController via Inf `ITenantService` —
      App can't reference Inf) + `HeaderTitle`.
    - **Report Definition** got a "Report Header" section: per-report `Report.HeaderTitle`
      (nvarchar 200; empty ⇒ company name) + a `ReportHeaderLogo` panel managing the SHARED
      tenant letterhead (same `/DocumentTemplate/logo` endpoints as the {{Logo}} token).
    - **Exports**: `ListExportHeader {company,title,generatedAt,logoDataUrl}` threaded
      useListPage(exportHeader) → ListExportConfig.header → listExport. PDF renders a letterhead
      block (`<Image src={dataUrl}>`); Excel goes through a NEW lazy **ExcelJS** branch
      (`exportExcelWithHeader`) because SheetJS CE cannot embed images — logo at A1, bold header
      C1–C3, head row 5, data row 6+. Plain lists keep the xlsx path (bundle unaffected).
    - **App-wide fix**: `DatabaseTenantStore.GetByIdentifierAsync` now falls back to a GUID Id
      lookup — cookie/claim flows carry the tenant GUID, so Finbuckle TenantInfo
      (name/subscription) was NULL on every cookie-authenticated request.
    - **Fix**: Document Templates → Add crashed the section error boundary — a Suspense
      hide/reveal re-ran `htmlEditorField.tsx`'s value-sync effect against the DESTROYED tiptap
      instance (`getHTML()` → "reading 'cached'"); guarded with `editor && !editor.isDestroyed`.
    - GOTCHAS: `useListColumnSelection` intersects prev∩new visible columns — keep the result
      page's `columns` EMPTY until report columns arrive or a static column becomes the only
      visible one; react-pdf REJECTS malformed PNGs that ExcelJS embeds blindly.
    - E2E (browser + file parsing): definition form section, viewer header, XLSX (1 embedded
      image, bold C1 header, No.=1) and PDF (embedded image XObject) all verified.

00C. **Critical Position approval workflow (2026-07-22, BE+FE, migration
    `AddCriticalPositionApprovalStatus` APPLIED to CERP — completes the §3.7.A trio with 00S/00T).**
    UNLIKE the other two, `CriticalPosition` had only `IsActive` (no status enum) → new
    `Status` column (nvarchar(20), `HasDefaultValue(Active)` so legacy rows need no backfill).
    `CriticalPositionStatus {Active, PendingApproval, Rejected}`; transitions also drive the flag:
    **pending/rejected force `IsActive=false`** (active-only feeds exclude unapproved flags),
    approve → Active + IsActive=true. `WorkflowEntityTypes.CriticalPosition` + seed
    ("Critical Position Approval": Manager Review → HR Approval). `SaveCriticalPosition`: Save DTO
    carries NO status (fully workflow-owned); force-pending on create, `EnsureNoRunningAsync`
    gates on update/delete, resubmit-on-save of Rejected; summary uses the position Code.
    DOWNSTREAM GATE: `SaveSuccessionPlan` 400s when anchoring to a Pending/Rejected critical
    position (succession may only anchor to an APPROVED flag). `CriticalPositionWorkflowHandler`
    in DI. FE: `CriticalPositionModel.status`, list's Active column replaced by a combined badge
    (workflow state wins, else Active/Inactive from the toggle), form banners,
    `criticalPositionStatusLabels`/`Label`, "Critical Position" in `workflowEntityTypeOptions`.
    E2E `criticalposition_wf_e2e.mjs` **30/30** (IsActive forcing, anchor gate before/after
    approval, reject→resubmit→approve, role-approver inbox). NOTE: EF-tools-9-vs-runtime-10
    snapshot gotcha did NOT strike for a property add — snapshot updated normally.

00T. **Talent Review approval workflow (2026-07-22, BE+FE, NO migration — mirrors item 00S).**
    `WorkflowEntityTypes.TalentReview` + seed default ("Talent Review Approval": Manager Review →
    HR Approval). `TalentReviewStatus` gained `PendingApproval(3)`/`Rejected(4)`; transitions
    `MarkPendingApproval` / **`ApproveViaWorkflow` → InProgress** (approval opens calibration
    directly; Draft stays the pre-submission state of direct mode) / `RejectViaWorkflow` →
    Rejected. `SaveTalentReview`: same force-pending on create, `EnsureNoRunningAsync` gates on
    update/delete, resubmit-on-save of a Rejected review. EXTRA vs succession:
    **`SaveTalentAssessment` 400s while the review is PendingApproval or Rejected** — calibration
    is the review's substance, so it must not proceed under an unapproved session
    (`IRepository<TalentReview>` injected for the status probe). `TalentReviewWorkflowHandler`
    registered in DI. FE: `talentReviewStatusLabels`/`Label`, list badge tones (Pending=warning,
    Rejected=error), form banners, "Talent Review" in `workflowEntityTypeOptions`. E2E
    `talentreview_wf_e2e.mjs` **29/29** (direct mode, forced pending, edit/delete/assessment
    gates, approve→InProgress→assessment OK, reject→blocked→resubmit→approve, role-approver inbox).

000. **MEGA-COMMIT (2026-07-22): everything built 2026-07-16 → 2026-07-22 committed as one batch**
    (~600 files). Modules, each E2E-verified in its session: **§3.10 Compensation & Benefit +
    Medical Benefit + Insurance + Loan + Trip** (masters, lifecycles, HR→Finance→Exec workflow
    chains, CB3 deductions-engine feed, Hangfire settlement reminders, self-service screens);
    **§3.8 Training** (needs w/ per-type workflow, sessions/enrollments/budgets, learning paths,
    certificates, CPD, provider payments, communities); **§3.9 Engagement** (anonymous-safe
    suggestions, grievances, targeted announcements, surveys) + **§3.9.3 Disciplinary cases**
    (eligibility service + reward/promotion hard-block gates); **§3.7.3 Employee Transfer**
    (deferred execution, assessment endpoint, transfer notices) + **§3.7.4 Reward & Recognition**
    (nominations w/ workflow, points ledger, disbursements, recognition wall); **strict RBAC**
    (deny-by-default menu, `[RequirePermission]` opt-in endpoint filter w/ 60s cached
    `EndpointPermissionService`, `IPerformanceVisibilityService` data scoping); **dynamic
    navigation** (`SeedDefaultMenu`); **standard report catalog** (13 seeded SP-driven reports +
    modernized viewer: tree first-open expands only first group, Save `#63d91d` / Email
    `#eea522`); **Role Permissions matrix rebuilt** self-contained (flat, client-side roleId
    filtering, direct JSON save); **10k-user performance pass** (9 N+1 batch fixes, lazy
    xlsx/react-pdf/tiptap, EmployeePicker swaps, `IX_User_UserName`); **Talent Review →
    Succession bridge** (candidate-profile talent outcome row + HiPo suggested-candidates
    endpoint/chips); **Succession Plan approval workflow** (next item + `logic.md` §1).
    Committed on `feature/hrms-buildout`; NOT yet pushed.

00S. **Succession Plan approval workflow (2026-07-22, BE+FE, NO migration).**
    `WorkflowEntityTypes.SuccessionPlan` + seed default ("Succession Plan Approval": Manager
    Review → HR Approval). `SuccessionPlanStatus` gained `PendingApproval(3)`/`Rejected(4)`
    (string-stored, ≤20 chars → no migration) + transitions `MarkPendingApproval` /
    `ApproveViaWorkflow` / `RejectViaWorkflow` (idempotent, only from PendingApproval).
    `SaveSuccessionPlan`: when an active definition exists, create/resubmit FORCES status to
    PendingApproval (`EnsureStartableAsync` pre-persist, `StartIfDefinedAsync(employeeId:null)` —
    a plan is position-scoped, no subject); update/delete gated by `EnsureNoRunningAsync`; saving
    a **Rejected** plan resubmits it (approval outcomes are workflow-owned, no hand-flip to
    Active). `SuccessionPlanWorkflowHandler` (approve→Active, reject→Rejected) registered in DI.
    FE: status badge tones + `successionPlanStatusLabels`/`Label` (selectable options stay the 3
    operational ones), pending/rejected info banners on the form, and
    `workflowEntityTypeOptions` (constants/orgStructure.ts) **synced with the full backend
    registry** (was missing 11 newer processes). E2E `succession_wf_e2e.mjs` 25 checks + 8-check
    role-approver inbox rerun — all pass. GOTCHA: My Approvals only lists steps that NAME the
    user; open steps (no approvers) are actionable from `/workflow` but never appear in the inbox.

00. **Employee & Candidate Education/Experience unified via a SHARED component (BE+FE, NO migration).**
    The two modules now render the SAME section components, so the forms are identical and custom
    fields defined once reflect in both. **Uncommitted.**
    - **Shared FE components** `components/common/personBackground/{educationSection,experienceSection}.tsx`
      + `types.ts` (`BackgroundDataSource<T>` adapter: ownerId/queryKey/list/save/remove/ownerIdField/
      renderAttachments/readOnly/hint). The employee (`admin/employee/*`) and candidate
      (`admin/candidate/*`) section files are now **thin wrappers** passing their adapter — the fields,
      columns, External/Governmental toggle rows, custom fields (HC021) and attachments live once.
    - **Custom fields reflect automatically:** definitions are OwnerType-scoped (Education/Experience),
      not module-scoped, so the shared `useCustomFields("Education"/"Experience")` shows the same fields
      in both, and values live on the same person-owned record. Candidate handlers
      (`CandidateBackgroundHandlers.cs`) gained `CustomFields` on the 4 DTOs + `ICustomFieldService`
      Apply/GetForOwners/Delete (mirroring the employee handlers).
    - **Candidate Experience now identical:** DTOs gained `IsExternal`/`IsGovernmental`; the save
      **honors them** (stopped forcing `isExternal:true`); Get projects them; the External/Internal/Gov
      badge column + toggle-row UI now render for candidates too.
    - **Save unified on `createSaveService`+FormData:** candidate save switched off the bespoke
      `saveCandidateChild` JSON helper to `createSaveService(\`Candidate/{id}/education|experience\`, …,
      { customFields:true, method:"POST" })` — added a **`method` override** to `createSaveService` so
      it can force POST to the candidate upsert endpoint (no backend route change). New candidate zod
      schemas (no employeeId, which rides in the URL). Candidate get services now return the shared
      `EmployeeEducationModel`/`EmployeeExperienceModel`.
    - E2E 14/14 (shared def reflects on candidate edu+exp, isExternal honored incl. false, governmental,
      required→400, unknown→400). Both build; test tenants purged.

0. **SMTP credentials moved to .NET user-secrets (config/ops, no code change).** `Email:UserName` /
   `Email:Password` are now in user-secrets (UserSecretsId `5d5ac854-…` on `CyberErp.Hrms.Api`, loaded
   automatically in Development) — set them with `dotnet user-secrets set "Email:UserName" <v>` /
   `"Email:Password" <v>` from `backend/CyberErp.Hrms.Api`. Committed `appsettings.json` keeps the
   non-secret Email structure with **empty** UserName/Password placeholders (real values come from
   user-secrets locally, env vars elsewhere). The Gmail app password that had been sitting in
   `appsettings.json` was scrubbed before it ever hit git history. **The earlier §1 buildout is now
   committed** (`23a2169` on `feature/hrms-buildout`).

1. **Dynamic Form / Tab Builder (SAP/Dynamics-style custom tabs) — new reusable subsystem**
   (BE+FE; migrations `AddDynamicForms` + `IndexDynamicFormRecordCreatedAt` **applied to CERP**; both
   build; E2E 22/22 + paging 6/6 then purged. **Uncommitted.**)
   - **Perf hardening (server-side paging):** `GetRecordsAsync` returns `PaginatedResponse` (skip/take)
     — the server bounds the fetch + JSON parse to one page; record index extended to
     `(DynamicFormId,OwnerType,OwnerId,CreatedAt)` so ordered pagination is index-supported (no sort).
     `getRecords(…,param)` is paged; `keepPreviousData` for smooth transitions.
   - **UI consistency (2026-07-13):** the record grid renders with the **same building blocks as the
     fixed employee child tabs** — shared `ChildManager` table + modal `FormProvider` + the standard
     `Pagination` component (shown only when a form exceeds one page, so small collections look exactly
     like Education/Experience). This **replaced** an interim `DataTableProvider isVirtual` render whose
     heavy VirtualDataTable chrome looked nothing like the rest of the app. Paging is preserved (page
     size 15 bounds fetch+DOM); explicit windowing was dropped as unnecessary at page scale.
   - **Attachment fields (2026-07-13, NO migration):** a new **`Attachment`** field type (Form Builder
     only — `dynamicFormFieldTypeOptions`) reuses the EXISTING `EmployeeDocument` subsystem exactly like
     Education/Experience. `EmployeeFieldDataType.Attachment` + `EmployeeDocumentOwner.DynamicFormRecord`
     (both string-stored enums → no schema change). `EmployeeDocument` Upload/Get handlers gained a
     DynamicFormRecord case (guarded via the record's employee). `DynamicFormService`: Attachment fields
     excluded from Data validation/storage; record delete cascade-deletes its docs
     (`DocumentStorage.DeleteForOwnerAsync`); record DTO gained `DocumentCount` (paperclip grid column).
     FE `DynamicFormSection` splits Attachment fields → the shared `DocumentAttachments` panel (edit-mode
     only, "Save first" hint on new) + a paperclip count column. E2E 12/12 (upload/list/count/download/
     bad-type-400/guard-404/cascade). ⚠ coupling: the module-agnostic `DynamicFormService` now depends on
     `EmployeeDocument` (employee-specific) — acceptable for the Employee-scoped v1.
   - **Per-field attachment pools (2026-07-13, migration `AddEmployeeDocumentOwnerField`):** each
     Attachment field now has its OWN file pool. Added nullable `EmployeeDocument.OwnerField` (the
     dynamic-form field name; null for education/experience). Upload/Get/count all scope by
     `(OwnerType,OwnerId,OwnerField)`; controller + `documents.ts` thread an `ownerField`; record delete
     still cascades ALL fields (delete is by OwnerType+OwnerId). Record DTO `DocumentCount`→
     `DocumentCounts` (dict field→count, one grouped query/page). FE renders one `DocumentAttachments`
     panel per Attachment field (`ownerField`+`title` props) + per-field paperclip counts. E2E 15/15
     (2 isolated pools, no cross-contamination, per-field counts, scoped delete, full cascade, Education
     regression).
   - **Storage = JSON document column, NOT EAV** (perf decision): 3 tables `hrms_DynamicForm` (tab def)
     → `hrms_DynamicFormField` (schema, reuses `EmployeeFieldDataType`) + `hrms_DynamicFormRecord`
     (one row/record, values in a single `Data nvarchar(max)` JSON `{field:value}`). Hot path (list a
     form's records for one owner) = **single indexed range scan** on `(DynamicFormId,OwnerType,OwnerId)`.
   - **Reusable/module-agnostic:** keyed on string `Module` + polymorphic `OwnerType`/`OwnerId` (same
     pattern as HC021 values / EmployeeDocument). Other modules just render `<DynamicTabs module=…/>`
     and define forms with that `Module`.
   - **Backend** slice `App/Features/Core/DynamicForms/`: `IDynamicFormService` (GetActiveForms/GetAll/
     GetById/SaveForm/DeleteForm + record Get/Save/Delete). Record save validates `Data` against the
     form's active fields (unknown→400, required→400) then stores compact JSON via System.Text.Json.
     Form save mirrors the `ClearanceDepartment` children pattern (child-field **TenantId stamp** +
     explicit AddAsync on update; old fields deleted first). Delete-form guard: 400 if records exist.
     2 controllers (`DynamicFormController` + `DynamicFormRecordController`). Repos = open-generic
     `IRepository<>`; service registered in App DI.
   - **Frontend** generic components `components/common/dynamicForm/`: `DynamicFormSection`
     (metadata-driven generalization of `childManager` — grid from `ShowInList` fields + FormProvider
     modal via the now-generalized `buildCustomFieldComponents(RenderableFieldDef[])`, bespoke JSON
     save from `values` state), `useDynamicForms(module)` (React-Query cached), `DynamicTabs` (standalone
     bar for other modules). Services `services/admin/dynamicForm/index.ts`.
   - **Employee integration:** `profile.tsx` — `tab` widened to string; custom tabs appended to the tab
     bar from `useDynamicForms("Employee")`, each rendering `<DynamicFormSection ownerType="Employee">`.
   - **Admin "Form Builder"** screen `/formBuilder` (System group): `EntityModuleShell` + list
     (`getAllForms`) + a hand-rolled editor (tab meta + repeatable field-row editor). Route + sidebar added.
   - **Note (v1 scope):** the generic record service is NOT branch-visibility-filtered (records aren't
     `IBranchScoped`) — tenant-isolated only; branch scoping is a future refinement. `GetAllRequest`
     gained `Module`; `ParameterModel` gained `module`.
2. **Custom-field engine (HC021) extended from the Employee form to ALL 6 employee child forms**
   (Education, Experience, Family, Movement, Discipline, Termination). Backend + frontend; migration
   `GeneralizeCustomFieldsToChildForms` **applied to CERP**; both halves build; E2E-verified then purged.
   **Uncommitted.**
   - **Domain:** new enum `EmployeeFieldOwnerType` (Employee/Education/Experience/Dependent/Movement/
     Discipline/Termination) + `OwnerType` on `EmployeeFieldDefinition`. `EmployeeFieldValue` made
     **polymorphic**: `EmployeeId`→`OwnerId` + `OwnerType`, **cascade FK dropped** (like
     `EmployeeDocument`) — each owner's delete handler now cleans up its values.
   - **Shared service** `ICustomFieldService`/`CustomFieldService` (`Features/Core/EmployeeFields/`):
     `ApplyAsync`/`GetValuesAsync`/`GetValuesForOwnersAsync`(bulk, avoids N+1)/`DeleteForOwnerAsync`.
     **ApplyAsync stages only** (no SaveChanges) so record+values commit atomically in one txn (the
     record's `Id` exists pre-save via `BaseEntity` ctor). `EmployeeHandlers` refactored onto it
     (+value cleanup on employee delete, since the cascade FK is gone).
   - **Child slices:** each Save/Get/Delete handler gained `CustomFields` on its DTOs +
     ApplyAsync(create&update)/GetValuesForOwnersAsync(list)/DeleteForOwnerAsync(delete). Definitions
     scoped by `OwnerType`: unique `(TenantId,OwnerType,Name)`; `EmployeeField` GetAll takes
     `?ownerType=`; name-dup check scoped by owner.
   - **Frontend:** admin "Employee Fields" screen → **"Custom Fields"** with an **"Applies To"**
     dropdown + column (`fieldOwnerTypeOptions`/`ownerTypeLabel`, "Dependent"→"Family"). Shared
     `buildCustomFieldComponents` (extracted from `masterForm`, which now reuses it) + `useCustomFields`
     hook drive every child form: fetch scoped defs → render `cf_`-prefixed fields into the
     FormProvider grid (with an "Additional Information" divider) → `createSaveService({customFields:
     true})` gathers `cf_*` FormData keys into a nested `customFields` dict. Works because DropDownField
     posts a hidden named input. Models gained `customFields`.
   - **Gotcha:** the new `OwnerType` columns backfill existing rows to `'Employee'` via the migration's
     `defaultValue` (hand-set from `""`) so pre-existing Employee custom fields keep working.
   - **Follow-up fix:** the Employee master form's `activeFieldParam` was still fetching defs with NO
     `ownerType`, so every form's fields leaked onto the Employee form — added `ownerType: "Employee"`
     (`masterForm.tsx`). The 3 def consumers now: masterForm→Employee, child forms→their owner (hook),
     admin list→unscoped (shows all, by design).
2. **Experience form: IsExternal now visible+editable + checkbox styling matched to Employee form**
   (backend + frontend; **no migration** — `IsExternal` column already exists. **Uncommitted.**
   ⚠️ **Requires API restart** to pick up the backend change — DLLs were locked by a running VS/IIS
   Express instance at build time, so only the OLD backend is live until restarted.):
   - `EmployeeExperienceHandlers.cs`: `SaveEmployeeExperienceDto` gained `IsExternal`; the Save
     handler now uses `dto.IsExternal` on both Create and Update instead of hard-coding `true`. The
     movement auto-registration path (`EmployeeMovementHandlers`, internal=false) is unchanged.
   - `experienceSection.tsx`: replaced the generic `type:"checkbox"` (which looked nothing like the
     Employee form) with a single `type:"custom"` field rendering TWO Employee-form-style **toggle
     rows** (border + icon + title + helper — copied from `masterForm.tsx`'s managerial toggle) for
     **External employment** (`Building2` icon) and **Governmental organization** (`Landmark`). The
     `<input name="isExternal/isGovernmental">` post through the form's FormData. New entries default
     `isExternal:true` (set in `open(null)`). Info note reworded (External is now a toggle).
   - `children.ts`: `saveExperience` booleanFields now `["isExternal","isGovernmental"]`.
   - Pattern note: to inject bespoke markup into a FormProvider grid, use `type:"custom"` +
     `customChildren` + `colSpan:"full"` (CustomField renders children as-is, no FieldShell/label).
2. **Employee child-form redesign (Education/Experience/Family/Movement/Discipline/Termination)**
   (frontend only, no migration; builds clean. **Uncommitted.**):
   - Added opt-in **`FormModel.fieldLayout`** (typed `FormComponentModel["layout"]` — NOT FieldLayout,
     which includes "horizontal" the component type excludes) applied in BOTH FormProvider grid
     mappings as `formColumn.layout ?? form.fieldLayout` (article path) — non-opt-in forms unchanged.
   - The 6 modal forms swapped `labelWidth:"w-[35%]"` (horizontal labels) → `fieldLayout:"auth"`
     (clean label-above-input, tiles 2-up because the auth FieldShell's `col-span-full` is inert in
     FormFieldRenderer's `min-w-0` cell) + a one-line modal `description`. Matches the earlier master
     form's field style for consistency.
   - Fixed `CheckBoxField`: a SINGLE checkbox showed its label twice (shell + inline) — now
     `hideLabel` when single (group keeps the shell label as its title). Improves all single checkboxes.
2. **Employee Movement → SalaryScale + salary rules + auto-experience; Experience IsExternal/IsGovernmental**
   (migration `MovementSalaryScaleAndExperienceFlags` applied to CERP; E2E candbg27 all green.
   **Uncommitted.**):
   - SCHEMA: `hrms_EmployeeMovement` From/ToJobGradeId → From/ToSalaryScaleId (⚠ hand-edited migration:
     scaffolder RENAMED grade→scale which would carry grade-ids into the FK → changed to DROP+ADD
     null); FK `ToSalaryScaleId → coreSalaryScale` (Restrict) + index; From is a snapshot (no FK).
     `hrms_EmployeeExperience` +IsExternal +IsGovernmental (bit, default 0).
   - Salary rule (domain Guard + validator): a Transfer may NOT set ToSalary or ToSalaryScaleId —
     pay changes only on Promotion/Demotion. E2E: transfer+salary → 400.
   - `Employee.ApplyMovement` now applies `salaryScaleId` (Promotion/Demotion); execute uses it. E2E:
     promotion→execute set employee scale=S2 + salary=9000.
   - Auto-experience on execute (`ExecuteEmployeeMovement.RegisterInternalExperienceAsync`): records
     the FROM role as INTERNAL experience (IsExternal=false; org=CompanyProfile name ?? "Internal";
     title=from-position ?? "Employee"; start=prior movement/hire, end=effective date). Added
     EmployeeExperience + CompanyProfile repos. E2E: 1 internal row auto-created.
   - Experience: manual save (employee + candidate) forces IsExternal=true; IsGovernmental from DTO.
     E2E: manual row isExternal=true, isGovernmental=true.
   - FE: movement form grade→scale filter (getAllSalaryScale scoped to grade) + auto-fill toSalary;
     scale/salary shown ONLY for Promotion/Demotion; MovementChange + model use salaryScaleName;
     saveMovement numberFields[toSalary]. Experience form: IsGovernmental checkbox (booleanFields) +
     External/Internal/Gov badges + "external" note. Models updated.
2. **Employee form redesign + IsManagerial field** (no migration — `IsManagerial` column already
   existed on `hrms_Employee`, just never wired to the form; E2E candbg26 create true→read true,
   update false→read false, DB col=0. **Uncommitted.**):
   - Backend: `IsManagerial` added to Create/Update EmployeeDto + read `EmployeeDto` + projection;
     `entity.SetManagerial(dto.IsManagerial)` in both create + update handlers. (`Employee.SetManagerial`
     already existed.)
   - Frontend: `EmployeeModel.isManagerial`, `save.ts` coerces it to a real boolean (like
     isProbation), `EmployeeSchema` gains the optional field.
   - `masterForm.tsx` REWRITTEN — dropped the flat FormProvider grid for a card-per-section layout
     (identity header w/ photo + live name preview + status/managerial/probation badges; sticky
     Save bar with `type=submit form=employeeMasterForm`; SectionCards Personal/Contact/
     Identification/Employment/Additional). REUSES the shared `FormFieldRenderer` per field with
     `layout:"auth"` (label-above-input) — keeps the searchable position/salary-scale DropDowns and
     validation. ⚠ Key insight: the auth-layout FieldShell has `col-span-full` but it's INERT
     inside FormFieldRenderer's `min-w-0` cell wrapper, so fields still tile 2-up in a
     `grid sm:grid-cols-2`; pass `colSpan:"full"` for full-width (location textarea + managerial row).
   - Managerial control is a hand-rolled prominent checkbox row (accent checkbox + icon + helper
     text), bound via `onChange e.target.checked`; header badge reflects it live.
2. **Large-scale performance pass** (migration `AddPerformanceIndexes` applied to CERP; measured
   before/after on a SQL-seeded 2,000-applicant vacancy [clone-template trick: create 1 via API,
   dynamic-SQL multiply ×1999 excluding rowversion/computed cols]; scripts `perf-seed.sh` /
   `perf-measure.sh` in scratchpad. **Uncommitted.**):
   - RESULTS: list 1.1–3.0 s → **0.13–0.19 s**; ranking 0.44 s → 0.23 s; hire-queue 1.3 s → 0.32 s;
     ranking payload 1.28 MB → 248 KB (brotli).
   - `RankingShared.ComputeEligibilityAsync` — NEW set-based eligibility (3 no-tracking projection
     queries over ALL page requisitions; uses stored ScreeningScore + mandatory-fail set + latest
     offers) feeding the SAME AssignRanksAndEligibility → identical values; GetAllJobApplications
     no longer calls the full ranking per requisition (dropped IGetApplicationRanking dep, added
     score/offer repos).
   - `AssignRanksAndEligibility` O(N²)→O(N log N): sort once + walk score tiers (competition rank
     accumulator; per-tier strictlyAhead; Tied = tier size>1). Semantics unchanged (E2E values
     verified: 10-way tie at top all rank 1/Eligible with 3 positions).
   - AsNoTracking: ranking hydration (Include CriterionScores), LatestOffersAsync, eligibility
     queries, hire-queue docs. ⚠ `Repository.GetAll()` TRACKS by default — opt out on read paths.
   - Hire-queue compliance docs: one batched query per vacancy pool (was per-candidate N+1).
   - Indexes: JobApplication (TenantId, AppliedAt); JobOffer (ApplicationId, CreatedAt) [the
     existing ApplicationId index is FILTERED to active statuses — couldn't serve latest-offer scans].
   - Api: `AddHrmsResponseCompression` (Brotli+gzip, Fastest, EnableForHttps) + UseResponseCompression
     first in pipeline (in HangfireConfiguration.cs file).
   - FE: QueryClient defaults staleTime 30 s / refetchOnWindowFocus false / retry 1 (main.tsx).
   - **Module-by-module audit (follow-up request):** Employee / Dashboard / Termination /
     EmployeeField / DocumentTemplate / OrganizationUnit / Position / PositionClass reviewed
     handler-by-handler. VERDICT: already projection-based paged queries with batched lookups
     (dashboard KPIs use take:1 count probes; probation/retirement widgets are SARGable
     projections; MyApprovals + WorkflowStats batched/grouped; org tree = single projection +
     in-memory build; termination list = single roundtrip w/ correlated latest-case subquery).
     Two real fixes applied: **DocumentTemplate list projection no longer ships
     Body/HeaderHtml/FooterHtml** (tens of KB per row; editor loads by id — new `ListProjection`,
     Body = "" for contract compat) and **GetEmployeeTerminations AsNoTracking**. All module
     endpoints smoke-tested 200 (candbg25); template byId still returns the full body. These
     modules also inherit the GLOBAL wins: response compression, React Query staleTime, and the
     existing indexes already cover their sorts ((TenantId, EmployeeNumber) unique etc.).
2. **Hangfire background e-mail dispatch** (packages Hangfire.AspNetCore/SqlServer 1.8.23 [Api] +
   Hangfire.Core [Inf]; NO EF migration — Hangfire auto-creates 11 tables in CERP schema
   `HangFire`; E2E candbg22/23 green. **Uncommitted.**):
   - *Design — compose in-request, send in background:* `IEmailService` → NEW `QueuedEmailService`
     (Inf): cheap guards in-request (no recipient / Email:Enabled=false → false, nothing enqueued —
     preserves offer stays-Approved semantics), else enqueues `EmailDispatchJob` with the FULL
     payload (attachments as List<EmailAttachment>, byte[]→base64 in job args) and returns true =
     "durably queued". Job is tenant-free by design (background jobs have no Finbuckle context) —
     all tenant-scoped composition (candidate/letter/PDF) stays in the request.
   - *Job:* `EmailDispatchJob` (Inf) resolves `SmtpEmailService` (now registered as itself), throws
     on failed send → `[AutomaticRetry(5, delays 60/300/900/3600/7200s)]`; re-checks Enabled at
     dispatch (config drift = drop, not retry). SEMANTIC CHANGE: approved offers mark **Sent on
     enqueue** (durable retries) instead of on synchronous delivery; controller Send message reworded
     ("queued for delivery").
   - *Config:* `Configuration/HangfireConfiguration.cs` — `AddHrmsBackgroundJobs` (chained in
     Program.cs before AddInfrastractureServices): SqlServerStorage w/ SlidingInvisibilityTimeout=5m
     + QueuePollInterval=Zero + UseRecommendedIsolationLevel + DisableGlobalLocks +
     CommandBatchMaxTimeout=5m; server WorkerCount=Clamp(cores,2,4). Dashboard `/hangfire` in
     `UseHrmsMiddlewarePipeline` after UseAuthorization; `IDashboardAsyncAuthorizationFilter` must
     `AuthenticateAsync("Cookies")` EXPLICITLY (default scheme is JWT → `User` never populated from
     the cookie outside controllers; plain IsAuthenticated check 401'd even when logged in).
   - E2E: 11 HangFire tables; anonymous /hangfire 401, cookie-authed 200; interview schedule 200 in
     0.43 s with e-mail delivered by job; offer submit → Sent immediately + PDF e-mail by job;
     HangFire.Job states: Succeeded=2.
2. **Tied-score ranking fix — no hidden tie-break, co-eligible ties** (no migration; E2E candbg21
   green. **Uncommitted.**). ROOT CAUSE: `GetApplicationRanking` sorted only by
   `OrderByDescending(TotalScore)` (stable) → tied rows kept the arbitrary DB/`Guid`-PK return
   order, and `AssignRanksAndEligibility` handed out Rank 1,2,3 + top-N Eligible in that order, so
   one tied applicant was silently Eligible and the rest Waitlisted. FIX in `RankingShared.AssignRanksAndEligibility`:
   - Standard-competition **Rank** (`1 + #strictly-higher`; ties share a rank).
   - **Tie-safe eligibility:** Eligible ⟺ `strictlyAhead < openSlots` (order-independent) → a tie at
     the cut-off makes ALL tied members co-eligible; HR picks (fill-close/hire-gate still cap hires).
   - Deterministic **display** order in `GetApplicationRanking`: TotalScore desc → AppliedAt asc →
     CandidateNumber (no arbitrary DB order). Added `AppliedAt` + `Tied` to `ApplicationRankingRowDto`.
   - FE: `ApplicationRankingRowModel` +appliedAt/tied; RankingModal "TIED" badge + co-eligible banner
     + updated description.
   - E2E candbg21: 1 pos/3×80 → all rank 1, tied, Eligible; 90/80/70 → only 90 Eligible; 2 pos/90/80/80
     → 90 + both 80 co-eligible. Note: could let HR over-offer to multiple co-eligible tied candidates
     (pre-existing multi-eligible concern; hire fill-close caps actual hires) — flagged, not expanded.
2. **Evaluator permissions made STRICT (visibility + adopt gate + score-sheet restriction)** (no
   migration; E2E candbg20 green. **Uncommitted.**). The prior increment only blocked at write-time
   and was invisible in the UI, so an evaluator still SAW every applicant → felt unenforced. Now:
   - `EvaluationGuard.GetContextAsync(users, evaluators, requisitions, userId)` → EvaluatorContext
     (employeeId, IsConstrained, AssignedCriterionIds, AssignedRequisitionIds). Constrained = a
     logged-in employee assigned as a `CriterionEvaluator` anywhere.
   - **Read filter:** `GetAllJobApplications` now injects User/CriterionEvaluator/CurrentUser and
     filters the pipeline to the evaluator's assigned requisitions — they see ONLY their applicants.
     E2E: evaluator list = [their R1 applicant only]; HR list = both.
   - **Adopt bypass closed:** `AdoptInterviewScores` now runs `EnsureMayScoreAsync` over the adopted
     criteria (was write-gate-free).
   - **UI:** new `GET JobApplication/evaluator-context` (`IGetEvaluatorContext`/`GetEvaluatorContext`,
     DI registered) → frontend `getEvaluatorContext`; ScoreModal gets `restrictToCriteria` (shows
     only the evaluator's criteria); "Evaluator view" chip on the Applications header.
   - ⚠ STILL requires the User↔Employee link (User form → "Linked Employee"). Unlinked account =
     HR (unconstrained). This is almost certainly why the user saw "not working" — the evaluator's
     login wasn't linked to their employee. Flag this to the user.
   - E2E candbg20: evaluator-context isConstrained=true/1 req/1 criterion; own criterion 200,
     unassigned 400; list filtered.
2. **User↔Employee relationship restructure — FK moved to User, User.BranchId/IsHeadOffice
   removed** (migration `ReverseUserEmployeeRelationship`, applied to CERP; E2E candbg17/19 green.
   **Uncommitted.**):
   - Reversed the link the previous increment added: dropped `Employee.UserId`; added nullable
     **`User.EmployeeId`** with a real FK → `hrms_Employee` (SET NULL). `User.LinkEmployee`;
     removed `Employee.LinkUserAccount`. ⚠ Hand-edited the scaffolded migration: EF tried to
     RENAME BranchId→EmployeeId — changed to DROP BranchId + ADD fresh null EmployeeId (a rename
     would have carried branch-ids into the FK and broken it).
   - **Removed `User.BranchId` + `User.IsHeadOffice` columns** + `MarkAsHeadOffice`/`AssignBranch`
     (and the two `RegisterRepository` calls). Branch scope + head-office are now **derived at
     login** (`LoginRepository`): branchId = linked employee's BranchId; isHeadOffice = (no branch).
     The `BranchId`/`IsHeadOffice` cookies + `UserResult` fields are unchanged downstream, so
     `CurrentUserService` / branch isolation still work. E2E: branch-employee user → isHeadOffice
     false + branchId set; owner (no employee) → head office.
   - Evaluator resolution now via `User.EmployeeId` (EvaluationGuard). `SaveUserDto`/UserDto +
     user-management handlers carry EmployeeId; **user form** gained a "Linked Employee" dropdown;
     employee-form login dropdown removed; EmployeeModel.userId removed, UserModel.employeeId added.
     E2E: evaluator scores own C1=200, unassigned C2=400 through the reversed FK.
   - ⚠ Head-office derivation decision (NOT explicitly confirmed with user): unlinked / no-branch
     account = head office (global); this preserves the tenant owner's global visibility but means
     a plain /User-created account (previously IsHeadOffice=false) is now head-office until linked
     to a branch employee. Flag if a stricter default is wanted.
2. **Recruitment review: evaluator permissions + score locking + hire auto-populate** (migration
   `AddEmployeeUserLink` — one nullable `Employee.UserId` column + index, applied to CERP; E2E
   candbg16 all green. **Uncommitted.**):
   - **Evaluator permissions:** NEW `Employee.UserId` login-account link (`Employee.LinkUserAccount`;
     on Create/Update employee DTO + a "Login Account (for evaluators)" dropdown in the employee
     master form; exposed on `EmployeeDto`). `EvaluationGuard.EnsureMayScoreAsync` in
     `ScoreJobApplication`: resolve current user → employee; if that employee is an assigned
     `CriterionEvaluator` anywhere, they may only score criteria whose evaluator set includes them —
     else 400. Unlinked / non-evaluator users (HR) unconstrained. Decision (asked): "only assigned
     evaluators constrained." E2E: evaluator scores own C1=200, unassigned C2=400, HR scores both=200.
   - **Score locking:** `EvaluationGuard.EnsureEvaluatable` — scoring/adopt allowed only at
     Received/Screening/Shortlisted/Interview; locked at Selected+ (decision made). Replaces the old
     Rejected/Withdrawn/Hired-only guard in `ScoreJobApplication` + `AdoptInterviewScores`. FE score
     button gated on `EVALUATABLE` stages. Decision (asked): "lock when Selected or beyond." E2E:
     score after Selected = 400 "evaluation is complete … locked."
   - **Hire auto-populate:** `HireCandidate` derives salaryScaleId (DTO ?? offer ?? requisition),
     salary (DTO ?? offer ?? scale amount), and position (DTO ?? a vacant position of the
     requisition's PositionClass, preferring its unit) — DTO values still override. `MarkPositionOccupied`
     uses the RESOLVED position. FE Hire modal prefills salary from the accepted offer + relabels the
     position picker "Auto — from the vacancy's role". E2E: hire with no position/salary → employee
     got salary 6500 (offer, not 5000 scale) + auto-picked vacant position + scale.
   - ⚠ `Employee.UserId` is opt-in: enforcement only bites once HR links evaluator employees to
     their login accounts; until then everyone is unconstrained (by design).
2. **Offer bug-fixes (3) + customizable offer-letter PDF template** (migration
   `AddOfferLetterTemplate` — CompanyProfile letterhead columns + `hrms_OfferLetterTemplate`;
   QuestPDF renderer reworked; E2E candbg14/candbg15 all green. **Uncommitted.**):
   - **Bug — application stuck at "Offer Pending" after accept:** new non-terminal stage
     `ApplicationStage.OfferAccepted = 9` (string enum, no schema change). `RespondJobOffer` accept
     path moves OfferPending → OfferAccepted (`OfferShared.MoveToOfferAcceptedAsync`). Wired every
     touchpoint: hire handler (`CandidateLifecycleHandlers`) + hire-queue (`HireQueueHandlers`
     pool/stageOk) accept it as hire-ready; move-TO guards (`JobApplicationHandlers` single+bulk)
     block it; `SaveJobOffer` refuses a new offer at it; FE STAGE_TONE + Move-Stage disabled +
     offer button view-only + `isMovable` exclude it. E2E: accept → stage OfferAccepted; hire
     queue shows it blocked only on missing docs (NOT a stage block).
   - **Bug — `New Offer` reappeared after acceptance:** offerModal now shows an "accepted — ready
     to hire" note when any offer is Accepted (was: Accepted left the ACTIVE set so the button
     returned). Backed by `SaveJobOffer` 400 at OfferAccepted. E2E: new-offer POST → 400.
   - **Bug — e-mail not actually sending:** Gmail (authenticated relay) rejects a `From` that is
     not the login mailbox; `SmtpEmailService` swallowed the SmtpException → silent fail. Fix:
     when `UserName` is an e-mail ≠ `FromAddress`, send AS the login and set the branded address as
     **Reply-To** (`LooksLikeEmail` guard leaves SendGrid-style `apikey` logins alone). NOTE:
     couldn't send a live Gmail test (sandbox blocks external mail) — verified via the pickup-dir
     path + reasoning; user can confirm real delivery via `! ` command. appsettings has live Gmail
     creds (Enabled=true) — approved offers now really e-mail; suggested moving the app password to
     user-secrets.
   - **Feature — customizable offer-letter PDF template (HC111):** `CompanyProfile` gained
     CompanyName/ContactAddress/ContactPhone/ContactEmail (letterhead; logo reused from
     `DocumentTemplate/logo`); new `OfferLetterTemplate` singleton (tokenized Body + signatory,
     default provided). `IOfferLetterComposer` merges 10 tokens (CandidateName/Position/Salary/
     StartDate/ExpiryDate/OfferNumber/EmploymentType/UnitName/CompanyName/Today) →
     `QuestPdfService.RenderOfferLetter` draws letterhead+body+signatory. `GenerateOfferLetter`,
     stored LetterText, and the e-mailed PDF all flow through the composer (one source of truth).
     New `OfferLetterTemplateController` (GET/PUT template, GET/PUT company, GET merge-fields, POST
     preview→PDF). FE admin page *Recruitment → Offer Letter Template* (company fields + logo
     upload + token-palette body editor + live Preview PDF); route + sidebar added. E2E: template
     saved, merge-fields=10, preview `%PDF-` 49 KB, generate-letter merges all 4 dynamic vars w/ no
     stray `{{tokens}}`, approved offer e-mails a 48 KB PDF attachment.
2. **Offer logic refinement: eligibility-gated Offer button, vacancy-derived defaults, manager
   hierarchy, auto PDF delivery on approval** (no migration; QuestPDF added to Inf; E2E candbg13
   verified end-to-end: list eligibility A=Eligible#1/B=Waitlisted#2 → defaults return scale
   G7/S1=5000 + manager resolved from PARENT unit → offer for waitlisted B 400s → offer for A
   without scale/manager auto-populates both → submit auto-approves → offer **Sent**, application
   **OfferPending**, one .eml with a valid 45 KB `OFR-0001.pdf` (`%PDF-` magic) → re-send 409.
   **Uncommitted.**):
   - *Offer button (the "3 applicants all offerable" bug):* the server rank gate existed but the
     list carried no eligibility → UI enabled everyone. `GetAllJobApplications` now batch-computes
     per-row `HireEligibility` + `Rank` via `IGetApplicationRanking` (criteria vacancies only);
     eligibility chip under the stage chip; Offer button active ONLY for Eligible (disabled with
     the specific reason; finished apps keep view). `OfferModal` gets `blockReason`.
   - *Defaults:* `GET JobOffer/defaults?applicationId=` (`GetOfferDefaults`) → unit/position,
     position pay point (requisition scale ?? position-class scale, label+amount), manager from
     `OfferShared.ResolveUnitManagerAsync` (active IsManagerial employee with a position in the
     unit; else walk ParentId ≤10; returns the answering unit). Form opens pre-populated: scale
     LOCKED to the position's pay point (free dropdown only when none), salary pre-filled, manager
     preselected with resolution source ("Manager of parent unit X…"). `SaveJobOffer` applies the
     same defaults server-side when a create omits scale/manager.
   - *Auto PDF delivery:* `IPdfService`/`QuestPdfService` (QuestPDF Community, A4 letter);
     `IEmailService.SendAsync` gained `EmailAttachment[]`; `IOfferDelivery` (`EmailOfferAsync` +
     `TryAutoSendAsync`) hooks BOTH final-approval paths (`JobOfferWorkflowHandler.OnApprovedAsync`
     + `SubmitJobOffer` auto-approve): ensures a letter (generates + attaches HC111 standard via
     new `JobOffer.AttachLetter`, frozen after Sent), renders PDF, e-mails; on success MarkSent +
     app→OfferPending ("approved and e-mailed"); on failure stays Approved. Manual Send = retry
     (also e-mails; `ISendJobOffer` returns bool; controller message says delivered-or-not, shown
     as an info banner in the modal). `OfferShared.MoveToOfferPendingAsync` extracted/shared.
   - ⚠️ `IsManagerial` is not settable via the employee create/update API (only consumed) — the
     E2E flags it via SQL; a UI/API toggle is a small gap if the manager hierarchy should be
     self-service.
2. **Automatic interview e-mails + first e-mail infrastructure** (no migration; E2E via
   `Email__PickupDirectory` .eml delivery: schedule→invitation, time-change reschedule→
   "Rescheduled" w/ old→new times, panel-only edit→NO mail, cancel→cancellation, no-email
   candidate→skipped gracefully; decoded .eml verified (MIME-encoded subjects/base64 bodies due
   to em/en-dashes — grep the DECODED content in tests). **Uncommitted.**):
   - *Infrastructure:* `IEmailService` (App) + `SmtpEmailService` (Inf, System.Net.Mail — no new
     deps): `Email` config section (Enabled=false default → logged no-op; Host/Port/EnableSsl/
     UserName/Password; FromAddress/FromName; `PickupDirectory` → .eml files for dev/test).
     NEVER throws; 15s timeout; registered in Inf DI; section added to appsettings.json.
   - *Triggers:* `IInterviewNotifier` (Recruitment) — invitation on `SaveInterview` create,
     rescheduled-notice when ScheduledStart/End actually CHANGE on update, cancellation on
     `SetInterviewStatus` cancel; all AFTER SaveChanges, internally try/caught; resolves
     application → candidate (Email/name) + requisition title; no-address = logged skip.
   - Production note: for real delivery set `Email:Enabled=true` + relay settings (or keep
     PickupDirectory for staging); an outbox/queue is the future hardening step for volume.
2. **Bulk stage moves (mass processing)** (no migration; E2E verified [mixed batch of 5 →
   moved=2, skips: offer-locked "Offer OFR-0001 (Draft) drives…", final "Rejected … is final",
   unknown "not found"; OfferPending destination → 400; shared note logged per app].
   **Uncommitted.**):
   - *Backend:* `PUT JobApplication/stage/bulk` (`BulkMoveApplicationStage`, max 200 ids) —
     per-item outcomes (SAP mass-processing style): each app checked against the SAME single-move
     rules (final stages, offer-driven lock incl. Accepted, already-there); the movable subset
     saves as ONE transaction; skips return `{applicationId, candidateName, reason}`; batched
     offer-lock + candidate-name queries.
   - *Frontend:* checkbox selection column on the Applications pipeline (final/offer-driven rows
     unselectable w/ tooltip), "Move N Selected" + Clear toolbar actions, `BulkStageModal`
     (stage select excluding OfferPending/Hired + shared note → moved/skipped result report),
     selection cleared + list refreshed on Done.
2. **StageModal score contradiction + error-message artifact** (frontend only, builds clean.
   **Uncommitted.**): the Move Application Stage form offered a manual "Screening Score (0–100)"
   field that the backend always rejects on criteria-scored vacancies — it now hides the field on
   such vacancies (`autoScored = totalCriteriaCount > 0`), shows the current auto-calculated total
   with an explanatory note (score sheet ★ owns it) and keeps only the remarks input;
   criteria-less vacancies keep the manual field. Separately, `errorMessageParser` was rewritten
   to emit PLAIN TEXT (single error verbatim; multiple as numbered "\n"-joined lines) — it used to
   build `"1 …<br/>"` strings that every consumer rendered as literal text (no consumer uses
   innerHTML for it); StageModal error line got `whitespace-pre-line`.
2. **Interview level-gating + panel inheritance from criteria evaluators** (no migration; E2E
   verified [schedule at Received → 400 citing the Interview level; at Interview → 200];
   builds clean. **Uncommitted.**):
   - *Level rule (backend):* `SaveInterview` requires the application AT the Interview stage —
     the auto-advance side effect was REMOVED (stage moves are deliberate decisions). Feedback,
     round status changes and viewing stay available from any stage.
   - *No interviewer re-entry (frontend):* `ScheduleForm` **pre-fills the panel from the vacancy's
     criteria evaluators** (Interview-level + global; employees → employee panelists, external
     persons/orgs → named panelists, deduped, first = lead) via `getJobRequisition`; the panel
     editor is now chip-based and supports **named external panelists** (previously employee-only —
     external evaluators couldn't even be carried onto a panel). Modal gained
     `requisitionId`/`applicationStage` props; Schedule + per-round Reschedule buttons render only
     at the Interview level (explanatory hints elsewhere); row tooltip explains the level rule.
2. **Action-sequence & criteria-flow refinement + recruitment data reset** (no migration;
   stage-lock E2E verified [400 citing the offer → withdraw → 200]; builds clean. **Uncommitted.**):
   - *Offer-driven stage lock (backend):* `MoveJobApplicationStage` now blocks manual moves while
     ANY offer is in play (Draft/PendingApproval/Approved/Sent/**Accepted**) — 400 naming the
     offer; previously a Sent offer could be stranded on a manually-Rejected application.
   - *Row actions reordered to process order:* Score → Interviews → Move Stage → Offers → History.
     Interviews ALWAYS viewable (modal `readOnly` for final applications — no schedule/score/adopt);
     Offers viewable from Selected onward + on final apps (modal `canCreate` gates New Offer to
     Selected/OfferPending); Score hidden at final stages; Move Stage disabled at OfferPending w/
     explanatory tooltip; every disabled state now explains itself.
   - *Criteria flow:* **Apply≠Save trap surfaced** — `criteriaDirty` badge "Not saved yet — Save
     Requisition to persist" until the form saves; button reads Define/Edit/View Criteria by
     context; empty state gained **Load Standard Template (50/30/20)**.
   - *Data reset:* new reusable script `backend/scripts/reset-recruitment-data.sql` — emptied ALL
     recruitment tables (9 candidates, 6 requisitions, 6 requests, 15 apps, 9 interviews, 1 offer),
     removed candidate-only persons + their edu/exp rows + pre-hire attachments + resume file,
     cleared recruitment workflow instances, reset HRQ/REQ/CND/OFR counters to 0001. PRESERVED:
     11 employees + persons, org structure, workflow definitions. DB is at a clean slate for the
     user's end-to-end test.
2. **Recruitment end-to-end review & hardening** (data-only migration
   `SeedRecruitmentNumberSequences`, applied; E2E **16/17 green** [the 1 "miss" = double-submit hit
   the handler's ValidationException pre-check (400) instead of the domain guard — correct
   behavior] + a follow-up probe confirming message pass-through; tenant purged. **Uncommitted.**):
   - *F1 — domain guards → 409:* `ExceptionMiddleware` maps `InvalidOperationException` to 409
     Conflict w/ the domain message (was a generic 500); removed from `IsTransientException` (Inf)
     so retry wrappers never re-execute a rule violation. Handler-level ValidationExceptions stay
     400 — most recruitment handlers double-guard, the 409 catches direct domain transitions
     (offer send/accept, interview complete, etc.).
   - *F2/F3/F8/F9 — nothing strands* (new `PipelineDisposition.CloseOutAsync` helper): hire filling
     the LAST position auto-closes the requisition + Rejects the runner-ups ("Position filled…");
     Close/Cancel requisition dispositions open applications + withdraws live offers; hire
     Withdraws the new employee's other active applications; anonymize Withdraws the pipeline
     BEFORE the PII scrub. All moves stage-logged; Draft/Approved/Sent offers withdrawn
     (PendingApproval offers stay with their running workflow — see §2).
   - *F4 — one source of truth:* manual `screeningScore` on stage moves → 400 when the vacancy has
     weighted criteria (the criterion engine owns the total).
   - *F7 — offers rank-gated:* `SaveJobOffer` enforces the same eligibility window as hire — no
     offer to Waitlisted/NotScored/FailsMandatory/OfferRejected candidates (specific 400s).
   - *F5 — adopt interview scores:* `POST JobApplication/{id}/adopt-interview-scores` copies the
     consolidated per-criterion averages into the score sheet (weights inherited) + recompute;
     "Adopt into Ranking" button on the consolidated report. E2E verified 90×60% + 80×40% = 86.
   - *F10 — unified numbering:* HRQ/REQ/CND moved to `INumberSequenceService`; counters seeded
     from each tenant's existing max (verified: main tenant Candidate=9, HiringRequest=6…).
   - *F6/F11 — UI:* stage dropdown no longer offers OfferPending/Hired (offer-driven); Applications
     toolbar gained a **vacancy filter** (parentId) + **Ranking** shortcut; `RankingModal` extracted
     to `jobRequisition/rankingModal.tsx` (shared).
2. **Multi-evaluator criteria + enterprise criteria-popup redesign** (migration
   `AddCriterionEvaluators`, applied — **hand-reordered Up(): CreateTable + data-copy SQL BEFORE
   the column drops** (scaffold order lost data); legacy empty-EvaluatorType rows purged. E2E
   **11/11 green**, tenant purged. **Uncommitted.**):
   - *Schema:* new `hrms_CriterionEvaluator` (criterion 1─< evaluator; EmployeeId? SET NULL +
     server-resolved name snapshot; ExternalPerson/Organization free-named). The 3 single-evaluator
     columns on `hrms_RequisitionScreeningCriterion` are GONE (data migrated into child rows).
   - *Domain/App:* `CriterionEvaluatorSpec` + `CriterionEvaluator.Create` (rejects None/incomplete);
     `ScreeningCriterionSpec.Evaluators` list; validator = per-row completeness + **no duplicate
     employee per criterion**; `BuildCriterionSpecsAsync` batch-resolves ALL employee names in one
     query; `StampCriteriaTenant` stamps BOTH child levels (aggregate gotcha, 2 deep); reads need
     `.Include(ScreeningCriteria).ThenInclude(Evaluators)`; `CriterionScoreDto.EvaluatorName` is
     now a deterministic alphabetical joined-names string (EvaluatorType removed from that DTO).
   - *Frontend:* `criteriaModal.tsx` fully redesigned (enterprise standard) — card-per-criterion
     layout w/ labeled fields (weight has % suffix), **evaluator chip panel** (kind icons, removable,
     inline add row: Employee picker | External/Org name, Enter-to-add, duplicate-employee guard),
     toolbar (Add pre-fills unassigned weight, **Distribute Evenly**), footer weight progress bar
     (green/amber/red) gating Apply, empty state. Models: `CriterionEvaluatorModel`,
     `ScreeningCriterionModel.evaluators[]`; requisition chips show "N evaluators" w/ tooltip.
   - E2E: 3-evaluator round-trip (2 external + org), zero-evaluator OK, duplicate-employee 400,
     unknown-employee 404, wholesale replace cascades (0 orphans), joined names in score sheet.
   ⚠️ Reminder: `kill %job` does NOT kill the dotnet API child — use `Stop-Process -Name
   CyberErp.Hrms.Api` before rebuilding (file locks).
2. **Weighted screening criteria, ranking/waitlist & Hire Employee menu** (migration
   `AddCriterionStageScope` [adds `RequisitionScreeningCriterion.AppliesAtStage`], applied; E2E
   **20/20 green** on disposable tenant, purged. **Uncommitted.**):
   1. *Criteria = percentages totaling exactly 100%* — domain (`SetScreeningCriteria`) + validator
      + UI enforce Σ==100; criteria setup moved to a **popup grid** (`jobRequisition/criteriaModal.tsx`,
      live Σ badge red/green, Apply gated; the form shows a chip summary card). Per-criterion
      **Level** scope (All Steps | Screening | Interview | Final Review = `AppliesAtStage?`) +
      internal/external evaluators (unchanged). **Weights inherited downstream:** score sheets show
      `weight%` read-only; interview feedback sheet filters to Interview+global criteria; interview
      consolidated adds `WeightedAverage` (Σ avg×w / Σw).
   2. *Ranking + waitlist* — `RankingShared` (in JobApplicationHandlers.cs) assigns Rank +
      HireEligibility (Eligible|Waitlisted|Hired|OfferRejected|OutOfContention|FailsMandatory|
      NotScored); Eligible window = top (NumberOfPositions − hired) in-play rows; latest offer
      Declined/Expired ⇒ out of contention ⇒ next waitlisted slides up. `HireCandidate` rank gate
      (only when the vacancy HAS criteria — legacy vacancies unaffected) with specific 400 messages.
      RankingModal shows 1st/2nd/3rd medals + eligibility badges + weight% in breakdowns.
   3. *Hire Employee menu* (`/hireEmployee`, sidebar Recruitment group) — `GET
      JobApplication/hire-queue` (`HireQueueHandlers.cs`): strictly Eligible+Waitlisted rows of
      Approved/Posted vacancies, grouped per requisition (hired/positions counter), per-row
      CanHire/BlockedReason (rank→stage→offer→compliance precedence). **Hire modal MOVED here from
      the candidate form** (candidate Documents card now shows a pointer note; hire button/modal
      and related code removed from `candidate/form.tsx`).
   E2E: 80% total→400 (message cites 100%), stage scope persists, weighted consolidated avg,
   A=83/1st Eligible vs B=69/2nd Waitlisted, queue canHire flags, waitlisted hire→400 citing
   waitlist, decline→A OfferRejected + B promoted Eligible, declined-#1 hire→400, promoted-#2
   hire→200, queue drains to 0.
   4. *Level-aware score-button visibility (follow-up, E2E 12/12):* the pipeline's score action
      renders per row iff the CURRENT stage has scoreable criteria — global ("All Steps") criteria
      always count, level-scoped ones only at their level. Server-computed:
      `JobApplicationDto.TotalCriteriaCount` + `ScoreableCriteriaCount` on list AND by-id
      (`GetAllJobApplications` batches criteria stages per requisition). ScoreModal filters its
      sheet to the same subset + distinguishes "no criteria" from "none at this step". Verified
      counts across Received(1)→Screening(2)→Interview(2)→Selected(1) with a 50-global/30-Screening/
      20-Interview criteria set.
2. **Recruitment Phase 2 — interviews, panels & offers (HC101–HC109, HC111–HC114)** (migration
   `AddRecruitmentInterviewsOffers`, applied to CERP; E2E 14/15 green on disposable tenant
   [the 1 "fail" = 81.5 vs 81.50 format artifact], purged. **Uncommitted.**):
   - *Entities (shapes adopted from the §7.1 DB review):* `hrms_Interview` (rounds, window CHECK)
     1─< `hrms_InterviewPanelist` (employee SET NULL + name snapshot OR free-text external; lead;
     attendance) 1─< `hrms_InterviewFeedback` (0–100 CHECK, loose criterion + snapshot);
     `hrms_JobOffer` (status machine, salary CHECK, filtered unique ACTIVE-offer-per-application
     index, HiredEmployeeId? no-FK); `hrms_NumberSequence` (PK TenantId+Key, NOT BaseEntity).
   - *Numbering:* new `INumberSequenceService` (Inf: atomic `UPDATE…OUTPUT` + lazy seed + dup-key
     retry) — offers use `OFR-####`; §7.1 count+1 race fixed for new numbers.
   - *Interview slices* (`InterviewHandlers.cs`): schedule w/ panel (names resolved server-side;
     externals allowed), reschedule/re-panel (Scheduled only), Complete/Cancel/NoShow, per-criterion
     feedback (auto-Attended), consolidated report (HC109), delete Scheduled-only. First round
     auto-advances the application to Interview (logged).
   - *Offer slices* (`JobOfferHandlers.cs`): save (HC113 scale-deviation needs justification),
     submit → generic workflow `JobOffer` (seeded HR → Approving Authority; auto-approve when
     undefined; rejection → Draft), send (app → OfferPending), respond Accept/Decline, withdraw,
     lazy expiry on read (all three release the app → Selected), generate-letter (HC111),
     delete Draft-only. `JobOfferWorkflowHandler` + seed entry + `WorkflowEntityTypes.JobOffer`.
   - *Hire integration:* `HireCandidate` now accepts stage Selected OR OfferPending; once ANY offer
     exists the newest must be ACCEPTED (400 otherwise); hire stamps `offer.AssignHiredEmployee`.
   - *Frontend:* `jobApplication/interviewsModal.tsx` (rounds, panel editor w/ lead radio, status
     actions, per-panelist score sheet from the requisition criteria + overall entry, consolidated
     report w/ bars) + `offerModal.tsx` (draft editor w/ scale pick + live deviation-justification
     prompt, letter generate/preview, submit/send/respond/withdraw, active-offer gating); two new
     row actions on the Applications pipeline; `JobOffer` added to `workflowEntityTypeOptions`
     (HC070 lesson) + `interviewFormatOptions`.
2. **Candidate structured background + internal flow + Source/Type UI cleanup** (NO migration;
   E2E-verified on a disposable tenant, then purged. **Uncommitted.**) — improving the Candidate
   feature step by step:
   1. *Standardize candidate data (#1):* candidate education/work history now writes the **same
      person-owned `hrms_EmployeeEducation` / `hrms_EmployeeExperience` rows the employee profile uses**
      (both keyed on **PersonId**, not EmployeeId). New slice
      `Features/Core/Recruitment/CandidateBackgroundHandlers.cs`: DTOs take `CandidateId`, resolve
      `Candidate.PersonId`, reuse the domain `Create`/`Update`. Routes on `CandidateController`:
      `GET/POST Candidate/{id}/education` + `…/experience`, `DELETE Candidate/education/{id}` +
      `…/experience/{id}`; DI in `App/DependencyInjection.cs`. **The person IS the hand-off** — hire
      creates the Employee on that same PersonId, so the rows are already the employee's (verified:
      candidate.PersonId == education.PersonId == experience.PersonId; zero copy, zero migration).
   2. *Internal applicant flow (#2):* selecting an internal Employee in the form fetches
      `GET Employee/{id}` and **prefills + locks** identity (name parts, gender, email, phone). Internal
      candidates are **read-only** for education/experience (backend guard 400s create/update/delete;
      GET still works) — the employee master is authoritative.
   3. *Source/Type UI cleanup (#3):* replaced the confusing `Source` dropdown (which mixed the
      internal-vs-external *type* with acquisition *channels*) with an **Applicant Type** segmented
      control → Internal (Employee picker + locked identity) | External (Source Channel dropdown +
      editable identity). `CandidateSource` enum unchanged; UI derives type = (source===Internal).
   Frontend: `components/admin/candidate/{form,educationSection,experienceSection}.tsx` (reuse the
   generic `ChildManager`, now with an optional `readOnly`/`hint` prop), `services/admin/recruitment/index.ts`
   (`getCandidateEducations`/`save…`/`delete…` + experience), models `CandidateEducationModel`/`…Experience`.
   Dropped the Education/Experience **textareas** from the form (columns kept — dropping = destructive);
   `SkillsSummary` stays (drives matching).
   - *Follow-up fixes (same increment):* (a) **"saved but grid/DB empty" bug — ROOT CAUSE was a
     `formId` collision**: `FormProvider` defaults its `<form>` DOM id to the hard-coded
     `"formProvider"`, and a modal FormProvider's Save button lives in the modal FOOTER (outside the
     form), wired via the HTML `form="<id>"` attribute (`actionBar.tsx` / `submitButton.tsx`). With
     the main candidate form and the education/experience modal mounted simultaneously, both had
     `id="formProvider"`, so the modal's Save submitted the FIRST match — the **main candidate form**
     → `saveCandidate()` ran, showed "Successfully saved", and the education POST never fired (DB
     empty; backend verified correct via authenticated curl against the user's real instance/tenant).
     Fix: explicit `formId` (`candidateForm` / `candidateEducationForm` / `candidateExperienceForm`).
     The employee profile never hit this because its tabs mount ONE FormProvider at a time
     (`profile.tsx` conditional rendering); every other `showModal` user renders a single provider.
     Also hardened: sections `await refetch()` after save/delete and surface the list-query error into
     `ChildManager` (a failing GET used to read silently as "No records yet").
     (b) **Candidate form UI polish** — restructured into clean titled cards (**Applicant Details**
     [with the Applicant Type toggle in its header], **Resume & Retention**, **Documents & Compliance**,
     **Background**), max-width container, name-as-title header with pill status badges.
     (c) Removed dead template scaffolding (`backgroundSyncService`/`database.ts`/`healthcheck/get.ts`)
     that pinged a nonexistent `Health/live` endpoint every 5 min (console 404 noise).
   - *UX iteration 2 (2026-07-10, E2E-verified, tenant purged):* (a) **Employee-style tabs** — the
     candidate form is now a tabbed profile mirroring `employee/profile.tsx` (tab bar ABOVE the
     persistent header card): **Applicant Details | Education | Experience** (last two gated until
     saved). (b) **Switch toggle** replaces the Internal/External segmented buttons — unchecked =
     External (default), checked = Internal (`role="switch"`). (c) **Background-row attachments** —
     education/experience rows now take file attachments stored in the SAME `hrms_EmployeeDocument`
     table the employee profile reads (OwnerType Education/Experience + OwnerId = row id), so they're
     on the employee's profile at hire automatically. `EmployeeId` (no FK) anchors to the CANDIDATE id
     until hire; new `EmployeeDocument.AssignEmployee()` + a `HireCandidate` step re-anchors them to
     the new employee. New slice handlers (Upload/Get/Download/Delete `CandidateBackgroundDocument`,
     write-guarded: internal/anonymized 400; reads allowed) + routes
     `GET/POST Candidate/{id}/background-documents` (+`ownerType`/`ownerId`),
     `GET Candidate/background-documents/{docId}/download`, `DELETE Candidate/background-documents/{docId}`;
     row deletes cascade attachments (`DocumentStorage.DeleteForOwnerAsync`); DTOs expose
     `DocumentCount` (paperclip column). Frontend: `candidate/backgroundAttachments.tsx` (mirror of
     employee `documentAttachments.tsx`) inside the education/experience modals. E2E: upload→list→
     count→byte-identical download; pre-hire anchor = candidate id; employee-side endpoint 404s for a
     candidate-only person (no leak); internal upload/delete 400 + read 200; cascade verified.
2. **Recruitment candidate lifecycle — 5 user requirements** (migration
   `AddRecruitmentCandidateLifecycle`, applied; E2E-verified end-to-end, tenant purged):
   1. *Evaluator scoring:* criteria carry an evaluator (Employee [FK SET NULL + server-resolved
      name] / ExternalPerson / Organization); `hrms_ApplicationCriterionScore` (unique per
      app×criterion, weight snapshot); total auto = Σ(score×weight)/Σweight (verified 81.67);
      `PUT JobApplication/scores` + `GET ranking?requisitionId=` (breakdown + FailsMandatory<50);
      score-sheet modal w/ live preview + Ranking modal.
   2. *CorePerson link:* `Candidate.PersonId` created at save (grandfather+gender now required;
      Internal candidates REUSE their employee's person); hire creates the Employee **on the same
      person** — verified same-person=YES.
   3. *Document migration:* `hrms_CandidateDocument` (typed, inline binary) + at hire ALL docs +
      resume auto-migrate → `hrms_EmployeeDocument` w/ new owner `Recruitment` (OwnerId=employeeId,
      string enum → no migration on that table).
   4. *Talent Pool* (`/talentPool`): searchable past applicants, application history
      (`JobApplication?categoryId=`), Apply-to-Vacancy, hired badges.
   5. *Mandatory documentation:* compliance set (ID + Guarantor + Medical + signed offer/contract)
      gates hire (400 lists missing); candidate form shows checklist + badge; Hire modal (number,
      vacant position, nature/contract, probation → status Probation).
   ⚠️ Gotcha: `Candidate.HiredEmployeeId` has NO FK — a second SET NULL path to hrms_Employee trips
   SQL Server's multiple-cascade-path rule (InternalEmployeeId holds the slot). Committed as `059f3b0`.
2. **Recruitment & Talent Acquisition — Phase 1, HC077–HC100 core** (migration
   `AddRecruitmentPhase1`, applied; full-pipeline E2E on a disposable tenant, then purged):
   - 6 tables: `hrms_HiringRequest` (need assessment: justification/budget/plan-link; submit gated
     by **vacant establishment seats** [HC082]; workflow `HiringRequest` seeded Directorate Head →
     HR → Finance) → `hrms_JobRequisition` (+ScreeningCriterion; **only from an APPROVED request**
     [HC080]; defaults from PositionClass; Σ positions ≤ request; workflow `JobRequisition`;
     posting generate/set/publish w/ channel Internal/External/Both) → `hrms_Candidate` (consent
     mandatory [HC097], resume upload PDF/DOC/DOCX, talent pool, anonymize scrubs PII+file,
     skills-token matching endpoint [HC090]) → `hrms_JobApplication` (+StageLog; unique
     candidate×requisition; stage machine w/ interview bypass [HC102], terminal lock, screening
     score; append-only transition log).
   - Slices `Features/Core/Recruitment/` (4 handler files + DTOs), `RecruitmentControllers.cs`,
     2 workflow handlers + seeds, **workflowEntityTypeOptions += HiringRequest/JobRequisition**
     (the HC070 lesson applied). Numbers HRQ-/REQ-/CND-#### (count+1, unique-indexed).
   - Frontend: **Recruitment** sidebar group → hiringRequest (budget-monitor modal), jobRequisition
     (criteria editor + posting designer + match modal), candidate (consent checkbox, resume
     upload/view, talent-pool toggle, anonymize confirm), jobApplication (stage chips + new/move/
     history modals). `App_Data/candidate-resumes/` gitignored.
   - E2E verified: establishment gate 400 → reduce → 3-step approval; requisition-before-approval
     400; class defaults; over-requisition 400; 2-step approval → posting generated/published;
     consentless candidate 400; resume round-trip; match ranked (75 = skills+exp+pool); duplicate
     application 400; Received→Screening(85)→Shortlisted→Selected with interview bypassed; terminal
     move 400; budget monitor row; anonymize scrubbed. (One test-side false alarm: Git Bash mangles
     em-dashes in inline JSON → 400s that are NOT app bugs; send UTF-8 via file.)
   - **Deferred:** Phase 2 interviews/offers (HC101–HC114), Phase 3 public portal + onboarding→
     employee creation (HC093/HC115–117), email notifications (no SMTP), resume parsing (HC094),
     job-board feeds (HC092 inbound). **Uncommitted.**
2. **Workforce-plan "Approved Budget resets to 0" bug — fixed** (frontend only): the save service
   coerced numerics with `Number(v) || 0`, so a budget typed with thousands separators
   ("500,000") became `NaN` → silently saved as **0** (user-reported). Fix in
   `services/admin/workforcePlan/index.ts`: exported `parsePlanNumber` strips `,`/spaces before
   parsing; header fields (budget/threshold/periods) now **fail validation** on genuinely
   non-numeric input instead of silently zeroing; line cells use the tolerant parse with a 0
   fallback; the form's live tiles (`n()` in `form.tsx`) mirror the same parsing so display ==
   what saves. ⚠️ Pattern note: `Number(x) || 0` is a silent-data-loss trap on formatted input —
   prefer separator-tolerant parse + explicit validation. **Uncommitted.**
2. **Dashboard "Approvals" inbox for workflow approvers** (no migration; fixes "approver logs in and
   has nothing to action"): reproduced E2E — the engine/API were correct (`canDecide:true` on the
   tracking list) but the only actionable surface was buried at System → Workflow Tracking with no
   cue. Added `GET Workflow/my-approvals` (`GetMyApprovals` in `WorkflowHandlers.cs`: Running
   instances whose CURRENT step lists the user specifically [user or role]; open steps excluded;
   `IsApprover` flag from active definitions drives tab visibility) + a conditionally-rendered
   **Approvals** tab on the Dashboard watchlist (before Clearance) with prominent Approve/Reject
   buttons + comment modal (reject requires a reason), invalidating
   myApprovals/workflows/workflowStats/workforcePlans/employees on decide, plus an "Open Workflow
   Tracking" link. Verified E2E on a disposable 2-user tenant (submitter sees isApprover:false;
   approver sees the item, step-advances 1→2, final approve → plan Approved + inbox empty), purged.
   **Uncommitted.**
2. **Workforce Planning module — HC053–HC076** (migration `AddWorkforcePlanning`, applied;
   E2E-verified on a disposable tenant incl. the full 4-step workflow approval, then purged):
   - Tables `hrms_WorkforcePlan` (horizon/scenario/status, unit-subtree scope, FY + PeriodCount
     horizon, budget + threshold + escalation justification, denormalized ProjectedCost, Version +
     RootPlanId chain) 1─< `hrms_WorkforcePlanLine` (unit × role × planned employment type
     [Permanent/Contract/Intern/Consultant — module enum, Employee untouched] × period; establishment
     snapshot, demand/supply/separations, critical-role + competencies text, per-head costs with
     salary defaulted from the scale ×12; computed EndHeadcount/Gap/LineCost; unique composite).
   - Slices `Features/Core/WorkforcePlans/` (save/get/list/delete/submit/new-version + establishment
     overview, populate-from-establishment, suggest-separations [DOB+60y within horizon], summary,
     compare, approved-demand). `WorkforcePlanWorkflowHandler` (approve + auto-archive superseded
     versions; reject → editable). Seeded chain Directorate → HR → Finance → Executive.
   - Budget gate: submit 400s without escalation justification when cost > budget×(1+threshold%).
     **Gotcha fixed during E2E:** domain `InvalidOperationException`s surface as 500 — module
     convention is handler-level `ValidationException` pre-checks (added on submit + update paths).
   - Frontend: **Planning** sidebar group → `/workforcePlan` (list w/ compare-checkboxes +
     CompareModal + Hiring-Demand modal; designer form: header FormProvider + 24-column editable
     lines grid [incl. visible Gap column, HC062] + live cost/variance tiles + **Period Projections
     table** [per-year headcount/demand/mobility/attrition/cost trend, HC069/HC073] +
     Populate/Suggest/Submit[escalation modal]/New-Version) and `/establishmentOverview` (tiles +
     occupancy bars + vacancy aging + **Excel export**, HC074). New model/service files; options in
     `constants/orgStructure.ts`.
   - **Review fixes (user caught HC070 gap):** `workflowEntityTypeOptions` was missing
     `WorkforcePlan` (and `LeaveRequest`) — the Workflow Definitions designer could not configure
     those chains even though the backend/seed existed. Added both. ⚠️ Rule: every new
     workflow-backed module must add its entity-type key to `workflowEntityTypeOptions`.
     **Uncommitted.**
2. **Employee Reinstatement + Clearance Document** (migration `AddTerminationReinstatement`, applied;
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

- **Recruitment review — incomplete/accepted items (2026-07-10):**
  - Offers stuck at **PendingApproval when their vacancy closes**: the disposition helper leaves
    them to the running workflow (approve/reject from the workflow screen, then withdraw) — a
    workflow-instance cancel API would close this gap cleanly.
  - **Hire-queue candidate deep-link** (row → candidate profile) not yet wired; `GetHireQueue`
    runs per-row compliance queries (fine at admin volume — batch if the queue grows).
  - Posting window (`OpenUntil`) intentionally does NOT block manual application entry
    (walk-ins/late registration by HR) — documented in `logic.md`, revisit for the public portal.
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
