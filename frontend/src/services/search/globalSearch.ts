import { api } from "@/utils/apiClient";

/** One hit in the global-search dropdown. `route` is the SPA path to navigate to on select. */
export interface SearchResultItem {
  id: string;
  title: string;
  subtitle?: string | null;
  route: string;
}

/** A category of results (e.g. "Employees"); `icon` is a lucide-react icon name. */
export interface SearchResultGroup {
  category: string;
  icon: string;
  order: number;
  items: SearchResultItem[];
}

export interface GlobalSearchResponse {
  query: string;
  totalCount: number;
  groups: SearchResultGroup[];
}

/**
 * Calls the header global-search endpoint. `signal` lets React Query abort superseded
 * requests (the debounce already limits how often this fires).
 */
export function searchGlobal(term: string, limit = 5, signal?: AbortSignal) {
  return api.get<GlobalSearchResponse>(
    `Search?q=${encodeURIComponent(term)}&limit=${limit}`,
    { signal },
  );
}
