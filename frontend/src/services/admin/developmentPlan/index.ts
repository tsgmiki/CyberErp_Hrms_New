import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { DevelopmentPlanModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface SaveResult {
  status: "success" | "error";
  message: string;
  id?: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke JSON save (POST create / PUT update) — the payload nests `actions`. Returns the saved id. */
async function jsonSave(resource: string, body: Record<string, unknown>): Promise<SaveResult> {
  const isUpdate = typeof body.id === "string" && body.id !== "";
  try {
    const res = await fetch(`${API_BASE_URL}/${resource}`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok) {
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    }
    return { status: "success", message: "Successfully saved", id: parsed.id, zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}

export const getAllDevelopmentPlans = createPagedQuery<DevelopmentPlanModel>("DevelopmentPlan");

export const getDevelopmentPlan = (id: string) => api.get<DevelopmentPlanModel>(`DevelopmentPlan/${id}`);

export const saveDevelopmentPlan = (plan: DevelopmentPlanModel) =>
  jsonSave("DevelopmentPlan", plan as Record<string, unknown>);

export const deleteDevelopmentPlan = createDeleteService("DevelopmentPlan");
