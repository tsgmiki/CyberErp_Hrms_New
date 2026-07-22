import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { EmployeeMovementModel, GeneratedDocumentModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Role-scoped paged movement list (admin all / manager subtree / employee own). */
export const getAllTransferRequests = createPagedQuery<EmployeeMovementModel>("EmployeeMovement/paged");

export const getTransferRequest = (id: string) => api.get<EmployeeMovementModel>(`EmployeeMovement/${id}`);

/* ---- HC173 — eligibility + impact assessment (advisory) ---- */

export interface TransferPlacementModel {
  positionId?: string;
  positionName?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  branchName?: string;
}
export interface TransferCompetencyModel {
  competencyId: string;
  name: string;
  weight: number;
  coveredByCurrentRole: boolean;
}
export interface TransferUnitImpactModel {
  unitName?: string;
  totalPositions: number;
  vacantPositions: number;
}
export interface TransferAssessmentModel {
  employeeId: string;
  employeeName?: string;
  current: TransferPlacementModel;
  target: TransferPlacementModel;
  performanceScorePercent?: number | null;
  performanceCycleName?: string;
  tenureMonthsTotal: number;
  tenureMonthsInCurrentRole: number;
  qualifications: string[];
  targetCompetencies: TransferCompetencyModel[];
  skillGapCount: number;
  currentUnitImpact: TransferUnitImpactModel;
  targetUnitImpact: TransferUnitImpactModel;
  currentSalary?: number | null;
  salaryUnchanged: boolean;
  flags: string[];
}

export const assessTransfer = (employeeId: string, toPositionId: string) =>
  api.get<TransferAssessmentModel>(`EmployeeMovement/assess?employeeId=${employeeId}&toPositionId=${toPositionId}`);

/* ---- HC174 — formal transfer notice ---- */

export const generateTransferNotice = async (templateId: string, movementId: string): Promise<GeneratedDocumentModel> => {
  const response = await fetch(`${API_BASE_URL}/DocumentTemplate/${templateId}/generate-movement/${movementId}`, {
    credentials: "include",
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Failed to generate the transfer notice");
  }
  return (await response.json()) as GeneratedDocumentModel;
};

/* ---- Save (bespoke JSON — the flat FormData template doesn't fit the picker-driven form) ---- */

export interface ActionResult {
  status: "success" | "error";
  message: string;
  id?: string;
  zodErrors: Record<string, string[] | undefined>;
}

export const saveTransferRequest = async (model: EmployeeMovementModel): Promise<ActionResult> => {
  try {
    const isUpdate = !!model.id;
    const res = await fetch(`${API_BASE_URL}/EmployeeMovement`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(model),
    });
    const text = await res.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    if (!res.ok)
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: parsed.errors ?? {} };
    return { status: "success", message: parsed.message ?? "Saved successfully", id: parsed.id ?? parsed, zodErrors: {} };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
};
