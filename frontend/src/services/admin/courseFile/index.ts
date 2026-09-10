import { api } from "@/utils/apiClient";
import type { CourseFileModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** The course's material library. Carries no bytes — only what the picker needs to show. */
export const getCourseFiles = (trainingCourseId: string) =>
  api.get<CourseFileModel[]>(
    `CourseFile?trainingCourseId=${encodeURIComponent(trainingCourseId)}`,
  );

/**
 * Adds a file to the library.
 *
 * Raw fetch rather than the shared client, because that one JSON-stringifies its body — a multipart
 * upload has to go out as FormData with the boundary the browser sets.
 */
export async function uploadCourseFile(
  trainingCourseId: string,
  file: File,
  description?: string,
): Promise<{ ok: boolean; message: string }> {
  const form = new FormData();
  form.append("trainingCourseId", trainingCourseId);
  if (description) form.append("description", description);
  form.append("file", file);

  const res = await fetch(`${API_BASE_URL}/CourseFile`, {
    method: "POST",
    credentials: "include",
    body: form,
  });
  const text = await res.text();
  // The server names the actual problem ("File must be between 1 byte and 25 MB.") — far more use
  // to an author than a generic failure.
  let message = "File uploaded";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors?.file?.[0] ?? parsed?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export async function deleteCourseFile(id: string): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/CourseFile/${id}`, {
    method: "DELETE",
    credentials: "include",
  });
  const text = await res.text();
  let message = "Deleted";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors?.id?.[0] ?? parsed?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

/** The author's preview. Fetched with credentials, then handed to the browser as a download. */
export async function previewCourseFile(id: string, fileName: string): Promise<void> {
  const res = await fetch(`${API_BASE_URL}/CourseFile/${id}/download`, { credentials: "include" });
  if (!res.ok) return;
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}
