import type { DataTableModel } from "@/models";
import { ArrowUpDown } from "lucide-react";
import {
  useRef,
  useState,
  useCallback,
  useMemo,
  useTransition,
  memo,
  lazy,
} from "react";
import { useTranslation } from "react-i18next";

const ActionBar = memo(
  lazy(() => import("@/components/common/virtualTable/actionBar"))
);

interface VirtualDataTableProps extends DataTableModel {
  isLoading?: boolean;
  error?: string | null;
  param?: any;
  defaultVisibleFields?: string[];
  reportInfo?: any;
}

const VirtualDataTable = ({
  data = [],
  columns = [],
  sortHandler,
  searchHandler,
  showSummary,
  isLoading = false,
  error = null,
  averageUnitPrice = 0,
  pageSizeHandler,
  selectedCols,
  setSelected,
  param,
  defaultVisibleFields,
  reportInfo,
}: VirtualDataTableProps) => {
  const [scrollTop, setScrollTop] = useState(0);
  const [, startTransition] = useTransition();
  const tableContainerRef = useRef<HTMLDivElement>(null);
  const { t } = useTranslation();

  // Compute active selected columns: use provided or derive from defaultVisibleFields
  const activeSelectedCols = useMemo(() => {
    if (selectedCols !== undefined) {
      return selectedCols;
    }
    if (defaultVisibleFields && defaultVisibleFields.length > 0 && columns.length > 0) {
      return columns
        .filter(col => defaultVisibleFields.includes(col.name as string))
        .map(col => ({ name: col.name, label: col.label, key: col.key }));
    }
    return columns.map(col => ({ name: col.name, label: col.label, key: col.key }));
  }, [selectedCols, defaultVisibleFields, columns]);

  const handleSetSelected = useCallback((newSelected: any[]) => {
    if (setSelected) {
      setSelected(newSelected);
    }
  }, [setSelected]);

  const ROW_HEIGHT = 48;
  const TABLE_HEIGHT = 600;
  const BUFFER_SIZE = 5;
  const INDEX_COL_WIDTH = "w-20";

  const records = useMemo(
    () => data?.map((a, index) => ({ ...a, internal_idx: index + 1 })) || [],
    [data]
  );

  const { visibleRows, startIndex } = useMemo(() => {
    const start = Math.max(0, Math.floor(scrollTop / ROW_HEIGHT) - BUFFER_SIZE);
    const end = Math.min(
      records.length,
      Math.ceil((scrollTop + TABLE_HEIGHT) / ROW_HEIGHT) + BUFFER_SIZE
    );
    return {
      visibleRows: records.slice(start, end),
      startIndex: start,
    };
  }, [scrollTop, records, ROW_HEIGHT, TABLE_HEIGHT]);

  const handleScroll = useCallback((event: React.UIEvent<HTMLDivElement>) => {
    const currentScroll = event.currentTarget.scrollTop;
    startTransition(() => {
      setScrollTop(currentScroll);
    });
  }, []);

  const visibleColumns = useMemo(() => {
    if (!activeSelectedCols || activeSelectedCols.length === 0) return columns;
    return columns.filter((col) => 
      (activeSelectedCols as string[]).includes(col.name as string) ||
      (activeSelectedCols as any[]).some((s: any) => s.name === col.name)
    );
  }, [columns, activeSelectedCols]);

  const summary = useMemo(() => {
    if (!data.length || !visibleColumns.length) return {};
    const summaryData: any = {};
    visibleColumns.forEach((col, index) => {
      const colName = col.name as string;
      if (colName === "Av_UnitPrice") {
        summaryData[colName] = averageUnitPrice.toLocaleString();
      } else if (index === 0) {
        summaryData[colName] = t("Total");
      } else {
        const total = data.reduce(
          (sum, row) => sum + (Number(row[colName]) || 0),
          0
        );
        summaryData[colName] = total > 0 ? total.toLocaleString() : "";
      }
    });
    return summaryData;
  }, [data, visibleColumns, averageUnitPrice, t]);

  if (isLoading)
    return (
      <div className="flex items-center justify-center p-16 rounded-2xl border border-primary shadow-xl transition-colors duration-300 bg-card">
        <div className="flex flex-col items-center gap-4">
          <div className="relative">
            <div className="w-14 h-14 border-4 border-primary border-t-secondary rounded-full animate-spin"></div>
            <div className="absolute inset-0 w-14 h-14 border-4 border-transparent rounded-full animate-spin border-b-primary" style={{ animationDirection: 'reverse', animationDuration: '1.5s' }}></div>
          </div>
          <div className="text-center">
            <span className="text-base font-semibold block text-foreground">
              {t("Loading data...")}
            </span>
            <span className="text-xs mt-1 block text-muted">
              {t("Please wait while we fetch your information")}
            </span>
          </div>
        </div>
      </div>
    );

  if (error)
    return (
      <div className="flex items-center justify-center p-16 rounded-2xl border border-error shadow-xl transition-colors duration-300 bg-error-light">
        <div className="flex flex-col items-center gap-4">
          <div className="w-16 h-16 rounded-2xl flex items-center justify-center shadow-lg bg-error-light">
            <svg
              className="w-8 h-8 text-error"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
              />
            </svg>
          </div>
          <div className="text-center">
            <span className="text-base font-semibold block text-error">
              {t("Error Loading Data")}
            </span>
            <span className="text-sm mt-1 block text-error">
              {error}
            </span>
          </div>
        </div>
      </div>
    );

  const topSpacerHeight = startIndex * ROW_HEIGHT;
  const bottomSpacerHeight = Math.max(
    0,
    (records.length - startIndex - visibleRows.length) * ROW_HEIGHT
  );

  return (
    <div className="flex flex-col w-full rounded-2xl shadow-xl border border-primary overflow-hidden transition-colors duration-300 bg-card">
      {/* Action Buttons */}
      <div className="border-b border-primary bg-secondary transition-colors duration-300">
        <ActionBar
          data={data?.concat(summary) as never}
          pageSizeHandler={pageSizeHandler}
          selectedCols={activeSelectedCols}
          setSelected={handleSetSelected}
          param={param}
          columns={columns}
          reportInfo={{
            ...reportInfo,
            columns: columns,
            visibleFields: activeSelectedCols?.map((col: any) => col.name),
          }}
          sortHandler={sortHandler}
          searchHandler={searchHandler}
        />
      </div>

      {/* Main Table Container */}
      <div
        ref={tableContainerRef}
        onScroll={handleScroll}
        className="overflow-auto relative transition-colors duration-300 bg-card"
        style={{ height: `${TABLE_HEIGHT}px` }}
      >
        <table className="w-full border-collapse relative z-10">
          <thead className="sticky top-0 z-10">
            <tr className="shadow-lg transition-colors duration-300 bg-secondary">
              <th
                className={`${INDEX_COL_WIDTH} px-5 py-4 text-xs font-bold uppercase tracking-widest text-left transition-colors duration-200 text-table-header border-b border-primary`}
              >
                #
              </th>
              {visibleColumns.map((col) => (
                <th
                  key={col.name as string}
                  className={`px-5 py-4 text-xs font-bold uppercase tracking-widest text-left transition-colors duration-200 text-table-header border-b border-primary`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="truncate">{col.label}</span>
                    <div className="inline-flex items-center gap-1">
                      {col.sort && (
                        <button
                          onClick={() => sortHandler?.(col.name)}
                          className="p-1.5 rounded-lg transition-all duration-200 shrink-0 backdrop-blur-sm hover:bg-primary text-foreground"
                        >
                          <ArrowUpDown className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </div>
                </th>
              ))}
            </tr>
          </thead>

          <tbody>
            {topSpacerHeight > 0 && (
              <tr style={{ height: `${topSpacerHeight}px` }}>
                <td colSpan={visibleColumns.length + 1} />
              </tr>
            )}
            {visibleRows.map((item, idx) => (
              <tr
                key={item.id || item.internal_idx}
                style={{ height: `${ROW_HEIGHT}px` }}
                className={`
                  ${idx % 2 === 0 ? "bg-secondary" : "bg-card"}
                  transition-all duration-200 ease-in-out
                  border-b border-primary
                  group
                  hover:bg-primary
                `}
              >
                <td
                  className={`${INDEX_COL_WIDTH} px-5 text-sm font-semibold transition-colors duration-200 text-primary group-hover:text-foreground`}
                >
                  {item.internal_idx}
                </td>
                {visibleColumns.map((col) => (
                  <td
                    key={`${item.internal_idx}-${col.name}`}
                    className={`px-5 text-sm truncate transition-colors duration-200 text-foreground`}
                  >
                    {col.render
                      ? col.render(item[col.name as string], item)
                      : item[col.name as string]}
                  </td>
                ))}
              </tr>
            ))}
            {bottomSpacerHeight > 0 && (
              <tr style={{ height: `${bottomSpacerHeight}px` }}>
                <td colSpan={visibleColumns.length + 1} />
              </tr>
            )}
          </tbody>

          {/* FIXED FOOTER (Aligned to Columns) */}
          {showSummary && (
            <tfoot className="sticky z-10 bottom-0">
              <tr className="border-t-2 shadow-2xl transition-colors duration-300 bg-secondary border-primary">
                <td
                  className={`${INDEX_COL_WIDTH} px-5 py-4 text-xs font-bold uppercase tracking-widest transition-colors duration-200 text-muted`}
                >
                  {t("Total")}
                </td>
                {visibleColumns.map((col) => (
                  <td
                    key={`summary-${col.name}`}
                    className={`px-5 py-4 text-sm font-bold truncate transition-colors duration-200 text-foreground`}
                  >
                    {summary[col.name as string] || ""}
                  </td>
                ))}
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </div>
  );
};

export default VirtualDataTable;