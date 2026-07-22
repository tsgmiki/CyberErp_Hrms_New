import { InsurancePolicySchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("InsurancePolicy", InsurancePolicySchema, {
  booleanFields: ["isRenewal"],
  integerFields: ["policyYear"],
  numberFields: ["coverageAmount", "annualPremium"],
});
