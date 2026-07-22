import { lazy, memo } from "react";
import { Target } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const WorkforcePlanForm = memo(lazy(() => import("./form")));
const WorkforcePlanList = memo(lazy(() => import("./list")));

function WorkforcePlan() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Workforce Plans"
      headerDescription="Versioned headcount & cost plans anchored to the establishment — demand, supply, separations, budget control and approval (HC053–HC076)"
      headerIcon={<Target className="h-6 w-6 text-primary" />}
      tableTitle="Workforce Plans"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<WorkforcePlanForm id={id} setId={setId} />}
      list={<WorkforcePlanList editHandler={editHandler} />}
    />
  );
}

export default WorkforcePlan;
