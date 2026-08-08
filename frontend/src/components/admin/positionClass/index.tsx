import { lazy, memo } from "react";
import { BriefcaseBusiness } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const PositionClassForm = memo(lazy(() => import("./form")));
const PositionClassList = memo(lazy(() => import("./list")));

function PositionClass() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/positionClass");

  return (
    <EntityModuleShell
      title="Position Classes"
      headerDescription="Reusable job definitions (title, grade, requirements)"
      headerIcon={<BriefcaseBusiness className="h-6 w-6 text-primary" />}
      tableTitle="Position Classes"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<PositionClassForm id={id} setId={setId} />}
      list={<PositionClassList editHandler={editHandler} />}
    />
  );
}

export default PositionClass;
