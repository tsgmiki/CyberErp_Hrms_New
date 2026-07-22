"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ShieldCheck, Ban } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import ButtonField from "@/components/ui/buttonField";
import Loading from "../../common/loader/loader";
import { getLoan, cancelLoan, consentLoan } from "@/services/admin/loan";
import { loanStatusBadge, DetailSection, LoanSummary, GuarantorList, ScheduleTable } from "../loan/shared";

/** HC252/HC257 — the borrower's own loan detail: balance, schedule, service-commitment consent, cancel. */
function MyLoanDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState(false);

  const { data: loan, isLoading } = useQuery({ queryKey: ["myLoan", id], queryFn: () => getLoan(id) });

  const run = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setBusy(true);
    const r = await fn();
    setBusy(false);
    if (r.ok) {
      queryClient.invalidateQueries({ queryKey: ["myLoans"] });
      queryClient.invalidateQueries({ queryKey: ["myLoan", id] });
      onClose();
    }
  };

  const needsConsent =
    !!loan &&
    (loan.status === "Approved" || loan.status === "Active") &&
    (loan.serviceCommitmentMonths ?? 0) > 0 &&
    !loan.serviceCommitmentConsentAt;

  return (
    <DialogModal title={loan?.loanNumber ?? t("Loan")} visible onClose={onClose} hideOk cancelLabel="Close">
      {isLoading || !loan ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          {/* Header strip: type · purpose · status */}
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-foreground">{loan.loanTypeName}</p>
              {loan.purpose ? <p className="truncate text-xs text-muted">{loan.purpose}</p> : null}
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

          {needsConsent && (
            <div className="rounded-lg border border-primary/40 bg-primary/5 p-3">
              <p className="text-sm font-medium text-foreground">
                {t("This loan carries a {{n}}-month service commitment.", { n: loan.serviceCommitmentMonths })}
              </p>
              <p className="mb-3 mt-0.5 text-xs text-muted">
                {t("By consenting you agree to remain in service for the commitment period after disbursement.")}
              </p>
              <ButtonField
                value="I consent to the service commitment"
                variant="primary"
                icon={<ShieldCheck size={15} />}
                disabled={busy}
                onClick={() => run(() => consentLoan(id))}
              />
            </div>
          )}

          {loan.status === "Requested" && (
            <div className="flex justify-end border-t border-border pt-3">
              <ButtonField
                value="Cancel this request"
                variant="danger"
                icon={<Ban size={15} />}
                disabled={busy}
                onClick={() => run(() => cancelLoan(id))}
              />
            </div>
          )}
        </div>
      )}
    </DialogModal>
  );
}

export default memo(MyLoanDetailModal);
