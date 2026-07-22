import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import { SubsystemSchema } from "@/components/util/validation";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function saveSubsystemService(formData: FormData) {
  const formDataObj = Object.fromEntries(formData);

  const result = SubsystemSchema.safeParse({ ...formDataObj });

  if (!result.success) {
    const zodErrors = result.error.flatten().fieldErrors;
    return {
      status: "error",
      message: "Validation failed",
      zodErrors,
    };
  }

  const isUpdate = typeof formDataObj.id !== "undefined" && formDataObj.id !== "" && formDataObj.id !== null;

  try {
    const requestBody: Record<string, unknown> = {
      ...formDataObj,
      sortOrder: Number(formDataObj.sortOrder || 0),
    };
    if (!isUpdate) {
      requestBody.id = "00000000-0000-0000-0000-000000000000";
    }

    const response = await fetch(`${API_BASE_URL}/Subsystem`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(requestBody),
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

    return { status: "success", message: "Successfully saved", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
