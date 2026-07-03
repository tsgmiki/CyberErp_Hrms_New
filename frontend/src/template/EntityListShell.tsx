"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import GridDataTableProvider from "@/components/common/dataTableProvider/gridDataTableProvider";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type ParameterModel from "@/models/ParameterModel";
import { useListPage } from "@/components/common/dataTableProvider/useListPage";
import type { ListDisplayMode } from "@/components/common/dataTableProvider/listViewToolbar";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import type { ReactNode } from "react";
export interface EntityListShellProps {
  listKey: string;
  /** i18n key or plain label for exports */
  listLabel: string;
  columns: DataTableColumnModel[];
  isLoading: boolean;
  rows?: unknown[];
  total: number;
  param: ParameterModel;
  setParam: (
    updater: ParameterModel | ((prev: ParameterModel) => ParameterModel),
  ) => void;
  displayMode: ListDisplayMode;
  setDisplayMode: (mode: ListDisplayMode) => void;
  fetchAllData: () => Promise<Record<string, unknown>[]>;
  listFilters?: ListFilterDefinition[];
  searchBarFilters?: ReactNode;
  header?: ReactNode;
  checkBox?: boolean;
  groupBy?: string;
  getGroupLabel?: (groupKey: string, rows: Record<string, unknown>[]) => string;
  rowKey?: string;
  className?: string;
}

/**
 * List UI shell: column selector, export, list/grid toggle, and data table.
 * Pair with {@link useEntityList} when columns depend on edit/delete handlers.
 */
export function EntityListShell({
  listKey,
  listLabel,
  columns,
  isLoading,
  rows,
  total,
  param,
  setParam,
  displayMode,
  setDisplayMode,
  fetchAllData,
  listFilters,
  searchBarFilters,
  header,
  checkBox,
  groupBy,
  getGroupLabel,
  rowKey = "id",
  className = "m-2 flex h-full min-h-0 flex-col gap-3",
}: EntityListShellProps) {
  const { t } = useTranslation();
  const [checkList, setCheckList] = useState<string[]>([]);
  const resolvedLabel = t(listLabel);

  const { displayColumns, toolbarEnd } = useListPage({
    listKey,
    listLabel: resolvedLabel,
    columns,
    data: rows as Record<string, unknown>[] | undefined,
    isLoading,
    displayMode,
    onDisplayModeChange: setDisplayMode,
    totalCount: total,
    fetchAllData,
  });

  const dataTableProps = useMemo(
    () => ({
      isLoading,
      columns: displayColumns as never,
      data: rows as never,
      pagination: "Visible" as const,
      search: "Visible" as const,
      listFilters,
      searchBarFilters,
      count: total,
      param,
      setParam,
      key: rowKey,
      checkBox,
      checkList: checkBox ? checkList : undefined,
      checkHandler: checkBox ? setCheckList : undefined,
      groupBy,
      getGroupLabel,
      toolbarEnd,
    }),
    [
      isLoading,
      displayColumns,
      rows,
      total,
      param,
      listFilters,
      searchBarFilters,
      rowKey,
      checkBox,
      checkList,
      groupBy,
      getGroupLabel,
      toolbarEnd,
    ],
  );

  const table =
    displayMode === "grid" ? (
      <GridDataTableProvider dataTable={dataTableProps} />
    ) : (
      <DataTableProvider dataTable={dataTableProps} />
    );

  return (
    <div className={className}>
      {header}
      {table}
    </div>
  );
}
