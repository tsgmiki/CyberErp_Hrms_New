import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { AppraisalModel, AppraisalPeerReviewModel, PerformanceHistoryModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ActionResult {
  status: "success" | "error";
  message: string;
  id?: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** POST/PUT a JSON body to an appraisal action endpoint and normalize the result. */
async function jsonCall(method: "POST" | "PUT", path: string, body?: Record<string, unknown>): Promise<ActionResult> {
  try {
    const res = await fetch(`${API_BASE_URL}/${path}`, {
      method,
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: body ? JSON.stringify(body) : undefined,
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok) {
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    }
    return { status: "success", message: parsed.message ?? "Success", id: parsed.id, zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}

export const getAllAppraisals = createPagedQuery<AppraisalModel>("Appraisal");

export const getAppraisal = (id: string) => api.get<AppraisalModel>(`Appraisal/${id}`);

export const generateAppraisal = (dto: { employeeId: string; reviewCycleId: string; appraisalTemplateId?: string }) =>
  jsonCall("POST", "Appraisal/generate", dto as Record<string, unknown>);

export interface SaveScoresPayload {
  id: string;
  scope: "Self" | "Manager";
  comments?: string;
  goals: { lineId: string; score: number | null; comments?: string }[];
  competencies: { lineId: string; score: number | null; comments?: string }[];
}

export const saveAppraisalScores = (payload: SaveScoresPayload) =>
  jsonCall("PUT", "Appraisal/score", payload as unknown as Record<string, unknown>);

export const submitAppraisalSelf = (id: string) => jsonCall("POST", `Appraisal/${id}/submit-self`);

export const completeAppraisal = (id: string) => jsonCall("POST", `Appraisal/${id}/complete`);

/** Second-level manager (reviewer) sign-off — approval signature + high-level comments. */
export const reviewerSignOffAppraisal = (dto: { id: string; signature: string; comments?: string }) =>
  jsonCall("POST", `Appraisal/${dto.id}/reviewer-signoff`, dto as Record<string, unknown>);

/** HR final sign-off — closes and locks the appraisal. */
export const hrCloseAppraisal = (dto: { id: string; signature: string }) =>
  jsonCall("POST", `Appraisal/${dto.id}/hr-close`, dto as Record<string, unknown>);

export const deleteAppraisal = createDeleteService("Appraisal");

/* ---- Peer assessment (HC127) ---- */

export const getAppraisalPeers = (appraisalId: string) =>
  api.get<AppraisalPeerReviewModel[]>(`AppraisalPeer?appraisalId=${appraisalId}`);

export const inviteAppraisalPeers = (dto: { appraisalId: string; peerEmployeeIds: string[] }) =>
  jsonCall("POST", "AppraisalPeer/invite", dto as unknown as Record<string, unknown>);

export const submitAppraisalPeer = (dto: { id: string; score: number | null; comments?: string }) =>
  jsonCall("PUT", "AppraisalPeer/submit", dto as unknown as Record<string, unknown>);

export const removeAppraisalPeer = createDeleteService("AppraisalPeer");

/* ---- Version history (HC132) ---- */

export const getPerformanceHistory = (entityType: string, entityId: string) =>
  api.get<PerformanceHistoryModel[]>(`PerformanceHistory?entityType=${entityType}&entityId=${entityId}`);

/* ---- Acknowledgment / signing / appeal (HC142–144) ---- */

export const acknowledgeAppraisal = (dto: { id: string; signature: string }) =>
  jsonCall("POST", "AppraisalSignature/acknowledge", dto as Record<string, unknown>);

export const managerSignAppraisal = (dto: { id: string; signature: string }) =>
  jsonCall("POST", "AppraisalSignature/manager-sign", dto as Record<string, unknown>);

export const submitAppraisalAppeal = (dto: { appraisalId: string; comments: string; requestFollowUp: boolean }) =>
  jsonCall("POST", "AppraisalAppeal", dto as Record<string, unknown>);
