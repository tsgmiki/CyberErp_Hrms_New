"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Users, X, UserPlus, Save, LogOut, Award } from "lucide-react";
import { getAllTrainingEnrollments, enrollTraining, recordParticipation, withdrawEnrollment } from "@/services/admin/trainingEnrollment";
import { issueCertificate } from "@/services/admin/trainingCertificate";
import EmployeePicker from "@/components/common/employeePicker";
import type { TrainingSessionModel, TrainingEnrollmentModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none";

const STATUS_TONE: Record<string, string> = {
  Enrolled: "bg-info/15 text-info",
  Completed: "bg-success/15 text-success",
  NoShow: "bg-warning/15 text-warning",
  Withdrawn: "bg-muted/30 text-muted",
};

/** One participant row: status / attendance / score editing (HR or the employee's manager). */
function ParticipantRow({ row, onChanged }: { row: TrainingEnrollmentModel; onChanged: (msg: string) => void }) {
  const { t } = useTranslation();
  const [status, setStatus] = useState(row.status ?? "Enrolled");
  const [attendance, setAttendance] = useState(row.attendancePercent != null ? String(row.attendancePercent) : "");
  const [score, setScore] = useState(row.assessmentScore != null ? String(row.assessmentScore) : "");
  const [busy, setBusy] = useState(false);

  const withdrawn = row.status === "Withdrawn";

  const save = async () => {
    if (!row.id) return;
    setBusy(true);
    const res = await recordParticipation({
      id: row.id,
      status,
      attendancePercent: attendance === "" ? undefined : Number(attendance),
      assessmentScore: score === "" ? undefined : Number(score),
      completedOn: status === "Completed" ? new Date().toISOString().slice(0, 10) : undefined,
    });
    setBusy(false);
    onChanged(res.message);
  };

  const withdraw = async () => {
    if (!row.id) return;
    setBusy(true);
    const res = await withdrawEnrollment(row.id);
    setBusy(false);
    onChanged(res.message);
  };

  return (
    <tr className="border-t border-border/60">
      <td className="px-3 py-2">
        <span className="block text-sm font-medium">{row.employeeName}</span>
        <span className="block text-xs text-muted">{row.employeeNumber}</span>
      </td>
      <td className="px-3 py-2">
        {withdrawn ? (
          <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE.Withdrawn}`}>{t("Withdrawn")}</span>
        ) : (
          <select className={INPUT} value={status} onChange={(e) => setStatus(e.target.value)}>
            <option value="Enrolled">{t("Enrolled")}</option>
            <option value="Completed">{t("Completed")}</option>
            <option value="NoShow">{t("No-show")}</option>
          </select>
        )}
      </td>
      <td className="px-3 py-2">
        {!withdrawn && <input type="number" min={0} max={100} className={INPUT} placeholder="%" value={attendance} onChange={(e) => setAttendance(e.target.value)} />}
      </td>
      <td className="px-3 py-2">
        {!withdrawn && <input type="number" min={0} max={100} className={INPUT} placeholder={t("Score")} value={score} onChange={(e) => setScore(e.target.value)} />}
      </td>
      <td className="px-3 py-2 text-xs text-muted">{row.feedbackRating ? `★ ${row.feedbackRating}` : "—"}</td>
      <td className="px-3 py-2">
        {!withdrawn && (
          <span className="flex items-center gap-1">
            <button type="button" disabled={busy} title={t("Save participation")} onClick={save} className="rounded p-1 text-muted hover:text-success disabled:opacity-50">
              <Save size={14} />
            </button>
            {row.status === "Completed" && (
              <button
                type="button"
                disabled={busy}
                title={t("Issue certificate (HC200)")}
                onClick={async () => {
                  if (!row.id) return;
                  setBusy(true);
                  const res = await issueCertificate(row.id);
                  setBusy(false);
                  onChanged(res.ok ? t("Certificate issued") : res.message);
                }}
                className="rounded p-1 text-muted hover:text-warning disabled:opacity-50"
              >
                <Award size={14} />
              </button>
            )}
            {row.status === "Enrolled" && (
              <button type="button" disabled={busy} title={t("Withdraw")} onClick={withdraw} className="rounded p-1 text-muted hover:text-error disabled:opacity-50">
                <LogOut size={14} />
              </button>
            )}
          </span>
        )}
      </td>
    </tr>
  );
}

/** HC198 — session participants: enroll, attendance/completion/score, withdrawals. */
function ParticipantsModal({ session, onClose }: { session: TrainingSessionModel; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [pickId, setPickId] = useState("");
  const [pickName, setPickName] = useState("");
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const param = { ...parameterInitialData, take: 100, sessionId: session.id };
  const { data } = useQuery({
    queryKey: ["trainingEnrollments", session.id],
    queryFn: () => getAllTrainingEnrollments(param as never),
  });
  const rows = data?.data ?? [];

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["trainingEnrollments", session.id] });
    queryClient.invalidateQueries({ queryKey: ["trainingSessions"] });
  };

  const enroll = async () => {
    if (!pickId || !session.id) return;
    setBusy(true);
    const res = await enrollTraining(session.id, pickId);
    setBusy(false);
    if (res.ok) {
      setPickId("");
      setPickName("");
    }
    refresh(res.message);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="flex max-h-[90vh] w-full max-w-3xl flex-col rounded-lg border border-border bg-background shadow-xl">
        <div className="flex items-center justify-between border-b border-border px-4 py-3">
          <h3 className="flex items-center gap-2 text-sm font-semibold text-foreground">
            <Users size={16} /> {session.courseName} — {t("Participants")}
            <span className="text-xs font-normal text-muted">
              ({rows.filter((r) => r.status !== "Withdrawn").length}{session.maxParticipants ? ` / ${session.maxParticipants}` : ""})
            </span>
          </h3>
          <button type="button" onClick={onClose} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
        </div>

        {session.status === "Scheduled" && (
          <div className="flex flex-wrap items-end gap-2 border-b border-border px-4 py-3">
            <div className="min-w-[240px] flex-1">
              <label className="mb-1 block text-xs font-medium text-muted">{t("Enroll employee")}</label>
              <EmployeePicker value={pickId} displayValue={pickName} onSelect={(id, name) => { setPickId(id); setPickName(name); }} />
            </div>
            <button
              type="button"
              disabled={busy || !pickId}
              onClick={enroll}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
            >
              <UserPlus size={14} /> {t("Enroll")}
            </button>
          </div>
        )}

        <div className="min-h-0 flex-1 overflow-auto p-2">
          {msg && <p className="mx-2 mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}
          {rows.length === 0 ? (
            <p className="p-6 text-center text-sm text-muted">{t("No participants yet.")}</p>
          ) : (
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs text-muted">
                  <th className="px-3 py-2 font-medium">{t("Employee")}</th>
                  <th className="px-3 py-2 font-medium">{t("Status")}</th>
                  <th className="px-3 py-2 font-medium">{t("Attendance %")}</th>
                  <th className="px-3 py-2 font-medium">{t("Score")}</th>
                  <th className="px-3 py-2 font-medium">{t("Feedback")}</th>
                  <th className="px-3 py-2 font-medium">{t("Action")}</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r) => <ParticipantRow key={r.id} row={r} onChanged={refresh} />)}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
}

export default memo(ParticipantsModal);
