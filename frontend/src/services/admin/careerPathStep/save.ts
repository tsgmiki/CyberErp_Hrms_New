import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { SaveResult } from "@/template/createSaveService";
import type { CareerPathStepModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Bespoke JSON save (the `competencies[]` child array). */
export async function saveCareerPathStep(payload: CareerPathStepModel): Promise<SaveResult> {
  const isUpdate = !!payload.id;
  try {
    const res = await fetch(`${API_BASE_URL}/CareerPathStep`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!res.ok) {
      const text = await res.text();
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: {} };
    }
    return { status: "success", message: "Successfully saved", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
