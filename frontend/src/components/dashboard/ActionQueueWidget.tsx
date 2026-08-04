import { memo, useCallback, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, ShieldAlert } from "lucide-react";
import type { MyApprovalItemModel, MyClearanceItemModel } from "@/models";
import type { ProfileChangeRequestModel } from "@/services/admin/employee/profileChangeRequest";
import { resolveProfileChangeRequest } from "@/services/admin/employee/profileChangeRequest";
import { updateClearance } from "@/services/admin/employee/termination";
import { approveWorkflow, rejectWorkflow } from "@/services/admin/workflow";
import { workflowEntityTypeLabel } from "@/constants/orgStructure";
import Modal from "@/components/common/modal";
import { EmptyRow, type ApprovalVerb, type ClearanceDecision } from "./shared";
import { TabbedCardSkeleton } from "./DashboardSkeletons";
import { useMyApprovals, useMyClearances, useProfileChangeRequests } from "./useActionQueues";

type QueueTab = "approvals" | "clearance" | "changeRequests";

/** One row of the approver's clearance queue: identity left, Clear/Block right. */
function ClearanceQueueRow({
  item,
  busy,
  onPick,
}: {
  item: MyClearanceItemModel;
  busy: boolean;
  onPick: (item: MyClearanceItemModel, status: ClearanceDecision) => void;
}) {
  const { t } = useTranslation();
  return (
    <div className="flex items-center gap-3 px-4 py-3">
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-semibold text-foreground">
          {item.employeeName}
          <span className="ml-1.5 rounded bg-secondary px-1.5 py-0.5 text-[11px] font-medium text-muted">
            {item.department}
          </span>
        </p>
        <p className="mt-0.5 truncate text-xs text-muted">
          {item.employeeNumber} — {item.description}
          {item.lastWorkingDate
            ? ` · ${t("Last day")} ${new Date(item.lastWorkingDate).toLocaleDateString()}`
            : ""}
        </p>
      </div>
      <div className="flex shrink-0 items-center gap-2">
        <button
          type="button"
          disabled={busy}
          onClick={() => onPick(item, "Cleared")}
          className="inline-flex items-center gap-1.5 rounded-lg border border-success/30 bg-success/10 px-3.5 py-2 text-[13px] font-semibold text-success transition-colors hover:bg-success/20 disabled:cursor-not-allowed disabled:opacity-40"
        >
          <CheckCircle2 size={17} /> {t("Clear")}
        </button>
        <button
          type="button"
          disabled={busy}
          onClick={() => onPick(item, "Blocked")}
          className="inline-flex items-center gap-1.5 rounded-lg border border-error/30 bg-error/10 px-3.5 py-2 text-[13px] font-semibold text-error transition-colors hover:bg-error/20 disabled:cursor-not-allowed disabled:opacity-40"
        >
          <ShieldAlert size={17} /> {t("Block")}
        </button>
      </div>
    </div>
  );
}

/** One row of the approver's workflow inbox: identity + prominent Approve / Reject actions. */
function ApprovalQueueRow({
  item,
  busy,
  onPick,
}: {
  item: MyApprovalItemModel;
  busy: boolean;
  onPick: (item: MyApprovalItemModel, verb: ApprovalVerb) => void;
}) {
  const { t } = useTranslation();
  return (
    <div className="flex items-center gap-3 px-4 py-3">
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-semibold text-foreground">{item.summary}</p>
        <p className="mt-0.5 truncate text-xs text-muted">
          {workflowEntityTypeLabel(item.entityType)} · {t("Step")} {item.currentStepOrder}/{item.totalSteps} —{" "}
          {item.currentStepName}
          {item.requestedBy ? ` · ${item.requestedBy}` : ""}
        </p>
      </div>
      <div className="flex shrink-0 items-center gap-2">
        {item.entityType === "Appraisal" ? (
          // Appraisals are acted on from the appraisal screen (score / sign / complete), not the generic buttons.
          <Link
            to="/appraisal"
            className="inline-flex items-center gap-1.5 rounded-lg border border-primary/30 bg-primary/10 px-3.5 py-2 text-[13px] font-semibold text-primary transition-colors hover:bg-primary/20"
          >
            <CheckCircle2 size={17} /> {t("Open in Appraisals")}
          </Link>
        ) : (
          <>
            <button
              type="button"
              disabled={busy}
              onClick={() => onPick(item, "approve")}
              className="inline-flex items-center gap-1.5 rounded-lg border border-success/30 bg-success/10 px-3.5 py-2 text-[13px] font-semibold text-success transition-colors hover:bg-success/20 disabled:cursor-not-allowed disabled:opacity-40"
            >
              <CheckCircle2 size={17} /> {t("Approve")}
            </button>
            <button
              type="button"
              disabled={busy}
              onClick={() => onPick(item, "reject")}
              className="inline-flex items-center gap-1.5 rounded-lg border border-error/30 bg-error/10 px-3.5 py-2 text-[13px] font-semibold text-error transition-colors hover:bg-error/20 disabled:cursor-not-allowed disabled:opacity-40"
            >
              <ShieldAlert size={17} /> {t("Reject")}
            </button>
          </>
        )}
      </div>
    </div>
  );
}

function ChangeRequestQueueRow({
  item,
  busy,
  onPick,
}: {
  item: ProfileChangeRequestModel;
  busy: boolean;
  onPick: (item: ProfileChangeRequestModel, verb: ApprovalVerb) => void;
}) {
  const { t } = useTranslation();
  return (
    <div className="flex items-center gap-3 px-4 py-3">
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-semibold text-foreground">
          {item.employeeName || item.employeeNumber} · {t(item.fieldLabel)}
        </p>
        <p className="mt-0.5 truncate text-xs text-muted">
          <span className="text-muted line-through">{item.currentValue || t("(empty)")}</span>
          {" → "}
          <span className="font-medium text-foreground">{item.requestedValue}</span>
          {item.kind === "Structural" ? ` · ${t("HR to apply")}` : ` · ${t("auto-applies")}`}
          {item.reason ? ` · ${item.reason}` : ""}
        </p>
      </div>
      <div className="flex shrink-0 items-center gap-2">
        <button
          type="button"
          disabled={busy}
          onClick={() => onPick(item, "approve")}
          className="inline-flex items-center gap-1.5 rounded-lg border border-success/30 bg-success/10 px-3.5 py-2 text-[13px] font-semibold text-success transition-colors hover:bg-success/20 disabled:cursor-not-allowed disabled:opacity-40"
        >
          <CheckCircle2 size={17} /> {t("Approve")}
        </button>
        <button
          type="button"
          disabled={busy}
          onClick={() => onPick(item, "reject")}
          className="inline-flex items-center gap-1.5 rounded-lg border border-error/30 bg-error/10 px-3.5 py-2 text-[13px] font-semibold text-error transition-colors hover:bg-error/20 disabled:cursor-not-allowed disabled:opacity-40"
        >
          <ShieldAlert size={17} /> {t("Reject")}
        </button>
      </div>
    </div>
  );
}

/**
 * The three "needs MY decision" queues (Approvals / Clearance / Profile Changes) sharing one tabbed
 * card, plus all three decision modals. Every piece of local UI state (active tab, which item is
 * being decided, the comment/note text, busy/error flags) lives HERE — not in the page shell — so
 * typing a rejection reason, or switching tabs, re-renders only this widget. It is itself memo()'d,
 * so the page shell re-rendering (e.g. on window focus) never cascades into it either.
 */
function ActionQueueWidget() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: myApprovals, isLoading: lma } = useMyApprovals();
  const { data: myClearances, isLoading: lmc } = useMyClearances();
  const { data: profileChanges, isLoading: lpc } = useProfileChangeRequests();

  const isWorkflowApprover = myApprovals?.isApprover === true;
  const approvalItems = myApprovals?.items ?? [];
  const isApprover = myClearances?.isApprover === true;
  const clearanceItems = myClearances?.items ?? [];
  const isChangeApprover = profileChanges?.isApprover === true;
  const changeItems = profileChanges?.items ?? [];

  const tabs: { key: QueueTab; label: string; count: number }[] = [
    ...(isWorkflowApprover ? [{ key: "approvals" as const, label: t("Approvals"), count: approvalItems.length }] : []),
    ...(isApprover ? [{ key: "clearance" as const, label: t("Clearance"), count: clearanceItems.length }] : []),
    ...(isChangeApprover
      ? [{ key: "changeRequests" as const, label: t("Profile Change Requests"), count: changeItems.length }]
      : []),
  ];
  const [activeTab, setActiveTab] = useState<QueueTab | null>(null);
  const currentTab = activeTab && tabs.some((x) => x.key === activeTab) ? activeTab : (tabs[0]?.key ?? null);

  // ---- Clearance decision ----
  const [clearanceBusy, setClearanceBusy] = useState(false);
  const [clearanceError, setClearanceError] = useState<string | null>(null);
  const [pendingDecision, setPendingDecision] = useState<{ item: MyClearanceItemModel; status: ClearanceDecision } | null>(null);
  const [pendingNote, setPendingNote] = useState("");

  const pickDecision = useCallback((item: MyClearanceItemModel, status: ClearanceDecision) => {
    setClearanceError(null);
    setPendingNote(item.note ?? "");
    setPendingDecision({ item, status });
  }, []);

  const confirmDecision = useCallback(async () => {
    if (!pendingDecision) return;
    if (pendingDecision.status === "Blocked" && !pendingNote.trim()) {
      setClearanceError(t("A reason is required to block a clearance."));
      return;
    }
    setClearanceBusy(true);
    const res = await updateClearance(pendingDecision.item.clearanceId, pendingDecision.status, pendingNote);
    setClearanceBusy(false);
    if (!res.ok) {
      setClearanceError(res.message);
      return;
    }
    queryClient.invalidateQueries({ queryKey: ["myClearances"] });
    queryClient.invalidateQueries({ queryKey: ["employeeTerminations"] });
    setPendingDecision(null);
    setPendingNote("");
  }, [pendingDecision, pendingNote, queryClient, t]);

  // ---- Workflow approval decision ----
  const [approvalDecision, setApprovalDecision] = useState<{ item: MyApprovalItemModel; verb: ApprovalVerb } | null>(null);
  const [approvalComment, setApprovalComment] = useState("");
  const [approvalBusy, setApprovalBusy] = useState(false);
  const [approvalError, setApprovalError] = useState<string | null>(null);

  const pickApproval = useCallback((item: MyApprovalItemModel, verb: ApprovalVerb) => {
    setApprovalError(null);
    setApprovalComment("");
    setApprovalDecision({ item, verb });
  }, []);

  const confirmApproval = useCallback(async () => {
    if (!approvalDecision) return;
    setApprovalBusy(true);
    const res = await (approvalDecision.verb === "approve" ? approveWorkflow : rejectWorkflow)(
      approvalDecision.item.instanceId,
      approvalComment,
    );
    setApprovalBusy(false);
    if (!res.ok) {
      setApprovalError(res.message);
      return;
    }
    queryClient.invalidateQueries({ queryKey: ["myApprovals"] });
    queryClient.invalidateQueries({ queryKey: ["workflows"] });
    queryClient.invalidateQueries({ queryKey: ["dashboardSummary"] });
    queryClient.invalidateQueries({ queryKey: ["workforcePlans"], refetchType: "none" });
    queryClient.invalidateQueries({ queryKey: ["employees"], refetchType: "none" });
    setApprovalDecision(null);
    setApprovalComment("");
  }, [approvalDecision, approvalComment, queryClient]);

  // ---- Profile-change decision ----
  const [changeDecision, setChangeDecision] = useState<{ item: ProfileChangeRequestModel; verb: ApprovalVerb } | null>(null);
  const [changeComment, setChangeComment] = useState("");
  const [changeBusy, setChangeBusy] = useState(false);
  const [changeError, setChangeError] = useState<string | null>(null);

  const pickChange = useCallback((item: ProfileChangeRequestModel, verb: ApprovalVerb) => {
    setChangeError(null);
    setChangeComment("");
    setChangeDecision({ item, verb });
  }, []);

  const confirmChange = useCallback(async () => {
    if (!changeDecision) return;
    setChangeBusy(true);
    const res = await resolveProfileChangeRequest(
      changeDecision.item.id,
      changeDecision.verb === "approve" ? "Approve" : "Reject",
      changeComment,
    );
    setChangeBusy(false);
    if (!res.ok) {
      setChangeError(res.message);
      return;
    }
    queryClient.invalidateQueries({ queryKey: ["profileChangeRequests"] });
    queryClient.invalidateQueries({ queryKey: ["employees"], refetchType: "none" });
    setChangeDecision(null);
    setChangeComment("");
  }, [changeDecision, changeComment, queryClient]);

  // Which of the three approver-flag queries is still resolving: while ANY of them is in flight, we
  // don't yet know whether this card will end up empty — show its skeleton rather than nothing, so a
  // late-arriving tab can't pop the whole card into existence after the page has already settled.
  if (lma || lmc || lpc) return <TabbedCardSkeleton tabs={3} />;
  // All three settled and none granted this user a queue — the card is genuinely not needed.
  if (tabs.length === 0) return null;

  return (
    <>
      <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
        <div className="flex items-center gap-1 border-b border-border px-2 pt-1.5">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              type="button"
              onClick={() => setActiveTab(tab.key)}
              className={`relative flex items-center gap-2 rounded-t-lg px-3 py-2 text-[13px] font-medium transition-colors ${
                currentTab === tab.key ? "text-primary" : "text-muted hover:text-foreground"
              }`}
            >
              {tab.label}
              <span
                className={`rounded-full px-1.5 py-0.5 text-[11px] font-semibold tabular-nums ${
                  currentTab === tab.key ? "bg-primary/10 text-primary" : "bg-muted/25 text-muted"
                }`}
              >
                {tab.count}
              </span>
              {currentTab === tab.key && <span className="absolute inset-x-2 -bottom-px h-0.5 rounded-full bg-primary" />}
            </button>
          ))}
        </div>

        {currentTab === "approvals" && (
          <div className="divide-y divide-border/60">
            {lma && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
            {!lma && approvalItems.length === 0 && (
              <EmptyRow text={t("No approvals awaiting your decision.", "No approvals awaiting your decision.")} />
            )}
            {approvalItems.map((item) => (
              <ApprovalQueueRow key={item.instanceId} item={item} busy={approvalBusy} onPick={pickApproval} />
            ))}
            <div className="px-4 py-2 text-right">
              <Link to="/workflow" className="text-xs font-medium text-primary hover:underline">
                {t("Open Workflow Tracking")}
              </Link>
            </div>
          </div>
        )}

        {currentTab === "clearance" && (
          <div className="divide-y divide-border/60">
            {lmc && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
            {!lmc && clearanceItems.length === 0 && (
              <EmptyRow text={t("No clearances awaiting your approval.", "No clearances awaiting your approval.")} />
            )}
            {clearanceItems.map((item) => (
              <ClearanceQueueRow key={item.clearanceId} item={item} busy={clearanceBusy} onPick={pickDecision} />
            ))}
          </div>
        )}

        {currentTab === "changeRequests" && (
          <div className="divide-y divide-border/60">
            {lpc && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
            {!lpc && changeItems.length === 0 && (
              <EmptyRow text={t("No profile change requests awaiting review.", "No profile change requests awaiting review.")} />
            )}
            {changeItems.map((item) => (
              <ChangeRequestQueueRow key={item.id} item={item} busy={changeBusy} onPick={pickChange} />
            ))}
          </div>
        )}
      </section>

      {/* Workflow decision modal — comment + confirm (Approve / Reject). */}
      {approvalDecision && (
        <Modal
          visible
          size="md"
          title={approvalDecision.verb === "approve" ? t("Approve Step") : t("Reject Workflow")}
          description={approvalDecision.item.summary}
          onClose={() => setApprovalDecision(null)}
          footer={
            <>
              <button
                type="button"
                onClick={() => setApprovalDecision(null)}
                className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
              >
                {t("Cancel")}
              </button>
              <button
                type="button"
                disabled={approvalBusy || (approvalDecision.verb === "reject" && !approvalComment.trim())}
                onClick={confirmApproval}
                title={
                  approvalDecision.verb === "reject" && !approvalComment.trim()
                    ? t("A reason is required to reject")
                    : undefined
                }
                className={`inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium text-on-accent disabled:cursor-not-allowed disabled:opacity-50 ${
                  approvalDecision.verb === "approve" ? "bg-success" : "bg-error"
                }`}
              >
                {approvalDecision.verb === "approve" ? (
                  <>
                    <CheckCircle2 size={16} /> {t("Confirm Approval")}
                  </>
                ) : (
                  <>
                    <ShieldAlert size={16} /> {t("Confirm Rejection")}
                  </>
                )}
              </button>
            </>
          }
        >
          <div className="space-y-2">
            <p className="text-sm text-muted">
              {t("Step")} {approvalDecision.item.currentStepOrder}/{approvalDecision.item.totalSteps} —{" "}
              {approvalDecision.item.currentStepName}
            </p>
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
              {approvalDecision.verb === "reject" ? (
                <>
                  {t("Reason")} <span className="text-error">*</span>
                </>
              ) : (
                t("Comment")
              )}
            </label>
            <textarea
              autoFocus
              rows={4}
              value={approvalComment}
              onChange={(e) => setApprovalComment(e.target.value)}
              placeholder={
                approvalDecision.verb === "approve"
                  ? t("Optional comment…")
                  : t("Explain why this request is being rejected…")
              }
              className="w-full resize-y rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground focus:border-primary focus:outline-none"
            />
            {approvalError && <p className="text-xs text-error">{approvalError}</p>}
          </div>
        </Modal>
      )}

      {/* Profile change decision modal — approve (auto-applies identity fields) / reject. */}
      {changeDecision && (
        <Modal
          visible
          size="md"
          title={changeDecision.verb === "approve" ? t("Approve Change Request") : t("Reject Change Request")}
          description={`${changeDecision.item.employeeName || changeDecision.item.employeeNumber} · ${t(changeDecision.item.fieldLabel)}`}
          onClose={() => setChangeDecision(null)}
          footer={
            <>
              <button
                type="button"
                onClick={() => setChangeDecision(null)}
                className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
              >
                {t("Cancel")}
              </button>
              <button
                type="button"
                disabled={changeBusy || (changeDecision.verb === "reject" && !changeComment.trim())}
                onClick={confirmChange}
                title={
                  changeDecision.verb === "reject" && !changeComment.trim()
                    ? t("A reason is required to reject")
                    : undefined
                }
                className={`inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium text-on-accent disabled:cursor-not-allowed disabled:opacity-50 ${
                  changeDecision.verb === "approve" ? "bg-success" : "bg-error"
                }`}
              >
                {changeDecision.verb === "approve" ? (
                  <>
                    <CheckCircle2 size={16} /> {t("Confirm Approval")}
                  </>
                ) : (
                  <>
                    <ShieldAlert size={16} /> {t("Confirm Rejection")}
                  </>
                )}
              </button>
            </>
          }
        >
          <div className="space-y-2">
            <div className="rounded-md border border-border bg-secondary/20 px-3 py-2 text-sm">
              <p className="text-muted line-through">{changeDecision.item.currentValue || t("(empty)")}</p>
              <p className="font-medium text-foreground">→ {changeDecision.item.requestedValue}</p>
              {changeDecision.item.reason && (
                <p className="mt-1 text-xs text-muted">
                  {t("Reason")}: {changeDecision.item.reason}
                </p>
              )}
            </div>
            {changeDecision.verb === "approve" && (
              <p className="rounded-md border border-info/30 bg-info/10 px-3 py-2 text-xs text-info">
                {changeDecision.item.kind === "Structural"
                  ? t("Approving records your decision — apply this change through the relevant HR module.")
                  : t("Approving writes this value to the employee record immediately.")}
              </p>
            )}
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
              {changeDecision.verb === "reject" ? (
                <>
                  {t("Reason")} <span className="text-error">*</span>
                </>
              ) : (
                t("Note")
              )}
            </label>
            <textarea
              autoFocus
              rows={3}
              value={changeComment}
              onChange={(e) => setChangeComment(e.target.value)}
              placeholder={changeDecision.verb === "approve" ? t("Optional note…") : t("Explain why this request is being rejected…")}
              className="w-full resize-y rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground focus:border-primary focus:outline-none"
            />
            {changeError && <p className="text-xs text-error">{changeError}</p>}
          </div>
        </Modal>
      )}

      {/* Clearance decision modal — larger remark textarea, confirm to submit. */}
      {pendingDecision && (
        <Modal
          visible
          size="md"
          title={pendingDecision.status === "Cleared" ? t("Mark Cleared") : t("Mark Blocked")}
          description={`${pendingDecision.item.department} · ${pendingDecision.item.employeeName} (${pendingDecision.item.employeeNumber})`}
          onClose={() => setPendingDecision(null)}
          footer={
            <>
              <button
                type="button"
                onClick={() => setPendingDecision(null)}
                className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
              >
                {t("Cancel")}
              </button>
              <button
                type="button"
                disabled={clearanceBusy || (pendingDecision.status === "Blocked" && !pendingNote.trim())}
                onClick={confirmDecision}
                title={
                  pendingDecision.status === "Blocked" && !pendingNote.trim()
                    ? t("A reason is required to block a clearance")
                    : undefined
                }
                className={`inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium text-on-accent disabled:cursor-not-allowed disabled:opacity-50 ${
                  pendingDecision.status === "Cleared" ? "bg-success" : "bg-error"
                }`}
              >
                {pendingDecision.status === "Cleared" ? (
                  <>
                    <CheckCircle2 size={16} /> {t("Confirm Clearance")}
                  </>
                ) : (
                  <>
                    <ShieldAlert size={16} /> {t("Confirm Block")}
                  </>
                )}
              </button>
            </>
          }
        >
          <div className="space-y-2">
            <p className="text-sm text-foreground">{pendingDecision.item.description}</p>
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
              {pendingDecision.status === "Blocked" ? (
                <>
                  {t("Reason")} <span className="text-error">*</span>
                </>
              ) : (
                t("Remarks")
              )}
            </label>
            <textarea
              autoFocus
              rows={5}
              value={pendingNote}
              onChange={(e) => setPendingNote(e.target.value)}
              placeholder={
                pendingDecision.status === "Cleared"
                  ? t("Optional note about this clearance…")
                  : t("Explain why this clearance is being blocked…")
              }
              className="w-full resize-y rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground focus:border-primary focus:outline-none"
            />
            {pendingDecision.status === "Blocked" && !pendingNote.trim() && (
              <p className="text-xs text-muted">{t("A reason is required to block a clearance.")}</p>
            )}
            {clearanceError && <p className="text-xs text-error">{clearanceError}</p>}
          </div>
        </Modal>
      )}
    </>
  );
}

export default memo(ActionQueueWidget);
