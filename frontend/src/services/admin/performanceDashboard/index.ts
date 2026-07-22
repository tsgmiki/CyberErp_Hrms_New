import { api } from "@/utils/apiClient";
import type { PerformanceDashboardModel, EmployeePerformanceSummaryModel } from "@/models";

/** Manager / HR dashboard aggregates (HC134), optionally scoped to one review cycle. */
export const getPerformanceDashboard = (reviewCycleId?: string) =>
  api.get<PerformanceDashboardModel>(`PerformanceDashboard${reviewCycleId ? `?reviewCycleId=${reviewCycleId}` : ""}`);

/** Unified per-employee performance summary (HC147). */
export const getEmployeePerformanceSummary = (employeeId: string) =>
  api.get<EmployeePerformanceSummaryModel>(`EmployeePerformanceSummary?employeeId=${employeeId}`);
