import type { SuccessionCandidateProfileModel } from "@/models";
import { api } from "@/utils/apiClient";
export const getCandidateProfile = (successionCandidateId: string) =>
  api.get<SuccessionCandidateProfileModel>(`SuccessionCandidate/${successionCandidateId}/profile`);
