import type { MedicalProviderModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<MedicalProviderModel>("MedicalProvider");
