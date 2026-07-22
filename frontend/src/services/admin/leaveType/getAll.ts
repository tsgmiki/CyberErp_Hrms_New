import type { LeaveTypeModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<LeaveTypeModel>("LeaveType");
