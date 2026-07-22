import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { LoanTypeModel, LoanModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ActionResult { ok: boolean; message: string; id?: string }

async function action(method: string, path: string, body?: unknown): Promise<ActionResult> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method, credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
  if (!res.ok) return { ok: false, message: errorMessageParser(parsed.errors || parsed) || "Request failed" };
  return { ok: true, message: parsed?.message ?? "Saved", id: parsed?.id };
}

/* ---- Loan types (HC251) ---- */
export const getAllLoanTypes = createPagedQuery<LoanTypeModel>("LoanType");
export const saveLoanType = (m: LoanTypeModel) => action(m.id ? "PUT" : "POST", "LoanType", m);
export const deleteLoanType = (id: string) => action("DELETE", `LoanType/${id}`);

/* ---- Loans (HC252–259) ---- */
export const getAllLoans = createPagedQuery<LoanModel>("Loan");
export const getLoan = (id: string) => api.get<LoanModel>(`Loan/${id}`);
export const requestLoan = (body: unknown) => action("POST", "Loan", body);
export const approveLoan = (id: string, note?: string) => action("POST", `Loan/${id}/approve`, { note });
export const rejectLoan = (id: string, reason: string) => action("POST", `Loan/${id}/reject`, { reason });
export const cancelLoan = (id: string) => action("POST", `Loan/${id}/cancel`, {});
export const disburseLoan = (id: string, reference?: string) => action("POST", `Loan/${id}/disburse`, { reference });
export const repayLoan = (id: string, amount: number) => action("POST", `Loan/${id}/repay`, { amount });
export const incrementInstallment = (id: string, newMonthlyInstallment: number) => action("POST", `Loan/${id}/increment-installment`, { newMonthlyInstallment });
export const consentLoan = (id: string) => action("POST", `Loan/${id}/consent`, {});
