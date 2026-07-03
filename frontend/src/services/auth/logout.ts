import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function LogoutService() {
  try {
    const response = await fetch(API_BASE_URL + "/auth/logout/cookie", {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({}),
    });

    if (!response.ok) {
      const text = await response.text();
      const result = isValidJson(text) ? JSON.parse(text) : { message: text };
      const message = errorMessageParser(result);
      return {
        status: "error",
        message,
        zodErrors: {},
      };
    }
    return {
      status: "success",
      message: "Logout successful",
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
