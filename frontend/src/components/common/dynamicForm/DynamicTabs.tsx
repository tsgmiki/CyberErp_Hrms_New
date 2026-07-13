"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { LayoutGrid } from "lucide-react";
import DynamicFormSection from "./DynamicFormSection";
import { useDynamicForms } from "./useDynamicForms";

/**
 * Self-contained dynamic-tab bar for a module: fetches the module's active custom forms and renders a
 * tab per form (each hosting a DynamicFormSection). Module-agnostic — drop it into any entity profile
 * as `<DynamicTabs module="X" ownerType="X" ownerId={id} />`. (The Employee profile instead folds these
 * tabs into its existing hardcoded tab bar via `useDynamicForms` directly.)
 */
function DynamicTabs({
  module,
  ownerType,
  ownerId,
}: {
  module: string;
  ownerType: string;
  ownerId: string;
}) {
  const { t } = useTranslation();
  const { data: forms } = useDynamicForms(module);
  const [active, setActive] = useState(0);

  if (!forms || forms.length === 0) return null;
  const current = forms[Math.min(active, forms.length - 1)];

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-1 border-b border-border">
        {forms.map((f, i) => (
          <button
            key={f.id}
            type="button"
            onClick={() => setActive(i)}
            className={`flex items-center gap-1.5 rounded-t-md border border-b-0 px-3 py-1.5 text-sm ${
              i === active ? "border-border bg-card text-primary" : "border-transparent text-muted hover:text-foreground"
            }`}
          >
            <LayoutGrid className="h-4 w-4" />
            {t(f.label ?? f.name ?? "")}
          </button>
        ))}
      </div>
      {current && <DynamicFormSection form={current} ownerType={ownerType} ownerId={ownerId} />}
    </div>
  );
}

export default memo(DynamicTabs);
