import type { ReadinessComputationModel } from "@/models";
import { api } from "@/utils/apiClient";
export const computeReadiness = (successionCandidateId: string) =>
  api.post<ReadinessComputationModel>(`SuccessionCandidate/${successionCandidateId}/compute-readiness`, {});
