import { lazy, Suspense } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/context/AuthContext";
import { Users } from "lucide-react";
import { CardSkeleton, KpiRowSkeleton, NavListSkeleton, TabbedCardSkeleton } from "@/components/dashboard/DashboardSkeletons";

// Every widget is its own lazy chunk: the shell (this file) mounts instantly, and each widget's JS +
// data fetch proceed independently behind its own Suspense boundary — a slow one can never block the
// others, and each is memo()'d so its internal state (a modal, a tab switch) never re-renders siblings.
const KpiOverviewWidget = lazy(() => import("@/components/dashboard/KpiOverviewWidget"));
const WorkflowActivityWidget = lazy(() => import("@/components/dashboard/WorkflowActivityWidget"));
const WorkforceWatchlistWidget = lazy(() => import("@/components/dashboard/WorkforceWatchlistWidget"));
const ActionQueueWidget = lazy(() => import("@/components/dashboard/ActionQueueWidget"));
const RecentActivityWidget = lazy(() => import("@/components/dashboard/RecentActivityWidget"));
const QuickAccessWidget = lazy(() => import("@/components/dashboard/QuickAccessWidget"));

function Dashboard() {
  const { t } = useTranslation();
  const { user } = useAuth();

  const firstName = user?.fullName?.trim().split(/\s+/)[0] || t("there", "there");
  const hour = new Date().getHours();
  const greetingKey = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
  const today = new Date().toLocaleDateString(undefined, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  return (
    // Fiori-style workspace canvas: a soft gray shell so the white tiles/cards carry the surface
    // hierarchy. The outer <main> scrolls.
    <div className="min-h-full rounded-xl bg-secondary/25">
      <div className="mx-auto max-w-350 space-y-5 p-4 md:p-6">
        {/* Page header — quiet, Fiori-style. No query — renders instantly. */}
        <header className="flex flex-wrap items-end justify-between gap-2 pb-1">
          <div>
            <h1 className="font-display text-xl font-bold tracking-tight text-foreground md:text-2xl">
              {t(greetingKey, greetingKey)}, {firstName}
            </h1>
            <p className="mt-0.5 text-[13px] text-muted">{today}</p>
          </div>
          <Link
            to="/employee"
            className="inline-flex items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-1.5 text-[13px] font-medium text-foreground shadow-sm transition-colors hover:border-primary/40 hover:text-primary"
          >
            <Users className="h-4 w-4" />
            {t("Manage employees", "Manage employees")}
          </Link>
        </header>

        {/* KPI strip */}
        <Suspense fallback={<KpiRowSkeleton count={6} />}>
          <KpiOverviewWidget />
        </Suspense>

        {/* Work area — left: approvals + watchlist; right: activity + shortcuts */}
        <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
          <div className="space-y-4 xl:col-span-2">
            <Suspense fallback={<CardSkeleton rows={4} />}>
              <WorkflowActivityWidget />
            </Suspense>
            <Suspense fallback={<TabbedCardSkeleton tabs={2} />}>
              <WorkforceWatchlistWidget />
            </Suspense>
            <Suspense fallback={<TabbedCardSkeleton tabs={3} />}>
              <ActionQueueWidget />
            </Suspense>
          </div>

          {/* Right rail */}
          <div className="space-y-4">
            <Suspense fallback={<CardSkeleton rows={4} />}>
              <RecentActivityWidget />
            </Suspense>
            <Suspense fallback={<NavListSkeleton rows={6} />}>
              <QuickAccessWidget />
            </Suspense>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
