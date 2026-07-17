import { RecognitionSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("Recognition", RecognitionSchema, { booleanFields: ["isPublic"] });
