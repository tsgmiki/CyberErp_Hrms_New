import type { ModuleModel } from "@/models";
import type { ReactNode } from "react";
import { getModuleIcon } from "@/components/menu/utils/getModuleIcon";

export interface LandingSubsystemCard {
  id: string;
  title: string;
  description: string;
  icon: ReactNode;
  previewItems: string[];
  totalItemCount: number;
}

export function buildLandingSubsystems(modules: ModuleModel[]): LandingSubsystemCard[] {
  const bySubsystem = new Map<string, ModuleModel[]>();

  for (const module of modules) {
    const key = (module.subSystem || module.name || "General").trim();
    if (!key) continue;
    const list = bySubsystem.get(key) ?? [];
    list.push(module);
    bySubsystem.set(key, list);
  }

  return Array.from(bySubsystem.entries()).map(([title, subsystemModules]) => {
    const moduleNames = subsystemModules
      .map((module) => module.name)
      .filter((name): name is string => Boolean(name?.trim()));

    const operations = subsystemModules.flatMap((module) =>
      (module.operations ?? [])
        .map((operation) => operation.name)
        .filter((name): name is string => Boolean(name?.trim())),
    );

    const previewItems = operations.length > 0 ? operations : moduleNames;

    return {
      id: title,
      title,
      description:
        moduleNames.length > 0
          ? moduleNames.join(", ")
          : `Access ${title} features`,
      icon: getModuleIcon(title),
      previewItems,
      totalItemCount: previewItems.length,
    };
  });
}
