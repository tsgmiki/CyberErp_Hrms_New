import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ArrowUpRight } from "lucide-react";

export type ClearanceDecision = "Cleared" | "Blocked";
export type ApprovalVerb = "approve" | "reject";

export const ACTION_TONE: Record<string, string> = {
  Created: "bg-success/15 text-success",
  Modified: "bg-info/15 text-info",
  Reassigned: "bg-warning/15 text-warning",
  Deleted: "bg-error/15 text-error",
  Rejected: "bg-muted/30 text-muted",
};

export const WF_TONE: Record<string, string> = {
  Running: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
};

export function relativeTime(iso?: string): string {
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

export function KpiTile({
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
      className="group rounded-xl border border-border bg-card p-4 shadow-sm transition-all hover:border-primary/40 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40"
    >
      <div className="flex items-start justify-between">
        <span className={`flex h-9 w-9 items-center justify-center rounded-lg ${toneClass}`}>{icon}</span>
        <ArrowUpRight className="h-3.5 w-3.5 text-muted/50 opacity-0 transition-opacity group-hover:opacity-100" />
      </div>
      {isLoading ? (
        <div className="mt-3 h-7 w-14 animate-pulse rounded bg-muted/30" />
      ) : (
        <p className="mt-3 text-2xl font-bold tracking-tight text-foreground tabular-nums">{total ?? 0}</p>
      )}
      <p className="mt-1.5 truncate text-xs font-medium text-muted">{t(label)}</p>
    </Link>
  );
}

export function Card({
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
    <section className="flex flex-col overflow-hidden rounded-xl border border-border bg-card shadow-sm">
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

export function EmptyRow({ text }: { text: string }) {
  return <p className="px-4 py-8 text-center text-sm text-muted">{text}</p>;
}

export function DaysBadge({ days, warnAt }: { days?: number | null; warnAt: number }) {
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
