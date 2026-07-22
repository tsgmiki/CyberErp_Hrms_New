import type { TripBudgetModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<TripBudgetModel>("TripBudget");
