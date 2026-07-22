import { CompetencySchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("Competency", CompetencySchema, { booleanFields: ["isActive"] });
