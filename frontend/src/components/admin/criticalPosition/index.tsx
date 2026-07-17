import { lazy, memo } from "react";
import { ShieldAlert } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const CriticalPositionForm = memo(lazy(() => import("./form")));
const CriticalPositionList = memo(lazy(() => import("./list")));

function CriticalPosition() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  return (
    <EntityModuleShell
      title="Critical Positions"
      headerDescription="Flag business-critical roles for succession planning"
      headerIcon={<ShieldAlert className="h-6 w-6 text-primary" />}
      tableTitle="Critical Positions"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<CriticalPositionForm id={id} setId={setId} />}
      list={<CriticalPositionList editHandler={editHandler} />}
    />
  );
}

export default CriticalPosition;
