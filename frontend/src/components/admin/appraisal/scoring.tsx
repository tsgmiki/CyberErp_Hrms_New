"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, Send, CheckCircle2, UserPlus, Trash2, History, PenLine, FileDown, Gavel, ClipboardCheck, Users, ShieldCheck, Lock } from "lucide-react";
import type { AppraisalLineModel } from "@/models";
import {
  getAppraisal,
  saveAppraisalScores,
  submitAppraisalSelf,
  completeAppraisal,
  reviewerSignOffAppraisal,
  hrCloseAppraisal,
  inviteAppraisalPeers,
  removeAppraisalPeer,
  getPerformanceHistory,
  acknowledgeAppraisal,
  managerSignAppraisal,
  submitAppraisalAppeal,
} from "@/services/admin/appraisal";
import { printAppraisalReport } from "./report";
import EmployeePicker from "@/components/common/employeePicker";
import { appraisalStageLabel } from "@/constants/performance";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";

const INPUT = "w-full rounded-md border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none";
const SCORE = "w-20 rounded-md border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none";
const SIG = "rounded-md border border-border bg-card px-2.5 py-1.5 text-sm sm:w-64";

const numOrNull = (v: unknown): number | null => {
  if (v === "" || v === null || typeof v === "undefined") return null;
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
};

/** One-line description of who the appraisal is currently waiting on. */
const STAGE_HINT: Record<string, string> = {
  SelfAssessment: "Employee self-assessment in progress.",
  ManagerReview: "Awaiting the direct manager's evaluation.",
  SecondLevelReview: "Awaiting the second-level reviewer's sign-off — see the Sign-off tab.",
  EmployeeAcknowledgment: "Awaiting the employee's final acknowledgment signature — see the Sign-off tab.",
  HrSignOff: "Awaiting HR's final sign-off to close & lock — see the Sign-off tab.",
  Completed: "Closed & locked.",
};

interface Props { id: string; setId: (id: string) => void }

function AppraisalScoring({ id }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: appraisal, isLoading } = useQuery({
    queryKey: ["appraisal", id],
    queryFn: () => getAppraisal(id),
    enabled: id !== "",
  });

  const [goals, setGoals] = useState<AppraisalLineModel[]>([]);
  const [competencies, setCompetencies] = useState<AppraisalLineModel[]>([]);
  const [selfComments, setSelfComments] = useState("");
  const [managerComments, setManagerComments] = useState("");
  const [formState, setFormState] = useState<any>({});
  const [isBusy, setIsBusy] = useState(false);
  const [invitePeerId, setInvitePeerId] = useState("");
  const [invitePeerName, setInvitePeerName] = useState("");
  const [empSig, setEmpSig] = useState("");
  const [mgrSig, setMgrSig] = useState("");
  const [reviewerSig, setReviewerSig] = useState("");
  const [reviewerComments, setReviewerComments] = useState("");
  const [hrSig, setHrSig] = useState("");
  const [appealComments, setAppealComments] = useState("");
  const [appealFollowUp, setAppealFollowUp] = useState(false);

  const { data: historyRows } = useQuery({
    queryKey: ["performanceHistory", "Appraisal", id],
    queryFn: () => getPerformanceHistory("Appraisal", id),
    enabled: id !== "",
  });

  useEffect(() => {
    if (appraisal) {
      setGoals(appraisal.goals ?? []);
      setCompetencies(appraisal.competencies ?? []);
      setSelfComments(appraisal.selfComments ?? "");
      setManagerComments(appraisal.managerComments ?? "");
    }
  }, [appraisal]);

  const stage = appraisal?.stage;
  // Editable only when it is BOTH the right stage AND the current user is the authorized actor for it
  // (the workflow engine resolves this): the employee owns self-assessment, the manager owns manager review.
  const canAct = appraisal?.canActCurrentStage ?? false;
  const selfEditable = stage === "SelfAssessment" && canAct;
  const mgrEditable = stage === "ManagerReview" && canAct;
  // Peer administration (invite/remove) belongs to the resolved manager or an HR administrator.
  const canManagePeers = !!(appraisal?.canManagerSign || appraisal?.canAdminister);

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["appraisal", id] });
    queryClient.invalidateQueries({ queryKey: ["appraisals"] });
    queryClient.invalidateQueries({ queryKey: ["performanceHistory", "Appraisal", id] });
  };

  const invitePeer = async () => {
    if (!invitePeerId) return;
    setIsBusy(true);
    const result = await inviteAppraisalPeers({ appraisalId: id, peerEmployeeIds: [invitePeerId] });
    setFormState(result);
    setIsBusy(false);
    setInvitePeerId("");
    setInvitePeerName("");
    if (result.status === "success") refresh();
  };

  const removePeer = async (reviewId: string) => {
    setIsBusy(true);
    const result = await removeAppraisalPeer(reviewId);
    setFormState(result);
    setIsBusy(false);
    if (result?.status !== "error") refresh();
  };

  const acknowledge = async () => {
    setIsBusy(true);
    const result = await acknowledgeAppraisal({ id, signature: empSig });
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") { setEmpSig(""); refresh(); }
  };
  const managerSign = async () => {
    setIsBusy(true);
    const result = await managerSignAppraisal({ id, signature: mgrSig });
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") { setMgrSig(""); refresh(); }
  };
  const reviewerSignOff = async () => {
    setIsBusy(true);
    const result = await reviewerSignOffAppraisal({ id, signature: reviewerSig, comments: reviewerComments || undefined });
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") { setReviewerSig(""); setReviewerComments(""); refresh(); }
  };
  const hrClose = async () => {
    setIsBusy(true);
    const result = await hrCloseAppraisal({ id, signature: hrSig });
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") { setHrSig(""); refresh(); }
  };
  const appeal = async () => {
    setIsBusy(true);
    const result = await submitAppraisalAppeal({ appraisalId: id, comments: appealComments, requestFollowUp: appealFollowUp });
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") { setAppealComments(""); setAppealFollowUp(false); refresh(); }
  };

  const patch = (
    setter: React.Dispatch<React.SetStateAction<AppraisalLineModel[]>>,
    lineId: string,
    field: keyof AppraisalLineModel,
    value: unknown,
  ) => setter((p) => p.map((l) => (l.id === lineId ? { ...l, [field]: value } : l)));

  /** Persist the current grid for a scope, returning the result (no busy/refresh side-effects). */
  const persistScores = (scope: "Self" | "Manager") => {
    const build = (lines: AppraisalLineModel[]) =>
      lines.map((l) => ({
        lineId: l.id as string,
        score: scope === "Self" ? numOrNull(l.selfScore) : numOrNull(l.managerScore),
        comments: scope === "Self" ? l.selfComments : l.managerComments,
      }));
    return saveAppraisalScores({
      id,
      scope,
      comments: scope === "Self" ? selfComments : managerComments,
      goals: build(goals),
      competencies: build(competencies),
    });
  };

  const save = async (scope: "Self" | "Manager") => {
    setIsBusy(true);
    const result = await persistScores(scope);
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") refresh();
  };

  /**
   * Save the current scores for a scope, THEN run the transition — so the manager (or employee) never loses
   * the scores they just typed by forgetting to click Save first (the cause of the spurious
   * "Score at least one goal or competency" error on Complete).
   */
  const saveThen = async (scope: "Self" | "Manager", transition: () => Promise<any>) => {
    setIsBusy(true);
    const saved = await persistScores(scope);
    if (saved.status !== "success") { setFormState(saved); setIsBusy(false); return; }
    const result = await transition();
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") refresh();
  };

  if (isLoading || !appraisal) return <Loading />;

  const renderLines = (
    lines: AppraisalLineModel[],
    setter: React.Dispatch<React.SetStateAction<AppraisalLineModel[]>>,
    heading: string,
  ) =>
    lines.length === 0 ? null : (
      <div>
        <h4 className="mb-2 text-sm font-semibold">{t(heading)}</h4>
        <div className="overflow-x-auto rounded-lg border border-border">
          <table className="w-full text-[13px]">
            <thead>
              <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                <th className="px-2 py-2 font-semibold">{t("Item")}</th>
                <th className="px-2 py-2 font-semibold">{t("Weight")}</th>
                <th className="px-2 py-2 font-semibold">{t("Self Score")}</th>
                <th className="px-2 py-2 font-semibold">{t("Manager Score")}</th>
              </tr>
            </thead>
            <tbody>
              {lines.map((l) => (
                <tr key={l.id} className="border-b border-border/60 align-top last:border-0">
                  <td className="px-2 py-2 text-foreground">{l.title}</td>
                  <td className="px-2 py-2 text-muted">{l.weight ?? 0}%</td>
                  <td className="px-2 py-2">
                    {selfEditable ? (
                      <input type="number" step="any" className={SCORE} value={l.selfScore ?? ""} onChange={(e) => patch(setter, l.id as string, "selfScore", e.target.value)} />
                    ) : (
                      <span className="text-foreground">{l.selfScore ?? "—"}</span>
                    )}
                  </td>
                  <td className="px-2 py-2">
                    {mgrEditable ? (
                      <input type="number" step="any" className={SCORE} value={l.managerScore ?? ""} onChange={(e) => patch(setter, l.id as string, "managerScore", e.target.value)} />
                    ) : (
                      <span className="text-foreground">{l.managerScore ?? "—"}</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    );

  const isCompleted = stage === "Completed";

  // A read-only signature card (used in the completed sign-off panel).
  const sigCard = (label: string, signature?: string, at?: string, accent = false) => (
    <div className={`rounded-md border p-3 ${accent ? "border-primary/30 bg-primary/5" : "border-border"}`}>
      <p className="text-sm font-semibold italic">{signature || "—"}</p>
      <p className="text-xs text-muted">{t(label)}{at ? ` · ${at.slice(0, 10)}` : ""}</p>
    </div>
  );

  // Read-only notice for viewers who are not the current stage's authorized actor.
  const awaitingNotice = (
    <p className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-center text-xs text-muted">
      {t("Awaiting")} <span className="font-medium text-foreground">{appraisal.currentStageActorName || t("the assigned approver")}</span> — {t("you do not have permission to act on this step.")}
    </p>
  );

  // Optional manager counter-signature — only the resolved manager, once their review is complete.
  const managerCounterSign = appraisal.canManagerSign && stage && ["SecondLevelReview", "EmployeeAcknowledgment", "HrSignOff"].includes(stage) ? (
    <div className="rounded-md border border-border p-3">
      <label className="mb-1 block text-xs font-medium text-muted">{t("Manager Signature")} ({t("optional")})</label>
      <div className="flex flex-wrap items-center gap-2">
        <input className={SIG} value={mgrSig} placeholder={appraisal.managerSignature || (t("Type full name to sign") ?? "")} onChange={(e) => setMgrSig(e.target.value)} />
        <button type="button" disabled={isBusy || !mgrSig} onClick={managerSign} className="inline-flex items-center gap-1 rounded-md border border-border px-3 py-1.5 text-xs font-semibold hover:bg-secondary/40 disabled:opacity-50">
          <PenLine className="h-3.5 w-3.5" /> {appraisal.managerSignature ? t("Re-sign") : t("Sign as Manager")}
        </button>
        {appraisal.managerSignature && <span className="text-xs text-muted">{t("Signed")}: {appraisal.managerSignature}</span>}
      </div>
    </div>
  ) : null;

  // Sign-off tab content, driven by the current stage.
  const signOffContent = () => {
    if (stage === "SelfAssessment" || stage === "ManagerReview") {
      return (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-center text-xs text-muted">
          {t("Sign-off actions become available after the manager completes the review.")}
        </p>
      );
    }

    if (stage === "SecondLevelReview") {
      return (
        <div className="space-y-4">
          {!canAct ? awaitingNotice : (
          <div className="rounded-md border border-border p-3">
            <p className="mb-2 text-xs font-semibold text-muted">{t("Second-level reviewer")}</p>
            <label className="mb-1 block text-xs font-medium text-muted">{t("High-level comments")}</label>
            <textarea className="mb-2 w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm" rows={2} value={reviewerComments} placeholder={t("Optional — note anything for the record") ?? ""} onChange={(e) => setReviewerComments(e.target.value)} />
            <label className="mb-1 block text-xs font-medium text-muted">{t("Signature")}</label>
            <div className="flex flex-wrap items-center gap-2">
              <input className={SIG} value={reviewerSig} placeholder={t("Type full name to sign off") ?? ""} onChange={(e) => setReviewerSig(e.target.value)} />
              <button type="button" disabled={isBusy || !reviewerSig} onClick={reviewerSignOff} className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                <ShieldCheck className="h-3.5 w-3.5" /> {t("Approve & Sign off")}
              </button>
            </div>
            <p className="mt-2 text-xs text-muted">{t("Signing off routes the appraisal back to the employee for their final signature.")}</p>
          </div>
          )}
          {managerCounterSign}
        </div>
      );
    }

    if (stage === "EmployeeAcknowledgment") {
      if (appraisal.acknowledgmentStatus === "Appealed") {
        return (
          <div className="flex items-center gap-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-sm">
            <Gavel className="h-4 w-4 text-warning" /> {t("This appraisal is under appeal.")}
          </div>
        );
      }
      return (
        <div className="space-y-4">
          {appraisal.reviewerSignature && (
            <div className="rounded-md border border-border bg-secondary/20 p-3 text-xs">
              <p className="font-semibold text-muted">{t("Reviewer")}: <span className="not-italic text-foreground">{appraisal.reviewerSignature}</span></p>
              {appraisal.reviewerComments && <p className="mt-1 text-muted">{appraisal.reviewerComments}</p>}
            </div>
          )}
          {/* The decision belongs to the appraisee ALONE — everyone else sees a read-only notice. */}
          {!canAct ? awaitingNotice : (
          <div className="rounded-md border border-border p-3">
            <p className="mb-2 text-xs font-semibold text-muted">{t("Employee decision")}</p>
            <div className="flex flex-wrap items-center gap-2">
              <input className={SIG} value={empSig} placeholder={t("Type full name to accept & sign") ?? ""} onChange={(e) => setEmpSig(e.target.value)} />
              <button type="button" disabled={isBusy || !empSig} onClick={acknowledge} className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                <CheckCircle2 className="h-3.5 w-3.5" /> {t("Accept & Sign")}
              </button>
            </div>

            <div className="mt-3 border-t border-border pt-3">
              <p className="mb-1 text-xs font-semibold text-muted">{t("…or appeal this evaluation")}</p>
              <textarea className="w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm" rows={2} value={appealComments} placeholder={t("Reason for appeal") ?? ""} onChange={(e) => setAppealComments(e.target.value)} />
              <div className="mt-2 flex flex-wrap items-center gap-3">
                <label className="flex items-center gap-1 text-xs">
                  <input type="checkbox" className="h-4 w-4 accent-primary" checked={appealFollowUp} onChange={(e) => setAppealFollowUp(e.target.checked)} /> {t("Request follow-up discussion")}
                </label>
                <button type="button" disabled={isBusy || !appealComments} onClick={appeal} className="inline-flex items-center gap-1 rounded-md border border-border px-3 py-1.5 text-xs font-semibold hover:bg-secondary/40 disabled:opacity-50">
                  <Gavel className="h-3.5 w-3.5" /> {t("Submit Appeal")}
                </button>
              </div>
            </div>
          </div>
          )}
          {managerCounterSign}
        </div>
      );
    }

    if (stage === "HrSignOff") {
      return (
        <div className="space-y-4">
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            {sigCard("Employee", appraisal.employeeSignature, appraisal.employeeSignedAt, true)}
            {appraisal.reviewerSignature ? sigCard("Reviewer", appraisal.reviewerSignature, appraisal.reviewerSignedAt) : null}
          </div>
          {!canAct ? awaitingNotice : (
          <div className="rounded-md border border-border p-3">
            <p className="mb-2 text-xs font-semibold text-muted">{t("HR final sign-off")}</p>
            <div className="flex flex-wrap items-center gap-2">
              <input className={SIG} value={hrSig} placeholder={t("Type full name to close & lock") ?? ""} onChange={(e) => setHrSig(e.target.value)} />
              <button type="button" disabled={isBusy || !hrSig} onClick={hrClose} className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                <Lock className="h-3.5 w-3.5" /> {t("Close & Lock")}
              </button>
            </div>
            <p className="mt-2 text-xs text-muted">{t("HR does not rate — this final signature closes the cycle and locks the document.")}</p>
          </div>
          )}
          {managerCounterSign}
        </div>
      );
    }

    // Completed — full read-only signature panel.
    return (
      <div className="space-y-3">
        {appraisal.acknowledgmentStatus === "Appealed" && (
          <div className="flex items-center gap-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-sm">
            <Gavel className="h-4 w-4 text-warning" /> {t("This appraisal is under appeal.")}
          </div>
        )}
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          {sigCard("Employee", appraisal.employeeSignature, appraisal.employeeSignedAt, true)}
          {sigCard("Manager", appraisal.managerSignature, appraisal.managerSignedAt)}
          {appraisal.reviewerSignature ? sigCard("Reviewer", appraisal.reviewerSignature, appraisal.reviewerSignedAt) : null}
          {appraisal.hrSignature ? sigCard("HR", appraisal.hrSignature, appraisal.hrSignedAt) : null}
        </div>
        {appraisal.reviewerComments && (
          <p className="rounded-md border border-border bg-secondary/20 p-3 text-xs text-muted">
            <span className="font-semibold">{t("Reviewer note")}:</span> {appraisal.reviewerComments}
          </p>
        )}
      </div>
    );
  };

  return (
    <div className="space-y-4 text-foreground">
      {/* Identity header — always visible above the tabs. */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-base font-semibold">{appraisal.employeeName}</h2>
            <p className="text-xs text-muted">
              {appraisal.reviewCycleName}
              {appraisal.periodStart ? ` · ${appraisal.periodStart.slice(0, 10)} – ${(appraisal.periodEnd || "").slice(0, 10)}` : ""}
            </p>
          </div>
          <div className="flex flex-col items-end gap-1">
            <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
              {appraisalStageLabel(stage)}
            </span>
            <span className="text-xs text-muted">
              {t("Goals")} {appraisal.goalsWeight}% · {t("Competencies")} {appraisal.competenciesWeight}%
            </span>
          </div>
        </div>
        {stage && STAGE_HINT[stage] && (
          <p className="mt-2 text-xs text-muted">
            {t(STAGE_HINT[stage])}
            {appraisal.currentStageActorName && !appraisal.canActCurrentStage && (
              <span className="text-muted"> — {t("with")} <span className="font-medium text-foreground">{appraisal.currentStageActorName}</span></span>
            )}
          </p>
        )}
        {!isCompleted && appraisal.canActCurrentStage && (
          <p className="mt-1 inline-flex items-center gap-1 rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-semibold text-primary">
            <CheckCircle2 className="h-3 w-3" /> {t("Your action is required")}
          </p>
        )}
        {isCompleted && (
          <div className="mt-3 flex flex-wrap items-center gap-2 rounded-md border border-primary/30 bg-primary/10 px-3 py-2 text-sm">
            <CheckCircle2 className="h-4 w-4 text-primary" />
            <span className="font-semibold">{t("Overall Score")}: {appraisal.overallScore}</span>
            {appraisal.finalRatingLabel && <span className="text-muted">· {appraisal.finalRatingLabel}</span>}
            <button type="button" onClick={() => printAppraisalReport(appraisal)} className="ml-auto inline-flex items-center gap-1 rounded-md border border-border bg-card px-3 py-1 text-xs font-semibold hover:bg-secondary/40">
              <FileDown className="h-3.5 w-3.5" /> {t("Print Report")}
            </button>
          </div>
        )}
      </section>

      <EntityFormTabs
        hasId
        tabs={[
          {
            key: "scorecard",
            label: "Scorecard",
            Icon: ClipboardCheck,
            description: "Rate goals and competencies, then capture comments",
            keepMounted: true,
            content: (
              <div className="space-y-5">
                {renderLines(goals, setGoals, "Goals")}
                {renderLines(competencies, setCompetencies, "Competencies")}
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                  <div>
                    <label className="mb-1 block text-xs font-medium text-muted">{t("Self Comments")}</label>
                    <textarea className={INPUT} rows={3} value={selfComments} disabled={!selfEditable} onChange={(e) => setSelfComments(e.target.value)} />
                  </div>
                  <div>
                    <label className="mb-1 block text-xs font-medium text-muted">{t("Manager Comments")}</label>
                    <textarea className={INPUT} rows={3} value={managerComments} disabled={!mgrEditable} onChange={(e) => setManagerComments(e.target.value)} />
                  </div>
                </div>
              </div>
            ),
          },
          {
            key: "peers",
            label: "Peer Reviews",
            Icon: Users,
            description: "Invite peers — each submits their own 360° feedback from their My Peer Reviews screen (HC127)",
            content: (
              <div>
                {appraisal.peerAverageScore != null && (
                  <div className="mb-3 flex justify-end">
                    <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
                      {t("Peer Average")}: {appraisal.peerAverageScore}
                    </span>
                  </div>
                )}

                {(appraisal.peerReviews?.length ?? 0) === 0 ? (
                  <p className="py-3 text-center text-sm text-muted">
                    {canManagePeers ? t("No peers invited yet.") : t("Peer feedback is collected independently — individual responses are not shown here.")}
                  </p>
                ) : (
                  <div className="overflow-x-auto rounded-lg border border-border">
                    <table className="w-full text-[13px]">
                      <thead>
                        <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                          <th className="px-2 py-2 font-semibold">{t("Peer")}</th>
                          <th className="px-2 py-2 font-semibold">{t("Status")}</th>
                          <th className="px-2 py-2 font-semibold">{t("Score")}</th>
                          <th className="px-2 py-2" />
                        </tr>
                      </thead>
                      <tbody>
                        {(appraisal.peerReviews ?? []).map((p) => (
                          <tr key={p.id} className="border-b border-border/60 last:border-0">
                            <td className="px-2 py-2 text-foreground">{p.peerEmployeeName}</td>
                            <td className="px-2 py-2 text-muted">{p.status === "Submitted" ? t("Submitted") : t("Awaiting peer")}</td>
                            <td className="px-2 py-2 text-foreground">{p.status === "Submitted" ? (p.score ?? "—") : "—"}</td>
                            <td className="px-2 py-1.5 text-right">
                              {canManagePeers && (
                                <button type="button" disabled={isBusy} onClick={() => removePeer(p.id as string)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                                  <Trash2 className="h-4 w-4" />
                                </button>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}

                {/* Peer invitations belong to the manager / HR — not the appraisee or bystanders.
                    Role-scoped searchable picker: a manager only sees their unit + child units. */}
                {canManagePeers && (
                  <div className="mt-3 flex items-end gap-2">
                    <div className="w-full sm:w-72">
                      {/* excludeId: the appraisee can never be offered as their own peer reviewer. */}
                      <EmployeePicker value={invitePeerId} displayValue={invitePeerName} excludeId={appraisal.employeeId} placeholder={t("Search a peer to invite…") ?? ""} onSelect={(pid, name) => { setInvitePeerId(pid); setInvitePeerName(name); }} />
                    </div>
                    <button type="button" disabled={isBusy || !invitePeerId} onClick={invitePeer} className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                      <UserPlus className="h-3.5 w-3.5" /> {t("Invite Peer")}
                    </button>
                  </div>
                )}
              </div>
            ),
          },
          {
            key: "signoff",
            label: "Sign-off",
            Icon: PenLine,
            description: "Reviewer, employee and HR signatures (HC142–146)",
            content: signOffContent(),
          },
          {
            key: "history",
            label: "History",
            Icon: History,
            description: "Audit trail of changes to this appraisal (HC132)",
            content: (
              <ul className="space-y-2">
                {(historyRows ?? []).length === 0 ? (
                  <li className="text-sm text-muted">{t("No history yet.")}</li>
                ) : (
                  (historyRows ?? []).map((h) => (
                    <li key={h.id} className="flex items-start gap-2 border-l-2 border-primary/40 pl-3 text-[13px]">
                      <span className="font-semibold text-primary">{h.action}</span>
                      <span className="text-foreground">{h.summary}</span>
                      <span className="ml-auto shrink-0 text-xs text-muted">{h.createdAt ? String(h.createdAt).slice(0, 19).replace("T", " ") : ""}</span>
                    </li>
                  ))
                )}
              </ul>
            ),
          },
        ]}
      />

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* Scoring transitions — persistent bar for the self / manager stages. */}
      {(selfEditable || mgrEditable) && (
        <div className="flex flex-wrap justify-end gap-2 border-t border-border pt-3">
          {selfEditable && (
            <>
              <button type="button" disabled={isBusy} onClick={() => save("Self")} className="inline-flex items-center gap-2 rounded-md border border-border px-4 py-2 text-sm font-semibold hover:bg-secondary/40 disabled:opacity-50">
                <Save className="h-4 w-4" /> {t("Save Self Assessment")}
              </button>
              <button type="button" disabled={isBusy} onClick={() => saveThen("Self", () => submitAppraisalSelf(id))} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                <Send className="h-4 w-4" /> {t("Submit for Manager Review")}
              </button>
            </>
          )}
          {mgrEditable && (
            <>
              <button type="button" disabled={isBusy} onClick={() => save("Manager")} className="inline-flex items-center gap-2 rounded-md border border-border px-4 py-2 text-sm font-semibold hover:bg-secondary/40 disabled:opacity-50">
                <Save className="h-4 w-4" /> {t("Save Manager Scores")}
              </button>
              <button type="button" disabled={isBusy} onClick={() => saveThen("Manager", () => completeAppraisal(id))} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                <CheckCircle2 className="h-4 w-4" /> {t("Complete Manager Review")}
              </button>
            </>
          )}
        </div>
      )}
    </div>
  );
}

export default memo(AppraisalScoring);
