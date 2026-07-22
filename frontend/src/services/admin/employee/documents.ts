import { api } from "@/utils/apiClient";
import type { EmployeeDocumentModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export type DocumentOwnerType = "Education" | "Experience" | "DynamicFormRecord";

/** Documents attached to an owner record. `ownerField` sub-scopes to a dynamic-form Attachment field
 * (each field is its own file pool); omit it for education/experience. */
export const getDocuments = (ownerType: DocumentOwnerType, ownerId: string, ownerField?: string) =>
  api.get<EmployeeDocumentModel[]>(
    `EmployeeDocument?ownerType=${ownerType}&ownerId=${ownerId}` +
      (ownerField ? `&ownerField=${encodeURIComponent(ownerField)}` : ""),
  );

export async function uploadDocument(
  employeeId: string,
  ownerType: DocumentOwnerType,
  ownerId: string,
  file: File,
  ownerField?: string,
): Promise<{ ok: boolean; message: string }> {
  const form = new FormData();
  form.append("employeeId", employeeId);
  form.append("ownerType", ownerType);
  form.append("ownerId", ownerId);
  if (ownerField) form.append("ownerField", ownerField);
  form.append("file", file);

  const res = await fetch(`${API_BASE_URL}/EmployeeDocument`, {
    method: "POST",
    credentials: "include",
    body: form,
  });
  const text = await res.text();
  let message = "Document uploaded";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors?.file?.[0] ?? parsed?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export async function deleteDocument(id: string): Promise<boolean> {
  const res = await fetch(`${API_BASE_URL}/EmployeeDocument/${id}`, {
    method: "DELETE",
    credentials: "include",
  });
  return res.ok;
}

/** Authenticated download URL (cookies ride along on navigation). */
export const documentDownloadUrl = (id: string) =>
  `${API_BASE_URL}/EmployeeDocument/${id}/download`;

/** Fetches the file with credentials and triggers a browser download with its filename. */
export async function downloadDocument(id: string, fileName: string): Promise<void> {
  const res = await fetch(documentDownloadUrl(id), { credentials: "include" });
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
