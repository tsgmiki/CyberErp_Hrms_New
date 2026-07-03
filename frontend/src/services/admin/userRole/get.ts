import type { UserRoleModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<UserRoleModel>("UserRole");
