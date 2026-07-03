import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

type ButtonVariant = "primary" | "secondary" | "outline" | "ghost" | "danger";

interface Props {
  value: string;
  icon?: ReactNode;
  className?: string;
  iconClassName?: string;
  htmlType?: "submit" | "button" | "reset";
  variant?: ButtonVariant;
  disabled?: boolean;
  onClick?: (e: React.MouseEvent<HTMLButtonElement>) => void;
}

const VARIANT_CLASS: Record<ButtonVariant, string> = {
  primary:
    "border border-primary bg-primary text-on-accent hover:bg-primary-hover focus:ring-primary/30",
  secondary:
    "border border-border bg-secondary text-foreground hover:bg-muted focus:ring-primary/20",
  outline:
    "border border-border bg-transparent text-foreground hover:bg-secondary focus:ring-primary/20",
  ghost:
    "border border-transparent bg-transparent text-foreground hover:bg-secondary focus:ring-primary/20",
  danger:
    "border border-error/40 bg-error/10 text-error hover:bg-error/20 focus:ring-error/30",
};

const ButtonField = ({
  value = "",
  icon,
  className,
  iconClassName,
  onClick,
  htmlType = "button",
  disabled = false,
  variant = "primary",
}: Props) => {
  const { t } = useTranslation();

  return (
    <button
      type={htmlType}
      disabled={disabled}
      onClick={onClick}
      className={`inline-flex h-10 items-center justify-center gap-2 rounded-lg text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60 max-md:w-full ${value ? "px-4" : "px-2.5"} ${VARIANT_CLASS[variant]} ${className ?? ""}`}
    >
      {icon ? <span className={`inline-flex shrink-0 ${iconClassName ?? ""}`}>{icon}</span> : null}
      {value ? <span>{t(value)}</span> : null}
    </button>
  );
};

export default ButtonField;
