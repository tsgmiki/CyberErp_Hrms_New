import type ParameterModel from "@/models/ParameterModel";
import { api } from "@/utils/apiClient";
import type { PagedResult } from "./types";

function toQueryString(param: ParameterModel): string {
  const query = new URLSearchParams();
  for (const key of Object.keys(param) as (keyof ParameterModel)[]) {
    const value = param[key];
    if (value !== undefined && value !== null) {
      query.append(String(key), String(value));
    }
  }
  return query.toString();
}

/**
 * Factory for standard paged GET services (`?page&size&sort…`).
 * Prefer this over duplicating fetch + URLSearchParams in each `getAll.ts`.
 */
export function createPagedQuery<T>(resourcePath: string) {
  const path = resourcePath.replace(/^\//, "").replace(/\/$/, "");
  return async (param: ParameterModel): Promise<PagedResult<T>> => {
    const query = toQueryString(param);
    const suffix = query ? `?${query}` : "";
    return api.get<PagedResult<T>>(`${path}${suffix}`);
  };
}
