import type { LucideIcon } from "lucide-react";
import { resolveStatusTone } from "../badge/resolveStatusTone";
import type { StatusCardItem } from "./types";

export interface AggregateStatusOptions<T> {
  /** Field used to group rows (e.g. `"status"`). */
  groupField: keyof T & string;
  /** Row id for the “all” summary card. */
  allId?: string;
  allTitle?: string;
  allIcon?: LucideIcon;
  /** Extra non-filterable cards appended at the end. */
  extraCards?: StatusCardItem[];
  /** Max status-specific cards (sorted by count desc). */
  maxStatusCards?: number;
}

export function buildStatusCountCards<T extends Record<string, unknown>>(
  rows: T[] | undefined,
  options: AggregateStatusOptions<T>,
): StatusCardItem[] {
  const data = rows ?? [];
  const {
    groupField,
    allId = "all",
    allTitle = "Total",
    allIcon,
    extraCards = [],
    maxStatusCards = 5,
  } = options;

  const counts = new Map<string, number>();
  for (const row of data) {
    const raw = row[groupField];
    const key =
      raw == null || raw === ""
        ? "—"
        : typeof raw === "string"
          ? raw
          : String(raw);
    counts.set(key, (counts.get(key) ?? 0) + 1);
  }

  const statusCards: StatusCardItem[] = Array.from(counts.entries())
    .sort((a, b) => b[1] - a[1])
    .slice(0, maxStatusCards)
    .map(([status, count]) => ({
      id: status,
      title: status,
      value: count,
      variant: resolveStatusTone(status === "—" ? "" : status),
      filterable: true,
    }));

  return [
    {
      id: allId,
      title: allTitle,
      value: data.length,
      icon: allIcon,
      variant: "default",
      filterable: true,
    },
    ...statusCards,
    ...extraCards,
  ];
}

export function formatStatusCardMoney(value: number): string {
  if (value >= 1_000_000) return `$${(value / 1_000_000).toFixed(1)}M`;
  if (value >= 1_000) return `$${(value / 1_000).toFixed(0)}K`;
  return `$${value.toLocaleString(undefined, { maximumFractionDigits: 0 })}`;
}
