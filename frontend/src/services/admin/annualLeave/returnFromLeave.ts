import { api } from "@/utils/apiClient";
import type {
  AnnualLeaveReturnPreviewModel,
  AnnualLeaveReturnResultModel,
  AnnualLeaveHistoryModel,
} from "@/models";

/**
 * What confirming this return date would do — day count, classification, whether it needs a comment
 * and approval. Called as the date changes so the employee sees the consequence BEFORE committing,
 * rather than confirming blind and finding out afterwards.
 */
export const previewAnnualLeaveReturn = (id: string, actualEndDate: string) =>
  api.get<AnnualLeaveReturnPreviewModel>(
    `AnnualLeave/${id}/return-preview?actualEndDate=${encodeURIComponent(actualEndDate)}`,
  );

/** Confirm the return. On time settles immediately; early/late go back through approval. */
export const confirmAnnualLeaveReturn = (body: {
  annualLeaveHeaderId: string;
  actualEndDate: string;
  comment?: string;
}) => api.post<AnnualLeaveReturnResultModel>("AnnualLeave/confirm-return", body);

/** The full lifecycle for the history popup: request, approvals, return, adjustment, settlement. */
export const getAnnualLeaveHistory = (id: string) =>
  api.get<AnnualLeaveHistoryModel>(`AnnualLeave/${id}/history`);
