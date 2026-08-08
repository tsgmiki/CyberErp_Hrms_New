import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import { NEW_SEGMENT } from "./useEntityRouteModule";

export interface UseEntityRecordOptions {
  /** Explicit id wins over the URL — lets a form keep taking `id` as a prop while it is unmigrated. */
  id?: string;
  enabled?: boolean;
}

/**
 * The reusable by-id fetch: `useParams` + an existing `createEntityGetById` service + React Query.
 *
 * Uses the project's established detail-key convention — `[singularCamelName, id]`, disabled while
 * the id is empty — so it shares a cache with the ~60 forms that hand-roll this today. Those can
 * migrate one at a time, or never.
 *
 *   const { data, isLoading, notFound } = useEntityRecord("branch", getBranch, { id });
 *
 * `notFound` matters: `createEntityGetById` swallows errors and resolves to `undefined`, so a
 * deleted or mistyped id is indistinguishable from a blank form unless the caller checks it. That
 * ambiguity is what lets a stale deep link POST a duplicate instead of failing an update.
 */
export function useEntityRecord<T>(
  key: string,
  fetchById: (id: string) => Promise<T | undefined>,
  options: UseEntityRecordOptions = {},
) {
  const { id: seg } = useParams();
  const fromRoute = seg && seg !== NEW_SEGMENT ? seg : "";
  const id = options.id ?? fromRoute;
  const enabled = (options.enabled ?? true) && id !== "";

  const query = useQuery({
    queryKey: [key, id],
    queryFn: () => fetchById(id),
    enabled,
  });

  return {
    ...query,
    id,
    /** True when creating — no record is expected. */
    isNew: id === "",
    /** True when an id WAS requested, the fetch settled, and nothing came back. */
    notFound: enabled && !query.isPending && query.data === undefined,
  };
}
