import { lazy, Suspense } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/context/AuthContext";
import { Users } from "lucide-react";
import {
  AnalyticsBandSkeleton,
  CardSkeleton,
  KpiRowSkeleton,
  NavListSkeleton,
  TabbedCardSkeleton,
} from "@/components/dashboard/DashboardSkeletons";
import { SectionLabel } from "@/components/dashboard/shared";

// Every widget is its own lazy chunk: the shell (this file) mounts instantly, and each widget's JS +
// data fetch proceed independently behind its own Suspense boundary — a slow one can never block the
// others, and each is memo()'d so its internal state (a modal, a tab switch) never re-renders siblings.
const KpiOverviewWidget = lazy(() => import("@/components/dashboard/KpiOverviewWidget"));
const WorkforceAnalyticsWidget = lazy(() => import("@/components/dashboard/WorkforceAnalyticsWidget"));
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
    // Neutral workspace canvas. Previously this was the brand tint (`bg-secondary`, a pale blue),
    // which washed the whole page blue and left the white cards barely distinguishable from it.
    <div className="min-h-full rounded-xl bg-background">
      <div className="mx-auto max-w-350 p-4 md:p-6">
        <header className="mb-4 flex flex-wrap items-center justify-between gap-3 border-b border-[var(--border)] pb-3">
          <div className="min-w-0">
            <h1 className="font-display text-lg font-bold tracking-tight text-foreground md:text-[22px]">
              {t(greetingKey, greetingKey)}, {firstName}
            </h1>
            <p className="mt-1 text-[13px] text-muted">{today}</p>
          </div>
          <Link
            to="/employee"
            className="focus-ring inline-flex shrink-0 items-center gap-2 rounded-md border border-border bg-card px-3.5 py-2 text-[13px] font-semibold text-foreground shadow-[0_1px_2px_rgba(15,23,42,0.05)] transition-colors hover:text-primary"
          >
            <Users className="h-4 w-4" />
            {t("Manage employees", "Manage employees")}
          </Link>
        </header>

        {/* Zone 1 — the numbers. */}
        <section className="mb-5">
          <SectionLabel>{t("Key Metrics", "Key Metrics")}</SectionLabel>
          <Suspense fallback={<KpiRowSkeleton count={6} />}>
            <KpiOverviewWidget />
          </Suspense>
        </section>

        {/* Zone 2 — the charts. Reads the SAME cached summary query as the KPI strip above. */}
        <section className="mb-5">
          <SectionLabel>{t("Analytics", "Analytics")}</SectionLabel>
          <Suspense fallback={<AnalyticsBandSkeleton />}>
            <WorkforceAnalyticsWidget />
          </Suspense>
        </section>

        {/* Zone 3 — the work. `lg:` (not `xl:`) splits the columns at 1024px, closing the gap where
            mid-size desktop windows stayed single-column and squeezed every card to its narrowest. */}
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-3 lg:gap-5">
          <div className="lg:col-span-2">
            <SectionLabel>{t("Action Center", "Action Center")}</SectionLabel>
            <div className="space-y-4">
              <Suspense fallback={<CardSkeleton rows={4} />}>
                <WorkflowActivityWidget />
              </Suspense>
              <Suspense fallback={<TabbedCardSkeleton tabs={3} />}>
                <ActionQueueWidget />
              </Suspense>
              {/* Watchlist lives in the WIDE column: its rows carry a name, position, date and a
                  status badge, which crush together in a narrow rail — and keeping three cards here
                  against two in the side rail is what stops the columns ending at wildly
                  different heights. */}
              <Suspense fallback={<TabbedCardSkeleton tabs={2} />}>
                <WorkforceWatchlistWidget />
              </Suspense>
            </div>
          </div>

          <div>
            <SectionLabel>{t("Insights", "Insights")}</SectionLabel>
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
    </div>
  );
}

export default Dashboard;
