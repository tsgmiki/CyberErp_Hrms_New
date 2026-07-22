import type { FiscalYearModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<FiscalYearModel>("FiscalYear");
