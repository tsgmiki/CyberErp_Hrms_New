import { type ReactNode, useCallback, useEffect, useState } from "react";
import { Download, RefreshCw } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { DataTableModel } from "@/models";
import SearchBar from "../searchBar/searchBar";
import ListSearchFilters from "../searchBar/listSearchFilters";
import {
  clearListFilterParams,
  countActiveListFilters,
} from "../searchBar/listFilterTypes";
import Pagination from "../pagination/pagination";
import Loading from "../loader/loader";
import DataTableEmptyState from "./dataTableEmptyState";
import DataTableSelectionBar from "./dataTableSelectionBar";
import { patchListParam } from "./listParamUtils";
import type ParameterModel from "@/models/ParameterModel";

interface DataTableChromeProps {
  dataTable: DataTableModel;
  children: ReactNode;
  showEmpty?: boolean;
}

function DataTableChrome({ dataTable, children, showEmpty = true }: DataTableChromeProps) {
  const { t } = useTranslation();
  const {
    isLoading,
    count,
    param,
    setParam,
    pagination,
    search,
    toolbarEnd,
    data,
    selectionCount,
    onClearSelection,
    onRefresh,
    onExport,
    showExport,
    listFilters,
    searchBarFilters,
  } = dataTable;

  const [searchValue, setSearchValue] = useState(param?.searchText ?? "");

  useEffect(() => {
    setSearchValue(param?.searchText ?? "");
  }, [param?.searchText]);

  useEffect(() => {
    if (!setParam || search !== "Visible") return;
    if (searchValue === (param?.searchText ?? "")) return;

    const timer = window.setTimeout(() => {
      setParam((prev) =>
        patchListParam(prev as ParameterModel | undefined, {
          searchText: searchValue,
          skip: 0,
        }),
      );
    }, 400);

    return () => window.clearTimeout(timer);
  }, [searchValue, param?.searchText, setParam, search]);

  const searchTextHandler = useCallback(() => {
    if (!setParam) return;
    setParam((prev) =>
      patchListParam(prev as ParameterModel | undefined, {
        searchText: searchValue,
        skip: 0,
      }),
    );
  }, [setParam, searchValue]);

  const clearSearch = useCallback(() => {
    setSearchValue("");
    if (!setParam) return;
    setParam((prev) =>
      patchListParam(prev as ParameterModel | undefined, {
        searchText: "",
        skip: 0,
      }),
    );
  }, [setParam]);

  const clearSearchAndFilters = useCallback(() => {
    setSearchValue("");
    if (!setParam) return;
    setParam((prev) => {
      const cleared = patchListParam(prev as ParameterModel | undefined, {
        searchText: "",
        skip: 0,
      });
      return listFilters?.length
        ? clearListFilterParams(cleared, listFilters)
        : cleared;
    });
  }, [listFilters, setParam]);

  const paginationHandler = useCallback(
    (valObj: { take: number; skip: number }) => {
      if (!setParam) return;
      setParam((prev) =>
        patchListParam(prev as ParameterModel | undefined, {
          take: valObj.take,
          skip: valObj.skip,
        }),
      );
    },
    [setParam],
  );

  const hasSearchText = Boolean(param?.searchText?.trim());
  const activeFilterCount = listFilters?.length
    ? countActiveListFilters(param, listFilters)
    : 0;
  const hasActiveFilters = activeFilterCount > 0;
  const hasSearchOrFilters = hasSearchText || hasActiveFilters;
  const isEmpty = !isLoading && (!data || data.length === 0);
  const showToolbar =
    search === "Visible" ||
    Boolean(searchBarFilters) ||
    Boolean(listFilters?.length) ||
    toolbarEnd ||
    onRefresh ||
    onExport ||
    showExport;

  const iconBtn =
    "flex h-9 w-9 shrink-0 items-center justify-center rounded-lg border border-border bg-secondary text-muted transition-colors hover:bg-muted/50 hover:text-foreground disabled:opacity-50";

  return (
    <div className="flex h-full min-h-0 flex-col">
      {showToolbar && (
        <div className="mb-3 flex shrink-0 flex-wrap items-center gap-2 px-1">
          <div className="flex min-w-0 flex-1 flex-wrap items-center gap-2">
            {onRefresh && (
              <button
                type="button"
                onClick={onRefresh}
                disabled={isLoading}
                className={iconBtn}
                title={t("Refresh")}
                aria-label={t("Refresh")}
              >
                <RefreshCw className={`h-4 w-4 ${isLoading ? "animate-spin" : ""}`} />
              </button>
            )}
            {(onExport || showExport) && (
              <button
                type="button"
                onClick={onExport}
                disabled={isLoading || !onExport}
                className={iconBtn}
                title={t("Export")}
                aria-label={t("Export")}
              >
                <Download className="h-4 w-4" />
              </button>
            )}
            {(search === "Visible" || searchBarFilters) && (
              <div className="flex min-w-0 flex-1 flex-wrap items-center gap-2">
                {search === "Visible" ? (
                  <div className="min-w-[200px] flex-1 sm:max-w-md">
                    <SearchBar
                      value={searchValue}
                      onChange={setSearchValue}
                      onEnter={searchTextHandler}
                      onClear={clearSearch}
                      disabled={isLoading}
                      placeholder={t("Search")}
                    />
                  </div>
                ) : null}
                {searchBarFilters}
                {!searchBarFilters && listFilters?.length ? (
                  <ListSearchFilters
                    filters={listFilters}
                    param={param}
                    setParam={setParam}
                    disabled={isLoading}
                  />
                ) : null}
              </div>
            )}
          </div>

          <div className="flex shrink-0 flex-wrap items-center gap-2 sm:ml-auto">
            {count !== undefined && (
              <p className="text-xs text-muted">
                <span className="font-semibold text-foreground">{count}</span>{" "}
                {t("records")}
              </p>
            )}
            {toolbarEnd}
          </div>
        </div>
      )}

      {selectionCount != null && selectionCount > 0 && onClearSelection && (
        <DataTableSelectionBar count={selectionCount} onClear={onClearSelection} />
      )}

      <div className="min-h-0 flex-1 overflow-auto">
        {isLoading ? (
          <div className="flex h-full min-h-[220px] items-center justify-center">
            <Loading />
          </div>
        ) : isEmpty && showEmpty ? (
          <DataTableEmptyState
            hasSearch={hasSearchOrFilters}
            onClearSearch={hasSearchOrFilters ? clearSearchAndFilters : undefined}
          />
        ) : (
          children
        )}
      </div>

      {pagination === "Visible" && !isEmpty && (
        <div className="mt-3 shrink-0">
          <Pagination
            recordCount={count ?? 0}
            take={param?.take}
            skip={param?.skip}
            paginationHandler={paginationHandler}
          />
        </div>
      )}
    </div>
  );
}

export default DataTableChrome;
