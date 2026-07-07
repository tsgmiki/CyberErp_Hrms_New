"use client";
import type { ReactNode } from "react";
import { Plus } from "lucide-react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import Loading from "../../common/loader/loader";

export interface ChildColumn<T> {
  name: keyof T & string;
  label: string;
  render?: (value: unknown, record: T) => ReactNode;
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
}

/** Small in-profile table for employee child collections (education, experience, family). */
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
}: Props<T>) {
  const { t } = useTranslation();

  return (
    <div className="m-1 rounded-lg border border-border bg-card">
      <div className="flex items-center justify-between border-b border-border px-4 py-2.5">
        <h3 className="text-sm font-semibold text-foreground">{t(title)}</h3>
        <button
          type="button"
          onClick={onAdd}
          className="flex items-center gap-1 rounded bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
        >
          <Plus className="h-3.5 w-3.5" /> {t(addLabel)}
        </button>
      </div>

      {error && (
        <div className="mx-4 mt-2 rounded border border-error/30 bg-error/15 px-3 py-2 text-xs text-error">
          {error}
        </div>
      )}

      {isLoading ? (
        <Loading />
      ) : (rows?.length ?? 0) === 0 ? (
        <p className="px-4 py-8 text-center text-sm text-muted">
          {t("No records yet. Use the add button above.")}
        </p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-[13px]">
            <thead>
              <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                {columns.map((c) => (
                  <th key={c.name} className="px-4 py-2 font-semibold">
                    {t(c.label)}
                  </th>
                ))}
                <th className="px-4 py-2 text-right font-semibold">{t("Action")}</th>
              </tr>
            </thead>
            <tbody>
              {rows!.map((row) => (
                <tr key={row.id} className="border-b border-border/60 hover:bg-secondary/40">
                  {columns.map((c) => (
                    <td key={c.name} className="px-4 py-2.5 text-foreground">
                      {c.render ? c.render(row[c.name], row) : String(row[c.name] ?? "")}
                    </td>
                  ))}
                  <td className="px-4 py-1.5 text-right">
                    <GridAction
                      id={row.id || ""}
                      record={row}
                      showAdd={false}
                      showEdit
                      showDelete
                      editHandler={() => onEdit(row)}
                      deleteHandler={() => row.id && onDelete(row.id)}
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default ChildManager;
