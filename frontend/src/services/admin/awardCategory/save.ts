import { AwardCategorySchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("AwardCategory", AwardCategorySchema, {
  booleanFields: ["isActive"],
  integerFields: ["sortOrder"],
});
