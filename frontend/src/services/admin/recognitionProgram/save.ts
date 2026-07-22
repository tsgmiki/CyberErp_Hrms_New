import { RecognitionProgramSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("RecognitionProgram", RecognitionProgramSchema, {
  booleanFields: ["isActive"],
});
