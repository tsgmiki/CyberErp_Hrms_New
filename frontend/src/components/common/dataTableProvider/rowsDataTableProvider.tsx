import type { DataTableColumnModel } from "@/models";
import { v6 as uuid } from "uuid";
import { useTranslation } from "react-i18next";
import DataTableChrome from "./dataTableChrome";

interface RowsDataTableProviderProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  dataTable: any;
}

function RowsDataTableProvider({ dataTable }: RowsDataTableProviderProps) {
  const { t } = useTranslation();
  const { data, columns } = dataTable;

  const visibleColumns = columns?.filter(
    (col: DataTableColumnModel) => col.name !== "Action",
  );

  const actionColumn = columns?.find(
    (col: DataTableColumnModel) => col.name === "Action",
  );

  return (
    <DataTableChrome dataTable={dataTable}>
      <div className="overflow-hidden rounded-xl border border-border bg-card">
        <div className="flex items-center border-b border-border bg-muted/50 px-4 py-3">
          {visibleColumns?.map((column: DataTableColumnModel) => (
            <div
              key={column.name}
              className="min-w-[100px] flex-1 px-2 text-xs font-semibold uppercase tracking-wide text-muted"
            >
              {column.label}
            </div>
          ))}
          <div className="w-20 shrink-0" />
        </div>

        <div className="divide-y divide-border">
          {data.map((row: Record<string, unknown>) => (
            <div
              key={String(row.id ?? uuid())}
              className="flex cursor-pointer items-center px-4 py-3 transition-colors hover:bg-muted/30"
            >
              {visibleColumns?.map((column: DataTableColumnModel) => {
                const value = row[column.name as string];
                return (
                  <div
                    key={column.name}
                    className="min-w-[100px] flex-1 px-2 text-sm text-foreground"
                  >
                    {column.render
                      ? column.render(value as string, row)
                      : (value as string) || t("N/A")}
                  </div>
                );
              })}
              <div className="flex w-20 shrink-0 justify-end">
                <div onClick={(e) => e.stopPropagation()} role="presentation">
                  {actionColumn?.render?.(
                    row[actionColumn.name as string] as string,
                    row,
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </DataTableChrome>
  );
}

export default RowsDataTableProvider;
