import type { AnnualLeaveModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<AnnualLeaveModel>("AnnualLeave");
