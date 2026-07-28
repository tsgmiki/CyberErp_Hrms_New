"use client";
import { useMemo, useState, type ReactNode } from "react";
import { ArrowLeft, Plus } from "lucide-react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import ButtonField from "@/components/ui/buttonField";
import { EntityListShell } from "@/template";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type ParameterModel from "@/models/ParameterModel";
import type { ListDisplayMode } from "@/components/common/dataTableProvider/listViewToolbar";
import { parameterInitialData } from "@/constants/initialization";

export interface ChildColumn<T> {
  name: keyof T & string;
  label: string;
  render?: (value: unknown, record: T) => ReactNode;
}

/** Server-side paging pass-through (e.g. dynamic-form records) — otherwise paging is client-side. */
export interface ChildPaging {
  param: ParameterModel;
  setParam: (updater: ParameterModel | ((prev: ParameterModel) => ParameterModel)) => void;
  total: number;
}

interface Props<T extends { id?: string }> {
  title: string;
  addLabel: string;
  columns: ChildColumn<T>[];
  rows: T[] | undefined;
  isLoading: boolean;
  error?: string | null;
  onAdd: () => void;
  onEdit: (record: T) => void;
  onDelete: (id: string) => void;
  /** View-only: hides the add button and the per-row action column. */
  readOnly?: boolean;
  /** Optional note shown under the header (e.g. why the table is read-only). */
  hint?: string;
  /** Custom Action-cell renderer (e.g. movement execute/cancel buttons). Default = edit + delete. */
  renderActions?: (record: T) => ReactNode;
  /** Server-paged consumers pass their param/total through; otherwise rows are paged client-side. */
  paging?: ChildPaging;
  /**
   * INLINE form mode (the Guarantees-tab layout — no popup): when true, the grid is replaced by
   * `formView` under a Back-to-list bar. Sections keep their add/edit state; the form itself is
   * the section's FormProvider rendered inline (no showModal).
   */
  formOpen?: boolean;
  /** Title of the inline form bar (e.g. "Add Education" / "Edit Education"). */
  formTitle?: string;
  formView?: ReactNode;
  onBack?: () => void;
}

const cellText = (value: unknown): string => (value == null ? "" : String(value));

/**
 * Employee child collection manager — SAME look as the profile Guarantees tab: the standard
 * `EntityListShell` grid (search, column selector, export, list/grid toggle, pagination) with an
 * Add button above it, and an inline (non-popup) form view behind a Back-to-list bar.
 */
function ChildManager<T extends { id?: string }>({
  title,
  addLabel,
  columns,
  rows,
  isLoading,
  error,
  onAdd,
  onEdit,
  onDelete,
  readOnly = false,
  hint,
  renderActions,
  paging,
  formOpen = false,
  formTitle,
  formView,
  onBack,
}: Props<T>) {
  const { t } = useTranslation();
  const [localParam, setLocalParam] = useState<ParameterModel>(
    () => ({ ...parameterInitialData, sortCol: "", dir: "asc" }) as ParameterModel,
  );
  const [displayMode, setDisplayMode] = useState<ListDisplayMode>("list");

  const param = paging?.param ?? localParam;
  const setParam = paging?.setParam ?? setLocalParam;

  // Client-side search + sort + page (small per-employee collections; server-paged consumers
  // pass `paging` and their rows are used as-is).
  const processed = useMemo(() => {
    if (paging) return rows ?? [];
    let list = rows ?? [];
    const q = (param.searchText ?? "").trim().toLowerCase();
    if (q)
      list = list.filter((r) =>
        columns.some((c) => cellText(r[c.name]).toLowerCase().includes(q)));
    if (param.sortCol && param.sortCol !== "createdAt") {
      const dir = String(param.dir).toUpperCase() === "DESC" ? -1 : 1;
      list = [...list].sort((a, b) =>
        dir * cellText(a[param.sortCol as keyof T]).localeCompare(cellText(b[param.sortCol as keyof T]), undefined, { numeric: true }));
    }
    return list;
  }, [rows, paging, param.searchText, param.sortCol, param.dir, columns]);

  const total = paging?.total ?? processed.length;
  const pageRows = useMemo(
    () => (paging ? processed : processed.slice(param.skip, param.skip + param.take)),
    [processed, paging, param.skip, param.take],
  );

  const tableColumns = useMemo(
    () =>
      [
        ...columns.map((c) => ({
          name: c.name,
          label: c.label,
          sort: !paging,
          render: c.render
            ? (v: string, r: T) => c.render!(v, r)
            : undefined,
        })),
        ...(!readOnly
          ? [{
              name: "Action",
              label: "Action",
              render: (_v: string, r: T) =>
                renderActions ? (
                  renderActions(r)
                ) : (
                  <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
                    editHandler={() => onEdit(r)} deleteHandler={() => r.id && onDelete(r.id)} />
                ),
            }]
          : []),
      ] as DataTableColumnModel[],
    [columns, readOnly, renderActions, onEdit, onDelete, paging],
  );

  // Inline list ↔ form swap (same UX as the profile Guarantees tab).
  if (formOpen && formView) {
    return (
      <div className="m-1 space-y-3">
        <div className="flex items-center justify-between">
          <button
            type="button"
            onClick={onBack}
            className="inline-flex h-9 items-center gap-1.5 rounded-lg border border-border px-3 text-sm font-medium text-foreground hover:bg-secondary/40"
          >
            <ArrowLeft className="h-4 w-4" /> {t("Back to list")}
          </button>
          {formTitle && <h3 className="text-sm font-semibold text-foreground">{t(formTitle)}</h3>}
        </div>
        {formView}
      </div>
    );
  }

  return (
    <div className="m-1 space-y-3">
      {(hint || !readOnly) && (
        <div className="flex items-center justify-between gap-3">
          <p className="text-xs text-muted">{hint ? t(hint) : ""}</p>
          {!readOnly && (
            <ButtonField
              value={t(addLabel)}
              icon={<Plus className="h-4 w-4" />}
              htmlType="button"
              variant="primary"
              onClick={onAdd}
            />
          )}
        </div>
      )}
      {error && (
        <div className="rounded border border-error/30 bg-error/15 px-3 py-2 text-xs text-error">
          {error}
        </div>
      )}
      <EntityListShell
        listKey={`child:${title}`}
        listLabel={title}
        columns={tableColumns}
        isLoading={isLoading}
        rows={pageRows as unknown[]}
        total={total}
        param={param}
        setParam={setParam}
        displayMode={displayMode}
        setDisplayMode={setDisplayMode}
        fetchAllData={async () => processed as unknown as Record<string, unknown>[]}
        className="flex h-full min-h-0 flex-col gap-3"
      />
    </div>
  );
}

export default ChildManager;
