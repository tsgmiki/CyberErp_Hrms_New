import type { CareerPathModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<CareerPathModel>("CareerPath");
