import { useCallback, useState } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";
import { fetchAllListRows } from "@/components/common/dataTableProvider/listFetchAll";
import type { ListDisplayMode } from "@/components/common/dataTableProvider/listViewToolbar";
import type { PagedQueryFn } from "./types";

export interface UseEntityListOptions<T = unknown> {
  queryKey: string;
  fetchPage: PagedQueryFn<T>;
  deleteById?: (id: string) => Promise<unknown>;
  initialParam?: Partial<ParameterModel>;
}

/**
 * Standard list state: pagination params, React Query page fetch, optional delete mutation.
 */
export function useEntityList<T = unknown>({
  queryKey,
  fetchPage,
  deleteById,
  initialParam,
}: UseEntityListOptions<T>) {
  const [displayMode, setDisplayMode] = useState<ListDisplayMode>("list");
  const [param, setParam] = useState<ParameterModel>({
    ...parameterInitialData,
    ...initialParam,
  });
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: [queryKey, param],
    queryFn: () => fetchPage(param),
    // Keep the previous page's rows on screen while the next filter/page loads (e.g. switching org-unit
    // tree nodes) — no loading flash or grid remount — and don't refetch an already-fresh page.
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });

  const { mutate: deleteRecord } = useMutation({
    mutationFn: (id: string) => deleteById?.(id) ?? Promise.resolve(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [queryKey] });
    },
  });

  const rows = data?.data;
  const total = data?.total ?? 0;

  const fetchAllData = useCallback(
    () => fetchAllListRows(fetchPage, param, total),
    [fetchPage, param, total],
  );

  return {
    param,
    setParam,
    displayMode,
    setDisplayMode,
    rows,
    total,
    isLoading,
    deleteRecord,
    fetchAllData,
  };
}
