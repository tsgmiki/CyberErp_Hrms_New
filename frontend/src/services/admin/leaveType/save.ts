import { LeaveTypeSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("LeaveType", LeaveTypeSchema, {
  booleanFields: ["isPaid", "requiresApproval", "allowHalfDay", "isActive"],
  numberFields: ["defaultAnnualEntitlement", "carryForwardMaxDays"],
  integerFields: ["maxConsecutiveDays"],
});
