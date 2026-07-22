import type { JobGradeModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<JobGradeModel>("JobGrade");
