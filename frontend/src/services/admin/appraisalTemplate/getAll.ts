import type { AppraisalTemplateModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<AppraisalTemplateModel>("AppraisalTemplate");
