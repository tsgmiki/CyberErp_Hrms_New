import { api } from "@/utils/apiClient";

/** Year-end rollover: carry balances into the next fiscal year and close this one. */
export default async function rolloverFiscalYear(id: string) {
  return api.post<{ message: string }>(`FiscalYear/${id}/rollover-leave`, {});
}
