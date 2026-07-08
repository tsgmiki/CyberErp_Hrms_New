import { api } from "@/utils/apiClient";
import { EmployeeTerminationSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import type {
  EmployeeTerminationModel,
  TerminatedEmployeeModel,
  MyClearancesModel,
  ReinstatementInfoModel,
} from "@/models";
import type ParameterModel from "@/models/ParameterModel";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Termination cases (with clearance checklist) for one employee, newest first. */
export const getTerminations = (employeeId: string) =>
  api.get<EmployeeTerminationModel[]>(`EmployeeTermination?employeeId=${employeeId}`);

const getTerminatedPage = createPagedQuery<TerminatedEmployeeModel>("EmployeeTermination/terminated");

/** Paged Termination List: terminated employees with their latest case. */
export const getTerminatedEmployees = async (param: ParameterModel) => {
  const page = await getTerminatedPage(param);
  // The list template keys rows by `id` — map it from the employee id.
  return { ...page, data: (page.data ?? []).map((r) => ({ ...r, id: r.employeeId })) };
};

/** The current user's clearance work queue (Dashboard "Clearance" tab, approver-only). */
export const getMyClearances = () =>
  api.get<MyClearancesModel>("EmployeeTermination/my-clearances");

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

/** Pre-reinstatement context: previous position + whether it is still available. */
export const getReinstatementInfo = (employeeId: string) =>
  api.get<ReinstatementInfoModel>(`EmployeeTermination/reinstatement-info?employeeId=${employeeId}`);

/** Reinstate a terminated employee onto the chosen (vacant) position. */
export async function reinstateEmployee(
  employeeId: string,
  positionId: string,
): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/EmployeeTermination/reinstate`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ employeeId, positionId }),
  });
  const text = await res.text();
  let message = res.ok ? "Employee reinstated" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}
