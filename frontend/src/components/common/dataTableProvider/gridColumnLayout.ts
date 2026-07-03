import type { DataTableColumnModel } from "@/models";

const HIGHLIGHT_FIELD_NAMES = new Set([
  "status",
  "amount",
  "salestype",
  "salesType",
  "isvisible",
  "isVisible",
  "ismandatory",
  "isMandatory",
  "isrequired",
  "isRequired",
  "type",
  "module",
  "role",
  "subSystem",
]);

/** Rows shown under the card header before “Show more”. Default none — use `gridPreview` to opt in. */
const DEFAULT_PREVIEW_COUNT = 0;

export interface GridColumnLayout {
  primary: DataTableColumnModel | undefined;
  highlights: DataTableColumnModel[];
  preview: DataTableColumnModel[];
  hidden: DataTableColumnModel[];
  action: DataTableColumnModel | undefined;
}

function isActionColumn(col: DataTableColumnModel) {
  return col.name === "Action";
}

function isHighlightColumn(col: DataTableColumnModel) {
  if (col.gridHighlight) return true;
  const name = (col.name ?? "").toLowerCase();
  return HIGHLIGHT_FIELD_NAMES.has(col.name ?? "") || HIGHLIGHT_FIELD_NAMES.has(name);
}

export function buildGridColumnLayout(
  columns: DataTableColumnModel[] = [],
): GridColumnLayout {
  const action = columns.find(isActionColumn);
  const dataCols = columns.filter((col) => !isActionColumn(col) && !col.gridOmit);

  const primary = dataCols.find((col) => col.gridPrimary) ?? dataCols[0];
  const rest = dataCols.filter((col) => col !== primary);

  const hasExplicitHighlight = rest.some((col) => col.gridHighlight === true);
  const highlights = (
    hasExplicitHighlight
      ? rest.filter((col) => col.gridHighlight)
      : rest.filter(isHighlightColumn)
  ).slice(0, 2);

  const highlightNames = new Set(highlights.map((c) => c.name));
  const afterHighlights = rest.filter((col) => !highlightNames.has(col.name));

  const explicitPreview = afterHighlights.filter((col) => col.gridPreview);
  const pool = afterHighlights.filter((col) => !col.gridPreview);
  const fillCount = Math.max(0, DEFAULT_PREVIEW_COUNT - explicitPreview.length);
  const preview = [...explicitPreview, ...pool.slice(0, fillCount)];

  const previewNames = new Set(preview.map((c) => c.name));
  const hidden = afterHighlights.filter((col) => !previewNames.has(col.name));

  return { primary, highlights, preview, hidden, action };
}
