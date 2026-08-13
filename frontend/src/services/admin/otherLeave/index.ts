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

/* ---- Supporting documents ------------------------------------------------ */

/**
 * The request as its assigned APPROVER may read it — details plus attachment metadata.
 *
 * Separate from `getOtherLeave`: that one authorises with the normal employee-visibility rule, and
 * an approver routed the request by the workflow does not necessarily manage the requester, so it
 * would refuse them. The server grants this one to the requester, their manager chain, HR, or the
 * approver the request is currently routed to.
 */
export const reviewOtherLeave = (id: string) => api.get<OtherLeaveModel>(`OtherLeave/${id}/review`);

/** Download one supporting document (credentialed fetch → browser save). */
export const downloadOtherLeaveAttachment = async (attachmentId: string, fileName: string) => {
  const res = await fetch(`${API_BASE_URL}/OtherLeave/attachments/${attachmentId}`, {
    credentials: "include",
  });
  if (!res.ok) return false;
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url; a.download = fileName; a.click();
  URL.revokeObjectURL(url);
  return true;
};

/** Read a File as base64 (strips the data: prefix) for attachment upload. */
export const fileToBase64 = (file: File): Promise<string> =>
  new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result).split(",")[1] ?? "");
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
