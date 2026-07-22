import type { EmployeeFieldModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<EmployeeFieldModel>("EmployeeField");
