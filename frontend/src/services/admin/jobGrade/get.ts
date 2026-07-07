import type { JobGradeModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<JobGradeModel>("JobGrade");
