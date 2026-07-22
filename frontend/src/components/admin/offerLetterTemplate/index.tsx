"use client";
import { useEffect, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { FileText, Image as ImageIcon, Save, Eye, Trash2, Loader2 } from "lucide-react";
import Loading from "@/components/common/loader/loader";
import errorMessageParser from "@/components/util/errorMessageParser";
import {
  getOfferLetterTemplate,
  saveOfferLetterTemplate,
  getCompanyProfile,
  saveCompanyProfile,
  getOfferMergeFields,
  previewOfferLetter,
  type CompanyProfileModel,
} from "@/services/admin/recruitment/offerLetterTemplate";
import {
  companyLogoUrl,
  getCompanyLogoInfo,
  uploadCompanyLogo,
  deleteCompanyLogo,
} from "@/services/admin/documentTemplate/logo";

const inputCls =
  "h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground";
const labelCls = "block text-xs font-semibold uppercase tracking-wide text-muted";

function OfferLetterTemplate() {
  const { t } = useTranslation();

  // Company letterhead
  const [company, setCompany] = useState<CompanyProfileModel>({});
  // Template
  const [body, setBody] = useState("");
  const [signatoryName, setSignatoryName] = useState("");
  const [signatoryTitle, setSignatoryTitle] = useState("");

  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [logoCacheBust, setLogoCacheBust] = useState(Date.now());
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [previewBusy, setPreviewBusy] = useState(false);
  const bodyRef = useRef<HTMLTextAreaElement>(null);

  const { data: template, isLoading: loadingTemplate } = useQuery({
    queryKey: ["offerLetterTemplate"],
    queryFn: getOfferLetterTemplate,
  });
  const { data: companyData, isLoading: loadingCompany } = useQuery({
    queryKey: ["offerCompanyProfile"],
    queryFn: getCompanyProfile,
  });
  const { data: mergeFields } = useQuery({
    queryKey: ["offerMergeFields"],
    queryFn: getOfferMergeFields,
  });
  const { data: logoInfo, refetch: refetchLogo } = useQuery({
    queryKey: ["companyLogoInfo"],
    queryFn: getCompanyLogoInfo,
  });

  useEffect(() => {
    if (template) {
      setBody(template.body ?? "");
      setSignatoryName(template.signatoryName ?? "");
      setSignatoryTitle(template.signatoryTitle ?? "");
    }
  }, [template]);
  useEffect(() => {
    if (companyData) setCompany(companyData);
  }, [companyData]);
  // Release the preview object URL when it changes or the editor unmounts.
  useEffect(() => () => {
    if (previewUrl) URL.revokeObjectURL(previewUrl);
  }, [previewUrl]);

  const insertToken = (token: string) => {
    const el = bodyRef.current;
    if (!el) {
      setBody((b) => b + token);
      return;
    }
    const start = el.selectionStart ?? body.length;
    const end = el.selectionEnd ?? body.length;
    const next = body.slice(0, start) + token + body.slice(end);
    setBody(next);
    // Restore focus + caret just after the inserted token.
    requestAnimationFrame(() => {
      el.focus();
      const caret = start + token.length;
      el.setSelectionRange(caret, caret);
    });
  };

  const save = async () => {
    setError(null);
    setInfo(null);
    if (!body.trim()) return setError(t("The offer-letter body cannot be empty."));
    setBusy(true);
    try {
      await saveCompanyProfile(company);
      await saveOfferLetterTemplate({ body, signatoryName, signatoryTitle });
      setInfo(t("Offer-letter template saved."));
    } catch (e) {
      setError(errorMessageParser(e));
    } finally {
      setBusy(false);
    }
  };

  const preview = async () => {
    setError(null);
    setPreviewBusy(true);
    try {
      // Save the letterhead first so the preview reflects the current company fields + logo.
      await saveCompanyProfile(company);
      const url = await previewOfferLetter({ body, signatoryName, signatoryTitle });
      if (!url) return setError(t("Could not generate the preview."));
      setPreviewUrl((old) => {
        if (old) URL.revokeObjectURL(old);
        return url;
      });
    } catch (e) {
      setError(errorMessageParser(e));
    } finally {
      setPreviewBusy(false);
    }
  };

  const onLogoPick = async (file?: File | null) => {
    if (!file) return;
    setError(null);
    const res = await uploadCompanyLogo(file);
    if (!res.ok) return setError(res.message);
    setLogoCacheBust(Date.now());
    refetchLogo();
  };

  const removeLogo = async () => {
    await deleteCompanyLogo();
    setLogoCacheBust(Date.now());
    refetchLogo();
  };

  if (loadingTemplate || loadingCompany) return <Loading />;

  return (
    <div className="m-1 flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      <div className="flex flex-wrap items-center gap-2 border-b border-border px-3 py-2">
        <h1 className="flex items-center gap-2 text-sm font-semibold text-foreground">
          <FileText size={16} className="text-primary" />
          {t("Offer Letter Template")}
          <span className="text-xs font-normal text-muted">
            — {t("customize the PDF offer letter e-mailed to candidates (HC111)")}
          </span>
        </h1>
        <div className="ml-auto flex items-center gap-2">
          <button
            type="button"
            disabled={previewBusy}
            onClick={preview}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-semibold text-foreground hover:border-primary hover:text-primary disabled:opacity-50"
          >
            {previewBusy ? <Loader2 size={14} className="animate-spin" /> : <Eye size={14} />}
            {t("Preview PDF")}
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={save}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            {busy ? <Loader2 size={14} className="animate-spin" /> : <Save size={14} />}
            {t("Save")}
          </button>
        </div>
      </div>

      {error && (
        <p className="mx-3 mt-2 rounded-md border border-error/30 bg-error/10 px-3 py-2 text-xs text-error">
          {error}
        </p>
      )}
      {info && (
        <p className="mx-3 mt-2 rounded-md border border-success/30 bg-success/10 px-3 py-2 text-xs text-success">
          {info}
        </p>
      )}

      <div className="grid min-h-0 flex-1 grid-cols-1 gap-4 overflow-auto p-3 lg:grid-cols-2">
        {/* ---- Left: letterhead + template fields ---- */}
        <div className="space-y-4">
          {/* Company letterhead */}
          <section className="space-y-3 rounded-lg border border-border p-3">
            <h2 className="text-sm font-semibold text-foreground">{t("Company Letterhead")}</h2>

            <div className="flex items-start gap-3">
              <div className="flex h-20 w-28 items-center justify-center overflow-hidden rounded-md border border-dashed border-border bg-secondary/40">
                {logoInfo?.hasLogo ? (
                  <img
                    src={companyLogoUrl(logoCacheBust)}
                    alt="logo"
                    className="max-h-full max-w-full object-contain"
                  />
                ) : (
                  <ImageIcon size={22} className="text-muted" />
                )}
              </div>
              <div className="space-y-1">
                <label className={labelCls}>{t("Company Logo")}</label>
                <div className="flex items-center gap-2">
                  <label className="cursor-pointer rounded-md border border-border px-2.5 py-1 text-xs text-foreground hover:border-primary hover:text-primary">
                    {t("Upload")}
                    <input
                      type="file"
                      accept="image/png,image/jpeg,image/webp,image/gif"
                      className="hidden"
                      onChange={(e) => onLogoPick(e.target.files?.[0])}
                    />
                  </label>
                  {logoInfo?.hasLogo && (
                    <button
                      type="button"
                      onClick={removeLogo}
                      className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs text-muted hover:border-error hover:text-error"
                    >
                      <Trash2 size={12} /> {t("Remove")}
                    </button>
                  )}
                </div>
                <p className="text-[10px] text-muted">{t("PNG, JPG, WEBP or GIF · max 2 MB")}</p>
              </div>
            </div>

            <div>
              <label className={labelCls}>{t("Company Name")}</label>
              <input
                type="text"
                value={company.companyName ?? ""}
                onChange={(e) => setCompany((c) => ({ ...c, companyName: e.target.value }))}
                className={inputCls}
              />
            </div>
            <div>
              <label className={labelCls}>{t("Contact Address")}</label>
              <textarea
                value={company.contactAddress ?? ""}
                onChange={(e) => setCompany((c) => ({ ...c, contactAddress: e.target.value }))}
                rows={2}
                className="w-full rounded-md border border-border bg-background px-2 py-1.5 text-sm text-foreground"
              />
            </div>
            <div className="grid grid-cols-2 gap-2">
              <div>
                <label className={labelCls}>{t("Contact Phone")}</label>
                <input
                  type="text"
                  value={company.contactPhone ?? ""}
                  onChange={(e) => setCompany((c) => ({ ...c, contactPhone: e.target.value }))}
                  className={inputCls}
                />
              </div>
              <div>
                <label className={labelCls}>{t("Contact Email")}</label>
                <input
                  type="text"
                  value={company.contactEmail ?? ""}
                  onChange={(e) => setCompany((c) => ({ ...c, contactEmail: e.target.value }))}
                  className={inputCls}
                />
              </div>
            </div>
          </section>

          {/* Letter body */}
          <section className="space-y-3 rounded-lg border border-border p-3">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold text-foreground">{t("Letter Body")}</h2>
            </div>

            {/* Token palette */}
            <div className="flex flex-wrap gap-1.5">
              {(mergeFields ?? []).map((f) => (
                <button
                  key={f.token}
                  type="button"
                  title={f.label}
                  onClick={() => insertToken(f.token)}
                  className="rounded border border-border bg-secondary/40 px-1.5 py-0.5 font-mono text-[11px] text-primary hover:border-primary"
                >
                  {f.token}
                </button>
              ))}
            </div>

            <textarea
              ref={bodyRef}
              value={body}
              onChange={(e) => setBody(e.target.value)}
              rows={14}
              className="w-full rounded-md border border-border bg-background px-2 py-1.5 font-mono text-xs text-foreground"
              placeholder={t("Dear {{CandidateName}}, …")}
            />

            <div className="grid grid-cols-2 gap-2">
              <div>
                <label className={labelCls}>{t("Signatory Name")}</label>
                <input
                  type="text"
                  value={signatoryName}
                  onChange={(e) => setSignatoryName(e.target.value)}
                  className={inputCls}
                />
              </div>
              <div>
                <label className={labelCls}>{t("Signatory Title")}</label>
                <input
                  type="text"
                  value={signatoryTitle}
                  onChange={(e) => setSignatoryTitle(e.target.value)}
                  className={inputCls}
                />
              </div>
            </div>
          </section>
        </div>

        {/* ---- Right: PDF preview ---- */}
        <div className="flex min-h-[400px] flex-col rounded-lg border border-border">
          <div className="border-b border-border px-3 py-1.5 text-xs font-semibold text-muted">
            {t("Preview")}
          </div>
          {previewUrl ? (
            <iframe title="offer-letter-preview" src={previewUrl} className="min-h-0 flex-1 rounded-b-lg" />
          ) : (
            <div className="flex flex-1 flex-col items-center justify-center gap-2 p-6 text-center text-sm text-muted">
              <Eye size={22} />
              <p>{t("Click \"Preview PDF\" to render the letter with sample data over your letterhead.")}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default OfferLetterTemplate;
