import { useQuery } from "@tanstack/react-query";
import { getMyClearances } from "@/services/admin/employee/termination";
import { getPendingProfileChangeRequests } from "@/services/admin/employee/profileChangeRequest";
import { getMyApprovals } from "@/services/admin/workflow";

/**
 * The three "does this need MY decision" queues. Centralized here (rather than inlined per widget)
 * so every widget that reads one — e.g. KpiOverviewWidget's Change-Requests tile AND
 * ActionQueueWidget's Change-Requests tab both need `useProfileChangeRequests` — shares the exact
 * same queryKey/queryFn and therefore the exact same React Query cache entry: mounting both widgets
 * still issues each of these calls only ONCE, deduped automatically, no prop-drilling required.
 */
export function useMyApprovals() {
  return useQuery({ queryKey: ["myApprovals"], queryFn: getMyApprovals, staleTime: 30_000 });
}

export function useMyClearances() {
  return useQuery({ queryKey: ["myClearances"], queryFn: getMyClearances, staleTime: 30_000 });
}

export function useProfileChangeRequests() {
  return useQuery({
    queryKey: ["profileChangeRequests"],
    queryFn: getPendingProfileChangeRequests,
    staleTime: 30_000,
  });
}
