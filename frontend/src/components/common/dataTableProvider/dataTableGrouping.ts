export interface DataTableGroup<T = Record<string, unknown>> {
  key: string;
  label: string;
  rows: T[];
}

export function groupTableRows<T extends Record<string, unknown>>(
  data: T[] | undefined,
  groupBy?: string,
  formatLabel?: (key: string, rows: T[]) => string,
): DataTableGroup<T>[] | null {
  if (!groupBy || !data?.length) return null;

  const map = new Map<string, T[]>();

  for (const row of data) {
    const raw = row[groupBy];
    const key =
      raw == null || raw === ""
        ? ""
        : typeof raw === "object"
          ? JSON.stringify(raw)
          : String(raw);

    const bucket = map.get(key);
    if (bucket) {
      bucket.push(row);
    } else {
      map.set(key, [row]);
    }
  }

  return Array.from(map.entries())
    .sort(([a], [b]) => a.localeCompare(b, undefined, { sensitivity: "base" }))
    .map(([key, rows]) => ({
      key,
      label: formatLabel ? formatLabel(key, rows) : key || "—",
      rows,
    }));
}
