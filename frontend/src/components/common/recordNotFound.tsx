import { memo } from "react";
import { FileQuestion } from "lucide-react";
import { useTranslation } from "react-i18next";

/**
 * Inline "this record is gone" panel for an entity form opened by URL.
 *
 * Rendered INSTEAD of the form when a by-id fetch resolves to nothing — the record was deleted, or
 * the link is stale. This is not cosmetic: `createEntityGetById` swallows the error and returns
 * undefined, so without this the form renders with empty fields and an empty hidden `id`, and
 * `createSaveService` reads that as "no id" → POST → a DUPLICATE record instead of a failed update.
 * (The hidden `id` now also falls back to the route id, so both halves of that hole are closed.)
 */
function RecordNotFound({ onBack }: { onBack?: () => void }) {
  const { t } = useTranslation();
  return (
    <div className="flex min-h-[40vh] flex-col items-center justify-center gap-3 p-6 text-center">
      <FileQuestion className="h-10 w-10 text-muted" />
      <h2 className="text-base font-semibold text-foreground">{t("Record not found")}</h2>
      <p className="max-w-md text-sm text-muted">
        {t("This record no longer exists or you don't have access to it. It may have been deleted.")}
      </p>
      {onBack && (
        <button
          type="button"
          onClick={onBack}
          className="mt-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90"
        >
          {t("Back to list")}
        </button>
      )}
    </div>
  );
}

export default memo(RecordNotFound);
