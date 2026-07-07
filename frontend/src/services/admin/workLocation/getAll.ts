import type { WorkLocationModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<WorkLocationModel>("WorkLocation");
