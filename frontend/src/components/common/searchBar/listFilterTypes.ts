import type { ParameterModel } from "@/models";

export interface ListFilterOption {
  value: string;
  label: string;
}

export interface ListFilterSelect {
  type: "select";
  paramKey: keyof ParameterModel;
  label: string;
  placeholder?: string;
  options: ListFilterOption[];
}

export interface ListFilterDateRange {
  type: "dateRange";
  fromKey?: keyof ParameterModel;
  toKey?: keyof ParameterModel;
  label?: string;
}

export interface ListFilterAsyncSelect {
  type: "asyncSelect";
  paramKey: keyof ParameterModel;
  label: string;
  placeholder?: string;
  queryKey: readonly unknown[];
  queryFn: () => Promise<{ data?: Array<Record<string, unknown>> }>;
  valueKey?: string;
  labelKey?: string;
}

export type ListFilterDefinition =
  | ListFilterSelect
  | ListFilterDateRange
  | ListFilterAsyncSelect;

export function getListFilterParamKeys(filters: ListFilterDefinition[]): string[] {
  const keys = new Set<string>();
  for (const filter of filters) {
    if (filter.type === "dateRange") {
      keys.add(String(filter.fromKey ?? "fromDate"));
      keys.add(String(filter.toKey ?? "toDate"));
      continue;
    }
    keys.add(String(filter.paramKey));
  }
  return Array.from(keys);
}

export function countActiveListFilters(
  param: ParameterModel | undefined,
  filters: ListFilterDefinition[],
): number {
  if (!param) return 0;
  let count = 0;
  for (const filter of filters) {
    if (filter.type === "dateRange") {
      const fromKey = filter.fromKey ?? "fromDate";
      const toKey = filter.toKey ?? "toDate";
      if (param[fromKey]) count += 1;
      else if (param[toKey]) count += 1;
      continue;
    }
    const value = param[filter.paramKey];
    if (value !== undefined && value !== null && value !== "" && value !== false) {
      count += 1;
    }
  }
  return count;
}

export function extractListFilterParams(
  source: ParameterModel,
  filters: ListFilterDefinition[],
): Partial<ParameterModel> {
  const patch: Partial<ParameterModel> = {};
  for (const filter of filters) {
    if (filter.type === "dateRange") {
      const fromKey = filter.fromKey ?? "fromDate";
      const toKey = filter.toKey ?? "toDate";
      (patch as Record<string, unknown>)[fromKey as string] = source[fromKey];
      (patch as Record<string, unknown>)[toKey as string] = source[toKey];
      continue;
    }
    (patch as Record<string, unknown>)[filter.paramKey as string] =
      source[filter.paramKey];
  }
  return patch;
}

export function applyListFilterParams(
  prev: ParameterModel,
  draft: ParameterModel,
  filters: ListFilterDefinition[],
): ParameterModel {
  return {
    ...prev,
    ...extractListFilterParams(draft, filters),
    skip: 0,
  };
}

export function clearListFilterParams(
  param: ParameterModel,
  filters: ListFilterDefinition[],
): ParameterModel {
  const next = { ...param, skip: 0 };
  for (const filter of filters) {
    if (filter.type === "dateRange") {
      const fromKey = filter.fromKey ?? "fromDate";
      const toKey = filter.toKey ?? "toDate";
      (next as Record<string, unknown>)[fromKey as string] = "";
      (next as Record<string, unknown>)[toKey as string] = "";
      continue;
    }
    const key = filter.paramKey as string;
    const current = next[filter.paramKey];
    (next as Record<string, unknown>)[key] =
      typeof current === "boolean" ? false : "";
  }
  return next;
}
