import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import { EmployeeSchema } from "@/components/util/validation";
import type { EmployeeModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface EmployeeSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
  /** Saved employee id (from the create response, or the submitted id on update). */
  id?: string;
}

/**
 * Bespoke employee save: the profile form submits a rich object (master fields + dynamic
 * customFields map, HC021), so it posts JSON built from component state rather than FormData.
 */
export default async function saveEmployee(
  data: EmployeeModel & { customFields?: Record<string, string | null> },
): Promise<EmployeeSaveResult> {
  const result = EmployeeSchema.safeParse(data);
  if (!result.success) {
    return {
      status: "error",
      message: "Validation failed",
      zodErrors: result.error.flatten().fieldErrors,
    };
  }

  const isUpdate = !!data.id;
  const body: Record<string, unknown> = { ...data };
  if (!isUpdate) delete body.id;
  // Drop empty optional strings so nullable columns/Guids bind as null.
  for (const key of Object.keys(body)) {
    if (body[key] === "") delete body[key];
  }
  // The probation flag comes off a Yes/No dropdown as "true"/"false"; JSON must send a real boolean.
  body.isProbation = body.isProbation === true || body.isProbation === "true";
  // IsTerminated is controlled by the termination flow, never by the profile form.
  delete body.isTerminated;
  // Job grade is derived from the salary scale — the dropdown is a client-side filter only, never
  // stored on the employee. These display/filter-only fields are not sent to the API.
  delete body.jobGradeId;
  delete body.jobGradeName;
  delete body.salaryScaleStep;
  delete body.salaryScaleAmount;

  try {
    const response = await fetch(`${API_BASE_URL}/Employee`, {
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

    // Create returns the new Guid as a JSON string; update returns a message object.
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
