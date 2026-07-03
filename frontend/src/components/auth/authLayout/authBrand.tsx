import { Building2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import BrandTitle from "@/components/common/brand/brandTitle";

function AuthBrand() {
  const { t } = useTranslation();

  return (
    <div className="text-center mb-10">
      <div className="inline-flex items-center gap-3 mb-4">
        <div className="w-12 h-12 rounded-xl bg-primary flex items-center justify-center shadow-lg">
          <Building2 className="w-6 h-6 text-primary-foreground" />
        </div>
        <h1>
          <BrandTitle size="md" />
        </h1>
      </div>
      <p className="text-muted-foreground text-sm">
        {t("AuthSubtitle", { defaultValue: "Enterprise Resource Planning System" })}
      </p>
    </div>
  );
}

export default AuthBrand;
