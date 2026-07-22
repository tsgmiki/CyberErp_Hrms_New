import type { IdentifyHiPosResultModel } from "@/models";
import { api } from "@/utils/apiClient";
export const identifyHipos = (talentReviewId: string) =>
  api.post<IdentifyHiPosResultModel>(`TalentReview/${talentReviewId}/identify-hipos`, {});
