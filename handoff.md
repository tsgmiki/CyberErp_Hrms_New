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
- Commit/push only when the user explicitly asks. The pre-commit hook prompts you to confirm
  `memory.md` / `handoff.md` / `logic.md` are updated when a commit changes code without them
  (bypass: `SKIP_DOC_CHECK=1` or `git commit --no-verify`). `App_Data/employee-photos/` is gitignored.

## 1. Most recent changes (latest first)

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
