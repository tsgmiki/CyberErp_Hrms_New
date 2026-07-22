import { api } from "@/utils/apiClient";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { PositionCompetencyModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface SaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

export interface SavePositionCompetenciesPayload {
  positionId: string;
  items: { competencyId: string; weight: number }[];
}

/** The competencies assigned to a position (join-resolved names + weights). */
export const getPositionCompetencies = (positionId: string) =>
  api.get<PositionCompetencyModel[]>(`PositionCompetency?positionId=${positionId}`);

/** Replace-set save — POST the whole list for the position; the server swaps the assignments atomically. */
export async function savePositionCompetencies(
  payload: SavePositionCompetenciesPayload,
): Promise<SaveResult> {
  try {
    const res = await fetch(`${API_BASE_URL}/PositionCompetency`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok) {
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    }
    return { status: "success", message: "Successfully saved", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
