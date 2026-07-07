import { memo, useState, type ReactNode } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/context/AuthContext";
import {
  Building,
  Network,
  Briefcase,
  ScrollText,
  ChevronRight,
  GitPullRequestArrow,
  Users,
  Hourglass,
  CalendarClock,
  ArrowUpRight,
} from "lucide-react";
import getAllBranch from "@/services/admin/branch/getAll";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import getAllPosition from "@/services/admin/position/getAll";
import getAllEmployee from "@/services/admin/employee/getAll";
import getAllAuditLog from "@/services/admin/auditLog/getAll";
import getEmployeesOnProbation from "@/services/admin/employee/onProbation";
import getUpcomingRetirements from "@/services/admin/employee/upcomingRetirements";
import { getAllWorkflows, getWorkflowStats } from "@/services/admin/workflow";
import { workflowEntityTypeLabel } from "@/constants/orgStructure";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";

const countParam: ParameterModel = { ...parameterInitialData, take: 1 };
const feedParam: ParameterModel = { ...parameterInitialData, take: 6 };

const ACTION_TONE: Record<string, string> = {
  Created: "bg-success/15 text-success",
  Modified: "bg-info/15 text-info",
  Reassigned: "bg-warning/15 text-warning",
  Deleted: "bg-error/15 text-error",
  Rejected: "bg-muted/30 text-muted",
};

const WF_TONE: Record<string, string> = {
  Running: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
};

function relativeTime(iso?: string): string {
  if (!iso) return "";
  const then = new Date(iso).getTime();
  const mins = Math.floor((Date.now() - then) / 60000);
  if (mins < 1) return "just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(iso).toLocaleDateString();
}

/* ---------- building blocks (Fiori-style tiles & cards) ---------- */

function KpiTile({
  to,
  label,
  icon,
  total,
  isLoading,
  tone = "primary",
}: {
  to: string;
  label: string;
  icon: ReactNode;
  total?: number;
  isLoading: boolean;
  tone?: "primary" | "warning" | "info";
}) {
  const { t } = useTranslation();
  const toneClass =
    tone === "warning"
      ? "bg-warning/10 text-warning"
      : tone === "info"
        ? "bg-info/10 text-info"
        : "bg-primary/8 text-primary";
  return (
    <Link
      to={to}
      className="group rounded-xl border border-border bg-card p-4 shadow-sm transition-all hover:border-primary/40 hover:shadow-md"
    >
      <div className="flex items-start justify-between">
        <span className={`flex h-9 w-9 items-center justify-center rounded-lg ${toneClass}`}>{icon}</span>
        <ArrowUpRight className="h-3.5 w-3.5 text-muted/50 opacity-0 transition-opacity group-hover:opacity-100" />
      </div>
      <p className="mt-3 text-2xl font-bold tracking-tight text-foreground tabular-nums">
        {isLoading ? <span className="text-muted">—</span> : (total ?? 0)}
      </p>
      <p className="mt-0.5 truncate text-xs font-medium text-muted">{t(label)}</p>
    </Link>
  );
}

function Card({
  title,
  icon,
  action,
  children,
}: {
  title: string;
  icon: ReactNode;
  action?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="flex flex-col rounded-xl border border-border bg-card shadow-sm">
      <header className="flex items-center justify-between gap-3 border-b border-border px-4 py-3">
        <h2 className="flex items-center gap-2 text-sm font-semibold text-foreground">
          <span className="text-primary">{icon}</span>
          {title}
        </h2>
        {action}
      </header>
      {children}
    </section>
  );
}

function EmptyRow({ text }: { text: string }) {
  return <p className="px-4 py-8 text-center text-sm text-muted">{text}</p>;
}

function DaysBadge({ days, warnAt }: { days?: number | null; warnAt: number }) {
  if (typeof days !== "number") return null;
  const cls =
    days < 0
      ? "bg-error/15 text-error"
      : days <= warnAt
        ? "bg-warning/15 text-warning"
        : "bg-muted/25 text-muted";
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-[11px] font-semibold tabular-nums ${cls}`}>
      {days < 0 ? `${-days}d overdue` : `${days}d left`}
    </span>
  );
}

/* ---------- page ---------- */

function Dashboard() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [watchTab, setWatchTab] = useState<"probation" | "retirements">("probation");

  const hour = new Date().getHours();
  const greetingKey = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
  const firstName = user?.fullName?.trim().split(/\s+/)[0] || t("there", "there");
  const today = new Date().toLocaleDateString(undefined, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  const { data: branches, isLoading: lb } = useQuery({
    queryKey: ["branches", countParam],
    queryFn: () => getAllBranch(countParam),
  });
  const { data: units, isLoading: lu } = useQuery({
    queryKey: ["organizationUnits", countParam],
    queryFn: () => getAllOrganizationUnit(countParam),
  });
  const { data: positions, isLoading: lp } = useQuery({
    queryKey: ["positions", countParam],
    queryFn: () => getAllPosition(countParam),
  });
  const { data: employees, isLoading: le } = useQuery({
    queryKey: ["employees", countParam],
    queryFn: () => getAllEmployee(countParam),
  });
  const { data: activity, isLoading: la } = useQuery({
    queryKey: ["auditLogs", feedParam],
    queryFn: () => getAllAuditLog(feedParam),
  });
  const { data: wfStats, isLoading: lws } = useQuery({
    queryKey: ["workflowStats"],
    queryFn: getWorkflowStats,
  });
  const { data: wfRecent, isLoading: lwr } = useQuery({
    queryKey: ["workflows", feedParam],
    queryFn: () => getAllWorkflows(feedParam),
  });
  const { data: probation, isLoading: lpr } = useQuery({
    queryKey: ["employeesOnProbation"],
    queryFn: getEmployeesOnProbation,
  });
  const { data: retirements, isLoading: lrt } = useQuery({
    queryKey: ["upcomingRetirements"],
    queryFn: getUpcomingRetirements,
  });

  const watchTabs = [
    { key: "probation" as const, label: t("On Probation"), count: probation?.length ?? 0 },
    { key: "retirements" as const, label: t("Upcoming Retirements"), count: retirements?.length ?? 0 },
  ];

  return (
    <div className="mx-auto max-w-350 space-y-4 p-4 md:p-6">
      {/* Page header — quiet, Fiori-style */}
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

      {/* KPI strip — one glanceable row, actionable counts included */}
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-6">
        <KpiTile to="/employee" label="Employees" icon={<Users className="h-4.5 w-4.5" />} total={employees?.total} isLoading={le} />
        <KpiTile to="/branch" label="Branches" icon={<Building className="h-4.5 w-4.5" />} total={branches?.total} isLoading={lb} />
        <KpiTile to="/organizationUnit" label="Organization Units" icon={<Network className="h-4.5 w-4.5" />} total={units?.total} isLoading={lu} />
        <KpiTile to="/position" label="Positions" icon={<Briefcase className="h-4.5 w-4.5" />} total={positions?.total} isLoading={lp} />
        <KpiTile to="/employee" label="On Probation" tone="warning" icon={<Hourglass className="h-4.5 w-4.5" />} total={probation?.length} isLoading={lpr} />
        <KpiTile to="/employee" label="Retiring Soon" tone="info" icon={<CalendarClock className="h-4.5 w-4.5" />} total={retirements?.length} isLoading={lrt} />
      </div>

      {/* Work area — left: approvals + watchlist; right: activity + shortcuts */}
      <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
        <div className="space-y-4 xl:col-span-2">
          {/* Approvals & workflows */}
          <Card
            title={t("Approvals & Workflows")}
            icon={<GitPullRequestArrow className="h-4 w-4" />}
            action={
              <div className="flex items-center gap-3">
                <span className="hidden items-center gap-1.5 text-xs text-muted sm:flex">
                  <span className="h-2 w-2 rounded-full bg-warning" />
                  {t("Running")}: <b className="text-foreground tabular-nums">{lws ? "—" : wfStats?.running ?? 0}</b>
                </span>
                <span className="hidden items-center gap-1.5 text-xs text-muted sm:flex">
                  <span className="h-2 w-2 rounded-full bg-success" />
                  {t("Approved")}: <b className="text-foreground tabular-nums">{lws ? "—" : wfStats?.approved ?? 0}</b>
                </span>
                <span className="hidden items-center gap-1.5 text-xs text-muted sm:flex">
                  <span className="h-2 w-2 rounded-full bg-error" />
                  {t("Rejected")}: <b className="text-foreground tabular-nums">{lws ? "—" : wfStats?.rejected ?? 0}</b>
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

          {/* Workforce watchlist — one card, tabbed (probation / retirements) */}
          <section className="rounded-xl border border-border bg-card shadow-sm">
            <div className="flex items-center gap-1 border-b border-border px-2 pt-1.5">
              {watchTabs.map((tab) => (
                <button
                  key={tab.key}
                  type="button"
                  onClick={() => setWatchTab(tab.key)}
                  className={`relative flex items-center gap-2 rounded-t-lg px-3 py-2 text-[13px] font-medium transition-colors ${
                    watchTab === tab.key ? "text-primary" : "text-muted hover:text-foreground"
                  }`}
                >
                  {tab.label}
                  <span
                    className={`rounded-full px-1.5 py-0.5 text-[11px] font-semibold tabular-nums ${
                      watchTab === tab.key ? "bg-primary/10 text-primary" : "bg-muted/25 text-muted"
                    }`}
                  >
                    {tab.count}
                  </span>
                  {watchTab === tab.key && (
                    <span className="absolute inset-x-2 -bottom-px h-0.5 rounded-full bg-primary" />
                  )}
                </button>
              ))}
            </div>

            {watchTab === "probation" ? (
              <div className="divide-y divide-border/60">
                {lpr && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
                {!lpr && (probation?.length ?? 0) === 0 && (
                  <EmptyRow text={t("No employees on probation.", "No employees on probation.")} />
                )}
                {probation?.map((e) => (
                  <Link
                    key={e.id}
                    to="/employee"
                    className="flex items-center gap-3 px-4 py-2.5 transition-colors hover:bg-secondary/40"
                  >
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-[13px] font-medium text-foreground">{e.fullName}</p>
                      <p className="truncate text-xs text-muted">
                        {e.employeeNumber}
                        {e.positionTitle ? ` · ${e.positionTitle}` : ""}
                      </p>
                    </div>
                    <div className="shrink-0 text-right">
                      <p className="text-xs text-muted">
                        {e.probationEndDate ? new Date(e.probationEndDate).toLocaleDateString() : "—"}
                      </p>
                      <DaysBadge days={e.daysRemaining} warnAt={7} />
                    </div>
                  </Link>
                ))}
              </div>
            ) : (
              <div className="divide-y divide-border/60">
                {lrt && <EmptyRow text={`${t("Loading", "Loading")}…`} />}
                {!lrt && (retirements?.length ?? 0) === 0 && (
                  <EmptyRow text={t("No retirements within a month.", "No retirements within a month.")} />
                )}
                {retirements?.map((e) => (
                  <Link
                    key={e.id}
                    to="/employee"
                    className="flex items-center gap-3 px-4 py-2.5 transition-colors hover:bg-secondary/40"
                  >
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-[13px] font-medium text-foreground">{e.fullName}</p>
                      <p className="truncate text-xs text-muted">{e.employeeNumber}</p>
                    </div>
                    <div className="shrink-0 text-right">
                      <p className="text-xs text-muted">{new Date(e.retirementDate).toLocaleDateString()}</p>
                      <DaysBadge days={e.daysRemaining} warnAt={14} />
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </section>
        </div>

        {/* Right rail */}
        <div className="space-y-4">
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

          <Card title={t("Quick Access", "Quick Access")} icon={<ChevronRight className="h-4 w-4" />}>
            <nav className="p-2">
              {[
                { to: "/employee", label: "Employees", Icon: Users },
                { to: "/leaveRequest", label: "Leave Requests", Icon: CalendarClock },
                { to: "/workflow", label: "Workflow Tracking", Icon: GitPullRequestArrow },
                { to: "/organizationUnit", label: "Organization Structure", Icon: Network },
                { to: "/position", label: "Positions", Icon: Briefcase },
                { to: "/branch", label: "Branches", Icon: Building },
              ].map(({ to, label, Icon }) => (
                <Link
                  key={to}
                  to={to}
                  className="flex items-center gap-2.5 rounded-lg px-2.5 py-2 text-[13px] text-foreground transition-colors hover:bg-secondary"
                >
                  <Icon className="h-4 w-4 text-muted" />
                  {t(label)}
                  <ChevronRight className="ml-auto h-3.5 w-3.5 text-muted/60" />
                </Link>
              ))}
            </nav>
          </Card>
        </div>
      </div>
    </div>
  );
}

export default memo(Dashboard);
