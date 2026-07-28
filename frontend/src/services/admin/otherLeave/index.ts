import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  OtherLeaveModel,
  OtherLeaveSettingModel,
  OtherLeaveBalanceModel,
  LumpSumEndModel,
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
  return { ok: true, message: parsed?.message ?? "Saved", id: typeof parsed === "string" ? parsed : parsed?.id };
}

/* ---- Other-leave settings (per fiscal year; static, position-based) ---- */
export const getAllOtherLeaveSettings = createPagedQuery<OtherLeaveSettingModel>("OtherLeaveSetting");
export const getOtherLeaveSetting = (id: string) => api.get<OtherLeaveSettingModel>(`OtherLeaveSetting/${id}`);
export const saveOtherLeaveSetting = (m: OtherLeaveSettingModel) => action(m.id ? "PUT" : "POST", "OtherLeaveSetting", m);
export const deleteOtherLeaveSetting = (id: string) => action("DELETE", `OtherLeaveSetting/${id}`);

/* ---- Other-leave requests (same approval mechanism as Annual Leave) ---- */
export const getAllOtherLeaves = createPagedQuery<OtherLeaveModel>("OtherLeave");
export const getOtherLeave = (id: string) => api.get<OtherLeaveModel>(`OtherLeave/${id}`);
export const submitOtherLeave = (body: unknown) => action("POST", "OtherLeave", body);
export const cancelOtherLeave = (id: string) => action("POST", "OtherLeave/cancel", { id });
/** The employee's selectable entitlements for the ACTIVE fiscal year (gender-filtered). */
export const getOtherLeaveBalances = (employeeId: string) =>
  api.get<OtherLeaveBalanceModel[]>(`OtherLeave/balances/${employeeId}`);
/** Server-computed end date of a lump-sum block (allocation working days from start). */
export const getLumpSumEnd = (employeeId: string, otherLeaveSettingId: string, startDate: string) =>
  api.get<LumpSumEndModel>(
    `OtherLeave/lump-sum-end?employeeId=${employeeId}&otherLeaveSettingId=${otherLeaveSettingId}&startDate=${startDate}`,
  );
