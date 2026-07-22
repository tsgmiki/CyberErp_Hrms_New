import { lazy, memo } from "react";
import { Gauge } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const RatingScaleForm = memo(lazy(() => import("./form")));
const RatingScaleList = memo(lazy(() => import("./list")));

function RatingScale() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Rating Scales"
      headerDescription="Scoring frameworks and their level bands"
      headerIcon={<Gauge className="h-6 w-6 text-primary" />}
      tableTitle="Rating Scales"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<RatingScaleForm id={id} setId={setId} />}
      list={<RatingScaleList editHandler={editHandler} />}
    />
  );
}

export default RatingScale;
