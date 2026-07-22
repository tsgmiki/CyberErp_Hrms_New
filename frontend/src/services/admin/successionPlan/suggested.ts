import { api } from "@/utils/apiClient";
import type { SuggestedSuccessorModel } from "@/models";

/** Talent-review HiPos not yet on this plan — the Talent Review → Succession hand-off. */
export const getSuggestedSuccessors = (successionPlanId: string) =>
  api.get<SuggestedSuccessorModel[]>(`SuccessionPlan/${successionPlanId}/suggested-candidates`);

export default getSuggestedSuccessors;
