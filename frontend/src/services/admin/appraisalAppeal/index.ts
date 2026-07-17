import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { AppraisalAppealModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ActionResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

async function jsonCall(method: "POST", path: string, body?: Record<string, unknown>): Promise<ActionResult> {
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
    return { status: "success", message: parsed.message ?? "Success", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}

export const getAllAppraisalAppeals = createPagedQuery<AppraisalAppealModel>("AppraisalAppeal");

export const getAppraisalAppeal = (id: string) => api.get<AppraisalAppealModel>(`AppraisalAppeal/${id}`);

export const startAppraisalAppealReview = (id: string) => jsonCall("POST", `AppraisalAppeal/${id}/start-review`);

export const resolveAppraisalAppeal = (dto: { id: string; upheld: boolean; resolution: string }) =>
  jsonCall("POST", "AppraisalAppeal/resolve", dto as Record<string, unknown>);
