"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  FileSignature, ShieldCheck, AlertTriangle, Download, Check, X, Search,
} from "lucide-react";
import { getObligations } from "@/services/admin/learningCompliance";
import {
  getTrainingRecord, verifyTrainingRecord, downloadTrainingRecord,
} from "@/services/admin/trainingRecord";
import type { SignableRecordModel } from "@/models";
import { toast } from "@/components/common/toast";
import Loading from "@/components/common/loader/loader";

const stamp = (v: string) =>
  new Date(v).toLocaleString(undefined, {
    day: "2-digit", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit",
  });

/** The verification panel: what is being confirmed, and the verifier's password. */
function VerifyBox({ record, onVerified }: { record: SignableRecordModel; onVerified: () => void }) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [password, setPassword] = useState("");
  const [note, setNote] = useState("");
  const [busy, setBusy] = useState(false);

  const verify = async () => {
    if (!password) {
      toast.error(t("Enter your password to verify."));
      return;
    }
    setBusy(true);
    try {
      await verifyTrainingRecord(record.trainingEnrollmentId, password, note.trim() || undefined);
      toast.success(t("Record verified."));
      setOpen(false);
      setNote("");
      onVerified();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : t("Could not verify this record."));
    } finally {
      setPassword("");
      setBusy(false);
    }
  };

  if (!open) {
    return (
      <button
        type="button" onClick={() => setOpen(true)}
        className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
      >
        <ShieldCheck className="h-3.5 w-3.5" /> {t("Verify")}
      </button>
    );
  }

  return (
    <div className="mt-2 w-full rounded-md border border-primary/25 bg-primary/5 p-3">
      <p className="mb-2 text-xs text-foreground">
        {t("You are confirming that this training record for")}{" "}
        <strong>{record.employeeName}</strong> {t("is accurate and complete.")}
      </p>
      <div className="mb-2 flex flex-wrap items-center gap-2">
        <input
          value={note}
          onChange={(e) => setNote(e.target.value)}
          placeholder={t("Note (optional)") ?? ""}
          className="min-w-0 flex-1 rounded border border-border bg-card px-2 py-1.5 text-xs text-foreground focus:border-primary focus:outline-none"
        />
      </div>
      <div className="flex flex-wrap items-center gap-2">
        <input
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          onKeyDown={(e) => { if (e.key === "Enter") void verify(); }}
          placeholder={t("Your password") ?? ""}
          className="min-w-0 flex-1 rounded border border-border bg-card px-2 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none"
        />
        <button
          type="button" disabled={busy} onClick={verify}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
        >
          <Check className="h-3.5 w-3.5" /> {busy ? t("Verifying…") : t("Confirm")}
        </button>
        <button
          type="button" onClick={() => { setOpen(false); setPassword(""); }}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md border border-border px-2.5 py-1.5 text-xs font-semibold text-muted hover:bg-secondary"
        >
          <X className="h-3.5 w-3.5" /> {t("Cancel")}
        </button>
      </div>
    </div>
  );
}

function RecordCard({ enrollmentId }: { enrollmentId: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: record, isLoading } = useQuery({
    queryKey: ["trainingRecord", enrollmentId],
    queryFn: () => getTrainingRecord(enrollmentId),
  });

  if (isLoading || !record) return null;

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["trainingRecord", enrollmentId] });

  return (
    <div className="rounded-lg border border-border bg-card p-3">
      <div className="flex flex-wrap items-start gap-2">
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-medium text-foreground">
            {record.employeeName}
            <span className="ml-1.5 text-xs font-normal text-muted">{record.employeeNumber}</span>
          </p>
          <p className="truncate text-xs text-muted">
            {[
              record.courseName,
              record.courseVersionNumber ? `v${record.courseVersionNumber}` : null,
              record.completedOn
                ? `${t("completed")} ${new Date(record.completedOn).toLocaleDateString()}`
                : null,
              record.assessmentScore != null
                ? `${t("score")} ${record.assessmentScore}%`
                : t("not assessed"),
            ].filter(Boolean).join(" · ")}
          </p>
        </div>

        <button
          type="button"
          onClick={async () => {
            const ok = await downloadTrainingRecord(record.employeeId, record.employeeName);
            if (!ok) toast.error(t("Could not generate the record."));
          }}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md border border-border px-2.5 py-1.5 text-xs font-semibold text-foreground hover:bg-secondary"
        >
          <Download className="h-3.5 w-3.5" /> {t("Record PDF")}
        </button>

        {record.verified ? (
          <span className="inline-flex shrink-0 items-center gap-1 rounded-full bg-success/15 px-2.5 py-1 text-[11px] font-semibold text-success">
            <ShieldCheck className="h-3.5 w-3.5" /> {t("Verified")}
          </span>
        ) : !record.signedByLearner ? (
          // A verifier confirms what the learner has attested — there is nothing to confirm first.
          <span className="shrink-0 rounded-md bg-secondary px-2.5 py-1 text-[11px] text-muted">
            {t("Awaiting the learner's signature")}
          </span>
        ) : (
          <VerifyBox record={record} onVerified={refresh} />
        )}
      </div>

      {record.signatures.length > 0 && (
        <div className="mt-2 space-y-1.5">
          {record.signatures.map((s) => (
            <div key={s.id} className="rounded-md border border-border bg-secondary/20 px-2.5 py-2">
              <p className="text-xs font-semibold text-foreground">
                {s.meaning === "Verification" ? t("Verified by") : t("Signed by")} {s.signedByName}
                <span className="ml-1.5 font-normal text-muted">{stamp(s.signedOn)}</span>
              </p>
              <p className="mt-0.5 text-[11px] italic text-muted">“{s.signedStatement}”</p>
              {s.note && <p className="mt-0.5 text-[11px] text-muted">{t("Note")}: {s.note}</p>}
              {!s.intact && (
                <p className="mt-1 flex items-center gap-1 text-[11px] font-semibold text-error">
                  <AlertTriangle className="h-3 w-3" />
                  {t("This record has changed since it was signed.")}
                </p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

/**
 * Signed training records — HR's side of the electronic signature.
 *
 * <p>Driven off completed obligations, because a mandatory-training record is the one an inspection
 * asks about. A verifier confirms what the learner has already attested, re-authenticating to do it,
 * and cannot verify their own record.</p>
 */
function RecordsPanel() {
  const { t } = useTranslation();
  const [term, setTerm] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["obligations", "records"],
    queryFn: () => getObligations({ skip: 0, take: 50, status: "Completed" }),
  });

  if (isLoading) return <Loading />;

  const rows = (data?.data ?? []).filter((o) => o.trainingEnrollmentId || true);
  const filtered = term.trim()
    ? rows.filter((o) =>
        [o.employeeName, o.employeeNumber, o.courseName]
          .some((v) => (v ?? "").toLowerCase().includes(term.trim().toLowerCase())))
    : rows;

  return (
    <div className="space-y-3">
      <p className="flex items-start gap-2 rounded-md border border-primary/25 bg-primary/5 px-3 py-2 text-xs text-foreground">
        <FileSignature className="mt-0.5 h-3.5 w-3.5 shrink-0 text-primary" />
        {t("A signature is bound to the facts as they stood when it was taken. If a record changes afterwards, the signature is shown as no longer covering it.")}
      </p>

      <div className="relative">
        <Search className="pointer-events-none absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
        <input
          type="search" value={term} onChange={(e) => setTerm(e.target.value)}
          placeholder={t("Search by employee or course") ?? ""}
          className="w-full rounded-md border border-border bg-card py-2 pl-8 pr-3 text-sm text-foreground focus:border-primary focus:outline-none"
        />
      </div>

      {filtered.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("No completed mandatory training yet.")}
        </p>
      ) : (
        <div className="space-y-2">
          {filtered.map((o) => (
            <RecordCard key={o.id} enrollmentId={o.trainingEnrollmentId as string} />
          ))}
        </div>
      )}
    </div>
  );
}

export default memo(RecordsPanel);
