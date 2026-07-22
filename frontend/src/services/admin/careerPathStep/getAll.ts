import type { CareerPathStepModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<CareerPathStepModel>("CareerPathStep");
