import type DataTableColumnModel from "@/models/DataTableColumnModel";

export type ExportLabelFn = (key: string) => string;

function isActionColumn(col: DataTableColumnModel): boolean {
  const name = (col.name ?? col.key ?? "").toString().toLowerCase();
  return name === "action";
}

export function getExportableColumns(columns: DataTableColumnModel[]): DataTableColumnModel[] {
  return columns.filter((col) => !isActionColumn(col));
}

export function formatExportCell(value: unknown): string {
  if (value == null || value === "") return "";
  if (typeof value === "boolean") return value ? "Yes" : "No";
  if (typeof value === "number") return String(value);
  if (typeof value === "string") return value;
  if (value instanceof Date) return value.toISOString().slice(0, 10);
  if (typeof value === "object") {
    const record = value as Record<string, unknown>;
    for (const key of ["name", "label", "fullName", "title", "value"]) {
      if (record[key] != null && record[key] !== "") return String(record[key]);
    }
    return JSON.stringify(value);
  }
  return String(value);
}

export function buildExportRows(
  data: Record<string, unknown>[] | undefined,
  columns: DataTableColumnModel[],
  labelFor: ExportLabelFn,
): { headers: string[]; rows: string[][] } {
  const exportColumns = getExportableColumns(columns);
  const headers = exportColumns.map((col) => {
    const label = col.label ?? col.name ?? col.key ?? "";
    return labelFor(typeof label === "string" ? label : String(label));
  });

  const rows =
    data?.map((record) =>
      exportColumns.map((col) => {
        const key = col.name ?? col.key;
        return formatExportCell(key ? record[key] : "");
      }),
    ) ?? [];

  return { headers, rows };
}

export function buildExportSheetData(
  data: Record<string, unknown>[] | undefined,
  columns: DataTableColumnModel[],
  labelFor: ExportLabelFn,
): Record<string, string>[] {
  const exportColumns = getExportableColumns(columns);
  return (
    data?.map((record) => {
      const row: Record<string, string> = {};
      exportColumns.forEach((col) => {
        const key = col.name ?? col.key ?? "";
        const label = col.label ?? col.name ?? col.key ?? key;
        const header = labelFor(typeof label === "string" ? label : String(label));
        row[header] = formatExportCell(key ? record[key] : "");
      });
      return row;
    }) ?? []
  );
}
