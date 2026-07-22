import { LoanTypeSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("LoanType", LoanTypeSchema, {
  booleanFields: ["requiresGuarantor", "isActive"],
  integerFields: ["maxTermMonths", "minGuarantors", "serviceCommitmentMonths"],
  numberFields: ["maxAmount", "maxSalaryMultiple", "interestRatePct"],
});
