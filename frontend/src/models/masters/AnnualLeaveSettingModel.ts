import type AbstractModel from "../AbstractModel";

export default interface AnnualLeaveSettingModel extends AbstractModel {
  fiscalYearId?: string;
  fiscalYearName?: string;
  leaveTypeId?: string;
  leaveTypeName?: string;
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
  isActive?: boolean;
}
