import { useCallback, useEffect, useMemo, useState } from "react";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { getDisplayColumns, getSelectableColumns } from "./listColumnUtils";

const STORAGE_PREFIX = "list-columns:";

function readStoredColumnNames(storageKey: string): string[] | null {
  try {
    const raw = localStorage.getItem(`${STORAGE_PREFIX}${storageKey}`);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as unknown;
    if (!Array.isArray(parsed)) return null;
    return parsed.filter((name): name is string => typeof name === "string");
  } catch {
    return null;
  }
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
    (names: string[] | null) => {
      if (!names?.length) return selectableColumns;
      const next = selectableColumns.filter((col) =>
        names.includes(col.name ?? col.key ?? ""),
      );
      return next.length > 0 ? next : selectableColumns;
    },
    [selectableColumns],
  );

  const [selectedCols, setSelectedCols] = useState<DataTableColumnModel[]>(() =>
    resolveSelection(storageKey ? readStoredColumnNames(storageKey) : null),
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
    const names = selectedCols
      .map((col) => col.name ?? col.key)
      .filter((name): name is string => Boolean(name));
    localStorage.setItem(`${STORAGE_PREFIX}${storageKey}`, JSON.stringify(names));
  }, [storageKey, selectedCols]);

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
