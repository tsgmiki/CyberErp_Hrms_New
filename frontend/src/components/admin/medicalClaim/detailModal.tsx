"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, XCircle, Banknote, Paperclip } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import DetailSection from "@/components/common/detailSection";
import InputField from "@/components/ui/inputField";
import ButtonField from "@/components/ui/buttonField";
import Loading from "../../common/loader/loader";
import { getMedicalClaim, approveMedicalClaim, rejectMedicalClaim, payMedicalClaim, downloadMedicalAttachment } from "@/services/admin/medical";
import { money, medicalStatusBadge } from "./shared";

function Field({ label, v }: { label: string; v?: string | null }) {
  return (
    <div>
      <p className="text-[11px] uppercase tracking-wide text-muted">{label}</p>
      <p className="font-medium text-foreground">{v || "—"}</p>
    </div>
  );
}

/** HC239–246 — HR medical-claim detail + actions: approve / reject / reimburse. */
function MedicalClaimDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [approvedAmount, setApprovedAmount] = useState("");
  const [note, setNote] = useState("");
  const [reason, setReason] = useState("");
  const [reference, setReference] = useState("");
  const [busy, setBusy] = useState(false);

  const { data: claim, isLoading } = useQuery({ queryKey: ["medicalClaim", id], queryFn: () => getMedicalClaim(id) });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["medicalClaims"] });
    queryClient.invalidateQueries({ queryKey: ["medicalClaim", id] });
  };
  const run = async (fn: () => Promise<{ ok: boolean; message: string }>, close = false) => {
    setBusy(true);
    const r = await fn();
    setBusy(false);
    if (r.ok) { invalidate(); if (close) onClose(); }
  };

  const canReview = claim?.status === "Pending" || claim?.status === "UnderReview";
  const amountOrDefault = () => (approvedAmount !== "" ? approvedAmount : String(claim?.claimedAmount ?? ""));

  return (
    <DialogModal title={claim?.claimNumber ?? t("Medical Claim")} visible onClose={onClose} hideOk cancelLabel="Close">
      {isLoading || !claim ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-foreground">{claim.employeeName}</p>
              <p className="truncate text-xs text-muted">{claim.beneficiaryName} · {t(claim.beneficiaryCategory ?? "")}</p>
            </div>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${medicalStatusBadge(claim.status)}`}>{t(claim.status ?? "")}</span>
          </div>

          <DetailSection title="Claim Details">
            <div className="grid grid-cols-2 gap-2 text-sm">
              <Field label={t("Plan")} v={claim.medicalPlanName} />
              <Field label={t("Provider")} v={claim.providerName} />
              <Field label={t("Source")} v={t(claim.source ?? "")} />
              <Field label={t("Service date")} v={claim.serviceDate?.slice(0, 10)} />
              <Field label={t("Claimed")} v={money(claim.claimedAmount)} />
              <Field label={t("Approved")} v={claim.approvedAmount == null ? "—" : money(claim.approvedAmount)} />
              {claim.diagnosis && <Field label={t("Diagnosis")} v={claim.diagnosis} />}
              {claim.paymentReference && <Field label={t("Payment ref")} v={claim.paymentReference} />}
            </div>
            {claim.description && <p className="mt-2 text-sm"><span className="text-muted">{t("Description")}: </span>{claim.description}</p>}
            {claim.resolution && <p className="mt-1 text-sm"><span className="text-muted">{t("Resolution")}: </span>{claim.resolution}</p>}
          </DetailSection>

          {(claim.attachments?.length ?? 0) > 0 && (
            <DetailSection title="Attachments">
              <div className="flex flex-wrap gap-2">
                {claim.attachments!.map((a) => (
                  <button key={a.id} type="button" onClick={() => downloadMedicalAttachment(a.id!, a.fileName ?? "attachment")} className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs hover:bg-secondary/30">
                    <Paperclip size={12} /> {a.fileName}
                  </button>
                ))}
              </div>
            </DetailSection>
          )}

          {canReview && (
            <DetailSection title="Review">
              <div className="space-y-2">
                <div className="flex items-end gap-2">
                  <div className="w-40"><InputField type="text" inputType="number" name="approvedAmount" label="" placeholder={t("Approved amount") ?? ""} value={amountOrDefault()} onChange={(e: any) => setApprovedAmount(e.target.value)} /></div>
                  <div className="flex-1"><InputField type="text" name="note" label="" placeholder={t("Approval note (optional)") ?? ""} value={note} onChange={(e: any) => setNote(e.target.value)} /></div>
                  <ButtonField value="Approve" variant="primary" icon={<CheckCircle2 size={15} />} disabled={busy} onClick={() => run(() => approveMedicalClaim(id, amountOrDefault() === "" ? undefined : Number(amountOrDefault()), note || undefined), true)} />
                </div>
                <div className="flex items-end gap-2">
                  <div className="flex-1"><InputField type="text" name="reason" label="" placeholder={t("Rejection reason") ?? ""} value={reason} onChange={(e: any) => setReason(e.target.value)} /></div>
                  <ButtonField value="Reject" variant="danger" icon={<XCircle size={15} />} disabled={busy || !reason.trim()} onClick={() => run(() => rejectMedicalClaim(id, reason), true)} />
                </div>
              </div>
            </DetailSection>
          )}

          {claim.status === "Approved" && (
            <DetailSection title="Reimbursement">
              <div className="flex items-end gap-2">
                <div className="flex-1"><InputField type="text" name="reference" label="" placeholder={t("Payment reference") ?? ""} value={reference} onChange={(e: any) => setReference(e.target.value)} /></div>
                <ButtonField value="Mark reimbursed" variant="primary" icon={<Banknote size={15} />} disabled={busy} onClick={() => run(() => payMedicalClaim(id, reference || undefined), true)} />
              </div>
            </DetailSection>
          )}
        </div>
      )}
    </DialogModal>
  );
}

export default memo(MedicalClaimDetailModal);
