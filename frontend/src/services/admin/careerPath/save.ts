import { CareerPathSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";
export default createSaveService("CareerPath", CareerPathSchema, { booleanFields: ["isActive"] });
