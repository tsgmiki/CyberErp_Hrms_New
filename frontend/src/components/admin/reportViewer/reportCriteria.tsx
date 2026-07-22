"use client";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { SlidersHorizontal } from "lucide-react";
import type { ReportSchemaFieldModel } from "@/models";
import Loading from "@/components/common/loader/loader";
import EmptyState from "@/components/common/emptyState";

interface ReportCriteriaProps {
  fields: ReportSchemaFieldModel[];
  loading?: boolean;
  /** Renders one criteria field with the project's standard form controls. Provided by the caller so
   * this stays a pure presentational layout (reusable for any schema-driven filter form). */
  renderField: (field: ReportSchemaFieldModel) => ReactNode;
}

/**
 * Presentational filter-criteria layout for the Report tab: one criterion per row (a centered single
 * column), the only exception being a From/To date range, which renders its two inputs side-by-side
 * within its own row. The field rendering is injected via `renderField`, so this component is reusable
 * for any schema-driven filter panel.
 */
function ReportCriteria({ fields, loading, renderField }: ReportCriteriaProps) {
  const { t } = useTranslation();

  if (loading) return <div className="p-8"><Loading /></div>;

  if (fields.length === 0)
    return (
      <div className="p-6">
        <EmptyState
          icon={<SlidersHorizontal className="h-6 w-6" />}
          title={t("No filter criteria")}
          description={t("This report runs without any filters — just generate it.")}
        />
      </div>
    );

  // Layout rows: two consecutive Date fields (e.g. a From date + a To date) pair onto one row;
  // every other input takes its own row. (A '#' From/To range is a single field that already
  // renders its two inputs side-by-side, so it stays on its own row.)
  const rows: ReportSchemaFieldModel[][] = [];
  for (let i = 0; i < fields.length; i++) {
    const f = fields[i];
    const next = fields[i + 1];
    if (!f.isRange && f.dataType === "Date" && next && !next.isRange && next.dataType === "Date") {
      rows.push([f, next]);
      i++;
    } else {
      rows.push([f]);
    }
  }

  return (
    <div className="mx-auto w-full max-w-2xl">
      {/* Selection-screen section header (enterprise ERP convention). */}
      <div className="mb-4 flex items-center gap-2 border-b border-border pb-2">
        <SlidersHorizontal className="h-4 w-4 text-primary" />
        <h4 className="text-xs font-semibold uppercase tracking-wide text-muted">{t("Filter Criteria")}</h4>
      </div>
      <div className="grid grid-cols-1 gap-4">
        {rows.map((row) =>
          row.length === 2 ? (
            <div key={row[0].field} className="grid grid-cols-2 gap-3">
              {row.map(renderField)}
            </div>
          ) : (
            <div key={row[0].field}>{renderField(row[0])}</div>
          ),
        )}
      </div>
    </div>
  );
}

export default ReportCriteria;
