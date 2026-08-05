/**
 * Fixed-dimension skeleton placeholders for the dashboard. Each one matches its real widget's outer
 * frame (border, header height, row height) EXACTLY, so there is zero layout shift across every
 * phase: Suspense fallback (widget chunk still downloading) → widget mounted but its own query still
 * loading (KpiTile etc. render their own field-level skeleton inside the SAME frame) → real data.
 * A skeleton that doesn't match its widget's shape just moves the layout-shift problem instead of
 * solving it, so treat these dimensions as contractual, not decorative.
 */
import { HAIRLINE, RECESSED, SURFACE } from "./shared";

const pulse = "animate-pulse rounded bg-muted/30";

/** Matches KpiTile: p-4, [14px label ⇄ 32px icon] row, mt-3, 32px metric. */
function KpiTileSkeleton() {
  return (
    <div className={`flex items-center gap-2.5 py-2.5 pl-4 pr-3 ${SURFACE}`}>
      <div className="min-w-0 flex-1">
        <div className={`h-3 w-16 ${pulse}`} />
        <div className={`mt-1 h-6 w-12 ${pulse}`} />
      </div>
      <div className={`h-7 w-7 shrink-0 rounded-md ${pulse}`} />
    </div>
  );
}

/** The KPI strip — same responsive grid the real row uses, so the tile count/columns never jump. */
export function KpiRowSkeleton({ count = 6 }: { count?: number }) {
  return (
    <div
      className={`grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:gap-4 ${count >= 7 ? "xl:grid-cols-7" : "xl:grid-cols-6"}`}
    >
      {Array.from({ length: count }).map((_, i) => (
        <KpiTileSkeleton key={i} />
      ))}
    </div>
  );
}

/** Matches CardHeader exactly: border-b, px-4 py-2, inline icon + micro-caps title. */
function HeaderSkeleton() {
  return (
    <header className={`flex items-center justify-between gap-3 border-b px-4 py-2 ${HAIRLINE}`}>
      <div className="flex items-center gap-2">
        <div className={`h-4 w-4 shrink-0 ${pulse}`} />
        <div className={`h-3.5 w-28 ${pulse}`} />
      </div>
      <div className={`h-4 w-16 ${pulse}`} />
    </header>
  );
}

/** The uppercase column-header strip that now sits above every feed. */
function TableHeadSkeleton({ cols = 3 }: { cols?: number }) {
  return (
    <div className={`flex items-center gap-3 border-b px-4 py-1.5 ${HAIRLINE} bg-muted/50`}>
      {Array.from({ length: cols }).map((_, i) => (
        <div key={i} className={`h-2.5 ${i === 1 ? "flex-1" : "w-14"} ${pulse}`} />
      ))}
    </div>
  );
}

/** The two analytics cards (donut + bars) in their real md:grid-cols-2 band. */
export function AnalyticsBandSkeleton() {
  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
      <section className={`flex flex-col overflow-hidden ${SURFACE}`}>
        <HeaderSkeleton />
        <div className="flex flex-1 items-center gap-5 p-4">
          <div className={`h-[132px] w-[132px] shrink-0 rounded-full ${pulse}`} />
          <div className="flex-1 space-y-3">
            {[0, 1, 2].map((i) => (
              <div key={i} className={`h-3.5 w-full ${pulse}`} />
            ))}
          </div>
        </div>
      </section>
      <section className={`flex flex-col overflow-hidden ${SURFACE}`}>
        <HeaderSkeleton />
        <div className="flex-1 p-4">
          <div className={`mb-4 h-8 w-24 ${pulse}`} />
          <div className="space-y-5">
            {[0, 1, 2].map((i) => (
              <div key={i} className="space-y-2">
                <div className={`h-3.5 w-1/3 ${pulse}`} />
                <div className={`h-2 w-full rounded-full ${pulse}`} />
              </div>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}

/** Matches Card's frame: header + N divide-y rows in the body. */
export function CardSkeleton({ rows = 4, tall = false }: { rows?: number; tall?: boolean }) {
  return (
    <section className={`flex flex-col overflow-hidden ${SURFACE}`}>
      <HeaderSkeleton />
      <TableHeadSkeleton />
      <div className={`divide-y ${HAIRLINE}`}>
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className={`flex items-center gap-3 px-4 ${tall ? "py-2.5" : "py-2"}`}>
            <div className={`h-4 w-16 shrink-0 ${pulse}`} />
            <div className="min-w-0 flex-1 space-y-1">
              <div className={`h-3 w-2/3 ${pulse}`} />
              <div className={`h-2.5 w-1/3 ${pulse}`} />
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
    <section className={`flex flex-col overflow-hidden ${SURFACE}`}>
      <HeaderSkeleton />
      <div className="space-y-1 p-2">
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className="flex items-center gap-2.5 rounded-md px-2.5 py-2">
            <div className={`h-4 w-4 shrink-0 rounded ${pulse}`} />
            <div className={`h-3.5 w-28 ${pulse}`} />
          </div>
        ))}
      </div>
    </section>
  );
}

/** The tabbed cards: header + segmented tab rail + rows — same chrome as the real widgets. */
export function TabbedCardSkeleton({ tabs = 2, rows = 4 }: { tabs?: number; rows?: number }) {
  return (
    <section className={`flex flex-col overflow-hidden ${SURFACE}`}>
      <HeaderSkeleton />
      {/* py-1.5 + 30px pill = the real rail's height */}
      <div className={`flex items-center gap-1 border-b px-2 py-1.5 ${HAIRLINE} ${RECESSED}`}>
        {Array.from({ length: tabs }).map((_, i) => (
          <div key={i} className={`h-[30px] w-28 rounded-md ${pulse}`} />
        ))}
      </div>
      <div className={`divide-y ${HAIRLINE}`}>
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className="flex items-center gap-3 px-4 py-2">
            <div className="min-w-0 flex-1 space-y-1">
              <div className={`h-3 w-1/2 ${pulse}`} />
              <div className={`h-2.5 w-1/3 ${pulse}`} />
            </div>
            <div className={`h-6 w-16 shrink-0 rounded-md ${pulse}`} />
          </div>
        ))}
      </div>
    </section>
  );
}
