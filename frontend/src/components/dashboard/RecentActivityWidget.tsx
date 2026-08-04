import { memo } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { ScrollText } from "lucide-react";
import getAllAuditLog from "@/services/admin/auditLog/getAll";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";
import { ACTION_TONE, Card, EmptyRow, relativeTime } from "./shared";

const feedParam: ParameterModel = { ...parameterInitialData, take: 6 };

/** The audit-log feed card — a standalone widget/query so it can never block or be blocked by anything else. */
function RecentActivityWidget() {
  const { t } = useTranslation();
  const { data: activity, isLoading: la } = useQuery({
    queryKey: ["auditLogs", feedParam],
    queryFn: () => getAllAuditLog(feedParam),
    staleTime: 30_000,
  });

  return (
    <Card
      title={t("Recent Activity", "Recent Activity")}
      icon={<ScrollText className="h-4 w-4" />}
      action={
        <Link to="/auditLog" className="text-xs font-medium text-primary hover:underline">
          {t("View all", "View all")}
        </Link>
      }
    >
      <div className="divide-y divide-border/60">
        {la && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
        {!la && (activity?.data?.length ?? 0) === 0 && (
          <EmptyRow text={t("No activity recorded yet.", "No activity recorded yet.")} />
        )}
        {activity?.data?.map((a) => (
          <div key={a.id} className="flex items-center gap-3 px-4 py-2.5">
            <span
              className={`shrink-0 rounded px-1.5 py-0.5 text-[11px] font-semibold ${ACTION_TONE[a.action ?? ""] ?? "bg-muted/30 text-muted"}`}
            >
              {a.action}
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate text-[13px] text-foreground">
                <span className="font-medium">{a.entityName || a.entityType}</span>
              </p>
              <p className="truncate text-[11px] text-muted">
                {a.performedBy || "—"} · {relativeTime(a.timestamp)}
              </p>
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
}

export default memo(RecentActivityWidget);
