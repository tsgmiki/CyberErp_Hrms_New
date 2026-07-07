import type { LeaveRequestModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<LeaveRequestModel>("LeaveRequest");
