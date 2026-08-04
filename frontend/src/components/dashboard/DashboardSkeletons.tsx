/**
 * Fixed-dimension skeleton placeholders for the dashboard. Each one matches its real widget's outer
 * frame (border, header height, row height) EXACTLY, so there is zero layout shift across every
 * phase: Suspense fallback (widget chunk still downloading) → widget mounted but its own query still
 * loading (KpiTile etc. render their own field-level skeleton inside the SAME frame) → real data.
 * A skeleton that doesn't match its widget's shape just moves the layout-shift problem instead of
 * solving it, so treat these dimensions as contractual, not decorative.
 */

const pulse = "animate-pulse rounded bg-muted/30";

/** Matches KpiTile's frame: rounded-xl border p-4, 36px icon circle, number line, label line. */
function KpiTileSkeleton() {
  return (
    <div className="rounded-xl border border-border bg-card p-4 shadow-sm">
      <div className={`h-9 w-9 rounded-lg ${pulse}`} />
      <div className={`mt-3 h-7 w-14 ${pulse}`} />
      <div className={`mt-1.5 h-3 w-20 ${pulse}`} />
    </div>
  );
}

/** The KPI strip — same responsive grid the real row uses, so the tile count/columns never jump. */
export function KpiRowSkeleton({ count = 6 }: { count?: number }) {
  return (
    <div
      className={`grid grid-cols-2 gap-3 sm:grid-cols-3 ${count >= 7 ? "xl:grid-cols-7" : "xl:grid-cols-6"}`}
    >
      {Array.from({ length: count }).map((_, i) => (
        <KpiTileSkeleton key={i} />
      ))}
    </div>
  );
}

/** Matches Card's frame: header (border-b, px-4 py-3) + N divide-y rows in the body. */
export function CardSkeleton({ rows = 4, tall = false }: { rows?: number; tall?: boolean }) {
  return (
    <section className="flex flex-col overflow-hidden rounded-xl border border-border bg-card shadow-sm">
      <header className="flex items-center justify-between gap-3 border-b border-border px-4 py-3">
        <div className={`h-4 w-36 ${pulse}`} />
        <div className={`h-4 w-16 ${pulse}`} />
      </header>
      <div className="divide-y divide-border/60">
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className={`flex items-center gap-3 px-4 ${tall ? "py-3" : "py-2.5"}`}>
            <div className={`h-5 w-12 shrink-0 ${pulse}`} />
            <div className="min-w-0 flex-1 space-y-1.5">
              <div className={`h-3.5 w-2/3 ${pulse}`} />
              <div className={`h-3 w-1/3 ${pulse}`} />
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}

/** Matches QuickAccessWidget's compact single-line nav rows (icon + label + chevron, p-2 list). */
export function NavListSkeleton({ rows = 6 }: { rows?: number }) {
  return (
    <section className="flex flex-col overflow-hidden rounded-xl border border-border bg-card shadow-sm">
      <header className="flex items-center gap-3 border-b border-border px-4 py-3">
        <div className={`h-4 w-24 ${pulse}`} />
      </header>
      <div className="space-y-1 p-2">
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className="flex items-center gap-2.5 rounded-lg px-2.5 py-2">
            <div className={`h-4 w-4 shrink-0 rounded ${pulse}`} />
            <div className={`h-3.5 w-28 ${pulse}`} />
          </div>
        ))}
      </div>
    </section>
  );
}

/** The tabbed watchlist card: tab bar + rows, same chrome as WorkforceWatchlistWidget/ActionQueueWidget. */
export function TabbedCardSkeleton({ tabs = 2, rows = 4 }: { tabs?: number; rows?: number }) {
  return (
    <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
      <div className="flex items-center gap-4 border-b border-border px-3 pt-1.5 pb-2.5">
        {Array.from({ length: tabs }).map((_, i) => (
          <div key={i} className={`h-4 w-24 ${pulse}`} />
        ))}
      </div>
      <div className="divide-y divide-border/60">
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className="flex items-center gap-3 px-4 py-3">
            <div className="min-w-0 flex-1 space-y-1.5">
              <div className={`h-3.5 w-1/2 ${pulse}`} />
              <div className={`h-3 w-1/3 ${pulse}`} />
            </div>
            <div className={`h-8 w-20 shrink-0 rounded-lg ${pulse}`} />
          </div>
        ))}
      </div>
    </section>
  );
}
