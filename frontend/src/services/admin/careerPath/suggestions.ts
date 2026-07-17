import type { CareerPathSuggestionModel } from "@/models";
import { api } from "@/utils/apiClient";
export const getCareerPathSuggestions = (employeeId: string) =>
  api.get<CareerPathSuggestionModel[]>(`CareerPath/suggestions?employeeId=${employeeId}`);
