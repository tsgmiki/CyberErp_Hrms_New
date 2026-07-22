"use client";
import { memo, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useReactToPrint } from "react-to-print";
import { Calculator, Plus, Trash2, Save, BadgeCheck, Banknote, Printer, X, FileText } from "lucide-react";
import {
  getSettlement, buildSettlement, updateSettlementLines, approveSettlement, markSettlementPaid,
  generateSettlementLetter,
} from "@/services/admin/employee/exitManagement";
import getAllDocumentTemplates from "@/services/admin/documentTemplate/getAll";
import type { SettlementLineModel, GeneratedDocumentModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none";
const money = (v?: number) => Number(v ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 });

const PRINT_CSS = `
  @page { margin: 18mm; }
  @media print {
    * { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
    .doc-header > div { display: flex !important; }
  }
`;

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-warning/15 text-warning",
  Approved: "bg-info/15 text-info",
  Paid: "bg-success/15 text-success",
};

/** HC216/HC217/HC218 — the case's final-settlement worksheet + letter print. */
function ExitSettlementPanel({ terminationId }: { terminationId: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [draftLines, setDraftLines] = useState<SettlementLineModel[]>([]);
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");
  const [payRef, setPayRef] = useState("");
  const [showLetter, setShowLetter] = useState(false);
  const [templateId, setTemplateId] = useState("");
  const [generated, setGenerated] = useState<GeneratedDocumentModel | null>(null);
  const contentRef = useRef<HTMLDivElement>(null);

  const { data: settlement } = useQuery({
    queryKey: ["settlement", terminationId],
    queryFn: () => getSettlement(terminationId),
    retry: false,
  });

  useEffect(() => {
    if (settlement?.lines) setDraftLines(settlement.lines);
  }, [settlement]);

  const [tplParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: templates } = useQuery({
    queryKey: ["documentTemplates", tplParam],
    queryFn: () => getAllDocumentTemplates(tplParam),
    enabled: showLetter,
  });
  const tplOptions = useMemo(
    () => (templates?.data ?? []).filter((tp) => tp.documentType === "SettlementLetter" || tp.documentType === "Other"),
    [templates],
  );

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["settlement", terminationId] });
  };

  const run = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setBusy(true);
    const res = await fn();
    setBusy(false);
    refresh(res.message);
  };

  const isDraft = settlement?.status === "Draft";
  const editedEarnings = draftLines.filter((l) => l.kind !== "Deduction").reduce((s, l) => s + Number(l.amount || 0), 0);
  const editedDeductions = draftLines.filter((l) => l.kind === "Deduction").reduce((s, l) => s + Number(l.amount || 0), 0);

  const generate = async () => {
    if (!templateId || !settlement?.id) return;
    setBusy(true);
    try {
      setGenerated(await generateSettlementLetter(templateId, settlement.id));
    } catch (e) {
      setMsg(e instanceof Error ? e.message : "Failed to generate the letter.");
    }
    setBusy(false);
  };
  const handlePrint = useReactToPrint({ contentRef, pageStyle: PRINT_CSS, documentTitle: generated?.title ?? "Settlement Letter" });

  return (
    <div className="border-t border-border">
      <div className="flex items-center justify-between px-4 py-2">
        <h4 className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wide text-muted">
          <Calculator size={13} /> {t("Final Settlement")}
        </h4>
        {settlement && (
          <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[settlement.status ?? ""] ?? ""}`}>
            {settlement.status}{settlement.paidReference ? ` · ${settlement.paidReference}` : ""}
          </span>
        )}
      </div>
      {msg && <p className="mx-4 mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-1.5 text-xs text-muted">{msg}</p>}

      <div className="px-4 pb-3">
        {!settlement ? (
          <button type="button" disabled={busy} onClick={() => run(() => buildSettlement(terminationId))}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-semibold text-foreground hover:bg-secondary/40 disabled:opacity-50">
            <Calculator size={13} /> {busy ? t("Building…") : t("Build Worksheet (auto-suggests leave payout & severance)")}
          </button>
        ) : (
          <>
            <table className="w-full text-[13px]">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                  <th className="py-1.5 pr-2 font-semibold">{t("Kind")}</th>
                  <th className="py-1.5 pr-2 font-semibold">{t("Item")}</th>
                  <th className="py-1.5 pr-2 text-right font-semibold">{t("Amount")}</th>
                  {isDraft && <th className="py-1.5 font-semibold" />}
                </tr>
              </thead>
              <tbody>
                {(isDraft ? draftLines : settlement.lines ?? []).map((l, i) => (
                  <tr key={l.id ?? i} className="border-b border-border/60">
                    <td className="py-1.5 pr-2">
                      {isDraft ? (
                        <select className={INPUT} value={l.kind ?? "Earning"}
                          onChange={(e) => setDraftLines((p) => p.map((x, j) => (j === i ? { ...x, kind: e.target.value } : x)))}>
                          <option value="Earning">{t("Earning")}</option>
                          <option value="Deduction">{t("Deduction")}</option>
                        </select>
                      ) : (
                        <span className={l.kind === "Deduction" ? "text-error" : "text-success"}>{l.kind}</span>
                      )}
                    </td>
                    <td className="py-1.5 pr-2">
                      {isDraft ? (
                        <input type="text" className={INPUT} value={l.label ?? ""}
                          onChange={(e) => setDraftLines((p) => p.map((x, j) => (j === i ? { ...x, label: e.target.value } : x)))} />
                      ) : (
                        <span>{l.label}{l.isAutoSuggested ? <em className="ml-1 text-[10px] text-muted">({t("suggested")})</em> : null}</span>
                      )}
                    </td>
                    <td className="py-1.5 pr-2 text-right">
                      {isDraft ? (
                        <input type="number" min={0} className={`${INPUT} text-right`} value={l.amount ?? 0}
                          onChange={(e) => setDraftLines((p) => p.map((x, j) => (j === i ? { ...x, amount: Number(e.target.value) } : x)))} />
                      ) : (
                        <span className={l.kind === "Deduction" ? "text-error" : ""}>{l.kind === "Deduction" ? "−" : ""}{money(l.amount)}</span>
                      )}
                    </td>
                    {isDraft && (
                      <td className="py-1.5 text-right">
                        <button type="button" onClick={() => setDraftLines((p) => p.filter((_x, j) => j !== i))} className="rounded p-1 text-muted hover:text-error">
                          <Trash2 size={13} />
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="mt-2 flex flex-wrap items-center justify-between gap-2">
              <p className="text-xs text-muted">
                {t("Earnings")} <strong className="text-foreground">{money(isDraft ? editedEarnings : settlement.totalEarnings)}</strong>
                {" · "}{t("Deductions")} <strong className="text-foreground">{money(isDraft ? editedDeductions : settlement.totalDeductions)}</strong>
                {" · "}{t("Net")} <strong className="text-primary">{money(isDraft ? editedEarnings - editedDeductions : settlement.netAmount)}</strong>
              </p>
              <span className="flex flex-wrap items-center gap-1.5">
                {isDraft && (
                  <>
                    <button type="button" onClick={() => setDraftLines((p) => [...p, { kind: "Earning", label: "", amount: 0 }])}
                      className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-foreground hover:bg-secondary/40">
                      <Plus size={12} /> {t("Line")}
                    </button>
                    <button type="button" disabled={busy || draftLines.some((l) => !l.label?.trim())}
                      onClick={() => settlement.id && run(() => updateSettlementLines(settlement.id!, draftLines, settlement.notes))}
                      className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-foreground hover:text-success disabled:opacity-50">
                      <Save size={12} /> {t("Save")}
                    </button>
                    <button type="button" disabled={busy}
                      onClick={() => settlement.id && run(() => approveSettlement(settlement.id!))}
                      className="inline-flex items-center gap-1 rounded-md bg-success px-2.5 py-1 text-xs font-semibold text-on-accent disabled:opacity-50">
                      <BadgeCheck size={12} /> {t("Approve & Lock")}
                    </button>
                  </>
                )}
                {settlement.status === "Approved" && (
                  <>
                    <input type="text" className="w-40 rounded-md border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none"
                      placeholder={t("Payment reference")} value={payRef} onChange={(e) => setPayRef(e.target.value)} />
                    <button type="button" disabled={busy}
                      onClick={() => settlement.id && run(() => markSettlementPaid(settlement.id!, payRef || undefined))}
                      className="inline-flex items-center gap-1 rounded-md bg-primary px-2.5 py-1 text-xs font-semibold text-on-accent disabled:opacity-50">
                      <Banknote size={12} /> {t("Mark Paid")}
                    </button>
                  </>
                )}
                <button type="button" onClick={() => setShowLetter(true)}
                  className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-foreground hover:bg-secondary/40">
                  <FileText size={12} /> {t("Letter")}
                </button>
              </span>
            </div>
          </>
        )}
      </div>

      {showLetter && settlement?.id && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="flex max-h-[90vh] w-full max-w-3xl flex-col rounded-lg border border-border bg-background shadow-xl">
            <div className="flex items-center justify-between border-b border-border px-4 py-3">
              <h3 className="flex items-center gap-2 text-sm font-semibold text-foreground"><FileText size={16} /> {t("Settlement Letter")}</h3>
              <button type="button" onClick={() => { setShowLetter(false); setGenerated(null); }} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
            </div>
            <div className="flex flex-wrap items-end gap-2 border-b border-border px-4 py-3">
              <div className="min-w-[220px] flex-1">
                <label className="mb-1 block text-xs font-medium text-muted">{t("Template")}</label>
                <select className="w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none"
                  value={templateId} onChange={(e) => setTemplateId(e.target.value)}>
                  <option value="">{t("Select a template")}</option>
                  {tplOptions.map((tp) => <option key={tp.id} value={tp.id}>{tp.name}</option>)}
                </select>
              </div>
              <button type="button" disabled={busy || !templateId} onClick={generate}
                className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                <FileText size={14} /> {busy ? t("Generating…") : t("Generate")}
              </button>
              {generated && (
                <button type="button" onClick={() => handlePrint()}
                  className="inline-flex items-center gap-1.5 rounded-md border border-border px-3.5 py-2 text-sm font-semibold text-foreground hover:bg-secondary/40">
                  <Printer size={14} /> {t("Print")}
                </button>
              )}
            </div>
            <div className="min-h-0 flex-1 overflow-auto p-4">
              {!generated ? (
                <p className="py-10 text-center text-sm text-muted">{t("Choose a template and generate the settlement letter.")}</p>
              ) : (
                <div className="rounded-md border border-border bg-white p-6 text-black">
                  <div ref={contentRef} className="doc-print-area">
                    <div dangerouslySetInnerHTML={{ __html: generated.html }} />
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default memo(ExitSettlementPanel);
