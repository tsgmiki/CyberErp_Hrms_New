import type { JobCategoryModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<JobCategoryModel>("JobCategory");
