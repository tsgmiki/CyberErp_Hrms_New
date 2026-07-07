import { BranchSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("Branch", BranchSchema, { booleanFields: ["isActive", "isHeadOffice"] });
