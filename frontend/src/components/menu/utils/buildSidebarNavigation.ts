import type { ModuleModel } from "@/models";
import type { SidebarNavigationModel, SidebarNavItem } from "./menuTypes";
import { getModuleIcon, getOperationIcon } from "./getModuleIcon";

function toNavPath(link?: string) {
  if (!link) return "/";
  return link.startsWith("/") ? link : `/${link}`;
}

function mapOperations(module: ModuleModel): SidebarNavItem[] {
  return (module.operations ?? []).map((operation) => ({
    id: operation.id ?? `${module.id}-${operation.name}`,
    title: operation.name ?? "",
    path: toNavPath(operation.link),
    icon: getOperationIcon(operation.name),
  }));
}

function getUniqueSubsystems(modules: ModuleModel[]) {
  return [...new Set(modules.map((module) => module.subSystem).filter(Boolean))] as string[];
}

export function buildSidebarNavigation(
  modules: ModuleModel[] | undefined,
  selectedSubsystem?: string,
): SidebarNavigationModel {
  const allModules = modules ?? [];
  const isSubsystemSelected = Boolean(selectedSubsystem?.trim());

  if (!isSubsystemSelected) {
    const subsystems = getUniqueSubsystems(allModules);

    return {
      isSubsystemSelected: false,
      entries: subsystems.map((subsystem) => ({
        id: subsystem,
        title: subsystem,
        icon: getModuleIcon(subsystem),
        categories: [],
        flatItems: [],
      })),
    };
  }

  const scopedModules = allModules.filter((module) => module.subSystem === selectedSubsystem);

  return {
    isSubsystemSelected: true,
    selectedSubsystem,
    selectedSubsystemIcon: getModuleIcon(selectedSubsystem),
    entries: scopedModules.map((module) => {
      const items = mapOperations(module);

      return {
        id: module.id ?? module.name ?? "",
        title: module.name ?? "",
        icon: getModuleIcon(module.name),
        categories: [
          {
            key: `${module.id ?? module.name}`,
            category: module.name ?? "General",
            items,
          },
        ],
        flatItems: items,
      };
    }),
  };
}
