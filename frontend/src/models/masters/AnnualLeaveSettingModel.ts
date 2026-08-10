import type AbstractModel from "../AbstractModel";

export default interface AnnualLeaveSettingModel extends AbstractModel {
  fiscalYearId?: string;
  fiscalYearName?: string;
  minExperienceMonths?: number;
  newEmployeeLeaveDays?: number;
  baseLeaveDays?: number;
  managerialLeaveDays?: number;
  incrementDays?: number;
  incrementIntervalYears?: number;
  maxLeaveDays?: number;
  expiryYears?: number;
  /** ServiceMilestone | ServiceYears | FiscalYears */
  ruleType?: string;
  considerExternalExperience?: boolean;
  milestoneDate?: string;
  preMilestoneBaseLeaveDays?: number;
  preMilestoneIncrementDays?: number;
  preMilestoneIntervalYears?: number;
  /** Fallback entitlement for balances the accrual engine has not generated (was on LeaveType). */
  defaultAnnualEntitlement?: number;
  /** Rollover carry cap; empty = unlimited, 0 = none (was on LeaveType). */
  carryForwardMaxDays?: number;
  /** Cap on one continuous request line; empty = no cap (was on LeaveType). */
  maxConsecutiveDays?: number;
  /** Allow half-day request lines (was LeaveType.allowHalfDay). */
  allowHalfDay?: boolean;
  isActive?: boolean;
}
