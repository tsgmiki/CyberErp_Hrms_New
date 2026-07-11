"use client";
import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  BadgeDollarSign,
  Send,
  CheckCircle2,
  XCircle,
  Undo2,
  FileText,
  Trash2,
  Pencil,
} from "lucide-react";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import getAllEmployee from "@/services/admin/employee/getAll";
import getAllSalaryScale from "@/services/admin/salaryScale/getAll";
import {
  getJobOffers,
  saveJobOffer,
  submitJobOffer,
  sendJobOffer,
  respondJobOffer,
  withdrawJobOffer,
  generateOfferLetter,
  deleteJobOffer,
} from "@/services/admin/recruitment";
import type { JobOfferModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";

const lookupParam = { ...parameterInitialData, take: 200 };
const inputCls = "h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground";
const fmtDate = (v?: string) => (v ? v.slice(0, 10) : "—");

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted",
  PendingApproval: "bg-info/15 text-info",
  Approved: "bg-primary/10 text-primary",
  Sent: "bg-warning/15 text-warning",
  Accepted: "bg-success/15 text-success",
  Declined: "bg-error/15 text-error",
  Withdrawn: "bg-muted/30 text-muted",
  Expired: "bg-error/15 text-error",
};

const ACTIVE = ["Draft", "PendingApproval", "Approved", "Sent"];

/** Draft-offer editor (HC111/HC113): terms, salary-scale validation, letter. */
function OfferForm({
  applicationId,
  editing,
  onClose,
  onDone,
}: {
  applicationId: string;
  editing: JobOfferModel | null;
  onClose: () => void;
  onDone: () => void;
}) {
  const { t } = useTranslation();
  const [salary, setSalary] = useState(editing?.salary != null ? String(editing.salary) : "");
  const [salaryScaleId, setSalaryScaleId] = useState(editing?.salaryScaleId ?? "");
  const [justification, setJustification] = useState(editing?.salaryJustification ?? "");
  const [managerId, setManagerId] = useState(editing?.hiringManagerEmployeeId ?? "");
  const [startDate, setStartDate] = useState(fmtDate(editing?.proposedStartDate));
  const [expiryDate, setExpiryDate] = useState(fmtDate(editing?.expiryDate));
  const [letterText, setLetterText] = useState(editing?.letterText ?? "");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: employees } = useQuery({
    queryKey: ["employees", lookupParam],
    queryFn: () => getAllEmployee(lookupParam),
  });
  const { data: scales } = useQuery({
    queryKey: ["salaryScales", lookupParam],
    queryFn: () => getAllSalaryScale(lookupParam),
  });

  const scaleAmount = (scales?.data ?? []).find((s) => s.id === salaryScaleId)?.salary;
  const deviates = scaleAmount != null && salary !== "" && Number(salary.replace(/[,\s]/g, "")) !== scaleAmount;

  const pickScale = (id: string) => {
    setSalaryScaleId(id);
    const amount = (scales?.data ?? []).find((s) => s.id === id)?.salary;
    if (amount != null) setSalary(String(amount));
  };

  const confirm = async () => {
    if (!salary || !startDate || !expiryDate)
      return setError(t("Salary, start date and expiry date are required."));
    setBusy(true);
    const res = await saveJobOffer({
      id: editing?.id,
      applicationId,
      salary: Number(salary.replace(/[,\s]/g, "")),
      salaryScaleId: salaryScaleId || undefined,
      salaryJustification: justification || undefined,
      hiringManagerEmployeeId: managerId || undefined,
      proposedStartDate: startDate,
      expiryDate: expiryDate,
      letterText: letterText || undefined,
    });
    setBusy(false);
    if (res.status !== "success") return setError(res.message);
    onDone();
  };

  return (
    <div className="space-y-2 rounded-lg border border-primary/30 bg-primary/5 p-3">
      <h4 className="text-sm font-semibold text-foreground">
        {editing ? t("Edit Offer {{n}}", { n: editing.offerNumber }) : t("New Offer")}
      </h4>
      <div className="grid grid-cols-2 gap-2">
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
            {t("Salary Scale (HC113)")}
          </label>
          <select value={salaryScaleId} onChange={(e) => pickScale(e.target.value)} className={inputCls}>
            <option value="">{t("No scale — free salary")}</option>
            {(scales?.data ?? []).map((s) => (
              <option key={s.id} value={s.id}>
                {s.jobGrade} / {s.step} — {s.salary?.toLocaleString()}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
            {t("Monthly Salary (ETB)")} <span className="text-error">*</span>
          </label>
          <input type="text" value={salary} onChange={(e) => setSalary(e.target.value)} className={inputCls} />
        </div>
      </div>
      {deviates && (
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-warning">
            {t("Deviation justification (required — salary differs from the scale amount {{a}})", {
              a: scaleAmount?.toLocaleString(),
            })}
          </label>
          <input
            type="text"
            value={justification}
            onChange={(e) => setJustification(e.target.value)}
            className={inputCls}
          />
        </div>
      )}
      <div className="grid grid-cols-3 gap-2">
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Hiring Manager")}</label>
          <select value={managerId} onChange={(e) => setManagerId(e.target.value)} className={inputCls}>
            <option value="">{t("—")}</option>
            {(employees?.data ?? []).map((e) => (
              <option key={e.id} value={e.id}>{e.fullName ?? e.employeeNumber}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
            {t("Proposed Start")} <span className="text-error">*</span>
          </label>
          <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} className={inputCls} />
        </div>
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
            {t("Offer Valid Until")} <span className="text-error">*</span>
          </label>
          <input type="date" value={expiryDate} onChange={(e) => setExpiryDate(e.target.value)} className={inputCls} />
        </div>
      </div>
      <div>
        <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
          {t("Offer Letter (HC111)")}
        </label>
        <textarea
          value={letterText}
          onChange={(e) => setLetterText(e.target.value)}
          rows={6}
          placeholder={t("Save the offer first, then use Generate Letter for the standard text.")}
          className="w-full rounded-md border border-border bg-background px-2 py-1.5 font-mono text-xs text-foreground"
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
          {t("Save Offer")}
        </button>
      </div>
    </div>
  );
}

/** Offer lifecycle manager for one application (HC111–HC114). */
function OfferModal({
  applicationId,
  candidateName,
  canCreate = true,
  onClose,
}: {
  applicationId: string;
  candidateName?: string;
  /** Offers are only CREATED for Selected/OfferPending applications; history stays viewable. */
  canCreate?: boolean;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<JobOfferModel | null>(null);
  const [respondFor, setRespondFor] = useState<{ offer: JobOfferModel; response: "Accept" | "Decline" } | null>(null);
  const [responseNote, setResponseNote] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const queryKey = ["jobOffers", applicationId];
  const { data: offers, isLoading, refetch } = useQuery({
    queryKey,
    queryFn: () => getJobOffers(applicationId),
  });

  const hasActive = (offers ?? []).some((o) => ACTIVE.includes(o.status ?? ""));

  const refresh = async () => {
    await refetch();
    queryClient.invalidateQueries({ queryKey: ["jobApplications"] });
  };

  const run = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setError(null);
    setBusy(true);
    const res = await fn();
    setBusy(false);
    if (!res.ok) return setError(res.message);
    await refresh();
  };

  const generateLetter = async (offer: JobOfferModel) => {
    setError(null);
    const text = await generateOfferLetter(offer.id!);
    const res = await saveJobOffer({ ...offer, letterText: text });
    if (res.status !== "success") return setError(res.message);
    await refresh();
  };

  const removeDraft = async (id: string) => {
    setError(null);
    const res: any = await deleteJobOffer(id);
    if (res?.status === "error") return setError(res.message);
    await refresh();
  };

  const confirmResponse = async () => {
    if (!respondFor) return;
    await run(() =>
      respondJobOffer({ id: respondFor.offer.id!, response: respondFor.response, note: responseNote || undefined }),
    );
    setRespondFor(null);
    setResponseNote("");
  };

  return (
    <Modal
      visible
      size="xl"
      title={t("Offers")}
      description={candidateName}
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
      <div className="space-y-3">
        {isLoading && <Loading />}
        {error && (
          <p className="rounded-md border border-error/30 bg-error/10 px-3 py-2 text-xs text-error">{error}</p>
        )}

        {!isLoading && (offers ?? []).length === 0 && (
          <p className="py-4 text-center text-sm text-muted">
            {t("No offers yet — offers are made to applications at the Selected stage.")}
          </p>
        )}

        {(offers ?? []).map((o) => (
          <div key={o.id} className="rounded-lg border border-border p-3">
            <div className="flex flex-wrap items-center gap-2">
              <BadgeDollarSign size={15} className="shrink-0 text-primary" />
              <span className="text-sm font-semibold text-foreground">{o.offerNumber}</span>
              <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[o.status ?? ""] ?? ""}`}>
                {t(o.status ?? "")}
              </span>
              {o.awaitingWorkflow && (
                <span className="rounded bg-info/15 px-2 py-0.5 text-[11px] text-info">
                  {t("Awaiting workflow approval")}
                </span>
              )}
              <span className="text-xs text-muted">
                {Number(o.salary).toLocaleString()} ETB · {t("start")} {fmtDate(o.proposedStartDate)} ·{" "}
                {t("valid until")} {fmtDate(o.expiryDate)}
              </span>
              <span className="ml-auto inline-flex items-center gap-1">
                {o.status === "Draft" && (
                  <>
                    <button
                      type="button"
                      title={t("Edit")}
                      onClick={() => {
                        setEditing(o);
                        setShowForm(true);
                      }}
                      className="rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
                    >
                      <Pencil size={13} />
                    </button>
                    <button
                      type="button"
                      title={t("Generate standard letter (HC111)")}
                      onClick={() => generateLetter(o)}
                      className="rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
                    >
                      <FileText size={13} />
                    </button>
                    <button
                      type="button"
                      disabled={busy}
                      onClick={() => o.id && run(() => submitJobOffer(o.id!))}
                      className="rounded bg-primary px-2.5 py-1 text-xs font-semibold text-on-accent disabled:opacity-50"
                    >
                      {t("Submit for Approval")}
                    </button>
                    <button
                      type="button"
                      title={t("Delete draft")}
                      onClick={() => o.id && removeDraft(o.id)}
                      className="rounded border border-border px-2 py-1 text-xs text-muted hover:border-error hover:text-error"
                    >
                      <Trash2 size={13} />
                    </button>
                  </>
                )}
                {o.status === "Approved" && (
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => o.id && run(() => sendJobOffer(o.id!))}
                    className="inline-flex items-center gap-1 rounded bg-success px-2.5 py-1 text-xs font-semibold text-on-accent disabled:opacity-50"
                  >
                    <Send size={12} /> {t("Send to Candidate")}
                  </button>
                )}
                {o.status === "Sent" && (
                  <>
                    <button
                      type="button"
                      onClick={() => setRespondFor({ offer: o, response: "Accept" })}
                      className="inline-flex items-center gap-1 rounded border border-success/40 bg-success/10 px-2.5 py-1 text-xs font-semibold text-success hover:bg-success/20"
                    >
                      <CheckCircle2 size={12} /> {t("Accepted")}
                    </button>
                    <button
                      type="button"
                      onClick={() => setRespondFor({ offer: o, response: "Decline" })}
                      className="inline-flex items-center gap-1 rounded border border-error/40 bg-error/10 px-2.5 py-1 text-xs font-semibold text-error hover:bg-error/20"
                    >
                      <XCircle size={12} /> {t("Declined")}
                    </button>
                  </>
                )}
                {ACTIVE.includes(o.status ?? "") && o.status !== "Draft" && !o.awaitingWorkflow && (
                  <button
                    type="button"
                    title={t("Withdraw offer")}
                    disabled={busy}
                    onClick={() => o.id && run(() => withdrawJobOffer(o.id!))}
                    className="rounded border border-border px-2 py-1 text-xs text-muted hover:border-error hover:text-error disabled:opacity-50"
                  >
                    <Undo2 size={13} />
                  </button>
                )}
              </span>
            </div>
            <div className="mt-1 flex flex-wrap gap-3 text-xs text-muted">
              {o.hiringManagerName && <span>{t("Hiring manager")}: {o.hiringManagerName}</span>}
              {o.salaryScaleAmount != null && (
                <span>
                  {t("Scale")}: {o.salaryScaleAmount.toLocaleString()} ETB
                  {o.salaryJustification && ` — ${o.salaryJustification}`}
                </span>
              )}
              {o.sentAt && <span>{t("Sent")}: {fmtDate(o.sentAt)}</span>}
              {o.respondedAt && (
                <span>
                  {t("Responded")}: {fmtDate(o.respondedAt)}
                  {o.responseNote && ` — ${o.responseNote}`}
                </span>
              )}
            </div>
            {o.letterText && (
              <details className="mt-2">
                <summary className="cursor-pointer text-xs font-medium text-primary">{t("Offer letter")}</summary>
                <pre className="mt-1 whitespace-pre-wrap rounded-md bg-secondary/50 p-2 font-mono text-xs text-foreground">
                  {o.letterText}
                </pre>
              </details>
            )}
          </div>
        ))}

        {showForm ? (
          <OfferForm
            applicationId={applicationId}
            editing={editing}
            onClose={() => {
              setShowForm(false);
              setEditing(null);
            }}
            onDone={async () => {
              setShowForm(false);
              setEditing(null);
              await refresh();
            }}
          />
        ) : (
          !hasActive &&
          (canCreate ? (
            <button
              type="button"
              onClick={() => setShowForm(true)}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
            >
              <BadgeDollarSign size={14} /> {t("New Offer")}
            </button>
          ) : (
            <p className="text-xs italic text-muted">
              {t("This application is final — the offer record is view-only.")}
            </p>
          ))
        )}

        {/* Response confirmation (HC114) */}
        {respondFor && (
          <div className="space-y-2 rounded-lg border border-warning/40 bg-warning/5 p-3">
            <p className="text-sm text-foreground">
              {respondFor.response === "Accept"
                ? t("Record that the candidate ACCEPTED offer {{n}}?", { n: respondFor.offer.offerNumber })
                : t("Record that the candidate DECLINED offer {{n}}?", { n: respondFor.offer.offerNumber })}
            </p>
            <input
              type="text"
              value={responseNote}
              onChange={(e) => setResponseNote(e.target.value)}
              placeholder={t("Response note (optional)")}
              className={inputCls}
            />
            <div className="flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setRespondFor(null)}
                className="rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:bg-secondary"
              >
                {t("Cancel")}
              </button>
              <button
                type="button"
                disabled={busy}
                onClick={confirmResponse}
                className={`rounded-md px-3 py-1.5 text-xs font-semibold text-on-accent disabled:opacity-50 ${
                  respondFor.response === "Accept" ? "bg-success" : "bg-error"
                }`}
              >
                {t("Confirm")}
              </button>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
}

export default OfferModal;
