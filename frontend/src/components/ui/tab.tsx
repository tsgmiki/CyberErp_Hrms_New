import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

interface TabProps {
  label: ReactNode;
  onClick: () => void;
  isActive?: boolean;
  disabled?: boolean;
  count?: number;
}

function Tab({ label, onClick, isActive, disabled, count }: TabProps) {
  const { t } = useTranslation();
  const text = typeof label === "string" ? t(label) : label;

  return (
    <button
      type="button"
      role="tab"
      aria-selected={isActive}
      disabled={disabled}
      className={`inline-flex shrink-0 items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-all duration-200 ${
        disabled
          ? "cursor-not-allowed opacity-50"
          : isActive
            ? "bg-card font-semibold text-primary shadow-sm ring-1 ring-border/80"
            : "text-muted hover:bg-card/60 hover:text-foreground"
      }`}
      onClick={onClick}
    >
      <span>{text}</span>
      {count !== undefined && (
        <span
          className={`rounded-full px-1.5 py-0.5 text-[10px] font-semibold leading-none ${
            isActive
              ? "bg-primary/15 text-primary"
              : "bg-secondary text-muted"
          }`}
        >
          {count}
        </span>
      )}
    </button>
  );
}

export default Tab;
