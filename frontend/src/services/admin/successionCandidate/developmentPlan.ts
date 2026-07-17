import type { CreateDevelopmentPlanResultModel } from "@/models";
import { api } from "@/utils/apiClient";

/** HC155/HC156 — turn the successor's competency gap into a structured Individual Development Plan. */
export const createDevelopmentPlanFromSuccession = (successionCandidateId: string) =>
  api.post<CreateDevelopmentPlanResultModel>(`SuccessionCandidate/${successionCandidateId}/create-development-plan`, {});
