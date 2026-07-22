"use client";
import { memo, useState, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import {
  ClipboardCheck,
  CheckCircle2,
  Hourglass,
  PenLine,
  TrendingUp,
  Gavel,
  Target,
  BarChart3,
  ShieldAlert,
} from "lucide-react";
import { getPerformanceDashboard } from "@/services/admin/performanceDashboard";
import getAllReviewCycle from "@/services/admin/reviewCycle/getAll";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const INPUT = "rounded-lg border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";

function StatTile({ label, icon, total, tone = "primary" }: { label: string; icon: ReactNode; total?: number; tone?: "primary" | "warning" | "info" | "success" }) {
  const { t } = useTranslation();
  const toneClass =
    tone === "warning" ? "bg-warning/10 text-warning"
    : tone === "info" ? "bg-info/10 text-info"
    : tone === "success" ? "bg-success/10 text-success"
    : "bg-primary/8 text-primary";
  return (
    <div className="rounded-xl border border-border bg-card p-4 shadow-sm">
      <span className={`flex h-9 w-9 items-center justify-center rounded-lg ${toneClass}`}>{icon}</span>
      <p className="mt-3 text-2xl font-bold tracking-tight text-foreground tabular-nums">{total ?? 0}</p>
      <p className="mt-0.5 truncate text-xs font-medium text-muted">{t(label)}</p>
    </div>
  );
}

function Card({ title, icon, children }: { title: string; icon: ReactNode; children: ReactNode }) {
  const { t } = useTranslation();
  return (
    <section className="flex flex-col rounded-xl border border-border bg-card shadow-sm">
      <header className="flex items-center gap-2 border-b border-border px-4 py-3">
        <span className="text-primary">{icon}</span>
        <h2 className="text-sm font-semibold text-foreground">{t(title)}</h2>
      </header>
      <div className="p-4">{children}</div>
    </section>
  );
}

function PerformanceDashboard() {
  const { t } = useTranslation();
  const [reviewCycleId, setReviewCycleId] = useState("");

  const [cycleParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: cycles } = useQuery({ queryKey: ["reviewCycles", cycleParam], queryFn: () => getAllReviewCycle(cycleParam) });

  const { data, isLoading } = useQuery({
    queryKey: ["performanceDashboard", reviewCycleId],
    queryFn: () => getPerformanceDashboard(reviewCycleId || undefined),
  });

  const dist = data?.ratingDistribution ?? [];
  const maxCount = Math.max(1, ...dist.map((d) => d.count ?? 0));
  const progress = Number(data?.averageGoalProgress ?? 0);

  return (
    <div className="mx-auto max-w-350 space-y-4 p-4 md:p-6">
      <header className="flex flex-wrap items-end justify-between gap-2 pb-1">
        <div>
          <h1 className="font-display text-xl font-bold tracking-tight text-foreground md:text-2xl">{t("Performance Dashboard")}</h1>
          <p className="mt-0.5 text-[13px] text-muted">{t("Progress, overdue reviews, rating distribution and risk indicators")}</p>
        </div>
        <select className={INPUT} value={reviewCycleId} onChange={(e) => setReviewCycleId(e.target.value)}>
          <option value="">{t("All cycles")}</option>
          {(cycles?.data ?? []).map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
      </header>

      {isLoading ? (
        <Loading />
      ) : (
        <>
          {/* KPI strip */}
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-6">
            <StatTile label="Total Appraisals" icon={<ClipboardCheck className="h-4.5 w-4.5" />} total={data?.totalAppraisals} />
            <StatTile label="Completed" tone="success" icon={<CheckCircle2 className="h-4.5 w-4.5" />} total={data?.completedCount} />
            <StatTile label="In Manager Review" tone="info" icon={<PenLine className="h-4.5 w-4.5" />} total={data?.managerReviewCount} />
            <StatTile label="Overdue Reviews" tone="warning" icon={<Hourglass className="h-4.5 w-4.5" />} total={data?.overdueReviews} />
            <StatTile label="Pending Acknowledgment" tone="warning" icon={<PenLine className="h-4.5 w-4.5" />} total={data?.pendingAcknowledgment} />
            <StatTile label="Active PIPs" tone="info" icon={<TrendingUp className="h-4.5 w-4.5" />} total={data?.activePips} />
          </div>

          <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
            {/* Rating distribution */}
            <div className="xl:col-span-2">
              <Card title="Rating Distribution" icon={<BarChart3 className="h-4 w-4" />}>
                {dist.length === 0 ? (
                  <p className="py-6 text-center text-sm text-muted">{t("No completed appraisals yet.")}</p>
                ) : (
                  <div className="space-y-3">
                    {dist.map((d) => (
                      <div key={d.levelId} className="flex items-center gap-3">
                        <span className="w-32 shrink-0 truncate text-sm text-foreground" title={d.label}>{d.label}</span>
                        <div className="h-5 flex-1 overflow-hidden rounded bg-secondary/40">
                          <div className="h-full rounded bg-primary" style={{ width: `${((d.count ?? 0) / maxCount) * 100}%` }} />
                        </div>
                        <span className="w-8 shrink-0 text-right text-sm font-semibold tabular-nums text-foreground">{d.count ?? 0}</span>
                      </div>
                    ))}
                  </div>
                )}
              </Card>
            </div>

            {/* Goal progress */}
            <Card title="Goal Progress" icon={<Target className="h-4 w-4" />}>
              <div className="space-y-3">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-muted">{t("Total Goals")}</span>
                  <span className="font-semibold tabular-nums text-foreground">{data?.totalGoals ?? 0}</span>
                </div>
                <div className="flex items-center justify-between text-sm">
                  <span className="text-muted">{t("Completed Goals")}</span>
                  <span className="font-semibold tabular-nums text-foreground">{data?.completedGoals ?? 0}</span>
                </div>
                <div>
                  <div className="mb-1 flex items-center justify-between text-sm">
                    <span className="text-muted">{t("Average Progress")}</span>
                    <span className="font-semibold tabular-nums text-foreground">{progress}%</span>
                  </div>
                  <div className="h-2.5 overflow-hidden rounded-full bg-secondary/40">
                    <div className="h-full rounded-full bg-primary" style={{ width: `${Math.min(100, Math.max(0, progress))}%` }} />
                  </div>
                </div>
              </div>
            </Card>
          </div>

          {/* Risk indicators */}
          <Card title="Risk Indicators" icon={<ShieldAlert className="h-4 w-4" />}>
            <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
              <div className="rounded-lg border border-border p-3">
                <p className="text-xl font-bold tabular-nums text-warning">{data?.activePips ?? 0}</p>
                <p className="text-xs text-muted">{t("Active PIPs")}</p>
              </div>
              <div className="rounded-lg border border-border p-3">
                <p className="text-xl font-bold tabular-nums text-warning">{data?.openAppeals ?? 0}</p>
                <p className="flex items-center gap-1 text-xs text-muted"><Gavel className="h-3 w-3" /> {t("Open Appeals")}</p>
              </div>
              <div className="rounded-lg border border-border p-3">
                <p className="text-xl font-bold tabular-nums text-error">{data?.overdueReviews ?? 0}</p>
                <p className="text-xs text-muted">{t("Overdue Reviews")}</p>
              </div>
              <div className="rounded-lg border border-border p-3">
                <p className="text-xl font-bold tabular-nums text-info">{data?.pendingAcknowledgment ?? 0}</p>
                <p className="text-xs text-muted">{t("Pending Acknowledgment")}</p>
              </div>
            </div>
          </Card>
        </>
      )}
    </div>
  );
}

export default memo(PerformanceDashboard);
