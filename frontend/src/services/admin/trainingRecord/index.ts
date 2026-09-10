import { api } from "@/utils/apiClient";
import type { SignableRecordModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** One training record with its signatures, as HR sees it. */
export const getTrainingRecord = (trainingEnrollmentId: string) =>
  api.get<SignableRecordModel>(`TrainingRecord/${encodeURIComponent(trainingEnrollmentId)}`);

/**
 * The verifier's counter-signature.
 *
 * ⚠️ The password is re-entered at the moment of signing — a verification is an assertion by a named
 * person, and a session cookie alone does not say who is at the keyboard. Sent once, never stored.
 */
export const verifyTrainingRecord = (
  trainingEnrollmentId: string,
  password: string,
  note?: string,
) => api.post<SignableRecordModel>("TrainingRecord/verify", { trainingEnrollmentId, password, note });

/**
 * Downloads the employee's full training record as a PDF.
 *
 * Fetched with credentials rather than linked directly, so the session cookie rides on it and the
 * server's filename is used.
 */
export async function downloadTrainingRecord(employeeId: string, employeeName: string): Promise<boolean> {
  const res = await fetch(`${API_BASE_URL}/TrainingRecord/${employeeId}/document`, {
    credentials: "include",
  });
  if (!res.ok) return false;
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `training-record-${employeeName.replace(/[^A-Za-z0-9]+/g, "-").toLowerCase()}.pdf`;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
  return true;
}
