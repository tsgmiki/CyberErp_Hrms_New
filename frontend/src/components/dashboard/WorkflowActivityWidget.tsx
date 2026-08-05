import { memo } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { GitPullRequestArrow } from "lucide-react";
import { getAllWorkflows } from "@/services/admin/workflow";
import { workflowEntityTypeLabel } from "@/constants/orgStructure";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";
import {
  Card,
  EmptyRow,
  HAIRLINE,
  StatChip,
  TABLE_HEAD,
  TABLE_ROW,
  TD,
  TD_STRONG,
  TH,
  WF_TONE,
  relativeTime,
} from "./shared";
import { useDashboardSummary } from "./useDashboardSummary";

const feedParam: ParameterModel = { ...parameterInitialData, take: 6 };

/** Shared column template — header and rows use the SAME grid so every cell lines up exactly. */
const COLS = "grid-cols-[78px_minmax(0,1fr)_120px_64px]";

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
        <div className="flex flex-wrap items-center gap-1.5">
          <StatChip tone="warning" label={t("Running")} value={ls ? "—" : summary?.workflowRunning ?? 0} />
          <StatChip tone="success" label={t("Approved")} value={ls ? "—" : summary?.workflowApproved ?? 0} />
          <StatChip tone="error" label={t("Rejected")} value={ls ? "—" : summary?.workflowRejected ?? 0} />
          <Link to="/workflow" className="ml-1 text-xs font-medium text-primary hover:underline">
            {t("View all", "View all")}
          </Link>
        </div>
      }
    >
      <div className={`${TABLE_HEAD} ${COLS}`}>
        <span className={TH}>{t("Status", "Status")}</span>
        <span className={TH}>{t("Request", "Request")}</span>
        <span className={`${TH} hidden sm:block`}>{t("Requested By", "Requested By")}</span>
        <span className={`${TH} hidden text-right sm:block`}>{t("When", "When")}</span>
      </div>
      <div className={`divide-y ${HAIRLINE}`}>
        {lwr && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
        {!lwr && (wfRecent?.data?.length ?? 0) === 0 && (
          <EmptyRow text={t("No workflow requests yet.", "No workflow requests yet.")} />
        )}
        {wfRecent?.data?.map((w) => (
          <Link key={w.id} to="/workflow" className={`${TABLE_ROW} ${COLS}`}>
            <span
              className={`inline-flex items-center gap-1.5 justify-self-start rounded px-1.5 py-0.5 text-[10px] font-semibold ${WF_TONE[w.status ?? ""] ?? "bg-muted/30 text-muted"}`}
            >
              <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-current" />
              <span className="truncate">{t(w.status ?? "")}</span>
            </span>
            <div className="min-w-0">
              <p className={TD_STRONG}>{w.summary}</p>
              <p className={TD}>
                {workflowEntityTypeLabel(w.entityType)}
                {w.status === "Running" &&
                  ` · ${t("Step")} ${w.currentStepOrder}/${w.totalSteps} — ${w.currentStepName}`}
              </p>
            </div>
            <span className={`hidden text-[11px] text-label sm:block ${TD_STRONG} !font-normal`}>
              {w.requestedBy || "—"}
            </span>
            <span className={`hidden text-right ${TD} sm:block`}>{relativeTime(w.requestedAt)}</span>
          </Link>
        ))}
      </div>
    </Card>
  );
}

export default memo(WorkflowActivityWidget);
