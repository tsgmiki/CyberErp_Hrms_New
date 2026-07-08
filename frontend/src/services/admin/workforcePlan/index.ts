import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  WorkforcePlanModel,
  EstablishmentRowModel,
  SeparationSuggestionModel,
  WorkforcePlanSummaryModel,
  WorkforcePlanComparisonModel,
  ApprovedDemandRowModel,
} from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const getAllWorkforcePlans = createPagedQuery<WorkforcePlanModel>("WorkforcePlan");

export const getWorkforcePlan = (id: string) =>
  api.get<WorkforcePlanModel>(`WorkforcePlan/${id}`);

export const deleteWorkforcePlan = createDeleteService("WorkforcePlan");

/** Live establishment: authorized / filled / vacant per unit × role (HC056). */
export const getEstablishment = (organizationUnitId?: string) =>
  api.get<EstablishmentRowModel[]>(
    `WorkforcePlan/establishment${organizationUnitId ? `?organizationUnitId=${organizationUnitId}` : ""}`,
  );

/** Budget position + time-phased aggregates (HC069/HC073). */
export const getWorkforcePlanSummary = (id: string) =>
  api.get<WorkforcePlanSummaryModel>(`WorkforcePlan/${id}/summary`);

/** Suggested per-line retirement counts inside the horizon (HC060). */
export const suggestSeparations = (id: string) =>
  api.get<SeparationSuggestionModel[]>(`WorkforcePlan/${id}/suggest-separations`);

/** Side-by-side scenario comparison (HC068). */
export const compareWorkforcePlans = (ids: string[]) =>
  api.get<WorkforcePlanComparisonModel[]>(`WorkforcePlan/compare?ids=${ids.join(",")}`);

/** Outstanding approved hiring demand — the recruitment feed (HC075). */
export const getApprovedDemand = () =>
  api.get<ApprovedDemandRowModel[]>("WorkforcePlan/approved-demand");

export interface WorkforcePlanSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
  id?: string;
}

/** Bespoke save: the plan carries a nested lines grid, so it posts JSON from component state. */
export async function saveWorkforcePlan(
  data: WorkforcePlanModel,
): Promise<WorkforcePlanSaveResult> {
  if (!data.name?.trim())
    return { status: "error", message: "Validation failed", zodErrors: { name: ["Name is required"] } };
  if (!data.startFiscalYearId)
    return {
      status: "error",
      message: "Validation failed",
      zodErrors: { startFiscalYearId: ["The starting fiscal year is required"] },
    };
  if ((data.lines ?? []).some((l) => !l.organizationUnitId || !l.positionClassId))
    return {
      status: "error",
      message: "Validation failed",
      zodErrors: { lines: ["Every line needs an organization unit and a role"] },
    };

  const isUpdate = !!data.id;
  const body: Record<string, unknown> = {
    ...data,
    periodCount: Number(data.periodCount) || 1,
    totalBudget: Number(data.totalBudget) || 0,
    budgetThresholdPercent: Number(data.budgetThresholdPercent) || 0,
    lines: (data.lines ?? []).map((l) => ({
      ...l,
      periodIndex: Number(l.periodIndex) || 0,
      authorizedHeadcount: Number(l.authorizedHeadcount) || 0,
      filledCount: Number(l.filledCount) || 0,
      vacantCount: Number(l.vacantCount) || 0,
      newHires: Number(l.newHires) || 0,
      replacements: Number(l.replacements) || 0,
      temporaryStaff: Number(l.temporaryStaff) || 0,
      mobilityIn: Number(l.mobilityIn) || 0,
      promotions: Number(l.promotions) || 0,
      actingAssignments: Number(l.actingAssignments) || 0,
      retirements: Number(l.retirements) || 0,
      resignations: Number(l.resignations) || 0,
      contractExpiries: Number(l.contractExpiries) || 0,
      isCriticalRole: l.isCriticalRole === true || String(l.isCriticalRole) === "true",
      annualSalaryCost: Number(l.annualSalaryCost) || 0,
      annualAllowances: Number(l.annualAllowances) || 0,
      annualBenefits: Number(l.annualBenefits) || 0,
    })),
  };
  if (!isUpdate) delete body.id;

  try {
    const response = await fetch(`${API_BASE_URL}/WorkforcePlan`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
    if (!response.ok) {
      const text = await response.text();
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: {} };
    }
    let savedId = data.id;
    if (!isUpdate) {
      const text = await response.text();
      savedId = isValidJson(text) ? JSON.parse(text) : text.replace(/"/g, "");
    }
    return { status: "success", message: "Successfully saved", zodErrors: {}, id: savedId };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}

async function post(path: string, body?: unknown): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: "POST",
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

/** Rebuilds the plan grid from the live establishment in the plan's scope (HC055). */
export const populateWorkforcePlan = (id: string) => post(`WorkforcePlan/${id}/populate`);

/** Submits for approval; an over-threshold cost requires an escalation justification (HC066). */
export const submitWorkforcePlan = (id: string, escalationJustification?: string) =>
  post(`WorkforcePlan/${id}/submit`, { id, escalationJustification: escalationJustification || null });

/** Creates the next editable version of a non-draft plan (HC071); returns the new plan id. */
export async function createWorkforcePlanVersion(
  id: string,
): Promise<{ ok: boolean; message: string; id?: string }> {
  const res = await fetch(`${API_BASE_URL}/WorkforcePlan/${id}/new-version`, {
    method: "POST",
    credentials: "include",
  });
  const text = await res.text();
  if (!res.ok) {
    let message = "Request failed";
    try {
      const parsed = JSON.parse(text);
      message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
    } catch {
      if (text) message = text;
    }
    return { ok: false, message };
  }
  const newId = isValidJson(text) ? JSON.parse(text) : text.replace(/"/g, "");
  return { ok: true, message: "New version created", id: newId };
}
