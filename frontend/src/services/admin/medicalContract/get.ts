import type { MedicalContractModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<MedicalContractModel>("MedicalContract");
