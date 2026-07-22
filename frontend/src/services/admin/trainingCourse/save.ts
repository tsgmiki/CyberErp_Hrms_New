import { TrainingCourseSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("TrainingCourse", TrainingCourseSchema, {
  booleanFields: ["isActive", "isExternal"],
  numberFields: ["durationHours", "cpdHours"],
});
