import { useTranslation } from "react-i18next";

interface DataTableSelectionBarProps {
  count: number;
  onClear: () => void;
}

function DataTableSelectionBar({ count, onClear }: DataTableSelectionBarProps) {
  const { t } = useTranslation();

  if (count <= 0) return null;

  return (
    <div className="mb-2 flex items-center justify-between gap-3 rounded-lg border border-primary/30 bg-primary/10 px-3 py-2 text-sm">
      <span className="font-medium text-foreground">
        {count} {t("selected")}
      </span>
      <button
        type="button"
        onClick={onClear}
        className="rounded-md px-2 py-1 text-xs font-medium text-primary transition-colors hover:bg-primary/15"
      >
        {t("Clear selection")}
      </button>
    </div>
  );
}

export default DataTableSelectionBar;
