import { useQuery } from "@tanstack/react-query";
import { api } from "@/utils/apiClient";

/** One value of a lookup category (from the generic 2-table lookup system). */
export interface LookupItemModel {
  id: string;
  name: string;
  code: string;
}

/**
 * Fetch a lookup category's values by its CODE (e.g. "EducationLevel", "FieldOfStudy"). Central entry
 * point for every combobox fed by the generic lookup system — add a category server-side and any screen
 * can consume it here, with no new endpoint.
 */
export const getLookup = (categoryCode: string) =>
  api.get<LookupItemModel[]>(`Lookup/items/${encodeURIComponent(categoryCode)}`);

/**
 * Reusable hook: cached lookup values for a category code. Reference data changes rarely, so it is
 * cached aggressively and shared across every consumer of the same category.
 */
export function useLookup(categoryCode: string) {
  return useQuery({
    queryKey: ["lookup", categoryCode],
    queryFn: () => getLookup(categoryCode),
    staleTime: 30 * 60 * 1000,
    enabled: !!categoryCode,
  });
}

/** A lookup category (the registry row grouping a set of values). */
export interface LookupCategoryModel {
  id: string;
  name: string;
  code: string;
}

/** All lookup categories (for binding pickers, e.g. the Form Builder's Select source). */
export const getLookupCategories = async (): Promise<LookupCategoryModel[]> => {
  const res = await api.get<{ data?: LookupCategoryModel[] }>("Lookup?Skip=0&Take=500");
  return res?.data ?? [];
};

/** Cached category list for binding pickers. */
export function useLookupCategories() {
  return useQuery({
    queryKey: ["lookupCategories"],
    queryFn: getLookupCategories,
    staleTime: 30 * 60 * 1000,
  });
}

/** Lookup values mapped to the `{ id, name }` option shape the standard combobox (DropDownField) wants.
 * `useName` stores the value's NAME (default — human-readable + backward-compatible with free text);
 * pass false to store the value's CODE instead. */
export function useLookupOptions(categoryCode: string, useName = true) {
  const query = useLookup(categoryCode);
  const options = (query.data ?? []).map((i) => ({
    id: useName ? i.name : i.code,
    name: i.name,
  }));
  return { ...query, options };
}
