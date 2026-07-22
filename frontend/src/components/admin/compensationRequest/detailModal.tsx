"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, XCircle, Eye } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import DetailSection from "@/components/common/detailSection";
import InputField from "@/components/ui/inputField";
import ButtonField from "@/components/ui/buttonField";
import type { CompensationRequestModel } from "@/models";
import { resolveCompensationRequest } from "@/services/admin/compensation";

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { maximumFractionDigits: 2 });
const STATUS_TONE: Record<string, string> = {
  Submitted: "bg-info/15 text-info",
  UnderReview: "bg-warning/15 text-warning",
  Resolved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
};

function Field({ label, v }: { label: string; v?: string | null }) {
  return (
    <div>
      <p className="text-[11px] uppercase tracking-wide text-muted">{label}</p>
      <p className="font-medium text-foreground">{v || "—"}</p>
    </div>
  );
}

/** HC234 — compensation request review: HR resolves / rejects / marks under review. */
function CompensationRequestDetailModal({ record, onClose }: { record: CompensationRequestModel; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [resolution, setResolution] = useState(record.resolution ?? "");
  const [busy, setBusy] = useState(false);

  const isOpen = record.status === "Submitted" || record.status === "UnderReview";

  const resolve = async (status: string, close = true) => {
    if (!record.id) return;
    setBusy(true);
    const r = await resolveCompensationRequest(record.id, status, resolution || undefined);
    setBusy(false);
    if (r.ok) {
      queryClient.invalidateQueries({ queryKey: ["compensationRequests"] });
      if (close) onClose();
    }
  };

  return (
    <DialogModal title={record.subject ?? t("Compensation Request")} visible onClose={onClose} hideOk cancelLabel="Close">
      <div className="space-y-3">
        <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold text-foreground">{record.employeeName}</p>
            <p className="truncate text-xs text-muted">{record.requestType === "PayrollDiscrepancy" ? t("Discrepancy") : t("Benefit change")}</p>
          </div>
          <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${STATUS_TONE[record.status ?? ""] ?? "bg-muted/30 text-muted"}`}>{t(record.status ?? "")}</span>
        </div>

        <DetailSection title="Request Details">
          <div className="mb-2 grid grid-cols-2 gap-2 text-sm">
            <Field label={t("Reference period")} v={record.referencePeriod} />
            <Field label={t("Disputed amount")} v={record.disputedAmount != null ? money(record.disputedAmount) : "—"} />
            {record.benefitPlanName && <Field label={t("Plan")} v={record.benefitPlanName} />}
          </div>
          <p className="whitespace-pre-wrap rounded-md bg-secondary/20 px-3 py-2 text-sm text-foreground">{record.details}</p>
        </DetailSection>

        {record.resolution && record.status !== "Submitted" && (
          <DetailSection title="Resolution">
            <p className="text-sm text-foreground">{record.resolution}</p>
          </DetailSection>
        )}

        {isOpen && (
          <DetailSection title="Resolve">
            <div className="space-y-2">
              <InputField type="text" name="resolution" label="" placeholder={t("Resolution / reason") ?? ""} value={resolution} onChange={(e: any) => setResolution(e.target.value)} />
              <div className="flex flex-wrap justify-end gap-2">
                <ButtonField value="Under review" variant="outline" icon={<Eye size={15} />} disabled={busy} onClick={() => resolve("UnderReview", false)} />
                <ButtonField value="Reject" variant="danger" icon={<XCircle size={15} />} disabled={busy} onClick={() => resolve("Rejected")} />
                <ButtonField value="Resolve" variant="primary" icon={<CheckCircle2 size={15} />} disabled={busy} onClick={() => resolve("Resolved")} />
              </div>
            </div>
          </DetailSection>
        )}
      </div>
    </DialogModal>
  );
}

export default memo(CompensationRequestDetailModal);
