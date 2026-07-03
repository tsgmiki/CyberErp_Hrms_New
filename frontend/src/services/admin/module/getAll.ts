import type { ModuleModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<ModuleModel>("Module");
