import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { PagedResult } from "@/template/types";
import type { DynamicFormModel, DynamicFormRecordModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface SaveResult {
  status: "success" | "error";
  message: string;
  id?: string;
  zodErrors: Record<string, string[] | undefined>;
}

/** Bespoke JSON save (POST create / PUT update) — the payload nests `fields`/`data`, which the flat
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

/* ---- Form definitions (Form Builder) ---- */

/** Active forms + their fields for a module — drives dynamic tab rendering. */
export const getActiveForms = (module: string) =>
  api.get<DynamicFormModel[]>(`DynamicForm/active?module=${encodeURIComponent(module)}`);

/** Paged list of forms for the admin Form Builder. */
export const getAllForms = createPagedQuery<DynamicFormModel>("DynamicForm");

export const getForm = (id: string) => api.get<DynamicFormModel>(`DynamicForm/${id}`);

export const saveForm = (form: DynamicFormModel) =>
  jsonSave("DynamicForm", form as Record<string, unknown>);

export const deleteForm = createDeleteService("DynamicForm");

/* ---- Form records (per owner) ---- */

/** Paged records for one owner — the server bounds the fetch + JSON parse to one page. */
export const getRecords = (
  formId: string,
  ownerType: string,
  ownerId: string,
  param: { skip?: number; take?: number },
) =>
  api.get<PagedResult<DynamicFormRecordModel>>(
    `DynamicFormRecord?formId=${formId}&ownerType=${encodeURIComponent(ownerType)}&ownerId=${ownerId}` +
      `&skip=${param.skip ?? 0}&take=${param.take ?? 25}`,
  );

export const saveRecord = (record: DynamicFormRecordModel) =>
  jsonSave("DynamicFormRecord", record as Record<string, unknown>);

export const deleteRecord = createDeleteService("DynamicFormRecord");
