import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  WorkflowInstanceModel,
  WorkflowDefinitionModel,
  WorkflowActionModel,
  WorkflowStatsModel,
} from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/* ---- Instances (tracking) -------------------------------------------------- */

export const getAllWorkflows = createPagedQuery<WorkflowInstanceModel>("Workflow");

export const getWorkflowStats = () => api.get<WorkflowStatsModel>("Workflow/stats");

export const getWorkflowActions = (id: string) =>
  api.get<WorkflowActionModel[]>(`Workflow/${id}/actions`);

async function decide(id: string, verb: "approve" | "reject", comment?: string) {
  const res = await fetch(`${API_BASE_URL}/Workflow/${id}/${verb}`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ comment: comment || null }),
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export const approveWorkflow = (id: string, comment?: string) => decide(id, "approve", comment);
export const rejectWorkflow = (id: string, comment?: string) => decide(id, "reject", comment);

/* ---- Definitions (admin configuration) ------------------------------------- */

export const getAllWorkflowDefinitions =
  createPagedQuery<WorkflowDefinitionModel>("WorkflowDefinition");

export const getWorkflowDefinition = (id: string) =>
  api.get<WorkflowDefinitionModel>(`WorkflowDefinition/${id}`);

export const deleteWorkflowDefinition = createDeleteService("WorkflowDefinition");

export interface WorkflowDefinitionSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke save: the definition carries a nested steps array, so it posts JSON from state. */
export async function saveWorkflowDefinition(
  data: WorkflowDefinitionModel,
): Promise<WorkflowDefinitionSaveResult> {
  if (!data.name?.trim())
    return { status: "error", message: "Validation failed", zodErrors: { name: ["Name is required"] } };
  if (!data.entityType?.trim())
    return { status: "error", message: "Validation failed", zodErrors: { entityType: ["Process is required"] } };
  if (!data.steps || data.steps.length === 0)
    return { status: "error", message: "Validation failed", zodErrors: { steps: ["At least one step is required"] } };
  if (data.steps.some((s) => !s.name?.trim()))
    return { status: "error", message: "Validation failed", zodErrors: { steps: ["Every step needs a name"] } };

  const isUpdate = !!data.id;
  const body: Record<string, unknown> = {
    ...data,
    isActive: data.isActive === true || String(data.isActive) === "true",
  };
  if (!isUpdate) delete body.id;

  try {
    const response = await fetch(`${API_BASE_URL}/WorkflowDefinition`, {
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

export async function seedDefaultWorkflows(): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/WorkflowDefinition/seed-defaults`, {
    method: "POST",
    credentials: "include",
  });
  const text = await res.text();
  let message = "Defaults created";
  try {
    message = JSON.parse(text)?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}
