import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { TrainingNeedModel, TrainingNeedSuggestionModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Role-scoped paged needs (HR all / manager subtree+raised / employee own). */
export const getAllTrainingNeeds = createPagedQuery<TrainingNeedModel>("TrainingNeed");

export const getTrainingNeed = (id: string) => api.get<TrainingNeedModel>(`TrainingNeed/${id}`);

export const deleteTrainingNeed = createDeleteService("TrainingNeed");

/** HC189 — performance-driven suggestions (competency gaps, weak results, active goals). */
export const getTrainingNeedSuggestions = (employeeId: string) =>
  api.get<TrainingNeedSuggestionModel[]>(`TrainingNeed/suggestions?employeeId=${employeeId}`);

export interface ActionResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke JSON save — the form is picker/dropdown-driven. */
export const saveTrainingNeed = async (model: TrainingNeedModel): Promise<ActionResult> => {
  try {
    const isUpdate = !!model.id;
    const res = await fetch(`${API_BASE_URL}/TrainingNeed`, {
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

async function postAction(path: string): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: "POST",
    credentials: "include",
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors?.id?.[0] ?? parsed?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

/** Direct decisions serve the no-workflow mode; with a running instance the API refuses (use the inbox). */
export const approveTrainingNeed = (id: string) => postAction(`TrainingNeed/${id}/approve`);
export const rejectTrainingNeed = (id: string) => postAction(`TrainingNeed/${id}/reject`);
export const cancelTrainingNeed = (id: string) => postAction(`TrainingNeed/${id}/cancel`);
