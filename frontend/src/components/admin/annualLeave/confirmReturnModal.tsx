"use client";

import { memo, useEffect, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Undo2, AlertTriangle, CheckCircle2 } from "lucide-react";
import Modal from "@/components/ui/modal";
import ButtonField from "@/components/ui/buttonField";
import { toast } from "@/components/common/toast";
import {
  previewAnnualLeaveReturn, confirmAnnualLeaveReturn,
} from "@/services/admin/annualLeave/returnFromLeave";
import type { AnnualLeaveModel, AnnualLeaveReturnPreviewModel } from "@/models";

interface Props {
  request: AnnualLeaveModel;
  onClose: () => void;
}

const INPUT =
  "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

const day = (v?: string | null) => (v ? String(v).slice(0, 10) : "");

/**
 * "I'm back" — the employee confirms their return date.
 *
 * <p>The consequence is computed SERVER-SIDE as the date changes ({@link previewAnnualLeaveReturn}),
 * because only the working calendar knows what a date costs: coming back two days early over a
 * weekend costs nothing, the same two days midweek costs two. Showing that before they commit is the
 * difference between an informed confirmation and a surprise.</p>
 *
 * <p>Three outcomes, mirroring the API: on time settles immediately; early and late both need a
 * comment and go back through approval.</p>
 */
function ConfirmReturnModal({ request, onClose }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  // Default to the approved last day — the common case is "I came back as planned".
  const [actualEndDate, setActualEndDate] = useState(day(request.plannedEndDate));
  const [comment, setComment] = useState("");
  const [preview, setPreview] = useState<AnnualLeaveReturnPreviewModel | null>(null);
  const [previewError, setPreviewError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (!actualEndDate || !request.id) { setPreview(null); return; }
    let cancelled = false;
    // A stale reply must never overwrite a newer one — the user can change the date faster than the
    // round trip returns.
    previewAnnualLeaveReturn(request.id, actualEndDate)
      .then((p) => { if (!cancelled) { setPreview(p); setPreviewError(null); } })
      .catch(() => { if (!cancelled) { setPreview(null); setPreviewError(t("Could not work out that date.")); } });
    return () => { cancelled = true; };
  }, [actualEndDate, request.id, t]);

  const type = preview?.returnType ?? "OnTime";
  const adj = preview?.adjustmentDays ?? 0;
  const needsComment = preview?.commentRequired === true;
  const canSubmit =
    !!actualEndDate && !!preview && !isSaving && (!needsComment || comment.trim().length > 0);

  const submit = async () => {
    if (!request.id || !canSubmit) return;
    setIsSaving(true);
    try {
      const res = await confirmAnnualLeaveReturn({
        annualLeaveHeaderId: request.id,
        actualEndDate,
        comment: comment.trim() || undefined,
      });
      toast.success(res.message ?? t("Return confirmed"));
      // Both the list and any open detail reflect the new status.
      queryClient.invalidateQueries({ queryKey: ["annualLeaves"] });
      queryClient.invalidateQueries({ queryKey: ["annualLeaveHistory", request.id] });
      onClose();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : t("Could not confirm the return."));
      setIsSaving(false);
    }
  };

  const tone =
    type === "Early" ? "border-info/40 bg-info/10 text-info"
    : type === "Late" ? "border-warning/40 bg-warning/10 text-warning"
    : "border-success/40 bg-success/10 text-success";

  return (
    <Modal isOpen onClose={onClose} size="md" title={t("Confirm return from leave") ?? undefined}>
      <div className="space-y-3">
        <div className="rounded-md border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">
          {t("Approved")}:{" "}
          <span className="font-semibold text-foreground">
            {request.totalLeaveDays} {t("day(s)")}
          </span>
          {request.plannedEndDate ? (
            <> · {t("last approved day")}{" "}
              <span className="font-semibold tabular-nums text-foreground">{day(request.plannedEndDate)}</span>
            </>
          ) : null}
        </div>

        <div>
          <label className={LABEL}>{t("Last day actually on leave")} *</label>
          <input
            type="date"
            className={INPUT}
            value={actualEndDate}
            onChange={(e) => setActualEndDate(e.target.value)}
          />
          <p className="mt-1 text-[11px] text-muted">
            {t("The day before you resumed work.")}
          </p>
        </div>

        {previewError && <p className="text-xs text-error">{previewError}</p>}

        {preview && (
          <div className={`rounded-md border px-3 py-2 text-xs ${tone}`}>
            <p className="flex items-center gap-1.5 font-semibold">
              {type === "OnTime" ? <CheckCircle2 size={13} /> : <Undo2 size={13} />}
              {type === "OnTime" && t("Returning as approved")}
              {type === "Early" && `${t("Returning early")} — ${Math.abs(adj)} ${t("day(s) fewer")}`}
              {type === "Late" && `${t("Returning late")} — ${adj} ${t("extra day(s)")}`}
            </p>
            <p className="mt-1 text-foreground">
              {t("Days taken")}:{" "}
              <span className="font-semibold tabular-nums">{preview.actualDays}</span>
              {" "}({t("approved")} {preview.approvedDays})
            </p>
            <p className="mt-1">
              {type === "OnTime"
                ? t("These days are already deducted, so this closes the request immediately.")
                : type === "Early"
                  ? t("The unused days are credited back only once an approver accepts this.")
                  : t("The extra days are deducted only once an approver accepts this.")}
            </p>
            {preview.warning && (
              <p className="mt-1 flex items-start gap-1 font-semibold">
                <AlertTriangle size={13} className="mt-0.5 shrink-0" /> {preview.warning}
              </p>
            )}
          </div>
        )}

        {needsComment && (
          <div>
            <label className={LABEL}>{t("Reason for the difference")} *</label>
            <textarea
              rows={3}
              className={INPUT}
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder={t("Explain why you returned on a different date") ?? ""}
            />
            <p className="mt-1 text-[11px] text-muted">
              {t("This is what the approver reads when deciding, so be specific.")}
            </p>
          </div>
        )}

        <div className="flex items-center justify-end gap-2 pt-1">
          <ButtonField value="Cancel" variant="outline" onClick={onClose} />
          <ButtonField
            value={preview?.requiresApproval ? "Submit for approval" : "Confirm return"}
            variant="primary"
            disabled={!canSubmit}
            onClick={submit}
          />
        </div>
      </div>
    </Modal>
  );
}

export default memo(ConfirmReturnModal);
