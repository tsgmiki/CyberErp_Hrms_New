import { lazy, memo } from "react";
import { Route } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const CareerPathForm = memo(lazy(() => import("./form")));
const CareerPathList = memo(lazy(() => import("./list")));

function CareerPath() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  return (
    <EntityModuleShell
      title="Career Paths"
      headerDescription="Define progression tracks and their required competencies"
      headerIcon={<Route className="h-6 w-6 text-primary" />}
      tableTitle="Career Paths"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<CareerPathForm id={id} setId={setId} />}
      list={<CareerPathList editHandler={editHandler} />}
    />
  );
}

export default CareerPath;
