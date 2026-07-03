import Badge, { type BadgeVariant } from "./badge";
import { resolveStatusTone } from "./resolveStatusTone";

export interface StatusBadgeProps {
  value?: string | null;
  variant?: BadgeVariant;
  className?: string;
  showDot?: boolean;
}

const dotColor: Record<BadgeVariant, string> = {
  default: "bg-primary",
  secondary: "bg-muted",
  outline: "bg-muted",
  success: "bg-success",
  warning: "bg-warning",
  error: "bg-error",
  info: "bg-info",
  muted: "bg-muted",
};

function StatusBadge({
  value,
  variant,
  className = "",
  showDot = true,
}: StatusBadgeProps) {
  const label = value?.trim() || "-";
  const tone = variant ?? resolveStatusTone(label);

  return (
    <Badge variant={tone} className={`tracking-wide ${className}`}>
      {showDot && (
        <span
          className={`h-1.5 w-1.5 shrink-0 rounded-full ${dotColor[tone]}`}
          aria-hidden
        />
      )}
      <span className="truncate">{label}</span>
    </Badge>
  );
}

export default StatusBadge;
