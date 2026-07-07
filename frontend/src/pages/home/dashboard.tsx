import { memo, type ReactNode } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/context/AuthContext";
import {
  Building,
  Building2,
  Network,
  Briefcase,
  BriefcaseBusiness,
  MapPin,
  ScrollText,
  Layers,
  Tags,
  ChevronRight,
  GitFork,
  GitPullRequestArrow,
  Users,
  Hourglass,
  CalendarClock,
} from "lucide-react";
import getAllBranch from "@/services/admin/branch/getAll";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import getAllPosition from "@/services/admin/position/getAll";
import getAllWorkLocation from "@/services/admin/workLocation/getAll";
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

function KpiTile({
  to,
  label,
  icon,
  total,
  isLoading,
}: {
  to: string;
  label: string;
  icon: ReactNode;
  total?: number;
  isLoading: boolean;
}) {
  const { t } = useTranslation();
  return (
    <Link
      to={to}
      className="group flex items-center gap-4 rounded-xl border border-border bg-card p-4 shadow-sm transition-all hover:border-primary/40 hover:shadow-md"
    >
      <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-primary/8 text-primary">
        {icon}
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-[11px] font-semibold uppercase tracking-wider text-muted">
          {t(label)}
        </p>
        <p className="mt-0.5 text-2xl font-bold tracking-tight text-foreground tabular-nums">
          {isLoading ? <span className="text-muted">—</span> : (total ?? 0)}
        </p>
      </div>
      <ChevronRight className="h-4 w-4 shrink-0 text-muted opacity-0 transition-opacity group-hover:opacity-100" />
    </Link>
  );
}

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

const QUICK_LINKS = [
  { to: "/employee", label: "Employees", Icon: Users },
  { to: "/branch", label: "Branches", Icon: Building },
  { to: "/organizationUnit", label: "Organization Structure", Icon: Network },
  { to: "/positionClass", label: "Position Classes", Icon: BriefcaseBusiness },
  { to: "/position", label: "Positions", Icon: Briefcase },
  { to: "/jobGrade", label: "Job Grades", Icon: Layers },
  { to: "/jobCategory", label: "Job Categories", Icon: Tags },
  { to: "/workLocation", label: "Work Locations", Icon: MapPin },
];

function Dashboard() {
  const { t } = useTranslation();
  const { user } = useAuth();

  const hour = new Date().getHours();
  const greetingKey = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
  const firstName = user?.fullName?.trim().split(/\s+/)[0] || t("there", "there");

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
  const { data: locations, isLoading: ll } = useQuery({
    queryKey: ["workLocations", countParam],
    queryFn: () => getAllWorkLocation(countParam),
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

  const today = new Date().toLocaleDateString(undefined, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  return (
    <div className="mx-auto max-w-350 space-y-5 p-4 md:p-6">
      {/* Welcome hero */}
      <section
        className="relative overflow-hidden rounded-2xl px-6 py-6 text-white shadow-sm md:px-8 md:py-7"
        style={{
          backgroundImage:
            "linear-gradient(135deg, var(--primary) 0%, color-mix(in srgb, var(--primary) 62%, #05122b) 100%)",
        }}
      >
        <div className="relative z-10">
          <p className="text-xs font-medium uppercase tracking-wider text-white/60">{today}</p>
          <h1 className="mt-1.5 font-display text-2xl font-bold tracking-tight md:text-[26px]">
            {t(greetingKey, greetingKey)}, {firstName}
          </h1>
          <p className="mt-1.5 max-w-xl text-sm leading-relaxed text-white/75">
            {t(
              "Here's what's happening across your organization today.",
              "Here's what's happening across your organization today.",
            )}
          </p>
        </div>
        <Building2
          className="pointer-events-none absolute -bottom-9 -right-6 h-44 w-44 text-white/10"
          strokeWidth={1.25}
          aria-hidden
        />
        <div
          className="pointer-events-none absolute -right-16 -top-16 h-52 w-52 rounded-full"
          style={{ background: "radial-gradient(circle, rgba(255,255,255,0.12), transparent 70%)" }}
          aria-hidden
        />
      </section>

      {/* KPI tiles */}
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-5">
        <KpiTile to="/employee" label="Employees" icon={<Users className="h-5 w-5" />} total={employees?.total} isLoading={le} />
        <KpiTile to="/branch" label="Branches" icon={<Building className="h-5 w-5" />} total={branches?.total} isLoading={lb} />
        <KpiTile to="/organizationUnit" label="Organization Units" icon={<Network className="h-5 w-5" />} total={units?.total} isLoading={lu} />
        <KpiTile to="/position" label="Positions" icon={<Briefcase className="h-5 w-5" />} total={positions?.total} isLoading={lp} />
        <KpiTile to="/workLocation" label="Work Locations" icon={<MapPin className="h-5 w-5" />} total={locations?.total} isLoading={ll} />
      </div>

      {/* Workflow tracking */}
      <section className="rounded-xl border border-border bg-card shadow-sm">
        <header className="flex items-center justify-between border-b border-border px-4 py-3">
          <h2 className="flex items-center gap-2 text-sm font-semibold text-foreground">
            <GitPullRequestArrow className="h-4 w-4 text-primary" />
            {t("Workflow Tracking")}
          </h2>
          <div className="flex items-center gap-3">
            <span className="flex items-center gap-1.5 text-xs text-muted">
              <span className="h-2 w-2 rounded-full bg-warning" />
              {t("Running")}: <b className="text-foreground tabular-nums">{lws ? "—" : wfStats?.running ?? 0}</b>
            </span>
            <span className="flex items-center gap-1.5 text-xs text-muted">
              <span className="h-2 w-2 rounded-full bg-success" />
              {t("Approved")}: <b className="text-foreground tabular-nums">{lws ? "—" : wfStats?.approved ?? 0}</b>
            </span>
            <span className="flex items-center gap-1.5 text-xs text-muted">
              <span className="h-2 w-2 rounded-full bg-error" />
              {t("Rejected")}: <b className="text-foreground tabular-nums">{lws ? "—" : wfStats?.rejected ?? 0}</b>
            </span>
            <Link to="/workflow" className="text-xs font-medium text-primary hover:underline">
              {t("View all", "View all")}
            </Link>
          </div>
        </header>
        <div className="divide-y divide-border/60">
          {lwr && <p className="px-4 py-5 text-center text-sm text-muted">{t("Loading", "Loading")}…</p>}
          {!lwr && (wfRecent?.data?.length ?? 0) === 0 && (
            <p className="px-4 py-5 text-center text-sm text-muted">
              {t("No workflow requests yet.", "No workflow requests yet.")}
            </p>
          )}
          {wfRecent?.data?.map((w) => (
            <Link key={w.id} to="/workflow" className="flex items-center gap-3 px-4 py-2.5 transition-colors hover:bg-secondary/40">
              <span
                className={`shrink-0 rounded px-1.5 py-0.5 text-[11px] font-semibold ${
                  { Running: "bg-warning/15 text-warning", Approved: "bg-success/15 text-success", Rejected: "bg-error/15 text-error" }[w.status ?? ""] ?? "bg-muted/30 text-muted"
                }`}
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
      </section>

      {/* Workforce watchlists */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        {/* Employees on probation */}
        <section className="rounded-xl border border-border bg-card shadow-sm">
          <header className="flex items-center justify-between border-b border-border px-4 py-3">
            <h2 className="flex items-center gap-2 text-sm font-semibold text-foreground">
              <Hourglass className="h-4 w-4 text-primary" />
              {t("Employees on Probation")}
            </h2>
            <span className="rounded-full bg-warning/15 px-2 py-0.5 text-xs font-semibold text-warning tabular-nums">
              {lpr ? "—" : probation?.length ?? 0}
            </span>
          </header>
          <div className="divide-y divide-border/60">
            {lpr && <p className="px-4 py-5 text-center text-sm text-muted">{t("Loading", "Loading")}…</p>}
            {!lpr && (probation?.length ?? 0) === 0 && (
              <p className="px-4 py-5 text-center text-sm text-muted">
                {t("No employees on probation.", "No employees on probation.")}
              </p>
            )}
            {probation?.map((e) => (
              <Link key={e.id} to="/employee" className="flex items-center gap-3 px-4 py-2.5 transition-colors hover:bg-secondary/40">
                <div className="min-w-0 flex-1">
                  <p className="truncate text-[13px] font-medium text-foreground">{e.fullName}</p>
                  <p className="truncate text-xs text-muted">
                    {e.employeeNumber}
                    {e.positionTitle ? ` · ${e.positionTitle}` : ""}
                  </p>
                </div>
                <div className="shrink-0 text-right text-xs">
                  <p className="text-muted">{e.probationEndDate ? new Date(e.probationEndDate).toLocaleDateString() : "—"}</p>
                  {typeof e.daysRemaining === "number" && (
                    <p className={`text-[11px] font-medium ${e.daysRemaining < 0 ? "text-error" : e.daysRemaining <= 7 ? "text-warning" : "text-muted/80"}`}>
                      {e.daysRemaining < 0 ? `${-e.daysRemaining}d overdue` : `${e.daysRemaining}d left`}
                    </p>
                  )}
                </div>
              </Link>
            ))}
          </div>
        </section>

        {/* Upcoming retirements */}
        <section className="rounded-xl border border-border bg-card shadow-sm">
          <header className="flex items-center justify-between border-b border-border px-4 py-3">
            <h2 className="flex items-center gap-2 text-sm font-semibold text-foreground">
              <CalendarClock className="h-4 w-4 text-primary" />
              {t("Upcoming Retirements")}
            </h2>
            <span className="rounded-full bg-info/15 px-2 py-0.5 text-xs font-semibold text-info tabular-nums">
              {lrt ? "—" : retirements?.length ?? 0}
            </span>
          </header>
          <div className="divide-y divide-border/60">
            {lrt && <p className="px-4 py-5 text-center text-sm text-muted">{t("Loading", "Loading")}…</p>}
            {!lrt && (retirements?.length ?? 0) === 0 && (
              <p className="px-4 py-5 text-center text-sm text-muted">
                {t("No retirements within a month.", "No retirements within a month.")}
              </p>
            )}
            {retirements?.map((e) => (
              <Link key={e.id} to="/employee" className="flex items-center gap-3 px-4 py-2.5 transition-colors hover:bg-secondary/40">
                <div className="min-w-0 flex-1">
                  <p className="truncate text-[13px] font-medium text-foreground">{e.fullName}</p>
                  <p className="truncate text-xs text-muted">{e.employeeNumber}</p>
                </div>
                <div className="shrink-0 text-right text-xs">
                  <p className="text-muted">{new Date(e.retirementDate).toLocaleDateString()}</p>
                  <p className={`text-[11px] font-medium ${e.daysRemaining < 0 ? "text-error" : e.daysRemaining <= 14 ? "text-warning" : "text-muted/80"}`}>
                    {e.daysRemaining < 0 ? `${-e.daysRemaining}d overdue` : `${e.daysRemaining}d left`}
                  </p>
                </div>
              </Link>
            ))}
          </div>
        </section>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        {/* Recent activity (audit trail) */}
        <section className="rounded-xl border border-border bg-card shadow-sm lg:col-span-2">
          <header className="flex items-center justify-between border-b border-border px-4 py-3">
            <h2 className="flex items-center gap-2 text-sm font-semibold text-foreground">
              <ScrollText className="h-4 w-4 text-primary" />
              {t("Recent Activity", "Recent Activity")}
            </h2>
            <Link to="/auditLog" className="text-xs font-medium text-primary hover:underline">
              {t("View all", "View all")}
            </Link>
          </header>
          <div className="divide-y divide-border/60">
            {la && (
              <p className="px-4 py-6 text-center text-sm text-muted">{t("Loading", "Loading")}…</p>
            )}
            {!la && (activity?.data?.length ?? 0) === 0 && (
              <p className="px-4 py-6 text-center text-sm text-muted">
                {t("No activity recorded yet.", "No activity recorded yet.")}
              </p>
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
                    <span className="text-muted"> · {a.entityType}</span>
                  </p>
                </div>
                <div className="shrink-0 text-right">
                  <p className="text-xs text-muted">{a.performedBy || "—"}</p>
                  <p className="text-[11px] text-muted/80">{relativeTime(a.timestamp)}</p>
                </div>
              </div>
            ))}
          </div>
        </section>

        {/* Quick actions */}
        <section className="rounded-xl border border-border bg-card shadow-sm">
          <header className="border-b border-border px-4 py-3">
            <h2 className="flex items-center gap-2 text-sm font-semibold text-foreground">
              <GitFork className="h-4 w-4 text-primary" />
              {t("Quick Access", "Quick Access")}
            </h2>
          </header>
          <nav className="p-2">
            {QUICK_LINKS.map(({ to, label, Icon }) => (
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
        </section>
      </div>
    </div>
  );
}

export default memo(Dashboard);
