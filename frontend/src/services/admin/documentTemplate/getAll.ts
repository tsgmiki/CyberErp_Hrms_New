import type { DocumentTemplateModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<DocumentTemplateModel>("DocumentTemplate");
