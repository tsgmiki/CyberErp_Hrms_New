import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { RatingScaleModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface SaveResult {
  status: "success" | "error";
  message: string;
  id?: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke JSON save (POST create / PUT update) — the payload nests `levels`, which the flat
 * FormData-based `createSaveService` can't carry. Returns the saved id. */
async function jsonSave(resource: string, body: Record<string, unknown>): Promise<SaveResult> {
  const isUpdate = typeof body.id === "string" && body.id !== "";
  try {
    const res = await fetch(`${API_BASE_URL}/${resource}`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok) {
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    }
    return { status: "success", message: "Successfully saved", id: parsed.id, zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}

export const getAllRatingScales = createPagedQuery<RatingScaleModel>("RatingScale");

export const getRatingScale = (id: string) => api.get<RatingScaleModel>(`RatingScale/${id}`);

export const saveRatingScale = (scale: RatingScaleModel) =>
  jsonSave("RatingScale", scale as Record<string, unknown>);

export const deleteRatingScale = createDeleteService("RatingScale");
