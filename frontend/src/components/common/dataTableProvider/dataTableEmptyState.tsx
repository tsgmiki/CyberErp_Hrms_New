import { useTranslation } from "react-i18next";
import EmptyState from "../emptyState";

interface DataTableEmptyStateProps {
  title?: string;
  description?: string;
  hasSearch?: boolean;
  onClearSearch?: () => void;
}

function DataTableEmptyState({
  title,
  description,
  hasSearch = false,
  onClearSearch,
}: DataTableEmptyStateProps) {
  const { t } = useTranslation();

  return (
    <EmptyState
      title={title}
      description={
        description ??
        (hasSearch ? t("Try adjusting your search or filters.") : undefined)
      }
      action={
        hasSearch && onClearSearch ? (
          <button
            type="button"
            onClick={onClearSearch}
            className="rounded-lg border border-border bg-secondary px-3 py-1.5 text-xs font-medium text-foreground transition-colors hover:bg-muted/50"
          >
            {t("Clear search")}
          </button>
        ) : undefined
      }
    />
  );
}

export default DataTableEmptyState;
