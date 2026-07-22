import { api } from "@/utils/apiClient";
import type { RewardPointsSummaryModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Own balance + statement by default; HR/managers may pass an employee in their scope (HC180). */
export const getRewardPoints = (employeeId?: string, skip = 0, take = 15) =>
  api.get<RewardPointsSummaryModel>(
    `RewardPoints?skip=${skip}&take=${take}${employeeId ? `&employeeId=${employeeId}` : ""}`,
  );

export interface RedeemResult {
  ok: boolean;
  message: string;
  balance?: number;
}

export const redeemRewardPoints = async (points: number, note?: string): Promise<RedeemResult> => {
  const res = await fetch(`${API_BASE_URL}/RewardPoints/redeem`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ points, note }),
  });
  const text = await res.text();
  try {
    const parsed = JSON.parse(text);
    if (!res.ok) {
      const message = parsed?.errors?.points?.[0] ?? parsed?.errors?.Points?.[0] ?? parsed?.message ?? "Redemption failed";
      return { ok: false, message };
    }
    return { ok: true, message: "Points redeemed", balance: parsed.balance };
  } catch {
    return { ok: res.ok, message: res.ok ? "Points redeemed" : text || "Redemption failed" };
  }
};
