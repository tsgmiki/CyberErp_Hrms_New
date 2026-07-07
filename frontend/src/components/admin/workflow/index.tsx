"use client";
import { useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { GitPullRequestArrow, Check, X, History } from "lucide-react";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import { EntityListShell, useEntityList } from "@/template";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { WorkflowInstanceModel } from "@/models";
import {
  getAllWorkflows,
  getWorkflowActions,
  approveWorkflow,
  rejectWorkflow,
} from "@/services/admin/workflow";
import { workflowEntityTypeLabel, workflowStatusOptions } from "@/constants/orgStructure";

const STATUS_TONE: Record<string, string> = {
  Running: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
  Cancelled: "bg-muted/30 text-muted",
};

const fmtDateTime = (v?: string) => (v ? new Date(v).toLocaleString() : "—");

/** Comment dialog for an approve / reject decision. */
function DecisionModal({
  instance,
  verb,
  onClose,
  onDone,
}: {
  instance: WorkflowInstanceModel;
  verb: "approve" | "reject";
  onClose: () => void;
  onDone: (message: string | null) => void;
}) {
  const { t } = useTranslation();
  const [comment, setComment] = useState("");
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    if (!instance.id) return;
    setBusy(true);
    const res = await (verb === "approve" ? approveWorkflow : rejectWorkflow)(instance.id, comment);
    setBusy(false);
    onDone(res.ok ? null : res.message);
  };

  return (
    <Modal
      visible
      size="md"
      title={verb === "approve" ? t("Approve Step") : t("Reject Workflow")}
      description={`${instance.summary} — ${t("Step")} ${instance.currentStepOrder}/${instance.totalSteps}: ${instance.currentStepName}`}
      onClose={onClose}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Close")}
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={submit}
            className={`rounded-md px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50 ${
              verb === "approve" ? "bg-success" : "bg-error"
            }`}
          >
            {verb === "approve" ? t("Approve") : t("Reject")}
          </button>
        </>
      }
    >
      <label className="mb-1 block text-sm font-medium text-foreground" htmlFor="wf-comment">
        {t("Comment (optional)")}
      </label>
      <textarea
        id="wf-comment"
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        rows={3}
        className="w-full rounded-lg border border-border bg-background px-3 py-2 text-sm text-foreground"
        placeholder={t("Add a note for the audit trail")}
      />
    </Modal>
  );
}

/** Step-decision history of one instance. */
function HistoryModal({ instance, onClose }: { instance: WorkflowInstanceModel; onClose: () => void }) {
  const { t } = useTranslation();
  const { data: actions, isLoading } = useQuery({
    queryKey: ["workflowActions", instance.id],
    queryFn: () => getWorkflowActions(instance.id!),
    enabled: !!instance.id,
  });

  return (
    <Modal visible size="lg" title={t("Workflow History")} description={instance.summary} onClose={onClose}>
      {isLoading && <Loading />}
      <ol className="space-y-2">
        {(actions ?? []).map((a, i) => (
          <li key={i} className="flex items-start gap-3 rounded-md border border-border/60 px-3 py-2">
            <span
              className={`mt-0.5 shrink-0 rounded px-1.5 py-0.5 text-[11px] font-semibold ${STATUS_TONE[a.action === "Submitted" ? "Running" : a.action] ?? "bg-muted/30 text-muted"}`}
            >
              {t(a.action)}
            </span>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-foreground">{a.stepName}</p>
              {a.comment && <p className="text-xs text-muted">{a.comment}</p>}
            </div>
            <div className="shrink-0 text-right text-xs text-muted">
              <p>{a.actedBy || "—"}</p>
              <p>{fmtDateTime(a.actedAt)}</p>
            </div>
          </li>
        ))}
      </ol>
    </Modal>
  );
}

/** Workflow tracking: every approval run across HR processes, with inline decisions. */
function WorkflowTracking() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [decision, setDecision] = useState<{ instance: WorkflowInstanceModel; verb: "approve" | "reject" } | null>(null);
  const [history, setHistory] = useState<WorkflowInstanceModel | null>(null);
  const [error, setError] = useState<string | null>(null);

  const list = useEntityList({
    queryKey: "workflows",
    fetchPage: getAllWorkflows,
  });

  const statusFilter = list.param.status || "";
  const setStatus = (status: string) =>
    list.setParam((p) => ({ ...p, status: status || undefined, skip: 0 }) as never);

  const columns = useMemo(
    () =>
      [
        {
          name: "summary",
          label: "Request",
          sort: true,
          render: (text: string, r: WorkflowInstanceModel) => (
            <span className="block">
              <span className="block font-semibold">{text}</span>
              <span className="block text-xs text-muted">{workflowEntityTypeLabel(r.entityType)}</span>
            </span>
          ),
        },
        {
          name: "currentStepName",
          label: "Progress",
          render: (_v: unknown, r: WorkflowInstanceModel) =>
            r.status === "Running" ? (
              <span className="block">
                <span className="block text-xs font-medium text-foreground">
                  {t("Step")} {r.currentStepOrder}/{r.totalSteps} — {r.currentStepName}
                </span>
                <span className="mt-1 block h-1.5 w-28 overflow-hidden rounded-full bg-secondary">
                  <span
                    className="block h-full rounded-full bg-primary"
                    style={{ width: `${(((r.currentStepOrder ?? 1) - 1) / (r.totalSteps || 1)) * 100}%` }}
                  />
                </span>
                {(r.currentStepApprovers?.length ?? 0) > 0 && (
                  <span className="mt-0.5 block text-[11px] text-muted">
                    {t("Approvers")}: {r.currentStepApprovers!.join(", ")}
                  </span>
                )}
              </span>
            ) : (
              <span className="text-xs text-muted">{fmtDateTime(r.completedAt)}</span>
            ),
        },
        {
          name: "requestedBy",
          label: "Requested By",
          render: (v: string, r: WorkflowInstanceModel) => (
            <span className="block">
              <span className="block text-foreground">{v || "—"}</span>
              <span className="block text-xs text-muted">{fmtDateTime(r.requestedAt)}</span>
            </span>
          ),
        },
        {
          name: "status",
          label: "Status",
          render: (v: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-muted/30 text-muted"}`}>
              {t(v ?? "")}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_v: unknown, r: WorkflowInstanceModel) => {
            const running = r.status === "Running";
            // Backend-computed step authorization: only listed users / role holders may act.
            const mayAct = running && r.canDecide !== false;
            const gateTitle = running && !mayAct ? t("You are not an approver for this step") : undefined;
            return (
              <span className="inline-flex items-center gap-0.5">
                <button
                  type="button" title={gateTitle ?? t("Approve")} disabled={!mayAct}
                  onClick={() => setDecision({ instance: r, verb: "approve" })}
                  className="rounded p-1 text-success hover:bg-success/10 disabled:cursor-not-allowed disabled:opacity-40"
                ><Check size={16} /></button>
                <button
                  type="button" title={gateTitle ?? t("Reject")} disabled={!mayAct}
                  onClick={() => setDecision({ instance: r, verb: "reject" })}
                  className="rounded p-1 text-error hover:bg-error/10 disabled:cursor-not-allowed disabled:opacity-40"
                ><X size={16} /></button>
                <button
                  type="button" title={t("History")}
                  onClick={() => setHistory(r)}
                  className="rounded p-1 text-primary hover:bg-primary/10"
                ><History size={15} /></button>
              </span>
            );
          },
        },
      ] as DataTableColumnModel[],
    [t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <h1 className="flex items-center gap-2 text-lg font-bold text-foreground">
          <GitPullRequestArrow className="h-5 w-5 text-primary" />
          {t("Workflow Tracking")}
        </h1>
        <div className="flex items-center gap-1">
          {["", ...workflowStatusOptions].map((s) => (
            <button
              key={s || "all"}
              type="button"
              onClick={() => setStatus(s)}
              className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
                statusFilter === s
                  ? "bg-primary text-on-accent"
                  : "border border-border text-foreground hover:border-primary/50"
              }`}
            >
              {s ? t(s) : t("All")}
            </button>
          ))}
        </div>
      </div>

      {error && (
        <div className="mb-2 flex items-center justify-between rounded border border-error/30 bg-error/15 px-3 py-2 text-xs text-error">
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="font-semibold">×</button>
        </div>
      )}

      <div className="min-h-0 flex-1 overflow-auto rounded-lg border border-border bg-card">
        <EntityListShell listKey="workflows" listLabel="Workflows" columns={columns} {...list} />
      </div>

      {decision && (
        <DecisionModal
          instance={decision.instance}
          verb={decision.verb}
          onClose={() => setDecision(null)}
          onDone={(err) => {
            setDecision(null);
            setError(err);
            queryClient.invalidateQueries({ queryKey: ["workflows"] });
            queryClient.invalidateQueries({ queryKey: ["workflowStats"] });
            queryClient.invalidateQueries({ queryKey: ["employees"] });
            queryClient.invalidateQueries({ queryKey: ["positions"] });
          }}
        />
      )}
      {history && <HistoryModal instance={history} onClose={() => setHistory(null)} />}
    </div>
  );
}

export default WorkflowTracking;
