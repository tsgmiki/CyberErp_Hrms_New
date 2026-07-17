import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

/**
 * Reusable floating-label wrapper (Material-style): the label sits over the control in the placeholder
 * position and floats above it when `active` (the field is focused or has a value). Pair it with any
 * single-line control that can compute `active` (e.g. a native <select>). Text/number/textarea inputs
 * instead use the CSS `peer` / `:placeholder-shown` technique inside their own components.
 */
export function FloatingLabel({ htmlFor, label, active, required, children }: {
  htmlFor?: string;
  label: string;
  active: boolean;
  required?: boolean;
  children: ReactNode;
}) {
  const { t } = useTranslation();
  return (
    <div className="relative w-full">
      {children}
      <label
        htmlFor={htmlFor}
        className={`pointer-events-none absolute left-2.5 z-10 origin-[0] bg-background px-1 transition-all duration-150 ${
          active
            ? "top-2 -translate-y-4 scale-90 text-primary"
            : "top-1/2 -translate-y-1/2 scale-100 text-sm text-muted"
        }`}
      >
        {t(label)}
        {required ? <span className="text-error"> *</span> : null}
      </label>
    </div>
  );
}

export default FloatingLabel;
