import { lazy, memo } from "react";
import { Scale } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const CalibrationForm = memo(lazy(() => import("./form")));
const CalibrationList = memo(lazy(() => import("./list")));

function Calibration() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/calibration");

  return (
    <EntityModuleShell
      title="Calibration"
      headerDescription="Review and normalize appraisal scores across a cohort"
      headerIcon={<Scale className="h-6 w-6 text-primary" />}
      tableTitle="Calibration Sessions"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<CalibrationForm id={id} setId={setId} />}
      list={<CalibrationList editHandler={editHandler} />}
    />
  );
}

export default Calibration;
