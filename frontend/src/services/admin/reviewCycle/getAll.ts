import type { ReviewCycleModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<ReviewCycleModel>("ReviewCycle");
