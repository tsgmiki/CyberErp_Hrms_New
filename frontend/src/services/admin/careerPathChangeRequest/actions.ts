import { api } from "@/utils/apiClient";

/** HC169 — light approval-flow transitions on a change request. */
export const submitChangeRequest = (id: string) =>
  api.post(`CareerPathChangeRequest/${id}/submit`, {});
export const approveChangeRequest = (id: string, decisionNotes?: string) =>
  api.post(`CareerPathChangeRequest/${id}/approve`, { decisionNotes });
export const rejectChangeRequest = (id: string, decisionNotes?: string) =>
  api.post(`CareerPathChangeRequest/${id}/reject`, { decisionNotes });
