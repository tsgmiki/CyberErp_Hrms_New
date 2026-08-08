import { lazy, memo } from "react";
import { MapPin } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const WorkLocationForm = memo(lazy(() => import("./form")));
const WorkLocationList = memo(lazy(() => import("./list")));

function WorkLocation() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/workLocation");

  return (
    <EntityModuleShell
      title="Work Locations"
      headerDescription="Country → Region → City → Office hierarchy"
      headerIcon={<MapPin className="h-6 w-6 text-primary" />}
      tableTitle="Work Locations"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<WorkLocationForm id={id} setId={setId} />}
      list={<WorkLocationList editHandler={editHandler} />}
    />
  );
}

export default WorkLocation;
