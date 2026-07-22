"use client";
import { memo, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { useReactToPrint } from "react-to-print";
import { Printer, FileText, X } from "lucide-react";
import getAllDocumentTemplates from "@/services/admin/documentTemplate/getAll";
import type { GeneratedDocumentModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// Same print treatment as the other document generators.
const PRINT_CSS = `
  @page { margin: 18mm; }
  @media print {
    * { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
    .doc-header > div { display: flex !important; }
  }
`;

/** Generate + print an exit letter (HC211) — resignation acceptance / termination notice. */
function ExitLetterModal({ terminationId, employeeName, onClose }: {
  terminationId: string;
  employeeName?: string;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const contentRef = useRef<HTMLDivElement>(null);
  const [templateId, setTemplateId] = useState("");
  const [generated, setGenerated] = useState<GeneratedDocumentModel | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const [tplParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: templates } = useQuery({ queryKey: ["documentTemplates", tplParam], queryFn: () => getAllDocumentTemplates(tplParam) });
  const options = useMemo(
    () => (templates?.data ?? []).filter((tp) => tp.documentType === "TerminationNotice" || tp.documentType === "Other"),
    [templates],
  );

  const generate = async () => {
    if (!templateId) return;
    setBusy(true);
    setError("");
    try {
      const res = await fetch(`${API_BASE_URL}/DocumentTemplate/${templateId}/generate-termination/${terminationId}`, {
        credentials: "include",
      });
      if (!res.ok) throw new Error(await res.text());
      setGenerated((await res.json()) as GeneratedDocumentModel);
    } catch (e) {
      setError(e instanceof Error && e.message ? e.message : "Failed to generate the letter.");
    }
    setBusy(false);
  };

  const handlePrint = useReactToPrint({ contentRef, pageStyle: PRINT_CSS, documentTitle: generated?.title ?? "Exit Letter" });

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="flex max-h-[90vh] w-full max-w-3xl flex-col rounded-lg border border-border bg-background shadow-xl">
        <div className="flex items-center justify-between border-b border-border px-4 py-3">
          <h3 className="flex items-center gap-2 text-sm font-semibold text-foreground">
            <FileText size={16} /> {t("Exit Letter")}{employeeName ? ` — ${employeeName}` : ""}
          </h3>
          <button type="button" onClick={onClose} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
        </div>

        <div className="flex flex-wrap items-end gap-2 border-b border-border px-4 py-3">
          <div className="min-w-[220px] flex-1">
            <label className="mb-1 block text-xs font-medium text-muted">{t("Template")}</label>
            <select className="w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none"
              value={templateId} onChange={(e) => setTemplateId(e.target.value)}>
              <option value="">{t("Select a template")}</option>
              {options.map((tp) => (
                <option key={tp.id} value={tp.id}>{tp.name}</option>
              ))}
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
          {error && <p className="mb-2 text-xs text-error">{error}</p>}
          {!generated ? (
            <p className="py-10 text-center text-sm text-muted">
              {options.length === 0
                ? t("No exit-letter template found — seed the defaults under Document Templates.")
                : t("Choose a template and generate the letter.")}
            </p>
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
  );
}

export default memo(ExitLetterModal);
