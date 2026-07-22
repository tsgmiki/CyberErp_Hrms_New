"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, XCircle, Banknote, TrendingUp } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import InputField from "@/components/ui/inputField";
import ButtonField from "@/components/ui/buttonField";
import Loading from "../../common/loader/loader";
import { getLoan, approveLoan, rejectLoan, disburseLoan, repayLoan, incrementInstallment } from "@/services/admin/loan";
import { loanStatusBadge, DetailSection, LoanSummary, GuarantorList, ScheduleTable } from "./shared";

/** HC252–259 — HR loan detail + actions: approve/reject, disburse, record repayment, raise installment. */
function LoanDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [note, setNote] = useState("");
  const [reason, setReason] = useState("");
  const [reference, setReference] = useState("");
  const [amount, setAmount] = useState("");
  const [newMonthly, setNewMonthly] = useState("");
  const [busy, setBusy] = useState(false);

  const { data: loan, isLoading } = useQuery({ queryKey: ["loan", id], queryFn: () => getLoan(id) });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["loans"] });
    queryClient.invalidateQueries({ queryKey: ["loan", id] });
  };
  const run = async (fn: () => Promise<{ ok: boolean; message: string }>, close = false) => {
    setBusy(true);
    const r = await fn();
    setBusy(false);
    if (r.ok) { invalidate(); if (close) onClose(); }
  };

  return (
    <DialogModal title={loan?.loanNumber ?? t("Loan")} visible onClose={onClose} hideOk cancelLabel="Close">
      {isLoading || !loan ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-foreground">{loan.employeeName}</p>
              <p className="truncate text-xs text-muted">{loan.loanTypeName}{loan.purpose ? ` · ${loan.purpose}` : ""}</p>
            </div>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${loanStatusBadge(loan.status)}`}>{t(loan.status ?? "")}</span>
          </div>

          <DetailSection title="Loan Summary">
            <LoanSummary loan={loan} />
          </DetailSection>

          {(loan.guarantors?.length ?? 0) > 0 && (
            <DetailSection title="Guarantors">
              <GuarantorList loan={loan} />
            </DetailSection>
          )}

          {(loan.schedule?.length ?? 0) > 0 && (
            <DetailSection title="Repayment Schedule">
              <ScheduleTable loan={loan} />
            </DetailSection>
          )}

          {loan.status === "Requested" && (
            <DetailSection title="Endorsement">
              <div className="space-y-2">
                <div className="flex items-end gap-2">
                  <div className="flex-1"><InputField type="text" name="note" label="" placeholder={t("Endorsement note (optional)") ?? ""} value={note} onChange={(e: any) => setNote(e.target.value)} /></div>
                  <ButtonField value="Approve" variant="primary" icon={<CheckCircle2 size={15} />} disabled={busy} onClick={() => run(() => approveLoan(id, note || undefined), true)} />
                </div>
                <div className="flex items-end gap-2">
                  <div className="flex-1"><InputField type="text" name="reason" label="" placeholder={t("Rejection reason") ?? ""} value={reason} onChange={(e: any) => setReason(e.target.value)} /></div>
                  <ButtonField value="Reject" variant="danger" icon={<XCircle size={15} />} disabled={busy || !reason.trim()} onClick={() => run(() => rejectLoan(id, reason), true)} />
                </div>
              </div>
            </DetailSection>
          )}

          {loan.status === "Approved" && (
            <DetailSection title="Disbursement">
              <div className="flex items-end gap-2">
                <div className="flex-1"><InputField type="text" name="reference" label="" placeholder={t("Disbursement reference (CBS)") ?? ""} value={reference} onChange={(e: any) => setReference(e.target.value)} /></div>
                <ButtonField value="Disburse" variant="primary" icon={<Banknote size={15} />} disabled={busy} onClick={() => run(() => disburseLoan(id, reference || undefined), true)} />
              </div>
            </DetailSection>
          )}

          {loan.status === "Active" && (
            <DetailSection title="Repayment">
              <div className="space-y-2">
                <div className="flex items-end gap-2">
                  <div className="flex-1"><InputField type="text" inputType="number" name="amount" label="" placeholder={t("Record repayment") ?? ""} value={amount} onChange={(e: any) => setAmount(e.target.value)} /></div>
                  <ButtonField value="Repay" variant="primary" icon={<Banknote size={15} />} disabled={busy || amount === ""} onClick={() => run(() => repayLoan(id, Number(amount)).then((r) => { if (r.ok) setAmount(""); return r; }))} />
                </div>
                <div className="flex items-end gap-2">
                  <div className="flex-1"><InputField type="text" inputType="number" name="newMonthly" label="" placeholder={t("Raise monthly installment") ?? ""} value={newMonthly} onChange={(e: any) => setNewMonthly(e.target.value)} /></div>
                  <ButtonField value="Increase" variant="outline" icon={<TrendingUp size={15} />} disabled={busy || newMonthly === ""} onClick={() => run(() => incrementInstallment(id, Number(newMonthly)).then((r) => { if (r.ok) setNewMonthly(""); return r; }))} />
                </div>
              </div>
            </DetailSection>
          )}
        </div>
      )}
    </DialogModal>
  );
}

export default memo(LoanDetailModal);
