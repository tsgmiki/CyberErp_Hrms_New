import type AbstractModel from "../AbstractModel";

/** §3.10.2 Medical Benefit Management (HC235–246). */

export interface MedicalProviderModel extends AbstractModel {
  name?: string;
  providerType?: string; // Hospital | Clinic | Laboratory | Pharmacy | Other
  specialization?: string;
  phoneNumber?: string;
  email?: string;
  address?: string;
  isActive?: boolean;
}

export interface MedicalPlanModel extends AbstractModel {
  name?: string;
  description?: string;
  annualCoverageLimit?: number | null;
  coveragePercent?: number;
  coversDependents?: boolean;
  benefitPlanId?: string;
  isActive?: boolean;
}

export interface MedicalContractModel extends AbstractModel {
  medicalProviderId?: string;
  providerName?: string;
  contractNumber?: string;
  terms?: string;
  startDate?: string;
  renewalDate?: string;
  endDate?: string;
  creditLimit?: number | null;
  status?: string; // Active | Expired | Terminated
  notes?: string;
}

export interface MedicalBeneficiaryModel {
  id?: string;
  category?: string; // Employee | Spouse | Child | Parent | Pensioner | Other
  fullName?: string;
  employeeDependentId?: string;
  dateOfBirth?: string;
  relationship?: string;
  isActive?: boolean;
}

export interface MedicalEnrollmentModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  medicalPlanId?: string;
  medicalPlanName?: string;
  coversDependents?: boolean;
  enrolledOn?: string;
  coverageStart?: string;
  coverageEnd?: string;
  status?: string; // Active | Suspended | Terminated
  remark?: string;
  beneficiaries?: MedicalBeneficiaryModel[];
}

export interface MedicalClaimAttachmentMetaModel {
  id?: string;
  fileName?: string;
  contentType?: string;
  fileSize?: number;
}

export interface MedicalClaimModel extends AbstractModel {
  claimNumber?: string;
  employeeId?: string;
  employeeName?: string;
  medicalBeneficiaryId?: string;
  beneficiaryName?: string;
  beneficiaryCategory?: string;
  medicalPlanId?: string;
  medicalPlanName?: string;
  medicalProviderId?: string;
  providerName?: string;
  source?: string; // Employee | Provider
  serviceDate?: string;
  submittedOn?: string;
  claimedAmount?: number;
  approvedAmount?: number | null;
  status?: string; // Pending | UnderReview | Approved | Rejected | Paid
  description?: string;
  diagnosis?: string;
  resolution?: string;
  paidAt?: string;
  paymentReference?: string;
  attachments?: MedicalClaimAttachmentMetaModel[];
}

export interface MedicalExpenseRowModel {
  category?: string;
  claimCount?: number;
  totalClaimed?: number;
  totalApproved?: number;
}

export interface MedicalExpenseReportModel {
  fromDate?: string;
  toDate?: string;
  rows?: MedicalExpenseRowModel[];
  totalClaims?: number;
  grandTotalClaimed?: number;
  grandTotalApproved?: number;
}
