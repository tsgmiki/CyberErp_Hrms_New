"use client";
import { memo } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Download } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import ButtonField from "@/components/ui/buttonField";
import DetailSection from "@/components/common/detailSection";
import Loading from "../../common/loader/loader";
import { getInsuranceClaim, downloadInsuranceAttachment } from "@/services/admin/insurance";

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const statusBadge = (s?: string) =>
  ({ Pending: "bg-warning/15 text-warning", UnderReview: "bg-primary/15 text-primary", Approved: "bg-success/15 text-success", Rejected: "bg-error/15 text-error", Paid: "bg-secondary/40 text-foreground" }[s ?? ""] ?? "bg-muted/30 text-muted");

/** HC248 — the employee's own insurance claim detail (read-only view + attachment download). */
function MyInsuranceClaimDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const { data: claim, isLoading } = useQuery({ queryKey: ["myInsuranceClaim", id], queryFn: () => getInsuranceClaim(id) });

  return (
    <DialogModal title={claim?.claimNumber ?? t("Insurance Claim")} visible onClose={onClose} hideOk cancelLabel="Close">
      {isLoading || !claim ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          {/* Header strip: policy · status */}
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-foreground">{claim.policyNumber}</p>
              {claim.insurerName ? <p className="truncate text-xs text-muted">{claim.insurerName}</p> : null}
            </div>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${statusBadge(claim.status)}`}>{t(claim.status ?? "")}</span>
          </div>

          <DetailSection title="Claim Details">
            <dl className="grid grid-cols-2 gap-x-4 gap-y-2 text-sm">
              <div><dt className="text-xs text-muted">{t("Claim Type")}</dt><dd className="text-foreground">{t(claim.claimType ?? "")}</dd></div>
              <div><dt className="text-xs text-muted">{t("Incident Date")}</dt><dd className="text-foreground">{claim.incidentDate?.slice(0, 10)}</dd></div>
              <div><dt className="text-xs text-muted">{t("Claimed Amount")}</dt><dd className="tabular-nums text-foreground">{money(claim.claimedAmount)}</dd></div>
              <div><dt className="text-xs text-muted">{t("Approved Amount")}</dt><dd className="tabular-nums text-foreground">{claim.approvedAmount == null ? "—" : money(claim.approvedAmount)}</dd></div>
              {claim.submittedOn ? <div><dt className="text-xs text-muted">{t("Submitted")}</dt><dd className="text-foreground">{claim.submittedOn.slice(0, 10)}</dd></div> : null}
              {claim.paidAt ? <div><dt className="text-xs text-muted">{t("Paid")}</dt><dd className="text-foreground">{claim.paidAt.slice(0, 10)}</dd></div> : null}
              {claim.description ? <div className="col-span-2"><dt className="text-xs text-muted">{t("Description")}</dt><dd className="text-foreground">{claim.description}</dd></div> : null}
              {claim.resolution ? <div className="col-span-2"><dt className="text-xs text-muted">{t("Resolution")}</dt><dd className="text-foreground">{claim.resolution}</dd></div> : null}
              {claim.paymentReference ? <div className="col-span-2"><dt className="text-xs text-muted">{t("Payment Reference")}</dt><dd className="text-foreground">{claim.paymentReference}</dd></div> : null}
            </dl>
          </DetailSection>

          {(claim.attachments?.length ?? 0) > 0 && (
            <DetailSection title="Attachments">
              <div className="flex flex-wrap gap-2">
                {claim.attachments?.map((a) => (
                  <ButtonField
                    key={a.id}
                    value={a.fileName ?? t("Document")}
                    variant="outline"
                    icon={<Download size={14} />}
                    onClick={() => a.id && downloadInsuranceAttachment(a.id, a.fileName ?? "attachment")}
                  />
                ))}
              </div>
            </DetailSection>
          )}
        </div>
      )}
    </DialogModal>
  );
}

export default memo(MyInsuranceClaimDetailModal);
