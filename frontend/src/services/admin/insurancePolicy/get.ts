import type { InsurancePolicyModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<InsurancePolicyModel>("InsurancePolicy");
