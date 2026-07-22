import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  MedicalProviderModel, MedicalPlanModel, MedicalContractModel,
  MedicalEnrollmentModel, MedicalClaimModel, MedicalExpenseReportModel,
} from "@/models";

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

/* ---- Providers / plans / contracts (HC238/235/236) ---- */
export const getAllMedicalProviders = createPagedQuery<MedicalProviderModel>("MedicalProvider");
export const saveMedicalProvider = (m: MedicalProviderModel) => action(m.id ? "PUT" : "POST", "MedicalProvider", m);
export const deleteMedicalProvider = (id: string) => action("DELETE", `MedicalProvider/${id}`);
export const getAllMedicalPlans = createPagedQuery<MedicalPlanModel>("MedicalPlan");
export const saveMedicalPlan = (m: MedicalPlanModel) => action(m.id ? "PUT" : "POST", "MedicalPlan", m);
export const deleteMedicalPlan = (id: string) => action("DELETE", `MedicalPlan/${id}`);
export const getAllMedicalContracts = createPagedQuery<MedicalContractModel>("MedicalContract");
export const saveMedicalContract = (m: MedicalContractModel) => action(m.id ? "PUT" : "POST", "MedicalContract", m);
export const deleteMedicalContract = (id: string) => action("DELETE", `MedicalContract/${id}`);

/* ---- Enrollment + beneficiaries (HC235/237) ---- */
export const getEmployeeMedicalEnrollments = (employeeId: string) =>
  api.get<MedicalEnrollmentModel[]>(`MedicalEnrollment?employeeId=${employeeId}`);
/** The signed-in employee's own enrollments (self-service claim entry). */
export const getMyMedicalEnrollments = () => api.get<MedicalEnrollmentModel[]>("MedicalEnrollment/mine");
export const saveMedicalEnrollment = (m: MedicalEnrollmentModel) => action(m.id ? "PUT" : "POST", "MedicalEnrollment", m);
export const setMedicalEnrollmentStatus = (id: string, status: string, coverageEnd?: string) =>
  action("POST", `MedicalEnrollment/${id}/status/${status}`, { coverageEnd });
export const deleteMedicalEnrollment = (id: string) => action("DELETE", `MedicalEnrollment/${id}`);
export const addMedicalBeneficiary = (body: { medicalEnrollmentId: string; category: string; employeeDependentId?: string; fullName?: string; dateOfBirth?: string; relationship?: string }) =>
  action("POST", "MedicalEnrollment/beneficiaries", body);
export const removeMedicalBeneficiary = (beneficiaryId: string) => action("DELETE", `MedicalEnrollment/beneficiaries/${beneficiaryId}`);

/* ---- Claims + reports (HC239–246) ---- */
export const getAllMedicalClaims = createPagedQuery<MedicalClaimModel>("MedicalClaim");
export const getMedicalClaim = (id: string) => api.get<MedicalClaimModel>(`MedicalClaim/${id}`);
export const submitMedicalClaim = (body: unknown) => action("POST", "MedicalClaim", body);
export const approveMedicalClaim = (id: string, approvedAmount?: number, note?: string) =>
  action("POST", `MedicalClaim/${id}/approve`, { approvedAmount, note });
export const rejectMedicalClaim = (id: string, reason: string) => action("POST", `MedicalClaim/${id}/reject`, { reason });
export const payMedicalClaim = (id: string, reference?: string) => action("POST", `MedicalClaim/${id}/pay`, { reference });
export const getMedicalExpenseReport = (fromDate?: string, toDate?: string) => {
  const q = new URLSearchParams();
  if (fromDate) q.set("fromDate", fromDate);
  if (toDate) q.set("toDate", toDate);
  return api.get<MedicalExpenseReportModel>(`MedicalClaim/expense-report?${q}`);
};

/** Download an attachment (credentialed fetch → browser save). */
export const downloadMedicalAttachment = async (attachmentId: string, fileName: string) => {
  const res = await fetch(`${API_BASE_URL}/MedicalClaim/attachments/${attachmentId}`, { credentials: "include" });
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
