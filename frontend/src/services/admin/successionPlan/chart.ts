import type { SuccessionChartModel } from "@/models";
import { api } from "@/utils/apiClient";
export const getSuccessionChart = (successionPlanId: string) =>
  api.get<SuccessionChartModel>(`SuccessionPlan/${successionPlanId}/chart`);
