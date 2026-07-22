import { RecognitionBadgeSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("RecognitionBadge", RecognitionBadgeSchema, {
  booleanFields: ["isActive"],
  integerFields: ["sortOrder", "pointsValue"],
  numberFields: ["monetaryValue", "autoGrantMinScore"],
});
