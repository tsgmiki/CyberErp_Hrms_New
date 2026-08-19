import type { ModuleModel, SubsystemModel } from "@/models";
import type { LucideIcon } from "lucide-react";
import { resolveNavIcon } from "@/components/menu/utils/lucideIconMap";

export interface LandingSubsystemCard {
  /**
   * The subsystem's ABBREVIATION — the card's identity, what selecting it hands back, and the key
   * every downstream lookup uses (app URL, portal exclusion, sidebar scope).
   *
   * ⚠️ This was the display NAME until 2026-08-19. Names are labels an administrator renames; when
   * "HRMS" became "Human Resource Management System" every name-keyed match broke at once.
   */
  id: string;
  /** The subsystem's display name — shown on the card, never matched on. */
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
    // Group by abbreviation; the name is only a fallback for a row that has none.
    const key = (module.subSystemAbbreviation || module.subSystem || module.name || "General").trim();
    if (!key) continue;
    const list = bySubsystem.get(key) ?? [];
    list.push(module);
    bySubsystem.set(key, list);
  }

  // Master rows indexed by the same key the cards are grouped on, supplying the display name and
  // the configured icon.
  const rowByAbbreviation = new Map(
    (subsystemRows ?? []).map((row) => [(row.abbreviation ?? "").trim().toUpperCase(), row]),
  );

  return Array.from(bySubsystem.entries()).map(([abbreviation, subsystemModules]) => {
    const row = rowByAbbreviation.get(abbreviation.toUpperCase());
    const title = (row?.name || subsystemModules[0]?.subSystem || abbreviation).trim();

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
      id: abbreviation,
      title,
      description:
        moduleNames.length > 0
          ? moduleNames.join(", ")
          : `Access ${title} features`,
      Icon: resolveNavIcon(row?.icon),
      previewItems,
      totalItemCount: previewItems.length,
    };
  });
}
