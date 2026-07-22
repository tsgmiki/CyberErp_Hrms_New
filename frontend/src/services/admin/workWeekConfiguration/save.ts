import { WorkWeekConfigurationSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

// PUT when the body carries an id, POST otherwise. Day fields are strings (Full/Half/Rest).
export default createSaveService("WorkWeekConfiguration", WorkWeekConfigurationSchema, {
  booleanFields: ["isActive"],
});
