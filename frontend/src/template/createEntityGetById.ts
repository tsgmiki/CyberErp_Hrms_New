import { api } from "@/utils/apiClient";

/**
 * Standard GET-by-id for REST resources (`GET Resource/{id}`).
 * Returns `undefined` on failure (matches legacy fetch + `response.ok` checks).
 */
export function createEntityGetById<T>(resourcePath: string) {
  const path = resourcePath.replace(/^\//, "").replace(/\/$/, "");
  return async (id: string): Promise<T | undefined> => {
    if (!id) return undefined;
    try {
      return await api.get<T>(`${path}/${encodeURIComponent(id)}`);
    } catch {
      return undefined;
    }
  };
}
