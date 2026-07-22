import type AbstractModel from "../AbstractModel";

/** §3.10.4 Employee Loan Management (HC251–259). */

export interface LoanTypeModel extends AbstractModel {
  name?: string;
  description?: string;
  maxAmount?: number | null;
  maxSalaryMultiple?: number | null;
  maxTermMonths?: number;
  interestRatePct?: number;
  requiresGuarantor?: boolean;
  minGuarantors?: number;
  serviceCommitmentMonths?: number;
  isActive?: boolean;
}

export interface LoanGuarantorModel {
  id?: string;
  guarantorEmployeeId?: string;
  fullName?: string;
  identificationNumber?: string;
  relationship?: string;
  phoneNumber?: string;
  guaranteedAmount?: number | null;
}

export interface LoanScheduleLineModel {
  id?: string;
  installmentNo?: number;
  dueDate?: string;
  principalPortion?: number;
  interestPortion?: number;
  amount?: number;
  status?: string; // Pending | Paid
  paidAt?: string;
}

export interface LoanModel extends AbstractModel {
  loanNumber?: string;
  employeeId?: string;
  employeeName?: string;
  loanTypeId?: string;
  loanTypeName?: string;
  principalAmount?: number;
  termMonths?: number;
  interestRatePct?: number;
  monthlyInstallment?: number;
  totalInterest?: number;
  totalRepayable?: number;
  purpose?: string;
  requestDate?: string;
  status?: string; // Requested | Approved | Rejected | Disbursed | Active | Settled | Cancelled
  resolution?: string;
  serviceCommitmentMonths?: number;
  disbursedAt?: string;
  disbursementReference?: string;
  settledAt?: string;
  serviceCommitmentConsentAt?: string;
  outstandingBalance?: number;
  paidInstallmentCount?: number;
  totalInstallmentCount?: number;
  guarantors?: LoanGuarantorModel[];
  schedule?: LoanScheduleLineModel[];
}
