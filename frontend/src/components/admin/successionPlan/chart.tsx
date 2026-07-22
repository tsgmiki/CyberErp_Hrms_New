"use client";
import { memo } from "react";
import { useQuery } from "@tanstack/react-query";
import { Crown, User } from "lucide-react";
import { getSuccessionChart } from "@/services/admin/successionPlan/chart";
import { readinessLevelOptions } from "@/constants/careerDevelopment";
import Loading from "../../common/loader/loader";

const READY_TONE: Record<string, string> = {
  ReadyNow: "bg-success/15 text-success",
  Ready1To2Years: "bg-info/15 text-info",
  Ready3PlusYears: "bg-warning/15 text-warning",
  NotReady: "bg-muted/30 text-muted",
};

/** Succession chart (HC159): the role at the top, its ranked successors below — a lightweight view. */
function SuccessionChart({ planId }: { planId: string }) {
  const { data, isLoading } = useQuery({
    queryKey: ["successionPlan", planId, "chart"],
    queryFn: () => getSuccessionChart(planId),
    enabled: !!planId,
  });
  const readyLabel = (v: string) => readinessLevelOptions.find((o) => o.id === v)?.name ?? v;

  return (
    <section className="rounded-xl border border-border bg-card p-4 shadow-sm">
      <h3 className="mb-3 text-sm font-semibold text-foreground">Succession Chart</h3>
      {isLoading ? (
        <Loading />
      ) : (
        <div className="flex flex-col items-center gap-2">
          <div className="flex items-center gap-2 rounded-lg border border-primary/40 bg-primary/10 px-4 py-2 text-sm font-semibold text-primary">
            <Crown size={15} /> {data?.roleTitle ?? data?.name ?? "Role"}
          </div>
          {(data?.successors?.length ?? 0) > 0 && <div className="h-4 w-px bg-border" />}
          <div className="flex w-full flex-col gap-2">
            {(data?.successors ?? []).map((s) => (
              <div key={s.candidateId} className="flex items-center gap-3 rounded-lg border border-border bg-secondary/20 px-3 py-2">
                <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">#{s.rank}</span>
                <User size={15} className="shrink-0 text-muted" />
                <span className="min-w-0 flex-1 truncate text-sm font-medium text-foreground">{s.employeeName ?? "—"}</span>
                <span className={`rounded px-2 py-0.5 text-xs font-semibold ${READY_TONE[s.readiness] ?? "bg-muted/30 text-muted"}`}>{readyLabel(s.readiness)}</span>
                {s.readinessScore != null && <span className="text-xs tabular-nums text-muted">{Number(s.readinessScore).toFixed(0)}%</span>}
              </div>
            ))}
            {(data?.successors?.length ?? 0) === 0 && <p className="text-center text-xs text-muted">No successors yet.</p>}
          </div>
        </div>
      )}
    </section>
  );
}

export default memo(SuccessionChart);
