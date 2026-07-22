import type { CareerPathVisualizeModel } from "@/models";
import { api } from "@/utils/apiClient";
export const getCareerPathVisualize = (careerPathId: string, employeeId?: string) =>
  api.get<CareerPathVisualizeModel>(
    `CareerPath/${careerPathId}/visualize${employeeId ? `?employeeId=${employeeId}` : ""}`,
  );
