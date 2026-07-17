"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, Send, CheckCircle2, UserPlus, Trash2, History, PenLine, FileDown, Gavel } from "lucide-react";
import type { AppraisalLineModel } from "@/models";
import {
  getAppraisal,
  saveAppraisalScores,
  submitAppraisalSelf,
  completeAppraisal,
  inviteAppraisalPeers,
  submitAppraisalPeer,
  removeAppraisalPeer,
  getPerformanceHistory,
  acknowledgeAppraisal,
  managerSignAppraisal,
  submitAppraisalAppeal,
} from "@/services/admin/appraisal";
import { printAppraisalReport } from "./report";
import getAllEmployee from "@/services/admin/employee/getAll";
import { appraisalStageLabel } from "@/constants/performance";
import { parameterInitialData } from "@/constants/initialization";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none";
const SCORE = "w-20 rounded-md border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none";

const numOrNull = (v: unknown): number | null => {
  if (v === "" || v === null || typeof v === "undefined") return null;
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
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
  const [peerScores, setPeerScores] = useState<Record<string, string>>({});
  const [showHistory, setShowHistory] = useState(false);
  const [empSig, setEmpSig] = useState("");
  const [mgrSig, setMgrSig] = useState("");
  const [appealComments, setAppealComments] = useState("");
  const [appealFollowUp, setAppealFollowUp] = useState(false);

  const [empParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: employees } = useQuery({ queryKey: ["employees", empParam], queryFn: () => getAllEmployee(empParam) });
  const { data: historyRows } = useQuery({
    queryKey: ["performanceHistory", "Appraisal", id],
    queryFn: () => getPerformanceHistory("Appraisal", id),
    enabled: showHistory && id !== "",
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
  const selfEditable = stage === "SelfAssessment";
  const mgrEditable = stage === "ManagerReview";

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
    if (result.status === "success") refresh();
  };

  const submitPeer = async (reviewId: string) => {
    setIsBusy(true);
    const raw = peerScores[reviewId];
    const result = await submitAppraisalPeer({ id: reviewId, score: raw === "" || raw == null ? null : Number(raw) });
    setFormState(result);
    setIsBusy(false);
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

  const save = async (scope: "Self" | "Manager") => {
    setIsBusy(true);
    const build = (lines: AppraisalLineModel[]) =>
      lines.map((l) => ({
        lineId: l.id as string,
        score: scope === "Self" ? numOrNull(l.selfScore) : numOrNull(l.managerScore),
        comments: scope === "Self" ? l.selfComments : l.managerComments,
      }));
    const result = await saveAppraisalScores({
      id,
      scope,
      comments: scope === "Self" ? selfComments : managerComments,
      goals: build(goals),
      competencies: build(competencies),
    });
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") refresh();
  };

  const runAction = async (fn: () => Promise<any>) => {
    setIsBusy(true);
    const result = await fn();
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
      <section className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t(heading)}</h3>
        <div className="overflow-x-auto">
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
                <tr key={l.id} className="border-b border-border/60 align-top">
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
      </section>
    );

  return (
    <div className="space-y-5 text-foreground">
      {/* Header */}
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
        {appraisal.stage === "Completed" && (
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

      {/* Acknowledgment / signing / appeal (HC142–144, HC146) */}
      {appraisal.stage === "Completed" && (
        <section className="rounded-lg border border-border bg-card p-4">
          <h3 className="mb-3 text-sm font-semibold">{t("Acknowledgment & Signatures")}</h3>

          {appraisal.acknowledgmentStatus === "Accepted" ? (
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <div className="rounded-md border border-primary/30 bg-primary/5 p-3">
                <p className="text-sm font-semibold italic">{appraisal.employeeSignature}</p>
                <p className="text-xs text-muted">{t("Employee")} · {(appraisal.employeeSignedAt || "").slice(0, 10)}</p>
              </div>
              <div className="rounded-md border border-border p-3">
                <p className="text-sm font-semibold italic">{appraisal.managerSignature || "—"}</p>
                <p className="text-xs text-muted">{t("Manager")}{appraisal.managerSignedAt ? ` · ${appraisal.managerSignedAt.slice(0, 10)}` : ""}</p>
              </div>
            </div>
          ) : appraisal.acknowledgmentStatus === "Appealed" ? (
            <div className="flex items-center gap-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-sm">
              <Gavel className="h-4 w-4 text-warning" /> {t("This appraisal is under appeal.")}
            </div>
          ) : (
            <div className="space-y-4">
              {/* Manager counter-signature */}
              <div>
                <label className="mb-1 block text-xs font-medium text-muted">{t("Manager Signature")}</label>
                <div className="flex flex-wrap items-center gap-2">
                  <input className="rounded-md border border-border bg-card px-2.5 py-1.5 text-sm sm:w-64" value={mgrSig} placeholder={appraisal.managerSignature || (t("Type full name to sign") ?? "")} onChange={(e) => setMgrSig(e.target.value)} />
                  <button type="button" disabled={isBusy || !mgrSig} onClick={managerSign} className="inline-flex items-center gap-1 rounded-md border border-border px-3 py-1.5 text-xs font-semibold hover:bg-secondary/40 disabled:opacity-50">
                    <PenLine className="h-3.5 w-3.5" /> {appraisal.managerSignature ? t("Re-sign") : t("Sign as Manager")}
                  </button>
                  {appraisal.managerSignature && <span className="text-xs text-muted">{t("Signed")}: {appraisal.managerSignature}</span>}
                </div>
              </div>

              {/* Employee accept & sign */}
              <div className="rounded-md border border-border p-3">
                <p className="mb-2 text-xs font-semibold text-muted">{t("Employee decision")}</p>
                <div className="flex flex-wrap items-center gap-2">
                  <input className="rounded-md border border-border bg-card px-2.5 py-1.5 text-sm sm:w-64" value={empSig} placeholder={t("Type full name to accept & sign") ?? ""} onChange={(e) => setEmpSig(e.target.value)} />
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
            </div>
          )}
        </section>
      )}

      {renderLines(goals, setGoals, "Goals")}
      {renderLines(competencies, setCompetencies, "Competencies")}

      {/* Comments */}
      <section className="rounded-lg border border-border bg-card p-4">
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
      </section>

      {/* Peer reviews (HC127) */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="text-sm font-semibold">{t("Peer Reviews")}</h3>
          {appraisal.peerAverageScore != null && (
            <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
              {t("Peer Average")}: {appraisal.peerAverageScore}
            </span>
          )}
        </div>

        {(appraisal.peerReviews?.length ?? 0) === 0 ? (
          <p className="py-3 text-center text-sm text-muted">{t("No peers invited yet.")}</p>
        ) : (
          <div className="overflow-x-auto">
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
                  <tr key={p.id} className="border-b border-border/60">
                    <td className="px-2 py-2 text-foreground">{p.peerEmployeeName}</td>
                    <td className="px-2 py-2 text-muted">{p.status}</td>
                    <td className="px-2 py-2">
                      {p.status === "Submitted" ? (
                        <span className="text-foreground">{p.score ?? "—"}</span>
                      ) : (
                        <input type="number" step="any" className="w-20 rounded-md border border-border bg-card px-2 py-1 text-sm" value={peerScores[p.id as string] ?? ""} onChange={(e) => setPeerScores((s) => ({ ...s, [p.id as string]: e.target.value }))} />
                      )}
                    </td>
                    <td className="px-2 py-1.5 text-right">
                      <div className="flex justify-end gap-1">
                        {p.status !== "Submitted" && (
                          <button type="button" disabled={isBusy} onClick={() => submitPeer(p.id as string)} className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs font-semibold hover:bg-secondary/40 disabled:opacity-50">
                            <Send className="h-3 w-3" /> {t("Submit")}
                          </button>
                        )}
                        <button type="button" disabled={isBusy} onClick={() => removePeer(p.id as string)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <div className="mt-3 flex items-end gap-2">
          <select className="rounded-md border border-border bg-card px-2.5 py-1.5 text-sm sm:w-72" value={invitePeerId} onChange={(e) => setInvitePeerId(e.target.value)}>
            <option value="">{t("Select a peer to invite")}</option>
            {(employees?.data ?? []).map((e) => (
              <option key={e.id} value={e.id}>{e.employeeNumber} — {e.fullName ?? ""}</option>
            ))}
          </select>
          <button type="button" disabled={isBusy || !invitePeerId} onClick={invitePeer} className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
            <UserPlus className="h-3.5 w-3.5" /> {t("Invite Peer")}
          </button>
        </div>
      </section>

      {/* Version history (HC132) */}
      <section className="rounded-lg border border-border bg-card p-4">
        <button type="button" onClick={() => setShowHistory((s) => !s)} className="flex items-center gap-2 text-sm font-semibold">
          <History className="h-4 w-4" /> {t("History")}
        </button>
        {showHistory && (
          <ul className="mt-3 space-y-2">
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
        )}
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* Stage actions */}
      <div className="flex flex-wrap justify-end gap-2">
        {selfEditable && (
          <>
            <button type="button" disabled={isBusy} onClick={() => save("Self")} className="inline-flex items-center gap-2 rounded-md border border-border px-4 py-2 text-sm font-semibold hover:bg-secondary/40 disabled:opacity-50">
              <Save className="h-4 w-4" /> {t("Save Self Assessment")}
            </button>
            <button type="button" disabled={isBusy} onClick={() => runAction(() => submitAppraisalSelf(id))} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              <Send className="h-4 w-4" /> {t("Submit for Manager Review")}
            </button>
          </>
        )}
        {mgrEditable && (
          <>
            <button type="button" disabled={isBusy} onClick={() => save("Manager")} className="inline-flex items-center gap-2 rounded-md border border-border px-4 py-2 text-sm font-semibold hover:bg-secondary/40 disabled:opacity-50">
              <Save className="h-4 w-4" /> {t("Save Manager Scores")}
            </button>
            <button type="button" disabled={isBusy} onClick={() => runAction(() => completeAppraisal(id))} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              <CheckCircle2 className="h-4 w-4" /> {t("Complete Appraisal")}
            </button>
          </>
        )}
      </div>
    </div>
  );
}

export default memo(AppraisalScoring);
