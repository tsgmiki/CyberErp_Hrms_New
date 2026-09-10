import { QuestionBankSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("QuestionBank", QuestionBankSchema, {
  booleanFields: ["isActive"],
});
