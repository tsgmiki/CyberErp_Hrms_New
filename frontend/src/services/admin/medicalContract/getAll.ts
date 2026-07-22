import type { MedicalContractModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<MedicalContractModel>("MedicalContract");
