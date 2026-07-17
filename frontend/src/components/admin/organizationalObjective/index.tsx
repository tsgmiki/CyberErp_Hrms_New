import { lazy, memo } from "react";
import { Goal } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const OrganizationalObjectiveForm = memo(lazy(() => import("./form")));
const OrganizationalObjectiveList = memo(lazy(() => import("./list")));

function OrganizationalObjective() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Organizational Objectives"
      headerDescription="Cascade objectives from organization to directorates and teams"
      headerIcon={<Goal className="h-6 w-6 text-primary" />}
      tableTitle="Organizational Objectives"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<OrganizationalObjectiveForm id={id} setId={setId} />}
      list={<OrganizationalObjectiveList editHandler={editHandler} />}
    />
  );
}

export default OrganizationalObjective;
