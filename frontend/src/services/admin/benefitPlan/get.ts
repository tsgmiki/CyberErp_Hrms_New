import type { BenefitPlanModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<BenefitPlanModel>("BenefitPlan");
