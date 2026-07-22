import { lazy, memo } from "react";
import { GitBranchPlus } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const SuccessionPlanForm = memo(lazy(() => import("./form")));
const SuccessionPlanList = memo(lazy(() => import("./list")));

function SuccessionPlan() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  return (
    <EntityModuleShell
      title="Succession Plans"
      headerDescription="Successor pools for critical roles"
      headerIcon={<GitBranchPlus className="h-6 w-6 text-primary" />}
      tableTitle="Succession Plans"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<SuccessionPlanForm id={id} setId={setId} />}
      list={<SuccessionPlanList editHandler={editHandler} />}
    />
  );
}
export default SuccessionPlan;
