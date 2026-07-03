import { ChevronDown, ChevronRight } from "lucide-react";
import type { ReactNode } from "react";

interface GroupTableHeaderProps {
  label: string;
  count: number;
  isCollapsed: boolean;
  onToggle: () => void;
  /** `bar` = full-width strip (list table); `card` = rounded section header (grid). */
  variant?: "bar" | "card";
}

export function GroupTableHeader({
  label,
  count,
  isCollapsed,
  onToggle,
  variant = "bar",
}: GroupTableHeaderProps) {
  const isCard = variant === "card";

  return (
    <button
      type="button"
      onClick={onToggle}
      className={`flex w-full items-center gap-2 text-left transition-colors ${
        isCard
          ? "border-b border-grid-divider bg-table-group-header px-3 py-2.5 hover:bg-table-group-header-hover"
          : "bg-table-group-header px-3 py-2.5 hover:bg-table-group-header-hover"
      }`}
    >
      {isCollapsed ? (
        <ChevronRight className="h-4 w-4 shrink-0 text-muted" />
      ) : (
        <ChevronDown className="h-4 w-4 shrink-0 text-muted" />
      )}
      <span className="min-w-0 flex-1 truncate text-xs font-semibold uppercase tracking-wide text-table-header">
        {label}
      </span>
      <CountBadge count={count} />
    </button>
  );
}

function CountBadge({ count }: { count: number }) {
  return (
    <span className="shrink-0 rounded-full border border-border bg-card px-2 py-0.5 text-[10px] font-medium text-muted">
      {count}
    </span>
  );
}

export function GroupSection({
  children,
  variant = "grid",
}: {
  children: ReactNode;
  variant?: "grid" | "table";
}) {
  if (variant === "table") {
    return <>{children}</>;
  }

  return (
    <section className="overflow-hidden rounded-lg border border-border bg-card shadow-sm">
      {children}
    </section>
  );
}

export function GroupBody({ children }: { children: ReactNode }) {
  return <div className="bg-background p-3">{children}</div>;
}
