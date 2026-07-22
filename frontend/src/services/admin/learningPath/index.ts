import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { LearningPathModel, LearningPathProgressModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const getAllLearningPaths = createPagedQuery<LearningPathModel>("LearningPath");

export const getLearningPath = (id: string) => api.get<LearningPathModel>(`LearningPath/${id}`);

export const deleteLearningPath = createDeleteService("LearningPath");

/** An employee's completion progress along the path (visibility-gated). */
export const getLearningPathProgress = (id: string, employeeId: string) =>
  api.get<LearningPathProgressModel>(`LearningPath/${id}/progress?employeeId=${employeeId}`);

export interface ActionResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke JSON save — the form carries the ordered steps array. */
export const saveLearningPath = async (
  model: LearningPathModel & { steps: { trainingCourseId?: string; isRequired?: boolean }[] },
): Promise<ActionResult> => {
  try {
    const isUpdate = !!model.id;
    const res = await fetch(`${API_BASE_URL}/LearningPath`, {
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
