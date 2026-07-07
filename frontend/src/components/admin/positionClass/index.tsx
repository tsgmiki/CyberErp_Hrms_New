import { lazy, memo } from "react";
import { BriefcaseBusiness } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const PositionClassForm = memo(lazy(() => import("./form")));
const PositionClassList = memo(lazy(() => import("./list")));

function PositionClass() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

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
