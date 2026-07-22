import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { PerDiemRateModel, TripBudgetModel, TripBudgetUtilizationModel, TripRequestModel, TripAgingReportModel } from "@/models";

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

/* ---- Config: per-diem rates + travel budgets (HC266/HC267) ---- */
export const getAllPerDiemRates = createPagedQuery<PerDiemRateModel>("PerDiemRate");
export const savePerDiemRate = (m: PerDiemRateModel) => action(m.id ? "PUT" : "POST", "PerDiemRate", m);
export const deletePerDiemRate = (id: string) => action("DELETE", `PerDiemRate/${id}`);
export const getAllTripBudgets = createPagedQuery<TripBudgetModel>("TripBudget");
export const saveTripBudget = (m: TripBudgetModel) => action(m.id ? "PUT" : "POST", "TripBudget", m);
export const deleteTripBudget = (id: string) => action("DELETE", `TripBudget/${id}`);
export const getTripBudgetUtilization = (fiscalYear: number, organizationUnitId?: string | null) => {
  const q = new URLSearchParams({ fiscalYear: String(fiscalYear) });
  if (organizationUnitId) q.set("organizationUnitId", organizationUnitId);
  return api.get<TripBudgetUtilizationModel>(`TripBudget/utilization?${q}`);
};

/* ---- Trips (HC260–265/268) ---- */
export const getAllTrips = createPagedQuery<TripRequestModel>("TripRequest");
export const getTrip = (id: string) => api.get<TripRequestModel>(`TripRequest/${id}`);
export const requestTrip = (body: unknown) => action("POST", "TripRequest", body);
export const approveTrip = (id: string, note?: string) => action("POST", `TripRequest/${id}/approve`, { note });
export const rejectTrip = (id: string, reason: string) => action("POST", `TripRequest/${id}/reject`, { reason });
export const cancelTrip = (id: string) => action("POST", `TripRequest/${id}/cancel`, {});
export const startTrip = (id: string) => action("POST", `TripRequest/${id}/start`, {});
export const completeTrip = (id: string) => action("POST", `TripRequest/${id}/complete`, {});
export const addTripExpense = (body: { tripRequestId: string; category: string; description?: string; expenseDate: string; amount: number; currency?: string }) =>
  action("POST", "TripRequest/expenses", body);
export const removeTripExpense = (expenseId: string) => action("DELETE", `TripRequest/expenses/${expenseId}`);
export const disburseTripAdvance = (id: string, reference?: string) => action("POST", `TripRequest/${id}/disburse-advance`, { reference });
export const settleTrip = (id: string, reference?: string) => action("POST", `TripRequest/${id}/settle`, { reference });
export const getTripAgingReport = () => api.get<TripAgingReportModel>("TripRequest/aging-report");
export const runSettlementReminders = () => action("POST", "TripRequest/run-settlement-reminders", {});

/* ---- Dropdown option sources ---- */
export const getJobGradeOptions = createPagedQuery<{ id: string; name: string; code?: string }>("JobGrade");
export const getOrgUnitOptions = createPagedQuery<{ id: string; name: string; code?: string }>("OrganizationUnit");
