import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import { registerSchema } from "./register.validation";
import type { RegisterFormData } from "./register.validation";
import type RegisterUserModel from "@/models/masters/RegisterUserModel";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function registerService(formData: FormData) {
  const formDataObj = Object.fromEntries(formData);
  const result = registerSchema.safeParse(formDataObj);

  if (!result.success) {
    const zodErrors = result.error.flatten().fieldErrors;
     return {
      status: "error",
      message: "Validation failed",
      zodErrors,
    };
  }
  try {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        ...formDataObj,
      }),
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

    const data = await response.json();

    return {
      status: "success",
      message: "Registration successful",
      data,
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

export type { RegisterUserModel, RegisterFormData };
