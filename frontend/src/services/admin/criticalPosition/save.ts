import { CriticalPositionSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";
export default createSaveService("CriticalPosition", CriticalPositionSchema, { booleanFields: ["isActive"] });
