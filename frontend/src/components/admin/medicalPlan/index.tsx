import { lazy, memo } from "react";
import { ShieldPlus } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const MedicalPlanForm = memo(lazy(() => import("./form")));
const MedicalPlanList = memo(lazy(() => import("./list")));

function MedicalPlan() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Medical Plans"
      headerDescription="Coverage plans — annual limit, coverage % and dependent eligibility"
      headerIcon={<ShieldPlus className="h-6 w-6 text-primary" />}
      tableTitle="Medical Plans"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<MedicalPlanForm id={id} setId={setId} />}
      list={<MedicalPlanList editHandler={editHandler} />}
    />
  );
}

export default MedicalPlan;
