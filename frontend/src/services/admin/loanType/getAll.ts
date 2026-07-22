import type { LoanTypeModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<LoanTypeModel>("LoanType");
