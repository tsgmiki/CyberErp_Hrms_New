import type { LucideIcon } from "lucide-react";
import { memo } from "react";
import type { BadgeVariant } from "../badge/badge";

const variantIconClass: Record<BadgeVariant, string> = {
  default: "text-primary bg-primary/10",
  secondary: "text-muted bg-secondary",
  success: "text-success bg-success/10",
  warning: "text-warning bg-warning/10",
  error: "text-error bg-error/10",
  info: "text-info bg-info/10",
  outline: "text-foreground bg-secondary",
  muted: "text-muted bg-muted/30",
};

const variantActiveBorderClass: Record<BadgeVariant, string> = {
  default: "border-primary",
  secondary: "border-primary",
  success: "border-[var(--success)]",
  warning: "border-[var(--warning)]",
  error: "border-[var(--error)]",
  info: "border-[var(--info)]",
  outline: "border-primary",
  muted: "border-primary",
};

const variantActiveBgClass: Record<BadgeVariant, string> = {
  default: "bg-primary/8",
  secondary: "bg-primary/8",
  success: "bg-success/10",
  warning: "bg-warning/10",
  error: "bg-error/10",
  info: "bg-info/10",
  outline: "bg-primary/8",
  muted: "bg-primary/5",
};

interface StatusCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: LucideIcon;
  variant?: BadgeVariant;
  isActive?: boolean;
  onClick?: () => void;
}

function StatusCard({
  title,
  value,
  subtitle,
  icon: Icon,
  variant = "default",
  isActive,
  onClick,
}: StatusCardProps) {
  const interactive = Boolean(onClick);
  const iconClass = variantIconClass[variant] ?? variantIconClass.default;
  const activeBorder = variantActiveBorderClass[variant] ?? variantActiveBorderClass.default;
  const activeBg = variantActiveBgClass[variant] ?? variantActiveBgClass.default;

  return (
    <button
      type="button"
      disabled={!interactive}
      onClick={onClick}
      className={`group flex w-full min-h-17 items-center gap-2.5 rounded-lg border bg-card px-3 py-2.5 text-left transition-all duration-200 ${
        isActive
          ? `${activeBorder} ${activeBg} shadow-sm`
          : "border-border hover:border-[color-mix(in_srgb,var(--primary)_35%,var(--border))] hover:bg-muted/15"
      } ${interactive ? "cursor-pointer" : "cursor-default"}`}
    >
      {Icon ? (
        <div
          className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-md ${iconClass}`}
        >
          <Icon className="h-3.5 w-3.5" />
        </div>
      ) : null}

      <div className="min-w-0 flex-1">
        <div className="flex items-baseline justify-between gap-2">
          <p className="truncate text-lg font-bold leading-tight tracking-tight text-(--text)">
            {value}
          </p>
          {subtitle ? (
            <span className="shrink-0 text-[9px] font-semibold uppercase tracking-wide text-muted">
              {subtitle}
            </span>
          ) : null}
        </div>
        <p className="truncate text-[11px] leading-snug text-muted">{title}</p>
      </div>
    </button>
  );
}

export default memo(StatusCard);
