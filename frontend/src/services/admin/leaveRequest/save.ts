import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { LeaveRequestModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface LeaveRequestSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
  id?: string;
}

/**
 * Bespoke submit: a leave request carries a nested `lines` grid (multiple date ranges and leave
 * types), so it posts JSON from component state rather than going through the FormData save
 * factory. Submit-only (POST) — requests are never edited, only cancelled.
 */
export default async function saveLeaveRequest(
  data: LeaveRequestModel,
): Promise<LeaveRequestSaveResult> {
  if (!data.employeeId)
    return { status: "error", message: "Validation failed", zodErrors: { employeeId: ["Employee is required"] } };

  const lines = data.lines ?? [];
  if (lines.length === 0)
    return { status: "error", message: "Validation failed", zodErrors: { lines: ["Add at least one leave line"] } };

  for (let i = 0; i < lines.length; i++) {
    const l = lines[i];
    const tag = `Line ${i + 1}`;
    if (!l.leaveTypeId)
      return { status: "error", message: "Validation failed", zodErrors: { lines: [`${tag}: choose a leave type`] } };
    if (!l.startDate || !l.endDate)
      return { status: "error", message: "Validation failed", zodErrors: { lines: [`${tag}: start and end dates are required`] } };
    if (l.endDate < l.startDate)
      return { status: "error", message: "Validation failed", zodErrors: { lines: [`${tag}: end date cannot precede start date`] } };
    if (l.dayPart !== "Full" && l.startDate !== l.endDate)
      return { status: "error", message: "Validation failed", zodErrors: { lines: [`${tag}: a half day must be a single date`] } };
  }

  const body = {
    employeeId: data.employeeId,
    reason: data.reason ?? null,
    lines: lines.map((l) => ({
      leaveTypeId: l.leaveTypeId,
      startDate: l.startDate,
      endDate: l.endDate,
      dayPart: l.dayPart || "Full",
    })),
  };

  try {
    const response = await fetch(`${API_BASE_URL}/LeaveRequest`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
    if (!response.ok) {
      const text = await response.text();
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: {} };
    }
    const text = await response.text();
    const savedId = isValidJson(text) ? JSON.parse(text) : text.replace(/"/g, "");
    return { status: "success", message: "Leave request submitted", zodErrors: {}, id: savedId };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
