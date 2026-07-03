import type ParameterModel from "@/models/ParameterModel";

type ListQueryResult<T> = { data?: T[]; total?: number };

/** Loads all rows for the current filters (export-all). */
export async function fetchAllListRows<T>(
  queryFn: (param: ParameterModel) => Promise<ListQueryResult<T>>,
  param: ParameterModel,
  total: number,
): Promise<Record<string, unknown>[]> {
  if (total <= 0) return [];
  const result = await queryFn({ ...param, skip: 0, take: total });
  return (result.data ?? []) as Record<string, unknown>[];
}
