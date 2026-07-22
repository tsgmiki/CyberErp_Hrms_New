import { api } from "@/utils/apiClient";
import type { CpdSummaryModel } from "@/models";

/** HC200 — CPD rollup: own record by default; HR/managers may pass an employee in their scope. */
export const getCpdSummary = (employeeId?: string, year?: number) =>
  api.get<CpdSummaryModel>(
    `TrainingCpd?${[employeeId ? `employeeId=${employeeId}` : "", year ? `year=${year}` : ""].filter(Boolean).join("&")}`,
  );
