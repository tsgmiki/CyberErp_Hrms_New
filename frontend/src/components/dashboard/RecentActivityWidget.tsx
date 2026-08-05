import { memo } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { ScrollText } from "lucide-react";
import getAllAuditLog from "@/services/admin/auditLog/getAll";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";
import {
  ACTION_TONE,
  Card,
  EmptyRow,
  HAIRLINE,
  TABLE_HEAD,
  TABLE_ROW,
  TD,
  TD_STRONG,
  TH,
  relativeTime,
} from "./shared";

/** Narrow right-rail table: action chip + record. */
const COLS = "grid-cols-[74px_minmax(0,1fr)]";

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
      <div className={`${TABLE_HEAD} ${COLS}`}>
        <span className={TH}>{t("Action", "Action")}</span>
        <span className={TH}>{t("Record", "Record")}</span>
      </div>
      <div className={`divide-y ${HAIRLINE}`}>
        {la && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
        {!la && (activity?.data?.length ?? 0) === 0 && (
          <EmptyRow text={t("No activity recorded yet.", "No activity recorded yet.")} />
        )}
        {activity?.data?.map((a) => (
          <div key={a.id} className={`${TABLE_ROW} ${COLS}`}>
            <span
              className={`inline-flex items-center gap-1.5 justify-self-start rounded px-1.5 py-0.5 text-[10px] font-semibold ${ACTION_TONE[a.action ?? ""] ?? "bg-muted/30 text-muted"}`}
            >
              <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-current" />
              <span className="truncate">{a.action}</span>
            </span>
            <div className="min-w-0">
              <p className={TD_STRONG}>{a.entityName || a.entityType}</p>
              <p className={TD}>
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
