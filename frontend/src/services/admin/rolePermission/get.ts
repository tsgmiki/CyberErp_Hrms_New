import type { RolePermissionModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<RolePermissionModel>("RolePermission");
