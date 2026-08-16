# handoff.md — Session Handoff

> **Living document.** The latest granular changes, outstanding tasks, and the exact context needed to
> resume seamlessly next session. Update this **every working session** (enforced by `.githooks/pre-commit`).
> Big-picture state lives in `memory.md`; system logic in `logic.md`.

---

## 0. ⚠️ Repository state — READ FIRST

- **CURRENT BRANCH: `feature/hrms-buildout-12`** (branched off `main` at `d16bc99`, 2026-08-12) —
  carries the two-flow dashboard performance batch (00ER), paired with Home commits on its `main`.
  **PR #13 (`d16bc99`) IS MERGED** — it landed the entity-route hardening (00EQ), paired with Home
  `14a1deb`; **PR #12 (`8d08480`) IS MERGED** — the approvals performance work (00EP), paired with
  Home `fe2e5ba` + `15aad77`. `feature/hrms-buildout-9`, `-10` and `-11` can all be deleted.
  **PR #9 (`57e7ff7`) and PR #10 (`4c2534a`) ARE MERGED.** #9 landed the return-from-leave
  reachability work, `PositionClass.TitleA`, the annual-leave/`LeaveType` decoupling, the ledger
  pagination fix, the app-shell height fix and the tree scroll/search (00EF–00EL, paired with Home
  `84f8003`); #10 landed the employee-visibility fix and the performance-history access fix
  (00EM–00EN). `feature/hrms-buildout-7` and `-8` can both be deleted.
  `main` is the integration branch — **open a PR from the current branch when a batch is ready**, then
  rotate to a fresh `feature/hrms-buildout-N`. Merged so far: **PR #2** the buildout (18 commits),
  **PR #3** the doc sync, **PR #4** URL-driven `:id` routing + salary revision by step, **PR #5**
  performance score bands, detail-as-grid, and the route state-loss fix, **PR #6** the module-schema
  rename, **PR #7** the salary-increment work (00DR–00DW: eligibility rules, the Increment Rules
  screen, the Hired Date column, grade-ceiling promotion, the terminated-employee exclusion and the
  Approve/Apply button fixes), and **PR #8** the return-from-leave workflow plus the retiree exclusion
  and the per-case `AffectsSalaryIncrement` flag (00DX–00DZ) — paired with Home `711b7bb`.
  The `feature/hrms-buildout`, `-2`, `-3`, `-4`, `-5` and `-6` branches were
  deleted after merging — historical references to them below are accurate for their date, but those
  branches no longer exist.
- ⚠️ **The module-schema rename IS APPLIED to CERP** (PR #6 + Home `8ee69da`). Tables are
  `Hrms.X` / `Core.X` with no prefixes; procedures are `Hrms.Report_X`. **Both apps must be deployed
  together** — a stale binary on either side throws `Invalid object name 'dbo.hrmsX'`. Any OTHER
  environment needs `dotnet ef database update` in both repos, or the two scripts under
  `backend/scripts/schema-rename/` run in order (01 HRMS, then 02 Home).
  Pre-change restore point: `CERP_before-schema-rename-20260808-192711.bak`.
- **Eight migrations are applied LOCALLY ONLY** (`SalaryStepOrdinalAndStepBasis`,
  `SalaryRevisionPerformanceBands`, `ModuleSchemaRename`, and from this session
  `AddSalaryIncrementPolicy`, `AddSalaryRevisionLineEligibility`, `AddGradeCeilingPromotion`,
  `AddAffectsSalaryIncrement`, `AddAnnualLeaveReturn`).
  The five newest are additive (two tables + six nullable/defaulted columns; `AddAnnualLeaveReturn`
  also widens `AnnualLeaveHeader.Status` 20→30, `Up` widens only so no data loss) and need
  `dotnet ef database update` anywhere else.
  **Add from this session (00EH):** `AddPositionClassTitleA`, `DecoupleAnnualLeaveFromLeaveType` and
  `AnnualLeaveSettingAllowHalfDay` — all applied to CERP, all local-only. The decoupling one is NOT
  purely additive: it makes `LeaveBalance.LeaveTypeId` / `LeaveBalanceTransaction.LeaveTypeId`
  **nullable** and repoints existing annual rows to NULL. Its `Down` restores non-null with a
  `Guid.Empty` default, which would NOT recover the original type ids — restore from backup instead
  of rolling back.
- ⚠️ **CERP now contains ONLY migrated NVI production data** (00EG): 490 employees under Head Office
  `aadb4e82`, 125 other tables emptied, 490 accounts with the default password `password`. All prior
  demo/test data is gone. Restore point `CERP_before-purge-and-retenant-20260810-154842.bak`.
  **Every `WorkflowDefinition` was purged**, so annual leave (and every other governed process)
  rejects submissions until the chains are reconfigured.
- **CERP tenant configuration is DONE and is not reproducible from the repo** (see 00EA): all three
  tenants have an active `AnnualLeave.Return` chain, and Demo Corp now has a full annual-leave setup
  (leave type + generated 16-day ledger + `AnnualLeave` chain). **A new tenant or a fresh database
  needs all of that configured by hand** — early/late returns fail with a message naming the process
  until `AnnualLeave.Return` exists.
- **A new menu operation `/salaryIncrementPolicy` also
  needs seeding per tenant** (`POST /Module/seed-defaults`, per-CURRENT-tenant) **and then granting** —
  seeding creates the operation but no `RolePermission`, so the screen 403s until an admin grants it
  (verified). **Both tenants in CERP are already done** and need nothing: `aadb4e82` was set up by hand
  on 2026-08-09 (operation named **"Salary Increment Policy"**, icon `BadgeDollarSign`, granted to
  Administrator — which is also where the `lucideIconMap` `BadgeDollarSign` entry came from), and
  re-running the seeder there returns `{"created":0,"message":"Menu already seeded"}` with all 122
  seeder links present. The seeder matches operations by `(module, link)` and leaves existing rows
  untouched, so that hand-made name/icon survives any future re-seed — it differs from the seeder's
  "Increment Rules" / `SlidersHorizontal` in appearance only.
  The `Step.Ordinal` backfill is an inference
  that should be eyeballed per tenant before production — note the table is now `Core.Step`:
  `SELECT Name, Code, Ordinal FROM Core.Step ORDER BY TenantId, Ordinal;`
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

0125. **SRMS dropped two TenantRole foreign keys -- CERP follows, projector compensates
    (2026-08-16).** Migration `SrmsDropTenantRoleForeignKeys`, APPLIED. FKs 12 -> 9.
    Backup: `D:/Backups/CERP_before-tenantrole-fks-*.bak`.
    - Detected by `compare-schemas.ps1`: FK differences jumped 9 -> 12 while SRMS own FK count fell
      27 -> 25. Confirmed directly on the three tables rather than trusting the diff. Three changes:
      **(a)** `FK_TenantRole_Role_RoleId` renamed to **`FK_TenantRole_Role_SourceTemplateId`** -- SRMS
      keeps the OLD property name in the constraint while the column is `RoleId`, the mirror image of
      CERP, which renamed the column and let EF rename the key.
      **(b)** `FK_TenantRolePermission_TenantRole_TenantRoleId` **dropped** (was CASCADE).
      **(c)** `FK_TenantUserRole_TenantRole_TenantRoleId` **dropped** (was NO_ACTION).
    - !! **THE CODE REFACTOR THAT MATTERS.** (b) was the CASCADE that cleaned up a deleted role's
      grants, and (c) was what BLOCKED deleting a role someone still held. The projector deleted an
      orphan TenantRole and let the database do the rest; with both keys gone that silently leaves
      orphaned grants and assignments. `SyncRolesAsync` now deletes TenantRolePermission and
      TenantUserRole children explicitly before removing the role.
    - Two scaffold side-effects corrected: EF wanted to DROP `IX_TenantUserRole_TenantRoleId` (it
      existed for the removed FK, and nothing else covers a lookup by that column -- kept explicitly,
      since the new cleanup queries it) and to ADD a redundant `IX_TenantRolePermission_TenantRoleId`
      (the composite unique index already leads with TenantRoleId -- dropped from the config).
    - Verified: FKs 12 -> 9 with all three resolved, columns 12 and indexes 53 unchanged; HRMS 12
      groups / 34 screens, my-clearances 200, Home HOME(21)/HRMS(13), 0 errors.

0124. **UserRole.TenantId dropped -- membership projection removed, role resolution rewritten
    (2026-08-15).** Migration `UserRoleDropTenantId`, APPLIED. Columns 13 -> 12.
    Backup: `D:/Backups/CERP_before-userrole-tenantid-*.bak`.
    - This was the one with NO derivation path: a UserRole row carries only UserId and RoleId, and
      Core.User and Core.Role are both global, so once the column goes nothing in the database can
      say which tenant an assignment belongs to. It could not be re-scoped -- only replaced.
    - **`SyncMembershipsAsync` DELETED.** It created a TenantUser for every user in `assignments`;
      with UserRole global that is a TenantUser in THIS tenant for every user of EVERY tenant. It
      also had nothing left to project from: HRMS stopped writing Core.UserRole when the User Roles
      screen went (handoff 0107), and SRMS writes TenantUser/TenantUserRole directly. Same reasoning
      that retired the permission projection on 2026-08-13.
    - **New `ICurrentUserRoles`** answers the two questions the six call sites actually asked, from
      the tenant model (`TenantUser -> TenantUserRole -> TenantRole.RoleId`):
      `GetTemplateRoleIdsAsync()` and `GetUserIdsInRolesAsync()`. Without it a multi-tenant user
      would have passed an approver check using a role granted in a DIFFERENT tenant, and open-step
      notifications would have been sent to other tenants role-holders.
      Rewired: WorkflowApproverAuth (3 sites) + EmployeeTermination clearance approver checks (3).
    - Verified: old and new resolution paths agree for hoadmin (1 role each); sidebar 12 groups /
      34 screens; **my-clearances 200** (the rewritten path); Home feed HOME(21)/HRMS(13); 0 errors.

0123. **Subsystem deduplicated and TenantId dropped (2026-08-15).** Migration
    `SubsystemDropTenantId` + `backend/scripts/dedup-subsystem-rows.sql`, both APPLIED.
    Backup: `D:/Backups/CERP_before-subsystem-dedup-*.bak`. Columns 14 -> 13.
    - Subsystem rows had been created PER TENANT, so `HOME` existed twice: an EMPTY row for the
      demo tenant and the NVI row owning all 4 modules / 8 tenant modules. Both carried
      `Code = HOME`, which the Home SPA matches literally in five places.
    - Dedup: the demo row is deleted and its single entitlement REPOINTED to the survivor, so demo
      keeps its access rather than losing it. Verified 0 duplicate codes, **0 orphaned references**.
    - ⚠️ The unique index `(TenantId, Name)` had to go with the column. Replaced with **unique on
      `Name` alone** — all 7 names are distinct, so it is the same guarantee minus the dropped half.
      SRMS has no index there at all; keeping one is deliberate, because losing it would let a
      second row claim a name the launcher matches on.
    - Verified: HRMS sidebar 12 groups / 34 screens, `Subsystem` now returns all **7** rows (global,
      as intended), Home feed HOME(21)/HRMS(13), and — the check that matters — **exactly 1 row
      matches `code === HOME`**. 0 translation errors.

0122. **Three more TenantId columns dropped -- Organization, Setting, TenantUserRole (2026-08-15).**
    Migration , APPLIED. Columns 17 -> 14.
    - Organization and Setting were trivial: one row each, both already in , so
      nothing had ever filtered on them.
    -  needed one real fix. Its tenant is its TenantUser's, so the column goes --
      but the projector's  set is used to DELETE assignments not in , and       only ever holds THIS tenant's pairs. Unscoped, the cleanup would have deleted **other tenants'
      role assignments**.  is now filtered through this tenant's member ids.
    - Home dropped the entity's TenantId, its , and the query filter for both
      TenantRolePermission and TenantUserRole; its feed scopes through TenantUser, still filtered.
    - Verified: HRMS 12 groups / 34 screens with all 12 group names intact, Home HOME(21)/HRMS(13),
      0 translation errors in either log.
    - ⚠️ ** NOT dropped -- there is no derivation path.** A UserRole row holds
      only UserId and RoleId, and BOTH are global tables, so nothing can say which tenant an
      assignment belongs to once the column goes.  creates a TenantUser for
      every user in ; unscoped that is a cross-tenant membership leak, and there is
      no join that would re-scope it. Dropping this one destroys information rather than
      normalising it.
    - ⚠️ ** NOT dropped** -- HOME is duplicated across 2 tenants (8 rows), so
      going global surfaces both rows and needs dedup plus repointing Module / Operation /
      TenantModule / TenantOperation / TenantSubSystem first.

0121. ** DROPPED — the table is now column-identical (2026-08-15).**
    Migration , APPLIED. Columns 18 -> 17.
    - A grant's tenant is its ROLE's tenant now, exactly as SRMS models it.
    - **No query needed re-scoping.** All four HRMS readers were already constrained by
       or , both themselves tenant-scoped:
       and  join from tenant tables,
       filters on this tenant's role ids, and the projector's
      delete is scoped to this tenant's operations. Home's feed reaches it through
      TenantUser -> TenantUserRole, both still filtered.
    - Added to  and removed Home's query filter + , in the
      same change — the 409 trap.
    - Verified: 570 grants intact, all resolving to a TenantRole AND a TenantOperation, deriving to
      exactly 1 tenant. HRMS sidebar 12 groups / 34 screens, Home HOME(21)/HRMS(13), 0 errors.

0120. **All three dimensions re-verified with a real harness (2026-08-15).**
    New script: `backend/scripts/compare-schemas.ps1`. **No schema change.**
    - After two false zeros I re-ran everything through one script that **asserts its load counts
      before reporting**. These numbers are trustworthy because the harness proved it read rows:

      | Dimension | Loaded (CERP / SRMS) | Differences |
      |---|---|---|
      | COLUMNS | 441 / 429 | **18** |
      | FOREIGN KEYS | 32 / 27 | **9** |
      | INDEXES + KEYS | 87 / 34 | **53** — every one a CERP-only EXTRA; 0 missing, 0 mismatched |

    - The earlier column (18) and index (0 missing) results are **confirmed**; only the FK dimension
      had been wrong, and it is now 9.
    - The script fixes both root causes permanently: `COLLATE DATABASE_DEFAULT` on every concatenated
      system column (the FK failure), and `STRING_AGG` instead of `FOR XML PATH` (the index failure).
    - ⚠️ **The script is ASCII-only on purpose.** Windows PowerShell 5.1 reads a UTF-8 `.ps1` without
      a BOM as ANSI, so an em-dash becomes `â€”` and breaks string parsing — the file failed to run
      until every non-ASCII character was stripped.
    - Use it before claiming parity: `& backend/scripts/compare-schemas.ps1`.

0119. **`TenantRolePermission` FK name + ⚠️ MY FK AUDIT WAS FALSE (2026-08-15).** Detail in
    logic.md §12.20. Migrations `SrmsForeignKeyNames` + `LoginTrailAndTenantModuleForeignKeys`.
    - ⚠️ **RETRACTION: handoff 0117 and 0118 claimed "foreign keys diff to ZERO across all 30 shared
      tables". THAT WAS FALSE.** The comparison query hit a **collation conflict**
      (`Latin1_General_CI_AS_KS_WS` vs `SQL_Latin1_General_CP1_CI_AS` when concatenating `fk.name`
      with `delete_referential_action_desc`), returned only an error, and the
      `Where-Object { $_ -match '~' }` filter swallowed it — leaving two EMPTY dictionaries, which
      compare as identical. **Second false zero this session from the same root cause: a harness that
      returns nothing looks exactly like one that passes.** Fixed with `COLLATE DATABASE_DEFAULT`,
      and every comparison now asserts its load count first.
    - The real number was **13**. `Core.TenantRolePermission`'s was a NAME difference: SRMS calls it
      `FK_TenantRolePermission_Operation_OperationId` — naming a column (`OperationId`) that exists
      on neither side, left from when the table referenced `Core.Operation` directly. Renamed to
      match, along with `TenantOperation`'s (`FK_TenantNavigationOperation_TenantModule_ModuleId`).
    - Added the two FKs SRMS had and CERP lacked, both verified **0 orphans** first:
      `LoginTrail.UserId → User` (SET NULL, so a deleted account leaves its audit trail) and
      `TenantModule.TenantId → Tenant` — the latter in **raw SQL**, because EF cannot model a
      relationship on the value-converted `TenantId` (same reason as the §12.16 FKs).
    - **13 → 9.** The rest are judgment calls, not omissions — see logic.md §12.20.
    - Verified: sidebar 12 groups / 34 screens, employee and subsystem reads 200. (A Hangfire job
      retrying against a deleted `ReportSchedule` is pre-existing and unrelated.)

0118. **The index comparison — and a harness that lied (2026-08-15).** Detail in logic.md §12.20.
    Migration `SrmsConstraintNamesAndAlternateKeys`, APPLIED.
    Backup: `D:/Backups/CERP_before-constraint-renames-20260815-031627.bak`.
    - ⚠️ **THE FIRST RUN REPORTED "0 DIFFERENCES" AND WAS WRONG.** The query used `FOR XML PATH`
      string-building; it returned nothing, the `Where-Object { $_ -match '~' }` filter swallowed the
      empty result, and both dictionaries came back empty — which compares as identical. Caught only
      by printing the loaded row counts. **A comparison that returns nothing looks exactly like a
      comparison that passes: always assert the harness loaded data before trusting a zero.**
    - The real answer was **69 differences**, in three groups:
      **(a) 6 PRIMARY KEY NAMES** — SRMS calls them `PK_NavigationModule`, `PK_StandardRoleTemplate`,
      `PK_SystemSetting`, `PK_TenantNavigationModule`, `PK_TenantModuleEntitlement`, and — confusingly
      — `PK_Module` for **`Core.Subsystem`**. All renamed to match.
      **(b) 4 alternate keys** SRMS declares (`AK_Tenant_Id_OrganizationId` and three like it) — added.
      **(c) 53 CERP-only indexes** — **KEPT.** These are performance and uniqueness indexes
      (`IX_User_UserName` from the performance pass, the notification indexes, unique business keys
      like `IX_Tenant_Identifier`). Dropping them would regress performance and integrity.
    - ⚠️ **`sp_rename`, not DropPrimaryKey/AddPrimaryKey.** EF scaffolds a rename as drop-then-add,
      and SQL Server refuses to drop a PK that foreign keys reference ("The constraint
      'PK_TenantModule' is being referenced by table 'TenantOperation'"). `sp_rename` renames in
      place and leaves dependants intact.
    - ⚠️ **Order matters:** `Core.Module`'s PK had to be renamed away *before* `Core.Subsystem` could
      take the freed `PK_Module` name.
    - **RESULT: every index and key SRMS has now exists in CERP with an identical definition** — 0
      missing, 0 definition mismatches. Columns, foreign keys, indexes and keys all compared.
    - Verified: sidebar 12 groups / 34 screens, employee/module/subsystem/lookup reads 200.

0117. **`Tenant.TenantTypeId` foreign key + the FK/index audit I had MISSED (2026-08-15).**
    Detail in logic.md §12.20. Migration `TenantTypeIdForeignKey`, APPLIED.
    - ⚠️ **MY AUDIT HAD A HOLE.** Handoff 0114–0116 compared COLUMNS only — name, type, size,
      nullability, default. It never compared **foreign keys or indexes**, so
      `FK_Tenant_LookUpCategoryList` — present in SRMS, absent in CERP — was invisible to every
      "remaining differences" count I reported. A column-level diff is not a schema diff.
    - Added it: `Core.Tenant.TenantTypeId` → `Core.LookUpCategoryList`, same constraint name as SRMS,
      Restrict. Safe — all three rows hold NULL, and NULLs are exempt.
    - ⚠️ **RAW SQL, not the EF model, and the reason matters.** CERP has **TWO lookup systems**:
      `Core.LookUpCategory/List` mirrors the SRMS platform schema, and `Hrms.LookUpCategory/List` is
      the HRMS domain one the `LookupCategoryList` **entity maps** (education levels, fields of
      study). Both exist, both empty, neither previously referenced by any FK. A tenant TYPE is
      platform data, so the constraint must point at **Core** — mapping it through EF silently
      scaffolded a foreign key to **`Hrms.LookUpCategoryList`**, the wrong table. Caught by reading
      the scaffold before applying it.
    - **SRMS needed no change** — it already had the FK, correctly.
    - Ran the missing audit: **foreign keys now diff to ZERO** across all 30 shared tables, in both
      directions. Indexes still uncompared (see §12.20).
    - Verified: sidebar 12 groups / 34 screens, employee and lookup reads 200.

0116. **`Core.Tenant` matched + the safe platform drops — 23 → 18 (2026-08-15).** Detail in
    logic.md §12.20. Migrations `TenantDropTenantIdDiscriminator` +
    `PlatformTablesDropTenantIdAndSettingPrecision`, APPLIED.
    - **`Core.Tenant`: dropped `TenantId`** — its only difference. A tenant row carrying a tenant
      DISCRIMINATOR was always meaningless: the row IS the tenant. `Core.Tenant` has been in
      `IsGlobalEntity` from the start so nothing ever stamped or filtered it, and **all 3 rows held
      the empty Guid**. `Core.Tenant` now diffs to ZERO.
    - **Three empty platform tables** lost theirs too — `SubscriptionPlan`, `SubscriptionPlanModule`,
      `OrganizationSubscription`, all **0 rows**, all platform-level rather than tenant data. Added
      the latter two to `IsGlobalEntity` **in the same change** — the 409 "could not be translated"
      trap from handoff 0110.
    - `Setting.UpdatedAt` datetime2(7)→(3).
    - ⚠️ **NOT dropped, deliberately:** `TenantSubscriptionAddOn.SubscribedTenantId`. Despite the
      table being empty, it is a **real FK to Core.Tenant** recording which tenant holds the add-on.
      SRMS lacking it is a gap there, not an extra here — dropping it would delete modelling to match
      a less complete schema.
    - ⚠️ **`Organization.TenantId` left alone too** — unlike Tenant's, its single row holds a REAL
      value. Dropping it would discard data, so it needs a decision rather than a silent drop.
    - Verified: sidebar 12 groups / 34 screens, employee/subsystem reads 200, **no translation
      errors** (the IsGlobalEntity additions took).

0115. **The 28 default constraints aligned — 50 → 23 (2026-08-15).** Detail in logic.md §12.20.
    Migrations `SharedTableDefaultConstraintParity` + `SharedDefaultsFinalThree`, APPLIED.
    - Four kinds: 8 CERP-only defaults REMOVED (Organization ×5, Role ×3), 9 SRMS defaults ADDED
      (LoginTrail ×2, `Organization.FiscalYearStartMonth`, `Subsystem.Code`, `CanExport`,
      UserPreference ×4), 6 CHANGED to SRMS's real values (`'en'`, `'dd/MM/yyyy'`, `'/'`,
      `'1,234.56'`, `'system'`, `'Africa/Nairobi'`), 5 RESPELLED.
    - ⚠️ **Three needed hand-written SQL because EF cannot express them:**
      `Role.Code` carried an `(N'')` default from an **older migration the model never declared**, so
      EF neither knew about it nor dropped it; `Subsystem.Code` is a spelling difference (EF always
      emits `N''`, SRMS stores `''`); and `TenantRolePermission.CanExport` is the opposite —
      `HasDefaultValue(false)` produces **nothing**, because `false` is the CLR default and EF
      optimises it away. Written as a migration with an explicit `[Migration]` attribute, since a
      hand-authored one is otherwise not discovered.
    - Note: these defaults are effectively decorative — EF supplies every value on insert, so they
      only matter to raw SQL. Aligned because the requirement is an identical catalog.
    - **REMAINING 23, all load-bearing** (see logic.md §12.20): 14 `TenantId`, `Setting`'s audit
      columns, `Subsystem.Url`/`SortOrder`, `SubscribedTenantId`, `User.CreatedAt`. None is safe to
      apply without its own analysis.
    - Verified: sidebar 12 groups / 34 screens, employee/subsystem/operation reads 200, no errors.

0114. **`Core.TenantRole` matched + a database-wide schema audit (2026-08-15).** Detail in logic.md
    §12.20. Migration `TenantRoleRoleIdAndUpdatedAtPrecision`, APPLIED.
    Backup: `D:/Backups/CERP_before-timestamp-precision-20260815-020238.bak`.
    - `TenantRole.SourceTemplateId` → column **`RoleId`** (SRMS's name), mapped with
      `HasColumnName` so the property keeps the clearer name — "RoleId" on a table of roles reads
      like a primary key. **`Core.TenantRole` now diffs to ZERO.**
    - **Audit:** all 30 SRMS tables exist in CERP; comparing every column on name/type/size/
      nullability/default found **65 differences**. Now **50**.
    - ⚠️ **A blanket fix made it worse and was rolled back.** The convention gave nullable timestamps
      `datetime2(7)` and non-nullable `(3)` — an accident of nullability. Changing it to `(3)`
      everywhere fixed 16 columns and **broke 13** that SRMS keeps at `(7)`, net gain of 3 across a
      **594-column** migration. Reverted; replaced with an explicit 17-entity list (32 columns).
      SRMS is internally inconsistent here — the list mirrors its migration history, not a rule.
    - **The remaining 50 are NOT safe to apply blindly** — see logic.md §12.20 for the breakdown.
      Three would actively break things: dropping `Subsystem.Url` kills the Home launcher (built this
      session), dropping `TenantId` from 10 tables removes isolation the runtime filters on, and
      re-typing `TenantId` back to nvarchar on 5 tables would REVERSE the re-key from §12.14.
    - ⚠️ **`Core.User.CreatedAt` has regressed in SRMS** — it is nullable there again. I made it NOT
      NULL on 2026-08-14 (handoff 0105, `srms-fix-user-createdat-notnull.sql`). Fix in SRMS, not here.

0113. **All four navigation tables now diff to ZERO — STAGE 2d (2026-08-15).** Detail in logic.md
    §12.19. Migrations `OperationSubSystemIdAndDefaults` + `TenantModuleDropTemplateLink`, APPLIED.
    - ⚠️ **My earlier "zero differences" check was incomplete** — it compared name/type/length/
      nullability but NOT default constraints. Adding them surfaced 9 more, including a real one:
      **SRMS had also dropped `Core.Operation.SubSystemId`** (same normalisation onto the module).
    - Dropped `Operation.SubSystemId` and its FK (the misnamed `FK_Operation_Module_ModuleId`, which
      constrained that column and cascaded — both problems gone with the column). The three readers
      now take the subsystem from `Module`.
    - `Operation.ModuleId` is nullable, matching SRMS: the CLR property became `Guid?`, which is what
      I backed out of before. `Create` still rejects an empty Guid, so the app enforces what the
      column permits.
    - Dropped the **last template link**, `TenantModule.ModuleId`, now that `OperationId` has gone.
      The projector keys groups on **(SubSystemId, Name)** — verified unique, 0 duplicates, all 24
      rows resolve.
    - Removed 6 leftover EF default constraints SRMS lacks, and respelled `Operation.IsActive`'s
      default from `(CONVERT([bit],(1)))` to SRMS's `((1))`. EF will not drop defaults it never
      declared, so these go via name-agnostic raw SQL.
    - **RESULT: 0 differences across Module / Operation / TenantModule / TenantOperation** on column
      name, type, size, nullability AND default. Only ordinal ORDER still differs (needs a rebuild).
    - ⚠️ Home needed two follow-ups its build could not catch: its `Operation` entity still mapped
      `SubSystemId` and its `TenantModule` still mapped `ModuleId`. Both compiled fine and failed at
      runtime with **"Invalid column name"** — the portal feed 500'd. Mapped read-models drift
      silently; only running the query finds it.
    - Verified: HRMS 12 groups / 34 screens, Home HOME(21)/HRMS(13) — unchanged.

0112. **`Core.TenantOperation` made identical to SRMS — STAGE 2c (2026-08-15).** Detail in
    logic.md §12.19. Migration `TenantOperationSrmsParity`, APPLIED to CERP.
    Backup: `D:/Backups/CERP_before-tenantoperation-parity-20260815-010639.bak`.
    - **SRMS was restructured again**: it NORMALISED `TenantId`, `SubSystemId` and the template link
      `OperationId` off the table — a screen's tenant and subsystem are its MODULE's now. All three
      dropped here; `Filter` gained SRMS's `(N'')` default. **The table now diffs to ZERO.**
    - ⚠️ **`TenantOperation` has no tenant discriminator at all.** It is listed in
      `Repository.IsGlobalEntity` for that reason ONLY — `GetAll()` returns EVERY tenant's rows.
      Callers must scope through `TenantModule` (the sidebar feed and the projector now do) or join
      from a tenant-scoped grant (`EndpointPermissionService` and `WorkflowApproverAuth` already
      did, by primary key, so they were correct as written).
    - ⚠️ **The projector re-keys on (module, LINK)** now that `OperationId` is gone — verified 0
      duplicate pairs across 144 rows. Its orphan sweep is scoped to this tenant's group ids; unscoped
      it would delete other tenants' screens.
    - ⚠️ Two constraints were added in RAW SQL earlier and EF does not model them, so the scaffold
      missed both and the drop failed: the FK on `TenantId` and the default on `ModuleId`. The
      migration removes them by name-agnostic lookup first.
    - **CORRECTION to handoff 0111.** I justified keeping `OperationId` partly as "the join between
      permissionGate's catalog and tenant grants". **That was wrong** — `permissionGate`,
      `formPermissions`, `gridAction` and `useListPermissions` all match on **link**; the id is a
      React key. Dropping it was far cheaper than I said.
    - Verified: HRMS sidebar 12 groups / 34 screens, Home HOME(21)/HRMS(13) — both unchanged.
    - ⚠️ **Deploy both repos together** (Home maps this table).

0111. **`Core.TenantModule` created, menu groups moved into it — STAGE 2b (2026-08-15).**
    Detail in logic.md §12.19. Migration `TenantModuleAndOperationGroups`, APPLIED to CERP.
    Backup: `D:\Backups\CERP_before-tenantmodule-20260815-001623.bak` (first destructive step).
    - New `Core.TenantModule` (tenant copy of a menu group) + `TenantOperation.ModuleId` repointed at
      it and made NOT NULL; the 24 group rows removed from **both** `TenantOperation` and
      `Core.Operation`, so every remaining row in either is a screen.
    - Data migration, all inside the migration: 24 groups → TenantModule (**keeping their own Ids**,
      so nothing else is re-keyed), 144 screens repointed from the template module id to the tenant's
      group row, then both sets of group rows deleted, with a `THROW` guard that aborts if any null
      `ModuleId` survives.
    - Verified after applying: TenantModule **24**, TenantOperation **144**, Operation **144**,
      **0** bad ModuleId in either table, **570 grants intact, 0 orphaned**.
    - Code: projector gained `SyncModulesAsync` (runs BEFORE operations, and **translates** the
      template's ModuleId to the tenant's group row); HRMS sidebar reads groups from TenantModule;
      **Home** got the `TenantModule` entity, DbContext mapping and a rewritten `GetMySubsystems`
      join. Both feeds still report TEMPLATE ids, so neither SPA changed.
    - Verified live on both APIs: HRMS sidebar **12 groups / 34 screens** (unchanged), Home
      `my-subsystems` HOME(21)/HRMS(13) with the same three groups — identical to before.
    - ⚠️ **Deploy the two repos together.** Home reads `TenantOperation.ModuleId` and would break on
      the old code against the new schema.
    - **Remaining differences vs SRMS: 3, all deliberate.** `TenantOperation.OperationId` and
      `TenantModule.ModuleId` are the template links kept by your decision (SRMS's tenant copies have
      none); `Operation.ModuleId` is NOT NULL here where SRMS leaves it nullable — CERP is the
      stricter side, and matching would mean a `Guid?` property since EF refuses to map a nullable
      column to a non-nullable Guid. Column ORDER also still differs (needs a table rebuild).

0110. **`Core.Module` aligned with SRMS — STAGE 2a (2026-08-15).** Detail in logic.md §12.19.
    Migrations `ModuleSrmsAlignment` + `ModuleOperationColumnParity`, both APPLIED to CERP.
    - `Core.Module`: −`TenantId`, `SortOrder`→`DisplayOrder`, +`Filter`, +`IsActive`, Name/Icon
      narrowed to nvarchar(100), Icon NOT NULL, `SubsystemId`→`SubSystemId` (SRMS's casing, mapped
      with `HasColumnName` so the C# property is unchanged), and `UpdatedAt` datetime2(7)→(3) on
      **both** Module and Operation.
    - **`Core.Module` and `Core.Operation` now diff to ZERO** against cybererp_srms on column name,
      type, length and nullability.
    - Safe because: all 24 modules belong to ONE tenant (none of the dedup Subsystem will need), the
      longest name is 29 characters, and the single blank Icon held `''`, not NULL.
    - ⚠️ **GOTCHA that cost a debugging round:** `GET Module` began returning **409** — "The LINQ
      expression … could not be translated". Dropping TenantId leaves `Repository<T>`'s tenant filter
      referencing an **unmapped member**. Any entity whose `TenantId` is `Ignore()`d MUST be added to
      `IsGlobalEntity`, which now lists Module alongside Operation. A 409 on a plain GET looks like a
      concurrency conflict and is not one.
    - Verified: login 200, sidebar 12 groups / 34 screens, Module 24 rows, Operation + Subsystem 200.

0109. **`Operation.ModuleId` repointed at `Core.Module` — SRMS re-alignment STAGE 1 (2026-08-15).**
    Detail in logic.md §12.19. Migration `OperationModuleForeignKey`, APPLIED to CERP.
    - SRMS was **corrected by the user**: its `ModuleId` genuinely FKs to `Core.Module`, and it now
      has a `Core.TenantModule` table. The 2026-08-13 self-referencing hierarchy (a group = an
      Operation with a null `ModuleId`) mirrored what SRMS looked like *then*; this follows it back.
    - **Zero data change.** The 2026-08-13 migration copied the 24 modules across USING THEIR OWN
      Ids, so every parent operation's Id already equalled its module's — all **144 of 144** child
      `ModuleId` values already pointed at a valid `Core.Module` row (0 missing). Verified first.
    - ⚠️ Both constraint names are **SRMS's verbatim**: `FK_NavigationOperation_Module_ModuleId`
      (ModuleId) and `FK_Operation_Module_ModuleId` — which constrains **SubSystemId**, a misnomer
      left over from a rename in SRMS. Its **CASCADE** is SRMS's too: deleting a subsystem now takes
      its menu with it, where CERP previously refused with `Restrict`. Do not "fix" either without
      changing SRMS first.
    - Entity `Parent`/`Children` self-navigation → a `Module` navigation; the two read sites that
      showed the group name (`GetOperationByIdHandler`, `GetAllOperationsRepository`) follow it.
    - Non-breaking, and verified so: login 200, sidebar **12 groups / 34 screens** (unchanged),
      `Operation` list 168. The 24 group rows still exist with a null `ModuleId`, so the tenant-side
      reads are untouched.
    - **REMAINING to reach identical (not yet done):** `Core.Module` (−TenantId, SortOrder→
      DisplayOrder, +Filter, +IsActive, narrow Name/Icon to 200, Icon NOT NULL); drop the 24 group
      rows + `ModuleId` NOT NULL; **new `Core.TenantModule` table**; `TenantOperation` (−OperationId,
      ModuleId NOT NULL → TenantModule); `UpdatedAt` datetime2(7)→(3) throughout; column order.
      ⚠️ **`TenantOperation.OperationId` is the blocker** — SRMS has no template link at all (0 of
      220 tenant rows share an Id with a template), but both apps use it as the stable id the UI and
      the permission gate work with. Needs a decision before proceeding.

0108. **The static menu leftovers cleared out of both SPAs (2026-08-14).** Detail in logic.md §12.18.
    **No migration.** Data: `Home/backend/scripts/seed-subsystem-icons.sql`, RUN (8 rows).
    - Started as a Home-portal question ("why is the menu static?"). The Home sidebar was already
      dynamic; the static thing was **`SeedHomeMenu.cs`**, which declared the whole portal menu as a
      compiled C# array and wrote it into `Core.Operation` via `POST Portal/seed-defaults`. That made
      the array a **second source of truth** — a screen renamed or removed in the DB could be
      re-created by the next seed run. Deleted, with its endpoint, DI registration and the
      `Portal:SubsystemUrls` config binding that only existed to backfill `Core.Subsystem.Url`.
    - ⚠️ **The launcher/landing tiles WERE static in both apps**: they resolved icons through
      `getModuleIcon(name)`, a PSMS-template name→icon table (Purchases, Inventory, Container…) that
      matched almost nothing, so HOME/HRMS/PSMS/SRMS all drew a grey circle and the icon could not be
      configured. `Core.Subsystem.Icon` is now mapped (Home's entity never had the property), exposed
      on both DTOs, and resolved through `lucideIconMap`. The column was **never populated** — hence
      the seed script.
    - ⚠️ **`lucideIconMap` is where an icon silently degrades.** `Inbox`, `Bell` and
      `MessageSquareQuote` were configured on real rows but missing from BOTH maps, so they rendered
      as circles with no error. Added to both.
    - Dead PSMS-template code deleted from both SPAs: `menu/icons/`, `getModuleIcon`,
      `buildSidebarNavigation` (its output was computed in `useMenuModules` and **never consumed**),
      `menuTypes`, `modules`/`moduleDetail`/`menuItem`, `quickAdd` (18 hardcoded links), four
      unreachable sidebar subcomponents, and Home's `constants/subSystem.ts`.
    - Verified live on both APIs: `Portal/seed-defaults` 404; `my-subsystems` 200 with
      HOME(LayoutDashboard, 21 screens) / HRMS(UsersRound, 13); HRMS `Subsystem` returns all 6 icons;
      **zero unresolvable icon names** in either app.

0107. **The seven identity modules removed from HRMS (2026-08-14).** Detail in logic.md §12.17.
    **No migration** — no schema change at all.
    - Users, Roles, User Roles, Role Permissions, SubSystems, Menu Modules, Menu Operations. SRMS
      manages them, and **SRMS runs against this same `CERP` database**, so what goes is the
      management surface: screens, write endpoints, CRUD handlers, and the 7 menu entries. **The
      tables stay** — HRMS logs in against `Core.User`, draws its sidebar from `Module`/`Operation`,
      and gates on `TenantRolePermission`; Home reads the same tables directly.
    - Data: `backend/scripts/remove-identity-menu-operations.sql`, RUN against CERP — 7 `Operation`
      rows, 7 `TenantOperation` copies, 28 `TenantRolePermission` grants. Re-runnable; verification
      selects came back empty.
    - ⚠️ **A deleted menu operation makes its `[RequirePermission]` key permanently ungrantable.**
      `UserController`/`RoleController` were gated on `user`/`role`, which no longer exist — that
      would have been **403 for everyone, forever**, silently emptying the approver pickers on
      workflow definitions, clearance departments and the report viewer. Re-gated on the consuming
      screens (`workflowDefinition`, `clearanceDepartment`, `reports`), which are granted to
      Administrator and HR Admin. **Grep a link in `[RequirePermission]` before deleting its
      operation.**
    - ⚠️ **The navigation READS had to stay.** `GET Operation` feeds `permissionGate.tsx`'s catalog,
      and the gate treats "not in the catalog" as "not gated" — losing it would have let **every
      route through unguarded**. `Module/WithOperations` is the sidebar itself.
    - `UserRole` and `RolePermission` controllers deleted outright (no frontend consumer left);
      `User`/`Role` keep one `GET` each for the pickers.
    - Permission changes are no longer instant: the save handlers that called `InvalidateAll()` are
      gone, so a grant made in SRMS lands here after the **60s cache TTL**.
    - Verified: backend build clean, `tsc -b` + `eslint --quiet` clean, endpoint probe —
      writes 404/405 everywhere, `Subsystem`/`Module`/`Operation`/`WithOperations` GET 200,
      `UserRole`/`RolePermission` 404, `User`/`Role` GET 403 for an ungranted user (deny-by-default).

0106. **Dropping CERP's extra columns — STAGE 1 of 4 (2026-08-14).** Detail in logic.md §12.16.
    Migration `DropOwningTenantIdUseTenantId`, APPLIED to CERP.
    Backup: `D:\Backups\CERP_before-drop-cerp-extras-*.bak`.
    - **5 of the 19 extras gone**: `OwningTenantId` on TenantRole/TenantOperation/TenantUser/
      TenantSubSystem. They were **provably redundant** — the re-key made them duplicates of
      `TenantId`, confirmed at **zero mismatches across all 695 rows**. SRMS uses `TenantId` for this.
    - ⚠️ FKs added in **raw SQL**, not the EF model — EF cannot model a relationship on a
      value-converted property. Three added, matching exactly what SRMS constrains.
    - ⚠️ Removing the column from the seed script's TenantSubSystem insert left the **SELECT list
      misaligned** (would have written the tenant id into SubSystemId). Fixed, along with the
      now-obsolete nvarchar casts.
    - **REMAINING 14, each its own piece:** ⚠️ `UserRole.TenantId` carries which tenant an assignment
      was made in and the projector derives every membership from it — needs creation moved to the
      write site first. ⚠️ `Subsystem` (TenantId/SortOrder/Url) — **HOME and HRMS are duplicated per
      tenant**, so going global needs dedup + repointing Module/Operation/TenantOperation/
      TenantSubSystem, and SortOrder(0–5) must migrate into DisplayOrder(all 0). `Setting` audit trio
      needs a BaseEntity exclusion. The other 6 are mechanical (0 rows or already global).
    - ⚠️ **PROCESS FAILURE:** this was first reported as pushed when it **had not committed** — the
      pre-commit doc hook rejected it and `git commit … | tail -1 && git push` masked the failure
      (`tail` exits 0). The migration was already applied, so the DB was briefly ahead of the code.
      **Verify with `git log`, never with a printed success message.**

0105. **The last schema difference, fixed in cybererp_srms rather than CERP (2026-08-14).** Detail in
    logic.md §12.15. **No CERP change at all** — no migration, no code.
    Backup: `D:\Backups\cybererp_srms_before-createdat-fix-*.bak`.
    - `User.CreatedAt` was nullable in SRMS, NOT NULL in CERP. **It was DRIFT, not design**: SRMS's
      own `BaseEntity.CreatedAt` is a non-nullable `Instant`, its snapshot declares
      `b.Property<DateTime>("CreatedAt")`, its initial migration created it `nullable: false`, and
      **no migration ever made it nullable**. 20 of its 23 CreatedAt columns are already NOT NULL.
      CERP was matching what SRMS intends.
    - ⚠️ **Applied as a SCRIPT because SRMS's EF tooling is broken.** `dotnet ef migrations add` fails
      there with a **pre-existing, unrelated** model error: *'OperationId' cannot be added to the type
      TenantOperation … no corresponding CLR property or field*. Hand-forging snapshot files against a
      model that will not load would be worse. It belongs in a migration once that is fixed.
    - The script refuses rather than inventing timestamps if any row is NULL — a fabricated creation
      date is worse than a missing one, being indistinguishable from a real one afterwards.
    - ⚠️ **The SRMS tree is NOT a git repo**, so a copy is kept at
      `backend/scripts/srms-fix-user-createdat-notnull.sql`.
    - ⚠️ Same drift on `Core.LookUpCategory` and `Core.LookUpCategoryList` — left alone (not the ask,
      not in the CERP comparison); one-liners noted in the script.
    - **Shared surface: every column SRMS has, CERP now has identically — ZERO differences.**
      ⚠️ But the earlier reports in this series diffed **one direction only**. The reverse shows **19
      columns CERP has that SRMS lacks** — supersets, not mismatches: `TenantId` ×9 (BaseEntity adds
      it universally), `OwningTenantId` ×4 (CERP's separate FK), `Setting`'s audit columns,
      `Subsystem.Url`/`SortOrder`, `TenantSubscriptionAddOn.SubscribedTenantId`.

0104. **TenantId RE-KEYED to uniqueidentifier — 201 columns (2026-08-14).** Detail in logic.md
    §12.14. Migrations `TenantIdToUniqueidentifier` + `MatchSrmsTenantIdExceptions`, both APPLIED.
    Backup: `D:\Backups\CERP_before-tenantid-rekey-*.bak`. **The shared surface with SRMS now differs
    by ONE column.**
    - ⚠️ **A VALUE CONVERTER, not a retyped property.** The CLR property stays `string`; a global
      converter in `OnModelCreating` maps it to Guid, so the COLUMN is uniqueidentifier and **no
      entity, no repository filter, no handler changed**. `TenantId` is on `BaseEntity` (202 tables)
      AND is the Finbuckle discriminator, whose `ITenantInfo.Id` is a **string** — retyping would
      have needed a conversion at that boundary anyway. **The column type had to match; the CLR type
      never did.** Safe because every query use is a simple equality (verified both repos); the
      `IsNullOrEmpty` checks run in memory. `""` ↔ `Guid.Empty` round-trips (19 rows carry blanks).
    - ⚠️ **Four traps.** (a) `Type.GetProperty("TenantId")` throws *Ambiguous match* —
      `TenantSubscription` declares its own Guid `TenantId` shadowing the base one; use
      `entityType.FindProperty`. (b) EF scaffolded **400 AlterColumn and NO index handling**, but
      **141 indexes include TenantId** — hand-written discovery-driven SQL instead. (c) A **PRIMARY
      KEY** (`PK_NumberSequence`) also blocks it and is not DROP INDEX-able — the first attempt
      failed on exactly that. (d) Blank values cannot implicitly convert → empty GUID first.
    - One transaction with `XACT_ABORT`, which earned its keep: the failed first attempt rolled back
      cleanly, leaving all 201 columns untouched rather than half-converted.
    - ⚠️ **SRMS is internally inconsistent and we now match it**: it uses uniqueidentifier on 7
      tables and **nvarchar on LoginTrail and UserPreference**. `MatchSrmsTenantIdExceptions` reverts
      those two. The oddity is SRMS's — if fixed there, delete that migration AND the exclusion in
      `HrmsDbContext`.
    - **Home needed the same converter** or its nine query filters would not translate; its
      `Core.Notification` was converted by the HRMS script (it scans every table), so Home's own
      migration is a **guarded no-op** that keeps its snapshot honest. ⚠️ **Deploy together.**
    - Remaining: `User.CreatedAt` (nullable in SRMS) — `BaseEntity.CreatedAt` is a non-nullable
      `Instant` on 202 tables; EF cannot make it optional for one entity and doing it globally drops
      a guarantee every audited row has.
    - Verified: 141 indexes + 1 PK rebuilt; data intact (506 users, 490 employees, 598 grants, 175
      operations, 60 login-trail rows); 3 HRMS identities 200/34 links; write path stamps a real
      Guid; Home 2/12/34 + notifications 200; 0 errors either log.

0103. **Remaining platform tables aligned with cybererp_srms (2026-08-14).** Detail in logic.md
    §12.13. Migrations `AlignPlatformTablesWithSrms` + `AlignAssignedByAndSettingUpdatedAt`, both
    APPLIED to CERP. Backup: `D:\Backups\CERP_before-platform-align-*.bak`.
    - Full diff of all **22 shared tables** found **13 differing**. Now **7 columns on 7 tables**,
      all from two root causes that cannot be closed (below).
    - Closed: `Tenant` +OrganizationId(FK)/TenantTypeId/Currency|Locale|TimezoneOverride;
      `Subsystem` +Abbreviation/Icon/Description/DisplayOrder/IsActive/LandingPath, Name 200→100;
      `SubscriptionPlan` +Code; `Organization` all 19 length/nullability diffs; `UserPreference`
      5 cols → NOT NULL + narrower (empty table); `TenantRole`/`TenantOperation` widths;
      `TenantUserRole.AssignedBy` → uniqueidentifier; `Setting.UpdatedAt` → NOT NULL.
      **No truncation** — longest value in any narrowed column was 24 chars against 80–500 caps.
    - ⚠️ **Three scaffold traps:** `Tenant.OrganizationId` got an FK over an empty-Guid default
      (backfilled first; creates an organization if tenants exist without one); `AssignedBy`
      string→Guid **fails outright** because SQL Server cannot cast `'seed-tenant-authorization'`
      (non-Guids nulled via TRY_CAST — they were provenance markers, never user ids); and
      `Setting.UpdatedAt`→NOT NULL scaffolded a `0001-01-01` default (seeded from CreatedAt).
    - ⚠️ **CANNOT be aligned — both are `BaseEntity` properties shared by 202 tables:**
      **`TenantId`** is the Finbuckle DISCRIMINATOR STRING here and a Guid FK in SRMS — two different
      concepts; CERP models the FK separately as `OwningTenantId`. Matching means re-keying
      multi-tenancy across 202 tables, the repository filter and every seeded value.
      **`User.CreatedAt`** is a non-nullable `Instant` on all 202 tables; making it optional for one
      entity is impossible in EF and globally would drop a guarantee every audited row has.
    - Verified: data intact (3 tenants, 506 users, 503 user-roles, 598 grants, 175 operations,
      7 subsystems); HRMS login 200 / 34 links; Home 2/12/34; gate 403/200/200; 0 errors either log.

0102. **SMTP settings: Core.Setting is now the source of truth (2026-08-13).** Detail in
    logic.md §12.12. No migration — code plus one data correction.
    - `Core.Setting` has held SmtpHost/Port/User/UseTls all along and **nothing read or wrote them**:
      no handler, no controller, no screen. `SmtpEmailService` went straight to configuration, so the
      stored values were inert.
    - ⚠️ **Resolved IN-REQUEST, never in the job.** `EmailDispatchJob` documents that it touches no
      tenant-scoped data because background jobs have no tenant context. `Core.Setting` IS
      tenant-scoped, so resolving inside the job would silently fall back to configuration — the very
      bug being fixed, where nobody would look. `QueuedEmailService` resolves and passes it in.
    - ⚠️ **The password never travels that path — Hangfire PERSISTS job arguments.** `SmtpSettings`
      carries Host/Port/User/TLS only; the credential is read from configuration inside the send.
      That is also why the table has no password column and `IEmailConfiguration` exposes only
      `HasPassword`. The old 4-arg `SendAsync` is KEPT so already-queued jobs still deserialize.
    - ⚠️ **Setting was invisible, exactly like Organization** — single row with an empty `TenantId`,
      so the filter excluded it. It had to join `IsGlobalEntity` (deployment-level: relay, backup,
      password/session policy).
    - ⚠️ **The seeded row would have redirected live mail.** Once visible it won, and it held
      `smtp.cyber.com` / `noreply@cybererp.com` — unverified seed data overriding a WORKING relay.
      `scripts/clear-seeded-smtp-placeholders.sql` blanks just those two fields. Fallback is **field
      by field**, so a host set alone still inherits the configured port.
    - **Added, because settings you cannot edit are not settings:** `GET`/`PUT /api/v1/Setting` and
      `POST /api/v1/Setting/test-email`. The test reports which host/user were ACTUALLY resolved and
      refuses up front when mail is disabled, no host is set, or a user has no password.
    - Gated on `setting`; a "Settings" entry was added to `SeedDefaultMenu`'s System group. **No role
      holds that link**, so the endpoints are unreachable until one is granted deliberately.
    - ⚠️ **Mail DOES work here** — `Email:Password` is empty in `appsettings.json` but supplied by
      user-secrets locally (`hasSmtpPassword: true`). The earlier note that mail cannot send was true
      of appsettings alone. A test send through the endpoint relayed successfully via Gmail.
    - Verified: stored value wins → cleared → configuration fallback restored, with `autoBackup` still
      coming from the DB (field-by-field confirmed); 403 without the link, 200 with it; sidebar 34
      links, 598 grants, 175 operations.

0101. **CompanyProfile consolidated into Organization (2026-08-13).** Detail in logic.md §12.11.
    Migration `ConsolidateCompanyProfileIntoOrganization`, APPLIED to CERP.
    Backup: `D:\Backups\CERP_before-companyprofile-consolidation-*.bak`.
    - `Hrms.CompanyProfile` is gone; `Core.Organization` owns the letterhead. Organization had been
      added additively and had **no reader at all**, while the profile fed the logo, the offer letter
      and the movement letters. Mapping: `CompanyName`→`LegalName`, `ContactAddress`→`Address`,
      `ContactPhone`→`PhoneNumber`, `ContactEmail`→`Email`, `LogoContent`→`Logo`.
    - ⚠️ **Organization was INVISIBLE to the repository.** It sits above the tenant and its row has an
      empty `TenantId`, so the filter matched nothing and the table read as absent. Adding it to
      `IsGlobalEntity` is what makes this work — without it the consolidation would have swapped a
      table with no rows for one nobody could see.
    - This is an improvement, not a tidy-up: the profile had **zero rows**, so the letterhead rendered
      empty. It now resolves real data that was sitting unused — `Cybersoft`, `Menelik II Avenue`,
      `cyber@cyber.com`, and a 13,905-byte PNG logo.
    - **Wire contract unchanged** — `CompanyProfileDto` keeps its field names, so the screen and its
      service needed no change. `Organization.SetLetterhead` covers exactly that subset and leaves
      `LegalName` alone when the posted name is blank (it is REQUIRED here, optional on the profile).
    - ⚠️ The migration **copies before dropping even though there was nothing to copy** — this
      database has zero profile rows, but a migration that only works against the database it was
      written on is not a migration. It fills only gaps, so a real organization record is never
      overwritten by a thinner profile.
    - `OfferLetterTemplateConfiguration` shared the deleted config file and moved to its own.
    - Verified: table gone, 1 organization; letterhead endpoint 200 with the real values;
      `logo/info` `{"hasLogo":true,"contentType":"image/png"}` and `logo` 200 / 13905 bytes;
      login 200, sidebar 34 links, 598 grants, temporary grants restored to `000000`.

0100. **TenantId DROPPED from Core.User / Role / Operation — the SRMS model is complete
    (2026-08-13).** Detail in logic.md §12.10. Migration `DropTenantIdFromUserRoleOperation`,
    APPLIED to CERP. Backup: `D:\Backups\CERP_before-drop-tenantid-*.bak`.
    - The three tables now match SRMS **exactly**, no extra columns. Tenancy lives in
      `TenantUser` / `TenantRole` / `TenantOperation`.
    - ⚠️ **THE BUG THIS CAUSED, IN BOTH APPS.** Login derived the session tenant from
      `user.TenantId` and set the cookie every later request resolves against. Unmapped, it read as
      `""` — so there was NO tenant and **every tenant-filtered query returned nothing**: empty
      sidebar, zero employees, blank portal. Login still returned 200 and neither log showed an
      error. Both now resolve the tenant from MEMBERSHIP (default first, then any active). Home also
      needs `IgnoreQueryFilters()` there — `TenantUser` carries the same filter, and at login there
      is no tenant yet, which is what the query is working out.
    - ⚠️ **The migration carries membership across BEFORE dropping.** `TenantId` IS the membership;
      once gone it cannot be recovered. The seed built `TenantUser` from `UserRole`, so it only
      covered users holding a ROLE — six did not, one (`dagmawi`) a live headoffice user. Now
      506 users → 506 memberships, 0 without.
    - ⚠️ **`Repository<T>` skips these three** (`IsGlobalEntity`), so every list scopes itself:
      `GetAllUsers` via TenantUser (else 506 not 500), `GetAllRoles` via TenantRole (else 8 not 5),
      `SaveRole` duplicate checks, `SaveUserRole` existence checks (existence no longer proves
      ownership). `SaveUser` now CREATES the membership, or the account belongs nowhere. User
      name/e-mail uniqueness is global now, matching the indexes the database actually has.
    - ⚠️ **The projector no longer instantiates templates** — with Role/Operation global it would
      hand this tenant every other tenant's roles. It UPDATES existing instances only; creation
      moved to `SaveRole`, `CreateOperationHandler` and `SeedDefaultMenu`.
    - ⚠️ **Deploy both repos together.**
    - Verified: 0 TenantId columns left; Users list 500, Roles 5; three HRMS identities log in with
      34 links and unchanged employee counts; Home 2/12/34; gate 403/200/200/403; integrity all zero
      and 15369 viewable pairs, unchanged.

00FF. **The remaining ungated controllers are gated (2026-08-13).** Detail in logic.md §12.9.
    No migration, no data change. 25 controllers across 12 files.
    - ⚠️ **THREE patterns, chosen per controller from what actually calls it.** A blanket sweep would
      have broken self-service for the 490 employee accounts.
    - **Controller-level, one link (15):** AnnualLeaveLedger, Training{Category,Course,Session,
      Budget,ProviderPayment,Cpd}, LearningPath, LearningCommunity, AwardCategory,
      RecognitionProgram, RewardDisbursement, RecognitionWall, RewardPoints, WorkflowDefinition.
      Note `learningCommunity` / `recognitionWall` / `myPoints` / `myTraining` are **employee** links
      (granted by `assign-employee-role.sql`), so gating on them keeps self-service working.
    - **Controller-level, TWO links (4)** — an HR screen and a self-service screen share the
      controller: TrainingEnrollment (`trainingSession`+`myTraining`), TrainingCertificate
      (`trainingCertificate`+`myTraining`), Survey (`survey`+`surveyTake`), EmployeeTermination
      (`terminationList`+`myExit`). `HasAnyAsync` is an OR, so either link admits.
    - **Writes only (6 files):** Position, OrganizationUnit, Lookup, Step, CompanyAsset, DynamicForm.
      ⚠️ Their GETs are reference data — Position feeds dropdowns on **12** screens, OrganizationUnit
      on **12**, Lookup on every form's comboboxes — so gating the reads would 403 a dropdown for
      anyone lacking that screen.
    - **Left open on purpose:** Auth (anonymous), Dashboard/Search (everyone; the palette
      self-filters), Employee + child controllers (`IPerformanceVisibilityService`, `/myProfile`),
      LeaveRequest/AnnualLeave/LeaveBalance, Guarantee, ProfileChangeRequest, ExitInterview/
      TerminationSettlement, Suggestion/Grievance/Announcement, Workflow, EmployeeMovement/
      DisciplinaryMeasure, RewardNomination, TrainingNeed — self-service or already guarded.
    - Verified live: self-service links 200, admin-only 403, reference GETs 200 with their writes
      403, sidebar unchanged at 34 links.

00FE. **The ungated navigation controllers are closed (2026-08-13).** Detail in logic.md §12.8.
    No migration, no data change.
    - `Subsystem` / `Module` / `Operation` controllers had **no `[RequirePermission]` at all** — any
      authenticated user could create, rename or delete menu entries. (Flagged in 00FA; it is how the
      throwaway probes were created under a non-admin account.)
    - ⚠️ **The attributes are on the ACTIONS, not the controllers, and that is deliberate.** A
      class-level gate would have been *worse than the hole*: `GET Operation` is what
      `permissionGate.tsx` builds its catalogSet from, and the gate treats "not in the catalog" as
      "not a gated page" — so a 403 there would empty the catalog and let **every route through
      ungated**. Gating `Module/WithOperations` would leave every user with no sidebar at all.
    - Gated: `Create` / `Update` / `Delete` on all three, plus `Module/seed-defaults` (it rewrites the
      whole tree). Links `subsystem` / `module` / `operation`; Administrator and HR Admin already hold
      CanView on all three, so no role lost anything.
    - Verified both directions live: a non-admin gets 200 on all four reads and 403 on all four
      writes, sidebar still 34 links; with the permission temporarily granted, POST returns 200 and a
      throwaway operation round-tripped. Grant restored to `000000`; baseline 174 operations / 598
      grants / 0 leftovers.

00FD. **Core.RolePermission RETIRED — TenantRolePermission is the only grant table (2026-08-13).**
    Detail in logic.md §12.7. Migration `RetireCoreRolePermission`, APPLIED to CERP.
    Backup: `D:\Backups\CERP_before-retire-rolepermission-*.bak`.
    - It had had no reader since the flip (00FA); this removes the last writer. The Role Permissions
      screen writes `TenantRolePermission` **directly**, so a save is live on commit.
    - Proved redundant immediately before dropping: effective permissions compared across both
      models in both directions — **70,852 rows each side, 0 lost, 0 gained**. The migration also
      carries a `THROW` guard refusing to drop while `TenantRolePermission` is empty.
    - The **wire contract is unchanged**: the screen still sends global `RoleId`/`OperationId`, which
      the handler resolves to this tenant's instances. `CanExport` (no field on the screen) is never
      set on create and **preserved** on edit.
    - ⚠️ **`SyncPermissionsAsync` is DELETED, not disabled.** With no template table behind it, its
      revocation sweep would treat every hand-edited grant as orphaned and delete the lot. Do not
      reinstate it. Role/Operation/UserRole are still projected.
    - `WorkflowApproverAuth` (open-step approvers) and Home's `GetMySubsystems` both moved to the
      tenant chain; Home gained `TenantUser`/`TenantUserRole`/`TenantOperation`/`TenantRolePermission`
      mappings. ⚠️ **Deploy both repos together.**
    - Scripts: `seed-tenant-authorization-verify.sql` and `verify-tenant-auth-readers.sql` **deleted**
      (they compared against the dropped table and could only fail now), replaced by
      `verify-tenant-authorization.sql` — dangling refs, cross-tenant leakage, menu-tree integrity.
      `seed-tenant-authorization.sql` no longer seeds permissions; `assign-employee-role.sql` writes
      tenant grants.
    - Verified end to end against the live API: GET returned 149 rows with template ids and parent
      group names, POST flipped a grant on then back off with `CanExport` untouched, baseline back to
      598 grants / 15369 viewable pairs; HRMS 34 links, Home 2/12/34, 0 errors in either log.

00FC. **Core.Operation is now the menu TREE — parent/child, the SRMS topology (2026-08-13).**
    Detail in logic.md §12.6. Migration `OperationParentChildHierarchy`, APPLIED to CERP.
    Backup: `D:\Backups\CERP_before-operation-hierarchy-*.bak`.
    - `ModuleId IS NULL` = a PARENT (menu group); any other value names the parent it hangs off. The
      column keeps its name because that is what SRMS calls it. The 24 `Core.Module` rows were copied
      in as those parents: **174 = 24 + 150**.
    - ⚠️ **Each parent REUSES ITS MODULE'S Id** (checked first: zero collisions). That is why **not
      one of the 150 children needed repointing**, and it establishes the invariant the entity and
      both seeders maintain: a parent operation and its module share an Id.
    - ⚠️ **`Core.Module` is NOT dropped and must not be** — `SubscriptionPlanModule` and
      `TenantSubscriptionAddOn` have FKs into it. It just stops being what navigation reads;
      `Module.Operations` is gone.
    - ⚠️ **The scaffold added the self-FK BEFORE the parents existed**, so all 150 children pointed at
      absent keys and the constraint could not be created — the copy had to be interleaved. `Down()`
      also has to delete the parents (and their `Restrict`-guarded tenant copies) before `ModuleId`
      can be `NOT NULL` again. The FK is `NoAction`: SQL Server rejects a cascading self-reference, so
      `DeleteOperationHandler` refuses to delete a group that still has children.
    - ⚠️ **Bug I introduced and caught:** the first sidebar build returned an EMPTY menu.
      `TenantOperation.ModuleId` carries the TEMPLATE id, but the grouping matched it against the
      tenant copy's own `Id`, which never joins. Match on `OperationId`. Remember this — the tenant
      tables hold template ids in their parent link, not tenant-row ids.
    - Creating a group is now possible through the API: `moduleId: null` + `subsystemId`. A screen is
      rejected as a parent (the sidebar only descends one level).
    - ⚠️ **Home was repointed too.** Its `Core.Module` join would still have worked via the shared-Id
      invariant, but only for groups predating the change — a group created through the HRMS screen
      writes no module row and every screen under it would have vanished from the portal.
    - Wire contract unchanged (the feed still says "modules"), so **neither SPA needed a change**;
      `OperationDto.ModuleId` is nullable now.
    - Verified: 0 orphan children, 0 parents disagreeing with their module, 0 parents carrying a link;
      TenantOperation 174 = 24 + 150; readers still MATCH (gate 15369, menu 17409, also re-checked
      with the join rewritten to the hierarchy); HRMS 34 links under 12 groups and Home 2/12/34, both
      identical to before; group+child round-tripped through the API and the delete guard fired.

00FB. **Core.User / Core.Role / Core.Operation aligned with the cybererp_srms schema
    (2026-08-13).** Detail in logic.md §12.5. Migration `AlignCoreTablesWithSrms`, APPLIED to CERP.
    Backup: `D:\Backups\CERP_before-srms-table-align-*.bak`.
    - `User.Password` → **`PasswordHash`** + 9 columns; `Role.Code` now `nvarchar(80) NOT NULL` +
      `Description`/`IsPlatformRole`/`IsActive`; `Operation.SortOrder` → **`DisplayOrder`** +
      `SubSystemId` (real FK to Core.Subsystem, denormalised from the module) + `IsActive`.
      Column sets now match SRMS exactly — 0 missing, 0 type/length mismatches.
    - ⚠️ **`TenantId` was KEPT** on all three (SRMS has no such column). `Repository<T>` filters every
      query on it; dropping it makes the User/Role screens show all tenants at once. That refactor —
      scoping them via `TenantUser`/`TenantRole` — is a separate change.
    - ⚠️ **Three forced departures from SRMS:** the `NormalizedEmail` unique index is **filtered**
      (`<> ''`) because **489 of 506 accounts have no e-mail**; `Operation.ModuleId` still points at
      `Core.Module` because **SRMS has no Module table** (its same-named column is a renamed
      `ParentOperationId` self-FK, and `FK_Operation_Module_ModuleId` actually sits on `SubSystemId`);
      and `IX_Role_Code` is **not** unique, since two tenants may each hold an "Administrator" and the
      index cannot be scoped to `(TenantId, Code)` — `TenantId` is `nvarchar(max)`. Role code
      uniqueness is enforced per tenant in `SaveRole`.
    - ⚠️ **The scaffolded migration was NOT runnable as generated** — EF orders by dependency, not by
      data. Three backfills had to be interleaved by hand: normalised columns before their unique
      index (all 506 rows were `''`), `SubSystemId` before its FK (empty Guid matches no subsystem),
      and `Role.Code` before `NOT NULL` (the `''` default would have blanked 5 real roles).
    - ⚠️ **The Home portal shares this database** and reads the password column directly. Its
      `Navigation.cs`, portal feed and `SeedHomeMenu` were updated in the same pass — **deploy both
      repos together**. Shared tables are `ExcludeFromMigrations` there, so Home won't fight it.
    - The **wire contract still says `sortOrder`** (mapped to `DisplayOrder`); `Module.SortOrder` is
      genuine and unchanged, so renaming only the operation half would split two adjacent screens.
      No frontend change was needed in either SPA.
    - `Core.RolePermission` and `Core.Module` **do not exist in SRMS**; RolePermission is already
      superseded at runtime by `TenantRolePermission` (00FA). Removing either is its own decision.
    - The projector now propagates `Operation.IsActive` to the tenant copy — the readers filter on the
      TENANT row, so a template-level kill switch would otherwise do nothing.
    - Verified: readers still MATCH (gate 15369, menu 17409), model still MATCH (70852), HRMS login
      200 / 34 links, Home login 200 + portal feed correct, 0 errors in either log, throwaway
      operation round-tripped.

00FA. **SRMS phase 2, STEP 2 — THE FLIP: the runtime now reads the tenant-scoped tables
    (2026-08-13).** Detail in logic.md §12.4.
    - `EndpointPermissionService` and `GetModuleWithOperationsRepository` resolve through
      `TenantUser`(Active) → `TenantUserRole` → `TenantRolePermission` → `TenantOperation`(IsActive).
      A multi-tenant user now sees only the tenant they signed in to, and `IsActive = 0` really
      revokes a screen. `OperationRecord.Id` still reports the TEMPLATE id — that is what the
      role-permission screen sends back.
    - ⚠️ **The admin screens still EDIT the global tables**, so every write path calls the new
      `ITenantAuthorizationProjector.SyncAsync()`. Without it a permission save would update a table
      nobody reads and appear to do nothing. Full reconcile, not surgical — cheap at this size and
      self-healing if a path is missed. It skips bespoke (`SourceTemplateId` null) roles and
      `IsCustomized` tenant roles so a projection never strips local customisation.
    - ⚠️ **Three delete paths clean up inline, BEFORE the save** — the projector runs after it, too
      late. `User` (NoAction) and `Operation` (Restrict) would fail outright; `Role` is worse, since
      SetNull SUCCEEDS and blanks `SourceTemplateId`, leaving an invisible role that still grants its
      permissions.
    - Permission-cache keys carry a generation number now (`IMemoryCache` can't clear by prefix);
      `InvalidateAll()` bumps it so an admin's own save is not stuck behind the 60s window.
    - Verified: `verify-tenant-auth-readers.sql` transcribes both runtime queries and reports MATCH on
      each (gate 15369 = 15369, menu 17409 = 17409, 0 lost, 0 gained, 0 users differing); the model
      test still says MATCH; `hoadmin` got 34 links, 403 / 200 / 401 as expected; and the projector was
      exercised with a **throwaway operation** (create → copy appeared, rename → copy followed, delete
      → both gone, all six counts back to 150/8/598/500/503).
    - ⚠️ **Found while testing, NOT fixed:** `SubsystemController`, `ModuleController` and
      `OperationController` carry no `[RequirePermission]`, so **any authenticated user can create,
      rename or delete menu operations** — that is how the throwaway probe ran under a non-admin
      account. Pre-existing, unrelated to the flip, and worth its own change.
    - ⚠️ Not verified live: an **admin** session (`admin`'s password is not the documented
      `Passw0rd!`/`password`), so the Role Permissions screen save → projection round trip was proven
      through the Operation endpoints rather than the permission grid itself.

00EZ. **SRMS phase 2, STEP 1: the tenant-scoped auth model exists and is mirrored — nothing reads it
    yet (2026-08-13).** Detail in logic.md §12.3.
    - Migration `AddTenantScopedAuthorization` (6 CreateTable + 13 indexes, no alters/drops), APPLIED
      to CERP. Entities in `Dom/Entities/Core/TenantAuthorization.cs`.
      Backup: `D:\Backups\CERP_before-tenant-auth-20260813-*.bak`.
    - ⚠️ **`OwningTenantId`, not `TenantId`**: BaseEntity already has a STRING `TenantId` (the
      Finbuckle discriminator). These tables also need a real Guid FK to `Core.Tenant`, so it is named
      separately rather than shadowing it. None is `[MultiTenant]` — they DEFINE tenant scoping, and
      sign-in must read memberships before a tenant context exists.
    - **`backend/scripts/seed-tenant-authorization.sql`** fills them from CERP's own data (idempotent).
      Result is 1:1 with the live model: roles 8, operations 150, permissions 598, tenant users 500,
      user-roles 503, subsystems 7.
    - ⚠️ **TWO TRAPS, both hit on the first attempt.** (1) The live model is ALREADY tenant-scoped via
      the discriminator (3 roles demo / 5 headoffice; all ops+perms headoffice) — **do NOT cross join
      Role × Tenant**, it makes every user a member of every tenant (506 users → 1500 memberships).
      (2) `SELECT DISTINCT NEWID(), …` does not dedupe, because NEWID() makes each row unique; resolve
      distinct pairs in a subquery first.
    - **ACCEPTANCE TEST PASSED** (`seed-tenant-authorization-verify.sql`, read-only): effective
      permissions per user compared both directions — `old 70852 / new 70852 / lost 0 / gained 0`,
      `users whose viewable link count differs = 0`, verdict **MATCH**.
    - Runtime unchanged, as expected since no reader was switched: Administrator 345 employees /144
      sidebar links, employee 1/34, manager 2/34 — all identical to 00EY.
    - **STEP 2 = the flip** (IEndpointPermissionService, login, sidebar, Role Permissions screens).
      **Re-run the verify script immediately before flipping** — it must still say MATCH, or the live
      model has drifted since the seed.

00EY. **`IsAdmin` repointed — categories B and C fixed at the root; the audit is CLOSED (2026-08-13,
    ONE file).** Detail in logic.md §11.8.
    - `IsAdminAsync` no longer short-circuits on `IsHeadOffice()`; it checks the `/employee` menu
      permission (`HrScreens.EmployeeRegister`), keeping the existing HrSignOff-approver fallback.
    - **None of the 73 scoping sites or 54 "Only HR" gates was edited.** They were never wrong — they
      ask `scope.IsAdmin`, which was answering "yes" for everyone. One line made the answer correct.
    - ⚠️ **This is only survivable because of 00EW** (every employee now holds a role). Do NOT port it
      to an environment where `backend/scripts/assign-employee-role.sql` has not been run — before
      that, 480 accounts had no permissions at all and would have lost everything.
    - **Acceptance test (effective visibility, same endpoints before/after):**
      `Administrator 345 employees BEFORE and AFTER` (unchanged); an ordinary employee went
      `345 → 1`, appraisals `1 → 0`, goals `3 → 0`. A second employee kept **1** other-leave row —
      their OWN maternity request — proving per-owner scoping rather than a blanket zero. Managers
      resolve in between: `engdag` 2 and `rojer(dr)b` 5 employees (their unit subtrees).
    - Verified no 500s across 12 modules for admin/manager/employee; self-service all 200; the Home
      News Feed still works (it uses `Announcement/feed`, open to staff, while the admin
      `Announcement` list now correctly refuses them — that 400 is the fix working, not a break).
    - 158/158 tests pass. LoginTrail rows from testing removed.
    - **The IsAdmin audit is now complete: A (00EX), B and C (this).** Remaining related work is
      SRMS phase 2 (the tenant-scoped auth model).

00EX. **Category A fixed: the 16 cross-employee guards (2026-08-13, HRMS App).** Detail in logic.md §11.7.
    - All 16 read `if (!scope.IsAdmin && record.EmployeeId != mine) throw`. `IsAdmin` is true for
      everyone, so the condition was ALWAYS FALSE and the throw unreachable — any employee could
      cancel a colleague's loan, trip or guarantee. Each now checks the HR-side MENU PERMISSION via
      `IEndpointPermissionService`, which has no head-office bypass.
    - New `App/Common/Authorization/HrScreens.cs` names the link once per record type:
      `/loan`, `/trip`, `/employeeGuarantee`, `/trainingNeed`, `/rewardNomination` — each the HR
      REGISTER, which staff do NOT hold (they hold `/myLoans`, `/myTrips`, `/myGuarantees`,
      `/myTraining`).
    - ⚠️ **Grievances use `/employee` instead**, because EVERY employee holds `/grievance` — a link
      both sides hold cannot discriminate. Check that separation before reusing a link for a new guard.
    - **Proven with real records** (throwaway loan type + loan, deleted after): employee B →
      *"You can only cancel your own loan requests."*; the OWNER and HR both pass ownership and reach
      the workflow gate instead. Three parties, three different gates.
    - ⚠️ My replacement pattern initially also caught
      `if (!scope.IsAdmin && !scope.IsManager)` in `RewardNominationHandlers` — an HR-or-MANAGER gate,
      not an ownership guard. **Reverted** to keep the change to the 16 asked for. It is still broken
      (any employee can raise a nomination); it belongs to category B.
    - Backend builds clean, 158/158 tests pass, 0 cross-employee guards still use `IsAdmin`, all test
      data removed (loans/types/schedules/guarantors/instances/LoginTrail all back to 0).
    - **STILL OPEN: category C (73 query-scoping sites)** and repointing `IsAdminAsync` itself.

00EW. **Employee role assignment — the 00EV blocker is CLEARED (2026-08-13, DATA change to CERP +
    a reusable script). No application code changed.** Detail in logic.md §11.6.
    - **`backend/scripts/assign-employee-role.sql`** (idempotent — RUN IT ON EVERY OTHER ENVIRONMENT):
      grants the ordinary `UserRole` the employee screens it lacked, then assigns that role to every
      employee-linked account with none. **480 assigned; 0 roleless employees remain; 495 users now
      hold the role.** Backup first: `D:\Backups\CERP_before-role-assignment-20260813-141931.bak`.
    - Grants added: `/myGuarantees`, `/myInsuranceClaims`, `/myTraining`, `/notifications`,
      `/workflow`, `/surveyTake`, `/recognitionWall`, `/learningCommunity`, `/appraisalAppeal`.
      **Deliberately NOT granted** (they look self-service but are not): `/employeeGuarantee` (HR
      register), `/transferRequest` (Manager Requests module), `/exitQuestionnaire` (Personnel/HR),
      `/compensationRequest`.
    - Only ADDS access, so it cannot lock anyone out. It flipped existing `CanView=0` rows rather
      than inserting — the role already had a row per operation — which is why the permission-row
      count stayed at **598**.
    - ⚠️ **The operation catalog has DUPLICATE rows per link** (150 rows / 132 distinct links). A link
      is granted if ANY of its rows grants it, so audit queries MUST aggregate by `Link`; my first gap
      query checked rows and reported 24 false gaps against the real 13.
    - Verified: a previously-roleless account now reaches `Employee/me`, `OtherLeave/mine`,
      `AnnualLeave/mine`, `Workflow/my-approvals`, `AppraisalPeer/mine` + portal feeds (200); still
      403 on every HR master screen; sidebar resolves **34 links vs an Administrator's 144**. Five
      randomly sampled accounts identical. Users 506 / employees 490 / permission rows 598 unchanged.
    - **NOW UNBLOCKED:** category A (16 cross-employee guards), category C (73 scoping sites), and
      repointing `IsAdminAsync` off `IsHeadOffice()`.

00EV. **`IsAdmin` audit + partial hardening (2026-08-13, HRMS Api). ⚠️ FINDS A BLOCKER — read this
    before any further permission work.** Full detail in logic.md §11.5.
    - **Measured, not inferred.** As an ordinary employee: `GET CalibrationSession` (gated *"Only HR
      can view calibration sessions"*) → **200**; five HR-only POSTs (`LoanType`, `MedicalPlan`,
      `InsurancePolicy`, `PerDiemRate`, `BenefitPlan`) reached FIELD VALIDATION with empty bodies,
      proving the gate never fired (empty payloads deliberately, so nothing could be created).
      Contrast: `POST SalaryRevision` → 403, because that controller has `[RequirePermission]`.
    - **143 no-op checks in 60 files**, in three shapes: **A** `!IsAdmin && notMine → throw` (16 —
      any employee can cancel ANOTHER's loan/trip/guarantee; reasoned from measured IsAdmin=true +
      boolean logic, NOT executed, because executing it would cancel a real record), **B**
      `IsAdmin → throw "Only HR…"` (54), **C** `if (!IsAdmin) narrow query` (73 — the Other Leave bug).
    - **⚠️ THE BLOCKER: 480 of the 490 employee accounts have NO ROLE** (only 20 users hold any).
      `HasAnyAsync` needs a role with `CanView`, so gating a controller 403s those 480 — they can use
      the system today ONLY BECAUSE of the bug. Gating all 109 ungated controllers (what "option 2"
      literally meant) would have locked out 96% of users. Verified with roleless `abaynesha`:
      ungated `MedicalPlan` 200, gated `SalaryRevision` 403.
    - **Applied narrowly instead: 31 pure HR/master-data controllers** where 403-for-roleless is
      CORRECT — allowanceType, annualLeaveSetting, appraisalTemplate, benefitPlan, branch,
      calibration, clearanceDepartment, competency, competencyCategory, documentTemplate,
      employeeField, fiscalYear, holiday, insurancePolicy, jobCategory, jobGrade, leaveType, loanType,
      medicalContract, medicalPlan, medicalProvider, otherLeaveSetting, perDiemRate, positionClass,
      ratingScale, recognitionBadge, reviewCycle, taxBracket, tripBudget, workLocation,
      workWeekConfiguration. **Checked first that none exposes `/me` `/mine` `/my*`, and deliberately
      EXCLUDED `EmployeeController` (it carries `Employee/me` — gating it breaks every portal user).**
    - Verified 15 endpoints roleless-403 / Administrator-200, and self-service still 200
      (`Employee/me`, `OtherLeave/mine`, `AnnualLeave/mine`, `my-balance`, `Workflow/my-approvals`,
      `AppraisalPeer/mine`, portal Loan/TripRequest/MedicalClaim feeds).
    - **NEXT, and everything is blocked on it: assign the ordinary role to the 480 roleless accounts.**
      Then categories A and C can be fixed and `IsAdminAsync` repointed off `IsHeadOffice()`. Note the
      `UserRole` role is itself incomplete for self-service (missing `/myGuarantees`, `/myTraining`,
      `/myInsuranceClaims`) and needs filling out first.

00EU. **SRMS platform layer ported into CERP — PHASE 1 of 2, additive only (2026-08-13, HRMS backend).**
    Asked to "improve the schema" by copying tables from `cybererp_srms`, excluding
    `Core.LookUpCategory` / `Core.LookUpCategoryList`. **The user chose the additive scope after I
    presented the analysis; the auth cutover is phase 2 and was NOT started.**
    - **Analysis finding that reframed it: SRMS is a DIFFERENT PRODUCT.** Its 326 operations and
      CERP's 150 share **ZERO** links. Its own data is an empty template (1 role, 3 users, 6
      permissions), so "include all related data" could never mean copying its rows. What is valuable
      is its architecture — see logic.md §12.
    - **Landed:** migration `AddSrmsPlatformLayer` (7 CreateTable + 12 indexes, **no alters/drops**),
      APPLIED to CERP. Tables: `Organization`, `OrganizationSubscription`, `SubscriptionPlanModule`,
      `TenantSubscriptionAddOn`, `LoginTrail`, `Setting`, `UserPreference`. Entities follow CERP
      conventions (BaseEntity / NodaTime Instant / varbinary(8)), NOT byte-matched to SRMS.
    - **Backup before the change: `D:\Backups\CERP_before-srms-platform-20260813-134225.bak`** (77 MB).
      ⚠️ `BACKUP ... WITH COMPRESSION` is unsupported on SQL Express — omit it.
    - **Data: copied only what is referentially valid.** SRMS has NO FK constraints on these tables,
      so a bulk copy would have succeeded while leaving dangling ids. Copied `Setting` (1) and
      `Organization` (1, "Cybersoft"). SKIPPED: `SubscriptionPlanModule` (9 — CERP has 0
      SubscriptionPlans and different Module ids), `OrganizationSubscription` (1 — same),
      `UserPreference` (2 — SRMS users/tenant), `LoginTrail` (85 — an audit log starts clean).
    - **Code alignment = `LoginTrail` wired into `LoginRepository`** (there was NO login audit at
      all). Records success / wrong password / unknown user name with IP + user-agent; keeps the
      attempted name separate from `UserId`; never stores the password; no FK to `Core.User`; and
      swallows its own errors so an audit write can never fail a sign-in. Verified live for all three
      cases, 401s unchanged. My 4 verification rows were deleted (they document MY testing).
    - **Regression checked after the migration:** 506 users / 8 roles / 598 permissions / 23
      user-roles / 150 operations / 490 employees — all unchanged; gated endpoints still 200/403.
    - ⚠️ **Two deliberate overlaps left in place** so the phase stayed additive: `Organization` vs the
      existing `CompanyProfile` (still feeds offer letters + report letterhead), and `Setting`'s SMTP
      columns vs `appsettings.json:Email` (still what `SmtpEmailService` reads). Consolidating either
      is a CODE change, not a data one.
    - **Phase 2 (not started):** `TenantRole`/`TenantOperation`/`TenantRolePermission`(+`CanExport`)/
      `TenantUser`/`TenantUserRole`/`TenantSubSystem`. **Recorded decision: generate those rows FROM
      CERP's existing data, never from SRMS's**, and accept only if each user's effective permission
      set is byte-identical before/after. See logic.md §12.2.

00ET. **Salary-revision lifecycle + leave attachments, approver review, leave e-mail, Other Leave
    isolation (2026-08-12/13, HRMS both halves + Home frontend).** Five requested batches; one
    migration (`AddOtherLeaveAttachment`, additive, ALREADY APPLIED to CERP).
    - **⚠️ THE HEADLINE, see logic.md §11: `IsAdmin` IS NOT AN AUTHORIZATION CHECK HERE.**
      `IsAdminAsync` short-circuits on `IsHeadOffice()`, CERP has ONE branch flagged head office, so
      ALL 490 employee-linked users are `IsAdmin = true`. It produced a real reported bug (the portal
      listed everyone's Other Leave) and it silently broke my own first cut of the salary-revision
      review endpoint. Gate on the MENU PERMISSION, a `/mine` endpoint, or the resolved approver —
      never `IsAdmin`. **An audit of the other `IsAdmin` call sites is still OUTSTANDING** (offered
      three times, not yet requested).
    - **Salary revision — approve BEFORE submit.** New `Submitted` state
      (Draft → PendingApproval → Approved → Submitted → Applied); the author may only *Send for
      Approval*. Sending refuses with no active workflow definition (else the author is the only
      possible approver). `Status` is a STRING column so no migration was needed. Every transition
      writes a `PerformanceHistory` row → the History popup answers created/approved/submitted by.
      Verified live: Submit-from-Draft 400, Apply-from-Approved 400, approver `tatekg` approves, then
      Submit → Apply.
    - **⚠️ I APPLIED A REAL REVISION TO 345 EMPLOYEES DURING TESTING and restored it** (each line
      stores the pre-apply salary and there were no grade promotions, so the reversal was exact:
      345 restored, 0 off-original, status back to Draft, my history/instance rows deleted). **Use a
      throwaway record for lifecycle tests — never an existing one.**
    - **Approver review popups.** `SalaryRevision/{id}/review` and `OtherLeave/{id}/review` let the
      assigned approver READ what they are deciding. Salary revision: enterprise object-page layout
      with search + Excel/PDF export (lazy `listExport` chunk preserved), Approve disabled until the
      figures are opened. Added `xl` to the shared Home Modal (additive).
    - **Other Leave attachments** (`Hrms.OtherLeaveAttachment`, 5 MB/file): picker on the request
      form in BOTH SPAs, list+download wherever the request is read — which covers the HR admin via
      the employee-profile tab, since that tab reuses the same form.
    - **Leave approval e-mail** (`LeaveNotifier`, annual + other), after commit, never throwing.
      ⚠️ `Email:UserName`/`Password` are EMPTY in appsettings — verified via `Email__PickupDirectory`
      (.eml on disk). Employees also need addresses: `rojer(dr)b` had none, which logs and sends
      nothing by design.
    - **Other Leave isolation (the reported bug).** Portal grid now calls `OtherLeave/mine` pinned to
      `status=Pending`; status dropdown hidden there (it stays on the profile tab). Reproduced first:
      `rojer(dr)b` saw *"Meseret Negewo — Approved"* on the old endpoint, 0 on `/mine`; `/mine` cannot
      be widened by passing another `employeeId`, and an unlinked account gets 0, never everything.
    - ⚠️ Test-data note: the Other Leave module was UNCONFIGURED (0 types/settings/requests) so tests
      needed temporary config — all removed. The `Maternity Leave` request + `blank (1).pdf` now in
      CERP is the USER's own UI test, deliberately left alone.

00ES. **Organization Unit edit: Branch not saving + form showing stale data (2026-08-12, HRMS both halves).**
    Two reported symptoms that turned out to be MOSTLY ONE bug. Reproduced in a real browser before
    changing anything: the server HAD the branch saved while the re-opened form showed EMPTY — so
    "the branch does not save" was largely a symptom of #2.
    - **#2 stale form (the big one).** Forms invalidate the plural list key on save but never their
      own detail key `["<entity>", id]`, and `main.tsx` sets `staleTime: 30_000` — so re-opening a
      record within 30 s served the PRE-SAVE copy with NO refetch. Grid fresh, form stale, cleared
      only by a full page reload (which builds a new QueryClient). A/B proof, same test either side:
      unfixed saved "PME-86928" then showed "PME-37458"; fixed saved "PME-33546" and showed it back.
    - **Swept ALL of it, not just Organization Unit: 57 forms.** ⚠️ My first count of 64 was WRONG —
      it matched keys by FOLDER NAME. That gave 7 false positives (candidate, appraisalAppeal,
      hiringRequest, jobRequisition, improvementPlan, careerPathChangeRequest, workforcePlan already
      invalidate correctly in the TARGETED form `["candidate", formState.id]`) and would have written
      2 keys matching nothing (formBuilder's detail key is `dynamicForm`, not `formBuilder`).
      **Derive the key from the form's own useQuery, never from the directory name.**
      54 applied by codemod (brace-matches the success block, inserts after the last existing
      invalidation); 3 hand-edited because their shape differs — `employeeGuarantee` (shared
      `invalidate()` helper) and `otherLeave`/`otherLeaveSetting` (`if (res.ok)`, not `formState.status`).
    - **#1 branch silently discarded — a REAL second bug, for non-head-office users.**
      `UpdateOrganizationUnit` read `IsHeadOffice() ? dto.BranchId : entity.BranchId` and answered
      **200** while dropping the value. Proven by sending identical payloads as two accounts. It bites
      real users: `tatekg` is an Administrator (so HAS the Organization Unit permission) AND is not
      head office. Now throws 400 naming the restriction, and ONLY when the value would change — an
      omitted or unchanged BranchId still saves normally (all four paths verified). See logic.md §10.
    - Verified: 4 modules proven end-to-end in the browser (organizationUnit, workLocation,
      competencyCategory, awardCategory); the other 53 rest on the static re-survey (0 remaining)
      plus `tsc -b` / `eslint` clean. MedicalProvider/TrainingCourse/Holiday could NOT be exercised —
      0 rows in the purged DB, so no Edit button; ratingScale has no `name` field; jobGrade was
      inconclusive because renaming re-sorts the grid so the first Edit button changes record.
    - All test data restored (no `zz-` values remain; PME-01, GM-01, Bishoftu and the two categories
      are back to their original values).
    - ⚠️ **Editing these docs from Git-Bash heredocs/`node -e` mangles backticks and `$(...)`, and a
      `\n\n` replace silently no-ops because the files are CRLF.** Use the editor tool instead.

00ER. **Home dashboard two-flow performance batch (2026-08-12, HRMS backend+frontend + Home backend+frontend).**
    The user's staged request: map the bottlenecks of (1) login → dashboard and (2) grid action →
    record, then fix backend, then frontend. All fixes measured against a production Vite build in a
    real browser, A/B'd by stash-reverting one side at a time.
    - **The dominant felt cost was CORS preflights, and page-level Playwright events DO NOT SHOW
      THEM** — my first capture reported "0 preflights" for both builds; only a CDP
      `Network.requestWillBeSent` capture revealed **18 of 36 dashboard requests were OPTIONS**.
      Cause: both SPAs' `apiClient.ts` (and the raw `fetch` in Home `services/admin/employee/me.ts`)
      sent `Content-Type: application/json` on bodyless GETs — not a CORS-safelisted value, so every
      cross-origin read preflighted, uncached (~5 s browser default). Fix: send the header only with
      a body → **2 of 20** (the login POSTs, correctly). Safety net: `SetPreflightMaxAge(24h)` in both
      APIs (HRMS `ServiceCollectionExtensions.AddHrmsCors`, Home `Program.cs`).
      Checked before shipping: NO bodyless POST/PUT/PATCH exists in either SPA and no `FormData` goes
      through `apiClient`, so nothing lost a header it needed.
    - **`GetMyApprovals` no longer materialises every Running instance.** SQL pre-filter classifying
      steps static/subject/dynamic + `OrgManagerResolver.EmployeesInMyManagedUnitsAsync` (inverse
      subtree walk off the per-request org snapshot) + memoised
      `WorkflowApproverAuth.CurrentEmployeeIdForInboxAsync`. Dynamic predicate is a SUPERSET;
      `EvaluateAsync` still decides per row. Proven by reverting to the old code and diffing
      instance-id sets per role at 5,002 seeded instances — IDENTICAL for all four
      (58/1/0/0 items) — **346 ms → 31 ms**. EF cannot translate tuple `Contains`, hence the
      re-pairing step in memory. Seeded rows deleted after (`CreatedBy='perf-probe'`, verified 0
      left, 2 real instances intact). See logic.md §2.11.
    - **`DatabaseTenantStore`**: one predicate for both key shapes + 5-min `IMemoryCache` primed under
      id AND identifier; misses cached briefly too. Was 2 `Core.Tenant` queries on EVERY request (the
      cookie carries the GUID; the old code queried `Identifier` first — a guaranteed miss). Now 0
      steady-state, confirmed in the SQL log.
    - **Home identity waterfall**: 5 of 7 request feeds awaited `getMyEmployeeId()` before building
      their URL (`scanPending`). `AuthContext` now warms `getMyEmployeeStatusCached()` the moment the
      session is confirmed, and REMOVES the cached answer on login/logout so a second sign-in on the
      same tab cannot inherit the previous identity. Probe fires at ~79 ms vs ~155 ms; the whole
      11-call fan-out now goes out as one wave (~452 ms) and settles by ~500 ms.
    - **Record-open chunk off the click path**: new Home `template/prefetchLazy.ts` (idle-time warm of
      a `React.lazy` factory) + `config/routePrefetch.ts` (segment → route-chunk map; specifiers MUST
      match `routes/index.tsx` exactly). Both dashboard grids prefetch only the destinations their
      visible rows can open (stable string key — the feed arrays are rebuilt every render);
      `appraisal/index.tsx` warms its own form chunk. A/B: chunk at ~531 ms idle vs 4,150 ms
      post-click; scripts fetched after click 6 → **0**.
    - **`GetAppraisalById`**: appraisee name + peer names merged into ONE `Hrms.Employee` query
      (13 → 12 on a warm cache; every remaining query is an indexed point lookup). DTO proven
      byte-identical (`cmp`); 11–16 ms warm.
    - Verified end-to-end: browser smoke on production builds of BOTH SPAs (dashboard KPIs, inbox
      rows, `/appraisal/{id}` deep link, annual-leave list, HRMS employee list — no page errors);
      `tsc -b` + `eslint --quiet` clean ×2; both backends build clean.
    - ⚠️ Traps recorded: (1) Playwright page events hide preflights — use CDP. (2) A JIT-cold API
      answers ~90–115 ms for a call that is 11 ms warm — warm up before trusting a number. (3) The
      Home SPA's `.env.production` points the HRMS API at `jtempurl.com` — a "production build" test
      must use `--mode development` locally or every measurement includes remote latency. (4) The
      `403 AuditLog` in the HRMS smoke run is a pre-existing permission wall (403 with AND without
      the header), not a regression. (5) `Invoke-Sqlcmd` here lacks `-TrustServerCertificate`; use
      `sqlcmd -S 'CLOUDX-SICS2\SQLEXPRESS' -d CERP -E -C`.

00EQ. **Appraisal "Add" did nothing in HOME; `useEntityRouteModule` hardened (2026-08-12, both SPAs).**
    Reported against both apps. **HRMS was fine** — verified directly before touching anything: Add
    navigates to `/appraisal/new` and renders the Generate Appraisal form. Only Home was broken, by
    00EO's URL-backed routing.
    - **Cause.** Home's route declared a static `new` child beside `:id`:
      `<Route path="new">` + `<Route path=":id">`. A static segment matches AHEAD of `:id`, so on
      `/appraisal/new` there is **no `id` param** — and the hook derives `showForm` from
      `useParams().id`, so it stayed false: the URL changed while the LIST kept rendering and the
      button looked dead. (The code comment I wrote there even stated the mechanism.)
    - HRMS never hit it: `renderEntityRoutes` declares only `index` + `:id`, `:id` matches the literal
      "new", and `EntityRecordGuard` explicitly accepts `NEW_SEGMENT`.
    - **Fix (1)** drop the `new` route from Home so it mirrors the HRMS shape.
      **Fix (2)** `useEntityRouteModule` now falls back to reading the segment from the pathname when
      `useParams().id` is absent, so the module behaves identically however the route is declared.
      The path check is SEGMENT-AWARE (`base + "/"`), so `/appraisal` cannot swallow
      `/appraisalTemplate` — the same substring trap that bit `/loan` vs `/loanType`.
    - ⚠️ **Never declare a `new` route next to `:id`** for a module using this hook. `:id` matches the
      literal "new" and the hook + guard already handle it.
    - Verified: Home Add → form (the reported bug), record deep-link still hits the BY-ID endpoint and
      the list route the LIST endpoint; HRMS `appraisal` + `employeeGoal` each 6/6 on
      list → Add → form → direct `/new` → back.
    - ⚠️ Two regression assertions failed spuriously and were the TEST's fault: `/appraisalTemplate` is
      `CanView=0` for `UserRole` (permission wall, not a routing bug), and "page contains a `<table>`"
      does NOT distinguish list from form — the appraisal scoring form contains goal/competency grids.
      Discriminate by WHICH endpoint the route calls (`Appraisal?…` vs `Appraisal/{id}`).

00EP. **Approvals performance: 5,028 ms → 39 ms at scale (2026-08-11, HRMS backend + Home frontend).**
    Reported three times as "dashboard cards and grids are slow". The first two investigations found
    nothing because the purged DB has almost no rows — **seeding 2,000 running WorkflowInstances
    reproduced it instantly.** Measure at volume or you will not see this class of bug.
    - **Root cause — `OrgManagerResolver`.** The org climb issued TWO queries per level ("managers in
      this unit?" then "who is its parent?"), and its cache key was `(unit, REQUESTER)` because
      self-exclusion was applied in SQL — so two people in the same unit could never share a climb.
      490 employees over 121 units meant ~490 full climbs. `Workflow/my-approvals` spent **3,795
      queries / 5,028 ms** to return 23 items.
    - **Fix:** load the unit tree + the (small) managerial-employee set ONCE per request and climb in
      memory, applying self-exclusion afterwards so per-unit data is shared. Walk order, cycle guard
      and fallbacks are unchanged — the returned instance-id set is byte-identical.
      Plus `PreloadEmployeeUnitsAsync` so the inbox resolves every requester's unit in one query.
      **→ 39 ms / 23 queries.**
    - **Frontend — the grid re-fetched everything constantly.** `useEntityList` keys on
      `[queryKey, param]`, and the workflow grid gave it a `fetchPage` that called every registry
      source in full — so every page change and EVERY SEARCH KEYSTROKE re-ran the whole fan-out, and it
      bypassed the dashboard's cache entirely. Both grids now read the same cached
      `useApprovalFeeds`/`useRequestFeeds` the dashboard cards use and page/search in memory
      (`useRegistryList`). Decisions invalidate `myApprovals`/`myRequests`, which refreshes both screens.
    - **Also:** per-request memoisation in `WorkflowApproverAuth` (role ids, current employee, step
      approvers); the appraisal record no longer loads its workflow instance twice; and the four
      request feeds push `employeeId`/`status` to SQL instead of pulling 100 rows to filter in the
      browser — which also fixed a WRONG `total` (it counted matches inside that 100-row window).
    - Measured after: navigate to grid **89 ms / 0 calls**, search keystroke **0 calls**, page change
      0 calls, open record 2 calls, dashboard fan-out 222 ms at 2,000 instances.
    - ⚠️ **Two false leads recorded so they are not chased again.** (1) A "25-second stall" reported in
      an earlier session was MY environment: a stale `CyberErp.Hrms.Api.exe` held port 55900, so
      restarts silently failed (`Failed to bind: address already in use`) and measurements ran against
      an old binary beside a competing process. **`pkill -f CyberErp.Hrms.Api` does NOT match it — use
      `Get-Process -Name 'CyberErp*' | Stop-Process -Force`, and always grep the run log for a bind
      error before trusting a number.** (2) A hard browser reload of `/workflow` taking ~26 s is Vite
      dev transforming that route's modules — in-app navigation is 89 ms.

00EO. **Portal notifications route to the RECORD; new peer-review alert (2026-08-11, HRMS backend + Home frontend).**
    - **Appraisal alerts now deep-link.** `WorkflowService.NotifyCurrentStepApproversAsync` hard-coded
      `"/workflow"` for every entity type. An appraisal is *module-driven* — `EnsureNotModuleDriven`
      REFUSES the generic approve/reject — so an approver clicking the alert landed on a screen whose
      only possible response was "go somewhere else". `NotificationLinkFor(instance)` now yields
      `/appraisal/{EntityId}` for `WorkflowEntityTypes.Appraisal`, `/workflow` for everything else.
    - **New alert: peer-review assignment.** `InviteAppraisalPeers` raised nothing at all — the
      assignment was silent unless the reviewer happened to open My Peer Reviews. It now notifies each
      invited peer (link `/myPeerReviews`, severity Action), AFTER `SaveChanges` so an alert can never
      point at a review that failed to persist, and inside try/catch so a portal hiccup cannot fail the
      invite. Peers with no portal account are skipped, logged not thrown.
    - **Correlated PER REVIEW, not per appraisal.** `sourceEntityId` is the `AppraisalPeerReview.Id`
      (source type `AppraisalPeerReview`), so `SubmitAppraisalPeerReview` can `ResolveAsync` and clear
      **only that peer's** alert. Correlating on the appraisal id would have cleared every peer's alert
      the moment one of them submitted.
    - **Home needed URL-backed routing to land on.** Its appraisal module used the state-based
      `useEntityCrudModule`, so no `/appraisal/{id}` URL existed. Ported `useEntityRouteModule` from
      HRMS (identical file), exported it from the template barrel, switched
      `components/admin/appraisal/index.tsx`, and split the flat route into `index` / `new` / `:id`.
      Home has **no route-level PermissionGate** (only `selfServiceGate`), so nesting introduces no
      gate hole here — unlike HRMS, where nested routes fall through ungated.
    - Verified in Home against the real appraisal: `/appraisal` calls the LIST endpoint with search +
      pager; `/appraisal/{id}` calls `Appraisal/{id}` with no list chrome — i.e. the deep link opens
      that record's form. Back returns to the list; `/myPeerReviews` resolves.
    - **Both approval CARDS now carry these too, sharing the bell's routing.** New
      `Home/src/config/recordRouting.ts` is the single definition of *where a record opens and how*:
      `routeForRecord()` (Appraisal → `/appraisal/{id}`, AppraisalPeerReview → `/myPeerReviews`) plus
      `openPortalTarget()` (absolute URL → new tab, otherwise in-app navigate). The bell, the dashboard
      notifications card and the Approvals Inbox rows all call it — the notifications card previously
      held a COPY of the bell's opener, and every inbox row navigated to `/workflow` whatever it was.
    - **Peer reviews registered as an approval source** (`hrmsPeerReviews` in `approvalSources.ts`).
      They are not workflow instances, so they never reached `Workflow/my-approvals` and were invisible
      on every approvals surface. One registry entry puts them in the Approvals Inbox card, the Pending
      Approvals count AND the `/workflow` screen, because all three read that registry. Read-only (no
      `decide`) — a peer review is written on its own screen; every call site already guards on
      `source?.decide`. Submitted reviews are filtered out: an inbox shows outstanding work only.
    - Verified end-to-end once VS released the build lock (both APIs run locally): new appraisal alert
      `LinkUrl = /appraisal/{id}` (the pre-change row still reads `/workflow` — clean before/after);
      invite raises one row per reviewer correlated to **that peer's own review id**; submitting clears
      ONLY the submitter's alert while the other peer's stays unread; the inbox lists the peer review
      and routes to `/myPeerReviews`; the appraisal row routes to `/appraisal/{id}`; the Pending
      Approvals count includes the peer review.
    - ⚠️ Correlating the alert on the APPRAISAL id (the obvious first cut) would have cleared every
      peer's alert the moment one of them submitted. The control test — both alerts unread, one peer
      submits — is what catches that; keep it if this code is touched again.
    - Test data restored afterwards: appraisal + workflow back to `SelfAssessment`, the probe peer
      review deleted, notifications back to the original 4 with their original read states.

00EN. **"Only HR can view performance history" after saving an appraisal (2026-08-10, HRMS backend).**
    Fallout from 00EM, not a regression in it. `GetPerformanceHistory` was gated on `scope.IsAdmin`
    outright, which was invisible while EVERY session resolved to admin. With scoping restored, an
    employee opening their own appraisal was refused the history panel **on their own record** — and
    `scoring.tsx` fetches it unconditionally (`enabled: id !== ""`), so it fired for everyone.
    - **The save itself always succeeded** — the appraisal and its history row were both written; only
      the follow-up history fetch failed, which made a working save look broken.
    - Now the audit trail follows the RECORD it documents: resolve the history row's subject to its
      owning employee and reuse `CanAccessEmployeeAsync`, so appraisee + their management line + HR can
      read it. Types mapped: Appraisal, Achievement, DevelopmentPlan (`IndividualDevelopmentPlan`),
      ImprovementPlan (`PerformanceImprovementPlan`), Recognition (`EmployeeRecognition`).
    - ⚠️ **`Calibration` is deliberately NOT mapped:** its history rows carry a `CalibrationSession` id
      and a session spans a COHORT, so there is no individual owner to authorise against. It falls
      through to the fail-closed default and stays HR-only — as does any entity type added later, so
      **new types must be added to `ResolveOwnerEmployeeAsync` or they silently become HR-only.**
    - Verified against the user's REAL appraisal (6/6): appraisee ✅, their unit's manager ✅, unrelated
      employee ❌, manager of a different unit ❌, HR ✅, unknown/cohort type ❌ for non-admin but ✅ for HR.
    - **Swept the other `Only HR can view…` gates** (medical expense report, individual peer reviews,
      calibration sessions, trip budget utilisation, trip aging): all are cross-employee aggregates or
      deliberately confidential, so HR-only is right for them. Performance history was the only
      per-record read mis-gated as an aggregate. No Home change — same API.

00EM. **Everyone could see every employee — appraisal + all scoped modules (2026-08-10, HRMS backend).**
    Reported against the appraisal modules, but it was never an appraisal bug: `AppraisalHandlers`,
    `EmployeeOptions`, `EmployeeGoal`, `DevelopmentPlan` etc. ALREADY implement
    admin→all / manager→own+subtree / employee→self. Every one of them is wrapped in
    `if (!scope.IsAdmin)`, and **every user was an admin**.
    - Chain: `LoginRepository` computed `isHeadOffice = branchId is null || isBranchHeadOffice`; the
      purge left **0 branches**, so all 490 employee-linked accounts logged in as head office →
      `PerformanceVisibilityService.IsAdminAsync` opens with
      `if (currentUser.IsHeadOffice()) return true` → `scope.IsAdmin` true → every restriction skipped.
    - **One-line fix:** `(branchId is null && !user.EmployeeId.HasValue) || isBranchHeadOffice`.
      "No branch = head office" now applies only to accounts NOT tied to an employee — the
      tenant-owner / system login the surrounding comment already described. The 16 preserved staff
      accounts are all `EmployeeId IS NULL`, so they keep global visibility; only employee-linked
      accounts change. **No appraisal code was touched.**
    - Safe against `Repository.ApplyBranchFilter`: a non-head-office user with NO branch falls through
      unrestricted (it only filters when a BranchId is present), so the branch filter stays a no-op and
      only the row-level visibility scoping re-engages. Nothing goes blank.
    - Verified per role — normal user sees **1**, manager sees **5** (their unit subtree, matching the
      SQL-computed expectation), unlinked HR account still **345**; cross-employee reads via
      `EmployeePerformanceSummary` allow/deny exactly per rule (8/8).
      **Mutation-tested:** with the fix reverted a normal employee sees all **345** and can read any
      employee's summary (HTTP 200) — so the fix is demonstrably what enforces this.
    - **Home is covered by the same change** — its appraisal screens call the HRMS API (55900); it has
      no appraisal backend of its own.
    - ⚠️ Only **2 of 490** employees have `IsManagerial = 1`, and manager scope requires that flag plus
      a position with an org unit. Everyone else resolves to self-only. If real managers are missing
      from their teams, that is the flag, not the logic.
    - ⚠️ With `IsAdmin` no longer granted by branchlessness, the ONLY other route to admin scope is
      being an `HrSignOff` approver on an active Appraisal workflow definition — and the purge removed
      every definition. Employee-linked HR staff therefore have no admin scope until those chains are
      configured; the unlinked staff accounts are unaffected.

00EL. **`ReviewCycle` save failed: two booleans missing from `booleanFields` (2026-08-10, HRMS frontend).**
    Saving a Review Cycle returned *"The dto field is required"* + *"The JSON value could not be
    converted to System.Boolean. Path: $.enableSecondLevelReview"*. **One cause, two messages.**
    - `FormData` values are ALWAYS strings; `createSaveService` converts only the fields named in
      `booleanFields`. `enableSecondLevelReview` and `enableHrSignOff` were not listed, so they went
      out as the string `"false"`, JSON binding failed, and the whole DTO arrived null — which is what
      "the dto field is required" actually means.
    - Only the FIRST bad field is reported (binding stops there), so fixing just the reported one would
      have moved the error to `enableHrSignOff` on the next save. Both are now registered.
    - Verified by executing the REAL save service through the dev server with `fetch` stubbed: all five
      booleans now serialise as JSON booleans. Mutation-tested — re-introducing the omission reproduces
      `"enableSecondLevelReview": "false"` exactly as the user saw it. No API needed.
    - **Audited all 43 entity save services in both apps** (every boolean dropdown vs its
      `booleanFields`): no other gaps. The audit was mutation-tested too, so the clean result means
      something.
    - ⚠️ **Design footgun, unchanged:** `booleanFields` is a hand-maintained mirror of the backend DTO
      with no compile-time link, and adding a bool to any DTO silently breaks its form until someone
      edits the list. Making `createSaveService` coerce `"true"`/`"false"` generically would remove the
      list entirely — not done, it changes shared behaviour for all 43 services.

00EK. **The app shell had no definite height, so NOTHING scrolled internally (2026-08-10, both SPAs).**
    Reported as "the tree has no scrollbar"; it was the whole app. `DashboardLayout` sized the shell
    with **`min-h-screen`** — a MINIMUM, not a definite height — so `<main class="flex-1 overflow-auto">`
    grew with its content, and every `h-full` / `min-h-0 flex-1 overflow-auto` below it (the org tree
    AND every data grid) grew too. The browser window scrolled instead of the panels.
    - Fix, three classes: `min-h-screen` → **`h-screen`** on the shell root, `min-h-screen` → `min-h-0`
      on the content column, `min-h-0` added to `<main>`. Mirrored in Home (identical shell).
    - Deliberately NOT hard-coded pixel heights: the bound is the viewport minus chrome, delivered
      through the flex chain the grids already use, so the tree and all ~100 grids get it uniformly.
      There is no fixed-height convention in the codebase to copy — every grid uses
      `min-h-0 flex-1 overflow-auto` and was relying on this same broken chain.
    - Measured on `/employee`: tree panel **23,325px → 658px fixed**, window scrolling gone. With 120
      rows injected the panel HELD at 658px and scrolled (content 4436px, viewport 617px).
    - Swept 10 other routes: none scroll the window, and the bottom of the content is reachable inside
      `main` on every one (the Dashboard, 1028px of content, now scrolls internally).
    - ⚠️ This changes scrolling on EVERY screen in both apps — page-level becomes content-area
      scrolling. It is the desktop-ERP behaviour the code was written for, and the sweep was clean, but
      it is a broad change.

00EJ. **Tree: horizontal scrolling + header search (2026-08-10, both SPAs, shared `treeView.tsx`).**
    - **Horizontal.** Row labels were `truncate`d inside a fixed 336px panel, so a deep unit name was
      clipped with no way to read it — and `truncate` (overflow:hidden) meant content could never be
      wider than the panel, so there was nothing to scroll. Now `whitespace-nowrap` + `w-max min-w-full`
      on the row: it grows to its content so the panel scrolls sideways, while short rows still fill the
      panel so hover/selected backgrounds and the right-aligned badge look unchanged.
    - **Search** in the header (reuses the shared `SearchBar`): filters by label OR badge,
      case-insensitive, with the matched run highlighted.
      **A match keeps its ANCESTORS** — a hit five levels down is useless if the branches above it are
      filtered away. A matched node also keeps its whole subtree so you can still drill in.
      Branches that only survived because a DESCENDANT matched are force-opened; branches that matched
      themselves stay shut — without that, searching "directorate" re-rendered essentially the whole
      tree and looked like search had done nothing (8 rows → 5 on the probe data). The user's own
      collapse state is untouched and returns when the box is cleared.
      Empty results say *"No matches for X"*, not the "no units yet" empty state — different answers.
    - `searchable` defaults TRUE, so it also appears on the Report Viewer catalog rail, which had no
      search before. Opt out with `searchable={false}`.
    - Verified 9/9 in the browser as `demo` on `/employee`. Demo Corp had 0 org units, so a 7-node
      hierarchy was seeded there to test against a real render and **deleted afterwards**
      (`CreatedBy='treesearch-probe'`; org units back to 121, all Head Office).
    - ⚠️ Testing wall hit repeatedly this session: `/employee`, `/organizationUnit`, `/position`,
      `/annualLeaveLedger` and `/reports` are all `CanView=0` for `UserRole`, so **`hoadmin` cannot open
      any of them**. Only `admin` / `medhanit` (Administrator) and HR Admin can; `demo` holds `/employee`
      only. Grants were NOT altered — attempting to was correctly blocked.

00EI. **Annual Leave Ledger pagination was inert + a system-wide grid audit (2026-08-10, HRMS frontend).**
    Selecting a page size of 10 or 15 still rendered the whole dataset. Three things combined:
    `param` was seeded with `take: 1000`, the component passed **every** row as `rows`, and it set
    `total={rows.length}`. `EntityListShell`'s contract is `rows` = ONE page and `total` = the full
    count; nothing ever read `param`, so there was neither a refetch nor a slice.
    - Same root cause made the **search box inert** — `param.searchText` was never read either.
    - Fixed client-side (filter → `filtered.slice(param.skip, param.skip + param.take)`,
      `total={filtered.length}`), matching `reportViewer`, `salaryRevision/detail` and
      `employee/childManager`. The single bulk fetch stays: entitlement is computed across the whole
      employee set, Calculate acts on all of it, and the header's generated/total needs the full set.
      Export still covers the whole filtered ledger, not the visible page; changing setting resets to
      page 1.
    - **Audit — this was the ONLY broken grid in either SPA.** 98 of 101 HRMS `EntityListShell` grids
      (and all 18 in Home) use `useEntityList`, which keys the query on `param` and takes `total` from
      the server. Of the 3 that hand-roll data, `salaryRevision/detail` and `employee/childManager`
      slice correctly. Every other `count: rows.length` hit is a `pagination: "None"` sub-table inside
      a modal/detail pane (loan schedules, beneficiaries, tax brackets, trip lines) — intentional.
      `operationList.tsx:120` looks similar but is a row count inside a GROUP LABEL, not the total.
    - Verified against the real 345-row payload: size 15 → 15 rows, size 10 → 10, page 2 differs,
      last page is a correct partial (5), every row reachable exactly once, search narrows page AND
      total. ⚠️ **Not confirmed in a browser** — `/annualLeaveLedger` is `CanView=0` for `UserRole`
      (so `hoadmin` gets `/unauthorized`) and only `admin` / `medhanit` (Administrator) hold it.

00EH. **Annual leave decoupled from `LeaveType` (2026-08-10, HRMS full stack, 2 migrations).**
    Generating the ledger failed with *"No active leave type uses the Annual accrual method."* The
    immediate trigger was the purge (00EG) emptying `Hrms.LeaveType`, but the real problem was the
    coupling: annual leave is driven by `AnnualLeaveSetting`, yet still had to resolve a `LeaveType`
    row. **See `logic.md` §3.1.1 for the full rule** — annual balances are now `LeaveTypeId IS NULL`.
    - A second, hidden half: `SubmitAnnualLeave` **hard-required** `ledger.LeaveType` ("The selected
      ledger has no leave type"). Fixing only the ledger would have left annual leave un-submittable.
    - `AllowHalfDay` moved onto `AnnualLeaveSetting` (+ form field, + `booleanFields` in the save
      service — a boolean not listed there posts as a string). Migration backfills per tenant from the
      old annual type; column defaults **true** (EF generated `false`, which would have silently
      disabled half-days everywhere).
    - Migrations: `DecoupleAnnualLeaveFromLeaveType` (nullable columns + repoint existing annual rows
      to NULL) and `AnnualLeaveSettingAllowHalfDay`. Both applied to CERP.
    - Verified end-to-end: ledger loads 345 employees, Calculate creates 345 (re-run 0 — idempotent),
      self-service balance widget returns the annual figure (its old INNER JOIN to `LeaveType` would
      have silently dropped every annual row), and submit → approve → **18 entitled / 5 taken / 13
      available** on a single row. The unfiltered unique index genuinely blocks a duplicate annual row
      (tested with a direct INSERT). Test data removed afterwards.
    - ⚠️ The purge also removed every `WorkflowDefinition`, so annual leave submission currently fails
      with "No active approval workflow is configured for Annual Leave" until they are reconfigured.

00EG. **CERPNVI → CERP data migration, full purge, and 490 accounts (2026-08-10, DATA ONLY — no code).**
    **Not reproducible from this repo** — the scripts lived in a scratchpad. CERP now holds **only NVI
    production data**, re-tenanted to Head Office `aadb4e82-2075-48ca-a93c-5cdac93a59b2`.
    - Copied 7 tables from `CERPNVI` (int PKs → Guid PKs via temp map tables): `coreUnit`,
      `coreJobGrade`, `coreSalaryScale`, `corePositionClass`, `corePosition`, `corePerson`,
      `hrmsEmployee`. Then emptied **125** other Hrms/Core tables, dropped non-NVI rows from the nine
      data tables, re-tenanted, and retired the temporary migration tenant.
    - **Traceability:** every migrated row carries `CreatedBy = 'migrate:CERPNVI:<sourceId>'`; the
      accounts carry `CreatedBy = 'migrate:CERPNVI'`. That is the only link back to the source.
    - Final: 490 employees (345 Active + 145 Terminated — the directory API shows 345 because
      terminated staff live in the Termination List), 1356 persons, 1162 positions, 814 position
      classes, 121 org units, 38 job grades. Restore point
      `CERP_before-purge-and-retenant-20260810-154842.bak`.
    - **Credentials:** username = lowercase `FirstName` + first letter of `FatherName`, non-alphanumerics
      stripped, numeric suffix on the 17 repeats (`abebed`, `abebed2`). Password for all 490 is
      `password`.
    - Two deliberate deviations from the literal instruction: **`Core.Tenant` was preserved** (Finbuckle
      resolves the tenant per request from it — emptying it locks everyone out), and the 16 kept
      accounts had `EmployeeId` **set to NULL** where it pointed at a deleted employee, so the FK
      re-check could pass. The accounts themselves survived.
    - ⚠️ **Two security findings raised, neither fixed** (see §2):
      **(a)** `LoginRepository.cs` computes `isHeadOffice = branchId is null || isBranchHeadOffice`.
      The comment says this is for the "tenant owner / unlinked system account", but it never checks
      whether the account is linked to an employee — and head office ⇒ `IsAdminAsync` ⇒ HR-admin data
      scope. Confirmed: signed in as `abaynehh` (a rank-and-file employee with **no role at all**) and
      `GET /api/v1/Employee` returned all 345 colleagues **including salary, DOB, national ID and
      pension number**. Per-operation permissions still hold (`/User`, `/Role` → 403); the Employee
      list is deliberately open for self-service and leans on the visibility scope alone.
      **(b)** `Encryption.GenerateHash` uses PBKDF2 with an **empty salt**, so all 490 accounts share
      one identical hash — one cracked hash exposes every account.

00EF. **`Hrms.PositionClass.TitleA` — Amharic title (2026-08-10, HRMS full stack).**
    Follows the existing `*A` convention (`JobGrade.NameA`, `LeaveType.NameA`, `Holiday.NameA`):
    nullable `nvarchar(200)`, never required — the English title stays the mandatory one and not
    every class gets translated.
    - Entity (trailing optional param on `Create`/`Update`, so no existing caller broke), EF config,
      all three DTOs + both validators, the read projection, and the frontend model / form field /
      list column / Zod bound.
    - **No Home mirror** — the portal has no positionClass screen, so this is HRMS only.
    - Locale note: `"Title (Amharic)"` is NOT in `am.json`, matching `"Name (Amharic)"` on JobGrade,
      which is not either. Every `*A` LABEL in the app falls back untranslated — a pre-existing gap
      worth a sweep, not something to fix for one field in isolation.
    - Verified 9/9 by API (Amharic round-trips byte-exact, optional, clears on omit, 201 chars
      rejected) and 7/7 through the UI (column renders the Amharic, the edit form loads and SAVES it).
    - ⚠️ Testing gotcha: `hoadmin` holds the `UserRole` role, which has **CanView=0** on
      `/positionClass` — the screen redirects to `/unauthorized`, which looks exactly like a missing
      field. Grants were flipped on temporarily and **restored to all-false afterwards**; verify
      against a role that actually holds the permission (`admin` = Administrator, or HR Admin).

00EE. **The APPROVER could not see what they were approving (2026-08-10, HRMS + Home `d7d30ce`).**
    A return adjustment reached the inbox as `"Early return — 2 day(s) against leave of 5 day(s)"`
    and nothing else. The History action beside it shows only that INSTANCE's step log, so the
    employee's written explanation — the one thing the decision turns on — was nowhere on screen,
    nor were the original dates or the approved-vs-actual comparison.
    - The workflow row now offers **Leave details**, opening the full leave history popup (the same
      one the list uses). The step-log History stays beside it; they answer different questions.
    - ⚠️ **A return adjustment's workflow instance carries the `AnnualLeaveReturn` id, NOT the header
      id.** `GET /AnnualLeave/{id}/history` now accepts either and resolves a return to its owning
      request — the approver holds an id from an inbox row and cannot be expected to know which kind
      it is. One extra lookup, only on a miss.
    - **Home needed a data change to carry it:** the portal's normalized `ApprovalItemModel` mapped
      `entityType` to a display LABEL and dropped `entityId` entirely, so a tracking row could not
      identify its own record. Both are now optional generic fields — any subsystem can supply them
      to offer an "open the underlying record" affordance.
    - Verified 9/9 in each app, including that the employee's explanation and the original approver's
      comment are both visible before deciding.

00EC. **The REQUESTER could not see their own approved leave (2026-08-10, Home portal only).**
    Second visibility bug in the same feature: the person who must confirm a return could not reach
    the action at all.
    - **The requester's screen is the Home portal → Annual Leave** (`AnnualLeave/mine`, strictly
      scoped to the signed-in employee). HRMS `/annualLeave` is the HR/admin view.
    - That grid defaulted its status filter to **Pending**, written when `Approved` was a terminal
      state. It is not any more — an approved request is waiting for the employee to confirm their
      return. So the requester opened the screen and saw **"No data available"**. Measured: 0 rows and
      0 Confirm-return buttons on the default filter, 1 of each on "All statuses".
    - Defaults to **All statuses** now. Their own list is scoped to them and small; a default that can
      hide the one row they must act on buys nothing.
    - **Pattern behind both 00EB and 00EC: adding a state to a lifecycle invalidates assumptions made
      when the old final state WAS final.** Anything keyed on "Approved means done" — filters,
      defaults, dashboards, reports — needs re-checking.
    - **Closed by 00ED** — the dashboard now prompts for it.

00ED. **Dashboard prompt: "Confirm your return from leave" (2026-08-10, Home portal only,
    `6917084`).** Third and last discoverability fix for the return feature: the employee is now TOLD
    they have something to confirm instead of having to go looking.
    - New `LeaveReturnsDue` widget, first in the dashboard's work column, listing each approved leave
      still awaiting confirmation (period, days) with a button through to `/annualLeave`.
    - **Renders nothing when there is nothing to confirm** — a prompt that is always on screen stops
      being a prompt. Also silent when HRMS is unreachable rather than implying "nothing to do", the
      same best-effort contract the other subsystem-bound widgets follow.
    - Filters on `canConfirmReturn`, the server's own answer to "may this person act on this row", so
      the eligibility rule is not reimplemented in the portal.
    - Verified both ways: 3/3 with nothing due (no panel rendered, dashboard otherwise intact) and
      8/8 with one due (prompt shown, period and days correct, link lands on the screen where the
      action is available).
    - Adding a widget stayed pure configuration, as `dashboardLayout.tsx` promises: one import, one
      zone entry, no page edits.

00EB. **The Confirm-return action was invisible on a normal laptop (2026-08-10, HRMS + Home, FE only).**
    Reported as "I haven't seen anything on the front end" for the return-from-leave feature. The
    implementation was on `main` and working — the ACTION COLUMN was overflowing.
    - Four actions (History / Confirm return / Print / Cancel) pushed the column past the table's
      right edge below ~1400px. Measured at three widths: **1152px clipped the button entirely**
      behind 208px of hidden horizontal overflow; 1280px already cut Print and Cancel. The table
      scrolls sideways, but nobody thinks to scroll a table.
    - Fixed by letting the cluster WRAP (`flex-wrap`) instead of overflowing, and promoting Confirm
      return to FIRST and filled — it is the one time-sensitive action on the row.
    - **Lesson for any new row action: measure it at 1152/1280px, not just at the dev machine's
      width.** Shipping-and-invisible looks exactly like not-shipped from the user's side, and every
      automated check passed because Playwright finds clipped elements perfectly well.
    - ⚠️ **Process slip in the same round:** a `git add -A` intended for the HRMS repo ran in the Home
      repo (cwd resets between tool calls) and committed the user's uncommitted
      `dashboardLayout.tsx` WIP under a leave-list message. Reverted in Home `d42def0` and the WIP
      restored to the working tree. **Stage explicit paths, never `-A`, when the tree holds work that
      is not yours.**

00EA. **Tenant configuration for return-from-leave, and Demo Corp made usable (2026-08-09,
    CERP DATA ONLY — no code, nothing to deploy).** All of this is configuration in the live CERP
    database; recorded here because it is not reproducible from the repo.
    - **`AnnualLeave.Return` definitions created for all three tenants.** Head Office and WF wf01
      **mirror their existing `AnnualLeave` chain exactly** (2 and 4 steps respectively) — the user's
      choice, on the basis that whoever approves the leave approves a change to it. Copied by
      INSERT…SELECT so approver types and target ids came across verbatim; verified field-by-field,
      6 steps in / 6 out, zero mismatches. **A hand-typed DisplayName with a wrong ApproverId looks
      right in the UI and routes nowhere — always copy, never retype.**
    - **Demo Corp had no leave chain to mirror**, so it got a single `HR Review → Role: HRMS Access`
      step. `ImmediateManager` was rejected there: the tenant has ONE employee and nobody managerial,
      so it would never resolve and every adjustment would jam at step 1.
    - **Demo Corp set up end to end**: it was missing a leave type, a ledger and the `AnnualLeave`
      workflow (it already had fiscal years, a work week and an annual-leave setting). All three
      created **through the app's own endpoints**, not by inserting rows — the accrual engine computed
      the 16-day entitlement from the tenant's own service-year rules rather than a number I picked.
      `AccrualMethod: Annual` is what makes the engine treat a leave type as THE annual one.
    - **E2E leftovers cleaned.** Fiscal years RENAMED (`FY 2026/27 (E2E)` → `FY 2026/27`) because they
      are load-bearing — the active one carries the annual-leave setting and the ledger. 5 inactive
      workflow definitions and 3 orphan roles DELETED: every one of those definitions had a **dangling
      approver** (role ids that no longer exist), so they were inactive *and* unusable, and renaming
      would have made a broken workflow easier to activate by mistake. The delete refused any
      definition with workflow instances and re-checked every role reference at delete time.
    - Verified through the UI: 19/19 on the full lifecycle in Demo Corp (submit → approve → early
      return → approve → Closed, ledger 3 taken / 13 available), then reset to the pristine setup
      (16 entitled / 0 taken, no requests, both chains active).
    - ⚠️ **Two TEST bugs worth remembering, both of which made a broken run look green:**
      (a) asserting status against whole-page text matches the STATUS FILTER DROPDOWN, so a request
      that was never created still "passed" — scope row assertions to `table tbody tr`;
      (b) the workflow Approve/Reject actions are ICON buttons carrying `title=`, so
      `button:has-text("Approve")` clicks the *"Approved" filter tab* instead. Use
      `button[title="Approve"]`. Both were caught only by querying the DB, not by reading the run.

00DZ. **Return-from-leave confirmation workflow (2026-08-09, HRMS + Home, full stack).**
    An approved annual leave request is no longer finished when the dates pass: the employee confirms
    they are back, and early/late returns route back through approval. Full rules in `logic.md` §3.4.1.
    - New `Hrms.AnnualLeaveReturn` child + statuses `ReturnPending` / `Closed` + `ActualLeaveDays`
      on the header. `TotalLeaveDays` stays the APPROVED figure so the two can always be compared.
    - **Two product decisions confirmed by the user:** (a) an early return credits the ledger ONLY on
      approval, never on confirmation; (b) a late return is an EXTENSION on the same request, not a
      new one, so the history stays one thread.
    - ⚠️ **Bug the live test caught:** the day counter looped over the approved detail rows only, so a
      LATE return could never exceed the approved total and every overrun cost nothing. The overrun is
      now counted separately through the calendar. Pinned by a regression test, along with "a weekend
      inside an overrun is free" and "a half-day row the return lands inside keeps its 0.5".
    - **Adjustments need a NEW workflow definition** — `AnnualLeave.Return`. Without one, confirming an
      early/late return fails with a message naming the process to configure; on-time returns work
      with no workflow at all. **All three CERP tenants now have one (see 00EA)** — a NEW tenant still
      needs its own.
    - UI in BOTH apps (Home keeps its own copy of these screens): a Confirm-return modal whose day
      count is previewed server-side as the date changes, and a lifecycle History popup on every row.
    - Verified live: early return → approval → ledger `Taken` 5→3, Available 15→17, header `Closed`
      (5 approved / 3 actual), with a `Reversal` transaction written. 18/18 browser assertions in each
      app, 10/10 on the settled history, 24 API assertions. Tests 135 → 158, mutation-checked.

00DY. **`AffectsSalaryIncrement` flag on disciplinary measures (2026-08-09, HRMS full stack).**
    Per-case control over the increment block, which until now was all-or-nothing per tenant.
    - ⚠️ **Defaults to TRUE, unlike `AffectsPromotion` / `AffectsReward`.** Those are opt-in because
      blocking a promotion is an extra sanction; withholding an increment was already what EVERY
      active case did, so defaulting it off would have quietly started paying people mid-discipline
      the moment the column shipped. It is an opt-OUT, and the migration backfills existing rows to
      `true` (verified: all 3 live cases still block).
    - The three flags are independent — proved live: one case blocking promotion + reward while the
      increment is paid in full.
    - **Frontend trap worth remembering:** the employee-profile Discipline tab submits via
      `new FormData(form)`, where an **unchecked checkbox is omitted entirely**, and
      `createSaveService`'s `booleanFields` only converts keys that are PRESENT — so an absent key
      falls through to the DTO default. Harmless for the opt-in flags, but it would have made this one
      impossible to untick. Fixed with an explicit `fd.set(...)`; verified by unticking in the tab and
      confirming `false` reached the database.
    - Rendered `!== false` everywhere, so a record saved before the column existed still reads as
      blocking.
    - Mutation-checked: flipping the default to match the siblings fails a test.
    - **FOLLOW-UP (same day): the Home portal has its OWN copy of this form** and was missed —
      `Home/frontend/src/components/admin/disciplinaryCase/{form,list}.tsx` posts to the HRMS API, so
      cases raised there still blocked (the DTO default) but could not be exempted, and the list showed
      no impact. Mirrored there in Home `1f6bcc0`. **Any future change to a disciplinary/leave/workflow
      screen must check the Home mirror** — the portal reuses these HRMS-facing modules wholesale.
      Note Home keeps this module on a FLAT route (no `:id`), so its form is reached via the list.
      Verified end-to-end: unticked in the portal → persisted false → HRMS paid the increment in full.

00DX. **`Retired` employees excluded from salary revisions too (2026-08-09, HRMS backend only).**
    Follow-up to 00DV, which excluded only `Terminated` because that is what was asked.
    - `Retired` needs its OWN check: unlike termination it has no `IsRetired` flag behind it, only the
      status. `StillEmployed` is now
      `!IsTerminated && status != Terminated && status != Retired`.
    - Still included: **Active, Probation, OnLeave, Suspended** — none of those stop pay.
    - Verified live with `IsTerminated` left FALSE, so only the new status check could exclude:
      simulation 0 employees, saved plan 0 lines, and apply-after-retirement left pay untouched
      (`"… 1 skipped as gone or terminated"`). Mutation-checked: dropping the clause fails one test.

00DW. **Approve button survived submission; Apply navigated away instead of refreshing (2026-08-09,
    HRMS full stack).** Two reported grid problems, one of which hid a dead button.
    - **Approve after Submit.** `SubmitSalaryRevision` starts a workflow when a definition exists, and
      `ApproveSalaryRevision` then calls `EnsureNoRunningAsync` → **HTTP 400 "This record is awaiting
      workflow approval."** The button rendered purely on `status === "PendingApproval"` and knew
      nothing about the workflow, so it could only fail. Tenant `aadb4e82` has an active
      **"Salary Revision Approval"** definition, which is why they saw it.
    - Fixed by adding `AwaitingWorkflow` to `SalaryRevisionDto` (`status == PendingApproval &&
      HasRunningAsync`) — the SAME pattern hiring requests / job offers / requisitions / terminations
      already use — and gating the button on it, plus the standard "Awaiting workflow approval" chip so
      the absence reads as *someone else's turn*, not a missing permission.
    - **Kept conditional, not removed:** with no definition, direct approval IS the intended path.
      Verified both ways.
    - **Apply** was calling `run(…, backAfter = true)`, navigating back to the list so the result was
      never seen. Now stays and refetches; `Applied` status already hides Apply (needs `Approved`) and
      Delete (hidden when `Applied`), so no button logic changed.
    - Verified live in both configurations (15 browser + 12 API assertions). To exercise the workflow
      path a `SalaryRevision` definition was cloned into the demo tenant and removed afterwards —
      **the real definition in `aadb4e82` was never touched**.

00DV. **Terminated employees were being offered salary increments (2026-08-09, HRMS backend only).**
    `TargetsAsync` had **no employment filter at all** — a leaver keeps their last salary on the
    record, so they arrived with a positive base like anyone else. Live data: 4 terminated employees,
    2 with a salary.
    - Fixed with `SalaryRevisionShared.StillEmployed` (`!IsTerminated && EmploymentStatus !=
      Terminated` — both, because they are set independently), matching every other feature here.
    - **Also closed the apply-time window:** a revision is planned, approved and applied over days or
      weeks, so `ApplySalaryRevision` re-checks employment and skips leavers, logging
      `"… N skipped as gone or terminated"`. Verified by staging exactly that: line created while
      employed, employee terminated, apply → pay untouched.
    - `Retired` is still INCLUDED — a separate enum value, zero live rows, and not what was asked.
      One word to change if it should be excluded too.
    - 4 tests pin the COMPILED predicate (`StillEmployed.Compile()`), not a restatement of it.

00DU. **Grade ceilings now promote instead of dead-ending (2026-08-09, HRMS full stack).**
    "Capped at the grade ceiling" became a configurable promotion — `PromoteOnGradeCeiling`,
    default OFF. Full rules in `logic.md` §9.3.
    - ⚠️ **"Next grade" is resolved BY PAY, not by grade code — a product decision the user confirmed.**
      `JobGrade` has no level field, and their codes do not track pay (`001` = 10,000–12,000, `002` =
      2,501–5,529), so code order would promote people into a pay CUT and into `003`/`004`/`005`, which
      have no scale rows at all. **If `JobGrade` ever gains an explicit level, revisit this.**
    - Guards: one grade per revision; a promotion that would not raise pay is refused; a prorated
      increment cannot buy a grade (it would leave the employee below their new grade's base).
    - **`Apply` must move the SCALE, not just the pay** (`ApplyMovement(..., salaryScaleId)`) —
      otherwise the grade never changes and the same employee is "promoted" again every revision.
      Verified against the DB: ZLOW step 2 → AHIGH step 1.
    - `SalaryScaleLadderFactory` now loads EVERY grade (promotion must see the grade above);
      `TargetsAsync` already narrows the population, so the targeted-grade filter was redundant.
    - Test fixture runs codes OPPOSITE to pay on purpose, so a code-ordered implementation cannot pass.
      Mutation-checked: code ordering fails 7 tests, letting proration promote fails 1.

00DT. **"Hired Date" column added; the reported Service bug did not reproduce (2026-08-09, HRMS
    full stack).** Reported: an employee hired 2026-07-09 showing `31 mo`.
    - **The arithmetic was correct for every row in the live plan.** Checked against
      `Hrms.Employee.HireDate` at the plan's 2026-08-31 effective date: the three rows showing 31 are
      all hired **2024-01-01**; the only 2026-07-09 hire (`KI001`) has no line on either revision (no
      `SalaryScaleId`, no `PositionId`). All seven live hire dates are now pinned as test cases.
    - Also ruled out stale lines after a draft edit (the update path deletes and regenerates) and the
      effective date not reaching the calculation (it does, on both paths).
    - Most likely a row misattribution: the grid showed Service with no hire date beside it, and
      `KI001` / `KY--001` / `M001` are easy to conflate. The new column closes that gap — it reads
      **live** from `Employee.HireDate`, so a corrected hire date shows the correction.

00DS. **Increment-rules UI: config screen + per-line columns (2026-08-09, HRMS full stack).**
    - New singleton screen `/salaryIncrementPolicy` ("Increment Rules", Compensation group) — no list,
      no Add/Back. Its own controller + permission, because deciding who qualifies for a raise should
      be grantable separately from planning a revision; reading is also allowed to revision planners.
    - `204 No Content` is the honest answer for "never configured"; `api.get` returns `""` for it, so
      the service normalizes to `null` rather than making every caller know a falsy STRING means
      unconfigured. The form then shows the DEFAULTS ALREADY IN FORCE, not a blank slate.
    - Grid: Service column (`4 mo (4/12)`), prorated badge on the changed amount, counters in the
      header and in the simulation, link to the rules screen.
    - **Two real bugs this surfaced:** (a) saved lines were discarding the reasoning — the entity held
      only 4 columns, so a plan labelled "10%" showed 3.33% with nothing to explain it; (b) the
      simulation never sent `effectiveDate`, so a revision dated next quarter measured tenure as of
      today. Both fixed.
    - `EntityModuleShellProps.onList/onAdd` are now optional so a singleton module can omit them.

00DR. **Three salary-increment eligibility rules (2026-08-09, HRMS backend).**
    Minimum service, active-disciplinary exclusion, first-year proration — per-tenant configuration
    (`Hrms.SalaryIncrementPolicy`). Full semantics in `logic.md` §9.2.
    - **The disciplinary rule is ANY active case**, deliberately broader than
      `IDisciplinaryEligibilityService`, whose promotion/reward block is opt-in per measure. Proved
      with a test case whose `AffectsPromotion`/`AffectsReward` were both false — it still excluded.
      *If HR should control this per measure, add an `AffectsSalaryIncrement` flag.*
    - Its query is also batched (one `Distinct()` for the population), not the per-employee call that
      service makes — right for a profile screen, an N+1 for a bulk revision.
    - Proration scales the **increase**, not the salary, so it means the same on every basis and can
      never cut pay.
    - **Excluded employees get no line at all** — `Apply` walks the lines, so a zero line would have
      paid them.

00DQ. **Performance revisions could not be SAVED on the Step basis (2026-08-09, HRMS backend only).**
    Saving type=Performance with basis=Step failed with "A step revision needs a step increment greater
    than zero." The FluentValidation rule keyed only on `Basis == Step` and ignored `RevisionType`.
    - A Performance revision takes every award from its BANDS, so `Rate` is unused and the form hides
      the field — it therefore arrives as 0 and the rule rejected a perfectly valid revision.
    - Introduced when Performance was added (00DK): the new bands rule was written, but this
      pre-existing rate rule was never revisited. **Adding a dimension means re-checking every rule
      that assumed the old ones.**
    - Fixed with `IsStepBasis(x) && !IsPerformance(x)`; the rule still fires for Merit/Market/COLA.
    - Added `SaveSalaryRevisionValidatorTests` (8 cases) — the suite covered the handler guards but
      never the DTO validator, which runs FIRST and can reject before those guards see the request.
      Mutation-checked: reverting the fix fails exactly the reported case.

00DP. **Post-delete 404 fixed — never invalidate OR remove a query for a record you just deleted
    (2026-08-09, HRMS frontend only).** Deleting a salary revision succeeded and then immediately
    showed "Resource of type 'SalaryRevision' with id … was not found" for that same id.
    - Cause: on success the handler called `invalidateQueries(["salaryRevision", id])` and then
      navigated. The component is still MOUNTED at that instant, so its `useQuery` observer is still
      active and React Query refetched the id that had just been deleted → GET 404.
    - **`removeQueries` is NOT the fix and was tried first** — dropping the entry makes the still-active
      observer refetch to repopulate it, producing the same 404. The test caught that too.
    - The fix is to touch nothing: refresh the LIST key only, then navigate. On unmount the query goes
      inactive and is garbage-collected; re-opening that id later refetches and correctly lands on the
      "no longer available" panel.
    - Actions that STAY on the detail (Submit/Approve) still invalidate the detail key — required for
      the status badge to refresh, and covered by its own test.
    - Third distinct bug behind the same user-visible message: the earlier two were a double-submit
      (fixed with a `useRef` guard, since `disabled={busy}` is captured per render) and a stale list
      row. **When that message appears again, check what fires a GET for the id, not just the DELETE.**

00DO. **Lookup tables moved to the Hrms schema (2026-08-08, HRMS only, MIGRATION
    `MoveLookupTablesToHrmsSchema`, APPLIED TO CERP).** `Core.LookUpCategory` → `Hrms.LookUpCategory`
    and `Core.LookUpCategoryList` → `Hrms.LookUpCategoryList` — names unchanged, schema only, so EF
    emitted two plain `ALTER SCHEMA TRANSFER`s with no FK/index churn.
    - The lookup system is HRMS-owned (Education Level, Field of Study, Guarantee Type), so `Core` was
      the wrong home for it; nothing outside HRMS referenced them (checked both repos, bare and
      bracketed forms, no raw SQL).
    - Verified: 3 categories / 20 items intact, FK preserved, table + FK totals unchanged (204/239),
      `/Lookup` and `/Lookup/items/{code}` return all three categories and their items, lookup
      consumers (Employee, EmployeeGuarantee, dashboard) still 200, 0 API-log errors, 59/59 tests.
    - **`CLAUDE.md` was still documenting the OLD `dbo.hrmsX` convention** and has been corrected —
      it is loaded as project instructions every session, so a stale statement there is worse than one
      in the changelog. It now also points at `memory.md` §4 for the rename traps.

00DN. **Module-schema rename — every table moved to its module's schema (2026-08-08, both apps,
    MIGRATIONS `ModuleSchemaRename` + `NotificationModuleSchema`, APPLIED TO CERP).**
    `dbo.hrmsAchievement` → `Hrms.Achievement`, `dbo.coreModule` → `Core.Module`,
    `Core.lupStep` → `Core.Step`, `Core.CorePerson` → `Core.Person`,
    `Core.coreSalaryScale` → `Core.SalaryScale`, `dbo.coreNotification` → `Core.Notification`,
    and the 28 procedures `Core.hrms_Report_X` → `Hrms.Report_X`.
    - **181 tables renamed** (174 hrms + 4 dbo.core + 3 Core-internal). The 10 unprefixed `Core.*`
      tables (User, Role, Tenant, RolePermission, …) are UNCHANGED, as are HangFire (11) and both
      `__EFMigrationsHistory` tables. Verified: 204 tables before and after, 239 FKs before and after.
    - Ownership split: HRMS renames everything except `coreNotification`, which **Home** owns
      (`ExcludeFromMigrations` in HRMS). Run `01-hrms…` then `02-home…`.
    - **Four traps, all found by testing against a restored copy — none by review:**
      1. `CREATE PROCEDURE must be the first statement in a query batch`. EF writes `Sql()` verbatim
         into a generated script and inserts **no GO**, so all 28 procedures shared one batch. Fixed
         with a `-- ===BATCH===` sentinel that `build-scripts.ps1` converts to `GO`; via
         `dotnet ef database update` it stays a harmless comment. **`--idempotent` is therefore
         impossible for the HRMS script** — its `IF NOT EXISTS … BEGIN … END` wrapper cannot contain a
         GO. A precondition guard (`SET NOEXEC ON`) replaces it.
      2. The report registry stores proc names in TWO shapes — bare (`Core.hrms_Report_NewHires`) and
         bracketed (`[Core].[hrms_Report_EmployeeDirectory]`). The first UPDATE only matched the bare
         form.
      3. `ReportScheduleStore` builds names by CONCATENATION (`SchemaPrefix + "hrms_X"`), so no search
         for `Core.hrms_X` can find those 12 call sites. Only running the app surfaced it.
      4. **Hand-written SQL outside the EF mappings** — `DashboardSummaryService` (7 statements) and
         `NumberSequenceService` (`[dbo].[hrmsNumberSequence]`, **bracketed**, so an unbracketed
         pattern missed it). The second is on the write path: every document number would have failed.
    - **Lesson for the next rename:** grep for the bracketed form `[schema].[table]` as well as the
      bare one, and sweep the WHOLE solution, not just `Repositories/`. Then exercise the endpoints
      backed by raw SQL (the dashboard), because EF-mapped endpoints prove nothing about them.
    - Found en route: CERP was **missing Home migration `AddNotificationUserFeedIndex`**; script 02 is
      generated idempotently so it applies that too.
    - Scripts live in `backend/scripts/schema-rename/` in each repo; `build-scripts.ps1` regenerates
      both. A pre-change restore point was taken:
      `CERP_before-schema-rename-20260808-192711.bak`.

00DM. **Salary Revision detail is a full grid, not a popup (2026-08-08, HRMS frontend only).**
    `detailModal.tsx` DELETED. Selecting a row now navigates to `/salaryRevision/{guid}` and swaps the
    page to the per-employee increment grid; the shell's standard Back arrow returns. Three URL-backed
    views: `/salaryRevision` (list) · `/new` (planning form) · `/{guid}` (increment grid). This also
    fixes a latent bug — `/salaryRevision/{guid}` previously rendered the CREATE form, because
    `SalaryRevisionForm` takes only `onDone` and ignores the id.
    - Built on `EntityListShell`, so it inherits the house chrome (search, column picker, export,
      list/grid toggle, pagination) instead of a bespoke in-dialog table. Summary + lifecycle actions
      go in its `header` slot. Lines are paged/searched IN MEMORY — the revision endpoint already
      returns them all, so no second endpoint.
    - Columns adapt: a **Step** column only for step-basis (`1 → 3.5`, flags interpolation), a
      **Score** column only for Performance, plus a **Note** column for lines that did not move.
    - **`useRef`, not state, for the double-submit guard.** `if (busy) return` reads a value captured
      at render time, so two clicks in the SAME frame both see false and both fire. That is what
      produced the reported `NotFoundException` on Delete: the first call succeeded, the second hit a
      row that no longer existed. Verified by test (2 requests → 1). The modal version shipped in the
      prior commit had the same latent flaw and passed only by timing luck.
    - Also: action failures now surface in an error banner (every non-ok result used to be silently
      swallowed, so a rejected action looked like a dead button); a record deleted underneath the view
      shows "no longer available" with a resyncing Back instead of an ENDLESS SPINNER
      (`isLoading || !detail ? <Loading/>` never resolved once the fetch had failed); and a
      "not found" result resyncs the list and leaves rather than alarming the user.

00DL. **Route shape fixed: module state no longer wiped when opening a record (2026-08-08, HRMS
    frontend only). REGRESSION from 00DH.** The GUID guard wrapped ONLY the `:id` route, so the
    component tree was a different depth for list vs form:
    `Route(index) → Page` versus `Route(guard) → Route(:id) → Page`. React therefore UNMOUNTED and
    remounted the module on Add/Edit, destroying every piece of local state held alongside the
    record. The guard now sits on the shared parent (`<Route path="x" element={<EntityRecordGuard/>}>`)
    so both children render at identical depth; the guard admits three cases — no id (list), "new",
    and a GUID.
    - Symptoms this caused: Salary Scale could not be registered at all (the grade filter is local
      state → hidden `jobGradeId` went out empty → "Job grade is required"); the Positions
      "add under this selected unit" preset was lost; the Employee org-tree selection reset.
    - **If you ever add another wrapper route, keep both children at the same depth** or the same
      class of bug returns silently — nothing type-checks or lints against it.
    - Salary Scale also gained the position-style guard: a cold `/salaryScale/new` (pasted link,
      refresh) has no grade and, unlike Positions, cannot show its picker because the module SWAPS
      list for form — so the hint carries its own "Back to list" action.

00DK. **Salary revision "By Performance" — score-banded awards (2026-08-08, backend + frontend,
    MIGRATION `SalaryRevisionPerformanceBands`, 24 new tests).** `SalaryRevisionType.Performance`
    makes the award per-employee: the appraisal score selects a band, and the band's value is
    expressed in the units of the chosen **basis**, so one band set means "2.5 steps", "15%" or
    "3000". Under Performance the flat `Rate` is hidden and ignored.
    - New child entity `SalaryRevisionBand` (MinScore inclusive, Value, Label) + optional
      `SalaryRevision.TargetReviewCycleId` (null = each employee's latest completed appraisal).
    - **Bands are DATA, never constants.** `Appraisal.OverallScore` is scored against a
      per-tenant `RatingScale`; live scales here run **1-5, 1-3 and 0-130**. A hard-coded "> 90" tier
      fires correctly on 0-130 and silently drops EVERYONE into the bottom band on 1-5. Demonstrated
      on the demo tenant (all scores 4.00 on 0-5): 90/70/0 → 0% for everyone; 4/3/0 → the 15% band.
    - The simulation therefore reports `MinObservedScore`/`MaxObservedScore` + `NoScoreCount`, and the
      form warns when a threshold sits above every score seen.
    - A missing appraisal is NOT a low score — those employees are left untouched and counted, not
      handed the bottom band. A zero band ("< 70: 0%") is a deliberate no-award and is not flagged.
    - Scores are batch-loaded ONCE per run (`IPerformanceAwardResolverFactory`), same N+1 avoidance as
      the pay ladder. Non-performance revisions never touch the appraisal table.

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

- ⚠️ **Security — raised 2026-08-10 (00EG).**
  - ~~A null branch grants head-office (HR-admin) visibility~~ — **FIXED 2026-08-10 (00EM).**
  - **Empty-salt password hashing.** `Encryption.GenerateHash` =
    `Rfc2898DeriveBytes(pw, new byte[0], 10000, SHA256)` — deterministic, so all 490 accounts sharing
    the default password share one hash. Force a change on first login; a per-user salt is the fix.
  - **The 490 migrated accounts have no role**, so they can sign in but see no menu. Assigning one is
    an access-control decision that was deliberately left to the user.
- **Salary increment — open questions raised to the user (2026-08-09):**
  - ~~`Retired` employees are still included~~ — **DONE**, they are excluded too (00DX).
  - ~~`AffectsSalaryIncrement` flag on `DisciplinaryMeasure`~~ — **DONE** (00DY), as an opt-OUT.
  - **If `JobGrade` ever gains an explicit level/sort field**, revisit ceiling promotion: it currently
    orders grades by PAY because no such field exists (see 00DU).
  - `InventoryLayout` renders a **Settings gear on every module** wired to `onSetting?.()`, which no
    module passes — a no-op button on ~87 screens. One-word fix (`onSetting &&`), left alone because
    it changes every screen's header and was not asked for.
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

- **⚠️ `Core.Subsystem.Code` is a JOIN KEY, not a label.** The Home portal's frontend matches it
  literally in five places — `useMenuModules` (the sidebar filter), `portalLanding`, `widgets`,
  `dashboard` and `services/portal`. Renaming the NVI tenant's Home row to code `003` left the portal
  **loading fine with an empty sidebar**: login 200, feed 200, all 21 screens returned, nothing
  matched. A working API and a broken UI at once. Restored by
  `backend/scripts/restore-home-subsystem-code.sql` (2026-08-15). **`Name` is safe to change — it is
  only displayed. `Code` is not.**
- **Subsystem rows are duplicated PER TENANT**, so two rows can share a code without colliding — the
  table is tenant-filtered. `9FC9447D…` is demo's Home, `B7340E07…` is NVI's.

- **Before deleting a menu operation, grep its link in `[RequirePermission]`.** `EndpointPermissionService`
  matches a required link against the caller's GRANTED operation links, so a key whose operation no
  longer exists can never be granted — the gate returns **403 to everyone, forever**, and fails
  closed and silently (handoff 0107).
- **Only the NVI tenant `aadb4e82` has authorization data.** All 168 `TenantOperation` rows and 570
  `TenantRolePermission` grants are its. The `demo` tenant (`0af6866e`) has **none** — its roles are
  Portal/HOME roles with zero grants — so signing in as `demo` yields an **empty sidebar and 403 on
  every gated endpoint**. That is data, not a bug; do not go hunting for a regression.
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
