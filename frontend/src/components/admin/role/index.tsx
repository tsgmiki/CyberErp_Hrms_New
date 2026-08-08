import { lazy, memo } from "react";
import { Shield } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const RoleForm = memo(lazy(() => import("./roleForm")));
const RoleList = memo(lazy(() => import("./roleList")));

function Role() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/role");

  return (
    <EntityModuleShell
      title="Roles"
      headerDescription="Manage user roles and access permissions"
      headerIcon={<Shield className="h-6 w-6 text-primary" />}
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<RoleForm id={id} setRoleId={setId} />}
      list={<RoleList editHandler={editHandler} />}
    />
  );
}

export default Role;
