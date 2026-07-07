import { AnnualLeaveSettingSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("AnnualLeaveSetting", AnnualLeaveSettingSchema, {
  booleanFields: ["isActive"],
  integerFields: [
    "minExperienceMonths", "newEmployeeLeaveDays", "baseLeaveDays", "managerialLeaveDays",
    "incrementDays", "incrementIntervalYears", "maxLeaveDays", "expiryYears",
  ],
});
