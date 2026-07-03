import type { UserRoleModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<UserRoleModel>("UserRole");
