import type { TrainingCourseModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<TrainingCourseModel>("TrainingCourse");
