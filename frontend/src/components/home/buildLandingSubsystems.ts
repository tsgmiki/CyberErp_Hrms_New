import type { ModuleModel, SubsystemModel } from "@/models";
import type { LucideIcon } from "lucide-react";
import { resolveNavIcon } from "@/components/menu/utils/lucideIconMap";

export interface LandingSubsystemCard {
  id: string;
  title: string;
  description: string;
  /**
   * Resolved from the subsystem row's own Icon column, NOT from a hardcoded name table — that
   * table was a PSMS-template leftover (Purchases, Inventory, Container…) that matched almost no
   * real subsystem, so most cards drew a blank circle and the icon could not be configured at all.
   */
  Icon: LucideIcon;
  previewItems: string[];
  totalItemCount: number;
}

export function buildLandingSubsystems(
  modules: ModuleModel[],
  subsystemRows?: SubsystemModel[],
): LandingSubsystemCard[] {
  const bySubsystem = new Map<string, ModuleModel[]>();

  for (const module of modules) {
    const key = (module.subSystem || module.name || "General").trim();
    if (!key) continue;
    const list = bySubsystem.get(key) ?? [];
    list.push(module);
    bySubsystem.set(key, list);
  }

  // Cards are keyed by subsystem NAME (that is all the menu feed carries), so the icon is looked up
  // by the same key on the master rows.
  const iconByName = new Map(
    (subsystemRows ?? []).map((row) => [(row.name ?? "").trim(), row.icon]),
  );

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
      Icon: resolveNavIcon(iconByName.get(title)),
      previewItems,
      totalItemCount: previewItems.length,
    };
  });
}
