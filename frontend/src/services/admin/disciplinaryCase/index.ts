import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { DisciplinaryMeasureModel, DisciplinaryEligibilityModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** HC222/HC225 — role-scoped paged case list (admin all / manager subtree+raised / employee own). */
export const getAllDisciplinaryCases = createPagedQuery<DisciplinaryMeasureModel>("DisciplinaryMeasure/paged");

/** Single case (visibility-scoped) for the standalone form. */
export const getDisciplinaryCase = (id: string) => api.get<DisciplinaryMeasureModel>(`DisciplinaryMeasure/${id}`);

/** HC224/HC225 — promotion/reward eligibility snapshot from active disciplinary measures. */
export const getDisciplinaryEligibility = (employeeId: string) =>
  api.get<DisciplinaryEligibilityModel>(`DisciplinaryMeasure/eligibility?employeeId=${employeeId}`);

export interface ActionResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke JSON save — the form is picker-driven (EmployeePicker + dropdowns + toggles). */
export const saveDisciplinaryCase = async (model: DisciplinaryMeasureModel): Promise<ActionResult> => {
  try {
    const isUpdate = !!model.id;
    const res = await fetch(`${API_BASE_URL}/DisciplinaryMeasure`, {
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
