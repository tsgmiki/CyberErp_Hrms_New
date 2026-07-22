import type { TrainingCategoryModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<TrainingCategoryModel>("TrainingCategory");
