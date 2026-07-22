import { TripBudgetSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("TripBudget", TripBudgetSchema, {
  integerFields: ["fiscalYear"],
  numberFields: ["amount"],
});
