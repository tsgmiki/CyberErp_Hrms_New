import type { BadgeVariant } from "./badge";

const successStatuses = new Set([
  "done",
  "completed",
  "complete",
  "approved",
  "active",
  "enabled",
  "success",
  "paid",
  "cash",
  "yes",
  "true",
  "in stock",
  "closed",
  "current",
  "compliant",
  "high",
]);

const warningStatuses = new Set([
  "pending",
  "draft",
  "in progress",
  "inprogress",
  "processing",
  "credit",
  "partial",
  "low",
  "waiting",
  "submitted",
  "on hold",
  "due soon",
  "pending audit",
  "medium",
  "under review",
]);

const errorStatuses = new Set([
  "posted",
  "rejected",
  "cancelled",
  "canceled",
  "failed",
  "error",
  "inactive",
  "disabled",
  "overdue",
  "out of stock",
  "no",
  "false",
  "non-compliant",
  "critical",
]);

const infoStatuses = new Set(["open", "new", "info", "review", "capa pending"]);

export function resolveStatusTone(value?: string | null): BadgeVariant {
  const key = value?.trim().toLowerCase() ?? "";
  if (!key) return "muted";
  if (successStatuses.has(key)) return "success";
  if (warningStatuses.has(key)) return "warning";
  if (errorStatuses.has(key)) return "error";
  if (infoStatuses.has(key)) return "info";
  return "secondary";
}
