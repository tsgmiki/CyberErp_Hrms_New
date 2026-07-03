import { Check, CloudCheck, Loader2, X } from "lucide-react";
import { useTranslation } from "react-i18next";

export interface ActionBarProps {
  disabled?: boolean;
  disableValidate?: boolean;
  label?: string;
  loading?: boolean;
  className?: string;
  formId?: string;
  showValidate?: boolean;
  showCancel?: boolean;
  validateLabel?: string;
  cancelLabel?: string;
  onValidate?: () => void;
  onCancel?: () => void;
  status?: string;
  validating?: boolean;
}

const btnBase =  "inline-flex h-9 items-center justify-center gap-1.5 rounded-lg border px-3 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50";

export default function ActionBar({
  disabled,
  disableValidate = false,
  label,
  loading,
  className = "",
  formId,
  showValidate = true,
  showCancel = true,
  validateLabel,
  cancelLabel,
  onValidate,
  onCancel,
  status,
  validating = false,
}: ActionBarProps) {
  const { t } = useTranslation();

  const defaultLabel = label ?? t("Save");
  const defaultValidateLabel = validateLabel ?? t("Validate");
  const defaultCancelLabel = cancelLabel ?? t("Cancel");
  const showValidateBtn = status !== "Done" && showValidate && onValidate;

  return (
    <div
      className={`flex flex-wrap items-center justify-end gap-2 ${className}`}
    >
      {showValidateBtn && (
        <button
          disabled={disabled || validating || disableValidate}
          type="button"
          onClick={onValidate}
          className={`${btnBase} border-success/30 bg-success/15 text-success hover:bg-success/25`}
        >
          {validating ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <Check className="h-4 w-4" />
          )}
          <span>{validating ? t("Validating...") : defaultValidateLabel}</span>
        </button>
      )}

      {showCancel && onCancel && (
        <button
          disabled={disabled || loading}
          type="button"
          onClick={onCancel}
          className={`${btnBase} border-border bg-secondary text-foreground hover:bg-muted/50`}
        >
          <X className="h-4 w-4" />
          <span>{defaultCancelLabel}</span>
        </button>
      )}

      <button
        disabled={disabled || loading}
        type="submit"
        form={formId}
        className={`${btnBase} border-transparent bg-primary text-on-accent hover:opacity-90`}
      >
        <CloudCheck className="h-4 w-4" />
        <span>{defaultLabel}</span>
      </button>
    </div>
  );
}
