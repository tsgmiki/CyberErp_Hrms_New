import type { ReactNode } from "react";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type ParameterModel from "@/models/ParameterModel";

/** Standard paged API response shape used across list screens. */
export interface PagedResult<T> {
  data: T[];
  total: number;
}

export type PagedQueryFn<T = unknown> = (param: ParameterModel) => Promise<PagedResult<T>>;

export interface EntityListViewProps {
  /** Stable id for React Query + column localStorage (`list-columns:{listKey}`). */
  listKey: string;
  /** Translated title for export file names and menus. */
  listLabel: string;
  columns: DataTableColumnModel[];
  queryKey: string;
  fetchPage: PagedQueryFn;
  deleteById?: (id: string) => Promise<unknown>;
  rowKey?: string;
  initialParam?: Partial<ParameterModel>;
  listFilters?: ListFilterDefinition[];
  searchBarFilters?: ReactNode;
  /** Optional content above the table (e.g. status KPI cards). */
  header?: ReactNode;
  checkBox?: boolean;
  groupBy?: string;
  getGroupLabel?: (groupKey: string, rows: Record<string, unknown>[]) => string;
  className?: string;
}

export interface EntityModuleShellProps {
  title: string;
  headerDescription?: string;
  headerIcon?: ReactNode;
  showForm: boolean;
  onList: () => void;
  onAdd: () => void;
  list?: ReactNode;
  form?: ReactNode;
  hideAdd?: boolean;
  hideBack?: boolean;
  /** Passed through to InventoryLayout when needed */
  tableTitle?: string;
  tableDescription?: string;
  tableIcon?: ReactNode;
  onSetting?: () => void;
  hideSetting?: boolean;
  /** Custom body (e.g. list + drawer form). Overrides list/form swap when set. */
  children?: ReactNode;
}
