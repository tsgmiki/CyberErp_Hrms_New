import { useCallback, useEffect, useMemo, useState } from "react";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { getDisplayColumns, getSelectableColumns } from "./listColumnUtils";

const STORAGE_PREFIX = "list-columns:";

/**
 * Persisted column state. `visible` is what the user chose to show; `known` is every selectable
 * column that existed when the choice was saved. Keeping `known` lets us tell a column the user
 * deliberately hid (in `known`, not in `visible`) from one that was added to the app afterwards
 * (not in `known`) — the latter should appear by default rather than stay silently hidden.
 */
interface StoredColumnConfig {
  visible: string[];
  known: string[];
}

function readStoredConfig(storageKey: string): StoredColumnConfig | null {
  try {
    const raw = localStorage.getItem(`${STORAGE_PREFIX}${storageKey}`);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as unknown;
    // Legacy format: a plain array of visible column names. Treat those as the known set too,
    // so any column added since then is surfaced as new.
    if (Array.isArray(parsed)) {
      const names = parsed.filter((n): n is string => typeof n === "string");
      return { visible: names, known: names };
    }
    if (parsed && typeof parsed === "object") {
      const obj = parsed as { v?: unknown; k?: unknown };
      const visible = Array.isArray(obj.v)
        ? obj.v.filter((n): n is string => typeof n === "string")
        : [];
      const known = Array.isArray(obj.k)
        ? obj.k.filter((n): n is string => typeof n === "string")
        : visible;
      return { visible, known };
    }
    return null;
  } catch {
    return null;
  }
}

function colName(col: DataTableColumnModel): string {
  return (col.name ?? col.key ?? "").toString();
}

export interface UseListColumnSelectionOptions {
  /** Persists visible columns per list (e.g. `sales-orders`). */
  storageKey?: string;
}

export function useListColumnSelection(
  columns: DataTableColumnModel[],
  options?: UseListColumnSelectionOptions,
) {
  const storageKey = options?.storageKey;

  const columnSignature = useMemo(
    () => columns.map((col) => col.name ?? col.key ?? "").join("|"),
    [columns],
  );

  const selectableColumns = useMemo(
    () => getSelectableColumns(columns),
    [columnSignature, columns],
  );

  const resolveSelection = useCallback(
    (config: StoredColumnConfig | null) => {
      if (!config || config.visible.length === 0) return selectableColumns;
      // Show a column when the user had it visible OR it is new (was not present when the
      // config was saved). Columns the user explicitly hid stay hidden.
      const next = selectableColumns.filter((col) => {
        const name = colName(col);
        return config.visible.includes(name) || !config.known.includes(name);
      });
      return next.length > 0 ? next : selectableColumns;
    },
    [selectableColumns],
  );

  const [selectedCols, setSelectedCols] = useState<DataTableColumnModel[]>(() =>
    resolveSelection(storageKey ? readStoredConfig(storageKey) : null),
  );

  useEffect(() => {
    setSelectedCols((prev) => {
      const names = new Set(prev.map((col) => col.name ?? col.key));
      const next = selectableColumns.filter((col) => names.has(col.name ?? col.key));
      return next.length > 0 ? next : selectableColumns;
    });
  }, [columnSignature, selectableColumns]);

  useEffect(() => {
    if (!storageKey || selectedCols.length === 0) return;
    const visible = selectedCols
      .map((col) => col.name ?? col.key)
      .filter((name): name is string => Boolean(name));
    const known = selectableColumns
      .map((col) => col.name ?? col.key)
      .filter((name): name is string => Boolean(name));
    localStorage.setItem(
      `${STORAGE_PREFIX}${storageKey}`,
      JSON.stringify({ v: visible, k: known }),
    );
  }, [storageKey, selectedCols, selectableColumns]);

  const displayColumns = useMemo(
    () => getDisplayColumns(columns, selectedCols),
    [columns, selectedCols],
  );

  const resetColumns = useCallback(() => {
    setSelectedCols(selectableColumns);
    if (storageKey) {
      localStorage.removeItem(`${STORAGE_PREFIX}${storageKey}`);
    }
  }, [selectableColumns, storageKey]);

  return {
    selectableColumns,
    selectedCols,
    setSelectedCols,
    displayColumns,
    resetColumns,
  };
}
