import { lazy, memo } from "react";
import { UserCog } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const UserRoleForm = memo(lazy(() => import("./userRoleForm")));
const UserRoleList = memo(lazy(() => import("./userRoleList")));

function UserRole(props:{userId?:string}) {
  const {userId} = props
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="User Roles"
      headerDescription="Assign roles to users"
      headerIcon={<UserCog className="h-6 w-6 text-red-600" />}
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<UserRoleForm id={id} setUserRoleId={setId} />}
      list={<UserRoleList editHandler={editHandler} userId={userId} />}
    />
  );
}

export default UserRole;
