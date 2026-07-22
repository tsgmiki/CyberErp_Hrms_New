import { AllowanceTypeSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("AllowanceType", AllowanceTypeSchema, {
  booleanFields: ["isTaxable", "isActive"],
  integerFields: ["sortOrder"],
  numberFields: ["defaultRate"],
});
