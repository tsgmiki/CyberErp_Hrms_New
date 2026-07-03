import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

export interface FormFieldProps {
  label?: string;
  required?: boolean;
  error?: unknown;
  hint?: string;
  htmlFor?: string;
  children: ReactNode;
  className?: string;
}

function formatError(error: unknown): string | undefined {
  if (!error) return undefined;
  if (typeof error === "string") return error;
  if (Array.isArray(error)) return error.filter(Boolean).join(", ");
  if (typeof error === "object" && error !== null && "_errors" in error) {
    const errs = (error as { _errors?: string[] })._errors;
    return errs?.join(", ");
  }
  return String(error);
}

function FormField({
  label,
  required = false,
  error,
  hint,
  htmlFor,
  children,
  className = "",
}: FormFieldProps) {
  const { t } = useTranslation();
  const errorMessage = formatError(error);

  return (
    <div className={`space-y-1.5 ${className}`}>
      {label ? (
        <label
          htmlFor={htmlFor}
          className="text-sm font-medium text-foreground"
        >
          {t(label)}
          {required ? (
            <span className="ml-0.5 text-error" aria-hidden>
              *
            </span>
          ) : null}
        </label>
      ) : null}
      {children}
      {hint && !errorMessage ? (
        <p className="text-[11px] text-muted">{hint}</p>
      ) : null}
      {errorMessage ? (
        <p className="text-xs text-error" role="alert">
          {errorMessage}
        </p>
      ) : null}
    </div>
  );
}

export default FormField;
