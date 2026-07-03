import type { FormComponentModel } from "@/models";
import { useTranslation } from "react-i18next";

const BreakField = ({ label, description }: FormComponentModel & { description?: string }) => {
  const { t } = useTranslation();

  if (!label?.trim()) {
    return <hr className="col-span-full border-border" />;
  }

  return (
    <div className="col-span-full space-y-3 pt-1">
      <div className="border-b border-border pb-2">
        <h3 className="text-sm font-semibold text-foreground">{t(label)}</h3>
        {description ? (
          <p className="mt-0.5 text-xs text-muted">{t(description)}</p>
        ) : null}
      </div>
    </div>
  );
};

export default BreakField;
