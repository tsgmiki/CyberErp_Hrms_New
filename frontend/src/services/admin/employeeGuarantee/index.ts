import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { EmployeeGuaranteeModel, GuaranteeDashboardModel } from "@/models";

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

/* ---- §3.12 Employee Guarantee Commitments (HC305–HC307) ---- */
export const getAllGuarantees = createPagedQuery<EmployeeGuaranteeModel>("EmployeeGuarantee");
/** The caller's OWN commitments — self-service list (own slice even for admins). */
export const getMyGuarantees = createPagedQuery<EmployeeGuaranteeModel>("EmployeeGuarantee/mine");
export const getGuarantee = (id: string) => api.get<EmployeeGuaranteeModel>(`EmployeeGuarantee/${id}`);
export const saveGuarantee = (m: EmployeeGuaranteeModel) => action(m.id ? "PUT" : "POST", "EmployeeGuarantee", m);
export const deleteGuarantee = (id: string) => action("DELETE", `EmployeeGuarantee/${id}`);
/** HR discharges an active commitment once the external obligation ends. */
export const releaseGuarantee = (id: string, note?: string) => action("POST", `EmployeeGuarantee/${id}/release`, { note });
export const getGuaranteeDashboard = () => api.get<GuaranteeDashboardModel>("EmployeeGuarantee/dashboard");
