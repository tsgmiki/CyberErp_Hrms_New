import { createPagedQuery } from "@/template/createPagedQuery";
import type { TrainingEnrollmentModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Role-scoped paged enrollments (HR all / manager subtree / employee own). */
export const getAllTrainingEnrollments = createPagedQuery<TrainingEnrollmentModel>("TrainingEnrollment");

async function jsonAction(method: string, path: string, body?: unknown): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method,
    credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message =
      parsed?.errors?.trainingSessionId?.[0] ?? parsed?.errors?.employeeId?.[0] ?? parsed?.errors?.id?.[0] ??
      parsed?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export const enrollTraining = (trainingSessionId: string, employeeId: string, trainingNeedId?: string) =>
  jsonAction("POST", "TrainingEnrollment", { trainingSessionId, employeeId, trainingNeedId });

/** HC198 — attendance/completion/score, recorded by HR or the employee's manager. */
export const recordParticipation = (model: {
  id: string;
  status: string;
  attendancePercent?: number;
  assessmentScore?: number;
  completedOn?: string;
}) => jsonAction("PUT", "TrainingEnrollment/participation", model);

/** HC199 — participant-only effectiveness feedback. */
export const submitTrainingFeedback = (id: string, rating: number, comments?: string) =>
  jsonAction("POST", `TrainingEnrollment/${id}/feedback`, { rating, comments });

export const withdrawEnrollment = (id: string) => jsonAction("POST", `TrainingEnrollment/${id}/withdraw`);
export const deleteEnrollment = (id: string) => jsonAction("DELETE", `TrainingEnrollment/${id}`);
