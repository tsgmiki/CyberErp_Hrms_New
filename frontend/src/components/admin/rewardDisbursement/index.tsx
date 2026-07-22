"use client";
import { memo, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Banknote, Download, CheckCircle2, X } from "lucide-react";
import { getAllDisbursements, markDisbursementPaid, downloadDisbursementCsv } from "@/services/admin/rewardDisbursement";
import type { RewardDisbursementModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";

const STATUS_TONE: Record<string, string> = {
  Pending: "bg-warning/15 text-warning",
  Paid: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

/** HC185 — payroll/finance hand-off: monetary rewards awaiting payment (no payroll module exists). */
function RewardDisbursement() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [payFor, setPayFor] = useState<RewardDisbursementModel | null>(null);
  const [reference, setReference] = useState("");
  const [busy, setBusy] = useState(false);
  const [actionMsg, setActionMsg] = useState("");

  const list = useEntityList({
    queryKey: "rewardDisbursements",
    fetchPage: getAllDisbursements,
  });

  const confirmPay = async () => {
    if (!payFor?.id) return;
    setBusy(true);
    const res = await markDisbursementPaid(payFor.id, reference || undefined);
    setBusy(false);
    setActionMsg(res.message);
    if (res.ok) {
      setPayFor(null);
      setReference("");
      queryClient.invalidateQueries({ queryKey: ["rewardDisbursements"] });
    }
  };

  const exportCsv = async () => {
    try {
      await downloadDisbursementCsv();
    } catch {
      setActionMsg(t("Export failed"));
    }
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          render: (text: string, record: RewardDisbursementModel) => (
            <span>
              <span className="block font-semibold">{text || "—"}</span>
              <span className="block text-xs text-muted">{record.employeeNumber}</span>
            </span>
          ),
        },
        { name: "badgeName", label: "Award", render: (v: string) => v || "—" },
        {
          name: "amount",
          label: "Amount",
          sort: true,
          render: (v: number) => <span className="font-semibold">{Number(v ?? 0).toLocaleString()}</span>,
        },
        { name: "grantedOn", label: "Granted", render: fmtDate },
        {
          name: "status",
          label: "Status",
          render: (v: string) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-secondary/40 text-foreground"}`}>{v}</span>
          ),
        },
        { name: "paidAt", label: "Paid At", render: fmtDate },
        { name: "reference", label: "Reference", render: (v: string) => v || "—" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: RewardDisbursementModel) =>
            record.status === "Pending" ? (
              <button
                type="button"
                onClick={() => { setPayFor(record); setReference(""); }}
                className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-foreground hover:bg-secondary/40"
              >
                <CheckCircle2 size={13} className="text-success" /> {t("Mark Paid")}
              </button>
            ) : null,
        },
      ] as DataTableColumnModel[],
    [t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Banknote className="h-5 w-5" /></span>
          <div>
            <h1 className="text-base font-semibold text-foreground">{t("Reward Payouts")}</h1>
            <p className="text-xs text-muted">{t("Monetary rewards handed off to payroll/finance — mark rows paid or export the batch.")}</p>
          </div>
        </div>
        <button
          type="button"
          onClick={exportCsv}
          className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-sm font-semibold text-foreground hover:bg-secondary/40"
        >
          <Download size={14} /> {t("Export CSV")}
        </button>
      </div>

      {actionMsg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{actionMsg}</p>}

      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell listKey="rewardDisbursements" listLabel="Reward Payouts" columns={columns} {...list} />
      </div>

      {payFor && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-sm rounded-lg border border-border bg-background p-4 shadow-xl">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-foreground">{t("Mark disbursement as paid")}</h3>
              <button type="button" onClick={() => setPayFor(null)} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
            </div>
            <p className="mb-3 text-xs text-muted">
              {payFor.employeeName} — {payFor.badgeName} · <span className="font-semibold text-foreground">{Number(payFor.amount ?? 0).toLocaleString()}</span>
            </p>
            <label className="mb-1 block text-xs font-medium text-muted">{t("Payment reference (cheque / transfer no.)")}</label>
            <input type="text" className={INPUT} value={reference} onChange={(e) => setReference(e.target.value)} placeholder={t("Optional")} />
            <div className="mt-4 flex justify-end gap-2">
              <button type="button" onClick={() => setPayFor(null)} className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary/40">{t("Cancel")}</button>
              <button
                type="button"
                disabled={busy}
                onClick={confirmPay}
                className="rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
              >
                {busy ? t("Saving…") : t("Confirm Paid")}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default memo(RewardDisbursement);
