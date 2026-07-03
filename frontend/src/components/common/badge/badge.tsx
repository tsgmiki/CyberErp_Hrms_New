import type { HTMLAttributes, ReactNode } from "react";

export type BadgeVariant =
  | "default"
  | "secondary"
  | "outline"
  | "success"
  | "warning"
  | "error"
  | "info"
  | "muted";

const variantClasses: Record<BadgeVariant, string> = {
  default:
    "border-transparent bg-primary/10 text-primary",
  secondary:
    "border-border bg-secondary text-foreground",
  outline:
    "border-border bg-card text-foreground",
  success:
    "border-success/20 bg-success/15 text-success",
  warning:
    "border-warning/20 bg-warning/15 text-warning",
  error:
    "border-error/20 bg-error/15 text-error",
  info:
    "border-info/20 bg-info/15 text-info",
  muted:
    "border-transparent bg-muted/30 text-muted-foreground",
};

export interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  children: ReactNode;
  variant?: BadgeVariant;
}

function joinClasses(...parts: Array<string | false | undefined>) {
  return parts.filter(Boolean).join(" ");
}

function Badge({ children, variant = "default", className = "", ...props }: BadgeProps) {
  return (
    <span
      className={joinClasses(
        "inline-flex max-w-full items-center gap-1 rounded-full border px-2.5 py-0.5 text-[11px] font-semibold leading-tight",
        variantClasses[variant],
        className,
      )}
      {...props}
    >
      {children}
    </span>
  );
}

export default Badge;
