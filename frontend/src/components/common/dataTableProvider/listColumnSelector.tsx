import { useEffect, useRef, useState } from "react";
import { Columns3 } from "lucide-react";
import { useTranslation } from "react-i18next";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { toast } from "../toast";

export interface ListColumnConfig {
  columns: DataTableColumnModel[];
  selectedCols: DataTableColumnModel[];
  onChange: (cols: DataTableColumnModel[]) => void;
  onReset?: () => void;
  disabled?: boolean;
}

const toolbarBtnClass =
  "flex h-9 w-9 items-center justify-center rounded-lg border border-border bg-background p-0 text-primary shadow-none transition-colors hover:bg-primary/20 disabled:cursor-not-allowed disabled:opacity-50";

function ListColumnSelector({
  columns,
  selectedCols,
  onChange,
  onReset,
  disabled = false,
}: ListColumnConfig) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (rootRef.current && !rootRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    };
  document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const toggleColumn = (col: DataTableColumnModel, checked: boolean) => {
    const colKey = col.name ?? col.key;
    if (!colKey) return;

    if (checked) {
      if (selectedCols.some((item) => (item.name ?? item.key) === colKey)) return;
      onChange([...selectedCols, col]);
      return;
    }

    if (selectedCols.length <= 1) {
      toast.error(t("At least one column must remain visible"));
      return;
    }

    onChange(
      selectedCols.filter((item) => (item.name ?? item.key) !== colKey),
    );
  };

  const labelFor = (col: DataTableColumnModel) => {
    const label = col.label ?? col.name ?? "";
    return typeof label === "string" ? t(label) : String(label);
  };

  return (
    <div className="relative" ref={rootRef}>
      <button
        type="button"
        onClick={() => setOpen((value) => !value)}
        disabled={disabled || columns.length === 0}
        className={toolbarBtnClass}
        title={t("Columns")}
        aria-label={t("Columns")}
        aria-haspopup="true"
        aria-expanded={open}
      >
        <Columns3 size={16} />
      </button>

      {open ? (
        <div
          role="menu"
          className="absolute right-0 z-50 mt-2 max-h-64 min-w-48 overflow-auto rounded-lg border border-border bg-card py-1 shadow-lg"
        >
          {columns.map((col) => {
            const colKey = col.name ?? col.key;
            const checked = selectedCols.some(
              (item) => (item.name ?? item.key) === colKey,
            );
            return (
              <label
                key={col.name ?? col.key}
                className="flex cursor-pointer items-center gap-2 px-3 py-2 text-sm transition-colors hover:bg-secondary"
              >
                <input
                  type="checkbox"
                  className="accent-primary"
                  checked={checked}
                  disabled={disabled}
                  onChange={(event) => toggleColumn(col, event.target.checked)}
                />
                <span className="text-foreground">{labelFor(col)}</span>
              </label>
            );
          })}
          {onReset ? (
            <div className="border-t border-border px-2 py-1.5">
              <button
                type="button"
                className="w-full rounded-md px-2 py-1.5 text-left text-xs font-medium text-primary transition-colors hover:bg-primary/10"
                onClick={() => {
                  onReset();
                  setOpen(false);
                }}
              >
                {t("Reset columns")}
              </button>
            </div>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}

export default ListColumnSelector;
