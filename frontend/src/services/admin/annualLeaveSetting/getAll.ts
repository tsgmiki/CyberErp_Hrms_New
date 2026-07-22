import type { AnnualLeaveSettingModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<AnnualLeaveSettingModel>("AnnualLeaveSetting");
