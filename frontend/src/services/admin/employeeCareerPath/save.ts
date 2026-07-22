import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { SaveResult } from "@/template/createSaveService";
import type { EmployeeCareerPathModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Bespoke JSON save (the `stepProgress[]` child array). Returns the created/updated id. */
export async function saveEmployeeCareerPath(payload: EmployeeCareerPathModel): Promise<SaveResult & { id?: string }> {
  const isUpdate = !!payload.id;
  try {
    const res = await fetch(`${API_BASE_URL}/EmployeeCareerPath`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok) {
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: {} };
    }
    return { status: "success", message: "Successfully saved", zodErrors: {}, id: parsed?.id ?? payload.id };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
