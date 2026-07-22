"use client";
import { useEffect, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { CalendarPlus, CheckCircle2, XCircle, UserX2, ClipboardCheck, Trash2, Users } from "lucide-react";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import getAllEmployee from "@/services/admin/employee/getAll";
import {
  getInterviews,
  getInterviewConsolidated,
  saveInterview,
  setInterviewStatus,
  submitInterviewFeedback,
  deleteInterview,
  getJobApplication,
  getJobRequisition,
  adoptInterviewScores,
} from "@/services/admin/recruitment";
import type { InterviewModel, InterviewPanelistModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import { interviewFormatOptions } from "@/constants/orgStructure";

const lookupParam = { ...parameterInitialData, take: 200 };
const inputCls = "h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground";
const fmtWhen = (v?: string) => (v ? new Date(v).toLocaleString([], { dateStyle: "medium", timeStyle: "short" }) : "—");

const STATUS_TONE: Record<string, string> = {
  Scheduled: "bg-info/15 text-info",
  Completed: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
  NoShow: "bg-error/15 text-error",
};

interface PanelistDraft {
  employeeId?: string;
  panelistName: string;
  isLead: boolean;
}

/**
 * Schedule a new round (or edit a pending one) with its panel (HC101/HC104). The panel
 * PRE-FILLS from the evaluators already assigned on the vacancy's Interview-level (and global)
 * criteria — the interviewers were defined once, on the criteria; nobody re-enters them.
 */
function ScheduleForm({
  applicationId,
  requisitionId,
  editing,
  onClose,
  onDone,
}: {
  applicationId: string;
  requisitionId?: string;
  editing: InterviewModel | null;
  onClose: () => void;
  onDone: () => void;
}) {
  const { t } = useTranslation();
  const [start, setStart] = useState(editing?.scheduledStart?.slice(0, 16) ?? "");
  const [end, setEnd] = useState(editing?.scheduledEnd?.slice(0, 16) ?? "");
  const [format, setFormat] = useState(editing?.format ?? "InPerson");
  const [location, setLocation] = useState(editing?.location ?? "");
  const [meetingLink, setMeetingLink] = useState(editing?.meetingLink ?? "");
  const [notes, setNotes] = useState(editing?.notes ?? "");
  const [panel, setPanel] = useState<PanelistDraft[]>(
    (editing?.panelists ?? []).map((p) => ({
      employeeId: p.employeeId || undefined,
      panelistName: p.panelistName ?? "",
      isLead: p.isLead === true,
    })),
  );
  const [prefilled, setPrefilled] = useState(false);
  const [draftKind, setDraftKind] = useState("Employee");
  const [draftEmployee, setDraftEmployee] = useState("");
  const [draftName, setDraftName] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: employees } = useQuery({
    queryKey: ["employees", lookupParam],
    queryFn: () => getAllEmployee(lookupParam),
  });
  const { data: requisition } = useQuery({
    queryKey: ["jobRequisition", requisitionId],
    queryFn: () => getJobRequisition(requisitionId!),
    enabled: !!requisitionId && !editing,
  });

  // Inherit the panel from the criteria evaluators (Interview-level + global) — defined once
  // on the vacancy, reused here. Editable afterwards; never re-typed.
  useEffect(() => {
    if (editing || prefilled || !requisition) return;
    const inherited: PanelistDraft[] = [];
    for (const c of requisition.screeningCriteria ?? []) {
      if (c.appliesAtStage && c.appliesAtStage !== "Interview") continue;
      for (const e of c.evaluators ?? []) {
        const duplicate = inherited.some(
          (p) => (e.employeeId && p.employeeId === e.employeeId) || (!e.employeeId && p.panelistName === e.name),
        );
        if (!duplicate && e.name)
          inherited.push({ employeeId: e.employeeId || undefined, panelistName: e.name, isLead: false });
      }
    }
    if (inherited.length > 0) {
      inherited[0].isLead = true;
      setPanel(inherited);
    }
    setPrefilled(true);
  }, [requisition, editing, prefilled]);

  const addPanelist = () => {
    if (draftKind === "Employee") {
      if (!draftEmployee || panel.some((p) => p.employeeId === draftEmployee)) return;
      const label =
        (employees?.data ?? []).find((e) => e.id === draftEmployee)?.fullName ?? draftEmployee;
      setPanel((p) => [...p, { employeeId: draftEmployee, panelistName: label, isLead: p.length === 0 }]);
      setDraftEmployee("");
    } else {
      if (!draftName.trim()) return;
      setPanel((p) => [...p, { panelistName: draftName.trim(), isLead: p.length === 0 }]);
      setDraftName("");
    }
  };

  const confirm = async () => {
    const panelists = panel.filter((p) => p.employeeId || p.panelistName.trim());
    if (!start || !end) return setError(t("Set the interview start and end."));
    if (panelists.length === 0) return setError(t("Add at least one panelist (HC104)."));
    setBusy(true);
    const res = await saveInterview({
      id: editing?.id,
      applicationId,
      scheduledStart: start,
      scheduledEnd: end,
      format,
      location: location || undefined,
      meetingLink: meetingLink || undefined,
      notes: notes || undefined,
      panelists: panelists.map((p) => ({
        employeeId: p.employeeId,
        panelistName: p.panelistName || undefined,
        isLead: p.isLead,
      })),
    });
    setBusy(false);
    if (!res.ok) return setError(res.message);
    onDone();
  };

  return (
    <div className="space-y-2 rounded-lg border border-primary/30 bg-primary/5 p-3">
      <h4 className="text-sm font-semibold text-foreground">
        {editing ? t("Reschedule Round {{n}}", { n: editing.round }) : t("Schedule Interview")}
      </h4>
      <div className="grid grid-cols-2 gap-2">
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
            {t("Start")} <span className="text-error">*</span>
          </label>
          <input type="datetime-local" value={start} onChange={(e) => setStart(e.target.value)} className={inputCls} />
        </div>
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
            {t("End")} <span className="text-error">*</span>
          </label>
          <input type="datetime-local" value={end} onChange={(e) => setEnd(e.target.value)} className={inputCls} />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-2">
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Format")}</label>
          <select value={format} onChange={(e) => setFormat(e.target.value)} className={inputCls}>
            {interviewFormatOptions.map((o) => (
              <option key={o.id} value={o.id}>{o.name}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Location")}</label>
          <input type="text" value={location} onChange={(e) => setLocation(e.target.value)} className={inputCls} />
        </div>
      </div>
      <div>
        <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Meeting Link")}</label>
        <input type="text" value={meetingLink} onChange={(e) => setMeetingLink(e.target.value)} className={inputCls} />
      </div>
      <div>
        <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Notes")}</label>
        <input type="text" value={notes} onChange={(e) => setNotes(e.target.value)} className={inputCls} />
      </div>

      {/* Panel (HC104) — inherited from the criteria evaluators, adjustable */}
      <div>
        <label className="mb-1 block text-xs font-semibold uppercase tracking-wide text-muted">
          {t("Panel")} <span className="text-error">*</span>
          {!editing && panel.length > 0 && (
            <span className="ml-1.5 font-normal normal-case text-muted">
              — {t("inherited from the vacancy's criteria evaluators")}
            </span>
          )}
        </label>
        <div className="mb-1.5 flex min-h-7 flex-wrap items-center gap-1">
          {panel.length === 0 && <span className="text-xs italic text-muted">{t("No panelists yet.")}</span>}
          {panel.map((p, idx) => (
            <span
              key={idx}
              className="inline-flex items-center gap-1 rounded-full border border-border bg-secondary/60 py-0.5 pl-2 pr-1 text-[11px] font-medium text-foreground"
            >
              <Users size={11} className={p.employeeId ? "text-primary" : "text-info"} />
              <span className="max-w-40 truncate">{p.panelistName || t("(unnamed)")}</span>
              <button
                type="button"
                title={p.isLead ? t("Lead interviewer") : t("Make lead")}
                onClick={() => setPanel((all) => all.map((x, i) => ({ ...x, isLead: i === idx })))}
                className={`rounded-full px-1 text-[10px] font-bold ${
                  p.isLead ? "bg-warning/20 text-warning" : "text-muted hover:text-warning"
                }`}
              >
                ★
              </button>
              <button
                type="button"
                aria-label={t("Remove panelist")}
                onClick={() => setPanel((all) => all.filter((_, i) => i !== idx))}
                className="rounded-full p-0.5 text-muted hover:bg-error/15 hover:text-error"
              >
                <Trash2 size={11} />
              </button>
            </span>
          ))}
        </div>
        <div className="flex items-center gap-1">
          <select
            value={draftKind}
            onChange={(e) => setDraftKind(e.target.value)}
            className="h-7 shrink-0 rounded-md border border-border bg-background px-1.5 text-[11px] text-foreground"
          >
            <option value="Employee">{t("Employee")}</option>
            <option value="External">{t("External")}</option>
          </select>
          {draftKind === "Employee" ? (
            <select
              value={draftEmployee}
              onChange={(e) => setDraftEmployee(e.target.value)}
              className="h-7 w-full min-w-0 rounded-md border border-border bg-background px-1.5 text-[11px] text-foreground"
            >
              <option value="">{t("Select employee…")}</option>
              {(employees?.data ?? []).map((e) => (
                <option key={e.id} value={e.id}>{e.fullName ?? e.employeeNumber}</option>
              ))}
            </select>
          ) : (
            <input
              type="text"
              value={draftName}
              onChange={(e) => setDraftName(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && (e.preventDefault(), addPanelist())}
              placeholder={t("External panelist's name…")}
              className="h-7 w-full min-w-0 rounded-md border border-border bg-background px-1.5 text-[11px] text-foreground"
            />
          )}
          <button
            type="button"
            onClick={addPanelist}
            className="inline-flex h-7 shrink-0 items-center rounded-md border border-primary/40 bg-primary/10 px-2 text-[11px] font-semibold text-primary hover:bg-primary/20"
          >
            + {t("Add")}
          </button>
        </div>
      </div>

      {error && <p className="text-xs text-error">{error}</p>}
      <div className="flex justify-end gap-2 pt-1">
        <button
          type="button"
          onClick={onClose}
          className="rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:bg-secondary"
        >
          {t("Cancel")}
        </button>
        <button
          type="button"
          disabled={busy}
          onClick={confirm}
          className="rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent disabled:opacity-50"
        >
          {editing ? t("Save Changes") : t("Schedule")}
        </button>
      </div>
    </div>
  );
}

/** One panelist's per-criterion score sheet (HC106) — criteria come from the requisition. */
function FeedbackForm({
  applicationId,
  panelist,
  onClose,
  onDone,
}: {
  applicationId: string;
  panelist: InterviewPanelistModel;
  onClose: () => void;
  onDone: () => void;
}) {
  const { t } = useTranslation();
  const [entries, setEntries] = useState<Record<string, { score: string; comments: string }>>({});
  const [overall, setOverall] = useState({
    score: String(panelist.feedback?.find((f) => !f.criterionId)?.score ?? ""),
    comments: panelist.feedback?.find((f) => !f.criterionId)?.comments ?? "",
  });
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // The requisition criteria arrive via the application's score sheet definition. Only the
  // criteria scoped to the Interview level (or to all steps) are scored here; their weights
  // are INHERITED from the requisition — panelists never re-enter them.
  const { data: application } = useQuery({
    queryKey: ["jobApplication", applicationId],
    queryFn: () => getJobApplication(applicationId),
  });
  const criteria = (application?.criterionScores ?? []).filter(
    (c) => !c.appliesAtStage || c.appliesAtStage === "Interview",
  );

  const get = (id: string) =>
    entries[id] ?? {
      score: String(panelist.feedback?.find((f) => f.criterionId === id)?.score ?? ""),
      comments: panelist.feedback?.find((f) => f.criterionId === id)?.comments ?? "",
    };
  const set = (id: string, patch: Partial<{ score: string; comments: string }>) =>
    setEntries((p) => ({ ...p, [id]: { ...get(id), ...patch } }));

  const confirm = async () => {
    const list = criteria
      .map((c) => ({ c, raw: get(c.criterionId) }))
      .filter((x) => x.raw.score !== "" && !Number.isNaN(Number(x.raw.score)))
      .map((x) => ({
        criterionId: x.c.criterionId,
        criterionName: x.c.criterionName,
        score: Number(x.raw.score),
        comments: x.raw.comments || undefined,
      })) as { criterionId?: string; criterionName?: string; score: number; comments?: string }[];
    if (overall.score !== "" && !Number.isNaN(Number(overall.score)))
      list.push({ criterionName: "Overall", score: Number(overall.score), comments: overall.comments || undefined });
    if (list.length === 0) return setError(t("Enter at least one score (0–100)."));

    setBusy(true);
    const res = await submitInterviewFeedback({ panelistId: panelist.id!, entries: list });
    setBusy(false);
    if (!res.ok) return setError(res.message);
    onDone();
  };

  return (
    <div className="space-y-2 rounded-lg border border-warning/40 bg-warning/5 p-3">
      <h4 className="text-sm font-semibold text-foreground">
        {t("Feedback")} — {panelist.panelistName}
      </h4>
      {criteria.map((c) => (
        <div key={c.criterionId} className="flex items-center gap-2">
          <span className="min-w-0 flex-1 truncate text-sm text-foreground">
            {c.criterionName}
            <span className="ml-1.5 rounded bg-primary/10 px-1.5 py-0.5 text-[10px] font-bold text-primary">
              {c.weight}%
            </span>
          </span>
          <input
            type="text"
            value={get(c.criterionId).score}
            onChange={(e) => set(c.criterionId, { score: e.target.value })}
            placeholder="0–100"
            className="h-8 w-20 rounded-md border border-border bg-background px-2 text-right text-sm text-foreground"
          />
          <input
            type="text"
            value={get(c.criterionId).comments}
            onChange={(e) => set(c.criterionId, { comments: e.target.value })}
            placeholder={t("Comments…")}
            className="h-8 w-40 rounded-md border border-border bg-background px-2 text-xs text-foreground"
          />
        </div>
      ))}
      <div className="flex items-center gap-2 border-t border-border pt-2">
        <span className="min-w-0 flex-1 text-sm font-medium text-foreground">{t("Overall impression")}</span>
        <input
          type="text"
          value={overall.score}
          onChange={(e) => setOverall((p) => ({ ...p, score: e.target.value }))}
          placeholder="0–100"
          className="h-8 w-20 rounded-md border border-border bg-background px-2 text-right text-sm text-foreground"
        />
        <input
          type="text"
          value={overall.comments}
          onChange={(e) => setOverall((p) => ({ ...p, comments: e.target.value }))}
          placeholder={t("Comments…")}
          className="h-8 w-40 rounded-md border border-border bg-background px-2 text-xs text-foreground"
        />
      </div>
      {error && <p className="text-xs text-error">{error}</p>}
      <div className="flex justify-end gap-2">
        <button
          type="button"
          onClick={onClose}
          className="rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:bg-secondary"
        >
          {t("Cancel")}
        </button>
        <button
          type="button"
          disabled={busy}
          onClick={confirm}
          className="rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent disabled:opacity-50"
        >
          {t("Save Feedback")}
        </button>
      </div>
    </div>
  );
}

/** Interview rounds, panels, feedback and the consolidated report (HC101–HC109). */
function InterviewsModal({
  applicationId,
  requisitionId,
  applicationStage,
  candidateName,
  readOnly = false,
  onClose,
}: {
  applicationId: string;
  /** Source of the criteria evaluators the panel inherits. */
  requisitionId?: string;
  /** Scheduling is the Interview LEVEL's activity — gated to that stage. */
  applicationStage?: string;
  candidateName?: string;
  /** Finished applications keep their interview record viewable, but nothing changes. */
  readOnly?: boolean;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showSchedule, setShowSchedule] = useState(false);
  const [editing, setEditing] = useState<InterviewModel | null>(null);
  const [feedbackFor, setFeedbackFor] = useState<InterviewPanelistModel | null>(null);
  const [showConsolidated, setShowConsolidated] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const queryKey = ["interviews", applicationId];
  const { data: interviews, isLoading, refetch } = useQuery({
    queryKey,
    queryFn: () => getInterviews(applicationId),
  });
  const { data: consolidated } = useQuery({
    queryKey: ["interviewConsolidated", applicationId],
    queryFn: () => getInterviewConsolidated(applicationId),
    enabled: showConsolidated,
  });

  const refresh = async () => {
    await refetch();
    queryClient.invalidateQueries({ queryKey: ["interviewConsolidated", applicationId] });
    queryClient.invalidateQueries({ queryKey: ["jobApplications"] });
  };

  const act = async (id: string, action: "Complete" | "Cancel" | "NoShow") => {
    setActionError(null);
    const res = await setInterviewStatus({ id, action });
    if (!res.ok) return setActionError(res.message);
    await refresh();
  };

  const remove = async (id: string) => {
    setActionError(null);
    const res: any = await deleteInterview(id);
    if (res?.status === "error") return setActionError(res.message);
    await refresh();
  };

  return (
    <Modal
      visible
      size="xl"
      title={t("Interviews")}
      description={candidateName}
      onClose={onClose}
      footer={
        <>
          <button
            type="button"
            onClick={() => setShowConsolidated((v) => !v)}
            className="mr-auto rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:border-primary hover:text-primary"
          >
            {showConsolidated ? t("Hide Consolidated Report") : t("Consolidated Report (HC109)")}
          </button>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Close")}
          </button>
        </>
      }
    >
      <div className="space-y-3">
        {isLoading && <Loading />}
        {actionError && (
          <p className="rounded-md border border-error/30 bg-error/10 px-3 py-2 text-xs text-error">{actionError}</p>
        )}

        {/* Consolidated report (HC109) */}
        {showConsolidated && consolidated && (
          <div className="rounded-lg border border-success/40 bg-success/5 p-3">
            <div className="mb-2 flex flex-wrap items-center gap-3 text-sm">
              <span className="font-semibold text-foreground">{t("Consolidated Evaluation")}</span>
              <span className="text-muted">
                {consolidated.rounds} {t("round(s)")} · {consolidated.scoredPanelists}/{consolidated.panelistCount}{" "}
                {t("panelists scored")}
              </span>
              <span className="ml-auto flex items-center gap-3">
                {consolidated.weightedAverage != null && (
                  <span
                    className="text-lg font-bold tabular-nums text-success"
                    title={t("Weighted by the criteria weights inherited from the requisition")}
                  >
                    {consolidated.weightedAverage}
                    <span className="ml-1 text-[10px] font-semibold uppercase text-muted">{t("weighted")}</span>
                  </span>
                )}
                <span className="text-sm font-semibold tabular-nums text-muted">
                  {consolidated.overallAverage ?? "—"}
                  <span className="ml-1 text-[10px] font-normal uppercase">{t("plain avg")}</span>
                </span>
                {/* One click carries the panel's per-criterion averages into the ranking —
                    no retyping numbers into the score sheet. */}
                {!readOnly && consolidated.criteria.some((c) => c.criterionId) && (
                  <button
                    type="button"
                    onClick={async () => {
                      setActionError(null);
                      const res = await adoptInterviewScores(applicationId);
                      if (!res.ok) return setActionError(res.message);
                      await refresh();
                      queryClient.invalidateQueries({ queryKey: ["applicationRanking"] });
                      queryClient.invalidateQueries({ queryKey: ["jobApplication", applicationId] });
                    }}
                    title={t("Copy the per-criterion averages into the application's score sheet (drives the ranking)")}
                    className="rounded-md bg-success px-2.5 py-1 text-xs font-semibold text-on-accent hover:opacity-90"
                  >
                    {t("Adopt into Ranking")}
                  </button>
                )}
              </span>
            </div>
            {consolidated.criteria.map((c) => (
              <div key={`${c.criterionId ?? c.criterionName}`} className="flex items-center gap-2 py-0.5 text-sm">
                <span className="min-w-0 flex-1 truncate text-foreground">
                  {c.criterionName}
                  {c.weight > 0 && (
                    <span className="ml-1.5 rounded bg-primary/10 px-1.5 py-0.5 text-[10px] font-bold text-primary">
                      {c.weight}%
                    </span>
                  )}
                </span>
                <span className="text-xs text-muted">({c.scores})</span>
                <div className="h-1.5 w-32 overflow-hidden rounded bg-border">
                  <div className="h-full bg-success" style={{ width: `${Math.min(100, c.average)}%` }} />
                </div>
                <span className="w-12 text-right tabular-nums text-foreground">{c.average}</span>
              </div>
            ))}
          </div>
        )}

        {/* Rounds */}
        {!isLoading && (interviews ?? []).length === 0 && (
          <p className="py-4 text-center text-sm text-muted">{t("No interviews scheduled yet.")}</p>
        )}
        {(interviews ?? []).map((i) => (
          <div key={i.id} className="rounded-lg border border-border p-3">
            <div className="mb-1 flex flex-wrap items-center gap-2">
              <span className="text-sm font-semibold text-foreground">
                {t("Round")} {i.round}
              </span>
              <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[i.status ?? ""] ?? ""}`}>
                {t(i.status ?? "")}
              </span>
              <span className="text-xs text-muted">
                {fmtWhen(i.scheduledStart)} → {fmtWhen(i.scheduledEnd)} ·{" "}
                {interviewFormatOptions.find((o) => o.id === i.format)?.name ?? i.format}
                {i.location ? ` · ${i.location}` : ""}
              </span>
              {i.averageScore != null && (
                <span className="rounded bg-success/15 px-2 py-0.5 text-xs font-semibold text-success">
                  {t("Avg")} {i.averageScore}
                </span>
              )}
              {i.status === "Scheduled" && !readOnly && (
                <span className="ml-auto inline-flex items-center gap-1">
                  {applicationStage === "Interview" && (
                    <button
                      type="button"
                      title={t("Reschedule / edit panel")}
                      onClick={() => {
                        setEditing(i);
                        setShowSchedule(true);
                      }}
                      className="rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
                    >
                      <CalendarPlus size={13} />
                    </button>
                  )}
                  <button
                    type="button"
                    title={t("Complete")}
                    onClick={() => i.id && act(i.id, "Complete")}
                    className="rounded border border-border px-2 py-1 text-xs text-success hover:border-success"
                  >
                    <CheckCircle2 size={13} />
                  </button>
                  <button
                    type="button"
                    title={t("No-show")}
                    onClick={() => i.id && act(i.id, "NoShow")}
                    className="rounded border border-border px-2 py-1 text-xs text-warning hover:border-warning"
                  >
                    <UserX2 size={13} />
                  </button>
                  <button
                    type="button"
                    title={t("Cancel round")}
                    onClick={() => i.id && act(i.id, "Cancel")}
                    className="rounded border border-border px-2 py-1 text-xs text-muted hover:border-error hover:text-error"
                  >
                    <XCircle size={13} />
                  </button>
                  <button
                    type="button"
                    title={t("Delete")}
                    onClick={() => i.id && remove(i.id)}
                    className="rounded border border-border px-2 py-1 text-xs text-muted hover:border-error hover:text-error"
                  >
                    <Trash2 size={13} />
                  </button>
                </span>
              )}
            </div>
            {i.notes && <p className="mb-1 text-xs text-muted">{i.notes}</p>}

            {/* Panel */}
            <div className="space-y-1">
              {(i.panelists ?? []).map((p) => (
                <div key={p.id} className="flex flex-wrap items-center gap-2 rounded-md border border-border/60 px-2.5 py-1.5 text-sm">
                  <Users size={13} className="shrink-0 text-muted" />
                  <span className="font-medium text-foreground">{p.panelistName}</span>
                  {p.isLead && (
                    <span className="rounded bg-primary/10 px-1.5 py-0.5 text-[11px] font-semibold text-primary">
                      {t("Lead")}
                    </span>
                  )}
                  <span className="text-xs text-muted">{t(p.attendance ?? "")}</span>
                  {p.averageScore != null && (
                    <span className="rounded bg-success/15 px-1.5 py-0.5 text-[11px] font-semibold text-success">
                      {p.averageScore}
                    </span>
                  )}
                  {(p.feedback ?? []).length > 0 && (
                    <span className="text-[11px] text-muted">
                      {(p.feedback ?? []).length} {t("score(s)")}
                    </span>
                  )}
                  {i.status !== "Cancelled" && !readOnly && (
                    <button
                      type="button"
                      onClick={() => setFeedbackFor(p)}
                      className="ml-auto inline-flex items-center gap-1 rounded border border-border px-2 py-0.5 text-xs text-foreground hover:border-warning hover:text-warning"
                    >
                      <ClipboardCheck size={12} /> {t("Score")}
                    </button>
                  )}
                </div>
              ))}
            </div>

            {/* Feedback sheet inline under the round it belongs to */}
            {feedbackFor && (i.panelists ?? []).some((p) => p.id === feedbackFor.id) && (
              <div className="mt-2">
                <FeedbackForm
                  applicationId={applicationId}
                  panelist={feedbackFor}
                  onClose={() => setFeedbackFor(null)}
                  onDone={async () => {
                    setFeedbackFor(null);
                    await refresh();
                  }}
                />
              </div>
            )}
          </div>
        ))}

        {/* Schedule / reschedule — the Interview LEVEL's activity; other levels view only */}
        {readOnly ? (
          <p className="text-xs italic text-muted">
            {t("This application is final — the interview record is view-only.")}
          </p>
        ) : applicationStage !== "Interview" ? (
          <p className="text-xs italic text-muted">
            {t("Interviews are scheduled at the Interview level — move the application there first (current: {{stage}}).", {
              stage: t(applicationStage ?? ""),
            })}
          </p>
        ) : showSchedule ? (
          <ScheduleForm
            applicationId={applicationId}
            requisitionId={requisitionId}
            editing={editing}
            onClose={() => {
              setShowSchedule(false);
              setEditing(null);
            }}
            onDone={async () => {
              setShowSchedule(false);
              setEditing(null);
              await refresh();
            }}
          />
        ) : (
          <button
            type="button"
            onClick={() => setShowSchedule(true)}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
          >
            <CalendarPlus size={14} /> {t("Schedule Interview")}
          </button>
        )}
      </div>
    </Modal>
  );
}

export default InterviewsModal;
