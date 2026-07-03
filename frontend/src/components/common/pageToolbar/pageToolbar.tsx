import type { ReactNode } from "react";
import { Download, Filter, Plus, RefreshCw, Upload } from "lucide-react";
import { useTranslation } from "react-i18next";
import SearchBar from "../searchBar/searchBar";

export interface PageToolbarProps {
  searchValue?: string;
  onSearchChange?: (value: string) => void;
  onSearchSubmit?: () => void;
  onSearchClear?: () => void;
  searchPlaceholder?: string;
  searchDisabled?: boolean;
  onFilter?: () => void;
  onRefresh?: () => void;
  onExport?: () => void;
  onImport?: () => void;
  onAdd?: () => void;
  addLabel?: string;
  end?: ReactNode;
  className?: string;
}

function PageToolbar({
  searchValue = "",
  onSearchChange,
  onSearchSubmit,
  onSearchClear,
  searchPlaceholder,
  searchDisabled = false,
  onFilter,
  onRefresh,
  onExport,
  onImport,
  onAdd,
  addLabel,
  end,
  className = "",
}: PageToolbarProps) {
  const { t } = useTranslation();

  const iconBtn =
    "flex h-9 w-9 items-center justify-center rounded-lg border border-border bg-secondary text-muted transition-colors hover:bg-muted/50 hover:text-foreground disabled:opacity-50";

  return (
    <div
      className={`flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between ${className}`}
    >
      <div className="flex min-w-0 flex-1 flex-wrap items-center gap-2">
        {onSearchChange && (
          <div className="min-w-[200px] flex-1 sm:max-w-xs">
            <SearchBar
              value={searchValue}
              onChange={onSearchChange}
              onEnter={onSearchSubmit}
              onClear={onSearchClear}
              disabled={searchDisabled}
              placeholder={searchPlaceholder ?? t("Search")}
            />
          </div>
        )}
        {onFilter && (
          <button type="button" onClick={onFilter} className={`${iconBtn} w-auto gap-1 px-3`}>
            <Filter className="h-4 w-4" />
            <span className="text-xs font-medium">{t("Filter")}</span>
          </button>
        )}
      </div>

      <div className="flex shrink-0 flex-wrap items-center gap-2 sm:ml-auto">
        {onRefresh && (
          <button
            type="button"
            onClick={onRefresh}
            className={iconBtn}
            title={t("Refresh")}
            aria-label={t("Refresh")}
          >
            <RefreshCw className="h-4 w-4" />
          </button>
        )}
        {onImport && (
          <button
            type="button"
            onClick={onImport}
            className={`${iconBtn} w-auto gap-1 px-3`}
            title={t("Import")}
          >
            <Upload className="h-4 w-4" />
            <span className="hidden text-xs font-medium sm:inline">{t("Import")}</span>
          </button>
        )}
        {onExport && (
          <button
            type="button"
            onClick={onExport}
            className={`${iconBtn} w-auto gap-1 px-3`}
            title={t("Export")}
          >
            <Download className="h-4 w-4" />
            <span className="hidden text-xs font-medium sm:inline">{t("Export")}</span>
          </button>
        )}
        {end}
        {onAdd && (
          <button
            type="button"
            onClick={onAdd}
            className="flex h-9 items-center gap-1.5 rounded-lg bg-primary px-3 text-sm font-semibold text-on-accent shadow-sm transition-opacity hover:opacity-90"
          >
            <Plus className="h-4 w-4" />
            <span>{addLabel ?? t("Add")}</span>
          </button>
        )}
      </div>
    </div>
  );
}

export default PageToolbar;
