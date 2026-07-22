import { MedicalPlanSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("MedicalPlan", MedicalPlanSchema, {
  booleanFields: ["coversDependents", "isActive"],
  numberFields: ["annualCoverageLimit", "coveragePercent"],
});
