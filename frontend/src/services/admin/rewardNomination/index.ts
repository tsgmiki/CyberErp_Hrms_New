import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { RewardNominationModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Role-scoped paged nominations (admin all / manager subtree+raised / employee own-as-nominee). */
export const getAllNominations = createPagedQuery<RewardNominationModel>("RewardNomination");

export const getNomination = (id: string) => api.get<RewardNominationModel>(`RewardNomination/${id}`);

export const deleteNomination = createDeleteService("RewardNomination");

export interface ActionResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke JSON save — the form is picker/dropdown-driven (EmployeePicker + badge/program). */
export const saveNomination = async (model: RewardNominationModel): Promise<ActionResult> => {
  try {
    const isUpdate = !!model.id;
    const res = await fetch(`${API_BASE_URL}/RewardNomination`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(model),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok)
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    return { status: "success", message: parsed.message ?? "Saved successfully", zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
};

async function postAction(path: string): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: "POST",
    credentials: "include",
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors?.id?.[0] ?? parsed?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

/** Direct decisions serve the no-workflow mode; with a running instance the API refuses (use the inbox). */
export const approveNomination = (id: string) => postAction(`RewardNomination/${id}/approve`);
export const rejectNomination = (id: string) => postAction(`RewardNomination/${id}/reject`);
