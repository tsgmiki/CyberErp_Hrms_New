import { lazy, memo } from "react";

const RolePermissionForm = memo(lazy(() => import("./form")));

function RolePermission() {
  return <RolePermissionForm />;
}

export default RolePermission;