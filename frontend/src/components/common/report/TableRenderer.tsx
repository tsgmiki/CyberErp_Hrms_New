import React from "react";

export type TableColumnDef<T = any> = {
  key: string;
  label: React.ReactNode;
  render?: (value: any, record: T) => React.ReactNode;
};

export type TableRendererProps<T> = {
  columns: Array<TableColumnDef<T>>;
  data: T[];
  actions?: React.ReactNode;
  getRowKey?: (row: T, index: number) => React.Key;
  numericKeys?: string[];
  formatCurrency?: (value: number) => React.ReactNode;
  renderFooter?: (args: { data: T[]; columns: Array<TableColumnDef<T>> }) => React.ReactNode;
};

export function TableRenderer<T>({
  columns,
  data,
  actions,
  getRowKey,
  numericKeys,
  formatCurrency,
  renderFooter,
}: TableRendererProps<T>) {
  const numericKeySet = React.useMemo(() => new Set(numericKeys ?? []), [numericKeys]);

  return (
    <div className="overflow-x-auto">
      {actions ? <div className="mb-3 flex justify-end print:hidden">{actions}</div> : null}
      <table className="min-w-full border-collapse border border-gray-300">
        <thead className="bg-gray-100">
          <tr>
            {columns.map((col) => (
              <th
                key={col.key}
                className="border border-gray-300 px-3 py-2 text-left text-sm font-semibold text-gray-700"
              >
                {col.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row, index) => {
            const rowKey = getRowKey ? getRowKey(row, index) : index;
            return (
              <tr key={rowKey} className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}>
                {columns.map((col) => {
                  const rawValue = (row as any)?.[col.key];
                  const cell =
                    col.render?.(rawValue, row) ??
                    (numericKeySet.has(col.key) && typeof rawValue === "number" && formatCurrency
                      ? formatCurrency(rawValue)
                      : (rawValue as React.ReactNode));

                  return (
                    <td key={col.key} className="border border-gray-300 px-3 py-2 text-sm text-gray-600">
                      {cell}
                    </td>
                  );
                })}
              </tr>
            );
          })}
          {renderFooter?.({ data, columns })}
        </tbody>
      </table>
    </div>
  );
}

