import type { LucideIcon } from "lucide-react";
import type { ReactNode } from "react";
import Badge, { type BadgeVariant } from "./badge";

export interface IconBadgeProps {
  value?: string | null;
  icon: LucideIcon;
  variant?: BadgeVariant;
  className?: string;
  href?: string;
  children?: ReactNode;
}

function IconBadge({
  value,
  icon: Icon,
  variant = "secondary",
  className = "",
  href,
  children,
}: IconBadgeProps) {
  const label = value?.trim() || "-";
  const content = (
    <Badge variant={variant} className={`max-w-[220px] ${className}`}>
      <Icon className="h-3 w-3 shrink-0 opacity-80" aria-hidden />
      <span className="truncate">{children ?? label}</span>
    </Badge>
  );

  if (href && label !== "-") {
    return (
      <a href={href} className="inline-flex hover:opacity-90" title={label}>
        {content}
      </a>
    );
  }

  return content;
}

export default IconBadge;
