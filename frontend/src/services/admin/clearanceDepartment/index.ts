import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { ClearanceDepartmentModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const getAllClearanceDepartments =
  createPagedQuery<ClearanceDepartmentModel>("ClearanceDepartment");

export const getClearanceDepartment = (id: string) =>
  api.get<ClearanceDepartmentModel>(`ClearanceDepartment/${id}`);

export const deleteClearanceDepartment = createDeleteService("ClearanceDepartment");

export interface ClearanceDepartmentSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke save: the department carries a nested approvers array, so it posts JSON from state. */
export async function saveClearanceDepartment(
  data: ClearanceDepartmentModel,
): Promise<ClearanceDepartmentSaveResult> {
  if (!data.name?.trim())
    return { status: "error", message: "Validation failed", zodErrors: { name: ["Name is required"] } };
  if (!data.description?.trim())
    return {
      status: "error",
      message: "Validation failed",
      zodErrors: { description: ["Describe what this department must clear"] },
    };

  const isUpdate = !!data.id;
  const body: Record<string, unknown> = {
    ...data,
    sortOrder: Number(data.sortOrder) || 0,
    isActive: data.isActive === true || String(data.isActive) === "true",
    approvers: data.approvers ?? [],
  };
  if (!isUpdate) delete body.id;

  try {
    const response = await fetch(`${API_BASE_URL}/ClearanceDepartment`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
    if (!response.ok) {
      const text = await response.text();
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: {} };
    }
    return { status: "success", message: "Successfully saved", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
