import { lazy, memo } from "react";
import { Users } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const UserForm = memo(lazy(() => import("./userForm")));
const UserList = memo(lazy(() => import("./userList")));

function User() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Users"
      headerDescription="Manage system users and credentials"
      headerIcon={<Users className="h-6 w-6 text-primary" />}
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<UserForm id={id} setUserId={setId} />}
      list={<UserList editHandler={editHandler} />}
    />
  );
}

export default User;
