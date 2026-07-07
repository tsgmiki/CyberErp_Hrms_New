import { PositionSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("Position", PositionSchema, { booleanFields: ["isActive"] });
