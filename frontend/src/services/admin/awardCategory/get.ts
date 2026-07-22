import type { AwardCategoryModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<AwardCategoryModel>("AwardCategory");
