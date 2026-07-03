import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { ZodErrors } from "../common/statusMessage/status";
import { getLabelWidthClass, requiredMarkClass } from "./fieldStyles";

export type FieldLayout = "horizontal" | "auth" | "toolbar" | "stack" | "default";

interface FieldShellProps {
  name?: string;
  label?: string;
  required?: boolean;
  labelWidth?: string;
  colSpan?: string;
  layout?: FieldLayout;
  error?: any;
  children: ReactNode;
  controlClassName?: string;
  hideLabel?: boolean;
}

/** Standard label + control + validation wrapper for inventory forms. */
export function FieldShell({
  name,
  label,
  required,
  labelWidth,
  colSpan,
  layout = "horizontal",
  error,
  children,
  controlClassName = "w-full min-w-0",
  hideLabel,
}: FieldShellProps) {
  const { t } = useTranslation();
  const mode = layout === "default" ? "horizontal" : layout;
  const showLabel = !hideLabel && Boolean(label?.trim());

  if (mode === "toolbar") {
    return (
      <div className={controlClassName} title={label ? t(label) : undefined}>
        {children}
      </div>
    );
  }

  if (mode === "stack") {
    return (
      <div className="w-full min-w-0 space-y-2">
        {showLabel ? (
          <label htmlFor={name} className="text-sm font-medium text-foreground">
            {t(label!)}
            {required ? <span className="ml-0.5 text-error">*</span> : null}
          </label>
        ) : null}
        <div className={controlClassName}>{children}</div>
        <ZodErrors error={error} />
      </div>
    );
  }

  if (mode === "auth") {
    return (
      <div className="col-span-full w-full space-y-2">
        {showLabel ? (
          <label htmlFor={name} className="text-sm font-medium text-foreground">
            {t(label!)}
            {required ? <span className="ml-0.5 text-error">*</span> : null}
          </label>
        ) : null}
        <div className={controlClassName}>{children}</div>
        <ZodErrors error={error} />
      </div>
    );
  }

  const isFullWidth = colSpan === "full";

  return (
    <div
      className={`w-full gap-1 ${isFullWidth ? "col-span-full" : "md:inline-flex md:gap-3"}`}
    >
      {showLabel ? (
        <label
          htmlFor={name}
          className={`flex shrink-0 items-center justify-end gap-0.5 text-end text-sm font-medium text-foreground max-md:w-full ${
            isFullWidth ? "w-full justify-start text-start" : getLabelWidthClass(labelWidth)
          }`}
        >
          {t(label!)}
          <span className={requiredMarkClass(required)}>*</span>
        </label>
      ) : null}
      <div className={controlClassName}>
        {children}
        <span className="mt-1 block pb-1">
          <ZodErrors error={error} />
        </span>
      </div>
    </div>
  );
}
