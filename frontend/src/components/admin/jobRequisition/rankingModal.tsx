"use client";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import { getApplicationRanking } from "@/services/admin/recruitment";

const ordinal = (n: number) => `${n}${n === 1 ? "st" : n === 2 ? "nd" : n === 3 ? "rd" : "th"}`;

const ELIGIBILITY_TONE: Record<string, string> = {
  Eligible: "bg-success/15 text-success",
  Waitlisted: "bg-warning/15 text-warning",
  Hired: "bg-primary/10 text-primary",
  OfferRejected: "bg-error/15 text-error",
  OutOfContention: "bg-muted/30 text-muted",
  FailsMandatory: "bg-error/15 text-error",
  NotScored: "bg-muted/30 text-muted",
};

const ELIGIBILITY_LABEL: Record<string, string> = {
  Eligible: "Eligible to Hire",
  Waitlisted: "Waitlisted",
  Hired: "Hired",
  OfferRejected: "Rejected Offer",
  OutOfContention: "Out of Contention",
  FailsMandatory: "Fails Mandatory",
  NotScored: "Not Scored",
};

/** Final weighted ranking (1st/2nd/3rd) with the top-N hire window and the waitlist. */
function RankingModal({ requisitionId, onClose }: { requisitionId: string; onClose: () => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({
    queryKey: ["applicationRanking", requisitionId],
    queryFn: () => getApplicationRanking(requisitionId),
  });

  // A genuine tie: two or more candidates share the exact same total AND are both hire-eligible.
  // The engine does not break the tie on merit — all are eligible and HR selects within the slots.
  const eligibleTie = (data ?? []).some((r) => r.hireEligibility === "Eligible" && r.tied);

  return (
    <Modal
      visible
      size="xl"
      title={t("Candidate Ranking & Waitlist")}
      description={t("Weighted totals decide the order; candidates who tie on score share a rank. Tied candidates at the cut-off are ALL eligible — the system never breaks a merit tie arbitrarily, so HR selects within the open positions. Waitlisted candidates slide up if someone declines.")}
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
      {isLoading && <Loading />}
      {!isLoading && (data ?? []).length === 0 && (
        <p className="py-6 text-center text-sm text-muted">{t("No applications on this vacancy yet.")}</p>
      )}
      {!isLoading && eligibleTie && (
        <p className="mb-2 rounded-md border border-warning/40 bg-warning/10 px-3 py-2 text-xs text-warning">
          {t("Tied scores at the cut-off: the tied candidates are all marked Eligible. Choose which to advance — the vacancy still closes at its open-position count.")}
        </p>
      )}
      {!isLoading && (data ?? []).length > 0 && (
        <div className="overflow-x-auto">
          <table className="w-full text-[13px]">
            <thead>
              <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                <th className="px-3 py-2 font-semibold">{t("Rank")}</th>
                <th className="px-3 py-2 font-semibold">{t("Candidate")}</th>
                <th className="px-3 py-2 font-semibold">{t("Status")}</th>
                <th className="px-3 py-2 font-semibold">{t("Criteria Scored")}</th>
                <th className="px-3 py-2 font-semibold">{t("Breakdown")}</th>
                <th className="px-3 py-2 text-right font-semibold">{t("Total")}</th>
              </tr>
            </thead>
            <tbody>
              {(data ?? []).map((r) => (
                <tr key={r.applicationId} className="border-b border-border/60">
                  <td className="px-3 py-2">
                    {r.rank ? (
                      <span
                        className={`inline-flex h-7 min-w-7 items-center justify-center rounded-full px-1.5 text-xs font-bold tabular-nums ${
                          r.rank === 1
                            ? "bg-warning/20 text-warning"
                            : r.rank <= 3
                              ? "bg-primary/10 text-primary"
                              : "bg-secondary text-muted"
                        }`}
                      >
                        {ordinal(r.rank)}
                      </span>
                    ) : (
                      <span className="text-muted">—</span>
                    )}
                  </td>
                  <td className="px-3 py-2">
                    <span className="block font-medium text-foreground">
                      {r.candidateName}
                      {r.tied && (
                        <span
                          title={t("Tied on total score — ranked equal; the tie is broken only for display order (earliest application first), never for eligibility.")}
                          className="ml-1.5 inline-block rounded bg-warning/15 px-1.5 py-0.5 align-middle text-[10px] font-semibold text-warning"
                        >
                          {t("TIED")}
                        </span>
                      )}
                    </span>
                    <span className="block text-xs text-muted">{r.candidateNumber}</span>
                    {r.failsMandatory && (
                      <span className="mt-0.5 inline-block rounded bg-error/15 px-1.5 py-0.5 text-[10px] font-semibold text-error">
                        {t("FAILS MANDATORY CRITERION")}
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-2">
                    <span
                      className={`rounded px-2 py-0.5 text-xs font-semibold ${ELIGIBILITY_TONE[r.hireEligibility ?? ""] ?? ""}`}
                    >
                      {t(ELIGIBILITY_LABEL[r.hireEligibility ?? ""] ?? r.hireEligibility ?? "")}
                    </span>
                    <span className="mt-0.5 block text-[11px] text-muted">
                      {t(r.stage)}
                      {r.latestOfferStatus && ` · ${t("offer")} ${t(r.latestOfferStatus)}`}
                    </span>
                  </td>
                  <td className="px-3 py-2 tabular-nums text-muted">
                    {r.scoredCriteria}/{r.totalCriteria}
                  </td>
                  <td className="px-3 py-2 text-xs text-muted">
                    {r.breakdown
                      .filter((b) => b.score !== null && b.score !== undefined)
                      .map((b) => `${b.criterionName} (${b.weight}%): ${b.score}`)
                      .join(" · ") || "—"}
                  </td>
                  <td className="px-3 py-2 text-right text-base font-bold tabular-nums text-primary">
                    {r.totalScore ?? "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Modal>
  );
}

export default RankingModal;
