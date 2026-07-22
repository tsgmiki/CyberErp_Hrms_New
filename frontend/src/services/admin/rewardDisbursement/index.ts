import { createPagedQuery } from "@/template/createPagedQuery";
import type { RewardDisbursementModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Admin-only paged payout hand-off list (HC185). */
export const getAllDisbursements = createPagedQuery<RewardDisbursementModel>("RewardDisbursement");

export const markDisbursementPaid = async (id: string, reference?: string): Promise<{ ok: boolean; message: string }> => {
  const res = await fetch(`${API_BASE_URL}/RewardDisbursement/${id}/mark-paid`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ reference }),
  });
  const text = await res.text();
  let message = res.ok ? "Marked as paid" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors?.id?.[0] ?? parsed?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
};

/** Downloads the payroll hand-off CSV (server-generated). */
export const downloadDisbursementCsv = async (status?: string): Promise<void> => {
  const res = await fetch(`${API_BASE_URL}/RewardDisbursement/export${status ? `?status=${status}` : ""}`, {
    credentials: "include",
  });
  if (!res.ok) throw new Error("Export failed");
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "reward-disbursements.csv";
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
};
