import { createPagedQuery } from "@/template/createPagedQuery";

export interface StepOption {
  id?: string;
  name?: string;
  code?: string;
}

export default createPagedQuery<StepOption>("Step");
