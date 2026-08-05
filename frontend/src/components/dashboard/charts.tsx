/**
 * Presentational chart primitives for the dashboard — hand-rolled SVG/CSS, ZERO dependencies and
 * zero data access. Every series is passed in by a caller from a query the dashboard ALREADY runs,
 * so nothing here adds a network call, a hook, or a re-render source.
 *
 * Honesty rule: these render only what they are given. When a total is 0 they draw an explicit
 * empty track rather than inventing a shape — a chart that fakes data is worse than no chart.
 */
import type { ReactNode } from "react";

export type Slice = { label: string; value: number; color: string };

/** Ring chart. `color` values are CSS custom properties (var(--success)) so themes follow along. */
export function DonutChart({
  slices,
  size = 132,
  thickness = 15,
  centerValue,
  centerLabel,
}: {
  slices: Slice[];
  size?: number;
  thickness?: number;
  centerValue?: ReactNode;
  centerLabel?: string;
}) {
  const total = slices.reduce((sum, s) => sum + s.value, 0);
  const radius = (size - thickness) / 2;
  const circumference = 2 * Math.PI * radius;
  const mid = size / 2;

  let consumed = 0;
  return (
    <div className="relative shrink-0" style={{ width: size, height: size }}>
      <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} aria-hidden="true">
        {/* Track — also the entire chart when there is nothing to plot. */}
        <circle
          cx={mid}
          cy={mid}
          r={radius}
          fill="none"
          strokeWidth={thickness}
          stroke="color-mix(in srgb, var(--border) 70%, transparent)"
        />
        {total > 0 &&
          slices
            .filter((s) => s.value > 0)
            .map((s) => {
              const length = (s.value / total) * circumference;
              const dash = (
                <circle
                  key={s.label}
                  cx={mid}
                  cy={mid}
                  r={radius}
                  fill="none"
                  strokeWidth={thickness}
                  stroke={s.color}
                  strokeDasharray={`${length} ${circumference - length}`}
                  strokeDashoffset={-consumed}
                  transform={`rotate(-90 ${mid} ${mid})`}
                />
              );
              consumed += length;
              return dash;
            })}
      </svg>
      <div className="absolute inset-0 flex flex-col items-center justify-center gap-0.5">
        <span className="text-[22px] font-bold leading-none tracking-tight text-foreground tabular-nums">
          {centerValue}
        </span>
        {centerLabel && (
          <span className="text-[10px] font-semibold uppercase tracking-wider text-muted">{centerLabel}</span>
        )}
      </div>
    </div>
  );
}

/** Dot + label + count legend, sized to sit beside a {@link DonutChart}. */
export function ChartLegend({ slices, total }: { slices: Slice[]; total: number }) {
  return (
    <ul className="min-w-0 flex-1 space-y-2">
      {slices.map((s) => (
        <li key={s.label} className="flex items-center gap-2.5">
          <span className="h-2.5 w-2.5 shrink-0 rounded-sm" style={{ backgroundColor: s.color }} />
          <span className="min-w-0 flex-1 truncate text-[12px] text-label">{s.label}</span>
          <span className="shrink-0 text-[12px] font-semibold text-foreground tabular-nums">{s.value}</span>
          <span className="w-9 shrink-0 text-right text-[11px] text-muted tabular-nums">
            {total > 0 ? `${Math.round((s.value / total) * 100)}%` : "—"}
          </span>
        </li>
      ))}
    </ul>
  );
}

/**
 * Horizontal magnitude bars. Bars are scaled to the LARGEST item (so small categories stay legible)
 * while the percentage is of the TOTAL — the pairing ERP dashboards use for composition breakdowns.
 */
export function BarBreakdown({ items }: { items: Slice[] }) {
  const total = items.reduce((sum, i) => sum + i.value, 0);
  const max = Math.max(1, ...items.map((i) => i.value));

  return (
    <ul className="space-y-2.5">
      {items.map((i) => (
        <li key={i.label}>
          <div className="flex items-baseline justify-between gap-3">
            <span className="min-w-0 truncate text-[12px] font-medium text-label">{i.label}</span>
            <span className="shrink-0 text-[12px] font-semibold text-foreground tabular-nums">
              {i.value}
              <span className="ml-1.5 text-[11px] font-normal text-muted">
                {total > 0 ? `${Math.round((i.value / total) * 100)}%` : "—"}
              </span>
            </span>
          </div>
          <div
            className="mt-1.5 h-2 w-full overflow-hidden rounded-full"
            style={{ backgroundColor: "color-mix(in srgb, var(--border) 65%, transparent)" }}
          >
            <div
              className="h-full rounded-full"
              style={{ width: `${(i.value / max) * 100}%`, backgroundColor: i.color }}
            />
          </div>
        </li>
      ))}
    </ul>
  );
}
