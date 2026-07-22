import { createPagedQuery } from "@/template/createPagedQuery";
import { createEntityGetById } from "@/template/createEntityGetById";
import { createSaveService } from "@/template/createSaveService";
import { createDeleteService } from "@/template/createDeleteService";
import { TrainingSessionSchema } from "@/components/util/validation";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { TrainingSessionModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const getAllTrainingSessions = createPagedQuery<TrainingSessionModel>("TrainingSession");
export const getTrainingSession = createEntityGetById<TrainingSessionModel>("TrainingSession");
export const saveTrainingSession = createSaveService("TrainingSession", TrainingSessionSchema, {
  numberFields: ["trainerCost", "materialsCost", "venueCost"],
  integerFields: ["maxParticipants"],
});
export const deleteTrainingSession = createDeleteService("TrainingSession");

async function postAction(path: string, body?: unknown): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: "POST",
    credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
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

export const completeTrainingSession = (id: string) => postAction(`TrainingSession/${id}/complete`);
export const cancelTrainingSession = (id: string) => postAction(`TrainingSession/${id}/cancel`);

/** HC197 — materialize a bounded weekly/monthly series from one blueprint. */
export const createTrainingSessionSeries = async (
  model: TrainingSessionModel & { recurrence: string; occurrences: number },
): Promise<{ status: "success" | "error"; message: string }> => {
  try {
    const res = await fetch(`${API_BASE_URL}/TrainingSession/series`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(model),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok) return { status: "error", message: errorMessageParser(parsed.errors || parsed) };
    return { status: "success", message: `Created ${(parsed.ids || []).length} sessions` };
  } catch {
    return { status: "error", message: "Network error" };
  }
};
