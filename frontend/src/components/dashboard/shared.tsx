import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Inbox } from "lucide-react";

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

/* ---------- design tokens ----------
 * NOTE FOR FUTURE EDITS: this app does NOT feed its palette into Tailwind's theme — every colour
 * utility (`bg-primary/10`, `border-success/20`, …) is hand-written in `src/config/theme.css`.
 * A step that isn't in that file (`bg-secondary/40`, `bg-border`, `hover:border-primary/30`)
 * compiles to NOTHING and renders transparent. Anything outside the hand-written set below must be
 * written as an arbitrary value bound to the CSS variable, which Tailwind always generates.
 */
export const HAIRLINE = "border-[var(--border)]";
export const ROW_DIVIDE = `divide-y ${HAIRLINE}`;
/** Zebra/hover wash — a hint of the brand tint, not the flat pale-blue block `bg-secondary` gives. */
export const ROW_HOVER = "hover:bg-[color-mix(in_srgb,var(--secondary)_55%,transparent)]";
/** Recessed strip used for tab rails and card footers. */
export const RECESSED = "bg-[color-mix(in_srgb,var(--secondary)_45%,var(--card))]";

/** The single surface definition every card and tile shares: identical radius, hairline and lift. */
export const SURFACE =
  "rounded-lg border border-border bg-card shadow-[0_1px_2px_rgba(15,23,42,0.04),0_1px_3px_rgba(15,23,42,0.05)]";

/* ---------- data-table language ----------
 * Lifted from the app's own list screens (`dataTableProvider`) so the dashboard reads as part of
 * the system rather than a bespoke page: uppercase micro-caps column headers over a tinted strip,
 * column-aligned rows, hairline dividers. Padding is one step tighter than the full list screens
 * because these are digest feeds, not working grids.
 */
export const TABLE_HEAD = `grid items-center gap-3 border-b px-4 py-1.5 ${HAIRLINE} bg-muted/50`;
export const TH = "truncate text-[10px] font-semibold uppercase tracking-wide text-muted";
export const TABLE_ROW = `grid items-center gap-3 px-4 py-2 transition-colors ${ROW_HOVER}`;
/** Primary cell text — the one thing in a row that must be instantly readable. */
export const TD_STRONG = "truncate text-[12px] font-medium text-foreground";
export const TD = "truncate text-[11px] text-muted";

/** Section eyebrow + rule — the band that splits the page into scannable zones. */
export function SectionLabel({ children, action }: { children: ReactNode; action?: ReactNode }) {
  return (
    <div className="mb-2 flex items-center gap-3">
      <h2 className="shrink-0 text-[11px] font-semibold uppercase tracking-[0.11em] text-muted">{children}</h2>
      <span className={`h-0 flex-1 border-t ${HAIRLINE}`} />
      {action}
    </div>
  );
}

/* ---------- tiles & cards ---------- */

const TILE_TONE = {
  primary: { icon: "bg-primary/10 text-primary", rail: "var(--primary)" },
  warning: { icon: "bg-warning/15 text-warning", rail: "var(--warning)" },
  info: { icon: "bg-info/15 text-info", rail: "var(--info)" },
} as const;

/**
 * KPI tile. The metric is the loudest thing in the tile; the label rides above it in small caps so
 * a two-line label can never shove the number off its baseline (the icon fixes that row's height).
 */
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
  const toneClass = TILE_TONE[tone];
  return (
    <Link
      to={to}
      className={`focus-ring group relative flex items-center gap-2.5 overflow-hidden py-2.5 pl-4 pr-3 transition-shadow duration-150 hover:shadow-[0_4px_12px_rgba(15,23,42,0.08)] ${SURFACE}`}
    >
      {/* Metric and label stack in a single tight column so the tile is ~64px instead of ~110px —
          seven of these fit one row without the strip eating a third of the viewport. */}
      <div className="min-w-0 flex-1">
        {/* Two lines allowed: "Organization Units" / "Change Requests" truncate to nonsense on one.
            Grid items stretch to the tallest in the row, so every tile stays the same height. */}
        <span className="line-clamp-2 min-h-[26px] text-[10px] font-semibold uppercase leading-[13px] tracking-wide text-muted">
          {t(label)}
        </span>
        {isLoading ? (
          <div className="mt-1 h-6 w-12 animate-pulse rounded bg-muted/30" />
        ) : (
          <p className="text-[22px] font-bold leading-7 tracking-tight text-foreground tabular-nums">
            {total ?? 0}
          </p>
        )}
      </div>
      <span className={`flex h-7 w-7 shrink-0 items-center justify-center rounded-md ${toneClass.icon}`}>
        {icon}
      </span>
      <span
        className="absolute inset-y-0 left-0 w-[3px] opacity-70 transition-opacity duration-150 group-hover:opacity-100"
        style={{ backgroundColor: toneClass.rail }}
      />
    </Link>
  );
}

/** Shared card chrome so `Card` and `ChartCard` are pixel-identical. */
function CardHeader({ title, icon, action }: { title: string; icon?: ReactNode; action?: ReactNode }) {
  return (
    <header className={`flex flex-wrap items-center justify-between gap-x-3 gap-y-1.5 border-b px-4 py-2 ${HAIRLINE}`}>
      <h3 className="flex min-w-0 items-center gap-2 text-[12px] font-semibold uppercase tracking-wide text-foreground">
        {icon && <span className="shrink-0 text-primary">{icon}</span>}
        <span className="truncate">{title}</span>
      </h3>
      {action}
    </header>
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
    <section className={`flex flex-col overflow-hidden ${SURFACE}`}>
      <CardHeader title={title} icon={icon} action={action} />
      {children}
    </section>
  );
}

/** Card whose body is a chart rather than a row feed — gets padding instead of dividers. */
export function ChartCard({
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
    <section className={`flex flex-col overflow-hidden ${SURFACE}`}>
      <CardHeader title={title} icon={icon} action={action} />
      <div className="flex flex-1 items-center p-4">{children}</div>
    </section>
  );
}

/** Compact status counter for card headers; wraps to its own line instead of colliding with a title. */
export function StatChip({
  tone,
  label,
  value,
}: {
  tone: "warning" | "success" | "error" | "info";
  label: string;
  value: ReactNode;
}) {
  const dot = { warning: "bg-warning", success: "bg-success", error: "bg-error", info: "bg-info" }[tone];
  return (
    <span
      className={`inline-flex items-center gap-1.5 whitespace-nowrap rounded-md border px-2 py-1 text-[11px] font-medium text-muted ${HAIRLINE} ${RECESSED}`}
    >
      <span className={`h-1.5 w-1.5 shrink-0 rounded-full ${dot}`} />
      {label}
      <b className="text-foreground tabular-nums">{value}</b>
    </span>
  );
}

/** Deliberate, compact empty state — a lone sentence in a tall blank box read as broken, which was
 * a large part of why the page felt hollow. */
export function EmptyRow({ text }: { text: string }) {
  return (
    <div className="flex items-center justify-center gap-2 px-4 py-5 text-center">
      <Inbox className="h-3.5 w-3.5 shrink-0 text-muted" />
      <p className="text-[12px] text-muted">{text}</p>
    </div>
  );
}

export function DaysBadge({ days, warnAt }: { days?: number | null; warnAt: number }) {
  if (typeof days !== "number") return null;
  const cls =
    days < 0
      ? "border-error/20 bg-error/15 text-error"
      : days <= warnAt
        ? "border-warning/20 bg-warning/15 text-warning"
        : `${HAIRLINE} ${RECESSED} text-muted`;
  return (
    <span className={`mt-1 inline-block rounded border px-1.5 py-0.5 text-[11px] font-semibold tabular-nums ${cls}`}>
      {days < 0 ? `${-days}d overdue` : `${days}d left`}
    </span>
  );
}
