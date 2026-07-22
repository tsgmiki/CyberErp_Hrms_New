import type { DevelopmentRecommendationModel, CreateDevelopmentGoalsResultModel } from "@/models";
import { api } from "@/utils/apiClient";

/** HC164 — competencies to develop for the employee's next career step. */
export const getRecommendations = (employeeCareerPathId: string) =>
  api.get<DevelopmentRecommendationModel>(`EmployeeCareerPath/${employeeCareerPathId}/recommendations`);

/** HC167 — materialise the gap as development goals on the performance engine, aligned to an org objective. */
export const createDevelopmentGoals = (
  employeeCareerPathId: string,
  reviewCycleId?: string,
  organizationalObjectiveId?: string,
) => {
  const qs = new URLSearchParams();
  if (reviewCycleId) qs.set("reviewCycleId", reviewCycleId);
  if (organizationalObjectiveId) qs.set("organizationalObjectiveId", organizationalObjectiveId);
  const q = qs.toString();
  return api.post<CreateDevelopmentGoalsResultModel>(
    `EmployeeCareerPath/${employeeCareerPathId}/create-goals${q ? `?${q}` : ""}`,
    {},
  );
};
