import type { EmployeeModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<EmployeeModel>("Employee");
