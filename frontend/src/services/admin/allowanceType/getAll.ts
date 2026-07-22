import type { AllowanceTypeModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<AllowanceTypeModel>("AllowanceType");
