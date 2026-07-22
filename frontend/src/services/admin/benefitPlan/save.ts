import { BenefitPlanSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("BenefitPlan", BenefitPlanSchema, {
  booleanFields: ["isActive"],
  numberFields: ["employeeContributionRate", "employerContributionRate"],
});
