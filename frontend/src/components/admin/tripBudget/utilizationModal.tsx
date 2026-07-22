"use client";
import { memo } from "react";
import { useQuery } from "@tanstack/react-query";
import DialogModal from "@/components/common/dialog";
import Loading from "../../common/loader/loader";
import getTripBudget from "@/services/admin/tripBudget/get";
import { getTripBudgetUtilization } from "@/services/admin/trip";

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

function UtilizationModal({ budgetId, onClose }: { budgetId: string; onClose: () => void }) {
  const { data: budget } = useQuery({ queryKey: ["tripBudget", budgetId], queryFn: () => getTripBudget(budgetId) });
  const { data, isLoading } = useQuery({
    queryKey: ["tripBudgetUtilization", budget?.fiscalYear, budget?.organizationUnitId],
    queryFn: () => getTripBudgetUtilization(budget!.fiscalYear!, budget!.organizationUnitId),
    enabled: !!budget?.fiscalYear,
  });
  const pct = data && (data.budgetAmount ?? 0) > 0 ? Math.round(((data.committed ?? 0) / (data.budgetAmount ?? 1)) * 100) : 0;

  return (
    <DialogModal title="Budget Utilization" visible onClose={onClose} hideOk cancelLabel="Close">
      <p className="mb-3 text-xs text-muted">
        {budget?.fiscalYear} · {budget?.organizationUnitName ?? "Organization-wide"}
      </p>
      {isLoading || !data ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          <div className="grid grid-cols-3 gap-2 text-sm">
            <div><p className="text-[11px] uppercase text-muted">Budget</p><p className="font-semibold tabular-nums">{money(data.budgetAmount)}</p></div>
            <div><p className="text-[11px] uppercase text-muted">Committed</p><p className="font-semibold tabular-nums text-warning">{money(data.committed)}</p></div>
            <div><p className="text-[11px] uppercase text-muted">Remaining</p><p className="font-semibold tabular-nums text-success">{money(data.remaining)}</p></div>
          </div>
          <div>
            <div className="mb-1 flex justify-between text-xs text-muted"><span>{data.tripCount ?? 0} trips</span><span>{pct}%</span></div>
            <div className="h-2 w-full overflow-hidden rounded-full bg-secondary/40">
              <div className={`h-full rounded-full ${pct > 100 ? "bg-error" : "bg-primary"}`} style={{ width: `${Math.min(pct, 100)}%` }} />
            </div>
          </div>
        </div>
      )}
    </DialogModal>
  );
}

export default memo(UtilizationModal);
