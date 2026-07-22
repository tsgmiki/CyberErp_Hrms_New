import { CompetencyCategorySchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("CompetencyCategory", CompetencyCategorySchema, {
  booleanFields: ["isActive"],
  integerFields: ["sortOrder"],
});
