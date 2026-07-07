import type { JobCategoryModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<JobCategoryModel>("JobCategory");
