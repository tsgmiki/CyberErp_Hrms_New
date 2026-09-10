import { LearningAssignmentSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("LearningAssignment", LearningAssignmentSchema, {
  booleanFields: ["isActive", "includeSubUnits"],
  integerFields: ["dueWithinDays", "recurrenceMonths"],
});
