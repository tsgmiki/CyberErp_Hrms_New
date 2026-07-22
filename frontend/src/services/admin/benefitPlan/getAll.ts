import type { BenefitPlanModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<BenefitPlanModel>("BenefitPlan");
