import type { LearningAssignmentModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<LearningAssignmentModel>("LearningAssignment");
