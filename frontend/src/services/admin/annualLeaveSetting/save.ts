import { AnnualLeaveSettingSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("AnnualLeaveSetting", AnnualLeaveSettingSchema, {
  booleanFields: ["isActive", "considerExternalExperience"],
  integerFields: [
    "minExperienceMonths", "newEmployeeLeaveDays", "baseLeaveDays", "managerialLeaveDays",
    "incrementDays", "incrementIntervalYears", "maxLeaveDays", "expiryYears",
    "preMilestoneBaseLeaveDays", "preMilestoneIncrementDays", "preMilestoneIntervalYears",
  ],
});
