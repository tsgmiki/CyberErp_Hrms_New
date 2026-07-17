import type { CompetencyGapModel } from "@/models";
import { api } from "@/utils/apiClient";
export const getCompetencyGap = (successionCandidateId: string) =>
  api.get<CompetencyGapModel>(`SuccessionCandidate/${successionCandidateId}/gap`);
