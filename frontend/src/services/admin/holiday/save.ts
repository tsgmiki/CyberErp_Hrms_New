import { HolidaySchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("Holiday", HolidaySchema, {
  booleanFields: ["isRecurring", "isActive"],
});
