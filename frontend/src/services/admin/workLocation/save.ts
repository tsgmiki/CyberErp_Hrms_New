import { WorkLocationSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("WorkLocation", WorkLocationSchema, { booleanFields: ["isActive"] });
