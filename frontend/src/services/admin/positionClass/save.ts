import { PositionClassSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("PositionClass", PositionClassSchema, {
  booleanFields: ["isActive"],
  integerFields: ["allocatedHeadcount", "minExperienceYears", "minimumAge", "maximumAge"],
  numberFields: ["weeklyWorkingHours"],
});
