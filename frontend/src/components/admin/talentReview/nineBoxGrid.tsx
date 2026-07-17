"use client";
import { memo } from "react";
import { useQuery } from "@tanstack/react-query";
import { getNineBox } from "@/services/admin/talentReview/nineBox";
import { bandLabel } from "@/constants/careerDevelopment";
import Loading from "../../common/loader/loader";

/** The 9-box grid (HC150): potential on the Y axis (High at top), performance on the X axis.
 * Counts come pre-aggregated from the server (one GROUP BY) — this only renders. */
function NineBoxGrid({ reviewId }: { reviewId: string }) {
  const { data, isLoading } = useQuery({
    queryKey: ["talentReview", reviewId, "nine-box"],
    queryFn: () => getNineBox(reviewId),
    enabled: !!reviewId,
  });

  const count = (perf: number, pot: number) =>
    (data?.cells ?? []).find((c) => c.performanceBand === perf && c.potentialBand === pot)?.count ?? 0;

  // Top-right (High perf + High potential) is the "stars" cell.
  const tone = (perf: number, pot: number) => {
    const s = perf + pot;
    if (perf === 3 && pot === 3) return "bg-success/15 border-success/40";
    if (s >= 5) return "bg-primary/10 border-primary/30";
    if (s <= 3) return "bg-error/10 border-error/25";
    return "bg-secondary/40 border-border";
  };

  return (
    <section className="rounded-xl border border-border bg-card p-4 shadow-sm">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-foreground">9-Box Grid</h3>
        <div className="flex items-center gap-2 text-xs text-muted">
          <span>{data?.total ?? 0} assessed</span>
          <span className="rounded-full bg-primary/10 px-2 py-0.5 font-medium text-primary">{data?.hiPoCount ?? 0} HiPo</span>
        </div>
      </div>
      {isLoading ? (
        <Loading />
      ) : (
        <div className="flex gap-2">
          {/* Y-axis label */}
          <div className="flex w-5 items-center justify-center">
            <span className="rotate-180 text-[10px] font-semibold uppercase tracking-wider text-muted [writing-mode:vertical-rl]">Potential →</span>
          </div>
          <div className="flex-1">
            <div className="grid grid-cols-3 gap-2">
              {[3, 2, 1].map((pot) =>
                [1, 2, 3].map((perf) => (
                  <div key={`${perf}-${pot}`} className={`flex min-h-[64px] flex-col items-center justify-center rounded-lg border ${tone(perf, pot)}`}>
                    <span className="text-lg font-bold text-foreground">{count(perf, pot)}</span>
                    <span className="text-[10px] text-muted">{bandLabel(perf)} / {bandLabel(pot)}</span>
                  </div>
                )),
              )}
            </div>
            <div className="mt-1 text-center text-[10px] font-semibold uppercase tracking-wider text-muted">Performance →</div>
          </div>
        </div>
      )}
    </section>
  );
}

export default memo(NineBoxGrid);
