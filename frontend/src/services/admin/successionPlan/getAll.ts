import type { SuccessionPlanModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<SuccessionPlanModel>("SuccessionPlan");
