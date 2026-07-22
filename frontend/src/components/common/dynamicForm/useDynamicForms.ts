import { useQuery } from "@tanstack/react-query";
import { getActiveForms } from "@/services/admin/dynamicForm";

/**
 * Fetches the active user-defined forms (custom tabs) for a module. Metadata changes rarely, so it is
 * cached aggressively — every consumer (profile tabs, DynamicTabs) shares one request per module.
 */
export function useDynamicForms(module: string) {
  return useQuery({
    queryKey: ["dynamicForms", module],
    queryFn: () => getActiveForms(module),
    staleTime: 5 * 60 * 1000,
  });
}
