import type { EmployeeDevelopmentProfileModel } from "@/models";
import { api } from "@/utils/apiClient";

/** HC158 — the employee's holistic Performance ↔ Career Development view. */
export const getEmployeeDevelopmentProfile = (employeeId: string) =>
  api.get<EmployeeDevelopmentProfileModel>(`EmployeeDevelopment/${employeeId}/profile`);
