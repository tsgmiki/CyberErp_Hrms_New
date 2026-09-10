import type { QuestionBankModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<QuestionBankModel>("QuestionBank");
