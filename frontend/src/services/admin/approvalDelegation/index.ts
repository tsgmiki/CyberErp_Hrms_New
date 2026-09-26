import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  ApprovalDelegationModel,
  DelegationEligibilityModel,
  DelegationPolicyModel,
} from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const getAllApprovalDelegations =
  createPagedQuery<ApprovalDelegationModel>("ApprovalDelegation");

/** Both directions for the signed-in user — what they lent, and what they hold. */
export const getMyDelegations = () =>
  api.get<ApprovalDelegationModel[]>("ApprovalDelegation/mine");

/**
 * Would this person be allowed to stand in?
 *
 * Called as the form is filled so the seniority rules answer BEFORE somebody commits — a rule that
 * only speaks at save time reads as the system being obstructive rather than as a policy.
 */
export const checkDelegationEligibility = (fromEmployeeId: string, toEmployeeId: string) =>
  api.get<DelegationEligibilityModel>(
    `ApprovalDelegation/eligibility?fromEmployeeId=${fromEmployeeId}&toEmployeeId=${toEmployeeId}`,
  );

export const getDelegationPolicy = () =>
  api.get<DelegationPolicyModel>("ApprovalDelegation/policy");

export const saveDelegationPolicy = (policy: DelegationPolicyModel) =>
  api.put<{ message: string }>("ApprovalDelegation/policy", policy);

export const revokeDelegation = (id: string, reason?: string) =>
  api.put<{ message: string }>("ApprovalDelegation/revoke", { id, reason });

export interface DelegationSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/**
 * Bespoke save: the delegation carries a process-scope array, so it posts JSON from state rather
 * than going through the form-data save factory.
 */
export async function saveApprovalDelegation(
  data: ApprovalDelegationModel,
): Promise<DelegationSaveResult> {
  const zodErrors: Record<string, string[]> = {};
  if (!data.fromEmployeeId) zodErrors.fromEmployeeId = ["Choose the approver whose authority is delegated"];
  if (!data.toEmployeeId) zodErrors.toEmployeeId = ["Choose the delegate"];
  if (!data.startDate) zodErrors.startDate = ["A start date is required"];
  if (!data.endDate) zodErrors.endDate = ["An end date is required"];
  if (data.startDate && data.endDate && data.endDate < data.startDate)
    zodErrors.endDate = ["The delegation cannot end before it starts"];
  if (!data.allProcesses && (data.entityTypes ?? []).length === 0)
    zodErrors.entityTypes = ["Select at least one process, or delegate all processes"];
  if (Object.keys(zodErrors).length > 0)
    return { status: "error", message: "Validation failed", zodErrors };

  const body: Record<string, unknown> = {
    id: data.id || undefined,
    fromEmployeeId: data.fromEmployeeId,
    toEmployeeId: data.toEmployeeId,
    startDate: data.startDate,
    endDate: data.endDate,
    reason: data.reason || null,
    allProcesses: data.allProcesses !== false,
    entityTypes: data.allProcesses !== false ? [] : (data.entityTypes ?? []),
    // An empty box means "no ceiling", which is not the same as a ceiling of zero.
    approvalLimit:
      data.approvalLimit === null || data.approvalLimit === undefined || (data.approvalLimit as unknown) === ""
        ? null
        : Number(data.approvalLimit),
  };

  try {
    const response = await fetch(`${API_BASE_URL}/ApprovalDelegation`, {
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
    return { status: "success", message: "Delegation saved", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
