import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import { DocumentTemplateSchema } from "@/components/util/validation";
import type { DocumentTemplateModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface DocumentTemplateSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
  id?: string;
}

/**
 * Bespoke save: the template body is authored in a rich-text editor (not a native form input),
 * so the form posts a JSON object built from component state rather than FormData.
 */
export default async function saveDocumentTemplate(
  data: DocumentTemplateModel,
): Promise<DocumentTemplateSaveResult> {
  const result = DocumentTemplateSchema.safeParse(data);
  if (!result.success) {
    return {
      status: "error",
      message: "Validation failed",
      zodErrors: result.error.flatten().fieldErrors,
    };
  }

  const isUpdate = !!data.id;
  const body: Record<string, unknown> = {
    ...data,
    // isActive arrives as "true"/"false" from the dropDown; coerce to a real boolean.
    isActive: data.isActive === true || String(data.isActive) === "true",
  };
  if (!isUpdate) delete body.id;
  for (const key of Object.keys(body)) {
    if (body[key] === "") delete body[key];
  }

  try {
    const response = await fetch(`${API_BASE_URL}/DocumentTemplate`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      const text = await response.text();
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      return {
        status: "error",
        message: errorMessageParser(parsed.errors || parsed),
        zodErrors: {},
      };
    }

    let savedId = data.id;
    if (!isUpdate) {
      const text = await response.text();
      savedId = isValidJson(text) ? JSON.parse(text) : text.replace(/"/g, "");
    }
    return { status: "success", message: "Successfully saved", zodErrors: {}, id: savedId };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
