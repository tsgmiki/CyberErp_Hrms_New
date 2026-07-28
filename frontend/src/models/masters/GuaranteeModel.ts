import type AbstractModel from "../AbstractModel";

// ===== §3.12 Employee Guarantee Commitment Management (HC305–HC307) =====

/** A commitment the employee holds toward an EXTERNAL organization per NBE guarantee procedures. */
export interface EmployeeGuaranteeModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  employeeNumber?: string;
  type?: string; // LoanGuarantee | EmploymentGuarantee | Other
  externalOrganization?: string;
  beneficiaryName?: string;
  beneficiaryRelationship?: string;
  referenceNumber?: string;
  amount?: number;
  startDate?: string;
  endDate?: string;
  /** Workflow-owned: Active | Released | PendingApproval | Rejected. */
  status?: string;
  remarks?: string;
  releasedDate?: string;
  releaseNote?: string;
}

/** HC307 — headline chips for the guarantee dashboard. */
export interface GuaranteeDashboardModel {
  total: number;
  active: number;
  pendingApproval: number;
  released: number;
  rejected: number;
  activeAmount: number;
  expiringSoon: number;
}
