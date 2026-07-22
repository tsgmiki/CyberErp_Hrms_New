"use client";
import { memo } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Paperclip } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import DetailSection from "@/components/common/detailSection";
import Loading from "../../common/loader/loader";
import { getMedicalClaim, downloadMedicalAttachment } from "@/services/admin/medical";
import { money, medicalClaimStatusBadge } from "./shared";

function Field({ label, value }: { label: string; value?: string | null }) {
  const { t } = useTranslation();
  return (
    <div>
      <p className="text-[11px] uppercase tracking-wide text-muted">{t(label)}</p>
      <p className="font-medium text-foreground">{value || "—"}</p>
    </div>
  );
}

/** HC240 — the claimant's own read-only medical claim detail (status, amounts, attachments). */
function MyMedicalClaimDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const { data: claim, isLoading } = useQuery({ queryKey: ["myMedicalClaim", id], queryFn: () => getMedicalClaim(id) });

  return (
    <DialogModal title={claim?.claimNumber ?? t("Medical Claim")} visible onClose={onClose} hideOk cancelLabel="Close">
      {isLoading || !claim ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          {/* Header strip: beneficiary · plan · status */}
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-foreground">
                {claim.beneficiaryName} <span className="text-xs font-normal text-muted">({t(claim.beneficiaryCategory ?? "")})</span>
              </p>
              {claim.medicalPlanName ? <p className="truncate text-xs text-muted">{claim.medicalPlanName}</p> : null}
            </div>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${medicalClaimStatusBadge(claim.status)}`}>{t(claim.status ?? "")}</span>
          </div>

          <DetailSection title="Claim Details">
            <div className="grid grid-cols-2 gap-3 text-sm">
              <Field label="Provider" value={claim.providerName} />
              <Field label="Service date" value={claim.serviceDate?.slice(0, 10)} />
              <Field label="Claimed" value={money(claim.claimedAmount)} />
              <Field label="Approved" value={claim.approvedAmount == null ? "—" : money(claim.approvedAmount)} />
              <Field label="Diagnosis" value={claim.diagnosis} />
              <Field label="Payment ref" value={claim.paymentReference} />
              {claim.description ? (
                <div className="col-span-2">
                  <p className="text-[11px] uppercase tracking-wide text-muted">{t("Description")}</p>
                  <p className="font-medium text-foreground">{claim.description}</p>
                </div>
              ) : null}
              {claim.resolution ? (
                <div className="col-span-2">
                  <p className="text-[11px] uppercase tracking-wide text-muted">{t("Resolution")}</p>
                  <p className="font-medium text-foreground">{claim.resolution}</p>
                </div>
              ) : null}
            </div>
          </DetailSection>

          {(claim.attachments?.length ?? 0) > 0 && (
            <DetailSection title="Attachments">
              <div className="flex flex-wrap gap-2">
                {claim.attachments!.map((a) => (
                  <button
                    key={a.id}
                    type="button"
                    onClick={() => downloadMedicalAttachment(a.id!, a.fileName ?? "attachment")}
                    className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs hover:bg-secondary/30"
                  >
                    <Paperclip size={12} /> {a.fileName}
                  </button>
                ))}
              </div>
            </DetailSection>
          )}
        </div>
      )}
    </DialogModal>
  );
}

export default memo(MyMedicalClaimDetailModal);
