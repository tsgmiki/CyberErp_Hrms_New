import type AbstractModel from "../AbstractModel";

/** One INPUT parameter of a report ('#' in the field name = From/To range pair). */
export interface ReportFieldModel {
  field: string;
  label: string;
  dataType: string; // Text | Number | Currency | Check | Date | Select | MultiSelect | Radio
  fieldOrder?: number;
  dependencyField?: string;
}

/** Admin registry row: a report is a data row naming its own stored procedure. */
export default interface ReportDefinitionModel extends AbstractModel {
  reportKey?: string;
  reportName?: string;
  reportGrouping?: string;
  storedProc?: string;
  sortOrder?: number;
  description?: string;
  isActive?: boolean;
  /** Pivot / grouping layout JSON (reference GridConfig). Empty = a flat report. */
  gridConfig?: string;
  fields?: ReportFieldModel[];
  /** Selectable output columns offered in the viewer's column chooser. */
  fieldOutputs?: { field: string; label: string; fieldOrder?: number }[];
}

export interface ReportCatalogItemModel {
  id: string;
  reportKey: string;
  reportName: string;
  description?: string;
  /** Saved filter sets — the report's "children" in the catalog tree. */
  savedFilters?: SavedFilterItemModel[];
}

export interface ReportCatalogGroupModel {
  grouping: string;
  reports: ReportCatalogItemModel[];
}

export interface ReportLookupOptionModel {
  value: string;
  label: string;
}

export interface ReportSchemaFieldModel extends ReportFieldModel {
  isRange: boolean;
  options?: ReportLookupOptionModel[];
}

export interface ReportOutputColumnModel {
  field: string;
  label: string;
  fieldOrder: number;
}

/** Pivot / grouping layout for a report (reference GridConfig). */
export interface ReportGroupingModel {
  supportsGrouping: boolean;
  allowUserCustomize: boolean;
  maxGroupLevels: number;
  showGroupSummary: boolean;
  /** Default group-by columns, in level order. */
  groupBy: string[];
  /** Columns the user may group by (the report's output columns). */
  groupableFields: ReportOutputColumnModel[];
}

export interface ReportSchemaModel {
  id: string;
  reportKey: string;
  reportName: string;
  description?: string;
  fields: ReportSchemaFieldModel[];
  /** Selectable output columns (empty = the SP always returns its full set). */
  outputColumns: ReportOutputColumnModel[];
  /** Pivot / grouping layout (null = a flat report). */
  grouping?: ReportGroupingModel | null;
}

export interface SavedFilterItemModel {
  id: string;
  name: string;
}

/** One result column AS DECLARED BY THE STORED PROCEDURE (its first result set). */
export interface ReportColumnModel {
  field: string;
  label: string;
  type: string; // string | number | currency | date | datetime | boolean
  width?: number;
  linkPage?: string;
  linkPageValue?: string;
}

export interface ReportResultModel {
  reportKey: string;
  reportName: string;
  columns: ReportColumnModel[];
  rows: Record<string, unknown>[];
  total: number;
  /** PIVOT: per-group subtotals from a grouping SP's 3rd result set (group column values + GroupCount +
   * numeric totals). Null/absent for a flat report. */
  summaries?: Record<string, unknown>[] | null;
}
