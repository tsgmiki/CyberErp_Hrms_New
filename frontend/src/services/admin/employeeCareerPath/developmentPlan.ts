import type { CreateDevelopmentPlanResultModel } from "@/models";
import { api } from "@/utils/apiClient";

/** HC130 — turn the next-step competency gap into a structured Individual Development Plan. */
export const createDevelopmentPlanFromCareerPath = (employeeCareerPathId: string) =>
  api.post<CreateDevelopmentPlanResultModel>(`EmployeeCareerPath/${employeeCareerPathId}/create-development-plan`, {});
