import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { TrainingBudgetModel, TrainingBudgetUtilizationModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Admin-only budget envelopes (HC190). */
export const getAllTrainingBudgets = createPagedQuery<TrainingBudgetModel>("TrainingBudget");

export const deleteTrainingBudget = createDeleteService("TrainingBudget");

/** Utilization = actual session costs + committed approved-need estimates. */
export const getBudgetUtilization = (fiscalYear: number, organizationUnitId?: string) =>
  api.get<TrainingBudgetUtilizationModel>(
    `TrainingBudget/utilization?fiscalYear=${fiscalYear}${organizationUnitId ? `&organizationUnitId=${organizationUnitId}` : ""}`,
  );

export interface ActionResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

export const saveTrainingBudget = async (model: TrainingBudgetModel): Promise<ActionResult> => {
  try {
    const isUpdate = !!model.id;
    const res = await fetch(`${API_BASE_URL}/TrainingBudget`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(model),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok)
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    return { status: "success", message: parsed.message ?? "Saved successfully", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
};
