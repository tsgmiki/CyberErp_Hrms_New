import type { MedicalPlanModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<MedicalPlanModel>("MedicalPlan");
