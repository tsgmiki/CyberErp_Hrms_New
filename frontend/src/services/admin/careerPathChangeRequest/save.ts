import { CareerPathChangeRequestSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";
export default createSaveService("CareerPathChangeRequest", CareerPathChangeRequestSchema, {});
