import { memo } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { GitPullRequestArrow } from "lucide-react";
import { getAllWorkflows } from "@/services/admin/workflow";
import { workflowEntityTypeLabel } from "@/constants/orgStructure";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";
import { Card, EmptyRow, WF_TONE, relativeTime } from "./shared";
import { useDashboardSummary } from "./useDashboardSummary";

const feedParam: ParameterModel = { ...parameterInitialData, take: 6 };

/**
 * "Approvals & Workflows" card. The Running/Approved/Rejected badges come from the aggregated
 * summary (no separate workflow-stats round trip); only the six-row recent list is its own query,
 * since row data can't be aggregated away. memo()'d so this widget's own list refetch never touches
 * its siblings.
 */
function WorkflowActivityWidget() {
  const { t } = useTranslation();
  const { data: summary, isLoading: ls } = useDashboardSummary();
  const { data: wfRecent, isLoading: lwr } = useQuery({
    queryKey: ["workflows", feedParam],
    queryFn: () => getAllWorkflows(feedParam),
    staleTime: 30_000,
  });

  return (
    <Card
      title={t("Approvals & Workflows")}
      icon={<GitPullRequestArrow className="h-4 w-4" />}
      action={
        <div className="flex items-center gap-3">
          <span className="hidden items-center gap-1.5 text-xs text-muted sm:flex">
            <span className="h-2 w-2 rounded-full bg-warning" />
            {t("Running")}: <b className="text-foreground tabular-nums">{ls ? "—" : summary?.workflowRunning ?? 0}</b>
          </span>
          <span className="hidden items-center gap-1.5 text-xs text-muted sm:flex">
            <span className="h-2 w-2 rounded-full bg-success" />
            {t("Approved")}: <b className="text-foreground tabular-nums">{ls ? "—" : summary?.workflowApproved ?? 0}</b>
          </span>
          <span className="hidden items-center gap-1.5 text-xs text-muted sm:flex">
            <span className="h-2 w-2 rounded-full bg-error" />
            {t("Rejected")}: <b className="text-foreground tabular-nums">{ls ? "—" : summary?.workflowRejected ?? 0}</b>
          </span>
          <Link to="/workflow" className="text-xs font-medium text-primary hover:underline">
            {t("View all", "View all")}
          </Link>
        </div>
      }
    >
      <div className="divide-y divide-border/60">
        {lwr && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
        {!lwr && (wfRecent?.data?.length ?? 0) === 0 && (
          <EmptyRow text={t("No workflow requests yet.", "No workflow requests yet.")} />
        )}
        {wfRecent?.data?.map((w) => (
          <Link
            key={w.id}
            to="/workflow"
            className="flex items-center gap-3 px-4 py-2.5 transition-colors hover:bg-secondary/40"
          >
            <span
              className={`shrink-0 rounded px-1.5 py-0.5 text-[11px] font-semibold ${WF_TONE[w.status ?? ""] ?? "bg-muted/30 text-muted"}`}
            >
              {t(w.status ?? "")}
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate text-[13px] font-medium text-foreground">{w.summary}</p>
              <p className="truncate text-xs text-muted">
                {workflowEntityTypeLabel(w.entityType)}
                {w.status === "Running" &&
                  ` · ${t("Step")} ${w.currentStepOrder}/${w.totalSteps} — ${w.currentStepName}`}
              </p>
            </div>
            <div className="shrink-0 text-right text-xs text-muted">
              <p>{w.requestedBy || "—"}</p>
              <p className="text-[11px] text-muted/80">{relativeTime(w.requestedAt)}</p>
            </div>
          </Link>
        ))}
      </div>
    </Card>
  );
}

export default memo(WorkflowActivityWidget);
