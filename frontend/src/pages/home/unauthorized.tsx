import { memo } from "react";
import { Link } from "react-router-dom";
import { ShieldX } from "lucide-react";
import { useTranslation } from "react-i18next";

/** Shown when a user opens a page their role isn't permitted to view (see PermissionGate). */
function UnauthorizedPage() {
  const { t } = useTranslation();
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-3 p-6 text-center">
      <ShieldX className="h-12 w-12 text-error" />
      <h1 className="text-lg font-semibold text-foreground">{t("Access denied")}</h1>
      <p className="max-w-md text-sm text-muted">
        {t("You don't have permission to view this page. If you believe this is a mistake, contact your administrator.")}
      </p>
      <Link to="/" className="mt-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90">
        {t("Back to dashboard")}
      </Link>
    </div>
  );
}

export default memo(UnauthorizedPage);
