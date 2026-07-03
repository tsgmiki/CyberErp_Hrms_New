import type { ReactNode } from "react";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import type DataTableColumnModel from "./DataTableColumnModel";
import type ParameterModel from "./ParameterModel";

export default interface DataTableModel {
  columns?: DataTableColumnModel[];
  data: any[];
  count?: number;
  pagination?: "Visible" | "None";
  search?: "Visible" | "None";
  paginationHandler?: Function;
  searchHandler?: Function;
  sortHandler?: Function;
  isLoading?: boolean;
  param?: ParameterModel;
  setParam?: (
    updater: ParameterModel | ((prev: ParameterModel) => ParameterModel),
  ) => void;
  GetChildren?: Function;
  showChildren?: boolean;
  hideHeader?: boolean;
  checkBox?: boolean;
  checkHandler?: (selectedIds: string[]) => void;
  checkList?: string[];
  /** Field name to group rows under collapsible section headers */
  groupBy?: string;
  getGroupLabel?: (groupKey: string, rows: any[]) => string;
  /** Row identifier field name (e.g. "id") */
  key?: string;
  rowIdKey?: string;
  selectionCount?: number;
  onClearSelection?: () => void;
  showSummary?: boolean;
  showColumnFilter?: boolean;
  showExport?: boolean;
  openCriteria?: boolean;
  setOpenCriteria?: Function;
  isVirtual?: boolean;
  showPreview?: boolean;
  showSort?: boolean;
  averageUnitPrice?: number;
  searchByColumnHandler?: Function;
  resetSearchByColumnHandler?: Function;
  pageSizeHandler?: Function;
  selectedCols?: any[];
  setSelected?: Function;
  toolbarEnd?: ReactNode;
  /** Generic filter config (rendered next to search when `searchBarFilters` is not set). */
  listFilters?: ListFilterDefinition[];
  /** Custom filter UI beside the search bar (e.g. domain-specific filters). */
  searchBarFilters?: ReactNode;
  onRefresh?: () => void;
  onExport?: () => void;
}
