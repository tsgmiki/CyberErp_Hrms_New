import { useCallback, useMemo, useState } from "react";
import { Download, FileSpreadsheet, FileText } from "lucide-react";
import { useTranslation } from "react-i18next";
import DisplayOptions from "../displayOptions";
import DropDownButton from "@/components/ui/dropDownButton";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import {
  baseExportFormat,
  type ListExportFormatId,
  type ListExportMenuItem,
} from "./listExportMenu";
import { toast } from "../toast";
import ListColumnSelector, { type ListColumnConfig } from "./listColumnSelector";

export type { ListExportMenuItem, ListExportFormatId } from "./listExportMenu";
export { listExportMenuItems, baseExportFormat, isAllExportFormat } from "./listExportMenu";
export type { ListColumnConfig } from "./listColumnSelector";
export { useListColumnSelection } from "./useListColumnSelection";

export type ListDisplayMode = "list" | "grid";

export interface ListExportConfig {
  title: string;
  data: Record<string, unknown>[] | undefined;
  columns: DataTableColumnModel[];
  items: ListExportMenuItem[];
  disabled?: boolean;
  getExportData?: (
    formatId: ListExportFormatId,
  ) => Promise<Record<string, unknown>[] | undefined>;
}

interface ListViewToolbarProps {
  displayMode: ListDisplayMode;
  onDisplayModeChange: (mode: ListDisplayMode) => void;
  exportConfig?: ListExportConfig;
  columnConfig?: ListColumnConfig;
}

const exportDropdownBtnClass =
  "flex h-9 w-9 items-center justify-center rounded-lg border border-border bg-background p-0 text-primary shadow-none hover:bg-primary/20";

function formatMenuIcon(id: ListExportFormatId) {
  if (id.startsWith("pdf")) {
    return <FileText size={16} />;
  }
  return <FileSpreadsheet size={16} />;
}

function ListViewToolbar({
  displayMode,
  onDisplayModeChange,
  exportConfig,
  columnConfig,
}: ListViewToolbarProps) {
  const { t } = useTranslation();
  const [exporting, setExporting] = useState<ListExportFormatId | null>(null);

  const exportBusy = Boolean(exporting);
  const pageDataEmpty = !exportConfig?.data?.length;

  const labelFor = useCallback((key: string) => t(key), [t]);

  const runExport = useCallback(
    async (formatId: ListExportFormatId) => {
      if (!exportConfig || exportConfig.disabled || exportBusy) return;

      setExporting(formatId);
      try {
        const rows = exportConfig.getExportData
          ? await exportConfig.getExportData(formatId)
          : exportConfig.data;

        if (!rows?.length) {
          toast.error(t("No data to export"));
          return;
        }

        const fileKind = baseExportFormat(formatId);
        const options = {
          title: exportConfig.title,
          data: rows,
          columns: exportConfig.columns,
          labelFor,
        };

        // PERFORMANCE: the export engines (xlsx + @react-pdf/renderer, ~2 MB raw) load ON DEMAND —
        // statically importing them shipped the whole bundle with every list screen's first paint.
        const { exportListToExcel, exportListToPdf } = await import("./listExport");
        if (fileKind === "excel") {
          await exportListToExcel(options);
        } else {
          await exportListToPdf(options);
        }

        toast.success(t("Export completed"));
      } catch {
        toast.error(t("Export failed"));
      } finally {
        setExporting(null);
      }
    },
    [exportConfig, exportBusy, labelFor, t],
  );

  const exportMenu = useMemo(
    () =>
      exportConfig?.items.map((item) => {
        const isAll = item.id.endsWith("-all");
        const itemDisabled =
          exportConfig.disabled || exportBusy || (!isAll && pageDataEmpty);

        return {
          id: item.id,
          label: item.label,
          icon: formatMenuIcon(item.id),
          disable: itemDisabled || exporting === item.id,
        };
      }) ?? [],
    [exportConfig, exportBusy, exporting, pageDataEmpty],
  );

  const handleExportSelect = useCallback(
    (item: { id: ListExportFormatId }) => {
      void runExport(item.id);
    },
    [runExport],
  );

  const exportButtonDisabled =
    exportConfig?.disabled || exportBusy || (pageDataEmpty && !exportConfig?.getExportData);

  return (
    <div className="flex shrink-0 items-center gap-2">
      {columnConfig ? <ListColumnSelector {...columnConfig} /> : null}
      {exportConfig ? (
        <DropDownButton
          icon={
            <Download
              size={16}
              className={exportBusy ? "animate-pulse" : undefined}
            />
          }
          className={exportDropdownBtnClass}
          menu={exportMenu}
          onClick={handleExportSelect}
          disabled={exportButtonDisabled}
        />
      ) : null}
      <DisplayOptions displayMode={displayMode} onDisplayModeChange={onDisplayModeChange} />
    </div>
  );
}

export default ListViewToolbar;
