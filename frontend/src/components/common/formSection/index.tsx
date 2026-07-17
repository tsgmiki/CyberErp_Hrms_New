"use client";
import type { ReactNode } from "react";
import { Plus, Trash2, type LucideIcon } from "lucide-react";

/** A titled card section for grouping fields inside a modal / form (design-standard look). */
export function FormSection({
  title,
  description,
  icon: Icon,
  action,
  children,
  className = "",
}: {
  title: string;
  description?: string;
  icon?: LucideIcon;
  action?: ReactNode;
  children: ReactNode;
  className?: string;
}) {
  return (
    <section className={`rounded-xl border border-border bg-card p-4 shadow-sm ${className}`}>
      <div className="mb-3 flex items-center gap-2.5">
        {Icon ? (
          <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
            <Icon size={15} />
          </span>
        ) : null}
        <div className="min-w-0">
          <h4 className="text-sm font-semibold text-foreground">{title}</h4>
          {description ? <p className="text-xs text-muted">{description}</p> : null}
        </div>
        {action ? <div className="ml-auto shrink-0">{action}</div> : null}
      </div>
      {children}
    </section>
  );
}

/** Column-header row above a set of repeat rows (aligns with the row grid template). */
export function RepeatHeader({ cols, className = "" }: { cols: string[]; className?: string }) {
  return (
    <div className={`hidden items-center gap-2 px-1 pb-1 text-[11px] font-medium uppercase tracking-wide text-muted sm:grid ${className}`}>
      {cols.map((c, i) => (
        <span key={i}>{c}</span>
      ))}
    </div>
  );
}

/** One repeat row shell (a subtle bordered strip). Pass the grid template via `className`. */
export function RepeatRow({ children, className = "" }: { children: ReactNode; className?: string }) {
  return (
    <div className={`grid items-center gap-2 rounded-lg border border-border/70 bg-background/50 p-2 transition-colors hover:border-border ${className}`}>
      {children}
    </div>
  );
}

export function SectionAddButton({ onClick, label }: { onClick: () => void; label: string }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="inline-flex items-center gap-1 rounded-lg border border-primary/40 bg-primary/10 px-2.5 py-1.5 text-xs font-semibold text-primary transition-colors hover:bg-primary/15"
    >
      <Plus size={13} /> {label}
    </button>
  );
}

export function RowRemoveButton({ onClick }: { onClick: () => void }) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-label="Remove"
      className="flex h-9 w-9 items-center justify-center rounded-lg text-muted transition-colors hover:bg-error/10 hover:text-error"
    >
      <Trash2 size={15} />
    </button>
  );
}

export function EmptyHint({ children }: { children: ReactNode }) {
  return (
    <p className="rounded-lg border border-dashed border-border/70 bg-card/40 px-3 py-5 text-center text-xs text-muted">
      {children}
    </p>
  );
}
