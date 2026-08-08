import { memo } from "react";
import { Link } from "react-router-dom";
import { FileQuestion } from "lucide-react";
import { useTranslation } from "react-i18next";

/**
 * Shown for an unrecognised URL, or for a record URL whose id isn't a well-formed GUID
 * (see EntityRecordGuard). Deliberately does NOT say whether the record exists — that would leak
 * existence to a user who may not be permitted to see it.
 */
function NotFoundPage() {
  const { t } = useTranslation();
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-3 p-6 text-center">
      <FileQuestion className="h-12 w-12 text-muted" />
      <h1 className="text-lg font-semibold text-foreground">{t("Page not found")}</h1>
      <p className="max-w-md text-sm text-muted">
        {t("The page or record you're looking for doesn't exist, or the link is no longer valid.")}
      </p>
      <Link to="/" className="mt-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90">
        {t("Back to dashboard")}
      </Link>
    </div>
  );
}

export default memo(NotFoundPage);
