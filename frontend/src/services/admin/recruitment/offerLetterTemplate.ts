import { api } from "@/utils/apiClient";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface OfferLetterTemplateModel {
  body: string;
  signatoryName?: string;
  signatoryTitle?: string;
}

export interface CompanyProfileModel {
  companyName?: string;
  contactAddress?: string;
  contactPhone?: string;
  contactEmail?: string;
  hasLogo?: boolean;
}

export interface OfferMergeFieldModel {
  token: string;
  label: string;
}

/* ---- Template body + signatory ---------------------------------------------------- */

export const getOfferLetterTemplate = () =>
  api.get<OfferLetterTemplateModel>("OfferLetterTemplate");

export const saveOfferLetterTemplate = (dto: OfferLetterTemplateModel) =>
  api.put("OfferLetterTemplate", dto);

/* ---- Company letterhead identity -------------------------------------------------- */

export const getCompanyProfile = () =>
  api.get<CompanyProfileModel>("OfferLetterTemplate/company");

export const saveCompanyProfile = (dto: CompanyProfileModel) =>
  api.put("OfferLetterTemplate/company", dto);

/* ---- Merge-field palette ---------------------------------------------------------- */

export const getOfferMergeFields = () =>
  api.get<OfferMergeFieldModel[]>("OfferLetterTemplate/merge-fields");

/* ---- Live PDF preview (sample data over the real letterhead) ---------------------- */

/**
 * Renders the (unsaved) template to a PDF and returns an object URL for an <iframe>/<embed>.
 * The caller must revokeObjectURL when replacing/unmounting.
 */
export async function previewOfferLetter(dto: OfferLetterTemplateModel): Promise<string | null> {
  const res = await fetch(`${API_BASE_URL}/OfferLetterTemplate/preview`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });
  if (!res.ok) return null;
  const blob = await res.blob();
  return URL.createObjectURL(blob);
}
