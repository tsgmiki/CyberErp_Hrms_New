import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import { loginSchema } from "./login.validation";
import { api } from "@/utils/apiClient";
import type { ModuleModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

interface LoginResponse {
  status: string;
  message: string;
  zodErrors: Record<string, string[]>;
  user?: ModuleModel;
}

export default async function loginService(
  formData: FormData,
): Promise<LoginResponse> {
  const formDataObj = Object.fromEntries(formData);

  const result = loginSchema.safeParse(formDataObj);

  if (!result.success) {
    const zodErrors = result.error.flatten().fieldErrors;
    return {
      status: "error",
      message: "Validation failed",
      zodErrors,
    };
  }
  try {
    const response = await fetch(`${API_BASE_URL}/auth/login/cookie`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        userName: formData.get("userName"),
        password: formData.get("password"),
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

    // Try to get user data from login response first
    try {
      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        const userData = await response.json();
        console.log("Login response user data:", userData);
        // Check if response contains user data directly
        if (userData && (userData.id || userData.userId || userData.user)) {
          return {
            status: "success",
            message: "Login successful",
            zodErrors: {},
            user: userData.user || userData,
          };
        }
      }
    } catch (parseError) {
      console.log("Could not parse login response as JSON");
    }

    // If no user data in response, try to get it from loginStatus
    // Wait a bit to ensure cookie is set
    await new Promise((resolve) => setTimeout(resolve, 100));

    try {
      console.log("Fetching loginStatus after login...");
      const userResponse = await api.get<ModuleModel>("auth/loginStatus", {
        skipAuthRedirect: true,
      });
      console.log("loginStatus response:", userResponse);
      return {
        status: "success",
        message: "Login successful",
        zodErrors: {},
        user: userResponse,
      };
    } catch (userError) {
      console.error("Error fetching loginStatus:", userError);
      // If we can't get user data, still return success
      return {
        status: "success",
        message: "Login successful",
        zodErrors: {},
      };
    }
  } catch (error) {
    return {
      status: "error",
      message: "Network error",
      zodErrors: {},
    };
  }
}
