import type { LearningAssignmentModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<LearningAssignmentModel>("LearningAssignment");
