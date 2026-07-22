import { lazy, memo } from "react";
import { Plane } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const PerDiemRateForm = memo(lazy(() => import("./form")));
const PerDiemRateList = memo(lazy(() => import("./list")));

function PerDiemRate() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Per-diem Rates"
      headerDescription="Daily travel allowance by job grade and trip type"
      headerIcon={<Plane className="h-6 w-6 text-primary" />}
      tableTitle="Per-diem Rates"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<PerDiemRateForm id={id} setId={setId} />}
      list={<PerDiemRateList editHandler={editHandler} />}
    />
  );
}

export default PerDiemRate;
