import { useCallback, useMemo, type ReactElement } from "react";
import { useTranslation } from "react-i18next";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import ListViewToolbar, {
  type ListDisplayMode,
  type ListExportConfig,
} from "./listViewToolbar";
import {
  listExportMenuItems,
  type ListExportFormatId,
} from "./listExportMenu";
import { useListColumnSelection } from "./useListColumnSelection";
import { useListPermissions } from "./useListPermissions";
import { toast } from "../toast";

const MAX_EXPORT_ROWS = 10_000;

export interface UseListPageOptions {
  /** Unique key for column persistence (`list-columns:{listKey}`). */
  listKey: string;
  /** Translated list title (export file name + menu labels). */
  listLabel: string;
  columns: DataTableColumnModel[];
  data: Record<string, unknown>[] | undefined;
  isLoading?: boolean;
  displayMode: ListDisplayMode;
  onDisplayModeChange: (mode: ListDisplayMode) => void;
  /** Total records for current filters (enables export-all menu items). */
  totalCount?: number;
  /** Fetches all rows matching current filters (skip/take overridden). */
  fetchAllData?: () => Promise<Record<string, unknown>[] | undefined>;
}

export function useListPage({
  listKey,
  listLabel,
  columns,
  data,
  isLoading = false,
  displayMode,
  onDisplayModeChange,
  totalCount,
  fetchAllData,
}: UseListPageOptions) {
  const { t } = useTranslation();
  const { canExport, canConfigureColumns } = useListPermissions();

  const {
    selectableColumns,
    selectedCols,
    setSelectedCols,
    displayColumns,
    resetColumns,
  } = useListColumnSelection(columns, { storageKey: listKey });

  const getExportData = useCallback(
    async (formatId: ListExportFormatId) => {
      const useAll =
        (formatId === "excel-all" || formatId === "pdf-all") && fetchAllData;

      if (useAll) {
        const rows = await fetchAllData();
        if ((rows?.length ?? 0) > MAX_EXPORT_ROWS) {
          toast.warning(
            t("Export limited to {{max}} rows", { max: MAX_EXPORT_ROWS }),
          );
          return rows?.slice(0, MAX_EXPORT_ROWS);
        }
        return rows;
      }

      return data;
    },
    [data, fetchAllData, t],
  );

  const exportConfig = useMemo<ListExportConfig | undefined>(() => {
    if (!canExport) return undefined;

    return {
      title: listLabel,
      items: listExportMenuItems(t, listLabel, {
        includeAll: Boolean(fetchAllData),
        totalCount,
      }),
      data,
      columns: selectedCols,
      disabled: isLoading,
      getExportData,
    };
  }, [
    canExport,
    listLabel,
    t,
    fetchAllData,
    totalCount,
    data,
    selectedCols,
    isLoading,
    getExportData,
  ]);

  const toolbarEnd = useMemo(
    () => (
      <ListViewToolbar
        displayMode={displayMode}
        onDisplayModeChange={onDisplayModeChange}
        columnConfig={
          canConfigureColumns
            ? {
                columns: selectableColumns,
                selectedCols,
                onChange: setSelectedCols,
                onReset: resetColumns,
                disabled: isLoading,
              }
            : undefined
        }
        exportConfig={exportConfig}
      />
    ),
    [
      displayMode,
      onDisplayModeChange,
      canConfigureColumns,
      selectableColumns,
      selectedCols,
      setSelectedCols,
      resetColumns,
      isLoading,
      exportConfig,
    ],
  );

  return {
    displayColumns,
    toolbarEnd: toolbarEnd as ReactElement,
    selectedCols,
    exportConfig,
    canExport,
    canConfigureColumns,
  };
}
