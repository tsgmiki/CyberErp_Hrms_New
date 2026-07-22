import { useEffect, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import type { ModuleModel, UserPermissionModel } from "@/models";
import GetAllModuleWithOperation from "@/services/admin/module/getAllWithOperation";
import store from "@/store";
import { useSignals } from "@preact/signals-react/runtime";
import { buildSidebarNavigation } from "../utils/buildSidebarNavigation";

export function useMenuModules() {
  useSignals();
  const selectedSubsystem = store.ModuleData.value.name;

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
          canView: operation.canView ?? true,
          canAdd: operation.canAdd ?? true,
          canEdit: operation.canEdit ?? true,
          canDelete: operation.canDelete ?? true,
          canApprove: operation.canApprove ?? true,
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

  const navigation = useMemo(
    () => buildSidebarNavigation(modules?.data, selectedSubsystem),
    [modules?.data, selectedSubsystem],
  );

  return {
    isLoading,
    navigation,
    selectedSubsystem,
    /** Raw menu feed (modules + role-visible operations) for the grouped sidebar. */
    modules: modules?.data as ModuleModel[] | undefined,
  };
}
