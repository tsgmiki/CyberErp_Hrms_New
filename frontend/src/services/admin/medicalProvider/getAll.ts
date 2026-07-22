import type { MedicalProviderModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<MedicalProviderModel>("MedicalProvider");
