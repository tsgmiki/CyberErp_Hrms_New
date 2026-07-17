import { lazy, memo } from "react";
import { GitPullRequestArrow } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const ChangeRequestForm = memo(lazy(() => import("./form")));
const ChangeRequestList = memo(lazy(() => import("./list")));

function CareerPathChangeRequest() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  return (
    <EntityModuleShell
      title="Career Path Change Requests"
      headerDescription="Employee requests to switch career path, with a light approval flow"
      headerIcon={<GitPullRequestArrow className="h-6 w-6 text-primary" />}
      tableTitle="Change Requests"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<ChangeRequestForm id={id} setId={setId} />}
      list={<ChangeRequestList editHandler={editHandler} />}
    />
  );
}

export default CareerPathChangeRequest;
