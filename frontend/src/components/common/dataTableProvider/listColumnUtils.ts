import type DataTableColumnModel from "@/models/DataTableColumnModel";

export function isActionColumn(col: DataTableColumnModel): boolean {
  const name = (col.name ?? col.key ?? "").toString().toLowerCase();
  return name === "action";
}

/** Columns the user can toggle (excludes Action). */
export function getSelectableColumns(
  columns: DataTableColumnModel[],
): DataTableColumnModel[] {
  return columns.filter((col) => !isActionColumn(col));
}

/** Visible table columns: selected fields plus Action when defined. */
export function getDisplayColumns(
  allColumns: DataTableColumnModel[],
  selectedCols: DataTableColumnModel[],
): DataTableColumnModel[] {
  const selectable = getSelectableColumns(allColumns);
  const action = allColumns.find(isActionColumn);

  const selected =
    selectedCols.length > 0
      ? selectedCols.filter((col) => !isActionColumn(col))
      : selectable;

  if (action) {
    return [...selected, action];
  }
  return selected.length > 0 ? selected : selectable;
}
