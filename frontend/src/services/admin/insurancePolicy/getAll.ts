import type { InsurancePolicyModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<InsurancePolicyModel>("InsurancePolicy");
