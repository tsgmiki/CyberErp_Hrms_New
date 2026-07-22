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
import {
  getInsuranceClaim,
  approveInsuranceClaim,
  rejectInsuranceClaim,
  payInsuranceClaim,
  downloadInsuranceAttachment,
} from "@/services/admin/insurance";

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const statusBadge = (s?: string) =>
  ({
    Pending: "bg-warning/15 text-warning",
    UnderReview: "bg-primary/15 text-primary",
    Approved: "bg-success/15 text-success",
    Rejected: "bg-error/15 text-error",
    Paid: "bg-secondary/40 text-foreground",
  }[s ?? ""] ?? "bg-muted/30 text-muted");

function Pair({ label, v }: { label: string; v?: string | null }) {
  return (
    <div>
      <p className="text-[11px] uppercase tracking-wide text-muted">{label}</p>
      <p className="font-medium text-foreground">{v || "—"}</p>
    </div>
  );
}

/** HC248/HC249 — HR insurance claim detail + actions: approve/reject, mark reimbursed, download attachments. */
function InsuranceClaimDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [approvedAmount, setApprovedAmount] = useState("");
  const [note, setNote] = useState("");
  const [reason, setReason] = useState("");
  const [reference, setReference] = useState("");
  const [busy, setBusy] = useState(false);

  const { data: claim, isLoading } = useQuery({ queryKey: ["insuranceClaim", id], queryFn: () => getInsuranceClaim(id) });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["insuranceClaims"] });
    queryClient.invalidateQueries({ queryKey: ["insuranceClaim", id] });
  };
  const run = async (fn: () => Promise<{ ok: boolean; message: string }>, close = false) => {
    setBusy(true);
    const r = await fn();
    setBusy(false);
    if (r.ok) {
      invalidate();
      if (close) onClose();
    }
  };

  const canReview = claim?.status === "Pending" || claim?.status === "UnderReview";
  const effectiveApproved = approvedAmount === "" ? claim?.approvedAmount ?? claim?.claimedAmount : Number(approvedAmount);

  return (
    <DialogModal title={claim?.claimNumber ?? t("Insurance Claim")} visible onClose={onClose} hideOk cancelLabel="Close">
      {isLoading || !claim ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-foreground">{claim.employeeName}</p>
              <p className="truncate text-xs text-muted">{claim.policyNumber}{claim.insurerName ? ` · ${claim.insurerName}` : ""}</p>
            </div>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${statusBadge(claim.status)}`}>{t(claim.status ?? "")}</span>
          </div>

          <DetailSection title="Claim Details">
            <div className="grid grid-cols-2 gap-3">
              <Pair label={t("Employee")} v={claim.employeeName} />
              <Pair label={t("Policy")} v={`${claim.policyNumber ?? ""}${claim.insurerName ? ` (${claim.insurerName})` : ""}`} />
              <Pair label={t("Claim type")} v={claim.claimType} />
              <Pair label={t("Incident date")} v={claim.incidentDate?.slice(0, 10)} />
              <Pair label={t("Claimed")} v={money(claim.claimedAmount)} />
              <Pair label={t("Approved")} v={claim.approvedAmount == null ? "—" : money(claim.approvedAmount)} />
              {claim.paymentReference && <Pair label={t("Payment ref")} v={claim.paymentReference} />}
              {claim.description && <Pair label={t("Description")} v={claim.description} />}
              {claim.resolution && <Pair label={t("Resolution")} v={claim.resolution} />}
            </div>
          </DetailSection>

          {(claim.attachments?.length ?? 0) > 0 && (
            <DetailSection title="Attachments">
              <div className="flex flex-wrap gap-2">
                {claim.attachments!.map((a) => (
                  <ButtonField
                    key={a.id}
                    value={a.fileName ?? "attachment"}
                    variant="outline"
                    icon={<Paperclip size={13} />}
                    onClick={() => downloadInsuranceAttachment(a.id!, a.fileName ?? "attachment")}
                  />
                ))}
              </div>
            </DetailSection>
          )}

          {canReview && (
            <DetailSection title="Review">
              <div className="space-y-2">
                <InputField type="text" inputType="number" name="approvedAmount" label="Approved amount" placeholder={t("Approved amount") ?? ""} value={approvedAmount} onChange={(e: any) => setApprovedAmount(e.target.value)} />
                <InputField type="text" name="note" label="Note (optional)" placeholder={t("Approval note (optional)") ?? ""} value={note} onChange={(e: any) => setNote(e.target.value)} />
                <div className="flex justify-end">
                  <ButtonField value="Approve" variant="primary" icon={<CheckCircle2 size={15} />} disabled={busy} onClick={() => run(() => approveInsuranceClaim(id, effectiveApproved == null ? undefined : Number(effectiveApproved), note || undefined), true)} />
                </div>
                <InputField type="text" name="reason" label="Rejection reason" placeholder={t("Rejection reason") ?? ""} value={reason} onChange={(e: any) => setReason(e.target.value)} />
                <div className="flex justify-end">
                  <ButtonField value="Reject" variant="danger" icon={<XCircle size={15} />} disabled={busy || !reason.trim()} onClick={() => run(() => rejectInsuranceClaim(id, reason), true)} />
                </div>
              </div>
            </DetailSection>
          )}

          {claim.status === "Approved" && (
            <DetailSection title="Reimbursement">
              <div className="space-y-2">
                <InputField type="text" name="reference" label="Payment reference" placeholder={t("Payment reference (CBS)") ?? ""} value={reference} onChange={(e: any) => setReference(e.target.value)} />
                <div className="flex justify-end">
                  <ButtonField value="Mark reimbursed" variant="primary" icon={<Banknote size={15} />} disabled={busy} onClick={() => run(() => payInsuranceClaim(id, reference || undefined), true)} />
                </div>
              </div>
            </DetailSection>
          )}
        </div>
      )}
    </DialogModal>
  );
}

export default memo(InsuranceClaimDetailModal);
