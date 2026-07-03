import type { ReactNode } from "react";
import { Inbox } from "lucide-react";
import { useTranslation } from "react-i18next";

export interface EmptyStateProps {
  icon?: ReactNode;
  title?: string;
  description?: string;
  action?: ReactNode;
  className?: string;
}

function EmptyState({
  icon,
  title,
  description,
  action,
  className = "",
}: EmptyStateProps) {
  const { t } = useTranslation();

  return (
    <div
      className={`flex min-h-[200px] flex-col items-center justify-center gap-3 rounded-xl border border-dashed border-border bg-card/50 px-6 py-10 text-center ${className}`}
    >
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-muted/40 text-muted">
        {icon ?? <Inbox className="h-6 w-6" aria-hidden />}
      </div>
      <div className="max-w-md space-y-1">
        <p className="text-sm font-semibold text-foreground">
          {title ?? t("No data available")}
        </p>
        {description ? (
          <p className="text-xs text-muted">{description}</p>
        ) : null}
      </div>
      {action}
    </div>
  );
}

export default EmptyState;
