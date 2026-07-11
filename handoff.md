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
  `SeedRecruitmentNumberSequences`, all applied to CERP). History through `709ece0` is pushed to
  origin; `d7058db`→`077e531` are **local only** (not pushed).
- **Uncommitted:** nothing (this docs-sync edit only). Untracked: `~$ Management.docx` (Office
  lock file — do not commit; consider gitignoring `~$*`).
- Commit/push only when the user explicitly asks. The pre-commit hook prompts you to confirm
  `memory.md` / `handoff.md` / `logic.md` are updated when a commit changes code without them
  (bypass: `SKIP_DOC_CHECK=1` or `git commit --no-verify`). `App_Data/employee-photos/` is gitignored.

## 1. Most recent changes (latest first)

1. **StageModal score contradiction + error-message artifact** (frontend only, builds clean.
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
