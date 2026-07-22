import type { EmployeeCareerPathModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<EmployeeCareerPathModel>("EmployeeCareerPath");
