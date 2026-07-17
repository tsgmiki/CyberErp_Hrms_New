import type { WorkWeekConfigurationModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<WorkWeekConfigurationModel>("WorkWeekConfiguration");
