import { useQuery } from "@tanstack/react-query";
import getDashboardSummary from "@/services/admin/dashboard/getSummary";

/**
 * The dashboard's ONE aggregated KPI/workflow-stats/watchlist-count read. Multiple widgets call
 * this same hook independently (KpiOverviewWidget, WorkflowActivityWidget, WorkforceWatchlistWidget)
 * — React Query dedupes identical concurrent queries by key, so this is still exactly ONE network
 * request no matter how many widgets mount it, with zero prop-drilling between them.
 */
export function useDashboardSummary() {
  return useQuery({
    queryKey: ["dashboardSummary"],
    queryFn: getDashboardSummary,
    staleTime: 30_000,
  });
}
