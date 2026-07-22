import type { EmployeeModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<EmployeeModel>("Employee");
