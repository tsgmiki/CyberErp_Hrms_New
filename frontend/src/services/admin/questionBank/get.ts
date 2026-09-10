import type { QuestionBankModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<QuestionBankModel>("QuestionBank");
