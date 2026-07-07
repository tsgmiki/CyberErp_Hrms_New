import { lazy, memo } from "react";
import { GitBranch } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const WorkflowDefinitionForm = memo(lazy(() => import("./form")));
const WorkflowDefinitionList = memo(lazy(() => import("./list")));

function WorkflowDefinition() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Workflow Definitions"
      headerDescription="Configure approval chains per HR process — records route through them automatically"
      headerIcon={<GitBranch className="h-6 w-6 text-primary" />}
      tableTitle="Workflow Definitions"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<WorkflowDefinitionForm id={id} setId={setId} />}
      list={<WorkflowDefinitionList editHandler={editHandler} />}
    />
  );
}

export default WorkflowDefinition;
