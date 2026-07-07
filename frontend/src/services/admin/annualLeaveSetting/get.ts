import type { AnnualLeaveSettingModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<AnnualLeaveSettingModel>("AnnualLeaveSetting");
