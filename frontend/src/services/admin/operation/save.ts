import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import { OperationSchema } from "@/components/util/validation";


const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function saveOperationService(formData: FormData) {
  const formDataObj = Object.fromEntries(formData);

  const result = OperationSchema.safeParse({
    ...formDataObj,
    isRelatedToApplication:
      formDataObj.isRelatedToApplication == "true" ? true : false,
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
      // Server-side record has non-nullable Filter/Icon and an int SortOrder.
      filter: (formDataObj.filter as string) || "",
      icon: (formDataObj.icon as string) || "Circle",
      sortOrder: Number(formDataObj.sortOrder || 0),
    };
    if (!isUpdate) {
      delete requestBody.id;
    }

    const response = await fetch(`${API_BASE_URL}/Operation`, {
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
