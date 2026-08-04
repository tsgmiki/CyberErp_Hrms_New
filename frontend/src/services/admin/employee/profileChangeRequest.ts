import { api } from "@/utils/apiClient";
import errorMessageParser from "@/components/util/errorMessageParser";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** One profile change request (HR review queue / employee tracking). */
export interface ProfileChangeRequestModel {
  id: string;
  employeeId: string;
  employeeName?: string;
  employeeNumber?: string;
  fieldKey: string;
  fieldLabel: string;
  /** IdentityField (auto-applied on approval) | Structural (HR fulfils via the owning module). */
  kind: string;
  currentValue?: string;
  requestedValue: string;
  reason?: string;
  status: string; // Pending | Approved | Rejected
  resolution?: string;
  autoApplied: boolean;
  submittedOn: string;
  resolvedOn?: string;
  resolvedBy?: string;
}

export interface ProfileChangeApprovalsModel {
  isApprover: boolean;
  items: ProfileChangeRequestModel[];
}

/** The HR review queue (dashboard) — pending requests; empty for non-HR. */
export const getPendingProfileChangeRequests = () =>
  api.get<ProfileChangeApprovalsModel>("ProfileChangeRequest/pending");

/** HR approves (auto-applies identity fields) or rejects a request. */
export async function resolveProfileChangeRequest(
  id: string,
  decision: "Approve" | "Reject",
  resolution?: string,
): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/ProfileChangeRequest/${id}/resolve`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ decision, resolution: resolution || null }),
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
