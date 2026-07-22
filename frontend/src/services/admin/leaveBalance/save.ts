import { LeaveBalanceSetSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("LeaveBalance", LeaveBalanceSetSchema, {
  numberFields: ["entitled", "carriedForward", "adjusted"],
});
