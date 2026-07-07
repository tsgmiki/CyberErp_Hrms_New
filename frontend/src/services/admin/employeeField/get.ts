import type { EmployeeFieldModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<EmployeeFieldModel>("EmployeeField");
