import { memo } from "react";
import { useTranslation } from "react-i18next";
import { LayoutDashboard } from "lucide-react";

function Dashboard() {
  const { t } = useTranslation();

  return (
    <div className="mx-auto max-w-350 space-y-6 p-4 md:p-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold tracking-tight text-[var(--text)]">
            <LayoutDashboard className="h-7 w-7 text-primary" />
            {t("Dashboard", "Dashboard")}
          </h1>
          <p className="mt-1 text-sm text-muted">
            {t("Welcome", "Welcome")}
          </p>
        </div>
      </div>

      <div className="rounded-xl border border-border bg-card p-8 text-center">
        <p className="text-sm text-muted">
          {t("NoDashboardContent", "No dashboard content available yet.")}
        </p>
      </div>
    </div>
  );
}

export default memo(Dashboard);
