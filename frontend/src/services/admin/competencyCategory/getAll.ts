import type { CompetencyCategoryModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<CompetencyCategoryModel>("CompetencyCategory");
