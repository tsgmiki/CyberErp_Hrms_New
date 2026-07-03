import { lazy, memo } from "react";
import { Shield } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const RoleForm = memo(lazy(() => import("./roleForm")));
const RoleList = memo(lazy(() => import("./roleList")));

function Role() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Roles"
      headerDescription="Manage user roles and access permissions"
      headerIcon={<Shield className="h-6 w-6 text-red-600" />}
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<RoleForm id={id} setRoleId={setId} />}
      list={<RoleList editHandler={editHandler} />}
    />
  );
}

export default Role;
