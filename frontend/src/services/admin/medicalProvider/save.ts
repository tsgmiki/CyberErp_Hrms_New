import { MedicalProviderSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("MedicalProvider", MedicalProviderSchema, {
  booleanFields: ["isActive"],
});
