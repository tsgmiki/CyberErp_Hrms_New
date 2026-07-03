"use client";
import { memo } from "react";
import EditableInput from "./editableInput";
import type { ParameterModel } from "@/models";
import { Trash2 } from "lucide-react";
import { useTranslation } from "react-i18next";

interface Column {
  key: string;
  label: string;
  render?: (item: any, index: number) => React.ReactNode;
  editable?: boolean;
  type?: "text" | "number" | "currency" | "date" | "dropdown";
  options?: { id: string | number; name: string; rate?: number }[];
  width?: string;
  param?: ParameterModel;
  displayValue?: string | ((item: any) => string);
  paramValue?: (param: ParameterModel | undefined, item: any) => any;
  setParam?: React.Dispatch<React.SetStateAction<ParameterModel>>;
  isLoading?: boolean;
  onSelect?: (rowId: string, value: any, record: any) => void;
  onFocus?: (item: any) => void;
}

interface EditableTableProps {
  data: any[];
  columns: Column[];
  onUpdate?: (id: string, field: string, value: any, record?: any) => void;
  onRemove?: (id: string) => void;
  keyField?: string;
  showRowNo?: boolean;
  rowNoLabel?: string;
  showSubtotal?: boolean;
  subtotalLabel?: string;
  subtotalValue?: number;
  validationErrors?: Record<string, string>;
}

const EditableTable = memo(function EditableTable({
  data,
  columns,
  onUpdate,
  onRemove,
  keyField = "id",
  showRowNo = true,
  rowNoLabel = "#",
  validationErrors,
}: EditableTableProps) {
  const { t } = useTranslation();

  const renderEditableInput = (item: any, col: Column, index: number) => {
    const value = item[col.key];
    const errorKey = `${index}_${col.key}`;
    const error = validationErrors?.[errorKey];
    let computedDisplayValue = "";
    if (typeof col.displayValue === "function") {
      computedDisplayValue = col.displayValue(item);
    } else if (col.type === "dropdown" && value && col.options) {
      const selectedOption = col.options.find(
        (opt) => String(opt.id) === String(value),
      );
      computedDisplayValue = selectedOption?.name || "";
    } else if (typeof col.displayValue === "string") {
      computedDisplayValue = col.displayValue;
    }

    return (
      <div className="w-full min-w-0 py-0.5">
        <EditableInput
          value={value}
          type={col.type}
          options={col.options || []}
          param={col.param}
          paramValue={col.paramValue}
          item={item}
          setParam={col.setParam}
          isLoading={col.isLoading}
          displayValue={computedDisplayValue}
          error={error}
          onChange={(newValue) => onUpdate?.(item[keyField], col.key, newValue)}
          onSelect={(selectedValue, record) => {
            col.onSelect?.(item[keyField], selectedValue, record);
            onUpdate?.(item[keyField], col.key, selectedValue);
          }}
          onFocus={() => col.onFocus?.(item)}
        />
      </div>
    );
  };

  const renderCell = (item: any, col: Column, index: number) => {
    if (col.render) {
      return col.render(item, index);
    }
    if (col.editable) {
      return renderEditableInput(item, col, index);
    }
    const value = item[col.key];
    if (col.type === "currency" && typeof value === "number") {
      return <span className="text-foreground">{value.toLocaleString()}</span>;
    }
    return <span className="text-foreground">{value || "-"}</span>;
  };

  const headerRow = (
    <div className="sticky top-0 z-20 flex items-center justify-between border-b border-grid-divider bg-[color-mix(in_srgb,var(--secondary)_35%,var(--card))] px-4 py-3 text-[11px] font-semibold uppercase tracking-wider text-muted backdrop-blur-sm">
      <div className="flex min-w-0 flex-1 items-center gap-4">
        {showRowNo && <div className="w-10 shrink-0">{rowNoLabel}</div>}
        {columns.map((col) => (
          <div key={col.key} className={`min-w-0 ${col.width || "flex-1"}`}>
            {t(col.label)}
          </div>
        ))}
      </div>
      {onRemove && <div className="w-10 shrink-0" />}
    </div>
  );

  return (
    <div className="overflow-hidden rounded-lg border border-border/80 bg-card text-foreground shadow-sm">
      <div className="max-h-[min(450px,50vh)] overflow-y-auto">
        {headerRow}
        {data.length === 0 ? (
          <p className="px-4 py-8 text-center text-sm text-muted">
            {t("No lines yet. Use Add line above.")}
          </p>
        ) : (
          <div>
            {data.map((item, index) => (
              <div
                key={item[keyField] || index}
                className="group flex items-center justify-between gap-2 border-b border-grid-divider px-4 py-1 transition-colors last:border-b-0 hover:bg-secondary/50"
              >
                <div className="flex min-w-0 flex-1 items-center gap-3">
                  {showRowNo && (
                    <div className="w-10 shrink-0 text-sm text-muted">{index + 1}</div>
                  )}
                  {columns.map((col) => (
                    <div key={col.key} className={`min-w-0 ${col.width || "flex-1"}`}>
                      {renderCell(item, col, index)}
                    </div>
                  ))}
                </div>
                {onRemove && (
                  <button
                    type="button"
                    onClick={() => onRemove(item[keyField])}
                    className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md text-muted opacity-0 transition-all hover:bg-error/10 hover:text-error group-hover:opacity-100"
                    title={t("Remove line")}
                    aria-label={t("Remove line")}
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
});

export default EditableTable;


