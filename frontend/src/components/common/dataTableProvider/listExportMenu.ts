import type { TFunction } from "i18next";

export type ListExportFormatId = "excel" | "pdf" | "excel-all" | "pdf-all";

export interface ListExportMenuItem {
  id: ListExportFormatId;
  label: string;
}

export interface ListExportMenuOptions {
  /** Adds Excel/PDF entries for the full filtered dataset. */
  includeAll?: boolean;
  totalCount?: number;
}

/** Per-list export dropdown entries. */
export function listExportMenuItems(
  t: TFunction,
  listLabel: string,
  options?: ListExportMenuOptions,
): ListExportMenuItem[] {
  const pageScope = ` ${t("(current page)")}`;
  const allScope =
    options?.totalCount != null
      ? ` ${t("(all {{count}} records)", { count: options.totalCount })}`
      : ` ${t("(all filtered)")}`;

  const items: ListExportMenuItem[] = [
    {
      id: "excel",
      label: t("{{list}} — Excel{{scope}}", { list: listLabel, scope: pageScope }),
    },
    {
      id: "pdf",
      label: t("{{list}} — PDF{{scope}}", { list: listLabel, scope: pageScope }),
    },
  ];

  if (options?.includeAll) {
    items.push(
      {
        id: "excel-all",
        label: t("{{list}} — Excel{{scope}}", { list: listLabel, scope: allScope }),
      },
      {
        id: "pdf-all",
        label: t("{{list}} — PDF{{scope}}", { list: listLabel, scope: allScope }),
      },
    );
  }

  return items;
}

export function isAllExportFormat(id: ListExportFormatId): boolean {
  return id === "excel-all" || id === "pdf-all";
}

export function baseExportFormat(
  id: ListExportFormatId,
): "excel" | "pdf" {
  return id.startsWith("pdf") ? "pdf" : "excel";
}
