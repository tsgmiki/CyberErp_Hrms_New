import { Lock } from "lucide-react";
import { useTranslation } from "react-i18next";

export interface SubmitButtonProps {
  disabled?: boolean;
  label?: string;
  loading?: boolean;
  showSaveAndNew?: boolean;
  showSaveAndAdd?: boolean;
  onSaveAndNew?: () => void;
  onSaveAndAdd?: () => void;
  className?: string;
  formId?: string;
  showLockIcon?: boolean;
}

export default function SubmitButton({
  disabled,
  label,
  loading,
  formId,
  className = "",
  showLockIcon = false,
}: SubmitButtonProps) {
  const { t } = useTranslation();

  const defaultLabel = label ?? t("Save");
  const savingText = t("Saving...");

  return (
    <button
      disabled={disabled || loading}
      type="submit"
      form={formId}
      className={`
        max-md:w-full px-2 h-9 flex items-center justify-center gap-3 rounded-xl font-semibold text-base transition-all duration-300
       
        disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:transform-none disabled:hover:shadow-none
        ${className}  bg-primary   
      `}
    >
      {loading ? (
        <>
          <svg
            className="animate-spin h-5 w-5"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
          <span>{savingText}</span>
        </>
      ) : (
        <>
          {showLockIcon ? (
            <Lock className="w-4 h-4" />
          ) : (
            <svg
              className="h-5 w-5"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2.5}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4"
              />
            </svg>
          )}
          <span>{defaultLabel}</span>
        </>
      )}
    </button>
  );
}
