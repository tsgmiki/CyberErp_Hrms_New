import { PerDiemRateSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("PerDiemRate", PerDiemRateSchema, {
  booleanFields: ["isActive"],
  numberFields: ["dailyRate"],
});
