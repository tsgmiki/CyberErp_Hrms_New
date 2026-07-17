import type { EmployeeRecognitionModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<EmployeeRecognitionModel>("Recognition");
