import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { InsurancePolicyModel, InsuranceClaimModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ActionResult { ok: boolean; message: string; id?: string }

async function action(method: string, path: string, body?: unknown): Promise<ActionResult> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method, credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
  if (!res.ok) return { ok: false, message: errorMessageParser(parsed.errors || parsed) || "Request failed" };
  return { ok: true, message: parsed?.message ?? "Saved", id: parsed?.id };
}

/* ---- Policies + premium schedule (HC247/HC250) ---- */
export const getAllInsurancePolicies = createPagedQuery<InsurancePolicyModel>("InsurancePolicy");
export const getInsurancePolicy = (id: string) => api.get<InsurancePolicyModel>(`InsurancePolicy/${id}`);
export const saveInsurancePolicy = (m: InsurancePolicyModel) => action(m.id ? "PUT" : "POST", "InsurancePolicy", m);
export const deleteInsurancePolicy = (id: string) => action("DELETE", `InsurancePolicy/${id}`);
export const generatePremiumSchedule = (policyId: string) => action("POST", `InsurancePolicy/${policyId}/generate-schedule`, {});
export const addPremiumSchedule = (body: { insurancePolicyId: string; dueDate: string; amount: number }) =>
  action("POST", "InsurancePolicy/schedule", body);
export const removePremiumSchedule = (scheduleId: string) => action("DELETE", `InsurancePolicy/schedule/${scheduleId}`);
export const payPremium = (scheduleId: string, reference?: string) => action("POST", `InsurancePolicy/schedule/${scheduleId}/pay`, { reference });

/* ---- Claims (HC248/HC249) ---- */
export const getAllInsuranceClaims = createPagedQuery<InsuranceClaimModel>("InsuranceClaim");
export const getInsuranceClaim = (id: string) => api.get<InsuranceClaimModel>(`InsuranceClaim/${id}`);
export const submitInsuranceClaim = (body: unknown) => action("POST", "InsuranceClaim", body);
export const approveInsuranceClaim = (id: string, approvedAmount?: number, note?: string) =>
  action("POST", `InsuranceClaim/${id}/approve`, { approvedAmount, note });
export const rejectInsuranceClaim = (id: string, reason: string) => action("POST", `InsuranceClaim/${id}/reject`, { reason });
export const payInsuranceClaim = (id: string, reference?: string) => action("POST", `InsuranceClaim/${id}/pay`, { reference });

/** Download a claim attachment (credentialed fetch → browser save). */
export const downloadInsuranceAttachment = async (attachmentId: string, fileName: string) => {
  const res = await fetch(`${API_BASE_URL}/InsuranceClaim/attachments/${attachmentId}`, { credentials: "include" });
  if (!res.ok) return;
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url; a.download = fileName; a.click();
  URL.revokeObjectURL(url);
};

/** Read a File as base64 (strips the data: prefix) for claim attachment upload. */
export const fileToBase64 = (file: File): Promise<string> =>
  new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result).split(",")[1] ?? "");
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
