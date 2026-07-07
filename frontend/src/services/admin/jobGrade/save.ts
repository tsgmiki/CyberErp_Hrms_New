import { JobGradeSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("JobGrade", JobGradeSchema);
