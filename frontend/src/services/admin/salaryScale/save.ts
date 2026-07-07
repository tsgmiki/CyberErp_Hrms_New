import { SalaryScaleSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("SalaryScale", SalaryScaleSchema);
