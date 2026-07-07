import { api } from "@/utils/apiClient";
import { EmployeeTerminationSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";
import errorMessageParser from "@/components/util/errorMessageParser";
import type { EmployeeTerminationModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Termination cases (with clearance checklist) for one employee, newest first. */
export const getTerminations = (employeeId: string) =>
  api.get<EmployeeTerminationModel[]>(`EmployeeTermination?employeeId=${employeeId}`);

export const saveTermination = createSaveService("EmployeeTermination", EmployeeTerminationSchema);

async function post(path: string, body?: unknown): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: body ? "PUT" : "POST",
    credentials: "include",
    ...(body ? { headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) } : {}),
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

/** Sets a departmental clearance item to Pending / Cleared / Blocked. */
export const updateClearance = (id: string, status: string, note?: string) =>
  post("EmployeeTermination/clearance", { id, status, note: note || null });

/** Final settlement: employee → Terminated, position decoupled + reopened. */
export const finalizeTermination = (id: string) => post(`EmployeeTermination/${id}/finalize`);

export const cancelTermination = (id: string) => post(`EmployeeTermination/${id}/cancel`);
