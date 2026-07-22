import { lazy, memo } from "react";
import { UserRoundCog } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const EmployeeCareerPathForm = memo(lazy(() => import("./form")));
const EmployeeCareerPathList = memo(lazy(() => import("./list")));

function EmployeeCareerPath() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  return (
    <EntityModuleShell
      title="Employee Career Paths"
      headerDescription="Assign employees to career paths and track their step progress"
      headerIcon={<UserRoundCog className="h-6 w-6 text-primary" />}
      tableTitle="Assignments"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<EmployeeCareerPathForm id={id} setId={setId} />}
      list={<EmployeeCareerPathList editHandler={editHandler} />}
    />
  );
}

export default EmployeeCareerPath;
