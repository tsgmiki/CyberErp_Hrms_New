import type AbstractModel from "../AbstractModel";

/** §3.10.3 Insurance Management (HC247–250). */

export interface InsurancePremiumScheduleModel {
  id?: string;
  installment?: number;
  dueDate?: string;
  amount?: number;
  status?: string; // Pending | Paid
  paidAt?: string;
  paymentReference?: string;
}

export interface InsurancePolicyModel extends AbstractModel {
  policyNumber?: string;
  insurerName?: string;
  insuranceType?: string; // Life | Health | Disability | Accident | WorkersCompensation | Other
  coverage?: string;
  coverageAmount?: number;
  policyYear?: number;
  startDate?: string;
  endDate?: string;
  annualPremium?: number;
  premiumFrequency?: string; // Annual | SemiAnnual | Quarterly | Monthly
  status?: string; // Active | Expired | Renewed | Cancelled
  isRenewal?: boolean;
  previousPolicyId?: string;
  notes?: string;
  premiumPaid?: number;
  premiumOutstanding?: number;
  schedule?: InsurancePremiumScheduleModel[];
}

export interface InsuranceClaimAttachmentMetaModel {
  id?: string;
  fileName?: string;
  contentType?: string;
  fileSize?: number;
}

export interface InsuranceClaimModel extends AbstractModel {
  claimNumber?: string;
  employeeId?: string;
  employeeName?: string;
  insurancePolicyId?: string;
  policyNumber?: string;
  insurerName?: string;
  claimType?: string;
  incidentDate?: string;
  submittedOn?: string;
  claimedAmount?: number;
  approvedAmount?: number | null;
  status?: string; // Pending | UnderReview | Approved | Rejected | Paid
  description?: string;
  resolution?: string;
  paidAt?: string;
  paymentReference?: string;
  attachments?: InsuranceClaimAttachmentMetaModel[];
}
