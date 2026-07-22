import type { MedicalPlanModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<MedicalPlanModel>("MedicalPlan");
