import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

export interface FormSectionProps {
  title: string;
  description?: string;
  children?: ReactNode;
}

function FormSection({ title, description, children }: FormSectionProps) {
  const { t } = useTranslation();

  return (
    <div className="col-span-full space-y-4">
      <div className="border-b border-border pb-2">
        <h3 className="text-sm font-semibold text-foreground">{t(title)}</h3>
        {description ? (
          <p className="mt-0.5 text-xs text-muted">{t(description)}</p>
        ) : null}
      </div>
      {children ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">{children}</div>
      ) : null}
    </div>
  );
}

export default FormSection;
