"use client";

import { useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  ClipboardList,
  Plus,
  History,
  ArrowRightCircle,
  Star,
  CalendarClock,
  BadgeDollarSign,
  Trophy,
} from "lucide-react";
import InterviewsModal from "./interviewsModal";
import OfferModal from "./offerModal";
import RankingModal from "../jobRequisition/rankingModal";
import { EntityListShell, useEntityList } from "@/template";
import {
  getAllJobApplications,
  getJobApplication,
  createJobApplication,
  moveApplicationStage,
  getAllCandidates,
  getAllJobRequisitions,
  scoreJobApplication,
} from "@/services/admin/recruitment";
import type { JobApplicationModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { applicationStageOptions } from "@/constants/orgStructure";

const lookupParam = { ...parameterInitialData, take: 200 };
const fmtDate = (v?: string) => (v ? v.slice(0, 10) : "—");

const STAGE_TONE: Record<string, string> = {
  Received: "bg-muted/30 text-muted",
  Screening: "bg-info/15 text-info",
  Shortlisted: "bg-primary/10 text-primary",
  Interview: "bg-warning/15 text-warning",
  Selected: "bg-success/15 text-success",
  OfferPending: "bg-warning/15 text-warning",
  Hired: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
  Withdrawn: "bg-muted/30 text-muted",
};

const TERMINAL = ["Rejected", "Withdrawn", "Hired"];

/** Register a new application: candidate × posted/approved requisition (HC098). */
function NewApplicationModal({ onClose, onDone }: { onClose: () => void; onDone: () => void }) {
  const { t } = useTranslation();
  const [candidateId, setCandidateId] = useState("");
  const [requisitionId, setRequisitionId] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: candidates } = useQuery({
    queryKey: ["candidates", lookupParam],
    queryFn: () => getAllCandidates(lookupParam),
  });
  const { data: requisitions } = useQuery({
    queryKey: ["jobRequisitions", "open-lookup"],
    queryFn: () => getAllJobRequisitions(lookupParam),
  });
  const openRequisitions = (requisitions?.data ?? []).filter(
    (r) => r.status === "Posted" || r.status === "Approved",
  );

  const confirm = async () => {
    if (!candidateId || !requisitionId) {
      setError(t("Select both a candidate and an open requisition."));
      return;
    }
    setBusy(true);
    const res = await createJobApplication({ candidateId, requisitionId });
    setBusy(false);
    if (!res.ok) {
      setError(res.message);
      return;
    }
    onDone();
  };

  const selectCls =
    "h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground";

  return (
    <Modal
      visible
      size="md"
      title={t("New Application")}
      description={t("Applications are received on approved or posted requisitions only.")}
      onClose={onClose}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Cancel")}
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={confirm}
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
          >
            {t("Register Application")}
          </button>
        </>
      }
    >
      <div className="space-y-3">
        <div>
          <label className="mb-1 block text-xs font-semibold uppercase tracking-wide text-muted">
            {t("Candidate")} <span className="text-error">*</span>
          </label>
          <select value={candidateId} onChange={(e) => setCandidateId(e.target.value)} className={selectCls}>
            <option value="">{t("Select a candidate…")}</option>
            {(candidates?.data ?? []).map((c) => (
              <option key={c.id} value={c.id}>
                {c.candidateNumber} — {c.fullName}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className="mb-1 block text-xs font-semibold uppercase tracking-wide text-muted">
            {t("Requisition")} <span className="text-error">*</span>
          </label>
          <select value={requisitionId} onChange={(e) => setRequisitionId(e.target.value)} className={selectCls}>
            <option value="">{t("Select an open requisition…")}</option>
            {openRequisitions.map((r) => (
              <option key={r.id} value={r.id}>
                {r.requisitionNumber} — {r.title}
              </option>
            ))}
          </select>
        </div>
        {error && <p className="text-xs text-error">{error}</p>}
      </div>
    </Modal>
  );
}

/** Advance / decline an application with a note (+ optional screening outcome, HC099). */
function StageModal({
  application,
  onClose,
  onDone,
}: {
  application: JobApplicationModel;
  onClose: () => void;
  onDone: () => void;
}) {
  const { t } = useTranslation();
  const [stage, setStage] = useState("");
  const [note, setNote] = useState("");
  const [score, setScore] = useState("");
  const [remarks, setRemarks] = useState(application.screeningRemarks ?? "");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // On a vacancy with weighted criteria, the criterion engine OWNS the screening total —
  // no manual score field is offered (the backend rejects it anyway).
  const autoScored = (application.totalCriteriaCount ?? 0) > 0;

  // OfferPending / Hired are driven by the offer & hire processes — never manual moves,
  // so they don't appear as options (the backend rejects them anyway).
  const options = applicationStageOptions.filter(
    (o) => o.id !== application.stage && o.id !== "OfferPending" && o.id !== "Hired",
  );

  const confirm = async () => {
    if (!stage) {
      setError(t("Select the stage to move to."));
      return;
    }
    setBusy(true);
    const res = await moveApplicationStage({
      id: application.id!,
      stage,
      note: note || undefined,
      screeningScore: autoScored || score === "" ? undefined : Number(score),
      screeningRemarks: remarks || undefined,
    });
    setBusy(false);
    if (!res.ok) {
      setError(res.message);
      return;
    }
    onDone();
  };

  return (
    <Modal
      visible
      size="md"
      title={t("Move Application Stage")}
      description={`${application.candidateName} → ${application.requisitionTitle}`}
      onClose={onClose}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Cancel")}
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={confirm}
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
          >
            {t("Apply")}
          </button>
        </>
      }
    >
      <div className="space-y-2 text-sm">
        <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
          {t("New Stage")} <span className="text-error">*</span>
        </label>
        <select
          value={stage}
          onChange={(e) => setStage(e.target.value)}
          className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
        >
          <option value="">{t("Select…")}</option>
          {options.map((o) => (
            <option key={o.id} value={o.id}>{o.name}</option>
          ))}
        </select>
        <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Note")}</label>
        <input
          type="text"
          value={note}
          onChange={(e) => setNote(e.target.value)}
          placeholder={t("Logged on the stage history")}
          className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
        />
        {autoScored ? (
          <>
            {/* No manual score on a criteria-scored vacancy — the weighted engine owns the total. */}
            <p className="rounded-md border border-info/30 bg-info/10 px-3 py-2 text-xs text-foreground">
              {t("This vacancy is scored against weighted criteria — the screening total ({{score}}) is calculated automatically from the score sheet (★).", {
                score: application.screeningScore ?? "—",
              })}
            </p>
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
              {t("Screening Remarks")}
            </label>
            <input
              type="text"
              value={remarks}
              onChange={(e) => setRemarks(e.target.value)}
              className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
            />
          </>
        ) : (
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
                {t("Screening Score (0–100)")}
              </label>
              <input
                type="text"
                value={score}
                onChange={(e) => setScore(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
                {t("Screening Remarks")}
              </label>
              <input
                type="text"
                value={remarks}
                onChange={(e) => setRemarks(e.target.value)}
                className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
              />
            </div>
          </div>
        )}
        {error && <p className="whitespace-pre-line text-xs text-error">{error}</p>}
      </div>
    </Modal>
  );
}

/** Evaluator score sheet: per-criterion scores → auto-calculated weighted total. */
function ScoreModal({
  applicationId,
  onClose,
  onDone,
}: {
  applicationId: string;
  onClose: () => void;
  onDone: () => void;
}) {
  const { t } = useTranslation();
  const [entries, setEntries] = useState<Record<string, { score: string; remarks: string }>>({});
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["jobApplication", applicationId],
    queryFn: () => getJobApplication(applicationId),
  });

  // Level-aware sheet: global criteria are scoreable at every step; level-scoped criteria only
  // while the application sits at that level (mirrors the backend's ScoreableCriteriaCount).
  const sheet = (data?.criterionScores ?? []).filter(
    (c) => !c.appliesAtStage || c.appliesAtStage === data?.stage,
  );
  const get = (criterionId: string) =>
    entries[criterionId] ?? {
      score: String(sheet.find((c) => c.criterionId === criterionId)?.score ?? ""),
      remarks: sheet.find((c) => c.criterionId === criterionId)?.remarks ?? "",
    };
  const set = (criterionId: string, patch: Partial<{ score: string; remarks: string }>) =>
    setEntries((p) => ({ ...p, [criterionId]: { ...get(criterionId), ...patch } }));

  // Live weighted total preview (mirrors the server's auto-calculation).
  const preview = (() => {
    const withValues = sheet
      .map((c) => ({ w: c.weight, raw: get(c.criterionId).score }))
      .filter((r) => r.raw !== "" && !Number.isNaN(Number(r.raw)));
    if (withValues.length === 0) return null;
    const tw = withValues.reduce((s, r) => s + r.w, 0);
    return tw === 0
      ? null
      : Math.round((withValues.reduce((s, r) => s + Number(r.raw) * r.w, 0) / tw) * 100) / 100;
  })();

  const confirm = async () => {
    const scores = sheet
      .map((c) => ({ criterionId: c.criterionId, raw: get(c.criterionId) }))
      .filter((x) => x.raw.score !== "" && !Number.isNaN(Number(x.raw.score)))
      .map((x) => ({
        criterionId: x.criterionId,
        score: Number(x.raw.score),
        remarks: x.raw.remarks || undefined,
      }));
    if (scores.length === 0) {
      setError(t("Enter at least one criterion score (0–100)."));
      return;
    }
    setBusy(true);
    const res = await scoreJobApplication({ id: applicationId, scores });
    setBusy(false);
    if (!res.ok) {
      setError(res.message);
      return;
    }
    onDone();
  };

  return (
    <Modal
      visible
      size="lg"
      title={t("Score Application")}
      description={data ? `${data.candidateName} → ${data.requisitionTitle}` : undefined}
      onClose={onClose}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Cancel")}
          </button>
          <button
            type="button"
            disabled={busy || isLoading}
            onClick={confirm}
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
          >
            {t("Save Scores")}
          </button>
        </>
      }
    >
      {isLoading && <Loading />}
      {!isLoading && sheet.length === 0 && (
        <p className="py-6 text-center text-sm text-muted">
          {(data?.totalCriteriaCount ?? 0) > 0
            ? t("None of the vacancy's criteria are scored at the {{stage}} step — they belong to other recruitment levels.", { stage: t(data?.stage ?? "") })
            : t("The requisition has no screening criteria — define them on the requisition first.")}
        </p>
      )}
      {!isLoading && sheet.length > 0 && (
        <div className="space-y-2">
          {sheet.map((c) => (
            <div key={c.criterionId} className="rounded-md border border-border/60 px-3 py-2">
              <div className="flex flex-wrap items-center gap-2">
                <span className="min-w-0 flex-1 text-sm font-medium text-foreground">
                  {c.criterionName}
                  {c.isMandatory && <span className="ml-1 text-error">*</span>}
                  <span className="ml-1.5 rounded bg-primary/10 px-1.5 py-0.5 text-[10px] font-bold text-primary">
                    {c.weight}%
                  </span>
                  {c.appliesAtStage && (
                    <span className="ml-1 rounded bg-info/15 px-1.5 py-0.5 text-[10px] font-semibold text-info">
                      {t(c.appliesAtStage)}
                    </span>
                  )}
                </span>
                {c.evaluatorName && (
                  <span className="rounded bg-secondary px-2 py-0.5 text-[11px] text-muted" title={c.evaluatorName}>
                    {t("Evaluators")}: {c.evaluatorName}
                  </span>
                )}
                <input
                  type="text"
                  value={get(c.criterionId).score}
                  onChange={(e) => set(c.criterionId, { score: e.target.value })}
                  placeholder="0–100"
                  className="h-8 w-20 rounded-md border border-border bg-background px-2 text-right text-sm text-foreground"
                />
              </div>
              <input
                type="text"
                value={get(c.criterionId).remarks}
                onChange={(e) => set(c.criterionId, { remarks: e.target.value })}
                placeholder={t("Remarks…")}
                className="mt-1.5 h-7 w-full rounded-md border border-border bg-background px-2 text-xs text-foreground"
              />
            </div>
          ))}
          <div className="flex items-center justify-between rounded-md bg-secondary/60 px-3 py-2 text-sm">
            <span className="font-medium text-muted">{t("Weighted total (auto-calculated)")}</span>
            <span className="text-lg font-bold tabular-nums text-primary">{preview ?? "—"}</span>
          </div>
          {error && <p className="text-xs text-error">{error}</p>}
        </div>
      )}
    </Modal>
  );
}

/** Full stage history of one application (HC098 traceability). */
function HistoryModal({ applicationId, onClose }: { applicationId: string; onClose: () => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({
    queryKey: ["jobApplication", applicationId],
    queryFn: () => getJobApplication(applicationId),
  });

  return (
    <Modal
      visible
      size="md"
      title={t("Application History")}
      description={data ? `${data.candidateName} → ${data.requisitionTitle}` : undefined}
      onClose={onClose}
      footer={
        <button
          type="button"
          onClick={onClose}
          className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
        >
          {t("Close")}
        </button>
      }
    >
      {isLoading && <Loading />}
      {!isLoading && data && (
        <ol className="space-y-2">
          {(data.stageLog ?? []).map((l, i) => (
            <li key={i} className="flex items-start gap-3 rounded-md border border-border/60 px-3 py-2">
              <span className={`mt-0.5 rounded px-2 py-0.5 text-xs font-semibold ${STAGE_TONE[l.stage] ?? ""}`}>
                {t(l.stage)}
              </span>
              <span className="min-w-0 flex-1 text-sm">
                <span className="block text-foreground">{l.note || "—"}</span>
                <span className="block text-xs text-muted">
                  {l.actedBy || "—"} · {new Date(l.actedAt).toLocaleString()}
                </span>
              </span>
            </li>
          ))}
        </ol>
      )}
    </Modal>
  );
}

/** Application pipeline (HC098–HC099): stage-filtered list with move/history actions. */
function JobApplications() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showNew, setShowNew] = useState(false);
  const [stageFor, setStageFor] = useState<JobApplicationModel | null>(null);
  const [historyFor, setHistoryFor] = useState<string | null>(null);
  const [scoreFor, setScoreFor] = useState<string | null>(null);
  const [interviewsFor, setInterviewsFor] = useState<JobApplicationModel | null>(null);
  const [offersFor, setOffersFor] = useState<JobApplicationModel | null>(null);
  const [rankingFor, setRankingFor] = useState<string | null>(null);

  const list = useEntityList({
    queryKey: "jobApplications",
    fetchPage: getAllJobApplications,
  });

  // Vacancy scope: filters the pipeline to one requisition and unlocks its ranking view.
  const { data: requisitions } = useQuery({
    queryKey: ["jobRequisitions", "pipeline-lookup"],
    queryFn: () => getAllJobRequisitions(lookupParam),
  });

  const activeStage = (list.param.status as string) || "";
  const setStage = (status: string) =>
    list.setParam((p) => ({ ...p, status: status || undefined, skip: 0 }) as never);
  const activeVacancy = (list.param.parentId as string) || "";
  const setVacancy = (parentId: string) =>
    list.setParam((p) => ({ ...p, parentId: parentId || undefined, skip: 0 }) as never);

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["jobApplications"] });
    queryClient.invalidateQueries({ queryKey: ["jobRequisitions"] });
    queryClient.invalidateQueries({ queryKey: ["candidates"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "candidateName",
          label: "Candidate",
          render: (text: string, r: JobApplicationModel) => (
            <span className="block">
              <span className="block font-semibold">{text}</span>
              <span className="block text-xs text-muted">{r.candidateNumber}</span>
            </span>
          ),
        },
        {
          name: "requisitionTitle",
          label: "Vacancy",
          render: (text: string, r: JobApplicationModel) => (
            <span className="block">
              <span className="block">{text}</span>
              <span className="block text-xs text-muted">{r.requisitionNumber}</span>
            </span>
          ),
        },
        { name: "appliedAt", label: "Applied", render: (v: string) => fmtDate(v) },
        {
          name: "screeningScore",
          label: "Screening",
          render: (v: number, r: JobApplicationModel) => (
            <span className="block">
              <span className="tabular-nums">{v ?? "—"}</span>
              {r.screeningRemarks && (
                <span className="block max-w-[180px] truncate text-xs text-muted" title={r.screeningRemarks}>
                  {r.screeningRemarks}
                </span>
              )}
            </span>
          ),
        },
        {
          name: "stage",
          label: "Stage",
          render: (text: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STAGE_TONE[text] ?? ""}`}>
              {t(text)}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: JobApplicationModel) => {
            const stage = record.stage ?? "";
            const terminal = TERMINAL.includes(stage);
            // Process order: evaluate → interview → decide (stage) → offer → audit (history).
            return (
              <span className="inline-flex items-center gap-1">
                {/* 1. Score — level-aware: global criteria keep it on every stage; level-scoped
                    criteria surface it only at their level. Nothing scoreable → no button. */}
                {(record.scoreableCriteriaCount ?? 0) > 0 && !terminal && (
                  <button
                    type="button"
                    onClick={() => record.id && setScoreFor(record.id)}
                    title={t("Score against the requisition criteria ({{n}} scoreable at this step)", {
                      n: record.scoreableCriteriaCount,
                    })}
                    className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-warning hover:text-warning"
                  >
                    <Star size={14} />
                  </button>
                )}
                {/* 2. Interviews — ALWAYS viewable (the record survives the decision);
                    scheduling/scoring is gated inside the modal for finished applications. */}
                <button
                  type="button"
                  onClick={() => setInterviewsFor(record)}
                  title={
                    stage === "Interview"
                      ? t("Interviews & panel feedback (HC101–HC109)")
                      : t("Interviews (view — scheduling happens at the Interview level)")
                  }
                  className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-info hover:text-info"
                >
                  <CalendarClock size={14} />
                </button>
                {/* 3. Move Stage — locked while an offer drives the pipeline (OfferPending) and
                    at final stages. */}
                <button
                  type="button"
                  disabled={terminal || stage === "OfferPending"}
                  onClick={() => setStageFor(record)}
                  title={
                    stage === "OfferPending"
                      ? t("The offer drives this application — respond to or withdraw it instead")
                      : terminal
                        ? t("A {{stage}} application is final", { stage: t(stage) })
                        : t("Move Stage")
                  }
                  className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary disabled:cursor-not-allowed disabled:opacity-40"
                >
                  <ArrowRightCircle size={14} />
                </button>
                {/* 4. Offers — from Selected onward, and viewable on finished applications
                    (creation is gated inside the modal). */}
                <button
                  type="button"
                  disabled={!["Selected", "OfferPending", "Hired", "Rejected", "Withdrawn"].includes(stage)}
                  onClick={() => setOffersFor(record)}
                  title={
                    ["Selected", "OfferPending"].includes(stage)
                      ? t("Offers (HC111–HC114)")
                      : terminal || stage === "Hired"
                        ? t("Offers (view)")
                        : t("Offers open once the candidate is Selected")
                  }
                  className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-success hover:text-success disabled:cursor-not-allowed disabled:opacity-40"
                >
                  <BadgeDollarSign size={14} />
                </button>
                {/* 5. History — the audit trail, always. */}
                <button
                  type="button"
                  onClick={() => record.id && setHistoryFor(record.id)}
                  title={t("Stage History")}
                  className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
                >
                  <History size={14} />
                </button>
              </span>
            );
          },
        },
      ] as DataTableColumnModel[],
    [t],
  );

  return (
    <div className="m-1 flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      <div className="flex flex-wrap items-center gap-2 border-b border-border px-3 py-2">
        <h1 className="flex items-center gap-2 text-sm font-semibold text-foreground">
          <ClipboardList size={16} className="text-primary" />
          {t("Applications")}
          <span className="text-xs font-normal text-muted">— {t("recruitment pipeline (HC098)")}</span>
        </h1>
        <div className="ml-auto flex items-center gap-1">
          {/* Vacancy scope + its ranking (only meaningful with a vacancy selected) */}
          <select
            value={activeVacancy}
            onChange={(e) => setVacancy(e.target.value)}
            title={t("Filter the pipeline to one vacancy")}
            className="h-7 max-w-56 rounded-md border border-border bg-background px-2 text-xs text-foreground"
          >
            <option value="">{t("All vacancies")}</option>
            {(requisitions?.data ?? []).map((r) => (
              <option key={r.id} value={r.id}>
                {r.requisitionNumber} — {r.title}
              </option>
            ))}
          </select>
          <button
            type="button"
            disabled={!activeVacancy}
            onClick={() => setRankingFor(activeVacancy)}
            title={activeVacancy ? t("Candidate ranking & waitlist") : t("Select a vacancy first")}
            className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs text-foreground hover:border-warning hover:text-warning disabled:cursor-not-allowed disabled:opacity-40"
          >
            <Trophy size={13} /> {t("Ranking")}
          </button>
          <span className="mx-1 h-4 w-px bg-border" />
          <button
            type="button"
            onClick={() => setStage("")}
            className={`rounded-full px-2.5 py-1 text-xs font-medium ${activeStage === "" ? "bg-primary/10 text-primary" : "text-muted hover:text-foreground"}`}
          >
            {t("All")}
          </button>
          {applicationStageOptions.map((o) => (
            <button
              key={o.id}
              type="button"
              onClick={() => setStage(o.id)}
              className={`rounded-full px-2.5 py-1 text-xs font-medium ${activeStage === o.id ? "bg-primary/10 text-primary" : "text-muted hover:text-foreground"}`}
            >
              {o.name}
            </button>
          ))}
          <button
            type="button"
            onClick={() => setShowNew(true)}
            className="ml-2 inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
          >
            <Plus size={14} /> {t("New Application")}
          </button>
        </div>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell listKey="jobApplications" listLabel="Applications" columns={columns} {...list} />
      </div>

      {showNew && (
        <NewApplicationModal
          onClose={() => setShowNew(false)}
          onDone={() => {
            setShowNew(false);
            refresh();
          }}
        />
      )}
      {stageFor && (
        <StageModal
          application={stageFor}
          onClose={() => setStageFor(null)}
          onDone={() => {
            setStageFor(null);
            refresh();
          }}
        />
      )}
      {historyFor && <HistoryModal applicationId={historyFor} onClose={() => setHistoryFor(null)} />}
      {interviewsFor && (
        <InterviewsModal
          applicationId={interviewsFor.id!}
          requisitionId={interviewsFor.requisitionId}
          applicationStage={interviewsFor.stage}
          candidateName={`${interviewsFor.candidateName} → ${interviewsFor.requisitionTitle}`}
          readOnly={TERMINAL.includes(interviewsFor.stage ?? "")}
          onClose={() => {
            setInterviewsFor(null);
            refresh();
          }}
        />
      )}
      {offersFor && (
        <OfferModal
          applicationId={offersFor.id!}
          candidateName={`${offersFor.candidateName} → ${offersFor.requisitionTitle}`}
          canCreate={["Selected", "OfferPending"].includes(offersFor.stage ?? "")}
          onClose={() => {
            setOffersFor(null);
            refresh();
          }}
        />
      )}
      {rankingFor && <RankingModal requisitionId={rankingFor} onClose={() => setRankingFor(null)} />}
      {scoreFor && (
        <ScoreModal
          applicationId={scoreFor}
          onClose={() => setScoreFor(null)}
          onDone={() => {
            setScoreFor(null);
            queryClient.invalidateQueries({ queryKey: ["jobApplication", scoreFor] });
            refresh();
          }}
        />
      )}
    </div>
  );
}

export default JobApplications;
