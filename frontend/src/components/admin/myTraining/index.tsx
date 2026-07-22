"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { GraduationCap, Star, Send, ScrollText, Video } from "lucide-react";
import { getCpdSummary } from "@/services/admin/trainingCpd";
import { getAllTrainingEnrollments, submitTrainingFeedback } from "@/services/admin/trainingEnrollment";
import { getAllTrainingCertificates } from "@/services/admin/trainingCertificate";
import type { TrainingEnrollmentModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";

const STATUS_TONE: Record<string, string> = {
  Enrolled: "bg-info/15 text-info",
  Completed: "bg-success/15 text-success",
  NoShow: "bg-warning/15 text-warning",
  Withdrawn: "bg-muted/30 text-muted",
};

const fmtDate = (v?: string) => (v ? v.slice(0, 10) : "—");

/** One of my enrollments — completed ones without feedback carry the HC199 feedback form. */
function EnrollmentCard({ row, onChanged }: { row: TrainingEnrollmentModel; onChanged: () => void }) {
  const { t } = useTranslation();
  const [rating, setRating] = useState("5");
  const [comments, setComments] = useState("");
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const needsFeedback = row.status === "Completed" && !row.feedbackRating;

  const submit = async () => {
    if (!row.id) return;
    setBusy(true);
    const res = await submitTrainingFeedback(row.id, Number(rating), comments || undefined);
    setBusy(false);
    if (res.ok) onChanged();
    else setMsg(res.message);
  };

  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="mb-2 flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-foreground">{row.courseName}</p>
          <p className="text-xs text-muted">
            {fmtDate(row.sessionStartDate)} → {fmtDate(row.sessionEndDate)}
            {row.cpdHours ? ` · ${row.cpdHours} CPD` : ""}
          </p>
        </div>
        <span className={`shrink-0 rounded-full px-2.5 py-0.5 text-xs font-semibold ${STATUS_TONE[row.status ?? ""] ?? "bg-secondary/40"}`}>
          {row.status}
        </span>
      </div>
      <div className="flex flex-wrap gap-3 text-xs text-muted">
        {row.attendancePercent != null && <span>{t("Attendance")}: <strong className="text-foreground">{row.attendancePercent}%</strong></span>}
        {row.assessmentScore != null && <span>{t("Score")}: <strong className="text-foreground">{row.assessmentScore}</strong></span>}
        {row.completedOn && <span>{t("Completed")}: <strong className="text-foreground">{fmtDate(row.completedOn)}</strong></span>}
        {row.feedbackRating && <span className="text-warning">★ {row.feedbackRating} {t("your feedback")}</span>}
      </div>

      {needsFeedback && (
        <div className="mt-3 rounded-md border border-border/70 bg-secondary/10 p-3">
          <p className="mb-2 flex items-center gap-1.5 text-xs font-semibold text-foreground">
            <Star size={13} className="text-warning" /> {t("How effective was this training?")}
          </p>
          <div className="flex flex-wrap items-end gap-2">
            <div className="w-24">
              <label className="mb-1 block text-[11px] text-muted">{t("Rating (1–5)")}</label>
              <select className={INPUT} value={rating} onChange={(e) => setRating(e.target.value)}>
                {[5, 4, 3, 2, 1].map((n) => <option key={n} value={n}>{n}</option>)}
              </select>
            </div>
            <div className="min-w-[180px] flex-1">
              <label className="mb-1 block text-[11px] text-muted">{t("Comments")}</label>
              <input type="text" className={INPUT} placeholder={t("What worked, what to improve")} value={comments} onChange={(e) => setComments(e.target.value)} />
            </div>
            <button type="button" disabled={busy} onClick={submit}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              <Send size={13} /> {busy ? t("Sending…") : t("Submit")}
            </button>
          </div>
          {msg && <p className="mt-2 text-xs text-error">{msg}</p>}
        </div>
      )}
    </div>
  );
}

/** My Training — the employee's own learning record: CPD, enrollments (+feedback), certificates. */
function MyTraining() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  // CPD resolves the caller's own employee id — everything else keys off it.
  const { data: cpd, isLoading } = useQuery({
    queryKey: ["myCpd"],
    queryFn: () => getCpdSummary(),
    retry: false,
  });
  const myEmployeeId = cpd?.employeeId;

  const { data: enrollments } = useQuery({
    queryKey: ["myEnrollments", myEmployeeId],
    queryFn: () => getAllTrainingEnrollments({ ...parameterInitialData, take: 100, employeeId: myEmployeeId } as never),
    enabled: !!myEmployeeId,
    placeholderData: keepPreviousData,
  });
  const { data: certificates } = useQuery({
    queryKey: ["myCertificates", myEmployeeId],
    queryFn: () => getAllTrainingCertificates({ ...parameterInitialData, take: 100, employeeId: myEmployeeId } as never),
    enabled: !!myEmployeeId,
  });

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["myEnrollments"] });
    queryClient.invalidateQueries({ queryKey: ["myCpd"] });
  };

  const rows = enrollments?.data ?? [];
  const certs = certificates?.data ?? [];

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center gap-2">
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><GraduationCap className="h-5 w-5" /></span>
        <div>
          <h1 className="text-base font-semibold text-foreground">{t("My Training")}</h1>
          <p className="text-xs text-muted">{t("Your enrollments, feedback, certificates and CPD record.")}</p>
        </div>
      </div>

      {isLoading ? (
        <Loading />
      ) : !myEmployeeId ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("Your account is not linked to an employee record.")}
        </p>
      ) : (
        <div className="min-h-0 flex-1 space-y-4 overflow-auto">
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div className="rounded-lg border border-border bg-card p-4">
              <p className="text-xs uppercase tracking-wide text-muted">{t("CPD Hours")}</p>
              <p className="mt-1 text-3xl font-bold text-primary">{cpd?.totalCpdHours ?? 0}</p>
            </div>
            <div className="rounded-lg border border-border bg-card p-4">
              <p className="text-xs uppercase tracking-wide text-muted">{t("Completed Trainings")}</p>
              <p className="mt-1 text-3xl font-bold text-foreground">{cpd?.completedTrainings ?? 0}</p>
            </div>
            <div className="rounded-lg border border-border bg-card p-4">
              <p className="text-xs uppercase tracking-wide text-muted">{t("Certificates")}</p>
              <p className="mt-1 text-3xl font-bold text-foreground">{cpd?.certificates ?? 0}</p>
            </div>
          </div>

          <div>
            <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted">{t("My Enrollments")} ({rows.length})</p>
            {rows.length === 0 ? (
              <p className="rounded-lg border border-dashed border-border bg-card/40 p-6 text-center text-sm text-muted">
                {t("You are not enrolled in any training yet — browse the sessions under Learning.")}
              </p>
            ) : (
              <div className="space-y-3">
                {rows.map((r) => <EnrollmentCard key={r.id} row={r} onChanged={refresh} />)}
              </div>
            )}
          </div>

          <div>
            <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted">{t("My Certificates")} ({certs.length})</p>
            {certs.length === 0 ? (
              <p className="rounded-lg border border-dashed border-border bg-card/40 p-6 text-center text-sm text-muted">
                {t("No certificates yet — they appear here once issued.")}
              </p>
            ) : (
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                {certs.map((c) => (
                  <div key={c.id} className="flex items-start gap-3 rounded-lg border border-border bg-card p-4">
                    <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-warning/15 text-warning"><ScrollText size={16} /></span>
                    <div className="min-w-0">
                      <p className="truncate text-sm font-semibold text-foreground">{c.title}</p>
                      <p className="text-xs text-muted">{c.certificateNo}</p>
                      <p className="mt-0.5 text-xs text-muted">
                        {t("Issued")} {fmtDate(c.issuedOn)}
                        {c.expiresOn ? ` · ${t("expires")} ${fmtDate(c.expiresOn)}` : ""}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {(cpd?.entries?.length ?? 0) > 0 && (
            <div className="rounded-lg border border-border bg-card">
              <p className="border-b border-border px-4 py-2 text-xs font-semibold uppercase tracking-wide text-muted">
                <Video size={12} className="mr-1 inline" /> {t("CPD Record")}
              </p>
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-left text-xs text-muted">
                    <th className="px-4 py-2 font-medium">{t("Course")}</th>
                    <th className="px-4 py-2 font-medium">{t("Completed")}</th>
                    <th className="px-4 py-2 text-right font-medium">{t("Score")}</th>
                    <th className="px-4 py-2 text-right font-medium">{t("CPD Hours")}</th>
                  </tr>
                </thead>
                <tbody>
                  {(cpd?.entries ?? []).map((e, i) => (
                    <tr key={i} className="border-t border-border/60">
                      <td className="px-4 py-2 text-xs">{e.courseName}</td>
                      <td className="px-4 py-2 text-xs">{fmtDate(e.completedOn)}</td>
                      <td className="px-4 py-2 text-right text-xs">{e.assessmentScore ?? "—"}</td>
                      <td className="px-4 py-2 text-right text-xs font-semibold text-primary">{e.cpdHours}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default memo(MyTraining);
