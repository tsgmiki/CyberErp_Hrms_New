/** Shared form control tokens for `src/components/ui` field components. */

export const FORM_INPUT_CLASS =
  "h-10 w-full rounded-lg border border-border bg-background px-3 text-sm text-foreground placeholder:text-muted transition-colors input-focus focus:ring-2 focus:ring-primary/20 disabled:cursor-not-allowed disabled:opacity-60";

export const FORM_TEXTAREA_CLASS =
  "min-h-[88px] w-full resize-y rounded-lg border border-border bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted transition-colors input-focus focus:ring-2 focus:ring-primary/20 disabled:cursor-not-allowed disabled:opacity-60";

export const FORM_SELECT_CLASS = `${FORM_INPUT_CLASS} appearance-none`;

/** Compact controls for list toolbar / search-bar filters. */
export const FORM_COMPACT_INPUT_CLASS =
  "h-9 min-w-[7.5rem] max-w-[12rem] rounded-lg border border-border bg-background px-2 text-xs text-foreground placeholder:text-muted transition-colors input-focus focus:ring-2 focus:ring-primary/20 disabled:cursor-not-allowed disabled:opacity-60 sm:text-sm";

export const FORM_COMPACT_SELECT_CLASS = `${FORM_COMPACT_INPUT_CLASS} appearance-none`;

export const LIST_FILTER_CONTROL_CLASS = "w-auto min-w-[7.5rem] max-w-[12rem]";

export const FORM_CHECKBOX_CLASS =
  "size-4 shrink-0 rounded border border-border text-primary accent-primary focus:ring-2 focus:ring-primary/20";

export const FORM_RADIO_CLASS =
  "size-4 shrink-0 border border-border accent-primary focus:ring-2 focus:ring-primary/50";

export const FORM_FILE_CLASS =
  "w-full cursor-pointer rounded-lg border border-dashed border-border bg-muted/20 px-3 py-4 text-sm text-foreground file:mr-3 file:rounded-md file:border-0 file:bg-primary file:px-3 file:py-1.5 file:text-sm file:font-medium file:text-on-accent hover:file:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60";

export const FORM_OPTION_GROUP_CLASS = "flex flex-wrap gap-3";

export const FORM_OPTION_LABEL_CLASS =
  "inline-flex cursor-pointer items-center gap-2 text-sm text-foreground";

export const DEFAULT_LABEL_WIDTH = "w-[20%]";

export function getLabelWidthClass(labelWidth?: string): string {
  return labelWidth ?? DEFAULT_LABEL_WIDTH;
}

export function requiredMarkClass(required?: boolean): string {
  return required
    ? "text-error text-lg leading-none"
    : "text-transparent text-lg leading-none select-none";
}
