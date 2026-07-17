import type { CareerPathUtilizationModel } from "@/models";
import { api } from "@/utils/apiClient";
export const getCareerPathUtilization = () =>
  api.get<CareerPathUtilizationModel[]>("CareerPath/analytics/utilization");
