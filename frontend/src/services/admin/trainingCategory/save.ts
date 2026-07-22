import { TrainingCategorySchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("TrainingCategory", TrainingCategorySchema, {
  booleanFields: ["isActive"],
  integerFields: ["sortOrder"],
});
