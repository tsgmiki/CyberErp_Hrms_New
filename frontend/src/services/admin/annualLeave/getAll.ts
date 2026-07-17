import type { AnnualLeaveModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<AnnualLeaveModel>("AnnualLeave");
