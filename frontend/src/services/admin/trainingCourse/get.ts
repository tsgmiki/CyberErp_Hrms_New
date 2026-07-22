import type { TrainingCourseModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<TrainingCourseModel>("TrainingCourse");
