import { AppraisalTemplateSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("AppraisalTemplate", AppraisalTemplateSchema, {
  booleanFields: ["isActive"],
  numberFields: ["goalsWeight", "competenciesWeight"],
});
