import { createPagedQuery } from "@/template/createPagedQuery";
import type { TrainingProviderPaymentModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Admin-only provider-payment hand-off list (HC202). */
export const getAllProviderPayments = createPagedQuery<TrainingProviderPaymentModel>("TrainingProviderPayment");

export const markProviderPaymentPaid = async (id: string, reference?: string): Promise<{ ok: boolean; message: string }> => {
  const res = await fetch(`${API_BASE_URL}/TrainingProviderPayment/${id}/mark-paid`, {
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

/** Downloads the finance hand-off CSV (server-generated). */
export const downloadProviderPaymentCsv = async (): Promise<void> => {
  const res = await fetch(`${API_BASE_URL}/TrainingProviderPayment/export`, { credentials: "include" });
  if (!res.ok) throw new Error("Export failed");
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "training-provider-payments.csv";
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
};
