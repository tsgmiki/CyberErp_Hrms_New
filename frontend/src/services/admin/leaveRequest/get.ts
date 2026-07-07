import type { LeaveRequestModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<LeaveRequestModel>("LeaveRequest");
