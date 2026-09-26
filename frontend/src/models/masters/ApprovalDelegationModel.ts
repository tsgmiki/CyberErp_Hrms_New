/** One approver's authority lent to another for a bounded period. */
export default interface ApprovalDelegationModel {
  id?: string;
  /** The real approver, whose authority is being lent. */
  fromEmployeeId?: string;
  fromEmployeeName?: string;
  /** The stand-in. */
  toEmployeeId?: string;
  toEmployeeName?: string;
  startDate?: string;
  endDate?: string;
  reason?: string;
  /** Cover every workflow process, rather than the named `entityTypes`. */
  allProcesses?: boolean;
  entityTypes?: string[];
  /**
   * Ceiling on what the delegate may approve. A request above it is NOT rejected — it stays with
   * the real approver.
   */
  approvalLimit?: number | null;
  /** Derived server-side from the dates + revocation: Scheduled | Active | Revoked | Expired. */
  status?: string;
  revokedAt?: string | null;
  revokedBy?: string | null;
  revocationReason?: string | null;
}

/** The seniority rules' verdict on one delegator/delegate pair. */
export interface DelegationEligibilityModel {
  isEligible: boolean;
  reasons: string[];
  /** Internal service + prior experience, merged. */
  delegateExperienceYears: number;
  /** Delegate salary as a percentage of the approver's; null when either is unrecorded. */
  salaryRatioPercent?: number | null;
}

/** Tenant-wide rules deciding who may hold somebody else's approval authority. */
export interface DelegationPolicyModel {
  minDelegateExperienceYears: number;
  minSalaryRatioPercent: number;
  requireManagerialDelegate: boolean;
  maxDelegationDays: number;
  defaultApprovalLimit?: number | null;
  allowSelfServiceDelegation: boolean;
}
