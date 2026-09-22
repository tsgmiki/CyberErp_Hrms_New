import { api } from "@/utils/apiClient";

/** One aggregated read for the dashboard's KPI row — see backend IDashboardSummary. */
export interface DashboardSummaryModel {
  branchCount: number;
  organizationUnitCount: number;
  positionCount: number;
  employeeCount: number;
  workflowRunning: number;
  workflowApproved: number;
  workflowRejected: number;
  probationCount: number;
  retirementCount: number;
  /** Approved leave, annual and other, whose last day has passed with no recorded return. */
  overdueLeaveReturnCount: number;
}

export default function getDashboardSummary(): Promise<DashboardSummaryModel> {
  return api.get<DashboardSummaryModel>("Dashboard/summary");
}
