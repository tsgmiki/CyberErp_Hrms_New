import { JobCategorySchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("JobCategory", JobCategorySchema, { booleanFields: ["isActive"] });
