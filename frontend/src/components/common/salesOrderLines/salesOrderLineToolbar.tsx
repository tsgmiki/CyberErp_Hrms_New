import { List, Plus } from "lucide-react";
import { useTranslation } from "react-i18next";

interface SalesOrderLineToolbarProps {
  hideAdd?: boolean;
  showListMode?: boolean;
  onPrimaryAction: () => void;
  addLabel?: string;
  listLabel?: string;
  count?: number;
}

export function SalesOrderLineToolbar({
  hideAdd,
  showListMode = false,
  onPrimaryAction,
  addLabel,
  listLabel,
  count = 0,
}: SalesOrderLineToolbarProps) {
  const { t } = useTranslation();

  if (hideAdd) return null;

  return (
    <div className="flex flex-wrap items-center justify-between gap-2 border-b border-grid-divider px-4 py-3">
      <button
        type="button"
        onClick={onPrimaryAction}
        className="inline-flex h-9 items-center gap-2 rounded-lg border border-border bg-secondary px-3 text-sm font-medium text-primary transition-colors hover:bg-primary/10"
      >
        {showListMode ? <List className="h-4 w-4" /> : <Plus className="h-4 w-4" />}
        {showListMode ? (listLabel ?? t("View list")) : (addLabel ?? t("Add line"))}
      </button>
      <span className="text-xs text-muted">
        <span className="font-semibold text-foreground">{count}</span> {t("lines")}
      </span>
    </div>
  );
}

export default SalesOrderLineToolbar;
