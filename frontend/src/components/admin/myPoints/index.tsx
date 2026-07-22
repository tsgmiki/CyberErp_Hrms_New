"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { Coins, ChevronLeft, ChevronRight, Gift } from "lucide-react";
import { getRewardPoints, redeemRewardPoints } from "@/services/admin/rewardPoints";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";
const PAGE_SIZE = 10;

const SOURCE_TONE: Record<string, string> = {
  Recognition: "text-success",
  Redemption: "text-warning",
  Adjustment: "text-muted",
};

/** HC180 — the employee's own reward-points balance, statement and self-service redemption. */
function MyPoints() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [points, setPoints] = useState("");
  const [note, setNote] = useState("");
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState<{ ok: boolean; text: string } | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["myRewardPoints", page],
    queryFn: () => getRewardPoints(undefined, page * PAGE_SIZE, PAGE_SIZE),
    placeholderData: keepPreviousData,
  });

  const balance = data?.balance ?? 0;
  const rows = data?.data ?? [];
  const pages = Math.max(1, Math.ceil((data?.total ?? 0) / PAGE_SIZE));

  const redeem = async () => {
    const n = Number(points);
    if (!Number.isFinite(n) || n <= 0) {
      setMsg({ ok: false, text: t("Enter a positive number of points.") });
      return;
    }
    setBusy(true);
    setMsg(null);
    const res = await redeemRewardPoints(n, note || undefined);
    setBusy(false);
    setMsg({ ok: res.ok, text: res.ok ? `${t("Redeemed")} ${n} ${t("points — new balance")} ${res.balance}` : res.message });
    if (res.ok) {
      setPoints("");
      setNote("");
      setPage(0);
      queryClient.invalidateQueries({ queryKey: ["myRewardPoints"] });
    }
  };

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center gap-2">
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Coins className="h-5 w-5" /></span>
        <div>
          <h1 className="text-base font-semibold text-foreground">{t("My Reward Points")}</h1>
          <p className="text-xs text-muted">{t("Points earned from recognitions — redeem them for perks per company policy.")}</p>
        </div>
      </div>

      {isLoading ? (
        <Loading />
      ) : (
        <div className="min-h-0 flex-1 space-y-4 overflow-auto">
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="rounded-lg border border-border bg-card p-4">
              <p className="text-xs uppercase tracking-wide text-muted">{t("Current Balance")}</p>
              <p className="mt-1 text-3xl font-bold text-primary">{balance}</p>
              <p className="text-xs text-muted">{t("points")}</p>
            </div>
            <div className="rounded-lg border border-border bg-card p-4">
              <h3 className="mb-2 flex items-center gap-1.5 text-sm font-semibold text-foreground"><Gift size={15} className="text-primary" /> {t("Redeem Points")}</h3>
              <div className="flex flex-wrap items-end gap-2">
                <div className="w-28">
                  <label className={LABEL}>{t("Points")}</label>
                  <input type="number" min={1} className={INPUT} value={points} onChange={(e) => setPoints(e.target.value)} />
                </div>
                <div className="min-w-[160px] flex-1">
                  <label className={LABEL}>{t("Note")}</label>
                  <input type="text" className={INPUT} placeholder={t("e.g. Cafeteria voucher")} value={note} onChange={(e) => setNote(e.target.value)} />
                </div>
                <button
                  type="button"
                  disabled={busy || !points}
                  onClick={redeem}
                  className="rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
                >
                  {busy ? t("Redeeming…") : t("Redeem")}
                </button>
              </div>
              {msg && <p className={`mt-2 text-xs ${msg.ok ? "text-success" : "text-error"}`}>{msg.text}</p>}
            </div>
          </div>

          <div className="rounded-lg border border-border bg-card">
            <p className="border-b border-border px-4 py-2 text-xs font-semibold uppercase tracking-wide text-muted">{t("Statement")}</p>
            {rows.length === 0 ? (
              <p className="p-6 text-center text-sm text-muted">{t("No points activity yet.")}</p>
            ) : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-left text-xs text-muted">
                    <th className="px-4 py-2 font-medium">{t("Date")}</th>
                    <th className="px-4 py-2 font-medium">{t("Source")}</th>
                    <th className="px-4 py-2 font-medium">{t("Note")}</th>
                    <th className="px-4 py-2 text-right font-medium">{t("Points")}</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((r) => (
                    <tr key={r.id} className="border-t border-border/60">
                      <td className="px-4 py-2 text-xs">{(r.transactionDate || "").slice(0, 10)}</td>
                      <td className={`px-4 py-2 text-xs font-medium ${SOURCE_TONE[r.source ?? ""] ?? ""}`}>{r.source}</td>
                      <td className="px-4 py-2 text-xs text-muted">{r.note || "—"}</td>
                      <td className={`px-4 py-2 text-right font-semibold ${Number(r.points) >= 0 ? "text-success" : "text-warning"}`}>
                        {Number(r.points) >= 0 ? `+${r.points}` : r.points}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
            {pages > 1 && (
              <div className="flex items-center justify-end gap-2 border-t border-border px-4 py-2 text-xs text-muted">
                <span>{t("Page")} {page + 1} / {pages}</span>
                <button type="button" disabled={page === 0} onClick={() => setPage((p) => p - 1)} className="rounded border border-border p-1 disabled:opacity-40"><ChevronLeft size={14} /></button>
                <button type="button" disabled={page + 1 >= pages} onClick={() => setPage((p) => p + 1)} className="rounded border border-border p-1 disabled:opacity-40"><ChevronRight size={14} /></button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default memo(MyPoints);
