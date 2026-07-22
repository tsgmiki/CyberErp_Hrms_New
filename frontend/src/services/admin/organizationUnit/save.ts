import { OrganizationUnitSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("OrganizationUnit", OrganizationUnitSchema, { booleanFields: ["isActive"] });
