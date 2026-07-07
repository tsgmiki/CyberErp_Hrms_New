import { EmployeeFieldSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("EmployeeField", EmployeeFieldSchema, {
  booleanFields: ["isRequired", "isActive"],
});
