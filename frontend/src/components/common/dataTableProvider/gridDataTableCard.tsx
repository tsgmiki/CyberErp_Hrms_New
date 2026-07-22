import type { DataTableColumnModel } from "@/models";
import { ChevronDown, ChevronUp } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { GridColumnLayout } from "./gridColumnLayout";
import { DataTableCheckbox } from "./dataTableCheckbox";

const isString = (v: unknown): v is string => typeof v === "string";

interface GridDataTableCardProps {
  row: Record<string, unknown>;
  rowId: string;
  layout: GridColumnLayout;
  isExpanded: boolean;
  onToggleExpand: () => void;
  checkBox?: boolean;
  isChecked?: boolean;
  onCheckChange?: (checked: boolean) => void;
}

function renderCell(
  column: DataTableColumnModel,
  row: Record<string, unknown>,
  fallback: string,
) {
  const value = row[column.name as string];
  if (column.gridRender) {
    return column.gridRender(value as string, row);
  }
  if (column.render) {
    return column.render(value as string, row);
  }
  const text = value != null && value !== "" ? String(value) : fallback;
  return <span className="text-sm text-foreground">{text}</span>;
}

function GridDataTableCard({
  row,
  rowId,
  layout,
  isExpanded,
  onToggleExpand,
  checkBox,
  isChecked = false,
  onCheckChange,
}: GridDataTableCardProps) {
  const { t } = useTranslation();
  const { primary, highlights, preview, hidden, action } = layout;
  const hasHidden = hidden.length > 0;
  const na = t("N/A");

  const labelFor = (col: DataTableColumnModel) =>
    isString(col.label) ? t(col.label) : col.label;

  return (
    <article
      className={`group flex flex-col overflow-hidden rounded-xl border bg-card shadow-sm transition-all duration-200 hover:border-primary/35 hover:shadow-md ${
        isChecked
          ? "border-primary/45 ring-2 ring-primary/25 bg-[color-mix(in_srgb,var(--primary)_6%,var(--card))]"
          : "border-border"
      }`}
    >
      <header className="relative border-b border-grid-divider bg-[color-mix(in_srgb,var(--secondary)_35%,var(--card))] px-4 pb-3 pt-4">
        {checkBox && (
          <div
            className="absolute left-2.5 top-2.5 z-10 rounded-md bg-card/90 p-0.5 shadow-sm backdrop-blur-sm"
            onClick={(e) => e.stopPropagation()}
            onKeyDown={(e) => e.stopPropagation()}
            role="presentation"
          >
            <DataTableCheckbox
              checked={isChecked}
              onChange={(checked) => onCheckChange?.(checked)}
              onClick={(e) => e.stopPropagation()}
              ariaLabel={t("Select row")}
            />
          </div>
        )}

        {action && (
          <div
            className="absolute right-2 top-2 z-10"
            onClick={(e) => e.stopPropagation()}
            onKeyDown={(e) => e.stopPropagation()}
            role="presentation"
          >
            {action.render?.(row[action.name as string] as string, row) ?? null}
          </div>
        )}

        <div
          className={`flex items-start gap-3 ${checkBox ? "pl-8" : ""} ${action ? "pr-10" : ""}`}
        >
          {primary && (
            <div className="min-w-0 flex-1">
              {!primary.gridHideLabel && (
                <p className="mb-1 text-[10px] font-semibold uppercase tracking-wider text-muted">
                  {labelFor(primary)}
                </p>
              )}
              <div className="text-base font-semibold leading-snug text-foreground">
                {renderCell(primary, row, na)}
              </div>
            </div>
          )}

          {highlights.length > 0 && (
            <div className="flex shrink-0 flex-col items-end gap-1.5">
              {highlights.map((col) => (
                <div key={col.name ?? col.key} className="min-w-0">
                  {renderCell(col, row, na)}
                </div>
              ))}
            </div>
          )}
        </div>
      </header>

      <div
        id={`grid-details-${rowId}`}
        className={`flex flex-1 flex-col gap-2 px-4 ${preview.length > 0 ? "py-3" : "pb-3 pt-2"}`}
      >
        {preview.map((col) => (
          <div
            key={col.name ?? col.key}
            className="grid grid-cols-[minmax(0,38%)_1fr] items-start gap-2 border-b border-grid-divider pb-2 last:border-0 last:pb-0"
          >
            <span className="text-xs font-medium text-muted">{labelFor(col)}</span>
            <div className="min-w-0">{renderCell(col, row, na)}</div>
          </div>
        ))}

        {isExpanded &&
          hidden.map((col) => (
            <div
              key={col.name ?? col.key}
              className="grid grid-cols-[minmax(0,38%)_1fr] items-start gap-2 border-b border-grid-divider pb-2 last:border-0 last:pb-0"
            >
              <span className="text-xs font-medium text-muted">{labelFor(col)}</span>
              <div className="min-w-0">{renderCell(col, row, na)}</div>
            </div>
          ))}

        {hasHidden && (
          <button
            type="button"
            onClick={onToggleExpand}
            className="mt-1 flex w-full items-center justify-center gap-1 rounded-lg border border-border bg-secondary/50 py-1.5 text-xs font-medium text-primary transition-colors hover:bg-primary/10"
            aria-expanded={isExpanded}
            aria-controls={`grid-details-${rowId}`}
          >
            {isExpanded ? (
              <>
                <ChevronUp size={14} />
                {t("Show less")}
              </>
            ) : (
              <>
                <ChevronDown size={14} />
                {t("Show more")} ({hidden.length})
              </>
            )}
          </button>
        )}
      </div>
    </article>
  );
}

export default GridDataTableCard;
