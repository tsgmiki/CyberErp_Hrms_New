import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import { ModuleSchema } from "@/components/util/validation";


const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function saveModuleService(formData: FormData) {
  const formDataObj = Object.fromEntries(formData);

  const result = ModuleSchema.safeParse({
    ...formDataObj,
  });

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
      // SortOrder binds to an int server-side; FormData always yields strings.
      sortOrder: Number(formDataObj.sortOrder || 0),
    };
    if (!isUpdate) {
      delete requestBody.id;
    }

    const response = await fetch(`${API_BASE_URL}/Module`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(requestBody),
    });

    if (!response.ok) {
      const text = await response.text();
      const result = isValidJson(text) ? JSON.parse(text) : { message: text };
      const message = errorMessageParser(result.errors || result);
      return {
        status: "error",
        message,
        zodErrors: {},
      };
    }

    return {
      status: "success",
      message: "Successfully saved",
      zodErrors: {},
    };
  } catch (error) {
    return {
      status: "error",
      message: "Network error",
      zodErrors: {},
    };
  }
}
