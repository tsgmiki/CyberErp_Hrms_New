import { Check, Minus } from "lucide-react";
import { useEffect, useRef, type MouseEvent } from "react";

export type CheckboxMarkStyle = "inverse" | "primary";

export interface DataTableCheckboxProps {
  checked?: boolean;
  indeterminate?: boolean;
  onChange?: (checked: boolean) => void;
  onClick?: (e: MouseEvent<HTMLInputElement>) => void;
  ariaLabel: string;
  disabled?: boolean;
  className?: string;
  /** `inverse` = white mark on primary fill; `primary` = primary mark on card (clearer in light tables). */
  markStyle?: CheckboxMarkStyle;
}

const BOX_BASE =
  "relative flex h-[1.125rem] w-[1.125rem] shrink-0 items-center justify-center rounded-[5px] border border-border bg-card shadow-sm transition-all duration-150 group-hover:border-[color-mix(in_srgb,var(--primary)_50%,var(--border))] group-hover:bg-secondary group-focus-within:outline-none group-focus-within:ring-2 group-focus-within:ring-primary/30 group-focus-within:ring-offset-1 group-focus-within:ring-offset-card group-has-disabled:cursor-not-allowed group-has-disabled:opacity-50";

const BOX_MARK_INVERSE =
  "group-has-checked:border-primary group-has-checked:bg-primary group-has-indeterminate:border-primary group-has-indeterminate:bg-primary";

const BOX_MARK_PRIMARY =
  "group-has-checked:border-primary group-has-checked:bg-card group-has-indeterminate:border-primary group-has-indeterminate:bg-card";

/** High-contrast mark on primary fill — `color` sets Lucide stroke directly */
const INVERSE_CHECK_COLOR =
  "var(--checkbox-check, var(--text-on-accent, #ffffff))";

const ICON_BASE = "pointer-events-none shrink-0";

const INVERSE_ICON_CLASS = `${ICON_BASE} text-on-accent drop-shadow-[0_1px_2px_rgba(0,0,0,0.35)]`;

const PRIMARY_ICON_CLASS = `${ICON_BASE} text-primary`;

function boxClass(markStyle: CheckboxMarkStyle) {
  return `${BOX_BASE} ${markStyle === "primary" ? BOX_MARK_PRIMARY : BOX_MARK_INVERSE}`;
}

function markIcons(markStyle: CheckboxMarkStyle) {
  if (markStyle === "primary") {
    return {
      color: "var(--primary)",
      checkClass: PRIMARY_ICON_CLASS,
      minusClass: PRIMARY_ICON_CLASS,
    };
  }
  return {
    color: INVERSE_CHECK_COLOR,
    checkClass: INVERSE_ICON_CLASS,
    minusClass: INVERSE_ICON_CLASS,
  };
}

export function DataTableCheckbox({
  checked = false,
  indeterminate = false,
  onChange,
  onClick,
  ariaLabel,
  disabled = false,
  className = "",
  markStyle = "primary",
}: DataTableCheckboxProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const icons = markIcons(markStyle);

  useEffect(() => {
    if (inputRef.current) {
      inputRef.current.indeterminate = indeterminate;
    }
  }, [indeterminate]);

  return (
    <label
      className={`group inline-flex cursor-pointer items-center justify-center p-0.5 ${disabled ? "cursor-not-allowed opacity-60" : ""} ${className}`}
    >
      <input
        ref={inputRef}
        type="checkbox"
        className="sr-only"
        checked={checked}
        disabled={disabled}
        onChange={(e) => onChange?.(e.target.checked)}
        onClick={onClick}
        aria-label={ariaLabel}
      />
      <span className={boxClass(markStyle)} aria-hidden>
        <Check
          color={icons.color}
          className={`${icons.checkClass} h-4 w-4 opacity-0 transition-opacity duration-150 group-has-checked:opacity-100 group-has-indeterminate:opacity-0`}
          strokeWidth={markStyle === "primary" ? 3.25 : 4}
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden
        />
        <Minus
          color={icons.color}
          className={`${icons.minusClass} absolute h-3.5 w-3.5 opacity-0 transition-opacity duration-150 group-has-indeterminate:opacity-100`}
          strokeWidth={markStyle === "primary" ? 3 : 3.5}
          strokeLinecap="round"
          aria-hidden
        />
      </span>
    </label>
  );
}

export default DataTableCheckbox;
