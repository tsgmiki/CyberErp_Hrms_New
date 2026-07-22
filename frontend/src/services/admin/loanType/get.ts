import type { LoanTypeModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<LoanTypeModel>("LoanType");
