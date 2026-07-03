import type { DataTableColumnModel } from "@/models";
import { Settings } from "lucide-react";
import { useSignals } from "@preact/signals-react/runtime";
import { useState, useRef } from "react";
import { useTranslation } from "react-i18next";

function ColumnFilter(props: {
  columns?: DataTableColumnModel[];
  selectedCols?: DataTableColumnModel[];
  setSelected: Function;
  reportCategory?: string;
  reportName?: string;
}) {
  useSignals();
  const { t } = useTranslation();

  const {
    columns: colData,
    selectedCols,
    setSelected,
  } = props;
  const [selectedColumns, setSelectedColumns] = useState<
    DataTableColumnModel[]
  >(selectedCols || []);
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const oncolumnSelect = (item: DataTableColumnModel, isAdd: boolean) => {
    let newSelected;
    if (isAdd) {
      newSelected = [...(selectedCols || []), item];
    } else {
      newSelected = (selectedCols || []).filter(
        (a: DataTableColumnModel) => a.name != item.name,
      );
    }
    setSelected(newSelected);
    setSelectedColumns(newSelected);
  };

  return (
    <div className="relative inline-flex ml-3" ref={dropdownRef}>
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={`flex items-center gap-2 px-3 py-2 text-sm rounded-lg border transition-colors duration-200 bg-card border-border hover:bg-primary/20`}
      >
        <Settings size={14} />
        {t("Columns")}
      </button>

      {isOpen && (
        <div
          className={`absolute top-full left-0 mt-1 z-50 min-w-48 rounded-md border shadow-lg max-h-64 overflow-auto transition-colors duration-200 bg-card border-border`}
        >
          {colData?.map((item) => (
            <label
              key={item.name}
              className={`flex items-center gap-2 p-2 cursor-pointer transition-colors duration-200 hover:bg-primary/10`}
            >
              <input
                type="checkbox"
                className="cursor-pointer accent-primary"
                checked={selectedColumns.some((a) => a.name === item.name)}
                onChange={(e) => {
                  if (e.target.checked) {
                    oncolumnSelect(item, true);
                  } else {
                    oncolumnSelect(item, false);
                  }
                }}
              />
              <span
                className={`text-sm text-foreground`}
              >
                {item.label}
              </span>
            </label>
          ))}
        </div>
      )}
    </div>
  );
}

export default ColumnFilter;