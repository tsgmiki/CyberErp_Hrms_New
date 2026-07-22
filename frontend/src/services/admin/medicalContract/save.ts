import { MedicalContractSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("MedicalContract", MedicalContractSchema, {
  numberFields: ["creditLimit"],
});
