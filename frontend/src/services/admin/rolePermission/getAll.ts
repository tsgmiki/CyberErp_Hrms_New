import type { RolePermissionModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<RolePermissionModel>("RolePermission");
