import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { ImprovementPlanModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface SaveResult {
  status: "success" | "error";
  message: string;
  id?: string;
  zodErrors: Record<string, string[] | undefined>;
}

async function jsonCall(method: "POST" | "PUT", path: string, body?: Record<string, unknown>): Promise<SaveResult> {
  try {
    const res = await fetch(`${API_BASE_URL}/${path}`, {
      method,
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: body ? JSON.stringify(body) : undefined,
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok) {
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    }
    return { status: "success", message: parsed.message ?? "Successfully saved", id: parsed.id, zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}

export const getAllImprovementPlans = createPagedQuery<ImprovementPlanModel>("ImprovementPlan");

export const getImprovementPlan = (id: string) => api.get<ImprovementPlanModel>(`ImprovementPlan/${id}`);

/** Nested-save (POST create / PUT update) — carries `objectives`. */
export const saveImprovementPlan = (plan: ImprovementPlanModel) => {
  const isUpdate = typeof plan.id === "string" && plan.id !== "";
  return jsonCall(isUpdate ? "PUT" : "POST", "ImprovementPlan", plan as Record<string, unknown>);
};

export const recordImprovementPlanOutcome = (dto: { id: string; outcome: string; notes?: string }) =>
  jsonCall("POST", "ImprovementPlan/outcome", dto as Record<string, unknown>);

export const deleteImprovementPlan = createDeleteService("ImprovementPlan");
