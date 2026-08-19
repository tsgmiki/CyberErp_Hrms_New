import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import type { ModuleModel, UserPermissionModel } from "@/models";
import GetAllModuleWithOperation from "@/services/admin/module/getAllWithOperation";
import store from "@/store";
import { useSignals } from "@preact/signals-react/runtime";

/**
 * The sidebar's menu source: GET Module/WithOperations, which returns this tenant's own operation
 * tree filtered to the caller's grants. Names, links, icons and order are all columns on those
 * rows — `sidebarNav.tsx` resolves the icon name through `lucideIconMap`. There is no hardcoded
 * menu and no name→icon table.
 */
export function useMenuModules() {
  useSignals();
  // The ABBREVIATION, not the display name — see store/module.tsx. Empty when the user came
  // straight from the Home portal, in which case the sidebar falls back to its own subsystem.
  const selectedSubsystem = store.ModuleData.value.abbreviation;

  const { data: modules, isLoading } = useQuery({
    queryKey: ["moduleWithOperations"],
    queryFn: () => GetAllModuleWithOperation(),
    staleTime: 5 * 60 * 1000,
  });

  useEffect(() => {
    if (!modules?.data) return;

    const allPermissions: UserPermissionModel[] = [];

    modules.data.forEach((module: ModuleModel) => {
      module.operations?.forEach((operation) => {
        allPermissions.push({
          id: operation.id,
          operationId: operation.id,
          operation: operation.name,
          module: module.name,
          link: operation.link,
          // ⚠️ DENY by default. These read `?? true` until 2026-08-19, so any privilege the
          // feed did not carry was treated as granted — which is how a revoked Create still drew
          // an enabled Add button. canView is the exception: an operation only reaches a SPA at
          // all when the view grant let it through the server-side filter.
          canView: operation.canView ?? true,
          canAdd: operation.canAdd ?? false,
          canEdit: operation.canEdit ?? false,
          canDelete: operation.canDelete ?? false,
          canApprove: operation.canApprove ?? false,
          canExport: operation.canExport ?? false,
          details: [],
        });
      });
    });

    store.PermissionData.value = allPermissions;
  }, [modules]);

  useEffect(() => {
    if (modules?.data) {
      // loadWorkflow();
    }
  }, [modules]);

  return {
    isLoading,
    selectedSubsystem,
    /** Raw menu feed (modules + role-visible operations) for the grouped sidebar. */
    modules: modules?.data as ModuleModel[] | undefined,
  };
}
