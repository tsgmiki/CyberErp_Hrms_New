import { lazy, memo } from "react";
import { GraduationCap } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const DevelopmentPlanForm = memo(lazy(() => import("./form")));
const DevelopmentPlanList = memo(lazy(() => import("./list")));

function DevelopmentPlan() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Development Plans"
      headerDescription="Individual development plans — competency gaps and learning actions"
      headerIcon={<GraduationCap className="h-6 w-6 text-primary" />}
      tableTitle="Development Plans"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<DevelopmentPlanForm id={id} setId={setId} />}
      list={<DevelopmentPlanList editHandler={editHandler} />}
    />
  );
}

export default DevelopmentPlan;
