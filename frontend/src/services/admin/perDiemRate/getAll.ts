import type { PerDiemRateModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<PerDiemRateModel>("PerDiemRate");
