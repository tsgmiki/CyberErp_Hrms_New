import { lazy, memo } from "react";
import { TrendingUp } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const ImprovementPlanForm = memo(lazy(() => import("./form")));
const ImprovementPlanList = memo(lazy(() => import("./list")));

function ImprovementPlan() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Improvement Plans"
      headerDescription="Performance improvement plans — objectives, timeline, and outcome"
      headerIcon={<TrendingUp className="h-6 w-6 text-primary" />}
      tableTitle="Improvement Plans"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<ImprovementPlanForm id={id} setId={setId} />}
      list={<ImprovementPlanList editHandler={editHandler} />}
    />
  );
}

export default ImprovementPlan;
