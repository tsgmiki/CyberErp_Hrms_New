import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { CalibrationSessionModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ActionResult {
  status: "success" | "error";
  message: string;
  id?: string;
  zodErrors: Record<string, string[] | undefined>;
}

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

export const getAllCalibrationSessions = createPagedQuery<CalibrationSessionModel>("CalibrationSession");

export const getCalibrationSession = (id: string) => api.get<CalibrationSessionModel>(`CalibrationSession/${id}`);

export const createCalibrationSession = (dto: { name: string; reviewCycleId: string; organizationUnitId?: string; notes?: string }) =>
  jsonCall("POST", "CalibrationSession", dto as Record<string, unknown>);

export const saveCalibrationItem = (dto: { itemId: string; calibratedScore: number | null; justification?: string }) =>
  jsonCall("PUT", "CalibrationSession/item", dto as unknown as Record<string, unknown>);

export const finalizeCalibrationSession = (id: string) => jsonCall("POST", `CalibrationSession/${id}/finalize`);

export const deleteCalibrationSession = createDeleteService("CalibrationSession");
