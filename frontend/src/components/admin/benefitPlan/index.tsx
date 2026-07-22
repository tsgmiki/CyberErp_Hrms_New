import { lazy, memo } from "react";
import { HeartPulse } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const BenefitPlanForm = memo(lazy(() => import("./form")));
const BenefitPlanList = memo(lazy(() => import("./list")));

function BenefitPlan() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Benefit Plans"
      headerDescription="Health, life, pension and other plans — with employee/employer contributions and an enrollment window"
      headerIcon={<HeartPulse className="h-6 w-6 text-primary" />}
      tableTitle="Benefit Plans"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<BenefitPlanForm id={id} setId={setId} />}
      list={<BenefitPlanList editHandler={editHandler} />}
    />
  );
}

export default BenefitPlan;
