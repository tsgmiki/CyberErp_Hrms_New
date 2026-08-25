import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { NotificationTemplateModel, NotificationEventModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ActionResult { ok: boolean; message: string; id?: string }

async function action(method: string, path: string, body?: unknown): Promise<ActionResult> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method,
    credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
  if (!res.ok) return { ok: false, message: errorMessageParser(parsed.errors || parsed) || "Request failed" };
  return { ok: true, message: parsed?.message ?? "Saved successfully", id: parsed?.id };
}

export const getAllNotificationTemplates =
  createPagedQuery<NotificationTemplateModel>("NotificationTemplate");

export const getNotificationTemplate = (id: string) =>
  api.get<NotificationTemplateModel>(`NotificationTemplate/${id}`);

/**
 * The event catalogue. Drives the event picker AND the token palette — an admin should never have to
 * guess which {{tokens}} an event publishes, because one it does not publish merges to blank.
 */
export const getNotificationEvents = () =>
  api.get<NotificationEventModel[]>("NotificationTemplate/events");

/** Template and its recipient rules save together — they are one thing to an administrator. */
export const saveNotificationTemplate = (m: NotificationTemplateModel) =>
  action(m.id ? "PUT" : "POST", "NotificationTemplate", m);

export const deleteNotificationTemplate = createDeleteService("NotificationTemplate");

/** Loads the code's event catalogue into this tenant. Idempotent; removes nothing. */
export const seedNotificationEvents = () =>
  action("POST", "NotificationTemplate/seed-defaults");
