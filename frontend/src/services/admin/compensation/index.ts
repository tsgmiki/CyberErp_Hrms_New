import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  AllowanceTypeModel, EmployeeAllowanceModel, CompensationSummaryModel,
  SalaryRevisionModel, SalarySimulationModel, SalaryRevisionBandModel, SalaryIncrementPolicyModel,
  BenefitPlanModel, BenefitEnrollmentModel,
  TaxBracketModel, PayrollDeductionsModel,
  MyCompensationModel, CompensationRequestModel,
} from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ActionResult { ok: boolean; message: string; id?: string }

/** Generic JSON action returning {ok,message,id}. */
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
  return { ok: true, message: parsed?.message ?? "Saved", id: parsed?.id };
}

/* ---- Allowance types (HC226) ---- */
export const getAllAllowanceTypes = createPagedQuery<AllowanceTypeModel>("AllowanceType");
export const saveAllowanceType = (m: AllowanceTypeModel) => action(m.id ? "PUT" : "POST", "AllowanceType", m);
export const deleteAllowanceType = (id: string) => action("DELETE", `AllowanceType/${id}`);

/* ---- Per-employee allowances + compensation summary (HC226/HC233) ---- */
export const getEmployeeAllowances = (employeeId: string) =>
  api.get<EmployeeAllowanceModel[]>(`EmployeeAllowance?employeeId=${employeeId}`);
export const getCompensationSummary = (employeeId: string) =>
  api.get<CompensationSummaryModel>(`EmployeeAllowance/summary?employeeId=${employeeId}`);
export const saveEmployeeAllowance = (m: EmployeeAllowanceModel) => action(m.id ? "PUT" : "POST", "EmployeeAllowance", m);
export const deleteEmployeeAllowance = (id: string) => action("DELETE", `EmployeeAllowance/${id}`);

/* ---- Salary revision + simulation (HC228) ---- */
export const getAllSalaryRevisions = createPagedQuery<SalaryRevisionModel>("SalaryRevision");
export const getSalaryRevision = (id: string) => api.get<SalaryRevisionModel>(`SalaryRevision/${id}`);
export const simulateSalaryRevision = (body: {
  basis: string;
  /** Merit | Market | CostOfLiving | Performance — Performance derives the rate from `bands`. */
  revisionType?: string;
  rate: number;
  bands?: SalaryRevisionBandModel[];
  targetJobGradeId?: string;
  targetOrganizationUnitId?: string;
  targetReviewCycleId?: string;
  /** Service and disciplinary rules are measured at this date; the API defaults it to today. */
  effectiveDate?: string;
}) =>
  api.post<SalarySimulationModel>("SalaryRevision/simulate", body);
export const saveSalaryRevision = (m: SalaryRevisionModel) => action(m.id ? "PUT" : "POST", "SalaryRevision", m);
export const setSalaryRevisionLine = (lineId: string, proposedSalary: number) =>
  action("PUT", `SalaryRevision/lines/${lineId}`, { proposedSalary });
export const submitSalaryRevision = (id: string) => action("POST", `SalaryRevision/${id}/submit`);
export const approveSalaryRevision = (id: string) => action("POST", `SalaryRevision/${id}/approve`);
export const applySalaryRevision = (id: string) => action("POST", `SalaryRevision/${id}/apply`);
export const deleteSalaryRevision = (id: string) => action("DELETE", `SalaryRevision/${id}`);

/* ---- Increment eligibility rules — a per-tenant singleton, so there is no list or id ---- */
/**
 * A tenant with no policy yet is a real, expected state, and the API says so with `204 No Content`.
 * `api.get` has no JSON content-type to parse there and hands back the empty body as `""`, so it is
 * normalized to `null` here — otherwise every caller would have to know that a falsy STRING means
 * "not configured", and React Query would cache an empty string as if it were a record.
 */
export const getSalaryIncrementPolicy = async (): Promise<SalaryIncrementPolicyModel | null> =>
  (await api.get<SalaryIncrementPolicyModel | "">("SalaryIncrementPolicy")) || null;
export const saveSalaryIncrementPolicy = (m: SalaryIncrementPolicyModel) =>
  action("PUT", "SalaryIncrementPolicy", m);

/* ---- Benefit plans + enrollment (HC230) ---- */
export const getAllBenefitPlans = createPagedQuery<BenefitPlanModel>("BenefitPlan");
export const saveBenefitPlan = (m: BenefitPlanModel) => action(m.id ? "PUT" : "POST", "BenefitPlan", m);
export const deleteBenefitPlan = (id: string) => action("DELETE", `BenefitPlan/${id}`);
export const getEmployeeBenefits = (employeeId: string) =>
  api.get<BenefitEnrollmentModel[]>(`BenefitEnrollment?employeeId=${employeeId}`);
export const enrollBenefit = (body: { employeeId: string; benefitPlanId: string; coverageStart: string; electedEmployeeContribution?: number }) =>
  action("POST", "BenefitEnrollment", body);
export const waiveBenefit = (id: string, remark?: string) => action("POST", `BenefitEnrollment/${id}/waive`, { remark });
export const terminateBenefit = (id: string, coverageEnd: string, remark?: string) =>
  action("POST", `BenefitEnrollment/${id}/terminate`, { coverageEnd, remark });

/* ---- Tax brackets + deductions (HC231/232) ---- */
export const getAllTaxBrackets = createPagedQuery<TaxBracketModel>("TaxBracket");
export const saveTaxBracket = (m: TaxBracketModel) => action(m.id ? "PUT" : "POST", "TaxBracket", m);
export const deleteTaxBracket = (id: string) => action("DELETE", `TaxBracket/${id}`);
export const getDeductions = (employeeId: string) =>
  api.get<PayrollDeductionsModel>(`TaxBracket/deductions?employeeId=${employeeId}`);

/* ---- Self-service (HC233/234) ---- */
export const getMyCompensation = () => api.get<MyCompensationModel>("MyCompensation");
export const getCompensationRequests = createPagedQuery<CompensationRequestModel>("MyCompensation/requests");
export const submitCompensationRequest = (m: CompensationRequestModel) => action("POST", "MyCompensation/requests", m);
export const resolveCompensationRequest = (id: string, status: string, resolution?: string) =>
  action("POST", `MyCompensation/requests/${id}/resolve`, { status, resolution });
