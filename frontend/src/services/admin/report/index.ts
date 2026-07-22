import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  ReportDefinitionModel,
  ReportCatalogGroupModel,
  ReportSchemaModel,
  ReportLookupOptionModel,
  ReportResultModel,
} from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Seeds the standard HRMS report catalog for the current tenant (idempotent; mirrors menu seeding). */
export async function seedDefaultReports(): Promise<{ ok: boolean; message: string }> {
  const response = await fetch(`${API_BASE_URL}/Report/seed-defaults`, {
    method: "POST",
    credentials: "include",
  });
  const text = await response.text();
  const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
  if (!response.ok) {
    return { ok: false, message: errorMessageParser(parsed.errors || parsed) || "Seeding failed" };
  }
  return { ok: true, message: parsed?.message ?? "Standard reports seeded" };
}

// ---- Viewer -----------------------------------------------------------------

/** Active reports grouped by category (the report menu). */
export const getReportCatalog = () => api.get<ReportCatalogGroupModel[]>("Report/catalog");

/** A report's dynamic parameter schema (choice fields arrive with preloaded options). */
export const getReportSchema = (reportKey: string) =>
  api.get<ReportSchemaModel>(`Report/${reportKey}/schema`);

/** Lookup options for one parameter (cascading via `dependency`, server search via `search`). */
export const getReportFieldValues = (
  reportKey: string,
  field: string,
  dependency?: string,
  search?: string,
) => {
  const q = new URLSearchParams({ field });
  if (dependency) q.append("dependency", dependency);
  if (search) q.append("search", search);
  return api.get<ReportLookupOptionModel[]>(`Report/${reportKey}/field-values?${q.toString()}`);
};

export interface GenerateReportResult {
  ok: boolean;
  message?: string;
  result?: ReportResultModel;
}

/** One customized output column (reference pReportFieldOutput Field■Label■order■sortOrder). */
export interface ReportOutputFieldInput {
  field: string;
  label: string;
  order: number;
  sortOrder: number;
}

/** Runs the report: dynamic columns + rows straight from its stored procedure. */
export async function generateReport(
  reportKey: string,
  values: Record<string, string>,
  outputFields?: ReportOutputFieldInput[],
): Promise<GenerateReportResult> {
  try {
    const response = await fetch(`${API_BASE_URL}/Report/generate`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ reportKey, values, outputFields }),
    });
    const text = await response.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : null;
    if (!response.ok)
      return { ok: false, message: errorMessageParser(parsed?.errors || parsed) || "Failed to generate report" };
    return { ok: true, result: parsed as ReportResultModel };
  } catch {
    return { ok: false, message: "Network error" };
  }
}

// ---- Saved filters + history ---------------------------------------------------

export interface SavedFilterDetail {
  id: string;
  reportKey: string;
  name: string;
  values: Record<string, string | null>;
  outputFields?: string[] | null;
}

export interface ReportRunModel {
  reportKey: string;
  ranBy?: string;
  ranAt: string;
  rowCount: number;
  durationMs: number;
}

export const saveReportFilter = (reportKey: string, name: string, values: Record<string, string>, outputFields?: string[]) =>
  api.post<string>("Report/filters", { reportKey, name, values, outputFields });

export const getReportFilter = (id: string) => api.get<SavedFilterDetail>(`Report/filters/${id}`);

export const deleteReportFilter = (id: string) => api.delete(`Report/filters/${id}`);

export interface ScheduleOutputFieldInput {
  field: string;
  label: string;
  fieldOrder: number;
  sortOrder: number;
}

/** The reference schedule cadence payload (Frequency + weekly bitmask + hour → server derives cron). */
export interface SaveReportScheduleInput {
  id?: string;
  reportKey: string;
  name: string;
  isScheduled: boolean;
  mailSubject?: string;
  mailBody?: string;
  isHideRecipients: boolean;
  frequency: string;
  frequencyWeekly: number;
  hour24: number;
  scheduleStartDate?: string;
  outputFormat: number;
  recipientUserIds: string[];
  recipientRoleIds: string[];
  recipientEmails: string[];
  values: Record<string, string>;
  outputFields?: ScheduleOutputFieldInput[];
}

export const saveReportSchedule = (input: SaveReportScheduleInput) =>
  api.post<string>("Report/schedules", input);

/** The reference _SendReportByEmail payload: multi-user + multi-role dispatch, CC me, output format. */
export interface EmailReportInput {
  reportKey: string;
  recipientUserIds: string[];
  recipientRoleIds: string[];
  recipientEmails: string[];
  isCc: boolean;
  isHideRecipients: boolean;
  subject: string;
  body: string;
  outputFormat: number;
  values: Record<string, string>;
  outputFields?: ReportOutputFieldInput[];
}

export const emailReport = async (input: EmailReportInput) => {
  const res = await fetch(`${API_BASE_URL}/Report/email`, {
    method: "POST", credentials: "include", headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  const text = await res.text();
  const parsed = isValidJson(text) ? JSON.parse(text) : null;
  if (!res.ok) throw new Error(errorMessageParser(parsed?.errors || parsed) || "Failed to send");
  return parsed as { rows: number; recipients: string; sent: boolean };
};

export interface ReportScheduleItem {
  id: string;
  reportKey: string;
  name: string;
  frequency: string;
  cronExpression: string;
  timeOfTheDay: number;
  scheduleStartDate?: string | null;
  isActive: boolean;
  isScheduled: boolean;
}

export type ReportScheduleDetail = SaveReportScheduleInput;

export const getReportSchedules = (reportKey: string) =>
  api.get<ReportScheduleItem[]>(`Report/schedules?reportKey=${encodeURIComponent(reportKey)}`);

export const getReportScheduleDetail = (id: string) =>
  api.get<ReportScheduleDetail>(`Report/schedules/${id}`);

export const setReportScheduleEnabled = (id: string, enabled: boolean) =>
  api.post(`Report/schedules/${id}/enable?enabled=${enabled}`, {});

export const runReportScheduleNow = (id: string) =>
  api.post<{ rows: number; recipients: string; sent: boolean }>(`Report/schedules/${id}/run-now`, {});

export const deleteReportSchedule = (id: string) => api.delete(`Report/schedules/${id}`);

export const setReportRestrictions = (reportKey: string, roleIds: string[]) =>
  api.post('Report/restrictions', { reportKey, roleIds });

export const getReportHistory = (reportKey?: string) =>
  api.get<ReportRunModel[]>(`Report/history${reportKey ? `?reportKey=${encodeURIComponent(reportKey)}` : ""}`);

// ---- Admin registry -----------------------------------------------------------

export const getAllReports = createPagedQuery<ReportDefinitionModel>("Report");
export const getReport = (id: string) => api.get<ReportDefinitionModel>(`Report/${id}`);
export const deleteReport = createDeleteService("Report");

export interface ReportSaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
  id?: string;
}

/** Bespoke save: the registry row carries a nested fields grid, so it posts JSON from state. */
export async function saveReport(data: ReportDefinitionModel): Promise<ReportSaveResult> {
  if (!data.reportKey?.trim())
    return { status: "error", message: "Validation failed", zodErrors: { reportKey: ["Report key is required"] } };
  if (!data.reportName?.trim())
    return { status: "error", message: "Validation failed", zodErrors: { reportName: ["Report name is required"] } };
  if (!data.storedProc?.trim())
    return { status: "error", message: "Validation failed", zodErrors: { storedProc: ["Stored procedure is required"] } };

  const isUpdate = !!data.id;
  const body: Record<string, unknown> = {
    ...data,
    sortOrder: Number(data.sortOrder) || 0,
    fields: (data.fields ?? []).map((f, i) => ({
      field: f.field,
      label: f.label,
      dataType: f.dataType || "Text",
      fieldOrder: f.fieldOrder ?? i + 1,
      dependencyField: f.dependencyField || null,
    })),
    fieldOutputs: (data.fieldOutputs ?? []).map((o, i) => ({
      field: o.field,
      label: o.label,
      fieldOrder: o.fieldOrder ?? i + 1,
    })),
  };
  if (!isUpdate) delete body.id;

  try {
    const response = await fetch(`${API_BASE_URL}/Report`, {
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
    let savedId = data.id;
    if (!isUpdate) {
      const text = await response.text();
      savedId = isValidJson(text) ? JSON.parse(text) : text.replace(/"/g, "");
    }
    return { status: "success", message: "Successfully saved", zodErrors: {}, id: savedId };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}
