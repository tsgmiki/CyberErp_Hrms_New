import type { TripBudgetModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<TripBudgetModel>("TripBudget");
