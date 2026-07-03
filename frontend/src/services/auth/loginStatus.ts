import type { ModuleModel } from "@/models";
import { apiClient } from "@/utils/apiClient";

export default async function getLoginStatus(): Promise<ModuleModel | null> {
  try {
    // Use skipAuthRedirect to avoid redirect loops during initial auth check
    const response = await apiClient<ModuleModel>("auth/loginStatus", {
      method: "GET",
      skipAuthRedirect: true,
    });
    return response;
  } catch (error) {
    console.error('Error checking login status:', error);
    return null;
  }
}
