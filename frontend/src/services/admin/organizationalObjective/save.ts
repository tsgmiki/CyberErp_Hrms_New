import { OrganizationalObjectiveSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("OrganizationalObjective", OrganizationalObjectiveSchema, {
  numberFields: ["weight"],
});
