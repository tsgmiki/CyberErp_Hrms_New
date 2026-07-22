import type { LeaveTypeModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<LeaveTypeModel>("LeaveType");
