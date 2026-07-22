import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { TrainingCertificateModel, GeneratedDocumentModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Role-scoped certificates (HR all / manager subtree / employee own). */
export const getAllTrainingCertificates = createPagedQuery<TrainingCertificateModel>("TrainingCertificate");

/** HC200 — renewal tracking: certificates lapsing within the window (admin-only). */
export const getExpiringCertificates = (days = 90) =>
  api.get<TrainingCertificateModel[]>(`TrainingCertificate/expiring?days=${days}`);

async function postAction(path: string, body?: unknown): Promise<{ ok: boolean; message: string; id?: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: "POST",
    credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  let id: string | undefined;
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors?.trainingEnrollmentId?.[0] ?? parsed?.errors?.id?.[0] ?? parsed?.message ?? message;
    id = parsed?.id;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message, id };
}

/** Issues the digital certificate for a COMPLETED enrollment (idempotent). */
export const issueCertificate = (trainingEnrollmentId: string, expiresOn?: string) =>
  postAction("TrainingCertificate/issue", { trainingEnrollmentId, expiresOn });

export const renewCertificate = (id: string, newExpiresOn: string) =>
  postAction(`TrainingCertificate/${id}/renew`, { newExpiresOn });

export const deleteCertificate = async (id: string): Promise<{ ok: boolean; message: string }> => {
  const res = await fetch(`${API_BASE_URL}/TrainingCertificate/${id}`, { method: "DELETE", credentials: "include" });
  const text = await res.text();
  let message = res.ok ? "Deleted" : "Request failed";
  try { message = JSON.parse(text)?.message ?? message; } catch { if (text) message = text; }
  return { ok: res.ok, message };
};

export interface ActionResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Manual record/edit (admin) — e.g. an externally earned credential. */
export const saveCertificate = async (model: TrainingCertificateModel): Promise<ActionResult> => {
  try {
    const isUpdate = !!model.id;
    const res = await fetch(`${API_BASE_URL}/TrainingCertificate`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(model),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok)
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    return { status: "success", message: parsed.message ?? "Saved successfully", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
};

/** Renders the digital certificate via the document merge engine (HC200). */
export const generateCertificateDocument = async (templateId: string, certificateId: string): Promise<GeneratedDocumentModel> => {
  const response = await fetch(`${API_BASE_URL}/DocumentTemplate/${templateId}/generate-certificate/${certificateId}`, {
    credentials: "include",
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Failed to generate the certificate");
  }
  return (await response.json()) as GeneratedDocumentModel;
};
