import type { SalaryScaleModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<SalaryScaleModel>("SalaryScale");
