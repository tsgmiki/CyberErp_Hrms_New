import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { AnnualLeaveModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface AnnualLeaveSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
  id?: string;
}

/**
 * Bespoke submit: an annual-leave request carries a nested `details` grid, so it posts JSON from
 * component state rather than going through the FormData save factory. Submit-only (POST) —
 * requests are never edited, only cancelled.
 */
export default async function saveAnnualLeave(
  data: AnnualLeaveModel,
): Promise<AnnualLeaveSaveResult> {
  if (!data.employeeId)
    return { status: "error", message: "Validation failed", zodErrors: { employeeId: ["Employee is required"] } };
  if (!data.annualLeaveLedgerId)
    return { status: "error", message: "Validation failed", zodErrors: { annualLeaveLedgerId: ["Select the annual-leave ledger"] } };

  const details = data.details ?? [];
  if (details.length === 0)
    return { status: "error", message: "Validation failed", zodErrors: { details: ["Add at least one leave line"] } };

  for (let i = 0; i < details.length; i++) {
    const d = details[i];
    const tag = `Line ${i + 1}`;
    if (!d.startDate || !d.endDate)
      return { status: "error", message: "Validation failed", zodErrors: { details: [`${tag}: start and end dates are required`] } };
    if (d.endDate < d.startDate)
      return { status: "error", message: "Validation failed", zodErrors: { details: [`${tag}: end date cannot precede start date`] } };
    if (d.leaveUsage === "HalfDay" && d.startDate !== d.endDate)
      return { status: "error", message: "Validation failed", zodErrors: { details: [`${tag}: a half day must be a single date`] } };
    if (d.leaveUsage === "HalfDay" && !d.halfDayPart)
      return { status: "error", message: "Validation failed", zodErrors: { details: [`${tag}: choose Morning or Afternoon for the half day`] } };
  }

  const body = {
    employeeId: data.employeeId,
    annualLeaveLedgerId: data.annualLeaveLedgerId,
    remark: data.remark ?? null,
    details: details.map((d) => ({
      leaveUsage: d.leaveUsage || "FullDay",
      halfDayPart: d.leaveUsage === "HalfDay" ? d.halfDayPart : null,
      startDate: d.startDate,
      endDate: d.endDate,
    })),
  };

  try {
    const response = await fetch(`${API_BASE_URL}/AnnualLeave`, {
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
    return { status: "success", message: "Annual leave submitted", zodErrors: {}, id: savedId };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
