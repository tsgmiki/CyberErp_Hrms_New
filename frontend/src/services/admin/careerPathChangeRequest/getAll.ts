import type { CareerPathChangeRequestModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<CareerPathChangeRequestModel>("CareerPathChangeRequest");
