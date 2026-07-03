import Badge, { type BadgeVariant } from "./badge";

export interface TagBadgeProps {
  value?: string | null;
  variant?: BadgeVariant;
  className?: string;
}

function TagBadge({ value, variant = "secondary", className = "" }: TagBadgeProps) {
  const label = value?.trim() || "-";

  return (
    <Badge variant={variant} className={className}>
      <span className="truncate">{label}</span>
    </Badge>
  );
}

export default TagBadge;
