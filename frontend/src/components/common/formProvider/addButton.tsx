import { Plus } from "lucide-react";
import { useTranslation } from "react-i18next";

interface AddButtonProps {
  title?: string;
  formId?: string;
  disable?: boolean;
  className?: string;
  onClick?: () => void;
}

export default function AddButton({
  title,
  formId,
  disable,
  className = "",
  onClick,
}: AddButtonProps) {
  const { t } = useTranslation();
  const defaultTitle = title ?? t("Add");

  return (
    <button
      disabled={disable}
      form={formId}
      type="submit"
      onClick={onClick}
      className={`
        flex items-center gap-2 px-5 py-2.5 rounded-lg font-semibold text-sm
        transition-all duration-200 shadow-sm hover:shadow-md
        disabled:opacity-50 disabled:cursor-not-allowed
        bg-primary text-on-accent hover:bg-primary/90
        ${className}
      `}
    >
      <Plus size={16} />
      <span>{defaultTitle}</span>
    </button>
  );
}
