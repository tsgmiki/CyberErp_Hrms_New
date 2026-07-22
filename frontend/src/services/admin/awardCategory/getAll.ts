import type { AwardCategoryModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<AwardCategoryModel>("AwardCategory");
