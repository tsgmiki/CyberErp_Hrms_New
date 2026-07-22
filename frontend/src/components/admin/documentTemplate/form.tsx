"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { DocumentTemplateModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Image as ImageIcon, Upload, Trash2 } from "lucide-react";
import saveDocumentTemplate from "@/services/admin/documentTemplate/save";
import getDocumentTemplate from "@/services/admin/documentTemplate/get";
import getMergeFields from "@/services/admin/documentTemplate/mergeFields";
import {
  companyLogoUrl,
  getCompanyLogoInfo,
  uploadCompanyLogo,
  deleteCompanyLogo,
} from "@/services/admin/documentTemplate/logo";
import Loading from "../../common/loader/loader";
import {
  activeStatusOptions,
  activeId,
  activeLabel,
  documentTypeOptions,
  documentTypeLabel,
} from "@/constants/orgStructure";
import { documentTemplateSamples } from "@/constants/documentTemplates";

const FormProvider = memo(FormProviders);

function DocumentTemplateForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<DocumentTemplateModel>({
    documentType: "EmploymentLetter",
    isActive: true,
  } as DocumentTemplateModel);
  const [copied, setCopied] = useState<string>("");
  const [logoCacheBust, setLogoCacheBust] = useState(() => Date.now());
  const [logoBusy, setLogoBusy] = useState(false);
  const logoInputRef = useRef<HTMLInputElement>(null);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["documentTemplate", id],
    queryFn: () => getDocumentTemplate(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: mergeFields } = useQuery({
    queryKey: ["documentMergeFields"],
    queryFn: getMergeFields,
    staleTime: 5 * 60 * 1000,
  });

  const { data: logoInfo } = useQuery({
    queryKey: ["companyLogoInfo"],
    queryFn: getCompanyLogoInfo,
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const result = await saveDocumentTemplate(formData);
    setFormState(result);
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);
  const headerChange = useCallback((html: string) => {
    setFormData((p) => ({ ...p, headerHtml: html }));
  }, []);
  const bodyChange = useCallback((html: string) => {
    setFormData((p) => ({ ...p, body: html }));
  }, []);
  const footerChange = useCallback((html: string) => {
    setFormData((p) => ({ ...p, footerHtml: html }));
  }, []);

  const loadSample = useCallback(() => {
    const s = documentTemplateSamples[formData.documentType || "EmploymentLetter"];
    if (s)
      setFormData((p) => ({
        ...p,
        headerHtml: s.header?.trim() ?? "",
        body: s.body.trim(),
        footerHtml: s.footer?.trim() ?? "",
      }));
  }, [formData.documentType]);

  const copyToken = useCallback(async (token: string) => {
    try {
      await navigator.clipboard.writeText(token);
      setCopied(token);
      setTimeout(() => setCopied(""), 1500);
    } catch {
      /* clipboard unavailable — chip still shows the token to type manually */
    }
  }, []);

  const onLogoPicked = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      e.target.value = ""; // allow re-picking the same file
      if (!file) return;
      setLogoBusy(true);
      const res = await uploadCompanyLogo(file);
      setLogoBusy(false);
      if (res.ok) {
        setLogoCacheBust(Date.now());
        queryClient.invalidateQueries({ queryKey: ["companyLogoInfo"] });
      } else {
        setFormState({ status: "error", message: res.message, zodErrors: {} });
      }
    },
    [queryClient],
  );

  const onLogoRemove = useCallback(async () => {
    setLogoBusy(true);
    await deleteCompanyLogo();
    setLogoBusy(false);
    setLogoCacheBust(Date.now());
    queryClient.invalidateQueries({ queryKey: ["companyLogoInfo"] });
  }, [queryClient]);

  useEffect(() => {
    if (typeof record != "undefined" && record != null) setFormData(record);
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ documentType: "EmploymentLetter", isActive: true } as DocumentTemplateModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["documentTemplates"] });
      setId("");
    }
  }, [formState]);

  const groupedFields = useMemo(() => {
    const groups: Record<string, { token: string; label: string }[]> = {};
    (mergeFields ?? []).forEach((f) => {
      (groups[f.group] ||= []).push({ token: f.token, label: f.label });
    });
    return groups;
  }, [mergeFields]);

  return (
    <div className="text-foreground">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            {
              name: "name", label: "Template Name", placeholder: "e.g. Employment Confirmation",
              required: true, value: formData.name, onChange: changeHandler,
              error: formState?.zodErrors?.name, type: "text",
            },
            {
              name: "documentType", label: "Document Type", required: true, type: "dropDown",
              onSelect: selectHandler, value: formData.documentType,
              displayValue: documentTypeLabel(formData.documentType),
              error: formState?.zodErrors?.documentType, data: documentTypeOptions as never,
            },
            {
              name: "description", label: "Description", placeholder: "Optional note",
              value: formData.description, onChange: changeHandler, type: "textarea",
              colSpan: "full",
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "headerBreak", label: "Header / Letterhead", type: "break", colSpan: "full",
              sectionDescription: "Optional. Appears above the body — insert {{Logo}} for the company logo." },
            {
              name: "headerHtml", label: "", type: "editor", colSpan: "full",
              value: formData.headerHtml ?? "", onHtmlChange: headerChange,
            },
            { name: "bodyBreak", label: "Template Body", type: "break", colSpan: "full",
              sectionDescription: "Design the document. Insert {{tokens}} where employee data should appear." },
            {
              name: "body", label: "", type: "editor", colSpan: "full",
              value: formData.body ?? "", onHtmlChange: bodyChange,
              error: formState?.zodErrors?.body,
            },
            { name: "footerBreak", label: "Footer", type: "break", colSpan: "full",
              sectionDescription: "Optional. Appears below the body (e.g. issue date, disclaimer)." },
            {
              name: "footerHtml", label: "", type: "editor", colSpan: "full",
              value: formData.footerHtml ?? "", onHtmlChange: footerChange,
            },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />

      {/* Company logo (shared across templates via the {{Logo}} token) */}
      <div className="mt-4 rounded-lg border border-border bg-card p-4">
        <div className="flex items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <div className="flex h-14 w-24 items-center justify-center overflow-hidden rounded-md border border-border bg-background">
              {logoInfo?.hasLogo ? (
                <img
                  src={companyLogoUrl(logoCacheBust)}
                  alt="Company logo"
                  className="max-h-full max-w-full object-contain"
                />
              ) : (
                <ImageIcon size={20} className="text-muted" />
              )}
            </div>
            <div>
              <h4 className="text-sm font-semibold text-foreground">Company Logo</h4>
              <p className="text-xs text-muted-foreground">
                Used by the <code className="rounded bg-secondary px-1">{"{{Logo}}"}</code> token in any
                template. JPG, PNG, WEBP or GIF, up to 2 MB.
              </p>
            </div>
          </div>
          <div className="flex shrink-0 items-center gap-2">
            <input
              ref={logoInputRef}
              type="file"
              accept="image/png,image/jpeg,image/webp,image/gif"
              className="hidden"
              onChange={onLogoPicked}
            />
            <button
              type="button"
              disabled={logoBusy}
              onClick={() => logoInputRef.current?.click()}
              className="inline-flex items-center gap-1.5 rounded-md border border-primary/40 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary hover:bg-primary/20 disabled:opacity-50"
            >
              <Upload size={14} /> {logoInfo?.hasLogo ? "Replace" : "Upload"}
            </button>
            {logoInfo?.hasLogo && (
              <button
                type="button"
                disabled={logoBusy}
                onClick={onLogoRemove}
                className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-error hover:text-error disabled:opacity-50"
              >
                <Trash2 size={14} /> Remove
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Merge-field palette + sample loader */}
      <div className="mt-4 rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between gap-2">
          <div>
            <h4 className="text-sm font-semibold text-foreground">Available Fields</h4>
            <p className="text-xs text-muted-foreground">
              Click a field to copy its token, then paste it into the header, body or footer.
              {copied && <span className="ml-2 font-medium text-primary">Copied {copied}</span>}
            </p>
          </div>
          <button
            type="button"
            onClick={loadSample}
            className="shrink-0 rounded-md border border-primary/40 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary hover:bg-primary/20"
          >
            Load sample for {documentTypeLabel(formData.documentType)}
          </button>
        </div>
        <div className="space-y-3">
          {Object.entries(groupedFields).map(([group, fields]) => (
            <div key={group}>
              <div className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
                {group}
              </div>
              <div className="flex flex-wrap gap-1.5">
                {fields.map((f) => (
                  <button
                    key={f.token}
                    type="button"
                    title={`Copy ${f.token}`}
                    onClick={() => copyToken(f.token)}
                    className="rounded border border-border bg-background px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
                  >
                    {f.label}
                  </button>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default DocumentTemplateForm;
