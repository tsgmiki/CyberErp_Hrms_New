import type { NineBoxModel } from "@/models";
import { api } from "@/utils/apiClient";
export const getNineBox = (talentReviewId: string) =>
  api.get<NineBoxModel>(`TalentReview/${talentReviewId}/nine-box`);
