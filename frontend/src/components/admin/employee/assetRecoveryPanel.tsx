"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { PackageCheck, PackageX } from "lucide-react";
import { getAssetRecoveries, resolveAssetRecovery } from "@/services/admin/companyAsset";
import type { AssetRecoveryModel } from "@/models";
import { assetCategoryOptions } from "@/constants/orgStructure";

const TONE: Record<string, string> = {
  Outstanding: "bg-warning/15 text-warning",
  Recovered: "bg-success/15 text-success",
  Waived: "bg-muted/30 text-muted",
};

const categoryLabel = (id?: string) => assetCategoryOptions.find((o) => o.id === id)?.name ?? id ?? "—";

function RecoveryRow({ item, onChanged }: { item: AssetRecoveryModel; onChanged: (msg: string) => void }) {
  const { t } = useTranslation();
  const [note, setNote] = useState("");
  const [busy, setBusy] = useState(false);

  const resolve = async (action: "Recover" | "Waive") => {
    if (!item.id) return;
    setBusy(true);
    const res = await resolveAssetRecovery(item.id, action, note || undefined);
    setBusy(false);
    onChanged(res.message);
  };

  return (
    <tr className="border-b border-border/60">
      <td className="px-4 py-2">
        <span className="block font-medium">{item.assetName}</span>
        <span className="block text-xs text-muted">{categoryLabel(item.category)}{item.serialNo ? ` · ${item.serialNo}` : ""}</span>
      </td>
      <td className="px-4 py-2">
        <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${TONE[item.status ?? ""] ?? ""}`}>{item.status}</span>
      </td>
      <td className="px-4 py-2 text-xs text-muted">
        {item.status === "Outstanding" ? (
          <input
            type="text"
            className="w-full rounded-md border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none"
            placeholder={t("Note (optional)")}
            value={note}
            onChange={(e) => setNote(e.target.value)}
          />
        ) : (
          <>{item.note || "—"}{item.resolvedOn ? ` · ${String(item.resolvedOn).slice(0, 10)}` : ""}</>
        )}
      </td>
      <td className="px-4 py-2">
        {item.status === "Outstanding" && (
          <span className="flex items-center gap-1.5">
            <button type="button" disabled={busy} title={t("Recovered — back to the pool")} onClick={() => resolve("Recover")}
              className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-foreground hover:text-success disabled:opacity-50">
              <PackageCheck size={13} /> {t("Recover")}
            </button>
            <button type="button" disabled={busy} title={t("Write off — asset retired")} onClick={() => resolve("Waive")}
              className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-muted hover:text-error disabled:opacity-50">
              <PackageX size={13} /> {t("Waive")}
            </button>
          </span>
        )}
      </td>
    </tr>
  );
}

/** HC214/HC215 — the exit case's asset-recovery checklist; settlement stays blocked while items are outstanding. */
function AssetRecoveryPanel({ terminationId }: { terminationId: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [msg, setMsg] = useState("");

  const { data: items } = useQuery({
    queryKey: ["assetRecoveries", terminationId],
    queryFn: () => getAssetRecoveries(terminationId),
  });

  const rows = items ?? [];
  if (rows.length === 0) return null;

  const outstanding = rows.filter((r) => r.status === "Outstanding").length;
  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["assetRecoveries", terminationId] });
  };

  return (
    <div className="border-t border-border">
      <div className="flex items-center justify-between px-4 py-2">
        <h4 className="text-xs font-bold uppercase tracking-wide text-muted">{t("Asset Recovery")}</h4>
        <span className={`text-xs ${outstanding > 0 ? "font-semibold text-warning" : "text-muted"}`}>
          {rows.length - outstanding}/{rows.length} {t("resolved")}
        </span>
      </div>
      <div className="px-4 pb-1 text-[11px] text-muted">
        {t("Company property assigned to the employee. Settlement stays blocked until every item is recovered or waived.")}
      </div>
      {msg && <p className="mx-4 mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-1.5 text-xs text-muted">{msg}</p>}
      <div className="overflow-x-auto">
        <table className="w-full text-[13px]">
          <thead>
            <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
              <th className="px-4 py-2 font-semibold">{t("Asset")}</th>
              <th className="px-4 py-2 font-semibold">{t("Status")}</th>
              <th className="px-4 py-2 font-semibold">{t("Note")}</th>
              <th className="px-4 py-2 font-semibold">{t("Action")}</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => <RecoveryRow key={r.id} item={r} onChanged={refresh} />)}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default memo(AssetRecoveryPanel);
