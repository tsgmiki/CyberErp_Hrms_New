import { FiscalYearSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("FiscalYear", FiscalYearSchema, { booleanFields: ["isActive"] });
