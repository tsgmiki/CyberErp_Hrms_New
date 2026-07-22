import type AbstractModel from "../AbstractModel";

/** §3.10.5 Trip Management (HC260–268). */

export interface PerDiemRateModel extends AbstractModel {
  jobGradeId?: string;
  jobGradeName?: string;
  tripType?: string; // Local | International
  dailyRate?: number;
  currency?: string;
  isActive?: boolean;
}

export interface TripBudgetModel extends AbstractModel {
  fiscalYear?: number;
  organizationUnitId?: string | null;
  organizationUnitName?: string;
  amount?: number;
  notes?: string;
}

export interface TripBudgetUtilizationModel {
  fiscalYear?: number;
  organizationUnitId?: string | null;
  budgetAmount?: number;
  committed?: number;
  remaining?: number;
  tripCount?: number;
}

export interface TripExpenseModel {
  id?: string;
  category?: string;
  description?: string;
  expenseDate?: string;
  amount?: number;
  currency?: string;
}

export interface TripRequestModel extends AbstractModel {
  tripNumber?: string;
  employeeId?: string;
  employeeName?: string;
  tripType?: string; // Local | International
  destination?: string;
  purpose?: string;
  startDate?: string;
  endDate?: string;
  days?: number;
  dailyPerDiemRate?: number;
  perDiemAmount?: number;
  advanceAmount?: number;
  currency?: string;
  tripBudgetId?: string | null;
  status?: string; // Requested | Approved | Rejected | InProgress | Completed | Settled | Cancelled
  resolution?: string;
  requestDate?: string;
  advanceDisbursedAt?: string;
  advanceReference?: string;
  settledAt?: string;
  settlementNet?: number | null;
  settlementReference?: string;
  totalExpenses?: number;
  expenses?: TripExpenseModel[];
}

export interface TripAgingItemModel {
  tripId?: string;
  tripNumber?: string;
  employeeName?: string;
  tripType?: string;
  endDate?: string;
  daysOutstanding?: number;
  bucket?: string;
  advanceAmount?: number;
  currency?: string;
}

export interface TripAgingRowModel {
  bucket?: string;
  count?: number;
  totalOutstanding?: number;
}

export interface TripAgingReportModel {
  buckets?: TripAgingRowModel[];
  items?: TripAgingItemModel[];
  totalCount?: number;
  totalOutstanding?: number;
}
