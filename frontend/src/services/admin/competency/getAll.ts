import type { CompetencyModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<CompetencyModel>("Competency");
