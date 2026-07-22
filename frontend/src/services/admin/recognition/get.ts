import type { EmployeeRecognitionModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<EmployeeRecognitionModel>("Recognition");
