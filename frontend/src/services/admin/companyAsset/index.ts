import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { CompanyAssetModel, AssetRecoveryModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** HR sees the registry; employees see their own assignments (HC214). */
export const getAllCompanyAssets = createPagedQuery<CompanyAssetModel>("CompanyAsset");

export interface ActionResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

export const saveCompanyAsset = async (model: CompanyAssetModel): Promise<ActionResult> => {
  try {
    const isUpdate = !!model.id;
    const res = await fetch(`${API_BASE_URL}/CompanyAsset`, {
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

async function jsonAction(method: string, path: string, body?: unknown): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method,
    credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = errorMessageParser(parsed.errors || parsed) || parsed?.message || message;
    if (res.ok) message = parsed?.message ?? "Done";
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export const deleteCompanyAsset = (id: string) => jsonAction("DELETE", `CompanyAsset/${id}`);
export const assignCompanyAsset = (id: string, employeeId: string) =>
  jsonAction("POST", `CompanyAsset/${id}/assign/${employeeId}`);
export const returnCompanyAsset = (id: string) => jsonAction("POST", `CompanyAsset/${id}/return`);

/** The exit case's asset-recovery checklist (HC215). */
export const getAssetRecoveries = async (terminationId: string): Promise<AssetRecoveryModel[]> => {
  const res = await fetch(`${API_BASE_URL}/CompanyAsset/recoveries/${terminationId}`, { credentials: "include" });
  if (!res.ok) return [];
  return (await res.json()) as AssetRecoveryModel[];
};

/** Recover (asset back to the pool) or Waive (written off) one checklist item. */
export const resolveAssetRecovery = (id: string, action: "Recover" | "Waive", note?: string) =>
  jsonAction("POST", `CompanyAsset/recoveries/${id}/resolve`, { action, note });
