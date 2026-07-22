"use client";

import { useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useReactToPrint } from "react-to-print";
import { Printer, FileText } from "lucide-react";
import { useTranslation } from "react-i18next";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import getAllDocumentTemplate from "@/services/admin/documentTemplate/getAll";
import generateAnnualLeaveDocument from "@/services/admin/annualLeave/generateDocument";
import type { DocumentTemplateModel } from "@/models";
import type ParameterModel from "@/models/ParameterModel";

interface Props {
  annualLeaveId: string;
  label?: string;
  onClose: () => void;
}

// Same print stylesheet as the employee document flow: page margins, preserved colours, and inline
// flexbox forced to stay on one row (some print engines otherwise collapse flex to a vertical stack).
const PRINT_STYLE = `
  @page { margin: 16mm; }
  @media print {
    * { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
    img { max-width: 100%; }
    table { border-collapse: collapse; }
    [style*="display:flex"], [style*="display: flex"] {
      display: flex !important;
      flex-wrap: nowrap !important;
    }
  }
`;

const ANNUAL_LEAVE_TYPE = "AnnualLeaveRequest";

function GenerateAnnualLeaveModal({ annualLeaveId, label, onClose }: Props) {
  const { t } = useTranslation();
  const [templateId, setTemplateId] = useState("");
  const contentRef = useRef<HTMLDivElement>(null);

  const { data: templatePage, isLoading: loadingTemplates } = useQuery({
    queryKey: ["activeDocumentTemplates"],
    queryFn: () =>
      getAllDocumentTemplate({ skip: "0", take: "200" } as unknown as ParameterModel),
  });
  // Only active Annual Leave Request templates apply to a leave request.
  const templates = (templatePage?.data ?? []).filter(
    (x: DocumentTemplateModel) => x.isActive !== false && x.documentType === ANNUAL_LEAVE_TYPE,
  );

  const {
    data: generated,
    isFetching: generating,
    error,
  } = useQuery({
    queryKey: ["generateAnnualLeaveDocument", templateId, annualLeaveId],
    queryFn: () => generateAnnualLeaveDocument(templateId, annualLeaveId),
    enabled: !!templateId && !!annualLeaveId,
  });

  const handlePrint = useReactToPrint({
    contentRef,
    documentTitle: generated?.title || "annual-leave-request",
    pageStyle: PRINT_STYLE,
  });

  return (
    <Modal
      visible
      size="fullscreen"
      title={`${t("Print Annual Leave Request")}${label ? ` — ${label}` : ""}`}
      onClose={onClose}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Close")}
          </button>
          <button
            type="button"
            onClick={() => handlePrint()}
            disabled={!generated || generating}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
          >
            <Printer size={15} /> {t("Print")}
          </button>
        </>
      }
    >
      <div className="flex min-h-[60vh] gap-4">
        {/* Template picker */}
        <aside className="w-60 shrink-0 space-y-1.5 overflow-y-auto border-r border-border pr-3">
          <div className="mb-1 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            {t("Annual Leave Templates")}
          </div>
          {loadingTemplates && <Loading />}
          {!loadingTemplates && templates.length === 0 && (
            <p className="text-sm text-muted">
              {t(
                "No active Annual Leave Request templates. Create one under Document Templates (type: Annual Leave Request).",
              )}
            </p>
          )}
          {templates.map((tpl: DocumentTemplateModel) => (
            <button
              key={tpl.id}
              type="button"
              onClick={() => tpl.id && setTemplateId(tpl.id)}
              className={`flex w-full items-start gap-2 rounded-md border px-2.5 py-2 text-left text-sm transition-colors ${
                templateId === tpl.id
                  ? "border-primary bg-primary/10 text-primary"
                  : "border-border hover:border-primary/50"
              }`}
            >
              <FileText size={15} className="mt-0.5 shrink-0" />
              <span className="min-w-0">
                <span className="block truncate font-medium">{tpl.name}</span>
                <span className="block text-xs text-muted">{t("Annual Leave Request")}</span>
              </span>
            </button>
          ))}
        </aside>

        {/* Preview */}
        <section className="min-w-0 flex-1 overflow-auto rounded-md bg-[color-mix(in_srgb,var(--secondary)_25%,var(--card))] p-4">
          {!templateId && (
            <div className="flex h-full items-center justify-center text-sm text-muted">
              {t("Select a template to preview the document.")}
            </div>
          )}
          {templateId && generating && <Loading />}
          {templateId && error && (
            <p className="text-sm text-error">
              {(error as Error).message || "Failed to generate document."}
            </p>
          )}
          {templateId && !generating && generated && (
            <div className="mx-auto max-w-[800px] rounded bg-white p-8 text-black shadow">
              <div ref={contentRef} className="doc-print-area">
                <div dangerouslySetInnerHTML={{ __html: generated.html }} />
              </div>
            </div>
          )}
        </section>
      </div>
    </Modal>
  );
}

export default GenerateAnnualLeaveModal;
